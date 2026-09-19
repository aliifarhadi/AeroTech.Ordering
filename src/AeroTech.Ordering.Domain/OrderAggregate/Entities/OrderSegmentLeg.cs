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
            LegId = source.LegId;
            Sequence = source.Sequence;
            OriginAirportId = source.OriginAirportId;
            OriginAirportTerminalId = source.OriginAirportTerminalId;
            DestinationAirportId = source.DestinationAirportId;
            DestinationAirportTerminalId = source.DestinationAirportTerminalId;
            DepartureDateTime = source.DepartureDateTime;
            ArrivalDateTime = source.ArrivalDateTime;
        }

        public long SegmentId { get; private set; }

        public long LegId { get; private set; }

        public int Sequence { get; private set; }

        public int? OriginAirportId { get; private set; }

        public int? OriginAirportTerminalId { get; private set; }

        public int? DestinationAirportId { get; private set; }

        public int? DestinationAirportTerminalId { get; private set; }

        public DateTimeOffset? DepartureDateTime { get; private set; }

        public DateTimeOffset? ArrivalDateTime { get; private set; }
    }
}
