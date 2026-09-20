using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Persistence;
using Microsoft.Data.SqlClient;
using AeroTech.Ordering.Application.OrderAggregate.Commands.RebuildOrderProjection;
using AeroTech.Ordering.Domain.Tests._Shared;
using AeroTech.Ordering.Persistence.Tests._Shared;
using AeroTech.Ordering.Query._Shared.DbContexts;
using AeroTech.Ordering.Query.OrderAggregate.Models;
using AeroTech.Ordering.Query.OrderAggregate.Projection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class ComponentTotalPersistenceTests
    {
        private readonly OrderingDatabaseFixture _fixture;

        public ComponentTotalPersistenceTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Component_totals_round_trip_through_sql_and_reconcile_with_the_committed_lines()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var orderId = (await harness.SendAsync(S1Commands.Backoffice(await PublishAsync(harness)))).OrderId;

            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, orderId);
            var document = await ProjectionAsync(harness, orderId);

            Assert.Equal(4, order.ComponentTotals.Count);
            Assert.Equal(4, document.ComponentTotals.Count);

            foreach (var total in order.ComponentTotals)
            {
                var lines = order.PricingLines.Where(line => line.Component == total.Component && line.Effect == total.Effect).ToList();

                Assert.Equal(lines.Where(line => line.Direction == OrderPricingLineDirection.Debit).Sum(line => line.SaleValue.Amount), total.DebitAmount);
                Assert.Equal(lines.Where(line => line.Direction == OrderPricingLineDirection.Credit).Sum(line => line.SaleValue.Amount), total.CreditAmount);
                Assert.Equal(order.CurrencyId, total.CurrencyId);

                var projected = document.ComponentTotals.Single(row => row.Component == total.Component && row.Effect == total.Effect);

                Assert.Equal(total.DebitAmount, Amount(projected.DebitAmount));
                Assert.Equal(total.CreditAmount, Amount(projected.CreditAmount));
                Assert.Equal(total.CurrencyId, projected.CurrencyId);
            }

            Assert.Equal(order.CurrencyId, document.CustomerTotal.CurrencyId);
            Assert.Equal(
                order.CustomerTotal.Amount,
                order.ComponentTotals.Where(total => total.Effect == PricingEffect.CustomerBalance).Sum(total => total.Net));
        }

        [Fact]
        public async Task A_component_total_is_identified_by_order_component_and_effect_on_sql_server()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var orderId = (await harness.SendAsync(S1Commands.Backoffice(await PublishAsync(harness)))).OrderId;

            var keyColumns = await harness.InScopeAsync(async services =>
            {
                var context = services.GetRequiredService<OrderingDbContext>();
                await using var command = context.Database.GetDbConnection().CreateCommand();
                command.CommandText =
                    "SELECT STRING_AGG(c.[name], ',') WITHIN GROUP (ORDER BY ic.[key_ordinal])"
                    + " FROM sys.indexes i"
                    + " JOIN sys.index_columns ic ON ic.[object_id] = i.[object_id] AND ic.[index_id] = i.[index_id]"
                    + " JOIN sys.columns c ON c.[object_id] = ic.[object_id] AND c.[column_id] = ic.[column_id]"
                    + " WHERE i.[object_id] = OBJECT_ID(N'[Order].[OrderComponentTotals]') AND i.[is_primary_key] = 1";

                if (command.Connection!.State != System.Data.ConnectionState.Open)
                    await command.Connection.OpenAsync();

                return (string)(await command.ExecuteScalarAsync())!;
            });

            Assert.Equal("OrderId,Component,Effect", keyColumns);

            var duplicate = await harness.InScopeAsync(async services =>
            {
                try
                {
                    await services.GetRequiredService<OrderingDbContext>().Database.ExecuteSqlRawAsync($@"
INSERT INTO [Order].[OrderComponentTotals] ([OrderId],[Component],[Effect],[DebitAmount],[CreditAmount],[CurrencyId],[LastUpdateTime])
SELECT [OrderId], [Component], [Effect], [DebitAmount], [CreditAmount], [CurrencyId], SYSDATETIMEOFFSET()
FROM [Order].[OrderComponentTotals] WHERE [OrderId] = {orderId};");
                    return false;
                }
                catch (SqlException)
                {
                    return true;
                }
            });

            Assert.True(duplicate);
        }

        [Fact]
        public async Task A_component_total_cannot_hold_a_negative_magnitude_on_sql_server()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var orderId = (await harness.SendAsync(S1Commands.Backoffice(await PublishAsync(harness)))).OrderId;

            await Assert.ThrowsAsync<SqlException>(() => harness.InScopeAsync(async services =>
            {
                await services.GetRequiredService<OrderingDbContext>().Database.ExecuteSqlRawAsync($@"
UPDATE [Order].[OrderComponentTotals] SET [DebitAmount] = -1.00 WHERE [OrderId] = {orderId};");
                return 0;
            }));
        }

        [Fact]
        public async Task A_settlement_component_is_totalled_and_projected_but_stays_out_of_the_customer_total()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var orderId = (await harness.SendAsync(S1Commands.Backoffice(await PublishAsync(harness)))).OrderId;

            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, orderId);
            var document = await ProjectionAsync(harness, orderId);
            var commission = document.ComponentTotals.Single(total => total.Component == PricingComponentType.Commission);

            Assert.Equal(PricingEffect.SettlementOnly, commission.Effect);
            Assert.Equal(20.00m, Amount(commission.DebitAmount));
            Assert.Equal(0m, Amount(commission.CreditAmount));
            Assert.Equal(405.00m, order.CustomerTotal.Amount);
            Assert.Equal(405.00m, Amount(document.CustomerTotal.Amount));
        }

        [Fact]
        public async Task A_settlement_attribution_survives_sql_the_projection_and_a_rebuild()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var orderId = (await harness.SendAsync(S1Commands.Backoffice(await PublishAsync(harness)))).OrderId;

            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, orderId);
            var commission = order.PricingLines.Single(line => line.Component == PricingComponentType.Commission);

            Assert.Equal("agency:77", commission.SettlementAttribution!.PartyRef);
            Assert.Equal("COMMISSION", commission.SettlementAttribution.CategoryCode);

            var before = await ProjectionJsonAsync(harness, orderId);
            await harness.SendAsync(new RebuildOrderProjectionCommand(orderId, S1Commands.NewKey("settlement")));
            var after = await ProjectionJsonAsync(harness, orderId);

            Assert.Equal(before, after);
            Assert.Contains("\"partyRef\":\"agency:77\"", after);

            var document = await ProjectionAsync(harness, orderId);
            var projectedLine = document.Pricing.Single(line => line.Component == PricingComponentType.Commission);

            Assert.Equal("agency:77", projectedLine.SettlementAttribution!.PartyRef);
            Assert.All(
                document.Pricing.Where(line => line.Effect != PricingEffect.SettlementOnly),
                line => Assert.Null(line.SettlementAttribution));
        }

        private static async Task<string> PublishAsync(S1Harness harness)
        {
            var offerId = $"SETTLE-{Guid.NewGuid():N}";

            await harness.Catalog.PublishAsync(
                new CandidateBuilder(harness.Clock.GetDateTime(), S1Harness.Scope())
                    .Traveller("PAX-A")
                    .Segment("SEG-1")
                    .AirService("S-A", "PAX-A", "SEG-1")
                    .Package("ITEM-A", "S-A")
                    .Line("FARE", "ITEM-A", PricingComponentType.Fare, 400m, "S-A")
                    .Line("BAG", "ITEM-A", PricingComponentType.Fee, 50m, "S-A")
                    .Line("PROMO", "ITEM-A", PricingComponentType.Discount, 45m, "S-A")
                    .Line("COMM", "ITEM-A", PricingComponentType.Commission, 20m, "S-A",
                        effect: PricingEffect.SettlementOnly,
                        settlementAttribution: new Domain._Shared.ValueObjects.SettlementAttribution("agency:77", "COMMISSION"))
                    .Offer(offerId)
                    .Build());

            return offerId;
        }

        private static decimal Amount(string value) => decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

        private static async Task<OrderProjectionDocument> ProjectionAsync(S1Harness harness, long orderId)
            => OrderProjectionJson.Read(await ProjectionJsonAsync(harness, orderId));

        private static Task<string> ProjectionJsonAsync(S1Harness harness, long orderId)
            => harness.InScopeAsync(services => services.GetRequiredService<OrderQueryDbContext>()
                .Set<OrderDetailsReadModel>().AsNoTracking()
                .Where(row => row.OrderId == orderId)
                .Select(row => row.DetailsJson)
                .SingleAsync());
    }
}
