using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateValidity(
        ValidityFact Offer,
        ValidityFact Price,
        ValidityFact Ticketing,
        ObservedTimeFact? ObservedTicketingDeadline);
}
