using System.Text.Json;
using System.Text.Json.Serialization;

namespace AeroTech.Ordering.Providers.AirOffer.Wire
{
    public sealed class AirOfferEnvelopeWire
    {
        public AirOfferDetailsWire? Data { get; set; }

        public List<AirOfferErrorWire>? Errors { get; set; }
    }
}
