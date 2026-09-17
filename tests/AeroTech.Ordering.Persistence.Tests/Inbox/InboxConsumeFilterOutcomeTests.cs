using AeroTech.Ordering.Consumers.Inbox;
using AeroTech.Ordering.Domain.Tests._Shared;
using AeroTech.Ordering.Persistence.Inbox;
using AeroTech.Ordering.Persistence.Outbox;
using AeroTech.Ordering.Persistence.Tests._Shared;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.Inbox
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class InboxConsumeFilterOutcomeTests
    {
        private const string Consumer = "/outcome-probe";

        private readonly OrderingDatabaseFixture _fixture;

        public InboxConsumeFilterOutcomeTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Message_without_transport_identity_is_rejected_before_the_consumer_runs()
        {
            var message = NewMessage();

            await AssertRejectedAsync(message, messageId: null, message.EventId);
        }

        [Theory]
        [InlineData("", "event-1")]
        [InlineData("InventoryProbe", "")]
        [InlineData(null, null)]
        public async Task Message_without_envelope_source_identity_is_rejected_before_the_consumer_runs(string? sourceSystem, string? eventId)
        {
            var message = NewMessage() with { SourceSystem = sourceSystem!, EventId = eventId! };

            await AssertRejectedAsync(message, Guid.NewGuid(), eventId);
        }

        [Fact]
        public async Task Message_that_is_not_an_envelope_type_is_rejected_before_the_consumer_runs()
        {
            await using var context = _fixture.NewCommandContext();
            var consumerRan = false;

            await Assert.ThrowsAsync<InboxMessageIdentityMissingException>(() => NewFilter<InboxProbeBareMessage>(context).Send(
                ConsumeContextStub.Create(new InboxProbeBareMessage("bare"), Guid.NewGuid(), Consumer),
                Pipe.Execute<ConsumeContext<InboxProbeBareMessage>>(_ => consumerRan = true)));

            Assert.False(consumerRan);
        }

        [Fact]
        public async Task Marker_and_work_commit_in_one_save_under_the_pack_identity()
        {
            await using var context = _fixture.NewCommandContext();
            var message = NewMessage();
            var savesWithBoth = 0;

            context.SavingChanges += (_, _) =>
            {
                var marker = context.ChangeTracker.Entries<InboxMessage>().Any(entry => entry.State == EntityState.Added);
                var work = context.ChangeTracker.Entries<OutboxMessage>().Any(entry => entry.State == EntityState.Added);

                if (marker && work)
                    savesWithBoth++;
            };

            await NewFilter<InboxProbeMessage>(context).Send(
                ConsumeContextStub.Create(message, Guid.NewGuid(), Consumer),
                WorkPipe(context, message.WorkEventId));

            await using var read = _fixture.NewCommandContext();
            var marker = await read.InboxMessages.AsNoTracking().SingleAsync(item => item.EventId == message.EventId);

            Assert.Equal(1, savesWithBoth);
            Assert.Equal(FixedHomeOperatorProvider.OwnerAirlineId, marker.OwnerAirlineId);
            Assert.Equal(InboxProbeMessage.Source, marker.SourceSystem);
            Assert.Equal(Consumer, marker.Consumer);
            Assert.Equal(InboxEnvelope.HashPayload(message), marker.PayloadHash);
            Assert.Equal(1, await CountWorkAsync(message.WorkEventId));
        }

        [Fact]
        public async Task Republished_event_with_a_new_transport_id_does_not_reach_the_consumer_again()
        {
            var message = NewMessage();
            var consumerRuns = 0;

            for (var delivery = 0; delivery < 2; delivery++)
            {
                await using var context = _fixture.NewCommandContext();

                await NewFilter<InboxProbeMessage>(context).Send(
                    ConsumeContextStub.Create(message, Guid.NewGuid(), Consumer),
                    Pipe.Execute<ConsumeContext<InboxProbeMessage>>(_ => consumerRuns++));
            }

            Assert.Equal(1, consumerRuns);
            Assert.Equal(1, await CountMarkersAsync(message.EventId));
        }

        [Fact]
        public async Task Same_event_id_from_another_source_system_is_a_different_fact()
        {
            var eventId = NewEventId();
            var consumerRuns = 0;

            foreach (var source in new[] { "InventoryProbe", "PricingProbe" })
            {
                await using var context = _fixture.NewCommandContext();

                await NewFilter<InboxProbeMessage>(context).Send(
                    ConsumeContextStub.Create(InboxProbeMessage.New(eventId, NewEventId(), InboxProbeMode.Commit, source), Guid.NewGuid(), Consumer),
                    Pipe.Execute<ConsumeContext<InboxProbeMessage>>(_ => consumerRuns++));
            }

            Assert.Equal(2, consumerRuns);
            Assert.Equal(2, await CountMarkersAsync(eventId));
        }

        [Fact]
        public async Task Same_identity_with_a_different_payload_is_a_conflict_and_is_not_processed()
        {
            var original = NewMessage();
            var tampered = original with { WorkEventId = NewEventId() };
            var consumerRuns = 0;

            await using (var context = _fixture.NewCommandContext())
                await NewFilter<InboxProbeMessage>(context).Send(
                    ConsumeContextStub.Create(original, Guid.NewGuid(), Consumer),
                    Pipe.Execute<ConsumeContext<InboxProbeMessage>>(_ => consumerRuns++));

            await using var second = _fixture.NewCommandContext();

            var conflict = await Assert.ThrowsAsync<InboxPayloadConflictException>(() => NewFilter<InboxProbeMessage>(second).Send(
                ConsumeContextStub.Create(tampered, Guid.NewGuid(), Consumer),
                Pipe.Execute<ConsumeContext<InboxProbeMessage>>(_ => consumerRuns++)));

            Assert.Equal(original.EventId, conflict.Identity.EventId);
            Assert.Equal(1, consumerRuns);
            Assert.Equal(1, await CountMarkersAsync(original.EventId));
        }

        [Fact]
        public async Task Unrelated_sql_failure_is_rethrown_and_commits_nothing()
        {
            var message = NewMessage();

            await using (var seed = _fixture.NewCommandContext())
            {
                seed.OutboxMessages.Add(NewWork(message.WorkEventId));
                await seed.SaveChangesAsync();
            }

            await using var context = _fixture.NewCommandContext();

            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => NewFilter<InboxProbeMessage>(context).Send(
                ConsumeContextStub.Create(message, Guid.NewGuid(), Consumer),
                WorkPipe(context, message.WorkEventId)));

            Assert.False(InboxDuplicateClassifier.IsDuplicateMarker(exception));
            Assert.Equal(0, await CountMarkersAsync(message.EventId));
            Assert.Equal(1, await CountWorkAsync(message.WorkEventId));
        }

        [Fact]
        public async Task Concurrent_duplicate_marker_is_discarded_without_committing_the_losing_work()
        {
            var message = NewMessage();

            await using var context = _fixture.NewCommandContext();

            await NewFilter<InboxProbeMessage>(context).Send(
                ConsumeContextStub.Create(message, Guid.NewGuid(), Consumer),
                Pipe.ExecuteAsync<ConsumeContext<InboxProbeMessage>>(async _ =>
                {
                    await using (var winner = _fixture.NewCommandContext())
                    {
                        winner.InboxMessages.Add(NewMarker(message, InboxEnvelope.HashPayload(message)));
                        await winner.SaveChangesAsync();
                    }

                    context.OutboxMessages.Add(NewWork(message.WorkEventId));
                    await context.SaveChangesAsync();
                }));

            Assert.Equal(1, await CountMarkersAsync(message.EventId));
            Assert.Equal(0, await CountWorkAsync(message.WorkEventId));
        }

        [Fact]
        public async Task Concurrent_marker_with_a_different_payload_is_a_conflict()
        {
            var message = NewMessage();

            await using var context = _fixture.NewCommandContext();

            await Assert.ThrowsAsync<InboxPayloadConflictException>(() => NewFilter<InboxProbeMessage>(context).Send(
                ConsumeContextStub.Create(message, Guid.NewGuid(), Consumer),
                Pipe.ExecuteAsync<ConsumeContext<InboxProbeMessage>>(async _ =>
                {
                    await using (var winner = _fixture.NewCommandContext())
                    {
                        winner.InboxMessages.Add(NewMarker(message, new string('0', InboxMessageConfiguration.PayloadHashLength)));
                        await winner.SaveChangesAsync();
                    }

                    context.OutboxMessages.Add(NewWork(message.WorkEventId));
                    await context.SaveChangesAsync();
                })));

            Assert.Equal(0, await CountWorkAsync(message.WorkEventId));
        }

        private async Task AssertRejectedAsync(InboxProbeMessage message, Guid? messageId, string? eventId)
        {
            await using var context = _fixture.NewCommandContext();
            var consumerRan = false;
            var markersBefore = await CountConsumerMarkersAsync();

            var exception = await Assert.ThrowsAsync<InboxMessageIdentityMissingException>(() => NewFilter<InboxProbeMessage>(context).Send(
                ConsumeContextStub.Create(message, messageId, Consumer),
                Pipe.Execute<ConsumeContext<InboxProbeMessage>>(_ => consumerRan = true)));

            Assert.Equal(typeof(InboxProbeMessage).FullName, exception.MessageType);
            Assert.Equal(Consumer, exception.Consumer);
            Assert.False(consumerRan);
            Assert.Equal(markersBefore, await CountConsumerMarkersAsync());

            if (!string.IsNullOrEmpty(eventId))
                Assert.Equal(0, await CountMarkersAsync(eventId));
        }

        private static IPipe<ConsumeContext<InboxProbeMessage>> WorkPipe(OrderingDbContext context, string workEventId)
            => Pipe.ExecuteAsync<ConsumeContext<InboxProbeMessage>>(async _ =>
            {
                context.OutboxMessages.Add(NewWork(workEventId));
                await context.SaveChangesAsync();
            });

        private static InboxConsumeFilter<T> NewFilter<T>(OrderingDbContext context) where T : class
            => new(
                new InboxStore(context, new TestClock()),
                new FixedHomeOperatorProvider(),
                NullLogger<InboxConsumeFilter<T>>.Instance);

        private async Task<int> CountMarkersAsync(string eventId)
        {
            await using var context = _fixture.NewCommandContext();
            return await context.InboxMessages.CountAsync(marker => marker.EventId == eventId);
        }

        private async Task<int> CountConsumerMarkersAsync()
        {
            await using var context = _fixture.NewCommandContext();
            return await context.InboxMessages.CountAsync(marker => marker.Consumer == Consumer);
        }

        private async Task<int> CountWorkAsync(string workEventId)
        {
            await using var context = _fixture.NewCommandContext();
            return await context.OutboxMessages.CountAsync(work => work.EventId == workEventId);
        }

        private static string NewEventId() => $"inbox-{Guid.NewGuid():N}";

        private static InboxProbeMessage NewMessage()
            => InboxProbeMessage.New(NewEventId(), NewEventId(), InboxProbeMode.Commit);

        private static InboxMessage NewMarker(InboxProbeMessage message, string payloadHash)
            => new()
            {
                OwnerAirlineId = FixedHomeOperatorProvider.OwnerAirlineId,
                SourceSystem = message.SourceSystem,
                EventId = message.EventId,
                Consumer = Consumer,
                MessageType = typeof(InboxProbeMessage).FullName!,
                PayloadHash = payloadHash,
                ReceivedOn = DateTimeOffset.UtcNow
            };

        private static OutboxMessage NewWork(string eventId)
            => new()
            {
                EventId = eventId,
                MessageType = typeof(InboxProbeMessage).FullName!,
                Payload = "{}",
                OccurredOn = DateTimeOffset.UtcNow
            };
    }
}
