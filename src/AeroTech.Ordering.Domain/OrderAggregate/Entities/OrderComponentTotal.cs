using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class OrderComponentTotal : Entity<long>
    {
        private OrderComponentTotal()
        {
        }

        internal OrderComponentTotal(
            long id,
            long orderId,
            PricingComponentType component,
            PricingEffect effect,
            decimal debitAmount,
            decimal creditAmount,
            string currencyRef)
        {
            if (debitAmount < 0 || creditAmount < 0)
                throw ExceptionFactory.ComponentTotalInvalid("debit and credit amounts are nonnegative magnitudes");

            if (string.IsNullOrWhiteSpace(currencyRef))
                throw ExceptionFactory.ComponentTotalInvalid("a currency reference is required");

            Id = id;
            OrderId = orderId;
            Component = component;
            Effect = effect;
            DebitAmount = debitAmount;
            CreditAmount = creditAmount;
            CurrencyRef = currencyRef;
        }

        public long OrderId { get; private set; }

        public PricingComponentType Component { get; private set; }

        public PricingEffect Effect { get; private set; }

        public decimal DebitAmount { get; private set; }

        public decimal CreditAmount { get; private set; }

        public string CurrencyRef { get; private set; } = null!;

        public decimal Net => DebitAmount - CreditAmount;
    }
}
