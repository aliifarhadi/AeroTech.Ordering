using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class PriceChangeSet : Entity<long>
    {
        private PriceChangeSet()
        {
        }

        internal PriceChangeSet(
            long id,
            long orderId,
            long changeId,
            int financialSequence,
            PriceChangeReason reason,
            string sourceDecisionRef,
            int baseCommercialVersion,
            DateTimeOffset committedAt)
        {
            Id = id;
            OrderId = orderId;
            ChangeId = changeId;
            FinancialSequence = financialSequence;
            Reason = reason;
            SourceDecisionRef = sourceDecisionRef;
            BaseCommercialVersion = baseCommercialVersion;
            CommittedAt = committedAt;
        }

        public long OrderId { get; private set; }

        public long ChangeId { get; private set; }

        public int FinancialSequence { get; private set; }

        public PriceChangeReason Reason { get; private set; }

        public string SourceDecisionRef { get; private set; } = null!;

        public int BaseCommercialVersion { get; private set; }

        public DateTimeOffset CommittedAt { get; private set; }
    }
}
