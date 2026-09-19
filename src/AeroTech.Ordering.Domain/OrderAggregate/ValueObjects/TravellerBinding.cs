using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.OrderAggregate.ValueObjects
{
    public sealed record TravellerBinding(
        string SourceTravellerRef,
        string ClientTravellerRef,
        string GivenName,
        string Surname,
        PassengerTypeCode PassengerTypeCode,
        DateOnly DateOfBirth,
        string? GuardianClientTravellerRef);
}
