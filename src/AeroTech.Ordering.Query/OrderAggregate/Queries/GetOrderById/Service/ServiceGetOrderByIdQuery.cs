using AeroTech.Ordering.Query.OrderAggregate.Dto;
using MediatR;

namespace AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrderById.Service
{
    public sealed record ServiceGetOrderByIdQuery(long OrderId) : IRequest<OrderDto>;
}
