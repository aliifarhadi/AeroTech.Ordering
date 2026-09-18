using System.Security.Claims;
using AeroTech.Framework.Core.Domain.Events;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Ordering.Persistence;
using AeroTech.Ordering.Query._Shared.DbContexts;
using AeroTech.Ordering.ReferenceData.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests._Shared
{
    public sealed class OrderingDatabaseFixture : IDisposable
    {
        private const string Server = @"localhost\SQLEXPRESS";
        private const string DatabasePrefix = "OrderingS1_";

        private readonly DbContextOptions<OrderingDbContext> _commandOptions;
        private readonly DbContextOptions<OrderQueryDbContext> _queryOptions;
        private readonly DbContextOptions<ReferenceDbContext> _referenceOptions;

        public OrderingDatabaseFixture()
        {
            var runId = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}"[..23];

            DatabaseName = $"{DatabasePrefix}{runId}";
            OwnerDatabaseName = $"{DatabasePrefix}{runId}_Owner";
            ConnectionString = BuildConnectionString(DatabaseName);
            OwnerConnectionString = BuildConnectionString(OwnerDatabaseName);

            _commandOptions = new DbContextOptionsBuilder<OrderingDbContext>()
                .UseSqlServer(
                    ConnectionString,
                    sql => sql.MigrationsHistoryTable(OrderingDbContext.MigrationsHistoryTable, OrderingDbContext.MigrationsHistorySchema))
                .Options;
            _queryOptions = new DbContextOptionsBuilder<OrderQueryDbContext>()
                .UseSqlServer(
                    ConnectionString,
                    sql => sql.MigrationsHistoryTable(OrderQueryDbContext.MigrationsHistoryTable, OrderQueryDbContext.MigrationsHistorySchema))
                .Options;
            _referenceOptions = new DbContextOptionsBuilder<ReferenceDbContext>()
                .UseSqlServer(
                    ConnectionString,
                    sql => sql.MigrationsHistoryTable(ReferenceDbContext.MigrationsHistoryTable, ReferenceDbContext.MigrationsHistorySchema))
                .Options;

            using var command = NewCommandContext();
            command.Database.Migrate();

            using var reference = NewReferenceContext();
            reference.Database.Migrate();

            using var query = NewQueryContext();
            query.Database.Migrate();
        }

        public string DatabaseName { get; }

        public string OwnerDatabaseName { get; }

        public string ConnectionString { get; }

        public string OwnerConnectionString { get; }

        public DbContextOptions<OrderingDbContext> CommandOptions => _commandOptions;

        public OrderingDbContext NewCommandContext() => NewCommandContext(new NullDomainEventDispatcher());

        public OrderingDbContext NewCommandContext(IDomainEventDispatcher dispatcher)
            => new(_commandOptions, new NullIdentityService(), new FixedClock(), dispatcher);

        public OrderQueryDbContext NewQueryContext() => new(_queryOptions);

        public ReferenceDbContext NewReferenceContext() => new(_referenceOptions);

        public void Dispose()
        {
            SqlConnection.ClearAllPools();
            DropDatabase(DatabaseName);
            DropDatabase(OwnerDatabaseName);
        }

        private static string BuildConnectionString(string database)
            => $"Server={Server};Database={database};Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False";

        private static void DropDatabase(string database)
        {
            if (!database.StartsWith(DatabasePrefix, StringComparison.Ordinal))
                throw new InvalidOperationException($"Refusing to drop '{database}'.");

            using var connection = new SqlConnection(BuildConnectionString("master"));
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText =
                $"IF DB_ID(N'{database}') IS NOT NULL BEGIN ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]; END";
            command.ExecuteNonQuery();
        }

        public sealed class FixedClock : IClock
        {
            public DateTimeOffset GetDateTime() => new(2026, 9, 8, 10, 0, 0, TimeSpan.Zero);

            public DateOnly GetDate() => new(2026, 9, 8);
        }

        public sealed class NullDomainEventDispatcher : IDomainEventDispatcher
        {
            public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
        }

        public sealed class NullIdentityService : IIdentityService
        {
            public long? CurrentUserId => 1;

            public long? CurrentCustomerId => 1;

            public long RequiredCurrentUserId => 1;

            public Guid RequiredDeviceId => Guid.Empty;

            public bool IsAuthenticated => true;

            public List<Claim>? Claims => null;

            public void CheckAccess(string scopeType, object scopeId)
            {
            }
        }
    }

    [CollectionDefinition(Name)]
    public sealed class OrderingDatabaseCollection : ICollectionFixture<OrderingDatabaseFixture>
    {
        public const string Name = "OrderingDatabase";
    }
}
