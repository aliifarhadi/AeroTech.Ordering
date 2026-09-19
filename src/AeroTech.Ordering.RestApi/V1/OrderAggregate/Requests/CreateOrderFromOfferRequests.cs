using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.RestApi.V1.OrderAggregate.Requests
{
    public sealed record BackofficeCreateOrderFromOfferRequest(
        long CustomerId,
        long AirlineOfficeId,
        string OfferId,
        IReadOnlyList<OrderTravellerRequest> Travellers,
        IReadOnlyList<OrderContactRequest> Contacts,
        string? ClientReference);

    public sealed record ServiceCreateOrderFromOfferRequest(
        long CustomerId,
        long? SellingOfficeId,
        SellingOfficeKind? SellingOfficeKind,
        string OfferId,
        IReadOnlyList<OrderTravellerRequest> Travellers,
        IReadOnlyList<OrderContactRequest> Contacts,
        string? ClientReference);

    public sealed record OtaCreateOrderFromOfferRequest(
        string OfferId,
        IReadOnlyList<OrderTravellerRequest> Travellers,
        IReadOnlyList<OrderContactRequest> Contacts,
        string? ClientReference);

    public sealed record OrderTravellerRequest(
        string OfferTravellerRef,
        string TravellerRef,
        string FirstName,
        string SurName,
        PassengerTypeCode PassengerType,
        DateOnly DateOfBirth,
        string? GuardianTravellerRef);

    public sealed record OrderContactRequest(
        ContactRole Role,
        string? Email,
        string? Phone);
}
