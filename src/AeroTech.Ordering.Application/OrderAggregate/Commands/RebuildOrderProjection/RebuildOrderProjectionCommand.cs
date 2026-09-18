using AeroTech.Ordering.Application._Shared.Authorization;
using MediatR;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.RebuildOrderProjection
{
    public sealed record RebuildOrderProjectionCommand(
        AdministrativeScopeRequest Scope,
        string IdempotencyKey,
        long OrderId) : IRequest<RebuildOrderProjectionResult>;
}
