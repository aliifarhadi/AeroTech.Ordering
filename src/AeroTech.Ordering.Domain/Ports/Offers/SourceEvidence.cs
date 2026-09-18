namespace AeroTech.Ordering.Domain.Ports.Offers
{
    public sealed record SourceEvidence(
        string EvidenceRef,
        string PayloadHash,
        string ContentType,
        string Payload);
}
