using AeroTech.Ordering.Domain.OrderAggregate;
using AeroTech.Ordering.Domain.OrderAggregate.Contracts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class OrderRepository : IOrderRepository
    {
        private readonly OrderingDbContext _dbContext;

        public OrderRepository(OrderingDbContext dbContext) => _dbContext = dbContext;

        public void Add(Order order) => _dbContext.Set<Order>().Add(order);

        public Task<Order?> LoadSnapshotAsync(long orderId, long ownerAirlineId, CancellationToken cancellationToken = default)
            => _dbContext.Set<Order>()
                .AsNoTracking()
                .AsSplitQuery()
                .Include(order => order.Travellers)
                .Include(order => order.Contacts)
                .Include(order => order.Journeys)
                .Include(order => order.Segments).ThenInclude(segment => segment.Legs)
                .Include(order => order.Items)
                .Include(order => order.Services)
                .Include(order => order.Changes)
                .Include(order => order.PriceChangeSets)
                .Include(order => order.PricingLines)
                .Include(order => order.FareConstructions).ThenInclude(construction => construction.Items)
                .Include(order => order.FareConstructions).ThenInclude(construction => construction.PricingUnits).ThenInclude(unit => unit.CoveredBounds)
                .Include(order => order.FareConstructions).ThenInclude(construction => construction.PricingUnits).ThenInclude(unit => unit.Components)
                .Include(order => order.FundingObligations)
                .SingleOrDefaultAsync(order => order.Id == orderId && order.OwnerAirlineId == ownerAirlineId, cancellationToken);
    }
}
