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
            order.AcceptedSource.SourceOfferId,
            order.Channel,
            order.FinancialCustomerId,
            order.SellingOfficeId,
            Money(order.CustomerTotal),
            order.SaleCurrency.CurrencyRef,
            order.SaleCurrency.CurrencyCode,
            order.SourceJourneyTypeRaw,
            order.JourneyType,
            order.ObservedTicketingDeadline is { } observed
                ? new ProjectedObservedTime(observed.Value, observed.SourceOwner, observed.SourceRef)
                : null,
            Travellers(order),
            Journeys(order),
            Segments(order),
            Items(order),
            Pricing(order),
            FareConstructions(order),
            ItemServiceLinks(order),
            order.CreatedAt);

        private static IReadOnlyList<ProjectedTraveller> Travellers(Order order)
            => order.Travelers
                .OrderBy(traveler => traveler.SourceTravellerRef, StringComparer.Ordinal)
                .Select(traveler => new ProjectedTraveller(
                    traveler.Id,
                    traveler.ClientTravelerRef,
                    traveler.SourceTravellerRef,
                    traveler.PassengerTypeCode,
                    traveler.InfantParentTravelerId))
                .ToList();

        private static IReadOnlyList<ProjectedJourney> Journeys(Order order)
            => order.Journeys
                .OrderBy(journey => journey.Sequence)
                .Select(journey => new ProjectedJourney(
                    journey.Id,
                    journey.SourceBoundRef,
                    journey.Sequence,
                    journey.SourceDirectionRaw,
                    journey.Direction,
                    journey.OriginRef,
                    journey.DestinationRef))
                .ToList();

        private static IReadOnlyList<ProjectedSegment> Segments(Order order)
            => order.Segments
                .OrderBy(segment => segment.Sequence)
                .Select(segment => new ProjectedSegment(
                    segment.Id,
                    segment.JourneyId,
                    segment.Sequence,
                    segment.SourceSegmentRef,
                    segment.Kind,
                    segment.OriginRef,
                    segment.OriginTerminalRef,
                    segment.DestinationRef,
                    segment.DestinationTerminalRef,
                    segment.SoldDeparture,
                    segment.SoldArrival,
                    segment.FlightRef,
                    segment.FlightNumber,
                    segment.FlightVersion,
                    segment.MarketingCarrierRef,
                    segment.OperatingCarrierRef,
                    segment.SourceCapacityRef,
                    segment.Duration,
                    segment.AircraftRef,
                    segment.Legs
                        .OrderBy(leg => leg.Sequence)
                        .Select(leg => new ProjectedLeg(
                            leg.Id,
                            leg.Sequence,
                            leg.SourceLegRef,
                            leg.OriginRef,
                            leg.OriginTerminalRef,
                            leg.DestinationRef,
                            leg.DestinationTerminalRef,
                            leg.Departure,
                            leg.Arrival))
                        .ToList()))
                .ToList();

        private static IReadOnlyList<ProjectedItem> Items(Order order)
            => order.Items
                .OrderBy(item => item.Id)
                .Select(item => new ProjectedItem(
                    item.Id,
                    item.SourceItemRef,
                    item.Kind,
                    item.CommercialStatus,
                    Money(item.AcceptedTotal),
                    new ProjectedProduct(
                        item.Product.SourceSystem,
                        item.Product.SourceOfferId,
                        item.Product.SourceOfferItemRef,
                        item.Product.ProductCode,
                        item.Product.ProductName,
                        item.Product.BrandCode,
                        item.Product.BrandName,
                        item.Product.ProductVersion),
                    new ProjectedCommercialTerms(
                        item.CommercialTerms.Refundability,
                        item.CommercialTerms.Changeability,
                        item.CommercialTerms.UpgradeEligibility,
                        item.CommercialTerms.SourceSystem,
                        item.CommercialTerms.SourcePolicyRef,
                        item.CommercialTerms.SourcePolicyVersion,
                        item.CommercialTerms.TermsCapturedAt),
                    Services(order, item.Id)))
                .ToList();

        private static IReadOnlyList<ProjectedService> Services(Order order, long itemId)
            => order.Services
                .Where(service => service.OrderItemId == itemId)
                .OrderBy(service => service.Id)
                .Select(service => new ProjectedService(
                    service.Id,
                    service.SourceServiceRef,
                    service.Type,
                    service.CommercialStatus,
                    service.ServiceCode,
                    service.Name,
                    service.PriceTreatment,
                    service.SupplierPartyRef,
                    service.DeliveryProviderRef,
                    DecimalRepresentation.Text(service.Quantity),
                    service.QuantityUnit,
                    service.Beneficiaries.Select(beneficiary => beneficiary.TravelerId).OrderBy(id => id).ToList(),
                    service.Coverage.Select(coverage => coverage.SegmentId).OrderBy(id => id).ToList(),
                    new ProjectedSoldTerms(service.SoldTerms.Refundable, service.SoldTerms.Changeable, service.SoldTerms.Upgradable),
                    new ProjectedFulfillmentProfile(
                        service.FulfillmentProfile.ProfileRef,
                        service.FulfillmentProfile.ProfileVersion,
                        service.FulfillmentProfile.Assurance,
                        service.FulfillmentProfile.ReservationRequirement,
                        service.FulfillmentProfile.DocumentKind,
                        service.FulfillmentProfile.FundingRequirement,
                        service.FulfillmentProfile.CapacityUnits),
                    AirTransport(service)))
                .ToList();

        private static ProjectedAirTransport? AirTransport(OrderService service)
            => service.AirTransport is { } detail
                ? new ProjectedAirTransport(
                    detail.CabinRef,
                    detail.RbdRef,
                    detail.BookingClass,
                    Baggage(detail.CheckedBaggage),
                    Baggage(detail.CabinBaggage))
                : null;

        private static ProjectedBaggage? Baggage(BaggageAllowance? allowance)
            => allowance is null
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
                    line.SourceLineRef,
                    line.Component,
                    line.Effect,
                    line.Direction,
                    line.Role,
                    line.SourceCode,
                    line.SourceName,
                    line.SourceReference,
                    line.CalculationKind,
                    Money(line.SaleValue),
                    Money(line.OriginalValue),
                    line.BasisType,
                    line.BasisId,
                    line.SourceBasisRef,
                    line.SourceConversionRef,
                    line.AppliedConversion is { } conversion
                        ? new ProjectedConversion(
                            conversion.SourceConversionRef,
                            conversion.FromCurrencyRef,
                            conversion.ToCurrencyRef,
                            DecimalRepresentation.Text(conversion.Rate),
                            conversion.DecimalPlaces,
                            conversion.RoundingToken)
                        : null))
                .ToList();

        private static IReadOnlyList<ProjectedFareConstruction> FareConstructions(Order order)
            => order.FareConstructions
                .OrderBy(construction => construction.Id)
                .Select(construction => new ProjectedFareConstruction(
                    construction.Id,
                    construction.Assurance,
                    construction.SourceContextRef,
                    construction.Items.Select(item => item.OrderItemId).OrderBy(id => id).ToList(),
                    construction.PricingGroups
                        .OrderBy(group => group.Id)
                        .Select(group => new ProjectedFareGroup(
                            group.Id,
                            group.PassengerTypeCode,
                            group.Quantity,
                            group.Travelers.Select(traveler => traveler.TravelerId).OrderBy(id => id).ToList()))
                        .ToList(),
                    construction.PricingUnits
                        .OrderBy(unit => unit.Sequence)
                        .Select(unit => new ProjectedFareUnit(
                            unit.Id,
                            unit.PricingGroupId,
                            unit.Sequence,
                            unit.SourceUnitRef,
                            unit.SourceKindRaw,
                            unit.Type,
                            unit.CombinationMethod,
                            unit.CoveredBounds.Select(bound => bound.SourceBoundRef).OrderBy(reference => reference, StringComparer.Ordinal).ToList(),
                            unit.Components
                                .OrderBy(component => component.Sequence)
                                .Select(Component)
                                .ToList()))
                        .ToList()))
                .ToList();

        private static ProjectedFareComponent Component(FareComponent component) => new(
            component.Id,
            component.Sequence,
            component.SourceFareRef,
            component.FareBasis,
            component.FareFamily,
            component.FareType,
            component.CabinRef,
            component.RbdRef,
            component.BookingClass,
            component.TicketingRestrictionMinutes,
            component.FareOwnerRef,
            component.TariffRef,
            component.RuleRef,
            component.RoutingRef,
            component.CoveredServices.Select(covered => covered.OrderServiceId).OrderBy(id => id).ToList(),
            component.CoveredSegments.Select(covered => covered.OrderSegmentId).OrderBy(id => id).ToList());

        private static IReadOnlyList<ProjectedItemServiceLink> ItemServiceLinks(Order order)
            => order.ItemServiceLinks
                .OrderBy(link => link.Id)
                .Select(link => new ProjectedItemServiceLink(
                    link.Id,
                    link.OrderItemId,
                    link.OrderServiceId,
                    link.LinkedByChangeId,
                    link.TravelersAtAssociation.Select(scope => scope.TravelerId).OrderBy(id => id).ToList(),
                    link.SegmentsAtAssociation.Select(scope => scope.SegmentId).OrderBy(id => id).ToList()))
                .ToList();

        private static ProjectedMoney Money(Money money)
            => new(DecimalRepresentation.Text(money.Amount), money.CurrencyRef);
    }
}
