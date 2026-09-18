using System.Text.Json;
using AeroTech.Ordering.Domain._Shared.Serialization;
using AeroTech.Ordering.Domain.OrderAggregate;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer
{
    public sealed record CreatedOrderResult(
        long OrderId,
        long OperationId,
        string OrderReference,
        int CommercialVersionAtCommit,
        long OrderRevisionAtCommit,
        string AcceptedSourceDigest,
        bool ReplayedFromReceipt)
    {
        public static CreatedOrderResult From(Order order, long operationId)
            => new(order.Id, operationId, order.OrderReference, order.CommercialVersion, order.OrderRevision, order.AcceptedSource.SnapshotDigest, false);

        public string ToReceiptJson() => CanonicalJson.Write(new Dictionary<string, object?>
        {
            ["orderId"] = CanonicalJson.Identifier(OrderId),
            ["orderReference"] = OrderReference,
            ["commercialVersionAtCommit"] = CommercialVersionAtCommit,
            ["orderRevisionAtCommit"] = OrderRevisionAtCommit,
            ["acceptedSourceDigest"] = AcceptedSourceDigest
        });

        public static CreatedOrderResult FromReceiptJson(string json, long operationId)
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            return new CreatedOrderResult(
                long.Parse(root.GetProperty("orderId").GetString()!, System.Globalization.CultureInfo.InvariantCulture),
                operationId,
                root.GetProperty("orderReference").GetString()!,
                root.GetProperty("commercialVersionAtCommit").GetInt32(),
                root.GetProperty("orderRevisionAtCommit").GetInt64(),
                root.GetProperty("acceptedSourceDigest").GetString()!,
                true);
        }
    }
}
