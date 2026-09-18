using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

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
        IReadOnlyList<string> CoveredServiceRefs);
}
