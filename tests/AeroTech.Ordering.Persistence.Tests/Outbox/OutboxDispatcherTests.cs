using System.Text.Json;
using AeroTech.Messages;
using AeroTech.Ordering.Consumers.Inbox;
using AeroTech.Ordering.Consumers.Outbox;
using AeroTech.Ordering.Domain.Tests._Shared;
using AeroTech.Ordering.Persistence.Inbox;
using AeroTech.Ordering.Persistence.Outbox;
using AeroTech.Ordering.Persistence.Tests._Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.Outbox
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class OutboxDispatcherTests
    {
        private const string Receiver = "/receiver";
        private const int LeaseSeconds = 60;
        private const long HomeAirlineId = 1;

        private static long _eventSequence = DateTime.UtcNow.Ticks;

        private readonly OrderingDatabaseFixture _fixture;
        private readonly TestClock _clock = new();

        public OutboxDispatcherTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Outbox_fact_is_committed_or_rolled_back_with_the_local_transaction()
        {
            var rolledBack = NewEventId();
            var committed = NewEventId();

            await using (var context = _fixture.NewCommandContext())
            {
                await using var transaction = await context.Database.BeginTransactionAsync();
                await NewWriter(context).WriteAsync(new OutboxProbeIntegrationEvent { Subject = "rollback" }, NewDomainEvent(rolledBack));
                await context.SaveChangesAsync();
                await transaction.RollbackAsync();
            }

            await using (var context = _fixture.NewCommandContext())
            {
                await NewWriter(context).WriteAsync(new OutboxProbeIntegrationEvent { Subject = "commit" }, NewDomainEvent(committed));
                await context.SaveChangesAsync();
            }

            await using var read = _fixture.NewCommandContext();
            var stored = await read.OutboxMessages.AsNoTracking().SingleAsync(message => message.EventId == committed);

            Assert.False(await read.OutboxMessages.AnyAsync(message => message.EventId == rolledBack));
            Assert.Contains($"\"EventId\":\"{committed}\"", stored.Payload);
            Assert.Contains("\"SourceSystem\":\"Ordering\"", stored.Payload);
            Assert.Null(stored.ProcessedOn);
        }

        [Fact]
        public async Task Same_event_identity_cannot_be_written_twice()
        {
            var eventId = NewEventId();

            await using var context = _fixture.NewCommandContext();
            var writer = NewWriter(context);
            await writer.WriteAsync(new OutboxProbeIntegrationEvent { Subject = "first" }, NewDomainEvent(eventId));
            await writer.WriteAsync(new OutboxProbeIntegrationEvent { Subject = "second" }, NewDomainEvent(eventId));

            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }

        [Fact]
        public async Task Lost_acknowledgement_republishes_the_same_identity_and_payload_and_the_receiver_commits_one_fact()
        {
            await ClearOutboxAsync();
            var eventId = await WriteEventAsync("ack-loss");
            var transport = new RecordingTransport();
            transport.LoseNextAcknowledgement();

            await using (var context = _fixture.NewCommandContext())
                Assert.Equal(0, await NewDispatcher(context, transport).DispatchPendingAsync("publisher-before-restart"));

            var afterLoss = await ReadAsync(eventId);
            Assert.Null(afterLoss.ProcessedOn);
            Assert.Equal(1, afterLoss.AttemptCount);
            Assert.Contains("acknowledgement was lost", afterLoss.LastError);

            await using (var context = _fixture.NewCommandContext())
                Assert.Equal(0, await NewDispatcher(context, transport).DispatchPendingAsync("publisher-after-restart"));

            _clock.Advance(TimeSpan.FromSeconds(LeaseSeconds + 1));

            await using (var context = _fixture.NewCommandContext())
                Assert.Equal(1, await NewDispatcher(context, transport).DispatchPendingAsync("publisher-after-restart"));

            var afterRepublish = await ReadAsync(eventId);
            Assert.NotNull(afterRepublish.ProcessedOn);
            Assert.Equal(2, afterRepublish.AttemptCount);
            Assert.Null(afterRepublish.LastError);
            Assert.Equal(afterLoss.Payload, afterRepublish.Payload);
            Assert.Equal(eventId, afterRepublish.EventId);

            Assert.Equal(2, transport.Published.Count);
            Assert.Single(transport.Published.Select(message => message.MessageId).Distinct());
            Assert.Single(transport.Published.Select(message => message.PayloadJson).Distinct());
            Assert.Equal(OutboxDispatcher.ToMessageId(eventId), transport.Published[0].MessageId);

            var receiverFacts = 0;
            foreach (var delivery in transport.Published)
                receiverFacts += await ReceiveAsync(delivery) ? 1 : 0;

            Assert.Equal(1, receiverFacts);
        }

        [Fact]
        public async Task Two_publishers_never_hold_the_same_row()
        {
            await ClearOutboxAsync();
            const int rows = 40;

            for (var index = 0; index < rows; index++)
                await WriteEventAsync($"parallel-{index}");

            var transport = new RecordingTransport();

            await Task.WhenAll(
                DispatchAllAsync("publisher-a", transport),
                DispatchAllAsync("publisher-b", transport));

            Assert.Equal(rows, transport.Published.Count);
            Assert.Equal(rows, transport.Published.Select(message => message.MessageId).Distinct().Count());

            await using var read = _fixture.NewCommandContext();
            Assert.Equal(0, await read.OutboxMessages.CountAsync(message => message.ProcessedOn == null));
        }

        [Fact]
        public async Task Expired_lease_holder_cannot_complete_a_row_claimed_by_another_publisher()
        {
            await ClearOutboxAsync();
            var eventId = await WriteEventAsync("fencing");

            await using var firstContext = _fixture.NewCommandContext();
            await using var secondContext = _fixture.NewCommandContext();
            var first = new OutboxLeaseStore(firstContext);
            var second = new OutboxLeaseStore(secondContext);
            var lease = TimeSpan.FromSeconds(LeaseSeconds);

            var firstLease = Assert.Single(await first.ClaimAsync("publisher-a", 10, _clock.GetDateTime(), lease));
            Assert.Empty(await second.ClaimAsync("publisher-b", 10, _clock.GetDateTime(), lease));

            _clock.Advance(lease + TimeSpan.FromSeconds(1));
            var secondLease = Assert.Single(await second.ClaimAsync("publisher-b", 10, _clock.GetDateTime(), lease));

            Assert.True(secondLease.LeaseVersion > firstLease.LeaseVersion);
            Assert.False(await first.CompleteAsync(firstLease, _clock.GetDateTime()));
            Assert.False(await first.RecordFailureAsync(firstLease, "stale"));
            Assert.Null((await ReadAsync(eventId)).ProcessedOn);

            Assert.True(await second.CompleteAsync(secondLease, _clock.GetDateTime()));
            Assert.NotNull((await ReadAsync(eventId)).ProcessedOn);
        }

        [Fact]
        public async Task Unusable_row_is_recorded_as_failed_and_does_not_block_usable_rows()
        {
            await ClearOutboxAsync();
            const string unusableEventId = "not-a-stable-identity";

            await using (var context = _fixture.NewCommandContext())
            {
                await NewWriter(context).WriteAsync(new OutboxProbeIntegrationEvent { Subject = "unusable" }, NewDomainEvent(unusableEventId));
                await context.SaveChangesAsync();
            }

            var usableEventId = await WriteEventAsync("usable");
            var transport = new RecordingTransport();

            await using (var context = _fixture.NewCommandContext())
                Assert.Equal(1, await NewDispatcher(context, transport).DispatchPendingAsync("publisher"));

            var unusable = await ReadAsync(unusableEventId);
            Assert.Null(unusable.ProcessedOn);
            Assert.Equal(1, unusable.AttemptCount);
            Assert.Contains(unusableEventId, unusable.LastError);
            Assert.NotNull((await ReadAsync(usableEventId)).ProcessedOn);
            Assert.Single(transport.Published);
        }

        private async Task DispatchAllAsync(string leaseOwner, RecordingTransport transport)
        {
            while (true)
            {
                await using var context = _fixture.NewCommandContext();

                if (await NewDispatcher(context, transport, batchSize: 7).DispatchPendingAsync(leaseOwner) == 0)
                    return;
            }
        }

        private OutboxDispatcher NewDispatcher(OrderingDbContext context, IOutboxTransport transport, int batchSize = 50)
            => new(
                new OutboxLeaseStore(context),
                transport,
                _clock,
                Options.Create(new OutboxPublisherOptions { BatchSize = batchSize, LeaseSeconds = LeaseSeconds }),
                NullLogger<OutboxDispatcher>.Instance);

        private OutboxWriter NewWriter(OrderingDbContext context)
            => new(
                context,
                _clock,
                new OrderingDatabaseFixture.NullIdentityService(),
                Options.Create(new IntegrationEventOptions()));

        private async Task<string> WriteEventAsync(string subject)
        {
            var eventId = NewEventId();

            await using var context = _fixture.NewCommandContext();
            await NewWriter(context).WriteAsync(new OutboxProbeIntegrationEvent { Subject = subject }, NewDomainEvent(eventId));
            await context.SaveChangesAsync();

            return eventId;
        }

        private async Task<bool> ReceiveAsync(PublishedMessage delivery)
        {
            var received = (BaseIntegrationEvent)JsonSerializer.Deserialize(delivery.PayloadJson, delivery.PayloadType)!;
            var identity = new InboxIdentity(HomeAirlineId, received.SourceSystem, received.EventId, Receiver);
            var payloadHash = InboxEnvelope.HashPayload(received);

            await using var context = _fixture.NewCommandContext();
            var inbox = new InboxStore(context, _clock);

            if (await inbox.FindPayloadHashAsync(identity) is { } processedHash)
            {
                Assert.Equal(processedHash, payloadHash);
                return false;
            }

            inbox.EnlistProcessed(identity, delivery.PayloadType.FullName!, payloadHash);
            await inbox.PersistProcessedAsync();
            return true;
        }

        private async Task<OutboxMessage> ReadAsync(string eventId)
        {
            await using var context = _fixture.NewCommandContext();
            return await context.OutboxMessages.AsNoTracking().SingleAsync(message => message.EventId == eventId);
        }

        private async Task ClearOutboxAsync()
        {
            await using var context = _fixture.NewCommandContext();
            await context.OutboxMessages.ExecuteDeleteAsync();
        }

        private OutboxProbeDomainEvent NewDomainEvent(string eventId)
            => new(eventId, "aggregate-1", _clock.GetDateTime());

        private static string NewEventId() => Interlocked.Increment(ref _eventSequence).ToString();
    }
}
