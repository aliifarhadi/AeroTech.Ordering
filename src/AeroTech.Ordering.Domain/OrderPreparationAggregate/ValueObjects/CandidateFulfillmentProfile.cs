using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateFulfillmentProfile(
        string ProfileRef,
        ReservationRequirement ReservationRequirement,
        FulfillmentDocumentKind DocumentKind,
        bool RequiresFunding,
        int CapacityUnits);
}
