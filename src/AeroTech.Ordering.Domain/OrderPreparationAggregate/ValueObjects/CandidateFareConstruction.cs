namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateFareConstruction(IReadOnlyList<CandidatePricingUnit> PricingUnits);
}
