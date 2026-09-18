namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateSegmentLeg(
        string SourceLegRef,
        int Sequence,
        string? OriginRef,
        string? OriginTerminalRef,
        string? DestinationRef,
        string? DestinationTerminalRef,
        DateTimeOffset? Departure,
        DateTimeOffset? Arrival);
}
