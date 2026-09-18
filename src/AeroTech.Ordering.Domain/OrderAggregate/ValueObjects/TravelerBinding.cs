using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.OrderAggregate.ValueObjects
{
    public sealed record TravelerBinding(
        string SourceTravellerRef,
        string ClientTravelerRef,
        string GivenName,
        string Surname,
        PassengerTypeCode PassengerTypeCode,
        DateOnly DateOfBirth,
        string? GuardianClientTravelerRef);
}
