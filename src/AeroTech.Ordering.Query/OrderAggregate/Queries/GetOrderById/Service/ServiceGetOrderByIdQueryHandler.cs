using AeroTech.Ordering.Application._Shared.Authorization;
using AeroTech.Ordering.Query.OrderAggregate.Dto;
using MediatR;

namespace AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrderById.Service
{
    public sealed class ServiceGetOrderByIdQueryHandler : IRequestHandler<ServiceGetOrderByIdQuery, OrderDto>
    {
        private readonly OrderDtoReader _reader;
        private readonly AuthorizedScopeResolver _scopeResolver;

        public ServiceGetOrderByIdQueryHandler(OrderDtoReader reader, AuthorizedScopeResolver scopeResolver)
        {
            _reader = reader;
            _scopeResolver = scopeResolver;
        }

        public async Task<OrderDto> Handle(ServiceGetOrderByIdQuery query, CancellationToken cancellationToken)
            => await _reader.ReadAsync(query.OrderId, await _scopeResolver.ServiceReadAsync(cancellationToken), cancellationToken);
    }
}
