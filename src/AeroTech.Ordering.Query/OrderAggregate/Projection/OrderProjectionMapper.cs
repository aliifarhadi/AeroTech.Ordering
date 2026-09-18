using System.Globalization;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Query.OrderAggregate.Dto;

namespace AeroTech.Ordering.Query.OrderAggregate.Projection
{
    public static class OrderProjectionMapper
    {
        public static OrderDto ToPublicOrder(OrderProjectionDocument document)
        {
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
                new MoneyDto(document.GrandTotal.Amount, document.GrandTotal.CurrencyRef),
                document.Travellers
                    .Select(traveller => new OrderTravellerDto(
                        traveller.TravellerId,
                        traveller.TravellerRef,
                        traveller.OfferTravellerRef,
                        traveller.PassengerType,
                        traveller.GuardianTravellerId,
                        null,
                        null,
                        null))
                    .ToList(),
                [],
                document.Segments.Select(Segment).ToList(),
                document.Items.Select(item => Item(item, segments)).ToList(),
                document.Pricing.Select(Pricing).ToList(),
                document.CreationDate);
        }

        private static OrderSegmentDto Segment(ProjectedSegment segment) => new(
            segment.SegmentId,
            segment.Sequence,
            segment.OriginRef,
            segment.DestinationRef,
            segment.Departure,
            segment.Arrival,
            segment.FlightRef,
            segment.Legs.Select(leg => new OrderSegmentLegDto(leg.Sequence, leg.SourceLegRef)).ToList());

        private static OrderItemDto Item(ProjectedItem item, IReadOnlyDictionary<long, ProjectedSegment> segments) => new(
            item.ItemId,
            item.Kind,
            item.Status,
            new MoneyDto(item.AcceptedTotal.Amount, item.AcceptedTotal.CurrencyRef),
            item.Services.Select(service => Service(service, segments)).ToList());

        private static OrderServiceDto Service(ProjectedService service, IReadOnlyDictionary<long, ProjectedSegment> segments) => new(
            service.ServiceId,
            service.ServiceType,
            service.Status,
            decimal.Parse(service.Quantity, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
            service.QuantityUnit,
            service.TravellerIds,
            service.SegmentIds,
            AirTransport(service, segments));

        private static OrderAirTransportDto? AirTransport(ProjectedService service, IReadOnlyDictionary<long, ProjectedSegment> segments)
        {
            if (service.AirTransport is not { } detail)
                return null;

            var segment = CoveredSegment(service, segments);

            return new OrderAirTransportDto(
                detail.CabinRef,
                detail.RbdRef,
                detail.BookingClass,
                segment.FlightNumber,
                segment.MarketingCarrierRef,
                segment.OperatingCarrierRef);
        }

        private static ProjectedSegment CoveredSegment(ProjectedService service, IReadOnlyDictionary<long, ProjectedSegment> segments)
        {
            if (service.SegmentIds.Count != 1)
                throw ExceptionFactory.ServiceCoverageIsNotASingleSegment(service.ServiceId, service.SegmentIds.Count);

            return segments.TryGetValue(service.SegmentIds[0], out var segment)
                ? segment
                : throw ExceptionFactory.ServiceCoverageIsNotASingleSegment(service.ServiceId, 0);
        }

        private static OrderPriceLineDto Pricing(ProjectedPricingLine line) => new(
            line.LineId,
            line.ItemId,
            line.Component,
            line.Effect,
            line.Direction,
            new MoneyDto(line.SaleValue.Amount, line.SaleValue.CurrencyRef),
            new MoneyDto(line.OriginalValue.Amount, line.OriginalValue.CurrencyRef));
    }
}
