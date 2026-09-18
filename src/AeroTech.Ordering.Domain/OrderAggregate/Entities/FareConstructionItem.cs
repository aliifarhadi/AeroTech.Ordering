using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class FareConstructionItem : Entity<long>
    {
        private FareConstructionItem()
        {
        }

        internal FareConstructionItem(long id, long fareConstructionId, long orderItemId)
        {
            Id = id;
            FareConstructionId = fareConstructionId;
            OrderItemId = orderItemId;
        }

        public long FareConstructionId { get; private set; }

        public long OrderItemId { get; private set; }
    }
}
