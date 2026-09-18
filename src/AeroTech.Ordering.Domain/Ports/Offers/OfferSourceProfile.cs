namespace AeroTech.Ordering.Domain.Ports.Offers
{
    public sealed record OfferSourceProfile(
        string ProviderProfileId,
        string ContractVersion,
        string AcceptanceProfile);
}
