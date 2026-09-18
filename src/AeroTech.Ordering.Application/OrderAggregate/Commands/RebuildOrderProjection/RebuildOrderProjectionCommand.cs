using MediatR;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.RebuildOrderProjection
{
    public sealed record RebuildOrderProjectionCommand(
        long OrderId,
        string IdempotencyKey) : IRequest<RebuildOrderProjectionResult>;
}
