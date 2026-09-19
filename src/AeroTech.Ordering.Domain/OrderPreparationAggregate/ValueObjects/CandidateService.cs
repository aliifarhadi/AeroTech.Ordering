using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateService(
        string ServiceKey,
        string TravellerRef,
        string SegmentKey,
        int? CabinClassId,
        long? RbdId,
        string? BookingClass,
        BaggageAllowance? CheckedBaggage,
        BaggageAllowance? CabinBaggage,
        SoldTermFlags SoldTerms);
}
