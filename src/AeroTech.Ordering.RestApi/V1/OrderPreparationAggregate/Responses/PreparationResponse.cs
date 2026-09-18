using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Application.OrderPreparationAggregate.Commands.PrepareOrderFromOffer;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Serialization;

namespace AeroTech.Ordering.RestApi.V1.OrderPreparationAggregate.Responses
{
    public sealed record PreparationResponse(
        long PreparationId,
        long OperationId,
        string SourceOfferId,
        string AcceptedSnapshotDigest,
        DateTimeOffset CapturedAt,
        DateTimeOffset PricedAt,
        PreparationValidityResponse Validity,
        AcceptanceAssurance AcceptanceAssurance,
        string PermittedAcceptanceProfile,
        IReadOnlyDictionary<string, object?> Candidate,
        IReadOnlyList<string> BlockingReasons)
    {
        public static PreparationResponse From(PreparationResult result) => new(
            result.PreparationId,
            result.OperationId,
            result.SourceOfferId,
            result.AcceptedSnapshotDigest,
            result.CapturedAt,
            result.PricedAt,
            new PreparationValidityResponse(
                ValidityFactResponse.From(result.OfferValidity),
                ValidityFactResponse.From(result.PriceValidity),
                ValidityFactResponse.From(result.TicketingValidity)),
            result.AcceptanceAssurance,
            result.PermittedAcceptanceProfile,
            NormalizedCandidateJson.ToNode(result.Candidate),
            result.BlockingReasons);
    }
}
