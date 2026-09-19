using System.Text.Json;
using System.Text.Json.Serialization;
using AeroTech.Ordering.Domain._Shared.Policies;
using AeroTech.Ordering.Query.OrderAggregate.Dto;

namespace AeroTech.Ordering.Query.OrderAggregate.Projection.Compatibility
{
    public static class LegacyProjectionReader
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() },
            WriteIndented = false
        };

        public static OrderDto ReadInternalSchemaThree(string json)
        {
            var document = JsonSerializer.Deserialize<OrderProjectionV3Document>(json, Options)!;
            var segments = document.Segments.ToDictionary(segment => segment.SegmentId);

            return new OrderDto(
                document.OrderId,
                document.OrderReference,
                document.Status,
                document.CommercialVersion,
                document.OfferId,
                document.Channel,
                document.CustomerId,
                document.AirlineOfficeId,
                LegacySellingOfficePolicy.KindOf(document.Channel, document.AirlineOfficeId),
                OrderProjectionMapper.Money(document.GrandTotal),
                document.Travellers.Select(OrderProjectionMapper.Traveller).ToList(),
                [],
                document.Segments.Select(OrderProjectionMapper.Segment).ToList(),
                document.Items.Select(item => OrderProjectionMapper.Item(item, segments)).ToList(),
                document.Pricing.Select(OrderProjectionMapper.Pricing).ToList(),
                document.CreationDate);
        }

        public static OrderDto ReadPublicSchemaTwo(string json)
        {
            var document = JsonSerializer.Deserialize<OrderDtoV2Document>(json, Options)!;

            return new OrderDto(
                document.OrderId,
                document.OrderReference,
                document.Status,
                document.CommercialVersion,
                document.OfferId,
                document.Channel,
                document.CustomerId,
                document.AirlineOfficeId,
                LegacySellingOfficePolicy.KindOf(document.Channel, document.AirlineOfficeId),
                document.GrandTotal,
                document.Travellers,
                document.Contacts,
                document.Itinerary,
                document.Items,
                document.Pricing,
                document.CreationDate);
        }
    }
}
