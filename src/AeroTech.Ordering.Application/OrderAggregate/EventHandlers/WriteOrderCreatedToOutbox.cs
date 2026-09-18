using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ordering.IntegrationEvents.V2;
using AeroTech.Ordering.Application._Shared.Events;
using AeroTech.Ordering.Domain.OrderAggregate.DomainEvents;
using MediatR;

namespace AeroTech.Ordering.Application.OrderAggregate.EventHandlers
{
    public sealed class WriteOrderCreatedToOutbox : INotificationHandler<DomainEventNotification<OrderCreatedDomainEvent>>
    {
        private readonly IOutboxWriter _outboxWriter;

        public WriteOrderCreatedToOutbox(IOutboxWriter outboxWriter) => _outboxWriter = outboxWriter;

        public Task Handle(DomainEventNotification<OrderCreatedDomainEvent> notification, CancellationToken cancellationToken)
        {
            var domainEvent = notification.DomainEvent;

            return _outboxWriter.WriteAsync(
                new OrderCreated(
                    domainEvent.OrderId,
                    domainEvent.ChangeId,
                    domainEvent.CommercialVersion,
                    domainEvent.AcceptedSourceDigest,
                    domainEvent.PriceChangeSetId,
                    domainEvent.FinancialSequence,
                    domainEvent.EventOrdinal),
                domainEvent,
                cancellationToken);
        }
    }
}
