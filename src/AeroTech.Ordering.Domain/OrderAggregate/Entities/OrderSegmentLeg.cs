using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class OrderSegmentLeg : Entity<long>
    {
        private OrderSegmentLeg()
        {
        }

        internal OrderSegmentLeg(long id, long segmentId, CandidateSegmentLeg source)
        {
            Id = id;
            SegmentId = segmentId;
            Sequence = source.Sequence;
            SourceLegRef = source.SourceLegRef;
            OriginRef = source.OriginRef;
            OriginTerminalRef = source.OriginTerminalRef;
            DestinationRef = source.DestinationRef;
            DestinationTerminalRef = source.DestinationTerminalRef;
            Departure = source.Departure;
            Arrival = source.Arrival;
        }

        public long SegmentId { get; private set; }

        public int Sequence { get; private set; }

        public string SourceLegRef { get; private set; } = null!;

        public string? OriginRef { get; private set; }

        public string? OriginTerminalRef { get; private set; }

        public string? DestinationRef { get; private set; }

        public string? DestinationTerminalRef { get; private set; }

        public DateTimeOffset? Departure { get; private set; }

        public DateTimeOffset? Arrival { get; private set; }
    }
}
