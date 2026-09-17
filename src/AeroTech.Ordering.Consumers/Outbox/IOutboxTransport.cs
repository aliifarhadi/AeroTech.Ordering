namespace AeroTech.Ordering.Consumers.Outbox
{
    public interface IOutboxTransport
    {
        Task PublishAsync(object payload, Type payloadType, Guid messageId, CancellationToken cancellationToken = default);
    }
}
