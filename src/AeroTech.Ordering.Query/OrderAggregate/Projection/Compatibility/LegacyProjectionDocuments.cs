using AeroTech.Messages.Ordering.Enums;
using AeroTech.Messages.Shared.Enums;
using AeroTech.Ordering.Query.OrderAggregate.Dto;

namespace AeroTech.Ordering.Query.OrderAggregate.Projection.Compatibility
{
    public sealed record OrderProjectionV3Document(
        long OrderId,
        string OrderReference,
        CommercialSummary Status,
        int CommercialVersion,
        string OfferId,
        SalesChannel Channel,
        long CustomerId,
        long? AirlineOfficeId,
        ProjectedMoney GrandTotal,
        IReadOnlyList<ProjectedTraveller> Travellers,
        IReadOnlyList<ProjectedSegment> Segments,
        IReadOnlyList<ProjectedItem> Items,
        IReadOnlyList<ProjectedPricingLine> Pricing,
        DateTimeOffset CreationDate);

    public sealed record OrderDtoV2Document(
        long OrderId,
        string OrderReference,
        CommercialSummary Status,
        int CommercialVersion,
        string OfferId,
        SalesChannel Channel,
        long CustomerId,
        long? AirlineOfficeId,
        MoneyDto GrandTotal,
        IReadOnlyList<OrderTravellerDto> Travellers,
        IReadOnlyList<OrderContactDto> Contacts,
        IReadOnlyList<OrderSegmentDto> Itinerary,
        IReadOnlyList<OrderItemDto> Items,
        IReadOnlyList<OrderPriceLineDto> Pricing,
        DateTimeOffset CreationDate);
}
