using AeroTech.Ordering.Domain.Ports.Offers;

namespace AeroTech.Ordering.Providers.Deterministic.Offers
{
    public static class ReferenceOfferProfile
    {
        public const string Owner = "AirOffer";
        public const string ProfileId = "REFERENCE-OFFER-2.0";
        public const string ContractVersion = "reference-offer-target-contract-v1";
        public const string ResolveOperation = "ResolveCandidate";
        public const string ReadBoundOperation = "ReadBoundCandidate";

        public static readonly OfferSourceProfile Profile = new(ProfileId, ContractVersion, ProfileId);
    }
}
