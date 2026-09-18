using AeroTech.Ordering.Application._Shared.Authorization;
using AeroTech.Ordering.Query.OrderAggregate.Dto;
using MediatR;

namespace AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrderById.OtaPanel
{
    public sealed class OtaPanelGetOrderByIdQueryHandler : IRequestHandler<OtaPanelGetOrderByIdQuery, OrderDto>
    {
        private readonly OrderDtoReader _reader;
        private readonly AuthorizedScopeResolver _scopeResolver;

        public OtaPanelGetOrderByIdQueryHandler(OrderDtoReader reader, AuthorizedScopeResolver scopeResolver)
        {
            _reader = reader;
            _scopeResolver = scopeResolver;
        }

        public async Task<OrderDto> Handle(OtaPanelGetOrderByIdQuery query, CancellationToken cancellationToken)
            => await _reader.ReadAsync(query.OrderId, await _scopeResolver.OtaPanelReadAsync(cancellationToken), cancellationToken);
    }
}
