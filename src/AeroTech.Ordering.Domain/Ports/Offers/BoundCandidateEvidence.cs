using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.Ports.Offers
{
    public sealed record BoundCandidateEvidence(
        OfferResolutionOutcome Outcome,
        NormalizedCandidate? Candidate,
        OfferSourceProfile Profile,
        DateTimeOffset ObservedAt,
        IReadOnlyList<string> Reasons);
}
