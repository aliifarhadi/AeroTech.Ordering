using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer;
using MediatR;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer.OtaPanel
{
    public sealed record OtaPanelCreateOrderFromOfferCommand(
        string OfferId,
        IReadOnlyList<CreateOrderTravellerInput> Travellers,
        IReadOnlyList<CreateOrderContactInput> Contacts,
        string? ClientReference,
        string IdempotencyKey) : IRequest<CreateOrderFromOfferResult>;
}
