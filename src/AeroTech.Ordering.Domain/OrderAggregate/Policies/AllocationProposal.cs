using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Policies
{
    public sealed record AllocationProposal(
        PricingAllocationPurpose Purpose,
        int Version,
        PricingAllocationMethod Method,
        PricingAllocationCompleteness Completeness,
        IReadOnlyList<AllocationShare> Rows);
}
