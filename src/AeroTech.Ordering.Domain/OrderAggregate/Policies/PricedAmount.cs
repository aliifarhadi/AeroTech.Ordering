using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Policies
{
    public sealed record PricedAmount(PricingEffect Effect, OrderPricingLineDirection Direction, Money SaleValue);
}
