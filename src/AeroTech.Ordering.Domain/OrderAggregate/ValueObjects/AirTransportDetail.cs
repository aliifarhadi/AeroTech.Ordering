namespace AeroTech.Ordering.Domain.OrderAggregate.ValueObjects
{
    public sealed record AirTransportDetail(
        string? CabinRef,
        string? RbdRef,
        string? BookingClass,
        string? FlightNumber,
        string? FlightVersion,
        string? MarketingCarrierRef,
        string? OperatingCarrierRef);
}
