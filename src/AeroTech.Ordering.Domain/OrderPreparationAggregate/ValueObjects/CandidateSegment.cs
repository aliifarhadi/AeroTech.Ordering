using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateSegment(
        string SegmentKey,
        string BoundId,
        int Sequence,
        SegmentKind Kind,
        int OriginAirportId,
        int? OriginAirportTerminalId,
        int DestinationAirportId,
        int? DestinationAirportTerminalId,
        DateTimeOffset? SoldDeparture,
        DateTimeOffset? SoldArrival,
        long? FlightId,
        string? FlightNumber,
        int? FlightVersion,
        int? MarketingAirlineId,
        int? OperatingAirlineId,
        long? FlightCapacityId,
        int? Duration,
        int? AircraftId,
        IReadOnlyList<CandidateSegmentLeg> Legs);
}
