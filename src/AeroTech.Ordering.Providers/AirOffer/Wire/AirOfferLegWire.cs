using System.Text.Json;
using System.Text.Json.Serialization;

namespace AeroTech.Ordering.Providers.AirOffer.Wire
{
    public sealed class AirOfferLegWire
    {
        public int Sequence { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public long LegId { get; set; }

        public int OriginAirportId { get; set; }

        public int? OriginAirportTerminalId { get; set; }

        public int DestinationAirportId { get; set; }

        public int? DestinationAirportTerminalId { get; set; }

        public DateTimeOffset DepartureDateTime { get; set; }

        public DateTimeOffset ArrivalDateTime { get; set; }

        public JsonElement? Stop { get; set; }
    }
}
