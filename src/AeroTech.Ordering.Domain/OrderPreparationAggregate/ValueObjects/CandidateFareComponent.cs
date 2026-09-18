namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateFareComponent(
        string SourceFareRef,
        string? FareBasis,
        string? FareFamily,
        string? FareType,
        string? CabinRef,
        string? RbdRef,
        string? BookingClass,
        int? TicketingRestrictionMinutes,
        string? FareOwnerRef,
        string? TariffRef,
        string? RuleRef,
        string? RoutingRef,
        IReadOnlyList<string> CoveredServiceRefs,
        IReadOnlyList<string> CoveredSegmentRefs);
}
