using MassTransit;

namespace AeroTech.Ordering.Consumers.Outbox
{
    public sealed class MassTransitOutboxTransport : IOutboxTransport
    {
        private readonly IBus _bus;

        public MassTransitOutboxTransport(IBus bus)
        {
            _bus = bus;
        }

        public Task PublishAsync(object payload, Type payloadType, Guid messageId, CancellationToken cancellationToken = default)
            => _bus.Publish(
                payload,
                payloadType,
                Pipe.Execute<PublishContext>(publish => publish.MessageId = messageId),
                cancellationToken);
    }
}
