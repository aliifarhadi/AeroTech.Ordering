using System.Text.Json;
using System.Text.Json.Serialization;

namespace AeroTech.Ordering.Providers.AirOffer.Wire
{
    public sealed class AirOfferPricingLineWire
    {
        public JsonElement Category { get; set; }

        public string? Name { get; set; }

        public string? Code { get; set; }

        public string? Reference { get; set; }

        public decimal Amount { get; set; }

        public int CurrencyId { get; set; }

        public bool IsPercentage { get; set; }

        public decimal EquivalentAmount { get; set; }

        public int EquivalentCurrencyId { get; set; }

        public string? RateOfExchangePeriodId { get; set; }
    }
}
