using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class OrderComponentTotal
    {
        private OrderComponentTotal()
        {
        }

        internal OrderComponentTotal(
            long orderId,
            PricingComponentType component,
            PricingEffect effect,
            decimal debitAmount,
            decimal creditAmount,
            int currencyId)
        {
            if (debitAmount < 0 || creditAmount < 0)
                throw ExceptionFactory.ComponentTotalInvalid("debit and credit amounts are nonnegative magnitudes");

            if (currencyId <= 0)
                throw ExceptionFactory.ComponentTotalInvalid("a currency identifier is required");

            OrderId = orderId;
            Component = component;
            Effect = effect;
            DebitAmount = debitAmount;
            CreditAmount = creditAmount;
            CurrencyId = currencyId;
        }

        public long OrderId { get; private set; }

        public PricingComponentType Component { get; private set; }

        public PricingEffect Effect { get; private set; }

        public decimal DebitAmount { get; private set; }

        public decimal CreditAmount { get; private set; }

        public int CurrencyId { get; private set; }

        public decimal Net => DebitAmount - CreditAmount;
    }
}
