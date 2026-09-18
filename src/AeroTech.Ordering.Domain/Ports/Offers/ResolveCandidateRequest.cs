using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.Ports.Offers
{
    public sealed record ResolveCandidateRequest(
        string OfferId,
        AuthorizedSalesScope AuthorizedSalesContext,
        IReadOnlyList<string> RequestedSelection);
}
