namespace AeroTech.Ordering.Domain.Ports.Offers
{
    public enum OfferResolutionOutcome
    {
        Resolved = 1,
        NotFound = 2,
        ContractMismatch = 3,
        UnsupportedCapability = 4,
        Unavailable = 5
    }
}
