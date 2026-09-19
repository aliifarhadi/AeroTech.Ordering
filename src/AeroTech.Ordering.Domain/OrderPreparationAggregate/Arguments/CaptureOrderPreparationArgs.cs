using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;
using AeroTech.Ordering.Domain.Ports.Offers;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.Arguments
{
    public sealed record CaptureOrderPreparationArgs(
        long PreparationId,
        long EvidenceId,
        AuthorizedSalesScope Scope,
        NormalizedCandidate Candidate,
        OfferSourceProfile Profile,
        SourceEvidence Evidence,
        DateTimeOffset CapturedAt);
}
