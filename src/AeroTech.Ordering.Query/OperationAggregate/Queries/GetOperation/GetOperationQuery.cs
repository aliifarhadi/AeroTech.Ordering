using AeroTech.Messages.Ordering.Enums;
using MediatR;

namespace AeroTech.Ordering.Query.OperationAggregate.Queries.GetOperation
{
    public sealed record GetOperationQuery(
        long OperationId,
        OrderingApiSurface Surface) : IRequest<OperationDetailsView>;
}
