namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateSegmentLeg(
        long LegId,
        int Sequence,
        int OriginAirportId,
        int? OriginAirportTerminalId,
        int DestinationAirportId,
        int? DestinationAirportTerminalId,
        DateTimeOffset DepartureDateTime,
        DateTimeOffset ArrivalDateTime);
}
