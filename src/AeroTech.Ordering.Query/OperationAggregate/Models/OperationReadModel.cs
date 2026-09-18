using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Query.OperationAggregate.Models
{
    public sealed class OperationReadModel
    {
        public long Id { get; set; }

        public long OperationId { get; set; }

        public long OwnerAirlineId { get; set; }

        public long FinancialCustomerId { get; set; }

        public OrderingCommandKind CommandKind { get; set; }

        public CommandReceiptStatus Status { get; set; }

        public long? PreparationId { get; set; }

        public long? OrderId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? CompletedAt { get; set; }
    }
}
