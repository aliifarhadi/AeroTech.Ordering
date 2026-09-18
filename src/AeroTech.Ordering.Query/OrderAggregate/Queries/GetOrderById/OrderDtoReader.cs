using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Query._Shared.DbContexts;
using AeroTech.Ordering.Query.OrderAggregate.Dto;
using AeroTech.Ordering.Query.OrderAggregate.Models;
using AeroTech.Ordering.Query.OrderAggregate.Projection;
using AeroTech.Ordering.Application._Shared.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrderById
{
    public sealed class OrderDtoReader
    {
        private readonly OrderQueryDbContext _dbContext;

        public OrderDtoReader(OrderQueryDbContext dbContext) => _dbContext = dbContext;

        public async Task<OrderDto> ReadAsync(long orderId, AuthorizedReadScope scope, CancellationToken cancellationToken)
        {
            var row = await _dbContext.Set<OrderDetailsReadModel>()
                .AsNoTracking()
                .Where(order => order.OrderId == orderId && order.OwnerAirlineId == scope.OwnerAirlineId)
                .Where(order => scope.RestrictToFinancialCustomerId == null || order.FinancialCustomerId == scope.RestrictToFinancialCustomerId)
                .SingleOrDefaultAsync(cancellationToken)
                ?? throw ExceptionFactory.OrderNotFound(orderId);

            var order = PublicOrder(row);

            return scope.MayReadProtectedPayloads
                ? order with
                {
                    Travellers = await NamedTravellersAsync(order, cancellationToken),
                    Contacts = await ContactsAsync(orderId, cancellationToken)
                }
                : order;
        }

        private static OrderDto PublicOrder(OrderDetailsReadModel row) => row.ProjectionSchemaVersion switch
        {
            OrderProjectionJson.SchemaVersion => OrderProjectionMapper.ToPublicOrder(OrderProjectionJson.Read(row.DetailsJson)),
            OrderDtoJson.SchemaVersion => OrderDtoJson.Read(row.DetailsJson),
            _ => throw ExceptionFactory.UnsupportedCapability($"order projection schema {row.ProjectionSchemaVersion} cannot be read")
        };

        private async Task<IReadOnlyList<OrderTravellerDto>> NamedTravellersAsync(OrderDto order, CancellationToken cancellationToken)
        {
            var identities = await _dbContext.Set<OrderTravelerIdentityReadModel>()
                .AsNoTracking()
                .Where(identity => order.Travellers.Select(traveller => traveller.TravellerId).Contains(identity.TravelerId))
                .ToDictionaryAsync(identity => identity.TravelerId, cancellationToken);

            return order.Travellers
                .Select(traveller => identities.TryGetValue(traveller.TravellerId, out var identity)
                    ? traveller with { FirstName = identity.GivenName, SurName = identity.Surname, DateOfBirth = identity.DateOfBirth }
                    : traveller)
                .ToList();
        }

        private async Task<IReadOnlyList<OrderContactDto>> ContactsAsync(long orderId, CancellationToken cancellationToken)
            => await _dbContext.Set<OrderContactReadModel>()
                .AsNoTracking()
                .Where(contact => contact.OrderId == orderId)
                .OrderBy(contact => contact.Sequence)
                .Select(contact => new OrderContactDto(contact.Role, contact.Email, contact.Phone))
                .ToListAsync(cancellationToken);
    }
}
