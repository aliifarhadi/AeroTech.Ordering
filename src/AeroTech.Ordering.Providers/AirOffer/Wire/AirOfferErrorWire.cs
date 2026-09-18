using System.Text.Json;
using System.Text.Json.Serialization;

namespace AeroTech.Ordering.Providers.AirOffer.Wire
{
    public sealed class AirOfferErrorWire
    {
        public int? Code { get; set; }

        public string? Title { get; set; }

        public string? Detail { get; set; }
    }
}
