using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateJourney(
        string BoundId,
        int Sequence,
        BoundDirection Direction,
        int OriginAirportId,
        int DestinationAirportId);
}
