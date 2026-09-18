using AeroTech.Framework.Core.Domain.Events;
using AeroTech.Ordering.Domain._Shared.Contracts;

namespace AeroTech.Ordering.Domain.OrderAggregate.DomainEvents
{
    public sealed record OrderCreatedDomainEvent(
        string EventId,
        string AggregateId,
        DateTimeOffset TimeOfOccurrence,
        long OrderId,
        long ChangeId,
        int CommercialVersion,
        string AcceptedSourceDigest,
        long PriceChangeSetId,
        int FinancialSequence,
        long EventOrdinal)
        : DomainEvent(EventId, AggregateId, TimeOfOccurrence), IStreamFact
    {
        public const string OrderStream = "Order";

        public string StreamKind => OrderStream;

        public long StreamId => OrderId;
    }
}
