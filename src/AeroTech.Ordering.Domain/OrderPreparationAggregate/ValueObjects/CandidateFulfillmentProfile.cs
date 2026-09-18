using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateFulfillmentProfile(
        string ProfileRef,
        string ProfileVersion,
        FulfillmentProfileAssurance Assurance,
        ReservationRequirement ReservationRequirement,
        FulfillmentDocumentKind DocumentKind,
        FundingRequirement FundingRequirement,
        int? CapacityUnits);
}
