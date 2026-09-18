using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class OrderItemServiceLinkSegment : Entity<long>
    {
        private OrderItemServiceLinkSegment()
        {
        }

        internal OrderItemServiceLinkSegment(long id, long linkId, long segmentId)
        {
            Id = id;
            LinkId = linkId;
            SegmentId = segmentId;
        }

        public long LinkId { get; private set; }

        public long SegmentId { get; private set; }
    }
}
