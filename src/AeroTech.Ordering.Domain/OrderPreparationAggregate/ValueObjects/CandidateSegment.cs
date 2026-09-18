using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateSegment(
        string SegmentRef,
        string JourneyRef,
        SegmentKind Kind,
        string OriginRef,
        string? OriginTerminalRef,
        string DestinationRef,
        string? DestinationTerminalRef,
        DateTimeOffset? SoldDeparture,
        DateTimeOffset? SoldArrival,
        string? FlightRef,
        string? FlightNumber,
        string? FlightVersion,
        string? MarketingCarrierRef,
        string? OperatingCarrierRef,
        string? SourceCapacityRef,
        int? Duration,
        string? AircraftRef,
        IReadOnlyList<CandidateSegmentLeg> Legs);
}
