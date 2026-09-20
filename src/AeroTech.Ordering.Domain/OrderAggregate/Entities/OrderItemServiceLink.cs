using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class OrderItemServiceLink : Entity<long>
    {
        private OrderItemServiceLink()
        {
        }

        internal OrderItemServiceLink(long id, long orderIdAtAssociation, long orderItemId, long orderServiceId, long linkedByChangeId)
        {
            Id = id;
            OrderIdAtAssociation = orderIdAtAssociation;
            OrderItemId = orderItemId;
            OrderServiceId = orderServiceId;
            LinkedByChangeId = linkedByChangeId;
        }

        public long OrderIdAtAssociation { get; private set; }

        public long OrderItemId { get; private set; }

        public long OrderServiceId { get; private set; }

        public long LinkedByChangeId { get; private set; }
    }
}
