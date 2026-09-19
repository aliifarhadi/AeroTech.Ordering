using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateItem(
        string ItemKey,
        OrderItemKind ItemKind,
        IReadOnlyList<string> ServiceKeys,
        Money AcceptedTotal);
}
