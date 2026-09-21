using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Ordering.Persistence.OrderAggregate;
using Microsoft.Data.SqlClient;
using AeroTech.Ordering.Domain.OrderPreparationAggregate;
using AeroTech.Ordering.Domain.Tests._Shared;
using AeroTech.Ordering.Persistence.Tests._Shared;
using AeroTech.Ordering.Query._Shared.DbContexts;
using AeroTech.Ordering.Query.OrderAggregate.Models;
using AeroTech.Ordering.Query.OrderAggregate.Projection;
using AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrderById.Backoffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class ReliabilityClosureTests
    {
        private const int UnsupportedCapability = 20273;

        private readonly OrderingDatabaseFixture _fixture;

        public ReliabilityClosureTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task R3_one_preparation_can_be_consumed_by_at_most_one_order()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var first = await AcceptedOrderIdAsync(harness);
            var second = await AcceptedOrderIdAsync(harness);
            var consumedPreparationId = (await CreateOrderFromOfferTests.LoadOrderAsync(harness, first)).SourcePreparationId;

            var violation = await Assert.ThrowsAsync<SqlException>(() => harness.InScopeAsync(async services =>
            {
                await services.GetRequiredService<OrderingDbContext>().Database.ExecuteSqlRawAsync(
                    $"UPDATE [Order].[Orders] SET [SourcePreparationId] = {consumedPreparationId} WHERE [Id] = {second};");
                return 0;
            }));

            Assert.Contains(violation.Number, new[] { 2601, 2627 });
            Assert.Contains(OrderConfiguration.SourcePreparationIndex, violation.Message);
        }

        [Fact]
        public async Task R3_the_accepted_order_is_the_only_link_back_to_its_preparation()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var orderId = await AcceptedOrderIdAsync(harness);
            var preparation = await PreparationAsync(harness, orderId);

            Assert.Equal(1, await CreateOrderFromOfferTests.CountAsync<Domain.OrderAggregate.Order>(
                harness, order => order.SourcePreparationId == preparation.Id));
            Assert.Equal(harness.Clock.GetDateTime().Date, preparation.CapturedAt.Date);
        }

        [Fact]
        public async Task R6_a_projection_row_written_by_an_unknown_schema_version_is_refused_not_misread()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);
            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));

            Assert.Equal(OrderProjectionJson.SchemaVersion, await SchemaVersionAsync(harness, created.OrderId));

            await SetSchemaVersionAsync(harness, created.OrderId, OrderProjectionJson.SchemaVersion + 97);

            var failure = await Assert.ThrowsAsync<BusinessException>(
                () => harness.SendAsync(new BackofficeGetOrderByIdQuery(created.OrderId)));

            Assert.Equal(UnsupportedCapability, failure.Code);
            Assert.Contains("order projection schema", failure.Message);
        }

        [Fact]
        public async Task R6_a_projection_row_of_the_current_schema_version_is_read()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);
            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));

            var dto = await harness.SendAsync(new BackofficeGetOrderByIdQuery(created.OrderId));

            Assert.Equal(created.OrderId, dto.OrderId);
            Assert.Equal("120", dto.CustomerTotal.Amount);
        }

        [Fact]
        public async Task R12_the_three_source_deadlines_keep_their_own_instants_and_are_never_conflated()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var now = harness.Clock.GetDateTime();
            var offerExpiresAt = now.AddHours(3);
            var priceValidUntil = now.AddHours(2);
            var lastTicketingDate = now.AddDays(4);

            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, (clock, scope) =>
                CandidateBuilder.OneWayFare100Tax20(clock, scope).Validity(offerExpiresAt, priceValidUntil, lastTicketingDate));

            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);
            var preparation = await PreparationAsync(harness, created.OrderId);

            Assert.Equal(lastTicketingDate, order.LastTicketingDate);
            Assert.Equal(offerExpiresAt, preparation.Candidate.OfferExpiresAt);
            Assert.Equal(priceValidUntil, preparation.Candidate.PriceValidUntil);
            Assert.Equal(lastTicketingDate, preparation.Candidate.LastTicketingDate);
            Assert.Equal(3, new[] { offerExpiresAt, priceValidUntil, lastTicketingDate }.Distinct().Count());
            Assert.NotEqual(order.LastTicketingDate, preparation.Candidate.OfferExpiresAt);
            Assert.NotEqual(order.LastTicketingDate, preparation.Candidate.PriceValidUntil);
        }

        [Fact]
        public async Task R12_a_missing_source_deadline_stays_null_and_is_not_filled_from_another_one()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var now = harness.Clock.GetDateTime();

            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, (clock, scope) =>
                CandidateBuilder.OneWayFare100Tax20(clock, scope).Validity(now.AddHours(3), now.AddHours(2), null));

            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);
            var preparation = await PreparationAsync(harness, created.OrderId);

            Assert.Null(order.LastTicketingDate);
            Assert.Null(preparation.Candidate.LastTicketingDate);
            Assert.NotNull(preparation.Candidate.OfferExpiresAt);
            Assert.NotNull(preparation.Candidate.PriceValidUntil);
        }

        private static async Task<long> AcceptedOrderIdAsync(S1Harness harness)
        {
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);
            return (await harness.SendAsync(S1Commands.Backoffice(offerId))).OrderId;
        }

        private static async Task<OrderPreparation> PreparationAsync(S1Harness harness, long orderId)
        {
            var preparationId = (await CreateOrderFromOfferTests.LoadOrderAsync(harness, orderId)).SourcePreparationId;

            return await harness.InScopeAsync(services => services.GetRequiredService<OrderingDbContext>()
                .Set<OrderPreparation>().AsNoTracking().SingleAsync(preparation => preparation.Id == preparationId));
        }

        private static Task<int> SchemaVersionAsync(S1Harness harness, long orderId)
            => harness.InScopeAsync(services => services.GetRequiredService<OrderQueryDbContext>()
                .Set<OrderDetailsReadModel>().AsNoTracking()
                .Where(row => row.OrderId == orderId)
                .Select(row => row.ProjectionSchemaVersion)
                .SingleAsync());

        private static Task SetSchemaVersionAsync(S1Harness harness, long orderId, int version)
            => harness.InScopeAsync(async services =>
            {
                await services.GetRequiredService<OrderQueryDbContext>().Database.ExecuteSqlRawAsync(
                    $"UPDATE [ReadModel].[OrderDetails] SET [ProjectionSchemaVersion] = {version} WHERE [OrderId] = {orderId};");
                return 0;
            });
    }
}
