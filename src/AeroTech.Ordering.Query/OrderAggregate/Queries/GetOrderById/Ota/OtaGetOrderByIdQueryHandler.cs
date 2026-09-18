using AeroTech.Ordering.Application._Shared.Authorization;
using AeroTech.Ordering.Query.OrderAggregate.Dto;
using MediatR;

namespace AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrderById.Ota
{
    public sealed class OtaGetOrderByIdQueryHandler : IRequestHandler<OtaGetOrderByIdQuery, OrderDto>
    {
        private readonly OrderDtoReader _reader;
        private readonly AuthorizedScopeResolver _scopeResolver;

        public OtaGetOrderByIdQueryHandler(OrderDtoReader reader, AuthorizedScopeResolver scopeResolver)
        {
            _reader = reader;
            _scopeResolver = scopeResolver;
        }

        public async Task<OrderDto> Handle(OtaGetOrderByIdQuery query, CancellationToken cancellationToken)
            => await _reader.ReadAsync(query.OrderId, await _scopeResolver.OtaReadAsync(cancellationToken), cancellationToken);
    }
}
