using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class OrderServiceCoverage : Entity<long>
    {
        private OrderServiceCoverage()
        {
        }

        internal OrderServiceCoverage(long id, long serviceId, long segmentId)
        {
            Id = id;
            ServiceId = serviceId;
            SegmentId = segmentId;
        }

        public long ServiceId { get; private set; }

        public long SegmentId { get; private set; }
    }
}
