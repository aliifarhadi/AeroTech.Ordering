using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderAggregate.ValueObjects;

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
            FundingObligationScope scope,
            long changeId,
            long priceChangeSetId)
        {
            if (amount.IsNegative)
                throw ExceptionFactory.FundingObligationScopeInvalid($"its amount {amount.Amount} is negative");

            Id = id;
            OrderId = orderId;
            Version = 1;
            Purpose = purpose;
            Amount = amount;
            OrderItemId = scope.OrderItemId;
            OrderServiceId = scope.OrderServiceId;
            PricingLineId = scope.PricingLineId;
            ChangeId = changeId;
            PriceChangeSetId = priceChangeSetId;
        }

        public long OrderId { get; private set; }

        public int Version { get; private set; }

        public FundingObligationPurpose Purpose { get; private set; }

        public Money Amount { get; private set; } = null!;

        public long? OrderItemId { get; private set; }

        public long? OrderServiceId { get; private set; }

        public long? PricingLineId { get; private set; }

        public long ChangeId { get; private set; }

        public long PriceChangeSetId { get; private set; }
    }
}
