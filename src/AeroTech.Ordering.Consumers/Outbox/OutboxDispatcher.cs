using System.Text.Json;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Ordering.Persistence.Outbox;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AeroTech.Ordering.Consumers.Outbox
{
    public sealed class OutboxDispatcher
    {
        private readonly OutboxLeaseStore _leaseStore;
        private readonly IOutboxTransport _transport;
        private readonly IClock _clock;
        private readonly OutboxPublisherOptions _options;
        private readonly ILogger<OutboxDispatcher> _logger;

        public OutboxDispatcher(
            OutboxLeaseStore leaseStore,
            IOutboxTransport transport,
            IClock clock,
            IOptions<OutboxPublisherOptions> options,
            ILogger<OutboxDispatcher> logger)
        {
            _leaseStore = leaseStore;
            _transport = transport;
            _clock = clock;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<int> DispatchPendingAsync(string leaseOwner, CancellationToken cancellationToken = default)
        {
            var leases = await _leaseStore.ClaimAsync(
                leaseOwner,
                _options.BatchSize,
                _clock.GetDateTime(),
                TimeSpan.FromSeconds(Math.Max(1, _options.LeaseSeconds)),
                cancellationToken);

            var completed = 0;

            foreach (var lease in leases)
            {
                try
                {
                    var payloadType = Type.GetType(lease.MessageType)
                                      ?? throw new OutboxMessageUnusableException($"Message type '{lease.MessageType}' cannot be resolved.");

                    var payload = JsonSerializer.Deserialize(lease.Payload, payloadType)
                                  ?? throw new OutboxMessageUnusableException($"Payload of '{lease.MessageType}' deserialized to null.");

                    await _transport.PublishAsync(payload, payloadType, ToMessageId(lease.EventId), cancellationToken);

                    if (await _leaseStore.CompleteAsync(lease, _clock.GetDateTime(), cancellationToken))
                    {
                        completed++;
                        continue;
                    }

                    _logger.LogWarning(
                        "Outbox message {Id} (EventId {EventId}) was published but its lease was lost; it is republished with the same identity.",
                        lease.Id,
                        lease.EventId);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _logger.LogError(
                        exception,
                        "Failed to publish outbox message {Id} (EventId {EventId}, {MessageType}).",
                        lease.Id,
                        lease.EventId,
                        lease.MessageType);

                    await _leaseStore.RecordFailureAsync(lease, exception.Message, cancellationToken);
                }
            }

            return completed;
        }

        public static Guid ToMessageId(string eventId)
        {
            if (!long.TryParse(eventId, out var value))
                throw new OutboxMessageUnusableException($"EventId '{eventId}' cannot produce a stable MessageId.");

            Span<byte> bytes = stackalloc byte[16];
            BitConverter.TryWriteBytes(bytes, value);
            return new Guid(bytes);
        }
    }
}
