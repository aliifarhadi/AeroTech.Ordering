using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Framework.Infrastructure.Persistence;
using AeroTech.Ordering.Persistence._Shared.Transactions;
using AeroTech.Ordering.Persistence.Inbox;
using AeroTech.Ordering.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AeroTech.Ordering.Persistence
{
    public sealed class OrderingDbContext : CommandDbContext, IUnitOfWork
    {
        public const string MigrationsHistorySchema = "dbo";
        public const string MigrationsHistoryTable = "__CommandsMigrationHistory";

        private readonly List<ICommandTransactionParticipant> _participants = new();

        public OrderingDbContext(
            DbContextOptions<OrderingDbContext> options,
            IIdentityService identityService,
            IClock clock,
            IDomainEventDispatcher domainEventDispatcher)
            : base(options, identityService, clock, domainEventDispatcher)
        {
        }

        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

        public void Enlist(ICommandTransactionParticipant participant)
        {
            if (!_participants.Contains(participant))
                _participants.Add(participant);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var transaction = Database.CurrentTransaction is null
                ? await Database.BeginTransactionAsync(cancellationToken)
                : null;

            try
            {
                var written = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

                foreach (var participant in _participants.ToList())
                    await participant.FlushAsync(Database.CurrentTransaction!.GetDbTransaction(), cancellationToken);

                if (transaction is not null)
                    await transaction.CommitAsync(cancellationToken);

                _participants.ForEach(participant => participant.Complete());
                return written;
            }
            catch (Exception exception)
            {
                if (transaction is not null)
                    await transaction.RollbackAsync(CancellationToken.None);

                ChangeTracker.Clear();
                _participants.ForEach(participant => participant.Abandon());

                if (exception is DbUpdateException update && CommitConflictTranslator.Translate(update) is { } conflict)
                    throw conflict;

                throw;
            }
            finally
            {
                if (transaction is not null)
                    await transaction.DisposeAsync();
            }
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
            => SaveChangesAsync(acceptAllChangesOnSuccess).GetAwaiter().GetResult();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Order");
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderingDbContext).Assembly);
        }
    }
}
