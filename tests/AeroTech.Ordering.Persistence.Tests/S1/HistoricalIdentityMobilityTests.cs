using System.Data;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.Tests._Shared;
using AeroTech.Ordering.Persistence.Tests._Shared;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class HistoricalIdentityMobilityTests
    {
        private const int ForeignKeyViolation = 547;

        private readonly OrderingDatabaseFixture _fixture;

        public HistoricalIdentityMobilityTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task A_service_can_move_to_another_order_of_the_same_owner_without_touching_its_history()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var source = await AcceptedOrderAsync(harness);
            var child = await ChildShapedOrderAsync(harness);

            var serviceId = await ScalarAsync(harness, $"SELECT TOP 1 [Id] FROM [Order].[OrderServices] WHERE [OrderId] = {source}");
            var creatingChangeId = await ScalarAsync(harness, $"SELECT [CreatedByChangeId] FROM [Order].[OrderServices] WHERE [Id] = {serviceId}");
            var obligationId = await AddFundingObligationAsync(harness, source, serviceId);

            var historyBefore = await HistorySnapshotAsync(harness, serviceId, obligationId);

            await MoveServiceAsync(harness, serviceId, child);

            Assert.Equal(child, await ScalarAsync(harness, $"SELECT [OrderId] FROM [Order].[OrderServices] WHERE [Id] = {serviceId}"));
            Assert.Equal(creatingChangeId, await ScalarAsync(harness, $"SELECT [CreatedByChangeId] FROM [Order].[OrderServices] WHERE [Id] = {serviceId}"));
            Assert.Equal(source, await ScalarAsync(harness, $"SELECT [OrderId] FROM [Order].[OrderChanges] WHERE [Id] = {creatingChangeId}"));

            Assert.Equal(historyBefore, await HistorySnapshotAsync(harness, serviceId, obligationId));
        }

        [Fact]
        public async Task The_item_service_link_keeps_its_occurrence_order_after_the_service_moves()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var source = await AcceptedOrderAsync(harness);
            var child = await ChildShapedOrderAsync(harness);
            var serviceId = await ScalarAsync(harness, $"SELECT TOP 1 [Id] FROM [Order].[OrderServices] WHERE [OrderId] = {source}");

            await MoveServiceAsync(harness, serviceId, child);

            Assert.Equal(source, await ScalarAsync(harness,
                $"SELECT [OrderIdAtAssociation] FROM [Order].[OrderItemServiceLinks] WHERE [OrderServiceId] = {serviceId}"));
            Assert.Equal(source, await ScalarAsync(harness, $@"
SELECT item.[OrderId]
FROM [Order].[OrderItemServiceLinks] link
JOIN [Order].[OrderItems] item ON item.[Id] = link.[OrderItemId]
WHERE link.[OrderServiceId] = {serviceId}"));
        }

        [Fact]
        public async Task A_pricing_line_keeps_its_occurrence_order_and_item_after_the_service_moves()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var source = await AcceptedOrderAsync(harness);
            var child = await ChildShapedOrderAsync(harness);
            var serviceId = await ScalarAsync(harness, $"SELECT TOP 1 [Id] FROM [Order].[OrderServices] WHERE [OrderId] = {source}");
            var linesBefore = await ScalarAsync(harness, $"SELECT COUNT(1) FROM [Order].[PricingLines] WHERE [OrderId] = {source}");

            await MoveServiceAsync(harness, serviceId, child);

            Assert.True(linesBefore > 0);
            Assert.Equal(linesBefore, await ScalarAsync(harness, $"SELECT COUNT(1) FROM [Order].[PricingLines] WHERE [OrderId] = {source}"));
            Assert.Equal(0, await ScalarAsync(harness, $@"
SELECT COUNT(1)
FROM [Order].[PricingLines] line
JOIN [Order].[OrderItems] item ON item.[Id] = line.[OrderItemId]
WHERE line.[OrderId] = {source} AND item.[OrderId] <> {source}"));
        }

        [Fact]
        public async Task A_funding_obligation_scoped_to_the_moved_service_stays_valid_and_keeps_its_occurrence_order()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var source = await AcceptedOrderAsync(harness);
            var child = await ChildShapedOrderAsync(harness);
            var serviceId = await ScalarAsync(harness, $"SELECT TOP 1 [Id] FROM [Order].[OrderServices] WHERE [OrderId] = {source}");
            var obligationId = await AddFundingObligationAsync(harness, source, serviceId);

            await MoveServiceAsync(harness, serviceId, child);

            Assert.Equal(source, await ScalarAsync(harness, $"SELECT [OrderId] FROM [Order].[FundingObligations] WHERE [Id] = {obligationId}"));
            Assert.Equal(serviceId, await ScalarAsync(harness, $"SELECT [OrderServiceId] FROM [Order].[FundingObligations] WHERE [Id] = {obligationId}"));
            Assert.Equal(1, await ScalarAsync(harness, $@"
SELECT COUNT(1)
FROM [Order].[FundingObligations] obligation
JOIN [Order].[OrderServices] service ON service.[Id] = obligation.[OrderServiceId]
WHERE obligation.[Id] = {obligationId} AND service.[OrderId] = {child}"));
        }

        [Fact]
        public async Task A_moved_service_must_still_take_the_current_item_traveller_and_segment_of_its_new_order()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var source = await AcceptedOrderAsync(harness);
            var child = await ChildShapedOrderAsync(harness);
            var serviceId = await ScalarAsync(harness, $"SELECT TOP 1 [Id] FROM [Order].[OrderServices] WHERE [OrderId] = {source}");

            var violation = await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(harness,
                $"UPDATE [Order].[OrderServices] SET [OrderId] = {child} WHERE [Id] = {serviceId};"));

            Assert.Equal(ForeignKeyViolation, violation.Number);
            Assert.Equal(source, await ScalarAsync(harness, $"SELECT [OrderId] FROM [Order].[OrderServices] WHERE [Id] = {serviceId}"));
        }

        [Theory]
        [InlineData("OrderItemId", "OrderItems", "FK_OrderServices_OrderItems_OrderId_OrderItemId")]
        [InlineData("TravellerId", "OrderTravellers", "FK_OrderServices_OrderTravellers_OrderId_TravellerId")]
        [InlineData("SegmentId", "OrderSegments", "FK_OrderServices_OrderSegments_OrderId_SegmentId")]
        public async Task A_moved_service_cannot_keep_a_containment_reference_to_its_previous_order(
            string column,
            string principalTable,
            string constraint)
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var source = await AcceptedOrderAsync(harness);
            var child = await ChildShapedOrderAsync(harness);
            var serviceId = await ScalarAsync(harness, $"SELECT TOP 1 [Id] FROM [Order].[OrderServices] WHERE [OrderId] = {source}");

            var childItemId = await ScalarAsync(harness, $"SELECT TOP 1 [Id] FROM [Order].[OrderItems] WHERE [OrderId] = {child}");
            var childTravellerId = await FreeChildTravellerAsync(harness, child);
            var childSegmentId = await ScalarAsync(harness, $"SELECT TOP 1 [Id] FROM [Order].[OrderSegments] WHERE [OrderId] = {child}");

            var values = new Dictionary<string, long>
            {
                ["OrderItemId"] = childItemId,
                ["TravellerId"] = childTravellerId,
                ["SegmentId"] = childSegmentId
            };

            values[column] = await ScalarAsync(harness, $"SELECT TOP 1 [Id] FROM [Order].[{principalTable}] WHERE [OrderId] = {source}");

            var violation = await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(harness, $@"
UPDATE [Order].[OrderServices]
SET [OrderId] = {child}, [OrderItemId] = {values["OrderItemId"]}, [TravellerId] = {values["TravellerId"]}, [SegmentId] = {values["SegmentId"]}
WHERE [Id] = {serviceId};"));

            Assert.Equal(ForeignKeyViolation, violation.Number);
            Assert.Contains(constraint, violation.Message);
        }

        private static async Task MoveServiceAsync(S1Harness harness, long serviceId, long child)
        {
            var childItemId = await ScalarAsync(harness, $"SELECT TOP 1 [Id] FROM [Order].[OrderItems] WHERE [OrderId] = {child}");
            var childTravellerId = await FreeChildTravellerAsync(harness, child);
            var childSegmentId = await ScalarAsync(harness, $"SELECT TOP 1 [Id] FROM [Order].[OrderSegments] WHERE [OrderId] = {child}");

            await ExecuteAsync(harness, $@"
UPDATE [Order].[OrderServices]
SET [OrderId] = {child}, [OrderItemId] = {childItemId}, [TravellerId] = {childTravellerId}, [SegmentId] = {childSegmentId}
WHERE [Id] = {serviceId};");
        }

        private static Task<long> FreeChildTravellerAsync(S1Harness harness, long child)
            => ScalarAsync(harness, $@"
SELECT TOP 1 traveller.[Id]
FROM [Order].[OrderTravellers] traveller
WHERE traveller.[OrderId] = {child}
  AND NOT EXISTS (SELECT 1 FROM [Order].[OrderServices] service WHERE service.[TravellerId] = traveller.[Id])
ORDER BY traveller.[Id]");

        private static async Task<long> AddFundingObligationAsync(S1Harness harness, long orderId, long serviceId)
        {
            var obligationId = await ScalarAsync(harness, "SELECT ISNULL(MAX([Id]), 0) + 1 FROM [Order].[FundingObligations]");
            var changeId = await ScalarAsync(harness, $"SELECT TOP 1 [Id] FROM [Order].[OrderChanges] WHERE [OrderId] = {orderId}");
            var setId = await ScalarAsync(harness, $"SELECT TOP 1 [Id] FROM [Order].[PriceChangeSets] WHERE [OrderId] = {orderId}");
            var currencyId = await ScalarAsync(harness, $"SELECT [CurrencyId] FROM [Order].[Orders] WHERE [Id] = {orderId}");

            await ExecuteAsync(harness, $@"
INSERT INTO [Order].[FundingObligations]
    ([Id],[OrderId],[Version],[Purpose],[AmountAmount],[AmountCurrencyId],[OrderServiceId],[ChangeId],[PriceChangeSetId],[LastUpdateTime])
VALUES
    ({obligationId}, {orderId}, 1, {(int)FundingObligationPurpose.OriginalSale}, 120, {currencyId}, {serviceId}, {changeId}, {setId}, SYSDATETIMEOFFSET());");

            return obligationId;
        }

        private static Task<string> HistorySnapshotAsync(S1Harness harness, long serviceId, long obligationId)
            => TextAsync(harness, $@"
SELECT
    CAST((SELECT COUNT(1) FROM [Order].[OrderItemServiceLinks] WHERE [OrderServiceId] = {serviceId}) AS nvarchar(32))
    + '|' + (SELECT CAST([OrderIdAtAssociation] AS nvarchar(32)) FROM [Order].[OrderItemServiceLinks] WHERE [OrderServiceId] = {serviceId})
    + '|' + (SELECT CAST([OrderItemId] AS nvarchar(32)) FROM [Order].[OrderItemServiceLinks] WHERE [OrderServiceId] = {serviceId})
    + '|' + (SELECT CAST([LinkedByChangeId] AS nvarchar(32)) FROM [Order].[OrderItemServiceLinks] WHERE [OrderServiceId] = {serviceId})
    + '|' + (SELECT CAST([OrderId] AS nvarchar(32)) FROM [Order].[FundingObligations] WHERE [Id] = {obligationId})
    + '|' + (SELECT CAST([OrderServiceId] AS nvarchar(32)) FROM [Order].[FundingObligations] WHERE [Id] = {obligationId})");

        private static async Task<long> AcceptedOrderAsync(S1Harness harness)
        {
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);
            return (await harness.SendAsync(S1Commands.Backoffice(offerId))).OrderId;
        }

        private static async Task<long> ChildShapedOrderAsync(S1Harness harness)
        {
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, (now, scope) => new CandidateBuilder(now, scope)
                .Traveller("PAX-A")
                .Traveller("PAX-B")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .Package("ITEM-A", "S-A")
                .Line("fare", "ITEM-A", PricingComponentType.Fare, 100m, "S-A"));

            var created = await harness.SendAsync(S1Commands.Backoffice(offerId, travellers: S1Commands.Travellers("PAX-A", "PAX-B")));
            return created.OrderId;
        }

        private static Task ExecuteAsync(S1Harness harness, string sql)
            => harness.InScopeAsync(async services =>
            {
                await services.GetRequiredService<OrderingDbContext>().Database.ExecuteSqlRawAsync(sql);
                return 0;
            });

        private static Task<long> ScalarAsync(S1Harness harness, string sql)
            => harness.InScopeAsync(async services => Convert.ToInt64(await RawScalarAsync(services, sql)));

        private static Task<string> TextAsync(S1Harness harness, string sql)
            => harness.InScopeAsync(async services => Convert.ToString(await RawScalarAsync(services, sql))!);

        private static async Task<object?> RawScalarAsync(IServiceProvider services, string sql)
        {
            var context = services.GetRequiredService<OrderingDbContext>();
            await using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            if (command.Connection!.State != ConnectionState.Open)
                await command.Connection.OpenAsync();

            return await command.ExecuteScalarAsync();
        }
    }
}
