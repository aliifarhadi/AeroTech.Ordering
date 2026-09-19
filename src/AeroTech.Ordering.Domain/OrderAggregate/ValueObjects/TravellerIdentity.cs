namespace AeroTech.Ordering.Domain.OrderAggregate.ValueObjects
{
    public sealed record TravellerIdentity(string GivenName, string Surname, DateOnly DateOfBirth);
}
