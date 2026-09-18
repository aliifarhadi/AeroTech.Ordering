using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain.OrderAggregate.Policies
{
    public static class PricingLineMatrix
    {
        public static void EnsureAllowed(
            PricingComponentType component,
            PricingEffect effect,
            OrderPricingLineDirection direction,
            PricingLineRole role)
        {
            if (!Enum.IsDefined(component) || !Enum.IsDefined(effect) || !Enum.IsDefined(direction) || !Enum.IsDefined(role))
                throw ExceptionFactory.PricingRuleViolated("component, effect, direction and role must be defined");

            if (component == PricingComponentType.Other && effect != PricingEffect.Informational)
                throw ExceptionFactory.PricingRuleViolated("Other is informational only");

            if (component == PricingComponentType.Tax && effect == PricingEffect.SettlementOnly)
                throw ExceptionFactory.PricingRuleViolated("Tax cannot be settlement-only");

            if (component == PricingComponentType.Commission && effect == PricingEffect.CustomerBalance)
                throw ExceptionFactory.PricingRuleViolated("Commission is not a customer charge");

            if (effect != PricingEffect.CustomerBalance || role != PricingLineRole.Original)
                return;

            var normal = NormalCustomerDirection(component);

            if (normal is not null && normal != direction)
                throw ExceptionFactory.PricingRuleViolated($"{component} customer line must be {normal}");
        }

        public static OrderPricingLineDirection? NormalCustomerDirection(PricingComponentType component) => component switch
        {
            PricingComponentType.Discount => OrderPricingLineDirection.Credit,
            PricingComponentType.Adjustment => null,
            PricingComponentType.Commission => null,
            PricingComponentType.Other => null,
            _ => OrderPricingLineDirection.Debit
        };

        public static int Sign(OrderPricingLineDirection direction) => direction switch
        {
            OrderPricingLineDirection.Debit => 1,
            OrderPricingLineDirection.Credit => -1,
            _ => throw ExceptionFactory.PricingRuleViolated("direction must be defined")
        };
    }
}
