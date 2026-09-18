using System.Text.Json;
using System.Text.Json.Serialization;

namespace AeroTech.Ordering.Providers.AirOffer.Wire
{
    public sealed class AirOfferDetailsWire
    {
        public string? OfferId { get; set; }

        public DateTimeOffset PricedAt { get; set; }

        public DateTimeOffset? LastTicketingDate { get; set; }

        public int CurrencyId { get; set; }

        public string? CurrencyCode { get; set; }

        public string? JourneyType { get; set; }

        public decimal BaseAmount { get; set; }

        public decimal ChargeAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public List<AirOfferAirTransportWire> AirTransports { get; set; } = [];

        public List<AirOfferPricingUnitWire> PricingUnits { get; set; } = [];

        public List<AirOfferTicketWire> Tickets { get; set; } = [];

        public List<AirOfferPricingLineWire> OrderCharges { get; set; } = [];

        public List<AirOfferRateOfExchangeWire> RatesOfExchange { get; set; } = [];
    }
}
