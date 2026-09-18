using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.ValueObjects
{
    public sealed record CommercialTermsSnapshot
    {
        public CommercialTermsSnapshot(
            CommercialTermState refundability,
            CommercialTermState changeability,
            CommercialTermState upgradeEligibility,
            string sourceSystem,
            string? sourcePolicyRef,
            string? sourcePolicyVersion,
            DateTimeOffset termsCapturedAt)
        {
            if (string.IsNullOrWhiteSpace(sourceSystem))
                throw ExceptionFactory.CandidateContractMismatch("a commercial terms snapshot requires its source system");

            Refundability = refundability;
            Changeability = changeability;
            UpgradeEligibility = upgradeEligibility;
            SourceSystem = sourceSystem;
            SourcePolicyRef = string.IsNullOrWhiteSpace(sourcePolicyRef) ? null : sourcePolicyRef;
            SourcePolicyVersion = string.IsNullOrWhiteSpace(sourcePolicyVersion) ? null : sourcePolicyVersion;
            TermsCapturedAt = termsCapturedAt;
        }

        public CommercialTermState Refundability { get; }

        public CommercialTermState Changeability { get; }

        public CommercialTermState UpgradeEligibility { get; }

        public string SourceSystem { get; }

        public string? SourcePolicyRef { get; }

        public string? SourcePolicyVersion { get; }

        public DateTimeOffset TermsCapturedAt { get; }

        public static CommercialTermsSnapshot Summarize(
            IReadOnlyList<SoldTermFlags> soldTerms,
            string sourceSystem,
            string? sourcePolicyRef,
            string? sourcePolicyVersion,
            DateTimeOffset termsCapturedAt)
            => new(
                Summarize(soldTerms.Select(terms => terms.Refundable)),
                Summarize(soldTerms.Select(terms => terms.Changeable)),
                Summarize(soldTerms.Select(terms => terms.Upgradable)),
                sourceSystem,
                sourcePolicyRef,
                sourcePolicyVersion,
                termsCapturedAt);

        private static CommercialTermState Summarize(IEnumerable<bool?> supplied)
        {
            var values = supplied.Where(value => value.HasValue).Select(value => value!.Value).ToList();

            if (values.Count == 0)
                return CommercialTermState.Unknown;

            if (values.All(value => value))
                return CommercialTermState.Permitted;

            return values.All(value => !value) ? CommercialTermState.Prohibited : CommercialTermState.Conditional;
        }
    }
}
