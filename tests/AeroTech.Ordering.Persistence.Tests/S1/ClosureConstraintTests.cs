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
        public async Task The_closure_check_constraints_exist_on_sql_server(string constraint, string table)
        {
            await using var harness = await S1Harness.StartAsync(_fixture);

            var found = await ScalarAsync(harness,
                "SELECT COUNT(1) FROM sys.check_constraints"
                + $" WHERE [name] = N'{constraint}' AND [parent_object_id] = OBJECT_ID(N'[Order].[{table}]')");

            Assert.Equal(1, found);
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
