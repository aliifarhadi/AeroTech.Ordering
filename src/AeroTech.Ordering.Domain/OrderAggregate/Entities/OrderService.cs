using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public abstract class OrderService : Entity<long>
    {
        protected OrderService()
        {
        }

        protected OrderService(
            long id,
            long orderId,
            long orderItemId,
            OrderServiceType serviceType,
            FulfillmentProfileSnapshot fulfillmentProfile,
            long changeId)
        {
            Id = id;
            OrderId = orderId;
            OrderItemId = orderItemId;
            ServiceType = serviceType;
            FulfillmentProfile = fulfillmentProfile;
            CommercialStatus = OrderServiceCommercialStatus.Active;
            CreatedByChangeId = changeId;
        }

        public long OrderId { get; private set; }

        public long OrderItemId { get; private set; }

        public OrderServiceType ServiceType { get; private set; }

        public FulfillmentProfileSnapshot FulfillmentProfile { get; private set; } = null!;

        public OrderServiceCommercialStatus CommercialStatus { get; private set; }

        public long CreatedByChangeId { get; private set; }
    }
}
