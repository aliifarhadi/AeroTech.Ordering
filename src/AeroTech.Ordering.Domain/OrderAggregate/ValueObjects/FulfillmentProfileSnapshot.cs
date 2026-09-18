using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.OrderAggregate.ValueObjects
{
    public sealed record FulfillmentProfileSnapshot(
        string ProfileRef,
        string ProfileVersion,
        FulfillmentProfileAssurance Assurance,
        ReservationRequirement ReservationRequirement,
        FulfillmentDocumentKind DocumentKind,
        FundingRequirement FundingRequirement,
        int? CapacityUnits)
    {
        public bool IsCertified => Assurance == FulfillmentProfileAssurance.Certified;
    }
}
