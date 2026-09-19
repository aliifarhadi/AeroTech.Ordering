namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateFareComponent(
        long AirFareId,
        string? FareBasis,
        string? FareFamily,
        string? FareType,
        int? CabinClassId,
        long? RbdId,
        string? BookingClass,
        int? TicketingRestrictionMinutes);
}
