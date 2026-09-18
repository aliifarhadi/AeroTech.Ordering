using AeroTech.Ordering.Application._Shared.Authorization;
using MediatR;

namespace AeroTech.Ordering.Application.OrderPreparationAggregate.Commands.PrepareOrderFromOffer
{
    public sealed record PrepareOrderFromOfferCommand(
        SalesScopeRequest Scope,
        string IdempotencyKey,
        string OfferId,
        string? ClientReference) : IRequest<PreparationResult>;
}
