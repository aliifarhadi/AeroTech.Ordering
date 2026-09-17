using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Ordering.Consumers.Inbox;
using AeroTech.Ordering.Domain._Shared.Contracts;
using AeroTech.Ordering.Domain.Tests._Shared;
using AeroTech.Ordering.Persistence.Inbox;
using AeroTech.Ordering.Persistence.Outbox;
using AeroTech.Ordering.Persistence.Tests._Shared;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.Inbox
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class InboxConsumeFilterTests
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(20);

        private readonly OrderingDatabaseFixture _fixture;

        public InboxConsumeFilterTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Business_crash_commits_neither_marker_nor_work_and_redelivery_commits_both_once()
        {
            await using var provider = BuildProvider();
            var harness = await StartAsync(provider);
            var state = provider.GetRequiredService<InboxProbeState>();
            var messageId = Guid.NewGuid();
            var message = InboxProbeMessage.New(NewEventId(), NewEventId(), InboxProbeMode.CrashBeforeCommitOnce);

            await harness.Bus.Publish(message, publish => publish.MessageId = messageId);

            Assert.True(await harness.Published.Any<Fault<InboxProbeMessage>>(
                fault => fault.Context.Message.FaultedMessageId == messageId));
            Assert.Equal(0, await CountMarkersAsync(message.EventId));
            Assert.Equal(0, await CountWorkAsync(message.WorkEventId));

            await harness.Bus.Publish(message, publish => publish.MessageId = messageId);
            await WaitUntilAsync(async () => await CountWorkAsync(message.WorkEventId) == 1);

            Assert.Equal(1, await CountMarkersAsync(message.EventId));

            await harness.Bus.Publish(message, publish => publish.MessageId = Guid.NewGuid());
            await PublishSentinelAndWaitAsync(harness);

            Assert.Equal(3, state.Invocations);
            Assert.Equal(1, await CountMarkersAsync(message.EventId));
            Assert.Equal(1, await CountWorkAsync(message.WorkEventId));
        }

        [Fact]
        public async Task Unrelated_sql_failure_is_not_classified_as_a_duplicate()
        {
            await using var provider = BuildProvider();
            var harness = await StartAsync(provider);
            var messageId = Guid.NewGuid();
            var message = InboxProbeMessage.New(NewEventId(), NewEventId(), InboxProbeMode.Commit);

            await using (var seed = _fixture.NewCommandContext())
            {
                seed.OutboxMessages.Add(NewWork(message.WorkEventId));
                await seed.SaveChangesAsync();
            }

            await harness.Bus.Publish(message, publish => publish.MessageId = messageId);

            Assert.True(await harness.Published.Any<Fault<InboxProbeMessage>>(
                fault => fault.Context.Message.FaultedMessageId == messageId));

            var fault = harness.Published.Select<Fault<InboxProbeMessage>>()
                .Single(item => item.Context.Message.FaultedMessageId == messageId);

            Assert.Contains(
                fault.Context.Message.Exceptions,
                exception => exception.ExceptionType == typeof(DbUpdateException).FullName);
            Assert.Equal(0, await CountMarkersAsync(message.EventId));
            Assert.Equal(1, await CountWorkAsync(message.WorkEventId));
        }

        [Fact]
        public async Task Concurrent_duplicate_marker_discards_the_losing_delivery_without_committing_its_work()
        {
            await using var provider = BuildProvider();
            var harness = await StartAsync(provider);
            var message = InboxProbeMessage.New(NewEventId(), NewEventId(), InboxProbeMode.ConcurrentDuplicateWins);

            await harness.Bus.Publish(message, publish => publish.MessageId = Guid.NewGuid());

            await PublishSentinelAndWaitAsync(harness);

            Assert.Equal(1, await CountMarkersAsync(message.EventId));
            Assert.Equal(0, await CountWorkAsync(message.WorkEventId));
        }

        [Fact]
        public async Task Classifier_accepts_only_the_inbox_key_violation()
        {
            var eventId = NewEventId();
            var workEventId = NewEventId();

            await using (var seed = _fixture.NewCommandContext())
            {
                seed.InboxMessages.Add(NewMarker(eventId));
                seed.OutboxMessages.Add(NewWork(workEventId));
                await seed.SaveChangesAsync();
            }

            await using var duplicateMarker = _fixture.NewCommandContext();
            duplicateMarker.InboxMessages.Add(NewMarker(eventId));
            var markerFailure = await Assert.ThrowsAsync<DbUpdateException>(() => duplicateMarker.SaveChangesAsync());

            await using var duplicateWork = _fixture.NewCommandContext();
            duplicateWork.OutboxMessages.Add(NewWork(workEventId));
            var workFailure = await Assert.ThrowsAsync<DbUpdateException>(() => duplicateWork.SaveChangesAsync());

            Assert.True(InboxDuplicateClassifier.IsDuplicateMarker(markerFailure));
            Assert.False(InboxDuplicateClassifier.IsDuplicateMarker(workFailure));
        }

        private ServiceProvider BuildProvider()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddSingleton<IClock>(new TestClock());
            services.AddSingleton<InboxProbeState>();
            services.AddSingleton<IHomeOperatorProvider, FixedHomeOperatorProvider>();
            services.AddSingleton<Func<OrderingDbContext>>(() => _fixture.NewCommandContext());
            services.AddScoped(_ => _fixture.NewCommandContext());
            services.AddScoped<InboxStore>();
            services.AddScoped(typeof(InboxConsumeFilter<>));

            services.AddMassTransitTestHarness(bus =>
            {
                bus.AddConsumer<InboxProbeConsumer>(consumer => consumer.ConcurrentMessageLimit = 1);

                bus.UsingInMemory((context, configurator) =>
                {
                    configurator.UseConsumeFilter(typeof(InboxConsumeFilter<>), context);
                    configurator.ConfigureEndpoints(context);
                });
            });

            return services.BuildServiceProvider(true);
        }

        private static async Task<ITestHarness> StartAsync(ServiceProvider provider)
        {
            var harness = provider.GetRequiredService<ITestHarness>();
            harness.TestTimeout = Timeout;
            await harness.Start();
            return harness;
        }

        private async Task PublishSentinelAndWaitAsync(ITestHarness harness)
        {
            var sentinel = InboxProbeMessage.New(NewEventId(), NewEventId(), InboxProbeMode.Commit);

            await harness.Bus.Publish(sentinel, publish => publish.MessageId = Guid.NewGuid());

            await WaitUntilAsync(async () => await CountWorkAsync(sentinel.WorkEventId) == 1);
        }

        private static async Task WaitUntilAsync(Func<Task<bool>> condition)
        {
            var deadline = DateTime.UtcNow.Add(Timeout);

            while (DateTime.UtcNow < deadline)
            {
                if (await condition())
                    return;

                await Task.Delay(100);
            }

            Assert.Fail("Condition was not met before the timeout.");
        }

        private async Task<int> CountMarkersAsync(string eventId)
        {
            await using var context = _fixture.NewCommandContext();
            return await context.InboxMessages.CountAsync(marker => marker.EventId == eventId);
        }

        private async Task<int> CountWorkAsync(string workEventId)
        {
            await using var context = _fixture.NewCommandContext();
            return await context.OutboxMessages.CountAsync(work => work.EventId == workEventId);
        }

        private static string NewEventId() => $"inbox-{Guid.NewGuid():N}";

        private static OutboxMessage NewWork(string eventId)
            => new()
            {
                EventId = eventId,
                MessageType = typeof(InboxProbeMessage).FullName!,
                Payload = "{}",
                OccurredOn = DateTimeOffset.UtcNow
            };

        private static InboxMessage NewMarker(string eventId)
            => new()
            {
                OwnerAirlineId = FixedHomeOperatorProvider.OwnerAirlineId,
                SourceSystem = "Classifier",
                EventId = eventId,
                Consumer = "/classifier",
                MessageType = "classifier",
                PayloadHash = new string('0', InboxMessageConfiguration.PayloadHashLength),
                ReceivedOn = DateTimeOffset.UtcNow
            };
    }
}
