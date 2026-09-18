using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class FareComponent : Entity<long>
    {
        private readonly List<FareComponentService> _coveredServices = new();
        private readonly List<FareComponentSegment> _coveredSegments = new();

        private FareComponent()
        {
        }

        internal FareComponent(
            long id,
            long pricingUnitId,
            int sequence,
            CandidateFareComponent source,
            IReadOnlyDictionary<string, long> serviceIds,
            IReadOnlyDictionary<string, long> segmentIds,
            Func<long> newId)
        {
            Id = id;
            PricingUnitId = pricingUnitId;
            Sequence = sequence;
            SourceFareRef = source.SourceFareRef;
            FareBasis = source.FareBasis;
            FareFamily = source.FareFamily;
            FareType = source.FareType;
            CabinRef = source.CabinRef;
            RbdRef = source.RbdRef;
            BookingClass = source.BookingClass;
            TicketingRestrictionMinutes = source.TicketingRestrictionMinutes;
            FareOwnerRef = source.FareOwnerRef;
            TariffRef = source.TariffRef;
            RuleRef = source.RuleRef;
            RoutingRef = source.RoutingRef;

            foreach (var serviceRef in source.CoveredServiceRefs)
                _coveredServices.Add(new FareComponentService(newId(), id, serviceIds[serviceRef]));

            foreach (var segmentRef in source.CoveredSegmentRefs)
                _coveredSegments.Add(new FareComponentSegment(newId(), id, segmentIds[segmentRef]));
        }

        public long PricingUnitId { get; private set; }

        public int Sequence { get; private set; }

        public string SourceFareRef { get; private set; } = null!;

        public string? FareBasis { get; private set; }

        public string? FareFamily { get; private set; }

        public string? FareType { get; private set; }

        public string? CabinRef { get; private set; }

        public string? RbdRef { get; private set; }

        public string? BookingClass { get; private set; }

        public int? TicketingRestrictionMinutes { get; private set; }

        public string? FareOwnerRef { get; private set; }

        public string? TariffRef { get; private set; }

        public string? RuleRef { get; private set; }

        public string? RoutingRef { get; private set; }

        public IReadOnlyCollection<FareComponentService> CoveredServices => _coveredServices.AsReadOnly();

        public IReadOnlyCollection<FareComponentSegment> CoveredSegments => _coveredSegments.AsReadOnly();
    }
}
