using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderAggregate;
using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Query.OrderAggregate.Projection;

namespace AeroTech.Ordering.Synchronizer.OrderAggregate
{
    public static class OrderProjectionBuilder
    {
        public static OrderProjectionDocument Build(Order order) => new(
            OrderProjectionJson.SchemaVersion,
            order.Id,
            order.OrderReference,
            order.CommercialSummary,
            order.CommercialVersion,
            order.SourceOfferId,
            order.Channel,
            order.FinancialCustomerId,
            new ProjectedSalesContext(
                order.SalesContext.SellerContextType,
                order.SalesContext.SellerId,
                order.SalesContext.SellingOfficeKind,
                order.SalesContext.SellingOfficeId),
            new ProjectedActor(order.InitiatingActor.ContextType, order.InitiatingActor.ActorId),
            order.CurrencyId,
            Money(order.CustomerTotal),
            order.JourneyType,
            order.LastTicketingDate,
            Travellers(order),
            Journeys(order),
            Segments(order),
            Items(order),
            Pricing(order),
            ComponentTotals(order),
            FareConstructions(order),
            order.CreatedAt);

        private static IReadOnlyList<ProjectedTraveller> Travellers(Order order)
            => order.Travellers
                .OrderBy(traveller => traveller.SourceTravellerRef, StringComparer.Ordinal)
                .Select(traveller => new ProjectedTraveller(
                    traveller.Id,
                    traveller.ClientTravellerRef,
                    traveller.SourceTravellerRef,
                    traveller.PassengerTypeCode,
                    traveller.InfantParentTravellerId))
                .ToList();

        private static IReadOnlyList<ProjectedJourney> Journeys(Order order)
            => order.Journeys
                .OrderBy(journey => journey.Sequence)
                .Select(journey => new ProjectedJourney(
                    journey.Id,
                    journey.BoundId,
                    journey.Sequence,
                    journey.Direction,
                    journey.OriginAirportId,
                    journey.DestinationAirportId))
                .ToList();

        private static IReadOnlyList<ProjectedSegment> Segments(Order order)
            => order.Segments
                .OrderBy(segment => segment.JourneyId)
                .ThenBy(segment => segment.Sequence)
                .Select(segment => new ProjectedSegment(
                    segment.Id,
                    segment.JourneyId,
                    segment.Sequence,
                    segment.Kind,
                    segment.OriginAirportId,
                    segment.OriginAirportTerminalId,
                    segment.DestinationAirportId,
                    segment.DestinationAirportTerminalId,
                    segment.SoldDeparture,
                    segment.SoldArrival,
                    segment.FlightId,
                    segment.FlightNumber,
                    segment.FlightVersion,
                    segment.MarketingAirlineId,
                    segment.OperatingAirlineId,
                    segment.FlightCapacityId,
                    segment.Duration,
                    segment.AircraftId,
                    segment.Legs
                        .OrderBy(leg => leg.Sequence)
                        .Select(leg => new ProjectedLeg(
                            leg.Id,
                            leg.LegId,
                            leg.Sequence,
                            leg.OriginAirportId,
                            leg.OriginAirportTerminalId,
                            leg.DestinationAirportId,
                            leg.DestinationAirportTerminalId,
                            leg.DepartureDateTime,
                            leg.ArrivalDateTime))
                        .ToList()))
                .ToList();

        private static IReadOnlyList<ProjectedItem> Items(Order order)
            => order.Items
                .OrderBy(item => item.Id)
                .Select(item => new ProjectedItem(
                    item.Id,
                    item.Kind,
                    item.CommercialStatus,
                    Money(item.AcceptedTotal),
                    order.Services
                        .Where(service => service.OrderItemId == item.Id)
                        .OrderBy(service => service.Id)
                        .Select(Service)
                        .ToList()))
                .ToList();

        private static ProjectedService Service(OrderService service) => new(
            service.Id,
            service.CommercialStatus,
            service.TravellerId,
            service.SegmentId,
            service.CabinClassId,
            service.RbdId,
            service.BookingClass,
            Baggage(service.CheckedBaggage),
            Baggage(service.CabinBaggage),
            service.SoldTerms.Refundable,
            service.SoldTerms.Changeable,
            service.SoldTerms.Upgradable);

        private static ProjectedBaggage? Baggage(BaggageAllowance? allowance) => allowance is null
            ? null
            : new ProjectedBaggage(
                allowance.Pieces,
                allowance.Weight is null ? null : DecimalRepresentation.Text(allowance.Weight.Value),
                allowance.WeightUnit);

        private static IReadOnlyList<ProjectedPricingLine> Pricing(Order order)
            => order.PricingLines
                .OrderBy(line => line.Id)
                .Select(line => new ProjectedPricingLine(
                    line.Id,
                    line.OrderItemId,
                    line.SourceOccurrencePath,
                    line.Component,
                    line.Effect,
                    line.Direction,
                    line.Code,
                    line.Name,
                    line.Reference,
                    line.CalculationKind,
                    Money(line.SaleValue),
                    Money(line.OriginalValue),
                    line.BasisType,
                    line.BasisId,
                    line.SourceConversionRef,
                    line.AppliedConversion is { } conversion
                        ? new ProjectedConversion(
                            conversion.SourceConversionRef,
                            conversion.FromCurrencyId,
                            conversion.ToCurrencyId,
                            DecimalRepresentation.Text(conversion.Rate),
                            conversion.DecimalPlaces,
                            conversion.RoundingToken)
                        : null,
                    line.SettlementAttribution is { } attribution
                        ? new ProjectedSettlementAttribution(attribution.PartyRef, attribution.CategoryCode)
                        : null))
                .ToList();

        private static IReadOnlyList<ProjectedComponentTotal> ComponentTotals(Order order)
            => order.PricingLines
                .GroupBy(line => (line.Component, line.Effect))
                .OrderBy(group => group.Key.Component)
                .ThenBy(group => group.Key.Effect)
                .Select(group => new ProjectedComponentTotal(
                    group.Key.Component,
                    group.Key.Effect,
                    DecimalRepresentation.Text(group.Where(line => line.Direction == OrderPricingLineDirection.Debit).Sum(line => line.SaleValue.Amount)),
                    DecimalRepresentation.Text(group.Where(line => line.Direction == OrderPricingLineDirection.Credit).Sum(line => line.SaleValue.Amount))))
                .ToList();

        private static IReadOnlyList<ProjectedFareConstruction> FareConstructions(Order order)
            => order.FareConstructions
                .OrderBy(construction => construction.Id)
                .Select(construction => new ProjectedFareConstruction(
                    construction.Id,
                    construction.Items.Select(item => item.OrderItemId).OrderBy(id => id).ToList(),
                    construction.PricingUnits
                        .OrderBy(unit => unit.Sequence)
                        .Select(unit => new ProjectedFareUnit(
                            unit.Id,
                            unit.Sequence,
                            unit.Type,
                            unit.CoveredBounds
                                .Select(bound => bound.CoveredBoundOfferId)
                                .OrderBy(value => value, StringComparer.Ordinal)
                                .ToList(),
                            unit.Components
                                .OrderBy(component => component.Sequence)
                                .Select(component => new ProjectedFareComponent(
                                    component.Id,
                                    component.Sequence,
                                    component.AirFareId,
                                    component.FareBasis,
                                    component.FareFamily,
                                    component.FareType,
                                    component.CabinClassId,
                                    component.RbdId,
                                    component.BookingClass,
                                    component.TicketingRestrictionMinutes))
                                .ToList()))
                        .ToList()))
                .ToList();

        private static ProjectedMoney Money(Money money) => new(DecimalRepresentation.Text(money.Amount), money.CurrencyId);
    }
}
