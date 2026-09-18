using AeroTech.Ordering.Domain.OrderPreparationAggregate;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Contracts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ordering.Persistence.OrderPreparationAggregate
{
    public sealed class OrderPreparationRepository : IOrderPreparationRepository
    {
        private readonly OrderingDbContext _dbContext;

        public OrderPreparationRepository(OrderingDbContext dbContext) => _dbContext = dbContext;

        public Task<OrderPreparation?> FindInScopeAsync(long preparationId, long ownerAirlineId, long financialCustomerId, CancellationToken cancellationToken = default)
            => _dbContext.Set<OrderPreparation>()
                .SingleOrDefaultAsync(preparation => preparation.Id == preparationId
                                                     && preparation.OwnerAirlineId == ownerAirlineId
                                                     && preparation.FinancialCustomerId == financialCustomerId,
                    cancellationToken);

        public void Add(OrderPreparation preparation) => _dbContext.Set<OrderPreparation>().Add(preparation);
    }
}
