using System.Text.Json;
using System.Text.Json.Serialization;

namespace AeroTech.Ordering.Providers.AirOffer.Wire
{
    public sealed class AirOfferFareComponentWire
    {
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public long AirFareId { get; set; }

        public int? CabinClassId { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public long? RbdId { get; set; }

        public string? BookingClass { get; set; }

        public string? FareBasis { get; set; }

        public string? FareFamily { get; set; }

        public string? FareType { get; set; }

        public int? TicketingRestrictionMinutes { get; set; }
    }
}
