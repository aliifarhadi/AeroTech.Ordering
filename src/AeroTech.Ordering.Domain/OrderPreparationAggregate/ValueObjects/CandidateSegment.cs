using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateSegment(
        string SegmentRef,
        SegmentKind Kind,
        string OriginRef,
        string DestinationRef,
        DateTimeOffset? SoldDeparture,
        DateTimeOffset? SoldArrival,
        string? FlightRef,
        IReadOnlyList<string> OperationalLegRefs);
}
