using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class OrderAirTransportService : OrderService
    {
        private OrderAirTransportService()
        {
        }

        internal OrderAirTransportService(
            long id,
            long orderId,
            long orderItemId,
            long travellerId,
            long segmentId,
            CandidateService source,
            long changeId)
            : base(id, orderId, orderItemId, OrderServiceType.AirTransportation, source.FulfillmentProfile.ToSnapshot(), changeId)
        {
            if (travellerId <= 0)
                throw ExceptionFactory.AirServiceScopeInvalid("it must name exactly one accepted traveller");

            if (segmentId <= 0)
                throw ExceptionFactory.AirServiceScopeInvalid("it must name exactly one accepted passenger segment");

            TravellerId = travellerId;
            SegmentId = segmentId;
            CabinClassId = source.CabinClassId;
            RbdId = source.RbdId;
            BookingClass = source.BookingClass;
            CheckedBaggage = source.CheckedBaggage;
            CabinBaggage = source.CabinBaggage;
            SoldTerms = source.SoldTerms;
        }

        public long TravellerId { get; private set; }

        public long SegmentId { get; private set; }

        public int? CabinClassId { get; private set; }

        public long? RbdId { get; private set; }

        public string? BookingClass { get; private set; }

        public BaggageAllowance? CheckedBaggage { get; private set; }

        public BaggageAllowance? CabinBaggage { get; private set; }

        public SoldTermFlags SoldTerms { get; private set; } = null!;
    }
}
