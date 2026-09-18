namespace AeroTech.Ordering.RestApi.V1.OrderAggregate.Responses
{
    public sealed record ProtectedTravelerResponse(
        long TravelerId,
        string SourceTravellerRef,
        string ClientTravelerRef,
        string PassengerTypeCode,
        string GivenName,
        string Surname,
        DateOnly DateOfBirth,
        long? InfantParentTravelerId);
}
