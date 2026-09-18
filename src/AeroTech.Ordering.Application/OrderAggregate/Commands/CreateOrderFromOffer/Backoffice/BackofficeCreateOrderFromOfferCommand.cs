using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer;
using MediatR;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer.Backoffice
{
    public sealed record BackofficeCreateOrderFromOfferCommand(
        long CustomerId,
        long AirlineOfficeId,
        string OfferId,
        IReadOnlyList<CreateOrderTravellerInput> Travellers,
        IReadOnlyList<CreateOrderContactInput> Contacts,
        string? ClientReference,
        string IdempotencyKey) : IRequest<CreateOrderFromOfferResult>;
}
