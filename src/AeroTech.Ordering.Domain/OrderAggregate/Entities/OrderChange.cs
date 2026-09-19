using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Aegis.Enums;
using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class OrderChange : Entity<long>
    {
        private OrderChange()
        {
        }

        internal OrderChange(
            long id,
            long orderId,
            OrderChangeType type,
            int commercialVersion,
            BusinessContextType actorContextType,
            long? actorId,
            DateTimeOffset committedAt)
        {
            Id = id;
            OrderId = orderId;
            Type = type;
            CommercialVersion = commercialVersion;
            ActorContextType = actorContextType;
            ActorId = actorId;
            CommittedAt = committedAt;
        }

        public long OrderId { get; private set; }

        public OrderChangeType Type { get; private set; }

        public int CommercialVersion { get; private set; }

        public BusinessContextType ActorContextType { get; private set; }

        public long? ActorId { get; private set; }

        public DateTimeOffset CommittedAt { get; private set; }
    }
}
