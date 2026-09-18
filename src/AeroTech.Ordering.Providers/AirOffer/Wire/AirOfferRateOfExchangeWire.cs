using System.Text.Json;
using System.Text.Json.Serialization;

namespace AeroTech.Ordering.Providers.AirOffer.Wire
{
    public sealed class AirOfferRateOfExchangeWire
    {
        public string? RateOfExchangePeriodId { get; set; }

        public int FromCurrencyId { get; set; }

        public int ToCurrencyId { get; set; }

        public decimal Rate { get; set; }

        public int DecimalPlaces { get; set; }

        public JsonElement RoundingFactor { get; set; }
    }
}
