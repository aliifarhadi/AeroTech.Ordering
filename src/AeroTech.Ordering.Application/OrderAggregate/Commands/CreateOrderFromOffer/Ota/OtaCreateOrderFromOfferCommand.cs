using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer;
using MediatR;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer.Ota
{
    public sealed record OtaCreateOrderFromOfferCommand(
        string OfferId,
        IReadOnlyList<CreateOrderTravellerInput> Travellers,
        IReadOnlyList<CreateOrderContactInput> Contacts,
        string? ClientReference,
        string IdempotencyKey) : IRequest<CreateOrderFromOfferResult>;
}
