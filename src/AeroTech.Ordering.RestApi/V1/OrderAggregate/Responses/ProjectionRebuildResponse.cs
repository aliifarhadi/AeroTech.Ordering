using AeroTech.Ordering.Application.OrderAggregate.Commands.RebuildOrderProjection;

namespace AeroTech.Ordering.RestApi.V1.OrderAggregate.Responses
{
    public sealed record ProjectionRebuildResponse(
        long OrderId,
        long OperationId,
        long OrderRevision,
        int CommercialVersion)
    {
        public static ProjectionRebuildResponse From(RebuildOrderProjectionResult result)
            => new(result.OrderId, result.OperationId, result.OrderRevision, result.CommercialVersion);
    }
}
