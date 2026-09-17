using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ordering.Persistence.Outbox
{
    public sealed class OutboxLeaseStore
    {
        private const string ClaimSql = $"""
            WITH claimable AS (
                SELECT TOP (@batchSize) *
                FROM [{OutboxMessageConfiguration.Schema}].[{OutboxMessageConfiguration.Table}] WITH (UPDLOCK, READPAST, ROWLOCK)
                WHERE ProcessedOn IS NULL AND (LeaseExpiresOn IS NULL OR LeaseExpiresOn <= @now)
                ORDER BY Id)
            UPDATE claimable
            SET LeaseOwner = @leaseOwner,
                LeaseExpiresOn = @leaseExpiresOn,
                LeaseVersion = LeaseVersion + 1,
                AttemptCount = AttemptCount + 1
            OUTPUT inserted.*
            """;

        private const string CompleteSql = $"""
            UPDATE [{OutboxMessageConfiguration.Schema}].[{OutboxMessageConfiguration.Table}]
            SET ProcessedOn = @now, LastError = NULL, LeaseOwner = NULL, LeaseExpiresOn = NULL
            WHERE Id = @id AND ProcessedOn IS NULL AND LeaseOwner = @leaseOwner AND LeaseVersion = @leaseVersion
            """;

        private const string FailSql = $"""
            UPDATE [{OutboxMessageConfiguration.Schema}].[{OutboxMessageConfiguration.Table}]
            SET LastError = @lastError
            WHERE Id = @id AND ProcessedOn IS NULL AND LeaseOwner = @leaseOwner AND LeaseVersion = @leaseVersion
            """;

        private readonly OrderingDbContext _dbContext;

        public OutboxLeaseStore(OrderingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<OutboxLease>> ClaimAsync(
            string leaseOwner,
            int batchSize,
            DateTimeOffset now,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default)
        {
            var claimed = await _dbContext.OutboxMessages
                .FromSqlRaw(
                    ClaimSql,
                    new SqlParameter("@batchSize", batchSize),
                    new SqlParameter("@now", now),
                    new SqlParameter("@leaseOwner", leaseOwner),
                    new SqlParameter("@leaseExpiresOn", now.Add(leaseDuration)))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return claimed
                .OrderBy(message => message.Id)
                .Select(message => new OutboxLease(
                    message.Id,
                    message.EventId,
                    message.MessageType,
                    message.Payload,
                    leaseOwner,
                    message.LeaseVersion))
                .ToList();
        }

        public async Task<bool> CompleteAsync(OutboxLease lease, DateTimeOffset now, CancellationToken cancellationToken = default)
        {
            var affected = await _dbContext.Database.ExecuteSqlRawAsync(
                CompleteSql,
                new object[]
                {
                    new SqlParameter("@now", now),
                    new SqlParameter("@id", lease.Id),
                    new SqlParameter("@leaseOwner", lease.LeaseOwner),
                    new SqlParameter("@leaseVersion", lease.LeaseVersion)
                },
                cancellationToken);

            return affected == 1;
        }

        public async Task<bool> RecordFailureAsync(OutboxLease lease, string error, CancellationToken cancellationToken = default)
        {
            var lastError = error.Length > OutboxMessageConfiguration.LastErrorMaxLength
                ? error[..OutboxMessageConfiguration.LastErrorMaxLength]
                : error;

            var affected = await _dbContext.Database.ExecuteSqlRawAsync(
                FailSql,
                new object[]
                {
                    new SqlParameter("@lastError", lastError),
                    new SqlParameter("@id", lease.Id),
                    new SqlParameter("@leaseOwner", lease.LeaseOwner),
                    new SqlParameter("@leaseVersion", lease.LeaseVersion)
                },
                cancellationToken);

            return affected == 1;
        }
    }
}
