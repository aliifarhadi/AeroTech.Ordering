using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateFareConstruction(
        FareConstructionAssurance Assurance,
        string SourceContextRef,
        IReadOnlyList<CandidatePricingUnit> PricingUnits);
}
