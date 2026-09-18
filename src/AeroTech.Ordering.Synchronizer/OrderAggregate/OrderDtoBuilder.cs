using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderAggregate;
using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Query.OrderAggregate.Dto;

namespace AeroTech.Ordering.Synchronizer.OrderAggregate
{
    public static class OrderDtoBuilder
    {
        public static OrderDto Build(Order order) => new(
            order.Id,
            order.OrderReference,
            order.CommercialSummary,
            order.CommercialVersion,
            order.AcceptedSource.SourceOfferId,
            order.Channel,
            order.FinancialCustomerId,
            order.SellingOfficeId,
            Money(order.CustomerTotal),
            Travellers(order),
            [],
            Itinerary(order),
            Items(order),
            Pricing(order),
            order.CreatedAt);

        private static IReadOnlyList<OrderTravellerDto> Travellers(Order order)
            => order.Travelers
                .OrderBy(traveler => traveler.SourceTravellerRef, StringComparer.Ordinal)
                .Select(traveler => new OrderTravellerDto(
                    traveler.Id,
                    traveler.ClientTravelerRef,
                    traveler.SourceTravellerRef,
                    traveler.PassengerTypeCode,
                    traveler.InfantParentTravelerId,
                    null,
                    null,
                    null))
                .ToList();

        private static IReadOnlyList<OrderSegmentDto> Itinerary(Order order)
            => order.Segments
                .OrderBy(segment => segment.Sequence)
                .Select(segment => new OrderSegmentDto(
                    segment.Id,
                    segment.Sequence,
                    segment.OriginRef,
                    segment.DestinationRef,
                    segment.SoldDeparture,
                    segment.SoldArrival,
                    segment.FlightRef,
                    segment.Legs
                        .OrderBy(leg => leg.Sequence)
                        .Select(leg => new OrderSegmentLegDto(leg.Sequence, leg.SourceLegRef))
                        .ToList()))
                .ToList();

        private static IReadOnlyList<OrderItemDto> Items(Order order)
            => order.Items
                .OrderBy(item => item.Id)
                .Select(item => new OrderItemDto(
                    item.Id,
                    item.Kind,
                    item.CommercialStatus,
                    Money(item.AcceptedTotal),
                    Services(order, item.Id)))
                .ToList();

        private static IReadOnlyList<OrderServiceDto> Services(Order order, long itemId)
            => order.Services
                .Where(service => service.OrderItemId == itemId)
                .OrderBy(service => service.Id)
                .Select(service => new OrderServiceDto(
                    service.Id,
                    service.Type,
                    service.CommercialStatus,
                    DecimalRepresentation.Normalize(service.Quantity),
                    service.QuantityUnit,
                    service.Beneficiaries.Select(beneficiary => beneficiary.TravelerId).OrderBy(id => id).ToList(),
                    service.Coverage.Select(coverage => coverage.SegmentId).OrderBy(id => id).ToList(),
                    AirTransport(service)))
                .ToList();

        private static OrderAirTransportDto? AirTransport(OrderService service)
            => service.AirTransport is { } detail
                ? new OrderAirTransportDto(
                    detail.CabinRef,
                    detail.RbdRef,
                    detail.BookingClass,
                    detail.FlightNumber,
                    detail.MarketingCarrierRef,
                    detail.OperatingCarrierRef)
                : null;

        private static IReadOnlyList<OrderPriceLineDto> Pricing(Order order)
            => order.PricingLines
                .OrderBy(line => line.Id)
                .Select(line => new OrderPriceLineDto(
                    line.Id,
                    line.OrderItemId,
                    line.Component,
                    line.Effect,
                    line.Direction,
                    Money(line.SaleValue),
                    Money(line.OriginalValue)))
                .ToList();

        private static MoneyDto Money(Money money)
            => new(DecimalRepresentation.Text(money.Amount), money.CurrencyRef);
    }
}
