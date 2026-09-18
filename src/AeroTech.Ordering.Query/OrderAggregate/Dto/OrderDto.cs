using AeroTech.Messages.Ordering.Enums;
using AeroTech.Messages.Shared.Enums;

namespace AeroTech.Ordering.Query.OrderAggregate.Dto
{
    public sealed record OrderDto(
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

    public sealed record MoneyDto(string Amount, string CurrencyRef);

    public sealed record OrderTravellerDto(
        long TravellerId,
        string TravellerRef,
        string OfferTravellerRef,
        PassengerTypeCode PassengerType,
        long? GuardianTravellerId,
        string? FirstName,
        string? SurName,
        DateOnly? DateOfBirth);

    public sealed record OrderSegmentDto(
        long SegmentId,
        int Sequence,
        string OriginRef,
        string DestinationRef,
        DateTimeOffset? Departure,
        DateTimeOffset? Arrival,
        string? FlightRef,
        IReadOnlyList<OrderSegmentLegDto> Legs);

    public sealed record OrderSegmentLegDto(int Sequence, string LegRef);

    public sealed record OrderItemDto(
        long ItemId,
        OrderItemKind Kind,
        OrderItemCommercialStatus Status,
        MoneyDto AcceptedTotal,
        IReadOnlyList<OrderServiceDto> Services);

    public sealed record OrderServiceDto(
        long ServiceId,
        OrderServiceType ServiceType,
        OrderServiceCommercialStatus Status,
        decimal Quantity,
        OrderItemUnitOfMeasure QuantityUnit,
        IReadOnlyList<long> TravellerIds,
        IReadOnlyList<long> SegmentIds,
        OrderAirTransportDto? AirTransport);

    public sealed record OrderAirTransportDto(
        string? CabinRef,
        string? RbdRef,
        string? BookingClass,
        string? FlightNumber,
        string? MarketingAirlineRef,
        string? OperatingAirlineRef);

    public sealed record OrderPriceLineDto(
        long LineId,
        long? ItemId,
        PricingComponentType Component,
        PricingEffect Effect,
        OrderPricingLineDirection Direction,
        MoneyDto SaleValue,
        MoneyDto OriginalValue);

    public sealed record OrderContactDto(ContactRole Role, string? Email, string? Phone);
}
