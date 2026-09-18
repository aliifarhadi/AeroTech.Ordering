using System.Text.Json;
using System.Text.Json.Serialization;

namespace AeroTech.Ordering.Providers.AirOffer.Wire
{
    public sealed class AirOfferCouponWire
    {
        public string CouponId { get; set; } = string.Empty;

        public int Sequence { get; set; }

        public string BoundId { get; set; } = string.Empty;

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public long FlightId { get; set; }

        public int BaggagePieces { get; set; }

        public int BaggageWeight { get; set; }

        public string? BaggageUnit { get; set; }

        public int CabinBaggagePieces { get; set; }

        public int CabinBaggageWeight { get; set; }

        public string? CabinBaggageUnit { get; set; }

        public bool IsRefundable { get; set; }

        public bool IsChangeable { get; set; }

        public bool IsUpgradable { get; set; }

        public decimal BaseAmount { get; set; }

        public decimal ChargeAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public List<AirOfferPricingLineWire> Pricings { get; set; } = [];
    }
}
