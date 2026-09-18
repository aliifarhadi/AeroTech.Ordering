namespace AeroTech.Ordering.Providers.AirOffer.Options
{
    public sealed class AirOfferOptions
    {
        public const string SectionName = "Offer";

        public string BaseUrl { get; set; } = default!;

        public int RequestTimeout { get; set; }

        public int RetryCount { get; set; }

        public int RetryInterval { get; set; }
    }
}
