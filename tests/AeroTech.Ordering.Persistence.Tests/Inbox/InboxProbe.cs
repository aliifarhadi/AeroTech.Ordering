using AeroTech.Messages;
using AeroTech.Ordering.Consumers.Inbox;
using AeroTech.Ordering.Domain._Shared.Contracts;
using AeroTech.Ordering.Persistence.Inbox;
using AeroTech.Ordering.Persistence.Outbox;
using MassTransit;

namespace AeroTech.Ordering.Persistence.Tests.Inbox
{
    public enum InboxProbeMode
    {
        Commit,
        CrashBeforeCommitOnce,
        ConcurrentDuplicateWins
    }

    public sealed record InboxProbeMessage : BaseIntegrationEvent
    {
        public const string Source = "InventoryProbe";

        public string WorkEventId { get; set; } = default!;

        public InboxProbeMode Mode { get; set; }

        public static InboxProbeMessage New(string eventId, string workEventId, InboxProbeMode mode, string sourceSystem = Source)
            => new()
            {
                EventId = eventId,
                SourceSystem = sourceSystem,
                AggregateId = "probe",
                TimeOfOccurrence = new DateTimeOffset(2026, 9, 8, 10, 0, 0, TimeSpan.Zero),
                WorkEventId = workEventId,
                Mode = mode
            };
    }

    public sealed record InboxProbeBareMessage(string WorkEventId);

    public sealed class FixedHomeOperatorProvider : IHomeOperatorProvider
    {
        public const long OwnerAirlineId = 1;

        public Task<long> GetOwnerAirlineIdAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(OwnerAirlineId);
    }

    public sealed class InboxProbeState
    {
        private int _invocations;
        private int _crashes;

        public int Invocations => _invocations;

        public void Invoked() => Interlocked.Increment(ref _invocations);

        public bool ShouldCrash() => Interlocked.Increment(ref _crashes) == 1;
    }

    public sealed class InboxProbeConsumer : IConsumer<InboxProbeMessage>
    {
        private readonly OrderingDbContext _dbContext;
        private readonly InboxProbeState _state;
        private readonly Func<OrderingDbContext> _independentContext;

        public InboxProbeConsumer(OrderingDbContext dbContext, InboxProbeState state, Func<OrderingDbContext> independentContext)
        {
            _dbContext = dbContext;
            _state = state;
            _independentContext = independentContext;
        }

        public async Task Consume(ConsumeContext<InboxProbeMessage> context)
        {
            _state.Invoked();

            _dbContext.OutboxMessages.Add(new OutboxMessage
            {
                EventId = context.Message.WorkEventId,
                MessageType = typeof(InboxProbeMessage).FullName!,
                Payload = "{}",
                OccurredOn = DateTimeOffset.UtcNow
            });

            if (context.Message.Mode == InboxProbeMode.CrashBeforeCommitOnce && _state.ShouldCrash())
                throw new InvalidOperationException("Business crash before commit.");

            if (context.Message.Mode == InboxProbeMode.ConcurrentDuplicateWins)
            {
                await using var other = _independentContext();
                other.InboxMessages.Add(new InboxMessage
                {
                    OwnerAirlineId = FixedHomeOperatorProvider.OwnerAirlineId,
                    SourceSystem = context.Message.SourceSystem,
                    EventId = context.Message.EventId,
                    Consumer = context.ReceiveContext.InputAddress.AbsolutePath,
                    MessageType = typeof(InboxProbeMessage).FullName!,
                    PayloadHash = InboxEnvelope.HashPayload(context.Message),
                    ReceivedOn = DateTimeOffset.UtcNow
                });
                await other.SaveChangesAsync(context.CancellationToken);
            }

            await _dbContext.SaveChangesAsync(context.CancellationToken);
        }
    }
}
