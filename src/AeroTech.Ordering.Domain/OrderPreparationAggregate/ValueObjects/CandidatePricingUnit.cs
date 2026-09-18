using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidatePricingUnit(
        string SourceUnitRef,
        string? SourceKindRaw,
        FarePricingUnitType Type,
        FareCombinationMethod CombinationMethod,
        IReadOnlyList<string> CoveredSourceBoundRefs,
        CandidatePricingGroup? PricingGroup,
        IReadOnlyList<CandidateFareComponent> Components);
}
