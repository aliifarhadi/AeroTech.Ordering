using System.Text.Json;
using AeroTech.Ordering.Application._Shared.Authorization;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Query._Shared.DbContexts;
using AeroTech.Ordering.Query.OrderAggregate.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrder
{
    public sealed class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDetailsView>
    {
        private readonly OrderQueryDbContext _dbContext;
        private readonly AuthorizedScopeResolver _scopeResolver;

        public GetOrderQueryHandler(OrderQueryDbContext dbContext, AuthorizedScopeResolver scopeResolver)
        {
            _dbContext = dbContext;
            _scopeResolver = scopeResolver;
        }

        public async Task<OrderDetailsView> Handle(GetOrderQuery query, CancellationToken cancellationToken)
        {
            var scope = await _scopeResolver.ResolveReadScopeAsync(query.Surface, cancellationToken);

            var details = await _dbContext.Set<OrderDetailsReadModel>()
                .AsNoTracking()
                .Where(row => row.OrderId == query.OrderId && row.OwnerAirlineId == scope.OwnerAirlineId)
                .Where(row => scope.RestrictToFinancialCustomerId == null || row.FinancialCustomerId == scope.RestrictToFinancialCustomerId)
                .SingleOrDefaultAsync(cancellationToken)
                ?? throw ExceptionFactory.OrderNotFound(query.OrderId);

            if (query.MinRevision is { } minRevision && details.OrderRevision < minRevision)
                throw ExceptionFactory.ProjectionRevisionLagging(query.OrderId, details.OrderRevision, minRevision);

            using var document = JsonDocument.Parse(details.DetailsJson);

            return new OrderDetailsView(
                details.OrderId,
                details.OrderRevision,
                details.ProjectionSchemaVersion,
                details.ProjectedAt,
                document.RootElement.Clone(),
                scope.MayReadProtectedPayloads ? await TravelersAsync(query.OrderId, cancellationToken) : [],
                scope.MayReadProtectedPayloads ? await ContactsAsync(query.OrderId, cancellationToken) : []);
        }

        private async Task<IReadOnlyList<ProtectedTravelerView>> TravelersAsync(long orderId, CancellationToken cancellationToken)
            => await (from traveler in _dbContext.Set<OrderTravelerReadModel>().AsNoTracking()
                      join identity in _dbContext.Set<OrderTravelerIdentityReadModel>().AsNoTracking()
                          on traveler.Id equals identity.TravelerId
                      where traveler.OrderId == orderId
                      orderby traveler.SourceTravellerRef
                      select new ProtectedTravelerView(
                          traveler.Id,
                          traveler.SourceTravellerRef,
                          traveler.ClientTravelerRef,
                          traveler.PassengerTypeCode,
                          identity.GivenName,
                          identity.Surname,
                          identity.DateOfBirth,
                          traveler.InfantParentTravelerId))
                .ToListAsync(cancellationToken);

        private async Task<IReadOnlyList<ProtectedContactView>> ContactsAsync(long orderId, CancellationToken cancellationToken)
            => await _dbContext.Set<OrderContactReadModel>()
                .AsNoTracking()
                .Where(contact => contact.OrderId == orderId)
                .OrderBy(contact => contact.Sequence)
                .Select(contact => new ProtectedContactView(contact.Id, contact.Sequence, contact.Role, contact.Email, contact.Phone))
                .ToListAsync(cancellationToken);
    }
}
