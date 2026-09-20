using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidatePricingUnit(
        int Sequence,
        FarePricingUnitType Type,
        AirFareConstructionType SourceConstructionType,
        IReadOnlyList<string> CoveredBoundOfferIds,
        IReadOnlyList<CandidateFareComponent> Components);
}
