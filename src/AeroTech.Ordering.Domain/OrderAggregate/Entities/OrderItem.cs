using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class OrderItem : Entity<long>
    {
        private OrderItem()
        {
        }

        internal OrderItem(long id, long orderId, CandidateItem source, long changeId)
        {
            Id = id;
            OrderId = orderId;
            Kind = source.ItemKind;
            AcceptedTotal = source.AcceptedTotal;
            CreatedByChangeId = changeId;
            CommercialStatus = OrderItemCommercialStatus.Active;
        }

        public long OrderId { get; private set; }

        public OrderItemKind Kind { get; private set; }

        public Money AcceptedTotal { get; private set; } = null!;

        public long CreatedByChangeId { get; private set; }

        public OrderItemCommercialStatus CommercialStatus { get; private set; }
    }
}
