using System.Globalization;
using System.Text.Json;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Serialization;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderAggregate;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer
{
    public sealed record CreateOrderFromOfferResult(
        long OrderId,
        string OrderReference,
        CommercialSummary Status,
        string GrandTotal,
        string CurrencyRef)
    {
        public static CreateOrderFromOfferResult From(Order order) => new(
            order.Id,
            order.OrderReference,
            order.CommercialSummary,
            DecimalRepresentation.Text(order.CustomerTotal.Amount),
            order.CustomerTotal.CurrencyRef);

        public string ToReceiptJson() => CanonicalJson.Write(new Dictionary<string, object?>
        {
            ["orderId"] = CanonicalJson.Identifier(OrderId),
            ["orderReference"] = OrderReference,
            ["status"] = Status.ToString(),
            ["grandTotal"] = GrandTotal,
            ["currencyRef"] = CurrencyRef
        });

        public static CreateOrderFromOfferResult FromReceiptJson(string json)
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            return new CreateOrderFromOfferResult(
                long.Parse(root.GetProperty("orderId").GetString()!, CultureInfo.InvariantCulture),
                root.GetProperty("orderReference").GetString()!,
                Enum.Parse<CommercialSummary>(root.GetProperty("status").GetString()!),
                root.GetProperty("grandTotal").GetString()!,
                root.GetProperty("currencyRef").GetString()!);
        }
    }
}
