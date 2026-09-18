using System.Text.Json;
using System.Text.Json.Serialization;

namespace AeroTech.Ordering.Providers.AirOffer.Wire
{
    public sealed class AirOfferTicketWire
    {
        public string TravellerRef { get; set; } = string.Empty;

        public int TravellerIndex { get; set; }

        public string PassengerTypeCode { get; set; } = string.Empty;

        public decimal BaseAmount { get; set; }

        public decimal ChargeAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public List<AirOfferCouponWire> Coupons { get; set; } = [];
    }
}
