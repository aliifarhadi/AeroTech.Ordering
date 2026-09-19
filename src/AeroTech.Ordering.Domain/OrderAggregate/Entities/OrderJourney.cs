using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class OrderJourney : Entity<long>
    {
        private OrderJourney()
        {
        }

        internal OrderJourney(long id, long orderId, CandidateJourney source)
        {
            Id = id;
            OrderId = orderId;
            BoundId = source.BoundId;
            Sequence = source.Sequence;
            Direction = source.Direction;
            OriginAirportId = source.OriginAirportId;
            DestinationAirportId = source.DestinationAirportId;
        }

        public long OrderId { get; private set; }

        public string BoundId { get; private set; } = null!;

        public int Sequence { get; private set; }

        public BoundDirection Direction { get; private set; }

        public int OriginAirportId { get; private set; }

        public int DestinationAirportId { get; private set; }
    }
}
