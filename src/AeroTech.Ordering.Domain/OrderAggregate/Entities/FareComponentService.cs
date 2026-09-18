using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class FareComponentService : Entity<long>
    {
        private FareComponentService()
        {
        }

        internal FareComponentService(long id, long fareComponentId, long orderServiceId)
        {
            Id = id;
            FareComponentId = fareComponentId;
            OrderServiceId = orderServiceId;
        }

        public long FareComponentId { get; private set; }

        public long OrderServiceId { get; private set; }
    }
}
