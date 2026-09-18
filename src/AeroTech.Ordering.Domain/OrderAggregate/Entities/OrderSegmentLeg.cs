using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class OrderSegmentLeg : Entity<long>
    {
        private OrderSegmentLeg()
        {
        }

        internal OrderSegmentLeg(long id, long segmentId, int sequence, string sourceLegRef)
        {
            Id = id;
            SegmentId = segmentId;
            Sequence = sequence;
            SourceLegRef = sourceLegRef;
        }

        public long SegmentId { get; private set; }

        public int Sequence { get; private set; }

        public string SourceLegRef { get; private set; } = null!;
    }
}
