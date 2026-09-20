using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.OrderAggregate.ValueObjects
{
    public sealed record FulfillmentProfileSnapshot(
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
        public bool IsCertified => Assurance == FulfillmentProfileAssurance.Certified;
    }
}
