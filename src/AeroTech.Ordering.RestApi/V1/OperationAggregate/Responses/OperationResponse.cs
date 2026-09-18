using AeroTech.Ordering.Query.OperationAggregate.Queries.GetOperation;

namespace AeroTech.Ordering.RestApi.V1.OperationAggregate.Responses
{
    public sealed record OperationResponse(
        long OperationId,
        long? OrderId,
        long? PreparationId,
        string Kind,
        string Phase,
        string Outcome,
        IReadOnlyList<object> TargetEvidence,
        IReadOnlyList<string> PendingActions,
        bool ReconciliationRequired,
        DateTimeOffset CreatedAt,
        DateTimeOffset? CompletedAt)
    {
        public static OperationResponse From(OperationDetailsView view) => new(
            view.OperationId,
            view.OrderId,
            view.PreparationId,
            view.Kind,
            view.Phase,
            view.Outcome,
            view.TargetEvidence,
            view.PendingActions,
            view.ReconciliationRequired,
            view.CreatedAt,
            view.CompletedAt);
    }
}
