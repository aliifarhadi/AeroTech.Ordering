using System.Data;
using AeroTech.Ordering.Domain.Tests._Shared;
using AeroTech.Ordering.Persistence.Tests._Shared;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class CurrentOwnershipConstraintTests
    {
        private const int ForeignKeyViolation = 547;

        private static readonly (string Table, string[] Columns, string PrincipalTable)[] OwnerScopedRelations =
        [
            ("Orders", ["OwnerAirlineId", "SourcePreparationId"], "OrderPreparations"),
            ("CommandReceipts", ["OwnerAirlineId", "OrderId"], "Orders"),
            ("OrderItems", ["OrderId", "CreatedByChangeId"], "OrderChanges"),
            ("OrderServices", ["OrderId", "OrderItemId"], "OrderItems"),
            ("OrderServices", ["OrderId", "CreatedByChangeId"], "OrderChanges"),
            ("OrderServices", ["OrderId", "TravellerId"], "OrderTravellers"),
            ("OrderServices", ["OrderId", "SegmentId"], "OrderSegments"),
            ("OrderSegments", ["OrderId", "JourneyId"], "OrderJourneys"),
            ("OrderTravellers", ["OrderId", "InfantParentTravellerId"], "OrderTravellers"),
            ("OrderItemServiceLinks", ["OrderIdAtAssociation", "OrderItemId"], "OrderItems"),
            ("OrderItemServiceLinks", ["OrderIdAtAssociation", "OrderServiceId"], "OrderServices"),
            ("OrderItemServiceLinks", ["OrderIdAtAssociation", "LinkedByChangeId"], "OrderChanges"),
            ("PriceChangeSets", ["OrderId", "ChangeId"], "OrderChanges"),
            ("PricingLines", ["OrderId", "PriceChangeSetId"], "PriceChangeSets"),
            ("PricingLines", ["OrderId", "OrderItemId"], "OrderItems"),
            ("FareConstructions", ["OrderId", "CreatedByChangeId"], "OrderChanges"),
            ("FundingObligations", ["OrderId", "ChangeId"], "OrderChanges"),
            ("FundingObligations", ["OrderId", "PriceChangeSetId"], "PriceChangeSets"),
            ("FundingObligations", ["OrderId", "OrderItemId"], "OrderItems"),
            ("FundingObligations", ["OrderId", "OrderServiceId"], "OrderServices"),
            ("FundingObligations", ["OrderId", "PricingLineId"], "PricingLines")
        ];

        private readonly OrderingDatabaseFixture _fixture;

        public CurrentOwnershipConstraintTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Every_scoped_relation_in_the_matrix_is_a_live_composite_foreign_key()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var live = await ForeignKeyColumnsAsync(harness);

            var missing = OwnerScopedRelations
                .Where(relation => !live.Any(candidate =>
                    candidate.Table == relation.Table
                    && candidate.PrincipalTable == relation.PrincipalTable
                    && candidate.Columns.SequenceEqual(relation.Columns)))
                .Select(relation => $"{relation.Table}({string.Join(", ", relation.Columns)}) -> {relation.PrincipalTable}")
                .ToList();

            Assert.Empty(missing);
            Assert.Equal(21, OwnerScopedRelations.Length);
        }

        [Fact]
        public async Task No_order_scoped_table_keeps_a_single_column_foreign_key_to_another_order_scoped_table()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var live = await ForeignKeyColumnsAsync(harness);

            var scopedTables = OwnerScopedRelations.Select(relation => relation.Table)
                .Concat(OwnerScopedRelations.Select(relation => relation.PrincipalTable))
                .ToHashSet();

            var unscoped = live
                .Where(relation => relation.Columns.Length == 1)
                .Where(relation => scopedTables.Contains(relation.Table) && scopedTables.Contains(relation.PrincipalTable))
                .Where(relation => relation.Columns[0] is not ("OrderId" or "OwnerAirlineId" or "OrderIdAtAssociation"))
                .Select(relation => $"{relation.Table}({relation.Columns[0]}) -> {relation.PrincipalTable}")
                .ToList();

            Assert.Empty(unscoped);
        }

        [Fact]
        public async Task An_order_cannot_be_moved_to_an_owner_that_did_not_prepare_it()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var orderId = await AcceptedOrderIdAsync(harness);
            var preparationId = await ScalarAsync(harness, $"SELECT [SourcePreparationId] FROM [Order].[Orders] WHERE [Id] = {orderId}");

            var violation = await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(harness,
                $"UPDATE [Order].[OrderPreparations] SET [OwnerAirlineId] = [OwnerAirlineId] + 1 WHERE [Id] = {preparationId};"));

            Assert.Equal(ForeignKeyViolation, violation.Number);
            Assert.Contains("FK_Orders_OrderPreparations_OwnerAirlineId_SourcePreparationId", violation.Message);
        }

        [Fact]
        public async Task A_command_receipt_cannot_be_moved_to_another_owner_while_it_points_at_an_order()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var orderId = await AcceptedOrderIdAsync(harness);

            var violation = await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(harness,
                $"UPDATE [Operations].[CommandReceipts] SET [OwnerAirlineId] = [OwnerAirlineId] + 1 WHERE [OrderId] = {orderId};"));

            Assert.Equal(ForeignKeyViolation, violation.Number);
            Assert.Contains("FK_CommandReceipts_Orders_OwnerAirlineId_OrderId", violation.Message);
        }

        [Theory]
        [InlineData("OrderItems", "CreatedByChangeId", "OrderChanges", "FK_OrderItems_OrderChanges_OrderId_CreatedByChangeId")]
        [InlineData("OrderServices", "CreatedByChangeId", "OrderChanges", "FK_OrderServices_OrderChanges_OrderId_CreatedByChangeId")]
        [InlineData("OrderServices", "OrderItemId", "OrderItems", "FK_OrderServices_OrderItems_OrderId_OrderItemId")]
        [InlineData("PricingLines", "OrderItemId", "OrderItems", "FK_PricingLines_OrderItems_OrderId_OrderItemId")]
        public async Task A_child_row_cannot_be_pointed_at_a_parent_of_another_order(
            string table,
            string column,
            string principalTable,
            string constraint)
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var first = await AcceptedOrderIdAsync(harness);
            var second = await AcceptedOrderIdAsync(harness);

            var foreignParentId = await ScalarAsync(harness,
                $"SELECT TOP 1 [Id] FROM [Order].[{principalTable}] WHERE [OrderId] = {second} ORDER BY [Id]");

            var violation = await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(harness,
                $"UPDATE [Order].[{table}] SET [{column}] = {foreignParentId} WHERE [OrderId] = {first};"));

            Assert.Equal(ForeignKeyViolation, violation.Number);
            Assert.Contains(constraint, violation.Message);
        }

        [Theory]
        [InlineData("OrderItemId", "OrderItems", "FK_OrderItemServiceLinks_OrderItems_OrderIdAtAssociation_OrderItemId")]
        [InlineData("OrderServiceId", "OrderServices", "FK_OrderItemServiceLinks_OrderServices_OrderIdAtAssociation_OrderServiceId")]
        [InlineData("LinkedByChangeId", "OrderChanges", "FK_OrderItemServiceLinks_OrderChanges_OrderIdAtAssociation_LinkedByChangeId")]
        public async Task An_item_service_link_cannot_bind_a_row_of_another_order(string column, string principalTable, string constraint)
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var first = await AcceptedOrderIdAsync(harness);
            var second = await AcceptedOrderIdAsync(harness);

            var foreignParentId = await ScalarAsync(harness,
                $"SELECT TOP 1 [Id] FROM [Order].[{principalTable}] WHERE [OrderId] = {second} ORDER BY [Id]");

            var violation = await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(harness,
                $"UPDATE [Order].[OrderItemServiceLinks] SET [{column}] = {foreignParentId} WHERE [OrderIdAtAssociation] = {first};"));

            Assert.Equal(ForeignKeyViolation, violation.Number);
            Assert.Contains(constraint, violation.Message);
        }

        [Fact]
        public async Task A_traveller_cannot_be_given_a_guardian_from_another_order()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var first = await AcceptedOrderIdAsync(harness);
            var second = await AcceptedOrderIdAsync(harness);

            var foreignTravellerId = await ScalarAsync(harness,
                $"SELECT TOP 1 [Id] FROM [Order].[OrderTravellers] WHERE [OrderId] = {second} ORDER BY [Id]");

            var violation = await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(harness,
                $"UPDATE [Order].[OrderTravellers] SET [InfantParentTravellerId] = {foreignTravellerId} WHERE [OrderId] = {first};"));

            Assert.Equal(ForeignKeyViolation, violation.Number);
            Assert.Contains("FK_OrderTravellers_OrderTravellers_OrderId_InfantParentTravellerId", violation.Message);
        }

        [Fact]
        public async Task A_price_change_set_cannot_be_attached_to_a_commercial_change_of_another_order()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var first = await AcceptedOrderIdAsync(harness);
            var second = await AcceptedOrderIdAsync(harness);
            var foreignChangeId = await SpareChangeAsync(harness, second);

            var violation = await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(harness,
                $"UPDATE [Order].[PriceChangeSets] SET [ChangeId] = {foreignChangeId} WHERE [OrderId] = {first};"));

            Assert.Equal(ForeignKeyViolation, violation.Number);
            Assert.Contains("FK_PriceChangeSets_OrderChanges_OrderId_ChangeId", violation.Message);
        }

        [Fact]
        public async Task A_fare_construction_cannot_be_attributed_to_a_commercial_change_of_another_order()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, (now, scope) =>
                CandidateBuilder.OneWayFare100Tax20(now, scope).OneWayConstruction("BOUND-1"));
            var first = (await harness.SendAsync(S1Commands.Backoffice(offerId))).OrderId;
            var second = await AcceptedOrderIdAsync(harness);
            var foreignChangeId = await SpareChangeAsync(harness, second);

            Assert.Equal(1, await ScalarAsync(harness, $"SELECT COUNT(1) FROM [Order].[FareConstructions] WHERE [OrderId] = {first}"));

            var violation = await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(harness,
                $"UPDATE [Order].[FareConstructions] SET [CreatedByChangeId] = {foreignChangeId} WHERE [OrderId] = {first};"));

            Assert.Equal(ForeignKeyViolation, violation.Number);
            Assert.Contains("FK_FareConstructions_OrderChanges_OrderId_CreatedByChangeId", violation.Message);
        }

        [Fact]
        public async Task A_segment_cannot_be_moved_into_a_journey_of_another_order()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var first = await AcceptedOrderIdAsync(harness);
            var second = await AcceptedOrderIdAsync(harness);
            var foreignJourneyId = await SpareJourneyAsync(harness, second);

            var violation = await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(harness,
                $"UPDATE [Order].[OrderSegments] SET [JourneyId] = {foreignJourneyId} WHERE [OrderId] = {first};"));

            Assert.Equal(ForeignKeyViolation, violation.Number);
            Assert.Contains("FK_OrderSegments_OrderJourneys_OrderId_JourneyId", violation.Message);
        }

        [Fact]
        public async Task A_pricing_line_cannot_be_moved_into_a_price_change_set_of_another_order()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var first = await AcceptedOrderIdAsync(harness);
            var second = await AcceptedOrderIdAsync(harness);
            var foreignSetId = await SparePriceChangeSetAsync(harness, second);

            var violation = await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(harness,
                $"UPDATE TOP (1) [Order].[PricingLines] SET [PriceChangeSetId] = {foreignSetId} WHERE [OrderId] = {first};"));

            Assert.Equal(ForeignKeyViolation, violation.Number);
            Assert.Contains("FK_PricingLines_PriceChangeSets_OrderId_PriceChangeSetId", violation.Message);
        }

        [Fact]
        public async Task A_same_order_reassociation_of_a_service_to_its_own_item_remains_valid()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var orderId = await AcceptedOrderIdAsync(harness);
            var itemId = await ScalarAsync(harness, $"SELECT TOP 1 [Id] FROM [Order].[OrderItems] WHERE [OrderId] = {orderId}");

            await ExecuteAsync(harness, $"UPDATE [Order].[OrderServices] SET [OrderItemId] = {itemId} WHERE [OrderId] = {orderId};");

            Assert.Equal(itemId, await ScalarAsync(harness,
                $"SELECT TOP 1 [OrderItemId] FROM [Order].[OrderServices] WHERE [OrderId] = {orderId}"));
        }

        private static async Task<long> SpareChangeAsync(S1Harness harness, long orderId)
        {
            var spareId = await NextSpareIdAsync(harness, "[Order].[OrderChanges]");
            var actorContextType = await ScalarAsync(harness, $"SELECT TOP 1 [ActorContextType] FROM [Order].[OrderChanges] WHERE [OrderId] = {orderId}");

            await ExecuteAsync(harness, $@"
INSERT INTO [Order].[OrderChanges] ([Id],[OrderId],[Type],[CommercialVersion],[ActorContextType],[CommittedAt],[LastUpdateTime])
VALUES ({spareId}, {orderId}, 1, 99, {actorContextType}, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());");

            return spareId;
        }

        private static async Task<long> SpareJourneyAsync(S1Harness harness, long orderId)
        {
            var spareId = await NextSpareIdAsync(harness, "[Order].[OrderJourneys]");

            await ExecuteAsync(harness, $@"
INSERT INTO [Order].[OrderJourneys] ([Id],[OrderId],[BoundId],[Sequence],[Direction],[OriginAirportId],[DestinationAirportId],[LastUpdateTime])
VALUES ({spareId}, {orderId}, 'SPARE-BOUND', 99, 1, 1001, 1002, SYSDATETIMEOFFSET());");

            return spareId;
        }

        private static async Task<long> SparePriceChangeSetAsync(S1Harness harness, long orderId)
        {
            var spareChangeId = await SpareChangeAsync(harness, orderId);
            var spareId = await NextSpareIdAsync(harness, "[Order].[PriceChangeSets]");

            await ExecuteAsync(harness, $@"
INSERT INTO [Order].[PriceChangeSets] ([Id],[OrderId],[ChangeId],[FinancialSequence],[Reason],[CommittedAt],[LastUpdateTime])
VALUES ({spareId}, {orderId}, {spareChangeId}, 99, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());");

            return spareId;
        }

        private static Task<long> NextSpareIdAsync(S1Harness harness, string table)
            => ScalarAsync(harness, $"SELECT ISNULL(MAX([Id]), 0) + 1 FROM {table}");

        private static async Task<long> AcceptedOrderIdAsync(S1Harness harness)
        {
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);
            return (await harness.SendAsync(S1Commands.Backoffice(offerId))).OrderId;
        }

        private static Task<List<(string Table, string[] Columns, string PrincipalTable)>> ForeignKeyColumnsAsync(S1Harness harness)
            => harness.InScopeAsync(async services =>
            {
                var context = services.GetRequiredService<OrderingDbContext>();
                await using var command = context.Database.GetDbConnection().CreateCommand();
                command.CommandText = @"
SELECT fk.[name] AS ConstraintName,
       OBJECT_NAME(fk.parent_object_id) AS ParentTable,
       OBJECT_NAME(fk.referenced_object_id) AS PrincipalTable,
       parentColumn.[name] AS ParentColumn
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.[object_id]
JOIN sys.columns parentColumn ON parentColumn.[object_id] = fkc.parent_object_id AND parentColumn.column_id = fkc.parent_column_id
WHERE SCHEMA_NAME(fk.[schema_id]) IN ('Order', 'Operations')
ORDER BY fk.[name], fkc.constraint_column_id;";

                if (command.Connection!.State != ConnectionState.Open)
                    await command.Connection.OpenAsync();

                var rows = new List<(string Constraint, string Table, string PrincipalTable, string Column)>();
                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                    rows.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));

                return rows
                    .GroupBy(row => row.Constraint)
                    .Select(group => (
                        group.First().Table,
                        group.Select(row => row.Column).ToArray(),
                        group.First().PrincipalTable))
                    .ToList();
            });

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

                if (command.Connection!.State != ConnectionState.Open)
                    await command.Connection.OpenAsync();

                return Convert.ToInt64(await command.ExecuteScalarAsync());
            });
    }
}
