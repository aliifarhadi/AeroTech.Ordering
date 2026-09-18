using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Policies
{
    public sealed record ReversibleLine(
        PricingComponentType Component,
        PricingEffect Effect,
        OrderPricingLineDirection Direction,
        PricingLineRole Role,
        Money OriginalValue,
        Money SaleValue);
}
