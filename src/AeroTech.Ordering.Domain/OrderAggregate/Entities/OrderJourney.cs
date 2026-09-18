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
            SourceBoundRef = source.JourneyRef;
            Sequence = source.Sequence;
            SourceDirectionRaw = source.SourceDirectionRaw;
            Direction = source.Direction;
            OriginRef = source.OriginRef;
            DestinationRef = source.DestinationRef;
        }

        public long OrderId { get; private set; }

        public string SourceBoundRef { get; private set; } = null!;

        public int Sequence { get; private set; }

        public string? SourceDirectionRaw { get; private set; }

        public BoundDirection? Direction { get; private set; }

        public string OriginRef { get; private set; } = null!;

        public string DestinationRef { get; private set; } = null!;
    }
}
