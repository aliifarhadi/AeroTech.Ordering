using AeroTech.Ordering.Domain.CommandReceiptAggregate;
using AeroTech.Ordering.Domain.CommandReceiptAggregate.Contracts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ordering.Persistence.CommandReceiptAggregate
{
    public sealed class CommandReceiptRepository : ICommandReceiptRepository
    {
        private readonly OrderingDbContext _dbContext;

        public CommandReceiptRepository(OrderingDbContext dbContext) => _dbContext = dbContext;

        public Task<CommandReceipt?> FindAsync(ReceiptScope scope, CancellationToken cancellationToken = default)
            => _dbContext.Set<CommandReceipt>()
                .AsNoTracking()
                .SingleOrDefaultAsync(receipt => receipt.OwnerAirlineId == scope.OwnerAirlineId
                                                 && receipt.CallerScope == scope.CallerScope
                                                 && receipt.CommandKind == scope.CommandKind
                                                 && receipt.IdempotencyKey == scope.IdempotencyKey,
                    cancellationToken);

        public void Add(CommandReceipt receipt) => _dbContext.Set<CommandReceipt>().Add(receipt);
    }
}
