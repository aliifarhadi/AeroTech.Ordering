using AeroTech.Messages.Ordering.Enums;
using AeroTech.Messages.Shared.Enums;

namespace AeroTech.Ordering.Query.OrderAggregate.Dto
{
    public sealed record OrderDto(
        long OrderId,
        string OrderReference,
        CommercialSummary Status,
        int CommercialVersion,
        string SourceOfferId,
        SalesChannel Channel,
        long FinancialCustomerId,
        long? SellingOfficeId,
        SellingOfficeKind? SellingOfficeKind,
        JourneyType JourneyType,
        DateTimeOffset? LastTicketingDate,
        MoneyDto CustomerTotal,
        string? SaleCurrencyCode,
        IReadOnlyList<OrderTravellerDto> Travellers,
        IReadOnlyList<OrderContactDto> Contacts,
        IReadOnlyList<OrderSegmentDto> Itinerary,
        IReadOnlyList<OrderItemDto> Items,
        IReadOnlyList<OrderPriceLineDto> Pricing,
        DateTimeOffset CreationDate);

    public sealed record MoneyDto(string Amount, int CurrencyId);

    public sealed record OrderTravellerDto(
        long TravellerId,
        string TravellerRef,
        string SourceTravellerRef,
        PassengerTypeCode PassengerType,
        long? InfantParentTravellerId,
        string? FirstName,
        string? SurName,
        DateOnly? DateOfBirth);

    public sealed record OrderSegmentDto(
        long SegmentId,
        int Sequence,
        int OriginAirportId,
        int DestinationAirportId,
        DateTimeOffset? Departure,
        DateTimeOffset? Arrival,
        long? FlightId,
        string? FlightNumber,
        int? MarketingAirlineId,
        int? OperatingAirlineId,
        IReadOnlyList<OrderSegmentLegDto> Legs);

    public sealed record OrderSegmentLegDto(int Sequence, long LegId);

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
        long TravellerId,
        long SegmentId,
        int? CabinClassId,
        long? RbdId,
        string? BookingClass);

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
