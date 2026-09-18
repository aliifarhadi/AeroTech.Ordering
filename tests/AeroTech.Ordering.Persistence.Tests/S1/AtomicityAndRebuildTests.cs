using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Ordering.Application.OrderAggregate.Commands.RebuildOrderProjection;
using AeroTech.Ordering.Domain.OrderAggregate;
using AeroTech.Ordering.Domain.OrderPreparationAggregate;
using AeroTech.Ordering.Domain.Tests._Shared;
using AeroTech.Ordering.Persistence.Tests._Shared;
using AeroTech.Ordering.Query._Shared.DbContexts;
using AeroTech.Ordering.Query.OrderAggregate.Models;
using AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrderById.Backoffice;
using AeroTech.Ordering.Synchronizer.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class AtomicityAndRebuildTests
    {
        private readonly OrderingDatabaseFixture _fixture;

        public AtomicityAndRebuildTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task SC_S1_006_projection_failure_before_commit_leaves_no_order_source_receipt_or_outbox()
        {
            var fault = new FailingProjection();
            await using var harness = await S1Harness.StartAsync(_fixture, services =>
                services.Replace(ServiceDescriptor.Singleton<IOrderProjectionFaultInjector>(fault)));

            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);
            var command = S1Commands.Backoffice(offerId);

            fault.Armed = true;
            var failure = await Assert.ThrowsAsync<InvalidOperationException>(() => harness.SendAsync(command));
            Assert.Equal(FailingProjection.Message, failure.Message);

            Assert.Equal(0, await CreateOrderFromOfferTests.CountAsync<Order>(harness, order => order.AcceptedSource.SourceOfferId == offerId));
            Assert.Equal(0, await CreateOrderFromOfferTests.CountAsync<OrderPreparation>(harness, preparation => preparation.SourceOfferId == offerId));
            Assert.Equal(0, await harness.InScopeAsync(services => services.GetRequiredService<OrderingDbContext>().OutboxMessages
                .CountAsync(message => message.StreamKind == "Order" && message.Payload.Contains(offerId))));

            fault.Armed = false;
            var created = await harness.SendAsync(command);

            Assert.Equal(1, await CreateOrderFromOfferTests.CountAsync<Order>(harness, order => order.AcceptedSource.SourceOfferId == offerId));
            Assert.Equal(1, (await harness.SendAsync(new BackofficeGetOrderByIdQuery(created.OrderId))).CommercialVersion);
        }

        [Fact]
        public async Task SC_S1_020_restart_owner_offline_projection_rebuild_reproduces_the_same_view()
        {
            long orderId;
            string original;

            await using (var first = await S1Harness.StartAsync(_fixture))
            {
                var offerId = await CreateOrderFromOfferTests.PublishAsync(first, CandidateBuilder.OneWayFare100Tax20);
                orderId = (await first.SendAsync(S1Commands.Backoffice(offerId))).OrderId;
                original = await ProjectionJsonAsync(first, orderId);
                await first.Catalog.SetAvailabilityAsync(false);
            }

            await using var restarted = await S1Harness.StartAsync(_fixture);

            try
            {
                var readsBefore = await restarted.Catalog.CountReadsAsync();

                var beforeDelete = await restarted.SendAsync(new BackofficeGetOrderByIdQuery(orderId));
                Assert.Equal(orderId, beforeDelete.OrderId);

                await restarted.InScopeAsync(services => services.GetRequiredService<OrderQueryDbContext>()
                    .Set<OrderDetailsReadModel>().Where(row => row.OrderId == orderId).ExecuteDeleteAsync());

                var key = S1Commands.NewKey("rebuild");
                var rebuilt = await restarted.SendAsync(new RebuildOrderProjectionCommand(orderId, key));
                var replayed = await restarted.SendAsync(new RebuildOrderProjectionCommand(orderId, key));

                Assert.Equal(original, await ProjectionJsonAsync(restarted, orderId));
                Assert.Equal(1, rebuilt.CommercialVersion);
                Assert.True(replayed.ReplayedFromReceipt);
                Assert.Equal(rebuilt.OperationId, replayed.OperationId);
                Assert.Equal(readsBefore, await restarted.Catalog.CountReadsAsync());

                var order = await CreateOrderFromOfferTests.LoadOrderAsync(restarted, orderId);
                Assert.Equal(1, order.CommercialVersion);
                Assert.Equal(1, order.FinancialSequence);
                Assert.Single(order.Changes);
                Assert.Equal(1, await restarted.InScopeAsync(services => services.GetRequiredService<OrderingDbContext>().OutboxMessages
                    .CountAsync(message => message.StreamId == orderId)));
            }
            finally
            {
                await restarted.Catalog.SetAvailabilityAsync(true);
            }
        }

        [Fact]
        public async Task Rebuild_never_overwrites_a_newer_projection_revision()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);
            var orderId = (await harness.SendAsync(S1Commands.Backoffice(offerId))).OrderId;

            await harness.InScopeAsync(services => services.GetRequiredService<OrderQueryDbContext>()
                .Set<OrderDetailsReadModel>().Where(row => row.OrderId == orderId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(row => row.OrderRevision, 7)));

            var conflict = await Assert.ThrowsAsync<BusinessException>(() =>
                harness.SendAsync(new RebuildOrderProjectionCommand(orderId, S1Commands.NewKey("stale"))));

            Assert.Equal(20286, conflict.Code);
        }

        private static Task<string> ProjectionJsonAsync(S1Harness harness, long orderId)
            => harness.InScopeAsync(services => services.GetRequiredService<OrderQueryDbContext>()
                .Set<OrderDetailsReadModel>().AsNoTracking().Where(row => row.OrderId == orderId).Select(row => row.DetailsJson).SingleAsync());

        private sealed class FailingProjection : IOrderProjectionFaultInjector
        {
            public const string Message = "Injected projection failure before commit.";

            public bool Armed { get; set; }

            public Task BeforeProjectionWriteAsync(long orderId, CancellationToken cancellationToken)
                => Armed ? throw new InvalidOperationException(Message) : Task.CompletedTask;
        }
    }
}
