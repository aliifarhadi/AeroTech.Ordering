using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.OrderAggregate.ValueObjects
{
    public sealed record AcceptedSource(
        long PreparationId,
        string SourceOwner,
        string SourceOfferId,
        string ProviderProfileId,
        string ContractVersion,
        string AcceptanceProfile,
        AcceptanceAssurance AcceptanceAssurance,
        string? OwnerBindingRef,
        string SnapshotDigest,
        string SourcePayloadHash,
        DateTimeOffset PricedAt,
        DateTimeOffset CapturedAt,
        DateTimeOffset ClientAcceptedAt,
        DateTimeOffset AcceptedAt)
    {
        public bool IsSandboxScoped => AcceptanceAssurance == AcceptanceAssurance.LocalCandidateOnly;
    }
}
