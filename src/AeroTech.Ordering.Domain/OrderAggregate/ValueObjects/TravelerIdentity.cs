namespace AeroTech.Ordering.Domain.OrderAggregate.ValueObjects
{
    public sealed record TravelerIdentity(string GivenName, string Surname, DateOnly DateOfBirth);
}
