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

        internal OrderSegment(long id, long orderId, long journeyId, CandidateSegment source, Func<long> newId)
        {
            Id = id;
            OrderId = orderId;
            JourneyId = journeyId;
            Sequence = source.Sequence;
            Kind = source.Kind;
            OriginAirportId = source.OriginAirportId;
            OriginAirportTerminalId = source.OriginAirportTerminalId;
            DestinationAirportId = source.DestinationAirportId;
            DestinationAirportTerminalId = source.DestinationAirportTerminalId;
            SoldDeparture = source.SoldDeparture;
            SoldArrival = source.SoldArrival;
            FlightId = source.FlightId;
            FlightNumber = source.FlightNumber;
            FlightVersion = source.FlightVersion;
            MarketingAirlineId = source.MarketingAirlineId;
            OperatingAirlineId = source.OperatingAirlineId;
            FlightCapacityId = source.FlightCapacityId;
            Duration = source.Duration;
            AircraftId = source.AircraftId;

            foreach (var leg in source.Legs)
                _legs.Add(new OrderSegmentLeg(newId(), id, leg));
        }

        public long OrderId { get; private set; }

        public long JourneyId { get; private set; }

        public int Sequence { get; private set; }

        public SegmentKind Kind { get; private set; }

        public int OriginAirportId { get; private set; }

        public int? OriginAirportTerminalId { get; private set; }

        public int DestinationAirportId { get; private set; }

        public int? DestinationAirportTerminalId { get; private set; }

        public DateTimeOffset? SoldDeparture { get; private set; }

        public DateTimeOffset? SoldArrival { get; private set; }

        public long? FlightId { get; private set; }

        public string? FlightNumber { get; private set; }

        public int? FlightVersion { get; private set; }

        public int? MarketingAirlineId { get; private set; }

        public int? OperatingAirlineId { get; private set; }

        public long? FlightCapacityId { get; private set; }

        public int? Duration { get; private set; }

        public int? AircraftId { get; private set; }

        public IReadOnlyCollection<OrderSegmentLeg> Legs => _legs.AsReadOnly();
    }
}
