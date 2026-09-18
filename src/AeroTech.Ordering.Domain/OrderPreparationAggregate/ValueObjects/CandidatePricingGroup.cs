using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidatePricingGroup(
        IReadOnlyList<string> TravelerRefs,
        string PassengerTypeCode,
        int Quantity);
}
