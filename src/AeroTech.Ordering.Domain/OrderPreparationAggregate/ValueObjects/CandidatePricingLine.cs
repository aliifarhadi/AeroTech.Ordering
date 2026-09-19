using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidatePricingLine(
        string SourceOccurrencePath,
        string? ItemKey,
        PricingComponentType Component,
        PricingEffect Effect,
        OrderPricingLineDirection Direction,
        string? Code,
        string? Name,
        string? Reference,
        PricingCalculationKind CalculationKind,
        Money OriginalValue,
        Money SaleValue,
        PricingBasisType BasisType,
        string BasisKey,
        string? SourceConversionRef,
        AppliedConversion? AppliedConversion,
        SettlementAttribution? SettlementAttribution);
}
