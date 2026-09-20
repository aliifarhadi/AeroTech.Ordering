using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Domain.Tests._Shared;
using AeroTech.Ordering.Persistence.Tests._Shared;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class AcceptedScopeConstraintTests
    {
        private const int ForeignKeyViolation = 547;
        private const int UniqueViolation = 2627;
        private const int UniqueIndexViolation = 2601;

        private readonly OrderingDatabaseFixture _fixture;

        public AcceptedScopeConstraintTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task An_air_service_cannot_be_pointed_at_another_orders_traveller()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var first = await AcceptedOrderIdAsync(harness);
            var second = await AcceptedOrderIdAsync(harness);

            var foreignTravellerId = await ScalarAsync(harness, $"SELECT TOP 1 [Id] FROM [Order].[OrderTravellers] WHERE [OrderId] = {second}");

            var violation = await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(harness,
                $"UPDATE [Order].[OrderServices] SET [TravellerId] = {foreignTravellerId} WHERE [OrderId] = {first};"));

            Assert.Equal(ForeignKeyViolation, violation.Number);
            Assert.Contains("OrderTravellers", violation.Message);
        }

        [Fact]
        public async Task An_air_service_cannot_be_pointed_at_another_orders_segment()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var first = await AcceptedOrderIdAsync(harness);
            var second = await AcceptedOrderIdAsync(harness);

            var foreignSegmentId = await ScalarAsync(harness, $"SELECT TOP 1 [Id] FROM [Order].[OrderSegments] WHERE [OrderId] = {second}");

            var violation = await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(harness,
                $"UPDATE [Order].[OrderServices] SET [SegmentId] = {foreignSegmentId} WHERE [OrderId] = {first};"));

            Assert.Equal(ForeignKeyViolation, violation.Number);
            Assert.Contains("OrderSegments", violation.Message);
        }

        [Fact]
        public async Task A_same_order_traveller_and_segment_reference_remains_valid()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, (now, scope) => new CandidateBuilder(now, scope)
                .Traveller("PAX-A").Traveller("PAX-B")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .Package("PACKAGE", "S-A")
                .Line("fare", "PACKAGE", PricingComponentType.Fare, 100m, "S-A"));

            var created = await harness.SendAsync(S1Commands.Backoffice(offerId, travellers: S1Commands.Travellers("PAX-A", "PAX-B")));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);
            var service = Assert.IsType<OrderAirTransportService>(Assert.Single(order.Services));
            var sameOrderOtherTraveller = order.Travellers.Single(traveller => traveller.Id != service.TravellerId).Id;

            await ExecuteAsync(harness,
                $"UPDATE [Order].[OrderServices] SET [TravellerId] = {sameOrderOtherTraveller} WHERE [Id] = {service.Id};");

            var reloaded = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);

            Assert.Equal(sameOrderOtherTraveller, Assert.IsType<OrderAirTransportService>(Assert.Single(reloaded.Services)).TravellerId);
        }

        [Fact]
        public async Task A_commercial_change_cannot_carry_a_second_price_change_set()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var orderId = await AcceptedOrderIdAsync(harness);
            var changeId = await ScalarAsync(harness, $"SELECT TOP 1 [ChangeId] FROM [Order].[PriceChangeSets] WHERE [OrderId] = {orderId}");

            var violation = await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(harness, $@"
INSERT INTO [Order].[PriceChangeSets] ([Id],[OrderId],[ChangeId],[FinancialSequence],[Reason],[CommittedAt],[LastUpdateTime])
VALUES (-{orderId}, {orderId}, {changeId}, 2, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());"));

            Assert.Contains(violation.Number, new[] { UniqueViolation, UniqueIndexViolation });
            Assert.Contains("PriceChangeSets", violation.Message);
        }

        [Fact]
        public async Task A_different_commercial_change_may_carry_its_own_price_change_set()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var orderId = await AcceptedOrderIdAsync(harness);
            var actorContextType = await ScalarAsync(harness, $"SELECT TOP 1 [ActorContextType] FROM [Order].[OrderChanges] WHERE [OrderId] = {orderId}");

            await ExecuteAsync(harness, $@"
INSERT INTO [Order].[OrderChanges] ([Id],[OrderId],[Type],[CommercialVersion],[ActorContextType],[CommittedAt],[LastUpdateTime])
VALUES (-{orderId}, {orderId}, 1, 2, {actorContextType}, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
INSERT INTO [Order].[PriceChangeSets] ([Id],[OrderId],[ChangeId],[FinancialSequence],[Reason],[CommittedAt],[LastUpdateTime])
VALUES (-{orderId}, {orderId}, -{orderId}, 2, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());");

            Assert.Equal(2, await ScalarAsync(harness, $"SELECT COUNT(1) FROM [Order].[PriceChangeSets] WHERE [OrderId] = {orderId}"));
        }

        private static async Task<long> AcceptedOrderIdAsync(S1Harness harness)
        {
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);
            return (await harness.SendAsync(S1Commands.Backoffice(offerId))).OrderId;
        }

        private static Task ExecuteAsync(S1Harness harness, string sql)
            => harness.InScopeAsync(async services =>
            {
                await services.GetRequiredService<OrderingDbContext>().Database.ExecuteSqlRawAsync(sql);
                return 0;
            });

        private static Task<long> ScalarAsync(S1Harness harness, string sql)
            => harness.InScopeAsync(async services =>
            {
                var context = services.GetRequiredService<OrderingDbContext>();
                await using var command = context.Database.GetDbConnection().CreateCommand();
                command.CommandText = sql;

                if (command.Connection!.State != System.Data.ConnectionState.Open)
                    await command.Connection.OpenAsync();

                return Convert.ToInt64(await command.ExecuteScalarAsync());
            });
    }
}
