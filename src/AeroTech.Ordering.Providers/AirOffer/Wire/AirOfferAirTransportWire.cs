using System.Text.Json;
using System.Text.Json.Serialization;

namespace AeroTech.Ordering.Providers.AirOffer.Wire
{
    public sealed class AirOfferAirTransportWire
    {
        public string BoundId { get; set; } = string.Empty;

        public JsonElement Direction { get; set; }

        public int Sequence { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int OriginAirportId { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int DestinationAirportId { get; set; }

        public List<AirOfferFlightWire> Flights { get; set; } = [];
    }
}
