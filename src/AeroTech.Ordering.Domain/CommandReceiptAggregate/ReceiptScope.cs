using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.CommandReceiptAggregate
{
    public sealed record ReceiptScope(
        long OwnerAirlineId,
        long FinancialCustomerId,
        string CallerScope,
        OrderingCommandKind CommandKind,
        string IdempotencyKey);
}
