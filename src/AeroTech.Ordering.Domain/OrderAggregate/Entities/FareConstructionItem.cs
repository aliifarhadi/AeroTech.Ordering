namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class FareConstructionItem
    {
        private FareConstructionItem()
        {
        }

        internal FareConstructionItem(long fareConstructionId, long orderItemId)
        {
            FareConstructionId = fareConstructionId;
            OrderItemId = orderItemId;
        }

        public long FareConstructionId { get; private set; }

        public long OrderItemId { get; private set; }
    }
}
