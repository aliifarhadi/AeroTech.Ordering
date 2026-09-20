using AeroTech.Ordering.Query.OrderAggregate.Dto;

namespace AeroTech.Ordering.Query.OrderAggregate.Projection
{
    public static class OrderProjectionMapper
    {
        public static OrderDto ToPublicOrder(OrderProjectionDocument document) => new(
            document.OrderId,
            document.OrderReference,
            document.Status,
            document.CommercialVersion,
            document.SourceOfferId,
            document.Channel,
            document.FinancialCustomerId,
            document.SalesContext.SellingOfficeId,
            document.SalesContext.SellingOfficeKind,
            document.JourneyType,
            document.LastTicketingDate,
            Money(document.CustomerTotal),
            document.SaleCurrencyCode,
            document.Travellers.Select(Traveller).ToList(),
            [],
            document.Segments.Select(Segment).ToList(),
            document.Items.Select(Item).ToList(),
            document.Pricing.Select(Pricing).ToList(),
            document.CreationDate);

        private static MoneyDto Money(ProjectedMoney money) => new(money.Amount, money.CurrencyId);

        private static OrderTravellerDto Traveller(ProjectedTraveller traveller) => new(
            traveller.TravellerId,
            traveller.TravellerRef,
            traveller.SourceTravellerRef,
            traveller.PassengerType,
            traveller.InfantParentTravellerId,
            null,
            null,
            null);

        private static OrderSegmentDto Segment(ProjectedSegment segment) => new(
            segment.SegmentId,
            segment.Sequence,
            segment.OriginAirportId,
            segment.DestinationAirportId,
            segment.SoldDeparture,
            segment.SoldArrival,
            segment.FlightId,
            segment.FlightNumber,
            segment.MarketingAirlineId,
            segment.OperatingAirlineId,
            segment.Legs.Select(leg => new OrderSegmentLegDto(leg.Sequence, leg.LegId)).ToList());

        private static OrderItemDto Item(ProjectedItem item) => new(
            item.ItemId,
            item.Kind,
            item.Status,
            Money(item.AcceptedTotal),
            item.Services.Select(Service).ToList());

        private static OrderServiceDto Service(ProjectedService service) => new(
            service.ServiceId,
            service.ServiceType,
            service.Status,
            service.TravellerId,
            service.SegmentId,
            service.CabinClassId,
            service.RbdId,
            service.BookingClass);

        private static OrderPriceLineDto Pricing(ProjectedPricingLine line) => new(
            line.LineId,
            line.ItemId,
            line.Component,
            line.Effect,
            line.Direction,
            Money(line.SaleValue),
            Money(line.OriginalValue));
    }
}
