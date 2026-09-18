using System.Text.Json;
using System.Text.Json.Serialization;

namespace AeroTech.Ordering.Providers.AirOffer.Wire
{
    public sealed class AirOfferPricingUnitWire
    {
        public string? Kind { get; set; }

        public List<string> CoveredBoundOfferIds { get; set; } = [];

        public List<AirOfferFareComponentWire> FareComponents { get; set; } = [];
    }
}
