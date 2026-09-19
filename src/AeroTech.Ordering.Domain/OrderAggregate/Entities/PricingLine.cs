using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class PricingLine : Entity<long>
    {
        private PricingLine()
        {
        }

        internal PricingLine(long id, long orderId, long priceChangeSetId, long? orderItemId, long? basisId, CandidatePricingLine source)
        {
            Id = id;
            OrderId = orderId;
            PriceChangeSetId = priceChangeSetId;
            OrderItemId = orderItemId;
            SourceOccurrencePath = source.SourceOccurrencePath;
            Component = source.Component;
            Effect = source.Effect;
            Direction = source.Direction;
            Code = source.Code;
            Name = source.Name;
            Reference = source.Reference;
            CalculationKind = source.CalculationKind;
            OriginalValue = source.OriginalValue;
            SaleValue = source.SaleValue;
            BasisType = source.BasisType;
            BasisId = basisId;
            SourceConversionRef = source.SourceConversionRef;
            AppliedConversion = source.AppliedConversion;
            SettlementAttribution = source.SettlementAttribution;
        }

        public long OrderId { get; private set; }

        public long PriceChangeSetId { get; private set; }

        public long? OrderItemId { get; private set; }

        public string SourceOccurrencePath { get; private set; } = null!;

        public PricingComponentType Component { get; private set; }

        public PricingEffect Effect { get; private set; }

        public OrderPricingLineDirection Direction { get; private set; }

        public string? Code { get; private set; }

        public string? Name { get; private set; }

        public string? Reference { get; private set; }

        public PricingCalculationKind CalculationKind { get; private set; }

        public Money OriginalValue { get; private set; } = null!;

        public Money SaleValue { get; private set; } = null!;

        public PricingBasisType BasisType { get; private set; }

        public long? BasisId { get; private set; }

        public string? SourceConversionRef { get; private set; }

        public AppliedConversion? AppliedConversion { get; private set; }

        public SettlementAttribution? SettlementAttribution { get; private set; }
    }
}
