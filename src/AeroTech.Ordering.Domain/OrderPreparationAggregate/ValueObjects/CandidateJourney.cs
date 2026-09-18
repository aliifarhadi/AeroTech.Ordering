using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateJourney(
        string JourneyRef,
        int Sequence,
        string? SourceDirectionRaw,
        BoundDirection? Direction,
        string OriginRef,
        string DestinationRef);
}
