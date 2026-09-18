using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class OrderSegment : Entity<long>
    {
        private readonly List<OrderSegmentLeg> _legs = new();

        private OrderSegment()
        {
        }

        internal OrderSegment(long id, long orderId, int sequence, CandidateSegment source, Func<long> newId)
        {
            Id = id;
            OrderId = orderId;
            Sequence = sequence;
            SourceSegmentRef = source.SegmentRef;
            Kind = source.Kind;
            OriginRef = source.OriginRef;
            DestinationRef = source.DestinationRef;
            SoldDeparture = source.SoldDeparture;
            SoldArrival = source.SoldArrival;
            FlightRef = source.FlightRef;

            for (var index = 0; index < source.OperationalLegRefs.Count; index++)
                _legs.Add(new OrderSegmentLeg(newId(), id, index + 1, source.OperationalLegRefs[index]));
        }

        public long OrderId { get; private set; }

        public int Sequence { get; private set; }

        public string SourceSegmentRef { get; private set; } = null!;

        public SegmentKind Kind { get; private set; }

        public string OriginRef { get; private set; } = null!;

        public string DestinationRef { get; private set; } = null!;

        public DateTimeOffset? SoldDeparture { get; private set; }

        public DateTimeOffset? SoldArrival { get; private set; }

        public string? FlightRef { get; private set; }

        public IReadOnlyCollection<OrderSegmentLeg> Legs => _legs.AsReadOnly();
    }
}
