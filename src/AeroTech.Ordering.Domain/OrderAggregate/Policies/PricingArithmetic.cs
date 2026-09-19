using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Policies
{
    public static class PricingArithmetic
    {
        public static Money CustomerTotal(IEnumerable<PricedAmount> lines, int currencyId)
        {
            var total = 0m;

            foreach (var line in lines)
            {
                if (line.Effect != PricingEffect.CustomerBalance)
                    continue;

                if (line.SaleValue.CurrencyId != currencyId)
                    throw ExceptionFactory.PricingRuleViolated($"customer line currency {line.SaleValue.CurrencyId} differs from sale currency {currencyId}");

                EnsureMagnitude(line.SaleValue);
                total += PricingLineMatrix.Sign(line.Direction) * line.SaleValue.Amount;
            }

            return new Money(total, currencyId);
        }

        public static void EnsureMagnitude(Money value)
        {
            if (value.IsNegative)
                throw ExceptionFactory.PricingRuleViolated("pricing amounts are nonnegative magnitudes");
        }
    }
}
