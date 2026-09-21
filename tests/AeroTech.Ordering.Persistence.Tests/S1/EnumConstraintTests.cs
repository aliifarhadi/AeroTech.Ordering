using System.Data;
using AeroTech.Ordering.Domain.Tests._Shared;
using AeroTech.Ordering.Persistence.Tests._Shared;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class EnumConstraintTests
    {
        private const int CheckConstraintViolation = 547;

        private readonly OrderingDatabaseFixture _fixture;

        public EnumConstraintTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Every_persisted_enum_column_is_guarded_by_a_live_check_constraint()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var columns = await EnumColumnsAsync(harness);
            var live = await CheckConstraintsAsync(harness);

            var unguarded = columns
                .Where(column => !live.ContainsKey($"CK_{column.Table}_{column.Column}_Enum"))
                .Select(column => $"{column.Schema}.{column.Table}.{column.Column} ({column.EnumType.Name})")
                .OrderBy(entry => entry)
                .ToList();

            Assert.NotEmpty(columns);
            Assert.Empty(unguarded);
        }

        [Fact]
        public async Task Every_enum_check_constraint_admits_exactly_the_defined_members_of_its_enum()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var columns = await EnumColumnsAsync(harness);
            var live = await CheckConstraintsAsync(harness);

            var wrong = new List<string>();

            foreach (var column in columns)
            {
                var name = $"CK_{column.Table}_{column.Column}_Enum";

                if (!live.TryGetValue(name, out var definition))
                    continue;

                var defined = Enum.GetValuesAsUnderlyingType(column.EnumType)
                    .Cast<object>()
                    .Select(value => Convert.ToInt32(value))
                    .Distinct()
                    .Order()
                    .ToList();

                var admitted = Digits(definition).Order().ToList();

                if (!defined.SequenceEqual(admitted))
                    wrong.Add($"{name}: admits [{string.Join(",", admitted)}] but {column.EnumType.Name} defines [{string.Join(",", defined)}]");

                if (column.IsNullable != definition.Contains("IS NULL"))
                    wrong.Add($"{name}: nullability of the column and of the constraint disagree");
            }

            Assert.Empty(wrong);
        }

        [Theory]
        [InlineData("Orders", "CommercialSummary")]
        [InlineData("Orders", "JourneyType")]
        [InlineData("Orders", "Channel")]
        [InlineData("Orders", "InitiatingActorContextType")]
        [InlineData("OrderItems", "Kind")]
        [InlineData("OrderItems", "CommercialStatus")]
        [InlineData("OrderServices", "ServiceType")]
        [InlineData("OrderServices", "CommercialStatus")]
        [InlineData("OrderServices", "FulfillmentProfileAssurance")]
        [InlineData("OrderServices", "ReservationRequirement")]
        [InlineData("OrderServices", "FulfillmentDocumentKind")]
        [InlineData("OrderServices", "FundingRequirement")]
        [InlineData("OrderSegments", "Kind")]
        [InlineData("OrderJourneys", "Direction")]
        [InlineData("OrderTravellers", "PassengerTypeCode")]
        [InlineData("OrderContacts", "Role")]
        [InlineData("OrderChanges", "Type")]
        [InlineData("OrderChanges", "ActorContextType")]
        [InlineData("PriceChangeSets", "Reason")]
        [InlineData("PricingLines", "Component")]
        [InlineData("PricingLines", "Effect")]
        [InlineData("PricingLines", "Role")]
        [InlineData("PricingLines", "CalculationKind")]
        [InlineData("PricingLines", "BasisType")]
        [InlineData("OrderComponentTotals", "Component")]
        [InlineData("OrderComponentTotals", "Effect")]
        [InlineData("FareConstructions", "Assurance")]
        [InlineData("FarePricingUnits", "Type")]
        [InlineData("FarePricingUnits", "SourceConstructionType")]
        public async Task An_undefined_enum_value_is_rejected_by_the_database(string table, string column)
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, (now, scope) =>
                CandidateBuilder.OneWayFare100Tax20(now, scope).OneWayConstruction("BOUND-1"));
            await harness.SendAsync(S1Commands.Backoffice(offerId));

            Assert.True(await ScalarAsync(harness, $"SELECT COUNT(1) FROM [Order].[{table}]") > 0);

            var violation = await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(harness,
                $"UPDATE TOP (1) [Order].[{table}] SET [{column}] = 9999;"));

            Assert.Equal(CheckConstraintViolation, violation.Number);
            Assert.Contains($"CK_{table}_{column}_Enum", violation.Message);
        }

        [Fact]
        public async Task An_optional_enum_column_still_accepts_null()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);
            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));

            await ExecuteAsync(harness, $"UPDATE [Order].[OrderServices] SET [DocumentAuthority] = NULL WHERE [OrderId] = {created.OrderId};");

            Assert.Equal(0, await ScalarAsync(harness,
                $"SELECT COUNT(1) FROM [Order].[OrderServices] WHERE [OrderId] = {created.OrderId} AND [DocumentAuthority] IS NOT NULL"));
        }

        [Fact]
        public async Task An_optional_enum_column_still_rejects_an_undefined_value()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);
            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));

            var violation = await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(harness,
                $"UPDATE [Order].[OrderServices] SET [DocumentAuthority] = 9999 WHERE [OrderId] = {created.OrderId};"));

            Assert.Equal(CheckConstraintViolation, violation.Number);
            Assert.Contains("CK_OrderServices_DocumentAuthority_Enum", violation.Message);
        }

        [Fact]
        public async Task A_defined_enum_value_is_accepted_by_the_database()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);
            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));
            var defined = (int)AeroTech.Messages.Ordering.Enums.OrderChangeType.Create;

            await ExecuteAsync(harness, $"UPDATE [Order].[OrderChanges] SET [Type] = {defined} WHERE [OrderId] = {created.OrderId};");

            Assert.Equal(defined, await ScalarAsync(harness,
                $"SELECT TOP 1 [Type] FROM [Order].[OrderChanges] WHERE [OrderId] = {created.OrderId}"));
        }

        public static Task<List<(string Schema, string Table, string Column, Type EnumType, bool IsNullable)>> EnumColumnsAsync(S1Harness harness)
            => harness.InScopeAsync(services =>
            {
                var model = services.GetRequiredService<OrderingDbContext>().Model;
                var columns = new List<(string, string, string, Type, bool)>();

                foreach (var entityType in model.GetEntityTypes())
                {
                    var table = entityType.GetTableName();
                    var schema = entityType.GetSchema();

                    if (table is null)
                        continue;

                    var store = StoreObjectIdentifier.Table(table, schema);

                    foreach (var property in entityType.GetDeclaredProperties())
                    {
                        var clrType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;

                        if (!clrType.IsEnum)
                            continue;

                        var column = property.GetColumnName(store);

                        if (column is null)
                            continue;

                        columns.Add((schema ?? "dbo", table, column, clrType, property.IsNullable));
                    }
                }

                return Task.FromResult(columns
                    .DistinctBy(column => (column.Item2, column.Item3))
                    .ToList());
            });

        public static Task<Dictionary<string, string>> CheckConstraintsAsync(S1Harness harness)
            => harness.InScopeAsync(async services =>
            {
                var context = services.GetRequiredService<OrderingDbContext>();
                await using var command = context.Database.GetDbConnection().CreateCommand();
                command.CommandText = @"
SELECT cc.[name], cc.[definition]
FROM sys.check_constraints cc
JOIN sys.objects o ON o.[object_id] = cc.parent_object_id
;";

                if (command.Connection!.State != ConnectionState.Open)
                    await command.Connection.OpenAsync();

                var rows = new Dictionary<string, string>(StringComparer.Ordinal);
                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                    rows[reader.GetString(0)] = reader.GetString(1);

                return rows;
            });

        private static IEnumerable<int> Digits(string definition)
        {
            var value = 0;
            var reading = false;

            foreach (var character in definition)
            {
                if (char.IsAsciiDigit(character))
                {
                    value = (value * 10) + (character - '0');
                    reading = true;
                    continue;
                }

                if (reading)
                    yield return value;

                value = 0;
                reading = false;
            }

            if (reading)
                yield return value;
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

                if (command.Connection!.State != ConnectionState.Open)
                    await command.Connection.OpenAsync();

                return Convert.ToInt64(await command.ExecuteScalarAsync());
            });
    }
}
