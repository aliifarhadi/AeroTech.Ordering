using AeroTech.Ordering.Persistence;
using AeroTech.Ordering.Persistence.Tests._Shared;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class ClosureConstraintTests
    {
        private readonly OrderingDatabaseFixture _fixture;

        public ClosureConstraintTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData("CK_PricingLines_SettlementAttribution", "PricingLines")]
        [InlineData("CK_PricingLines_CommissionNotCustomer", "PricingLines")]
        [InlineData("CK_PricingLines_TaxNotSettlement", "PricingLines")]
        [InlineData("CK_PricingLines_OtherInformational", "PricingLines")]
        [InlineData("CK_PricingLines_OriginalMagnitude", "PricingLines")]
        [InlineData("CK_PricingLines_SaleMagnitude", "PricingLines")]
        [InlineData("CK_PricingLines_Direction", "PricingLines")]
        [InlineData("CK_FundingObligations_Version", "FundingObligations")]
        [InlineData("CK_FundingObligations_Amount", "FundingObligations")]
        [InlineData("CK_FundingObligations_ExactlyOneScope", "FundingObligations")]
        [InlineData("CK_OrderComponentTotals_Magnitudes", "OrderComponentTotals")]
        [InlineData("CK_Orders_Root", "Orders")]
        public async Task The_closure_check_constraints_exist_on_sql_server(string constraint, string table)
        {
            await using var harness = await S1Harness.StartAsync(_fixture);

            var found = await ScalarAsync(harness,
                "SELECT COUNT(1) FROM sys.check_constraints"
                + $" WHERE [name] = N'{constraint}' AND [parent_object_id] = OBJECT_ID(N'[Order].[{table}]')");

            Assert.Equal(1, found);
        }

        [Fact]
        public async Task A_funding_obligation_needs_exactly_one_scope_on_sql_server()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var orderId = await AcceptedOrderIdAsync(harness);

            var itemId = await ScalarAsync(harness, $"SELECT TOP 1 [Id] FROM [Order].[OrderItems] WHERE [OrderId] = {orderId}");
            var serviceId = await ScalarAsync(harness, $"SELECT TOP 1 [Id] FROM [Order].[OrderServices] WHERE [OrderId] = {orderId}");

            await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(harness,
                $"UPDATE [Order].[FundingObligations] SET [OrderServiceId] = {serviceId} WHERE [OrderId] = {orderId};"));

            await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(harness,
                $"UPDATE [Order].[FundingObligations] SET [OrderItemId] = NULL WHERE [OrderId] = {orderId} AND [OrderItemId] = {itemId};"));
        }

        [Fact]
        public async Task A_root_order_identifier_is_positive_but_is_not_pinned_to_the_order_identifier()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var orderId = await AcceptedOrderIdAsync(harness);

            Assert.Equal(orderId, await ScalarAsync(harness, $"SELECT [RootOrderId] FROM [Order].[Orders] WHERE [Id] = {orderId}"));

            await ExecuteAsync(harness, $"UPDATE [Order].[Orders] SET [RootOrderId] = {orderId} + 1 WHERE [Id] = {orderId};");
            Assert.Equal(orderId + 1, await ScalarAsync(harness, $"SELECT [RootOrderId] FROM [Order].[Orders] WHERE [Id] = {orderId}"));

            await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(harness,
                $"UPDATE [Order].[Orders] SET [RootOrderId] = 0 WHERE [Id] = {orderId};"));

            await ExecuteAsync(harness, $"UPDATE [Order].[Orders] SET [RootOrderId] = {orderId} WHERE [Id] = {orderId};");
        }

        [Fact]
        public async Task A_settlement_attribution_cannot_be_half_populated_on_sql_server()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var orderId = await AcceptedOrderIdAsync(harness);

            await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(harness,
                $"UPDATE [Order].[PricingLines] SET [SettlementPartyRef] = N'agency:77' WHERE [OrderId] = {orderId};"));

            await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(harness,
                $"UPDATE [Order].[PricingLines] SET [SettlementCategoryCode] = N'COMMISSION' WHERE [OrderId] = {orderId};"));
        }

        private static async Task<long> AcceptedOrderIdAsync(S1Harness harness)
        {
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, Domain.Tests._Shared.CandidateBuilder.OneWayFare100Tax20);
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
                await using var connection = context.Database.GetDbConnection().CreateCommand();
                connection.CommandText = sql;

                if (connection.Connection!.State != System.Data.ConnectionState.Open)
                    await connection.Connection.OpenAsync();

                return Convert.ToInt64(await connection.ExecuteScalarAsync());
            });
    }
}
