using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class FareComponent : Entity<long>
    {
        private FareComponent()
        {
        }

        internal FareComponent(long id, long pricingUnitId, int sequence, CandidateFareComponent source)
        {
            Id = id;
            PricingUnitId = pricingUnitId;
            Sequence = sequence;
            AirFareId = source.AirFareId;
            FareBasis = source.FareBasis;
            FareFamily = source.FareFamily;
            FareType = source.FareType;
            CabinClassId = source.CabinClassId;
            RbdId = source.RbdId;
            BookingClass = source.BookingClass;
            TicketingRestrictionMinutes = source.TicketingRestrictionMinutes;
        }

        public long PricingUnitId { get; private set; }

        public int Sequence { get; private set; }

        public long AirFareId { get; private set; }

        public string? FareBasis { get; private set; }

        public string? FareFamily { get; private set; }

        public string? FareType { get; private set; }

        public int? CabinClassId { get; private set; }

        public long? RbdId { get; private set; }

        public string? BookingClass { get; private set; }

        public int? TicketingRestrictionMinutes { get; private set; }
    }
}
