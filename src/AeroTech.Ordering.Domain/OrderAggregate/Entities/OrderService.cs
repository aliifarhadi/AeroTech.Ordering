using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class OrderService : Entity<long>
    {
        private OrderService()
        {
        }

        internal OrderService(
            long id,
            long orderId,
            long orderItemId,
            long travellerId,
            long segmentId,
            CandidateService source,
            long changeId)
        {
            Id = id;
            OrderId = orderId;
            OrderItemId = orderItemId;
            TravellerId = travellerId;
            SegmentId = segmentId;
            CabinClassId = source.CabinClassId;
            RbdId = source.RbdId;
            BookingClass = source.BookingClass;
            CheckedBaggage = source.CheckedBaggage;
            CabinBaggage = source.CabinBaggage;
            SoldTerms = source.SoldTerms;
            CommercialStatus = OrderServiceCommercialStatus.Active;
            CreatedByChangeId = changeId;
        }

        public long OrderId { get; private set; }

        public long OrderItemId { get; private set; }

        public long TravellerId { get; private set; }

        public long SegmentId { get; private set; }

        public int? CabinClassId { get; private set; }

        public long? RbdId { get; private set; }

        public string? BookingClass { get; private set; }

        public BaggageAllowance? CheckedBaggage { get; private set; }

        public BaggageAllowance? CabinBaggage { get; private set; }

        public SoldTermFlags SoldTerms { get; private set; } = null!;

        public OrderServiceCommercialStatus CommercialStatus { get; private set; }

        public long CreatedByChangeId { get; private set; }
    }
}
