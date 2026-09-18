using System.Security.Cryptography;
using System.Text;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Serialization;
using AeroTech.Ordering.Domain.Ports.Offers;
using AeroTech.Ordering.Providers.Deterministic._Shared.Persistence;
using AeroTech.Ordering.Providers.Deterministic.Offers.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ordering.Providers.Deterministic.Offers
{
    public sealed class ReferenceOfferSourceAdapter : IOfferSourcePort
    {
        public const string PayloadContentType = "application/vnd.aerotech.reference-offer+json";

        private readonly IDbContextFactory<DeterministicOwnerDbContext> _contextFactory;
        private readonly IClock _clock;

        public ReferenceOfferSourceAdapter(IDbContextFactory<DeterministicOwnerDbContext> contextFactory, IClock clock)
        {
            _contextFactory = contextFactory;
            _clock = clock;
        }

        public async Task<CandidateResolution> ResolveCandidateAsync(ResolveCandidateRequest request, CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var now = _clock.GetDateTime();

            if (!await IsAvailableAsync(context, cancellationToken))
                return await RecordAsync(context, ReferenceOfferProfile.ResolveOperation, request.OfferId, now,
                    CandidateResolution.Failed(OfferResolutionOutcome.Unavailable, ReferenceOfferProfile.Profile, null, now, "reference offer owner is unavailable"), cancellationToken);

            if (request.RequestedSelection.Count > 0)
                return await RecordAsync(context, ReferenceOfferProfile.ResolveOperation, request.OfferId, now,
                    CandidateResolution.Failed(OfferResolutionOutcome.UnsupportedCapability, ReferenceOfferProfile.Profile, null, now, "reference offers are sold as full candidates"), cancellationToken);

            var scope = request.AuthorizedSalesContext;
            var offer = await context.ReferenceOffers.AsNoTracking()
                .SingleOrDefaultAsync(item => item.OfferId == request.OfferId
                                              && item.IsCurrent
                                              && item.OwnerAirlineId == scope.OwnerAirlineId
                                              && item.FinancialCustomerId == scope.FinancialCustomerId
                                              && item.Channel == scope.Channel
                                              && item.SellingOfficeId == scope.SellingOfficeId,
                    cancellationToken);

            if (offer is null)
                return await RecordAsync(context, ReferenceOfferProfile.ResolveOperation, request.OfferId, now,
                    CandidateResolution.Failed(OfferResolutionOutcome.NotFound, ReferenceOfferProfile.Profile, null, now, "offer is not available to this sales context"), cancellationToken);

            try
            {
                var payloadHash = Hash(offer.CandidateJson);
                var template = NormalizedCandidateJson.Read(offer.CandidateJson);
                var candidate = template with
                {
                    Source = template.Source with
                    {
                        ProviderProfileId = ReferenceOfferProfile.ProfileId,
                        OwnerBindingRef = offer.OwnerBindingRef,
                        SourcePayloadHash = payloadHash
                    },
                    AcceptanceAssurance = AcceptanceAssurance.OwnerBound,
                    CapturedAt = now
                };

                var evidence = new SourceEvidence($"reference-offer:{offer.OfferId}:r{offer.Revision}", payloadHash, PayloadContentType, offer.CandidateJson);

                return await RecordAsync(context, ReferenceOfferProfile.ResolveOperation, request.OfferId, now,
                    CandidateResolution.Resolved(candidate, ReferenceOfferProfile.Profile, evidence, now), cancellationToken);
            }
            catch (BusinessException exception)
            {
                return await RecordAsync(context, ReferenceOfferProfile.ResolveOperation, request.OfferId, now,
                    CandidateResolution.Failed(exception.Code == 20273 ? OfferResolutionOutcome.UnsupportedCapability : OfferResolutionOutcome.ContractMismatch,
                        ReferenceOfferProfile.Profile, null, now, exception.Message), cancellationToken);
            }
        }

        public async Task<BoundCandidateEvidence> ReadBoundCandidateAsync(ReadBoundCandidateRequest request, CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var now = _clock.GetDateTime();
            var scope = request.AuthorizedSalesContext;

            var offer = await context.ReferenceOffers.AsNoTracking()
                .SingleOrDefaultAsync(item => item.OwnerBindingRef == request.OwnerBindingRef
                                              && item.OwnerAirlineId == scope.OwnerAirlineId
                                              && item.FinancialCustomerId == scope.FinancialCustomerId,
                    cancellationToken);

            BoundCandidateEvidence result;

            if (offer is null)
                result = new BoundCandidateEvidence(OfferResolutionOutcome.NotFound, null, ReferenceOfferProfile.Profile, now, ["binding is not visible to this sales context"]);
            else if (!string.Equals(Hash(offer.CandidateJson), request.ExpectedSourceDigest, StringComparison.Ordinal))
                result = new BoundCandidateEvidence(OfferResolutionOutcome.ContractMismatch, null, ReferenceOfferProfile.Profile, now, ["bound candidate digest differs from the expected digest"]);
            else
                result = new BoundCandidateEvidence(OfferResolutionOutcome.Resolved, NormalizedCandidateJson.Read(offer.CandidateJson), ReferenceOfferProfile.Profile, now, []);

            context.OwnerReads.Add(new OwnerReadRecord
            {
                Owner = ReferenceOfferProfile.Owner,
                Operation = ReferenceOfferProfile.ReadBoundOperation,
                Reference = request.OwnerBindingRef,
                Outcome = result.Outcome.ToString(),
                ObservedAt = now
            });
            await context.SaveChangesAsync(cancellationToken);

            return result;
        }

        private static async Task<bool> IsAvailableAsync(DeterministicOwnerDbContext context, CancellationToken cancellationToken)
        {
            var availability = await context.OwnerAvailability.AsNoTracking()
                .SingleOrDefaultAsync(item => item.Owner == ReferenceOfferProfile.Owner, cancellationToken);

            return availability?.IsAvailable ?? true;
        }

        private static async Task<CandidateResolution> RecordAsync(
            DeterministicOwnerDbContext context,
            string operation,
            string reference,
            DateTimeOffset now,
            CandidateResolution resolution,
            CancellationToken cancellationToken)
        {
            context.OwnerReads.Add(new OwnerReadRecord
            {
                Owner = ReferenceOfferProfile.Owner,
                Operation = operation,
                Reference = reference,
                Outcome = resolution.Outcome.ToString(),
                ObservedAt = now
            });

            await context.SaveChangesAsync(cancellationToken);
            return resolution;
        }

        private static string Hash(string payload)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
    }
}
