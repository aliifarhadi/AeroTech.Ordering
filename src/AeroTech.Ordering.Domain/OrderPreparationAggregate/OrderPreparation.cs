using AeroTech.Framework.Core.Domain.Aggregates;
using AeroTech.Messages.Aegis.Enums;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Domain._Shared.Serialization;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Arguments;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Contracts;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Entities;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Policies;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Serialization;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate
{
    public sealed class OrderPreparation : AggregateRoot<long>
    {
        public const string LiveAcceptanceBlocked = "LIVE_ACCEPTANCE_BLOCKED(BD-001)";
        public const string OfferValidityNotSupplied = "OFFER_VALIDITY_NOT_SUPPLIED";
        public const string PriceValidityNotSupplied = "PRICE_VALIDITY_NOT_SUPPLIED";
        public const string AcceptanceProfileNotPermitted = "ACCEPTANCE_PROFILE_NOT_PERMITTED";

        private readonly List<PreparationSourceEvidence> _evidence = new();
        private NormalizedCandidate? _candidate;

        private OrderPreparation()
        {
        }

        public long OwnerAirlineId { get; private set; }

        public long FinancialCustomerId { get; private set; }

        public string Channel { get; private set; } = null!;

        public long? SellingOfficeId { get; private set; }

        public string CallerScope { get; private set; } = null!;

        public BusinessContextType ActorContextType { get; private set; }

        public long? ActorId { get; private set; }

        public string SourceOwner { get; private set; } = null!;

        public string SourceOfferId { get; private set; } = null!;

        public string ProviderProfileId { get; private set; } = null!;

        public string ContractVersion { get; private set; } = null!;

        public string AcceptanceProfile { get; private set; } = null!;

        public AcceptanceAssurance AcceptanceAssurance { get; private set; }

        public string? OwnerBindingRef { get; private set; }

        public string SourcePayloadHash { get; private set; } = null!;

        public string CanonicalizationVersion { get; private set; } = null!;

        public string SnapshotDigest { get; private set; } = null!;

        public string CandidateJson { get; private set; } = null!;

        public DateTimeOffset PricedAt { get; private set; }

        public DateTimeOffset CapturedAt { get; private set; }

        public ValidityFact OfferValidity { get; private set; } = null!;

        public ValidityFact PriceValidity { get; private set; } = null!;

        public ValidityFact TicketingValidity { get; private set; } = null!;

        public string? ClientReference { get; private set; }

        public long? ConsumedByOrderId { get; private set; }

        public DateTimeOffset? ConsumedAt { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public IReadOnlyCollection<PreparationSourceEvidence> Evidence => _evidence.AsReadOnly();

        public NormalizedCandidate Candidate => _candidate ??= NormalizedCandidateJson.Read(CandidateJson);

        public static OrderPreparation Capture(CaptureOrderPreparationArgs args)
        {
            CandidateValidator.EnsureValid(args.Candidate, args.Scope);

            if (!string.Equals(args.Candidate.Source.ProviderProfileId, args.Profile.ProviderProfileId, StringComparison.Ordinal))
                throw ExceptionFactory.CandidateContractMismatch("candidate provider profile differs from the resolving profile");

            if (!string.Equals(args.Candidate.Source.SourcePayloadHash, args.Evidence.PayloadHash, StringComparison.Ordinal))
                throw ExceptionFactory.CandidateContractMismatch("candidate source payload hash differs from the captured evidence");

            var candidateJson = NormalizedCandidateJson.Write(args.Candidate);

            var preparation = new OrderPreparation
            {
                Id = args.PreparationId,
                OwnerAirlineId = args.Scope.OwnerAirlineId,
                FinancialCustomerId = args.Scope.FinancialCustomerId,
                Channel = args.Scope.Channel,
                SellingOfficeId = args.Scope.SellingOfficeId,
                CallerScope = args.Scope.CallerScope,
                ActorContextType = args.Scope.ActorContextType,
                ActorId = args.Scope.ActorId,
                SourceOwner = args.Candidate.Source.Owner,
                SourceOfferId = args.Candidate.Source.OfferId,
                ProviderProfileId = args.Profile.ProviderProfileId,
                ContractVersion = args.Profile.ContractVersion,
                AcceptanceProfile = args.Profile.AcceptanceProfile,
                AcceptanceAssurance = args.Candidate.AcceptanceAssurance,
                OwnerBindingRef = args.Candidate.Source.OwnerBindingRef,
                SourcePayloadHash = args.Candidate.Source.SourcePayloadHash,
                CanonicalizationVersion = CanonicalJson.Version,
                CandidateJson = candidateJson,
                PricedAt = args.Candidate.PricedAt,
                CapturedAt = args.Candidate.CapturedAt,
                OfferValidity = args.Candidate.Validity.Offer,
                PriceValidity = args.Candidate.Validity.Price,
                TicketingValidity = args.Candidate.Validity.Ticketing,
                ClientReference = args.ClientReference,
                CreatedAt = args.CreatedAt,
                _candidate = args.Candidate
            };

            preparation.SnapshotDigest = preparation.ComputeDigest();
            preparation._evidence.Add(new PreparationSourceEvidence(
                args.EvidenceId,
                args.PreparationId,
                args.Evidence.EvidenceRef,
                args.Evidence.PayloadHash,
                args.Evidence.ContentType,
                args.Evidence.Payload,
                args.Candidate.CapturedAt));

            return preparation;
        }

        public bool IsConsumed => ConsumedByOrderId is not null;

        public IReadOnlyList<ValidityFact> ValidityFacts => [OfferValidity, PriceValidity, TicketingValidity];

        public IReadOnlyList<string> BlockingReasons(IAcceptanceProfilePolicy policy)
        {
            var reasons = new List<string>();

            if (AcceptanceAssurance == AcceptanceAssurance.LocalCandidateOnly)
                reasons.Add(LiveAcceptanceBlocked);

            if (OfferValidity.State == ValidityState.NotSupplied)
                reasons.Add(OfferValidityNotSupplied);

            if (PriceValidity.State == ValidityState.NotSupplied)
                reasons.Add(PriceValidityNotSupplied);

            if (!policy.Permits(AcceptanceProfile))
                reasons.Add(AcceptanceProfileNotPermitted);

            return reasons;
        }

        public void EnsureAcceptable(string acceptedSnapshotDigest, IAcceptanceProfilePolicy policy, DateTimeOffset now)
        {
            if (!string.Equals(acceptedSnapshotDigest, SnapshotDigest, StringComparison.Ordinal))
                throw ExceptionFactory.AcceptedDigestMismatch(Id);

            if (ConsumedByOrderId is { } orderId)
                throw ExceptionFactory.PreparationAlreadyConsumed(Id, orderId);

            if (!policy.Permits(AcceptanceProfile))
                throw ExceptionFactory.AcceptanceProfileNotPermitted(AcceptanceProfile, policy.EnvironmentClass);

            EnsureValidity("offer", OfferValidity, now);
            EnsureValidity("price", PriceValidity, now);
        }

        public void Consume(long orderId, DateTimeOffset at)
        {
            if (ConsumedByOrderId is { } existing)
                throw ExceptionFactory.PreparationAlreadyConsumed(Id, existing);

            ConsumedByOrderId = orderId;
            ConsumedAt = at;
        }

        private void EnsureValidity(string subject, ValidityFact fact, DateTimeOffset now)
        {
            if (fact.IsExpiredAt(now))
                throw ExceptionFactory.SourceValidityExpired(subject, fact.Owner, fact.Value!.Value);

            if (fact.State == ValidityState.NotSupplied && AcceptanceAssurance == AcceptanceAssurance.OwnerBound)
                throw ExceptionFactory.SourceValidityNotEstablished(subject, fact.State);
        }

        private string ComputeDigest() => CanonicalJson.Sha256Hex(CanonicalJson.Write(new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["canonicalizationVersion"] = CanonicalizationVersion,
            ["providerProfileId"] = ProviderProfileId,
            ["contractVersion"] = ContractVersion,
            ["acceptanceProfile"] = AcceptanceProfile,
            ["authorizedScope"] = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["ownerAirlineId"] = CanonicalJson.Identifier(OwnerAirlineId),
                ["financialCustomerId"] = CanonicalJson.Identifier(FinancialCustomerId),
                ["channel"] = Channel,
                ["sellingOfficeId"] = CanonicalJson.Identifier(SellingOfficeId),
                ["callerScope"] = CallerScope
            },
            ["candidate"] = NormalizedCandidateJson.ToNode(Candidate)
        }));
    }
}
