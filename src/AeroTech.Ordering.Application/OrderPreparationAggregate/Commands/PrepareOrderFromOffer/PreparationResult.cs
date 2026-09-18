using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Contracts;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Application.OrderPreparationAggregate.Commands.PrepareOrderFromOffer
{
    public sealed record PreparationResult(
        long PreparationId,
        long OperationId,
        string SourceOfferId,
        string AcceptedSnapshotDigest,
        DateTimeOffset CapturedAt,
        DateTimeOffset PricedAt,
        ValidityFact OfferValidity,
        ValidityFact PriceValidity,
        ValidityFact TicketingValidity,
        AcceptanceAssurance AcceptanceAssurance,
        string PermittedAcceptanceProfile,
        NormalizedCandidate Candidate,
        IReadOnlyList<string> BlockingReasons,
        bool ReplayedFromReceipt)
    {
        public static PreparationResult From(OrderPreparation preparation, long operationId, IAcceptanceProfilePolicy policy, bool replayed)
            => new(
                preparation.Id,
                operationId,
                preparation.SourceOfferId,
                preparation.SnapshotDigest,
                preparation.CapturedAt,
                preparation.PricedAt,
                preparation.OfferValidity,
                preparation.PriceValidity,
                preparation.TicketingValidity,
                preparation.AcceptanceAssurance,
                preparation.AcceptanceProfile,
                preparation.Candidate,
                preparation.BlockingReasons(policy),
                replayed);
    }
}
