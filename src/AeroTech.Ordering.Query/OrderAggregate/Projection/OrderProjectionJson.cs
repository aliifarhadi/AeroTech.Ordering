using System.Text.Json;
using System.Text.Json.Serialization;

namespace AeroTech.Ordering.Query.OrderAggregate.Projection
{
    public static class OrderProjectionJson
    {
        public const int SchemaVersion = 4;

        public const int LegacyInternalSchemaVersion = 3;

        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() },
            WriteIndented = false
        };

        public static string Write(OrderProjectionDocument document) => JsonSerializer.Serialize(document, Options);

        public static OrderProjectionDocument Read(string json) => JsonSerializer.Deserialize<OrderProjectionDocument>(json, Options)!;
    }
}
