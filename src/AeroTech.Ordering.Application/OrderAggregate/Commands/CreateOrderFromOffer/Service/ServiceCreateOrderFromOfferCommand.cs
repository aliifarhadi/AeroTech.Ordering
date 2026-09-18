using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer;
using MediatR;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer.Service
{
    public sealed record ServiceCreateOrderFromOfferCommand(
        long CustomerId,
        long? AirlineOfficeId,
        string OfferId,
        IReadOnlyList<CreateOrderTravellerInput> Travellers,
        IReadOnlyList<CreateOrderContactInput> Contacts,
        string? ClientReference,
        string IdempotencyKey) : IRequest<CreateOrderFromOfferResult>;
}
