using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateFulfillmentProfile(
        string ProfileRef,
        string ProfileVersion,
        FulfillmentProfileAssurance Assurance,
        ReservationRequirement ReservationRequirement,
        FulfillmentDocumentKind DocumentKind,
        DocumentAuthority? DocumentAuthority,
        FundingRequirement FundingRequirement,
        int? CapacityUnits,
        string? ResourceUnitPolicyRef,
        string? DeliveryControlPolicyRef,
        string? DependencyTreatmentPolicyRef,
        bool? PartialFulfillmentSupported);
}
