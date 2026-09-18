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

        internal OrderSegment(long id, long orderId, long journeyId, int sequence, CandidateSegment source, Func<long> newId)
        {
            Id = id;
            OrderId = orderId;
            JourneyId = journeyId;
            Sequence = sequence;
            SourceSegmentRef = source.SegmentRef;
            Kind = source.Kind;
            OriginRef = source.OriginRef;
            OriginTerminalRef = source.OriginTerminalRef;
            DestinationRef = source.DestinationRef;
            DestinationTerminalRef = source.DestinationTerminalRef;
            SoldDeparture = source.SoldDeparture;
            SoldArrival = source.SoldArrival;
            FlightRef = source.FlightRef;
            FlightNumber = source.FlightNumber;
            FlightVersion = source.FlightVersion;
            MarketingCarrierRef = source.MarketingCarrierRef;
            OperatingCarrierRef = source.OperatingCarrierRef;
            SourceCapacityRef = source.SourceCapacityRef;
            Duration = source.Duration;
            AircraftRef = source.AircraftRef;

            foreach (var leg in source.Legs)
                _legs.Add(new OrderSegmentLeg(newId(), id, leg));
        }

        public long OrderId { get; private set; }

        public long? JourneyId { get; private set; }

        public int Sequence { get; private set; }

        public string SourceSegmentRef { get; private set; } = null!;

        public SegmentKind Kind { get; private set; }

        public string OriginRef { get; private set; } = null!;

        public string? OriginTerminalRef { get; private set; }

        public string DestinationRef { get; private set; } = null!;

        public string? DestinationTerminalRef { get; private set; }

        public DateTimeOffset? SoldDeparture { get; private set; }

        public DateTimeOffset? SoldArrival { get; private set; }

        public string? FlightRef { get; private set; }

        public string? FlightNumber { get; private set; }

        public string? FlightVersion { get; private set; }

        public string? MarketingCarrierRef { get; private set; }

        public string? OperatingCarrierRef { get; private set; }

        public string? SourceCapacityRef { get; private set; }

        public int? Duration { get; private set; }

        public string? AircraftRef { get; private set; }

        public IReadOnlyCollection<OrderSegmentLeg> Legs => _legs.AsReadOnly();
    }
}
