using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateFareConstruction(
        FareConstructionAssurance Assurance,
        IReadOnlyList<string> ItemKeys,
        IReadOnlyList<CandidatePricingUnit> PricingUnits);
}
