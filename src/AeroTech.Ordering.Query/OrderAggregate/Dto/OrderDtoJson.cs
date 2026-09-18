using System.Text.Json;
using System.Text.Json.Serialization;

namespace AeroTech.Ordering.Query.OrderAggregate.Dto
{
    public static class OrderDtoJson
    {
        public const int SchemaVersion = 2;

        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() },
            WriteIndented = false
        };

        public static string Write(OrderDto order) => JsonSerializer.Serialize(order, Options);

        public static OrderDto Read(string json) => JsonSerializer.Deserialize<OrderDto>(json, Options)!;
    }
}
