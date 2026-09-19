using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain.OrderAggregate.ValueObjects
{
    public sealed record FundingObligationScope
    {
        private FundingObligationScope(long? orderItemId, long? orderServiceId, long? pricingLineId)
        {
            OrderItemId = orderItemId;
            OrderServiceId = orderServiceId;
            PricingLineId = pricingLineId;
        }

        public long? OrderItemId { get; }

        public long? OrderServiceId { get; }

        public long? PricingLineId { get; }

        public static FundingObligationScope ForItem(long orderItemId)
            => new(Required(orderItemId, "order item"), null, null);

        public static FundingObligationScope ForService(long orderServiceId)
            => new(null, Required(orderServiceId, "order service"), null);

        public static FundingObligationScope ForPricingLine(long pricingLineId)
            => new(null, null, Required(pricingLineId, "pricing line"));

        private static long Required(long id, string subject)
            => id > 0 ? id : throw ExceptionFactory.FundingObligationScopeInvalid(subject);
    }
}
