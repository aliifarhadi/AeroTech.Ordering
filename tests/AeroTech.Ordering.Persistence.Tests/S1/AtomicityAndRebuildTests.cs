using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Application._Shared.Authorization;
using AeroTech.Ordering.Application.OrderAggregate.Commands.RebuildOrderProjection;
using AeroTech.Ordering.Domain.CommandReceiptAggregate;
using AeroTech.Ordering.Domain.OrderAggregate;
using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Domain.Tests._Shared;
using AeroTech.Ordering.Persistence.Tests._Shared;
using AeroTech.Ordering.Query._Shared.DbContexts;
using AeroTech.Ordering.Query.OrderAggregate.Models;
using AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrder;
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
        private static readonly AdministrativeScopeRequest Administrator = new(OrderingApiSurface.Internal);

        private readonly OrderingDatabaseFixture _fixture;

        public AtomicityAndRebuildTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task SC_S1_006_projection_failure_before_commit_leaves_no_order_price_outbox_receipt_or_consumption()
        {
            var fault = new FailingProjection();
            await using var harness = await S1Harness.StartAsync(_fixture, services =>
                services.Replace(ServiceDescriptor.Singleton<IOrderProjectionFaultInjector>(fault)));

            var preparation = await harness.SendAsync(S1Commands.Prepare(await PrepareCreateGetTests.PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20)));
            var create = S1Commands.Create(preparation, harness.Clock.GetDateTime());

            fault.Armed = true;
            var failure = await Assert.ThrowsAsync<InvalidOperationException>(() => harness.SendAsync(create));
            Assert.Equal(FailingProjection.Message, failure.Message);

            Assert.Equal(0, await PrepareCreateGetTests.CountAsync<Order>(harness, order => order.SourcePreparationId == preparation.PreparationId));
            Assert.Equal(0, await PrepareCreateGetTests.CountAsync<PriceChangeSet>(harness, set => set.SourceDecisionRef.Contains(preparation.AcceptedSnapshotDigest)));
            Assert.Equal(0, await PrepareCreateGetTests.CountAsync<CommandReceipt>(harness, receipt => receipt.PreparationId == preparation.PreparationId && receipt.OrderId != null));
            Assert.Null((await PrepareCreateGetTests.LoadPreparationAsync(harness, preparation.PreparationId)).ConsumedByOrderId);
            Assert.Equal(0, await harness.InScopeAsync(services => services.GetRequiredService<OrderingDbContext>().OutboxMessages
                .CountAsync(message => message.StreamKind == "Order" && message.Payload.Contains(preparation.AcceptedSnapshotDigest))));

            fault.Armed = false;
            var created = await harness.SendAsync(create);

            Assert.False(created.ReplayedFromReceipt);
            Assert.Equal(1, await PrepareCreateGetTests.CountAsync<Order>(harness, order => order.SourcePreparationId == preparation.PreparationId));
            Assert.Equal(1, (await harness.SendAsync(new GetOrderQuery(created.OrderId, OrderingApiSurface.Backoffice, null))).OrderRevision);
        }

        [Fact]
        public async Task SC_S1_020_restart_owner_offline_projection_rebuild_reproduces_the_same_view()
        {
            string offerId;
            long orderId;
            string original;

            await using (var first = await S1Harness.StartAsync(_fixture))
            {
                offerId = await PrepareCreateGetTests.PublishAsync(first, CandidateBuilder.OneWayFare100Tax20);
                orderId = (await PrepareCreateGetTests.CreateAsync(first, offerId)).OrderId;
                original = await ProjectionJsonAsync(first, orderId);
                await first.Catalog.SetAvailabilityAsync(false);
            }

            await using var restarted = await S1Harness.StartAsync(_fixture);

            try
            {
                var readsBefore = await restarted.Catalog.CountReadsAsync();

                var beforeDelete = await restarted.SendAsync(new GetOrderQuery(orderId, OrderingApiSurface.Backoffice, null));
                Assert.Equal(1, beforeDelete.OrderRevision);

                await restarted.InScopeAsync(services => services.GetRequiredService<OrderQueryDbContext>()
                    .Set<OrderDetailsReadModel>().Where(row => row.OrderId == orderId).ExecuteDeleteAsync());

                var key = S1Commands.NewKey("rebuild");
                var rebuilt = await restarted.SendAsync(new RebuildOrderProjectionCommand(Administrator, key, orderId));
                var replayed = await restarted.SendAsync(new RebuildOrderProjectionCommand(Administrator, key, orderId));

                Assert.Equal(original, await ProjectionJsonAsync(restarted, orderId));
                Assert.Equal(1, rebuilt.CommercialVersion);
                Assert.True(replayed.ReplayedFromReceipt);
                Assert.Equal(rebuilt.OperationId, replayed.OperationId);
                Assert.Equal(readsBefore, await restarted.Catalog.CountReadsAsync());

                var order = await PrepareCreateGetTests.LoadOrderAsync(restarted, orderId);
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
            var orderId = (await PrepareCreateGetTests.CreateAsync(harness, await PrepareCreateGetTests.PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20))).OrderId;

            await harness.InScopeAsync(services => services.GetRequiredService<OrderQueryDbContext>()
                .Set<OrderDetailsReadModel>().Where(row => row.OrderId == orderId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(row => row.OrderRevision, 7)));

            var conflict = await Assert.ThrowsAsync<AeroTech.Framework.Core.Domain.Exceptions.BusinessException>(() =>
                harness.SendAsync(new RebuildOrderProjectionCommand(Administrator, S1Commands.NewKey("stale"), orderId)));

            Assert.Equal(20286, conflict.Code);
            Assert.Equal(7, (await harness.SendAsync(new GetOrderQuery(orderId, OrderingApiSurface.Backoffice, null))).OrderRevision);
        }

        [Fact]
        public async Task Get_order_reports_lag_instead_of_stale_success_for_a_future_revision()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var orderId = (await PrepareCreateGetTests.CreateAsync(harness, await PrepareCreateGetTests.PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20))).OrderId;

            var lag = await Assert.ThrowsAsync<AeroTech.Framework.Core.Domain.Exceptions.BusinessException>(() =>
                harness.SendAsync(new GetOrderQuery(orderId, OrderingApiSurface.Backoffice, 2)));

            Assert.Equal(20284, lag.Code);
            Assert.Equal(503, lag.HttpStatus);
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
