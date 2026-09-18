using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.OrderAggregate.ValueObjects
{
    public sealed record FulfillmentProfileSnapshot(
        string ProfileRef,
        ReservationRequirement ReservationRequirement,
        FulfillmentDocumentKind DocumentKind,
        bool RequiresFunding,
        int CapacityUnits);
}
