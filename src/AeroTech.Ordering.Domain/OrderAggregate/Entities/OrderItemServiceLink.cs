using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class OrderItemServiceLink : Entity<long>
    {
        private readonly List<OrderItemServiceLinkTraveler> _travelersAtAssociation = new();
        private readonly List<OrderItemServiceLinkSegment> _segmentsAtAssociation = new();

        private OrderItemServiceLink()
        {
        }

        internal OrderItemServiceLink(
            long id,
            long orderIdAtAssociation,
            long orderItemId,
            OrderService service,
            long linkedByChangeId,
            Func<long> newId)
        {
            Id = id;
            OrderIdAtAssociation = orderIdAtAssociation;
            OrderItemId = orderItemId;
            OrderServiceId = service.Id;
            LinkedByChangeId = linkedByChangeId;

            foreach (var beneficiary in service.Beneficiaries)
                _travelersAtAssociation.Add(new OrderItemServiceLinkTraveler(newId(), id, beneficiary.TravelerId));

            foreach (var coverage in service.Coverage)
                _segmentsAtAssociation.Add(new OrderItemServiceLinkSegment(newId(), id, coverage.SegmentId));
        }

        public long OrderIdAtAssociation { get; private set; }

        public long OrderItemId { get; private set; }

        public long OrderServiceId { get; private set; }

        public long LinkedByChangeId { get; private set; }

        public IReadOnlyCollection<OrderItemServiceLinkTraveler> TravelersAtAssociation => _travelersAtAssociation.AsReadOnly();

        public IReadOnlyCollection<OrderItemServiceLinkSegment> SegmentsAtAssociation => _segmentsAtAssociation.AsReadOnly();
    }
}
