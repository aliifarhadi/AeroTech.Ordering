using AeroTech.Framework.Core.Domain.Aggregates;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Domain._Shared.Serialization;
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
        private readonly List<PreparationSourceEvidence> _evidence = new();
        private NormalizedCandidate? _candidate;

        private OrderPreparation()
        {
        }

        public long OwnerAirlineId { get; private set; }

        public long FinancialCustomerId { get; private set; }

        public string CallerScope { get; private set; } = null!;

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

        public DateTimeOffset? OfferExpiresAt { get; private set; }

        public DateTimeOffset? PriceValidUntil { get; private set; }

        public IReadOnlyCollection<PreparationSourceEvidence> Evidence => _evidence.AsReadOnly();

        public NormalizedCandidate Candidate => _candidate ??= NormalizedCandidateJson.Read(CandidateJson);

        public static OrderPreparation Capture(CaptureOrderPreparationArgs args)
        {
            CandidateValidator.EnsureValid(args.Candidate, args.Scope);

            if (!string.Equals(args.Candidate.Source.ProviderProfileId, args.Profile.ProviderProfileId, StringComparison.Ordinal))
                throw ExceptionFactory.CandidateContractMismatch("candidate provider profile differs from the resolving profile");

            if (!string.Equals(args.Candidate.Source.SourcePayloadHash, args.Evidence.PayloadHash, StringComparison.Ordinal))
                throw ExceptionFactory.CandidateContractMismatch("candidate source payload hash differs from the captured evidence");

            var preparation = new OrderPreparation
            {
                Id = args.PreparationId,
                OwnerAirlineId = args.Scope.OwnerAirlineId,
                FinancialCustomerId = args.Scope.FinancialCustomerId,
                CallerScope = args.Scope.CallerScope,
                SourceOwner = args.Candidate.Source.Owner,
                SourceOfferId = args.Candidate.Source.OfferId,
                ProviderProfileId = args.Profile.ProviderProfileId,
                ContractVersion = args.Profile.ContractVersion,
                AcceptanceProfile = args.Profile.AcceptanceProfile,
                AcceptanceAssurance = args.Candidate.AcceptanceAssurance,
                OwnerBindingRef = args.Candidate.Source.OwnerBindingRef,
                SourcePayloadHash = args.Candidate.Source.SourcePayloadHash,
                CanonicalizationVersion = CanonicalJson.Version,
                CandidateJson = NormalizedCandidateJson.Write(args.Candidate),
                PricedAt = args.Candidate.PricedAt,
                CapturedAt = args.Candidate.CapturedAt,
                OfferExpiresAt = args.Candidate.OfferExpiresAt,
                PriceValidUntil = args.Candidate.PriceValidUntil,
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

        public void EnsureAcceptable(IAcceptanceProfilePolicy policy, DateTimeOffset now)
        {
            if (!policy.Permits(AcceptanceProfile))
                throw ExceptionFactory.AcceptanceProfileNotPermitted(AcceptanceProfile, policy.EnvironmentClass);

            EnsureValidity("offer", OfferExpiresAt, now);
            EnsureValidity("price", PriceValidUntil, now);
        }

        private void EnsureValidity(string subject, DateTimeOffset? validUntil, DateTimeOffset now)
        {
            if (validUntil is { } expiry && expiry <= now)
                throw ExceptionFactory.SourceValidityExpired(subject, SourceOwner, expiry);

            if (validUntil is null && AcceptanceAssurance == AcceptanceAssurance.OwnerBound)
                throw ExceptionFactory.SourceValidityNotEstablished(subject, SourceOwner);
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
                ["callerScope"] = CallerScope
            },
            ["candidate"] = NormalizedCandidateJson.ToNode(Candidate)
        }));
    }
}
