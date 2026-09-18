using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidatePricingUnit(
        string SourceUnitRef,
        FarePricingUnitType Type,
        FareCombinationMethod CombinationMethod,
        IReadOnlyList<string> CoveredSourceBoundRefs,
        CandidatePricingGroup? PricingGroup,
        IReadOnlyList<CandidateFareComponent> Components);
}
