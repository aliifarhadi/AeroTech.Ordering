using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Policies
{
    public static class DeadlinePolicy
    {
        public static ValidityFact? EarliestKnown(IEnumerable<ValidityFact> facts)
            => facts
                .Where(fact => fact.State == ValidityState.Known)
                .OrderBy(fact => fact.Value!.Value)
                .FirstOrDefault();

        public static IReadOnlyList<ValidityFact> ExpiredAt(IEnumerable<ValidityFact> facts, DateTimeOffset now)
            => facts.Where(fact => fact.IsExpiredAt(now)).ToList();

        public static IReadOnlyList<ValidityFact> Unestablished(IEnumerable<ValidityFact> facts)
            => facts.Where(fact => fact.State == ValidityState.NotSupplied).ToList();
    }
}
