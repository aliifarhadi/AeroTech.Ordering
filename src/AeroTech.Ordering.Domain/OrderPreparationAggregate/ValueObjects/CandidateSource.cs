using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateSource(
        string Owner,
        string OfferId,
        string ProviderProfileId,
        string? OwnerBindingRef,
        string SourcePayloadHash);
}
