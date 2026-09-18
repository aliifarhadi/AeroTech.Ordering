using System.Text.Json;
using System.Text.Json.Serialization;

namespace AeroTech.Ordering.Providers.AirOffer.Wire
{
    public sealed class AirOfferFlightWire
    {
        public int Sequence { get; set; }

        public int? CabinClassId { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public long? RbdId { get; set; }

        public string? BookingClass { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public long FlightCapacityId { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public long FlightId { get; set; }

        public int FlightVersion { get; set; }

        public string? FlightNumber { get; set; }

        public int OriginAirportId { get; set; }

        public int? OriginAirportTerminalId { get; set; }

        public int DestinationAirportId { get; set; }

        public int? DestinationAirportTerminalId { get; set; }

        public int OperatingAirlineId { get; set; }

        public int MarketingAirlineId { get; set; }

        public DateTimeOffset DepartureDateTime { get; set; }

        public DateTimeOffset ArrivalDateTime { get; set; }

        public int Duration { get; set; }

        public int AircraftId { get; set; }

        public JsonElement? Stop { get; set; }

        public List<AirOfferLegWire> Legs { get; set; } = [];
    }
}
