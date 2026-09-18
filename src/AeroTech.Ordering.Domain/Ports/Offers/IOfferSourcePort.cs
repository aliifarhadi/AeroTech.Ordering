namespace AeroTech.Ordering.Domain.Ports.Offers
{
    public interface IOfferSourcePort
    {
        Task<CandidateResolution> ResolveCandidateAsync(ResolveCandidateRequest request, CancellationToken cancellationToken = default);

        Task<BoundCandidateEvidence> ReadBoundCandidateAsync(ReadBoundCandidateRequest request, CancellationToken cancellationToken = default);
    }
}
