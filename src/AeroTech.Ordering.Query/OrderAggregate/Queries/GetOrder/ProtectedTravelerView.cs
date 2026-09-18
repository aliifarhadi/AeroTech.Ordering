namespace AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrder
{
    public sealed record ProtectedTravelerView(
        long TravelerId,
        string SourceTravellerRef,
        string ClientTravelerRef,
        string PassengerTypeCode,
        string GivenName,
        string Surname,
        DateOnly DateOfBirth,
        long? InfantParentTravelerId);
}
