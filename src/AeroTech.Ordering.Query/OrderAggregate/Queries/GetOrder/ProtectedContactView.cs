using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrder
{
    public sealed record ProtectedContactView(
        long ContactId,
        int Sequence,
        ContactRole Role,
        string? Email,
        string? Phone);
}
