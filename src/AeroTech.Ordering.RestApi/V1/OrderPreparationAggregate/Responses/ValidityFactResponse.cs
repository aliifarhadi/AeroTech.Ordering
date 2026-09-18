using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.RestApi.V1.OrderPreparationAggregate.Responses
{
    public sealed record ValidityFactResponse(
        ValidityState State,
        DateTimeOffset? Value,
        string Owner,
        string? SourceRef,
        string? Reason)
    {
        public static ValidityFactResponse From(ValidityFact fact)
            => new(fact.State, fact.Value, fact.Owner, fact.SourceRef, fact.Reason);
    }
}
