using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer;

namespace AeroTech.Ordering.RestApi.V1.OrderAggregate.Responses
{
    public sealed record CreatedOrderResponse(
        long OrderId,
        long OperationId,
        string OrderReference,
        int CommercialVersionAtCommit,
        long OrderRevisionAtCommit,
        string AcceptedSourceDigest,
        string OrderUrl)
    {
        public static CreatedOrderResponse From(CreatedOrderResult result, string orderUrl) => new(
            result.OrderId,
            result.OperationId,
            result.OrderReference,
            result.CommercialVersionAtCommit,
            result.OrderRevisionAtCommit,
            result.AcceptedSourceDigest,
            orderUrl);
    }
}
