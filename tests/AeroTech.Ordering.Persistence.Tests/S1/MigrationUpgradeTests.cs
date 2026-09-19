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
                    Assert.NotEmpty(await context.Database.GetPendingMigrationsAsync());
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
