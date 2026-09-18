using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class OrderItemServiceLinkTraveler : Entity<long>
    {
        private OrderItemServiceLinkTraveler()
        {
        }

        internal OrderItemServiceLinkTraveler(long id, long linkId, long travelerId)
        {
            Id = id;
            LinkId = linkId;
            TravelerId = travelerId;
        }

        public long LinkId { get; private set; }

        public long TravelerId { get; private set; }
    }
}
