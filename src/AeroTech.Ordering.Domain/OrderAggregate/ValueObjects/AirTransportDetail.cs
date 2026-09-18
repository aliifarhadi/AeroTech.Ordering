using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.ValueObjects
{
    public sealed record AirTransportDetail
    {
        private AirTransportDetail()
        {
        }

        public AirTransportDetail(
            string? cabinRef,
            string? rbdRef,
            string? bookingClass,
            BaggageAllowance? checkedBaggage,
            BaggageAllowance? cabinBaggage)
        {
            CabinRef = cabinRef;
            RbdRef = rbdRef;
            BookingClass = bookingClass;
            CheckedBaggage = checkedBaggage;
            CabinBaggage = cabinBaggage;
        }

        public string? CabinRef { get; private set; }

        public string? RbdRef { get; private set; }

        public string? BookingClass { get; private set; }

        public BaggageAllowance? CheckedBaggage { get; private set; }

        public BaggageAllowance? CabinBaggage { get; private set; }
    }
}
