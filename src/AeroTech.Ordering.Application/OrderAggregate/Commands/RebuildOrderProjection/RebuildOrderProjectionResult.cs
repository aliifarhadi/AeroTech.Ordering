using System.Text.Json;
using AeroTech.Ordering.Domain._Shared.Serialization;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.RebuildOrderProjection
{
    public sealed record RebuildOrderProjectionResult(
        long OrderId,
        long OperationId,
        long OrderRevision,
        int CommercialVersion,
        bool ReplayedFromReceipt)
    {
        public string ToReceiptJson() => CanonicalJson.Write(new Dictionary<string, object?>
        {
            ["orderId"] = CanonicalJson.Identifier(OrderId),
            ["orderRevision"] = OrderRevision,
            ["commercialVersion"] = CommercialVersion
        });

        public static RebuildOrderProjectionResult FromReceiptJson(string json, long operationId)
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            return new RebuildOrderProjectionResult(
                long.Parse(root.GetProperty("orderId").GetString()!, System.Globalization.CultureInfo.InvariantCulture),
                operationId,
                root.GetProperty("orderRevision").GetInt64(),
                root.GetProperty("commercialVersion").GetInt32(),
                true);
        }
    }
}
