using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.Ports.Offers
{
    public sealed record ReadBoundCandidateRequest(
        string OwnerBindingRef,
        string ExpectedSourceDigest,
        AuthorizedSalesScope AuthorizedSalesContext);
}
