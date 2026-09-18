using AeroTech.Ordering.Persistence.Tests._Shared;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    public sealed class MigrationUpgradeTests
    {
        private const string B0Migration = "20260917144148_B0InfrastructureShell";
        private const string S1OrderCreateMigration = "20260918201810_S1OrderCreate";
        private const string S1ParityBackfillMigration = "20260918225050_S1DomainParityBackfill";

        [Fact]
        public async Task S1_migration_upgrades_a_b0_database_without_losing_outbox_or_inbox_rows()
        {
            var database = $"OrderingS1_Upgrade_{Guid.NewGuid():N}"[..40];
            var connectionString = $@"Server=localhost\SQLEXPRESS;Database={database};Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False";
            var options = new DbContextOptionsBuilder<OrderingDbContext>()
                .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable(OrderingDbContext.MigrationsHistoryTable, OrderingDbContext.MigrationsHistorySchema))
                .Options;

            try
            {
                await using (var context = NewContext(options))
                {
                    await context.GetService<IMigrator>().MigrateAsync(B0Migration);
                    await context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO [dbo].[OutboxMessages] ([EventId],[MessageType],[Payload],[OccurredOn],[AttemptCount],[LeaseVersion]) VALUES (N'900001', N'B0.Message', N'{{}}', SYSDATETIMEOFFSET(), 0, 0)");
                    await context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO [dbo].[InboxMessages] ([OwnerAirlineId],[SourceSystem],[EventId],[Consumer],[MessageType],[PayloadHash],[ReceivedOn]) VALUES (1, N'B0', N'e-1', N'/c', N't', REPLICATE('0', 64), SYSDATETIMEOFFSET())");
                }

                await using (var context = NewContext(options))
                {
                    Assert.Contains("20260918201810_S1OrderCreate", await context.Database.GetPendingMigrationsAsync());
                    await context.Database.MigrateAsync();

                    Assert.Empty(await context.Database.GetPendingMigrationsAsync());

                    var outbox = await context.OutboxMessages.AsNoTracking().SingleAsync(message => message.EventId == "900001");
                    Assert.Null(outbox.StreamKind);
                    Assert.Null(outbox.EventOrdinal);
                    Assert.Equal(1, await context.InboxMessages.CountAsync(message => message.EventId == "e-1"));
                    Assert.False(context.Database.HasPendingModelChanges());
                }
            }
            finally
            {
                SqlConnection.ClearAllPools();
                await using var master = new SqlConnection(@"Server=localhost\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False");
                await master.OpenAsync();
                await using var drop = master.CreateCommand();
                drop.CommandText = $"IF DB_ID(N'{database}') IS NOT NULL BEGIN ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]; END";
                await drop.ExecuteNonQueryAsync();
            }
        }

        [Fact]
        public async Task S1_domain_parity_upgrade_preserves_accepted_rows_without_inventing_facts()
        {
            var database = $"OrderingS1_Parity_{Guid.NewGuid():N}"[..40];
            var connectionString = $@"Server=localhost\SQLEXPRESS;Database={database};Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False";
            var options = new DbContextOptionsBuilder<OrderingDbContext>()
                .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable(OrderingDbContext.MigrationsHistoryTable, OrderingDbContext.MigrationsHistorySchema))
                .Options;

            try
            {
                await using (var context = NewContext(options))
                {
                    await context.GetService<IMigrator>().MigrateAsync(S1OrderCreateMigration);
                    await ExecuteAsync(context, LegacyOrderFixture.InsertSql);
                }

                await using (var context = NewContext(options))
                {
                    await context.GetService<IMigrator>().MigrateAsync(S1ParityBackfillMigration);

                    Assert.Equal(1, await ScalarAsync(context,
                        "SELECT COUNT(1) FROM [Order].[FareConstructions] WHERE [PricingUnitsJson] IS NOT NULL"));
                    Assert.Equal(1, await ScalarAsync(context,
                        "SELECT COUNT(1) FROM [Order].[AirTransportServiceDetails] WHERE [FlightNumber] = N'XX100'"));
                }

                await using (var context = NewContext(options))
                {
                    await context.Database.MigrateAsync();
                    Assert.Empty(await context.Database.GetPendingMigrationsAsync());
                    Assert.False(context.Database.HasPendingModelChanges());

                    Assert.Equal(0, await ScalarAsync(context,
                        "SELECT COUNT(1) FROM sys.columns WHERE [object_id] = OBJECT_ID(N'[Order].[FareConstructions]') AND [name] = N'PricingUnitsJson'"));
                    Assert.Equal(0, await ScalarAsync(context,
                        "SELECT COUNT(1) FROM sys.columns WHERE [object_id] = OBJECT_ID(N'[Order].[AirTransportServiceDetails]') AND [name] IN (N'FlightNumber', N'FlightVersion', N'MarketingCarrierRef', N'OperatingCarrierRef')"));

                    Assert.Equal(1, await ScalarAsync(context, "SELECT COUNT(1) FROM [Order].[Orders]"));
                    Assert.Equal(1, await ScalarAsync(context, "SELECT COUNT(1) FROM [Order].[OrderSegments]"));
                    Assert.Equal(1, await ScalarAsync(context, "SELECT COUNT(1) FROM [Order].[PricingLines]"));

                    Assert.Equal(0, await ScalarAsync(context, "SELECT COUNT(1) FROM [Order].[OrderJourneys]"));
                    Assert.Equal(1, await ScalarAsync(context, "SELECT COUNT(1) FROM [Order].[OrderSegments] WHERE [JourneyId] IS NULL"));

                    Assert.Equal(1, await ScalarAsync(context, "SELECT COUNT(1) FROM [Order].[PricingLines] WHERE [CalculationKind] = 1"));
                    Assert.Equal(0, await ScalarAsync(context, "SELECT COUNT(1) FROM [Order].[PricingLines] WHERE [CalculationKind] = 2"));

                    Assert.Equal(1, await ScalarAsync(context,
                        "SELECT COUNT(1) FROM [Order].[OrderSegments] WHERE [FlightNumber] = N'XX100' AND [FlightVersion] = N'3'"
                        + " AND [MarketingCarrierRef] = N'CARRIER-M' AND [OperatingCarrierRef] = N'CARRIER-O'"));
                    Assert.Equal(1, await ScalarAsync(context,
                        "SELECT COUNT(1) FROM [Order].[OrderSegments] WHERE [FlightRef] IS NULL AND [OriginTerminalRef] IS NULL"
                        + " AND [Duration] IS NULL AND [AircraftRef] IS NULL AND [SourceCapacityRef] IS NULL"));

                    Assert.Equal(1, await ScalarAsync(context,
                        "SELECT COUNT(1) FROM [Order].[OrderServices] WHERE [PriceTreatment] = 4 AND [FulfillmentProfileAssurance] = 2"
                        + " AND [ReservationRequirement] = 4 AND [DocumentKind] = 5 AND [FundingRequirement] = 1 AND [CapacityUnits] IS NULL"));

                    Assert.Equal(1, await ScalarAsync(context,
                        "SELECT COUNT(1) FROM [Order].[OrderItems] WHERE [ProductSourceSystem] = N'AirOffer'"
                        + " AND [ProductSourceOfferId] = N'LEGACY-OFFER-1' AND [TermsSourceSystem] = N'AirOffer'"
                        + " AND [TermsRefundability] = 1 AND [TermsChangeability] = 1 AND [TermsUpgradeEligibility] = 1"
                        + " AND [ProductCode] IS NULL AND [ProductName] IS NULL"));
                    Assert.Equal(1, await ScalarAsync(context,
                        "SELECT COUNT(1) FROM [Order].[OrderItems] item INNER JOIN [Order].[Orders] o ON o.[Id] = item.[OrderId]"
                        + " WHERE item.[TermsCapturedAt] = o.[SourceCapturedAt]"));

                    Assert.Equal(1, await ScalarAsync(context,
                        "SELECT COUNT(1) FROM [Order].[FarePricingUnits] WHERE [SourceUnitRef] = N'pricingUnits/0'"
                        + " AND [Type] = 1 AND [CombinationMethod] = 1 AND [SourceKindRaw] IS NULL AND [PricingGroupId] IS NULL"));
                    Assert.Equal(1, await ScalarAsync(context,
                        "SELECT COUNT(1) FROM [Order].[FarePricingUnitCoveredBounds] WHERE [SourceBoundRef] = N'BOUND-1'"));
                    Assert.Equal(1, await ScalarAsync(context,
                        "SELECT COUNT(1) FROM [Order].[FareComponents] WHERE [SourceFareRef] = N'AIRFARE-77' AND [FareBasis] = N'YOW'"
                        + " AND [FareFamily] = N'FLEX' AND [FareType] = N'Published' AND [CabinRef] = N'CABIN-1' AND [RbdRef] = N'RBD-1'"
                        + " AND [BookingClass] = N'Y' AND [TicketingRestrictionMinutes] IS NULL AND [TariffRef] IS NULL"));
                    Assert.Equal(0, await ScalarAsync(context, "SELECT COUNT(1) FROM [Order].[FarePricingGroups]"));
                    Assert.Equal(0, await ScalarAsync(context, "SELECT COUNT(1) FROM [Order].[FareComponentServices]"));
                    Assert.Equal(0, await ScalarAsync(context, "SELECT COUNT(1) FROM [Order].[FareComponentSegments]"));
                    Assert.Equal(1, await ScalarAsync(context, "SELECT COUNT(1) FROM [Order].[FareConstructionItems]"));

                    Assert.Equal(1, await ScalarAsync(context, "SELECT COUNT(1) FROM [Order].[OrderItemServiceLinkTravelers]"));
                    Assert.Equal(1, await ScalarAsync(context, "SELECT COUNT(1) FROM [Order].[OrderItemServiceLinkSegments]"));

                    Assert.Equal(1, await ScalarAsync(context,
                        "SELECT COUNT(1) FROM [Order].[Orders] WHERE [SaleCurrencyRef] = N'978' AND [SaleCurrencyCode] IS NULL"
                        + " AND [ObservedTicketingDeadlineValue] IS NULL AND [JourneyType] IS NULL"));

                    Assert.Equal(1, await ScalarAsync(context,
                        "SELECT COUNT(1) FROM [Order].[OrderPreparations] WHERE [Id] = " + LegacyOrderFixture.PreparationId));
                }
            }
            finally
            {
                await DropAsync(database);
            }
        }

        private static async Task ExecuteAsync(OrderingDbContext context, string sql)
        {
            await context.Database.OpenConnectionAsync();
            await using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();
        }

        private static async Task<int> ScalarAsync(OrderingDbContext context, string sql)
        {
            await using var command = context.Database.GetDbConnection().CreateCommand();
            await context.Database.OpenConnectionAsync();
            command.CommandText = sql;
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        private static async Task DropAsync(string database)
        {
            SqlConnection.ClearAllPools();
            await using var master = new SqlConnection(@"Server=localhost\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False");
            await master.OpenAsync();
            await using var drop = master.CreateCommand();
            drop.CommandText = $"IF DB_ID(N'{database}') IS NOT NULL BEGIN ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]; END";
            await drop.ExecuteNonQueryAsync();
        }

        private static OrderingDbContext NewContext(DbContextOptions<OrderingDbContext> options)
            => new(options, new OrderingDatabaseFixture.NullIdentityService(), new OrderingDatabaseFixture.FixedClock(), new OrderingDatabaseFixture.NullDomainEventDispatcher());
    }
}
