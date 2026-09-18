using AeroTech.Ordering.Application._Shared.Authorization;
using AeroTech.Ordering.Domain.OrderAggregate.ValueObjects;
using MediatR;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer
{
    public sealed record CreateOrderFromOfferCommand(
        SalesScopeRequest Scope,
        string IdempotencyKey,
        long PreparationId,
        string AcceptedSnapshotDigest,
        DateTimeOffset AcceptedAt,
        IReadOnlyList<TravelerBinding> TravelerBindings,
        IReadOnlyList<ContactDetails> Contacts,
        string? ClientReference) : IRequest<CreatedOrderResult>;
}
