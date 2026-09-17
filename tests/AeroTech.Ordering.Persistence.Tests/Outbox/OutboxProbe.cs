using System.Collections.Concurrent;
using System.Text.Json;
using AeroTech.Framework.Core.Domain.Events;
using AeroTech.Messages;
using AeroTech.Ordering.Consumers.Outbox;

namespace AeroTech.Ordering.Persistence.Tests.Outbox
{
    public sealed record OutboxProbeDomainEvent(string EventId, string AggregateId, DateTimeOffset TimeOfOccurrence)
        : DomainEvent(EventId, AggregateId, TimeOfOccurrence);

    public sealed record OutboxProbeIntegrationEvent : BaseIntegrationEvent
    {
        public string Subject { get; set; } = default!;
    }

    public sealed record PublishedMessage(Guid MessageId, Type PayloadType, string PayloadJson);

    public sealed class RecordingTransport : IOutboxTransport
    {
        private readonly ConcurrentQueue<PublishedMessage> _published = new();
        private int _acknowledgementsToLose;

        public IReadOnlyList<PublishedMessage> Published => _published.ToList();

        public void LoseNextAcknowledgement() => Interlocked.Exchange(ref _acknowledgementsToLose, 1);

        public Task PublishAsync(object payload, Type payloadType, Guid messageId, CancellationToken cancellationToken = default)
        {
            _published.Enqueue(new PublishedMessage(messageId, payloadType, JsonSerializer.Serialize(payload, payloadType)));

            if (Interlocked.Exchange(ref _acknowledgementsToLose, 0) == 1)
                throw new TimeoutException("Broker accepted the message but the acknowledgement was lost.");

            return Task.CompletedTask;
        }
    }
}
