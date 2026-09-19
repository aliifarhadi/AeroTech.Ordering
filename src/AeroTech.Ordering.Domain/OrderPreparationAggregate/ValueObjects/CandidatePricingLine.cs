using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidatePricingLine(
        string LineRef,
        string? ItemRef,
        PricingComponentType Component,
        PricingEffect Effect,
        OrderPricingLineDirection Direction,
        PricingLineRole LineRole,
        string? SourceCode,
        string? SourceName,
        string? SourceReference,
        PricingCalculationKind CalculationKind,
        Money OriginalValue,
        Money SaleValue,
        string SourceLineRef,
        PricingBasisType BasisType,
        string BasisRef,
        string? SourceConversionRef,
        AppliedConversion? AppliedConversion,
        SettlementAttribution? SettlementAttribution);
}
