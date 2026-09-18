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
            SourceLineRef = source.SourceLineRef;
            CandidateLineRef = source.LineRef;
            Component = source.Component;
            Effect = source.Effect;
            Direction = source.Direction;
            Role = source.LineRole;
            OriginalValue = source.OriginalValue;
            SaleValue = source.SaleValue;
            BasisType = source.BasisType;
            BasisId = basisId;
            SourceBasisRef = source.BasisRef;
            SourceConversionRef = source.SourceConversionRef;
        }

        public long OrderId { get; private set; }

        public long PriceChangeSetId { get; private set; }

        public long? OrderItemId { get; private set; }

        public string SourceLineRef { get; private set; } = null!;

        public string CandidateLineRef { get; private set; } = null!;

        public PricingComponentType Component { get; private set; }

        public PricingEffect Effect { get; private set; }

        public OrderPricingLineDirection Direction { get; private set; }

        public PricingLineRole Role { get; private set; }

        public Money OriginalValue { get; private set; } = null!;

        public Money SaleValue { get; private set; } = null!;

        public PricingBasisType BasisType { get; private set; }

        public long? BasisId { get; private set; }

        public string SourceBasisRef { get; private set; } = null!;

        public string? SourceConversionRef { get; private set; }

        public long? OriginalPricingLineId { get; private set; }
    }
}
