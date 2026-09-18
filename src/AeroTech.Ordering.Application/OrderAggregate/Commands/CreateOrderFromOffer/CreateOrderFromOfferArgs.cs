using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer
{
    public sealed record CreateOrderFromOfferArgs(
        AuthorizedSalesScope Scope,
        string IdempotencyKey,
        string OfferId,
        IReadOnlyList<CreateOrderTravellerInput> Travellers,
        IReadOnlyList<CreateOrderContactInput> Contacts,
        string? ClientReference);

    public sealed record CreateOrderTravellerInput(
        string OfferTravellerRef,
        string TravellerRef,
        string FirstName,
        string SurName,
        PassengerTypeCode PassengerType,
        DateOnly DateOfBirth,
        string? GuardianTravellerRef);

    public sealed record CreateOrderContactInput(
        ContactRole Role,
        string? Email,
        string? Phone);
}
