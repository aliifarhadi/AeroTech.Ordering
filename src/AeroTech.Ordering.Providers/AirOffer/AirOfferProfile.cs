using AeroTech.Ordering.Domain.Ports.Offers;

namespace AeroTech.Ordering.Providers.AirOffer
{
    public static class AirOfferProfile
    {
        public const string Owner = "AirOffer";
        public const string LiveCandidateSandbox = "LIVE-CANDIDATE-SANDBOX";
        public const string ContractVersion = "airoffer-service-flightoffers-details-observed-v1";
        public const string DetailsRoute = "v1/FlightOffers/Details";

        public static readonly OfferSourceProfile Profile = new(LiveCandidateSandbox, ContractVersion, LiveCandidateSandbox);
    }
}
