using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateItem(
        string ItemRef,
        OrderItemKind ItemKind,
        string? SourceOfferItemRef,
        IReadOnlyList<string> ServiceRefs,
        Money AcceptedTotal,
        ProductSnapshot Product);
}
