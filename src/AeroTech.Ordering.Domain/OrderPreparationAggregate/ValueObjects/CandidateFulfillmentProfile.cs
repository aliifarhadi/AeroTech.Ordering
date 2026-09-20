using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateFulfillmentProfile(
        string? ProfileRef,
        string? ProfileVersion,
        FulfillmentProfileAssurance Assurance,
        ReservationRequirement ReservationRequirement,
        FulfillmentDocumentKind DocumentKind,
        DocumentAuthority? DocumentAuthority,
        FundingRequirement FundingRequirement,
        int? CapacityUnits,
        string? ResourceUnitPolicyRef,
        string? DeliveryControlPolicyRef,
        string? DependencyTreatmentPolicyRef,
        bool? PartialFulfillmentSupported)
    {
        public static CandidateFulfillmentProfile Unresolved { get; } = new(
            null,
            null,
            FulfillmentProfileAssurance.NotCertified,
            ReservationRequirement.Unresolved,
            FulfillmentDocumentKind.Unresolved,
            null,
            FundingRequirement.Unresolved,
            null,
            null,
            null,
            null,
            null);

        public FulfillmentProfileSnapshot ToSnapshot() => new(
            ProfileRef,
            ProfileVersion,
            Assurance,
            ReservationRequirement,
            DocumentKind,
            DocumentAuthority,
            FundingRequirement,
            CapacityUnits,
            ResourceUnitPolicyRef,
            DeliveryControlPolicyRef,
            DependencyTreatmentPolicyRef,
            PartialFulfillmentSupported);
    }
}
