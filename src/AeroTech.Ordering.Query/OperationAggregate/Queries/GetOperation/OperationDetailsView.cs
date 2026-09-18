namespace AeroTech.Ordering.Query.OperationAggregate.Queries.GetOperation
{
    public sealed record OperationDetailsView(
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
        DateTimeOffset? CompletedAt);
}
