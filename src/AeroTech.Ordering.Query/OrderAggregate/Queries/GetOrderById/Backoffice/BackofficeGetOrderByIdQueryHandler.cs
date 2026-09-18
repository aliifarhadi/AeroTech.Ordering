using AeroTech.Ordering.Application._Shared.Authorization;
using AeroTech.Ordering.Query.OrderAggregate.Dto;
using MediatR;

namespace AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrderById.Backoffice
{
    public sealed class BackofficeGetOrderByIdQueryHandler : IRequestHandler<BackofficeGetOrderByIdQuery, OrderDto>
    {
        private readonly OrderDtoReader _reader;
        private readonly AuthorizedScopeResolver _scopeResolver;

        public BackofficeGetOrderByIdQueryHandler(OrderDtoReader reader, AuthorizedScopeResolver scopeResolver)
        {
            _reader = reader;
            _scopeResolver = scopeResolver;
        }

        public async Task<OrderDto> Handle(BackofficeGetOrderByIdQuery query, CancellationToken cancellationToken)
            => await _reader.ReadAsync(query.OrderId, await _scopeResolver.BackofficeReadAsync(cancellationToken), cancellationToken);
    }
}
