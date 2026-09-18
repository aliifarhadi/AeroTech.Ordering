using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.Ports.Offers
{
    public sealed record CandidateResolution(
        OfferResolutionOutcome Outcome,
        NormalizedCandidate? Candidate,
        OfferSourceProfile Profile,
        SourceEvidence? Evidence,
        DateTimeOffset ObservedAt,
        IReadOnlyList<string> Reasons)
    {
        public static CandidateResolution Resolved(NormalizedCandidate candidate, OfferSourceProfile profile, SourceEvidence evidence, DateTimeOffset observedAt)
            => new(OfferResolutionOutcome.Resolved, candidate, profile, evidence, observedAt, []);

        public static CandidateResolution Failed(OfferResolutionOutcome outcome, OfferSourceProfile profile, SourceEvidence? evidence, DateTimeOffset observedAt, params string[] reasons)
            => new(outcome, null, profile, evidence, observedAt, reasons);
    }
}
