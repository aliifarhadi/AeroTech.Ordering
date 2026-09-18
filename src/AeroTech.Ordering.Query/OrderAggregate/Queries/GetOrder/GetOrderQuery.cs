using AeroTech.Messages.Ordering.Enums;
using MediatR;

namespace AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrder
{
    public sealed record GetOrderQuery(
        long OrderId,
        OrderingApiSurface Surface,
        long? MinRevision) : IRequest<OrderDetailsView>;
}
