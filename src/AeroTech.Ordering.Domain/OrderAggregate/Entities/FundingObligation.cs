using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class FundingObligation : Entity<long>
    {
        private FundingObligation()
        {
        }

        internal FundingObligation(
            long id,
            long orderId,
            FundingObligationPurpose purpose,
            Money amount,
            long? orderItemId,
            long changeId,
            string sourceDecisionRef)
        {
            Id = id;
            OrderId = orderId;
            Version = 1;
            Purpose = purpose;
            Amount = amount;
            OrderItemId = orderItemId;
            ChangeId = changeId;
            SourceDecisionRef = sourceDecisionRef;
        }

        public long OrderId { get; private set; }

        public int Version { get; private set; }

        public FundingObligationPurpose Purpose { get; private set; }

        public Money Amount { get; private set; } = null!;

        public long? OrderItemId { get; private set; }

        public long ChangeId { get; private set; }

        public string SourceDecisionRef { get; private set; } = null!;

        public long? SupersededObligationId { get; private set; }
    }
}
