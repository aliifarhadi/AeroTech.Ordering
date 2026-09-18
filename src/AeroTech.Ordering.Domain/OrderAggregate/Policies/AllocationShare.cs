using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Policies
{
    public sealed record AllocationShare(
        PricingBasisType TargetType,
        string TargetRef,
        Money OriginalValue,
        Money SaleValue);
}
