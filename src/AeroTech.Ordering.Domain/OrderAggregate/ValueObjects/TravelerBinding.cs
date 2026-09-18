namespace AeroTech.Ordering.Domain.OrderAggregate.ValueObjects
{
    public sealed record TravelerBinding(
        string SourceTravellerRef,
        string ClientTravelerRef,
        string GivenName,
        string Surname,
        string PassengerTypeCode,
        DateOnly DateOfBirth,
        string? GuardianClientTravelerRef);
}
