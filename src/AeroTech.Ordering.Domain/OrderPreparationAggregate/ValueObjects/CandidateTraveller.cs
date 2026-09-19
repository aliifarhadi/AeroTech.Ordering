using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateTraveller(string TravellerRef, PassengerTypeCode PassengerTypeCode);
}
