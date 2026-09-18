using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class FarePricingGroupTraveler : Entity<long>
    {
        private FarePricingGroupTraveler()
        {
        }

        internal FarePricingGroupTraveler(long id, long pricingGroupId, long travelerId)
        {
            Id = id;
            PricingGroupId = pricingGroupId;
            TravelerId = travelerId;
        }

        public long PricingGroupId { get; private set; }

        public long TravelerId { get; private set; }
    }
}
