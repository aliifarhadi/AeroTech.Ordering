using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class OrderServiceBeneficiary : Entity<long>
    {
        private OrderServiceBeneficiary()
        {
        }

        internal OrderServiceBeneficiary(long id, long serviceId, long travelerId)
        {
            Id = id;
            ServiceId = serviceId;
            TravelerId = travelerId;
        }

        public long ServiceId { get; private set; }

        public long TravelerId { get; private set; }
    }
}
