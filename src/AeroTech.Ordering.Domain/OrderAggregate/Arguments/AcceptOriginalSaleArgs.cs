using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderAggregate.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate;

namespace AeroTech.Ordering.Domain.OrderAggregate.Arguments
{
    public sealed record AcceptOriginalSaleArgs(
        long OrderId,
        string OrderReference,
        OrderPreparation Preparation,
        AuthorizedSalesScope AcceptingScope,
        IReadOnlyList<TravellerBinding> TravellerBindings,
        IReadOnlyList<ContactDetails> Contacts,
        DateTimeOffset ClientAcceptedAt,
        DateTimeOffset AcceptedAt,
        string? ClientReference);
}
