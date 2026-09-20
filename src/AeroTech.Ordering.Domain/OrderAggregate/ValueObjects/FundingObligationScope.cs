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

        public static FundingObligationScope ForItem(long orderItemId) => Single(orderItemId, null, null);

        public static FundingObligationScope ForService(long orderServiceId) => Single(null, orderServiceId, null);

        public static FundingObligationScope ForPricingLine(long pricingLineId) => Single(null, null, pricingLineId);

        private static FundingObligationScope Single(long? orderItemId, long? orderServiceId, long? pricingLineId)
        {
            var named = new[] { orderItemId, orderServiceId, pricingLineId }.Count(value => value is not null);

            if (named != 1)
                throw ExceptionFactory.FundingObligationScopeInvalid($"{named} scopes were named");

            if (orderItemId is <= 0 || orderServiceId is <= 0 || pricingLineId is <= 0)
                throw ExceptionFactory.FundingObligationScopeInvalid("a scope identifier must be positive");

            return new FundingObligationScope(orderItemId, orderServiceId, pricingLineId);
        }
    }
}
