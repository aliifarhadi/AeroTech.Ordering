using System.Net.Http;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderAggregate;
using AeroTech.Ordering.Query._Shared.DbContexts;
using AeroTech.Ordering.Persistence.Tests._Shared;
using AeroTech.Ordering.Providers.AirOffer.Services;
using AeroTech.Ordering.Query.OrderAggregate.Models;
using AeroTech.Ordering.Query.OrderAggregate.Projection;
using AeroTech.Ordering.Synchronizer.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class InformationPreservationTests
    {
        private const string OfferId = "SYNTHETIC-PRICED-OFFER";

        private readonly OrderingDatabaseFixture _fixture;

        public InformationPreservationTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Every_supplied_air_offer_fact_survives_wire_candidate_order_sql_and_projection()
        {
            var handler = Handler();
            await using var harness = await StartAsync(handler);

            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));
            var callsAfterCreate = handler.Calls;

            var candidate = await AirOfferLiveCandidateBridgeTests.AcceptedCandidateAsync(harness, created.OrderId);
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);
            var document = await ProjectionAsync(harness, created.OrderId);

            Assert.Equal("EUR", candidate.SaleCurrencyCode);
            Assert.Equal("978", order.SaleCurrency.CurrencyRef);
            Assert.Equal("EUR", order.SaleCurrency.CurrencyCode);
            Assert.Equal("EUR", document.SaleCurrencyCode);

            var fare = order.PricingLines.Single(line => line.Component == PricingComponentType.Fare);
            var tax = order.PricingLines.Single(line => line.Component == PricingComponentType.Tax);
            Assert.Equal("YOW", fare.SourceCode);
            Assert.Equal("Fare", fare.SourceName);
            Assert.Equal("9001", fare.SourceReference);
            Assert.Equal("AT", tax.SourceCode);
            Assert.Equal("Airport tax", tax.SourceName);
            Assert.Null(tax.SourceReference);
            Assert.Equal(PricingCalculationKind.Amount, fare.CalculationKind);
            Assert.Equal(100m, fare.OriginalValue.Amount);
            Assert.Equal(100m, fare.SaleValue.Amount);
            Assert.Contains(document.Pricing, line => line.SourceCode == "YOW" && line.SourceName == "Fare" && line.SourceReference == "9001");

            var journey = Assert.Single(order.Journeys);
            Assert.Equal("BOUND-1", journey.SourceBoundRef);
            Assert.Equal("Outbound", journey.SourceDirectionRaw);
            Assert.Null(journey.Direction);
            Assert.Equal("OneWay", order.SourceJourneyTypeRaw);
            Assert.Null(order.JourneyType);
            Assert.Equal("BOUND-1", Assert.Single(document.Journeys).SourceBoundRef);

            var segment = Assert.Single(order.Segments);
            Assert.Equal(journey.Id, segment.JourneyId);
            Assert.Equal("7001", segment.FlightRef);
            Assert.Equal("AT101", segment.FlightNumber);
            Assert.Equal("4", segment.FlightVersion);
            Assert.Equal("5001", segment.SourceCapacityRef);
            Assert.Equal("1", segment.MarketingCarrierRef);
            Assert.Equal("1", segment.OperatingCarrierRef);
            Assert.Equal("61", segment.OriginTerminalRef);
            Assert.Equal("62", segment.DestinationTerminalRef);
            Assert.Equal(270, segment.Duration);
            Assert.Equal("9", segment.AircraftRef);

            var projectedSegment = Assert.Single(document.Segments);
            Assert.Equal("AT101", projectedSegment.FlightNumber);
            Assert.Equal("5001", projectedSegment.SourceCapacityRef);
            Assert.Equal(270, projectedSegment.Duration);

            var legs = segment.Legs.OrderBy(leg => leg.Sequence).ToList();
            Assert.Equal(2, legs.Count);
            Assert.Equal("11", legs[0].OriginRef);
            Assert.Equal("33", legs[0].DestinationRef);
            Assert.Equal("33", legs[1].OriginRef);
            Assert.Equal("22", legs[1].DestinationRef);
            Assert.NotNull(legs[0].Departure);
            Assert.NotNull(legs[1].Arrival);
            Assert.Equal(2, Assert.Single(document.Segments).Legs.Count);

            var service = Assert.Single(order.Services);
            Assert.True(service.SoldTerms.Refundable);
            Assert.True(service.SoldTerms.Changeable);
            Assert.False(service.SoldTerms.Upgradable);
            Assert.Equal(1, service.AirTransport!.CheckedBaggage!.Pieces);
            Assert.Equal(20m, service.AirTransport.CheckedBaggage.Weight);
            Assert.Equal(BaggageWeightUnit.Kg, service.AirTransport.CheckedBaggage.WeightUnit);
            Assert.Equal(1, service.AirTransport.CabinBaggage!.Pieces);
            Assert.Equal(7m, service.AirTransport.CabinBaggage.Weight);
            Assert.Equal(ServicePriceTreatment.SupplierOpaque, service.PriceTreatment);
            Assert.Null(service.ServiceCode);
            Assert.Null(service.Name);
            Assert.Null(service.SupplierPartyRef);
            Assert.Null(service.DeliveryProviderRef);

            var item = Assert.Single(order.Items);
            Assert.Equal("AirOffer", item.Product.SourceSystem);
            Assert.Equal(OfferId, item.Product.SourceOfferId);
            Assert.Null(item.Product.ProductCode);
            Assert.Equal(CommercialTermState.Permitted, item.CommercialTerms.Refundability);
            Assert.Equal(CommercialTermState.Permitted, item.CommercialTerms.Changeability);
            Assert.Equal(CommercialTermState.Prohibited, item.CommercialTerms.UpgradeEligibility);
            Assert.Equal(order.AcceptedSource.CapturedAt, item.CommercialTerms.TermsCapturedAt);

            var unit = Assert.Single(order.FareConstructions.Single().PricingUnits);
            Assert.Equal("OneWay", unit.SourceKindRaw);
            Assert.Equal(FarePricingUnitType.Unspecified, unit.Type);
            Assert.Equal(FareCombinationMethod.Unspecified, unit.CombinationMethod);
            Assert.Equal("BOUND-1", Assert.Single(unit.CoveredBounds).SourceBoundRef);

            var component = Assert.Single(unit.Components);
            Assert.Equal("9001", component.SourceFareRef);
            Assert.Equal("YOW", component.FareBasis);
            Assert.Equal("FLEX", component.FareFamily);
            Assert.Equal("Published", component.FareType);
            Assert.Equal(45, component.TicketingRestrictionMinutes);
            Assert.Empty(component.CoveredServices);

            var observed = order.ObservedTicketingDeadline;
            Assert.NotNull(observed);
            Assert.Equal(new DateTimeOffset(2026, 9, 10, 12, 0, 0, TimeSpan.FromHours(3.5)), observed.Value);
            Assert.Equal("AirOffer", observed.SourceOwner);
            Assert.Equal(AirOfferCandidateMapper.TicketingDeadlineSourceRef, observed.SourceRef);
            Assert.Equal(ValidityState.NotSupplied, order.TicketingValidity.State);
            Assert.NotNull(document.ObservedTicketingDeadline);

            var link = Assert.Single(order.ItemServiceLinks);
            Assert.Equal(service.Beneficiaries.Single().TravelerId, Assert.Single(link.TravelersAtAssociation).TravelerId);
            Assert.Equal(segment.Id, Assert.Single(link.SegmentsAtAssociation).SegmentId);
            Assert.Equal(item.Id, link.OrderItemId);

            Assert.Equal(callsAfterCreate, handler.Calls);
        }

        [Fact]
        public async Task Schema_three_service_detail_does_not_duplicate_flight_level_facts()
        {
            var handler = Handler();
            await using var harness = await StartAsync(handler);

            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));
            var candidate = await AirOfferLiveCandidateBridgeTests.AcceptedCandidateAsync(harness, created.OrderId);
            var details = Assert.Single(candidate.Services).Details;

            Assert.Equal(["bookingClass", "cabinRef", "rbdRef"], details.Keys.OrderBy(key => key, StringComparer.Ordinal));
            Assert.DoesNotContain("flightNumber", details.Keys);
            Assert.DoesNotContain("marketingCarrierRef", details.Keys);
            Assert.DoesNotContain("operatingCarrierRef", details.Keys);
            Assert.DoesNotContain("flightVersion", details.Keys);

            var document = await ProjectionAsync(harness, created.OrderId);
            var service = Assert.Single(Assert.Single(document.Items).Services);
            Assert.Equal("3", service.AirTransport!.CabinRef);

            var publicOrder = OrderProjectionMapper.ToPublicOrder(document);
            var publicService = Assert.Single(Assert.Single(publicOrder.Items).Services);
            Assert.Equal("AT101", publicService.AirTransport!.FlightNumber);
            Assert.Equal("1", publicService.AirTransport.MarketingAirlineRef);
            Assert.Equal("1", publicService.AirTransport.OperatingAirlineRef);
        }

        [Fact]
        public async Task Applied_conversion_evidence_is_preserved_without_recomputing_money()
        {
            var handler = new AirOfferWireFixtures.StubHandler(() => AirOfferWireFixtures
                .Details(lineCurrencyId: 840, equivalent: 92m, conversionRate: 0.92m, withTerminals: true)
                .Replace("\"baseAmount\":100,\"chargeAmount\":20,\"totalAmount\":120", "\"baseAmount\":92,\"chargeAmount\":20,\"totalAmount\":112"));
            await using var harness = await StartAsync(handler);

            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);
            var fare = order.PricingLines.Single(line => line.Component == PricingComponentType.Fare);

            Assert.Equal(100m, fare.OriginalValue.Amount);
            Assert.Equal("840", fare.OriginalValue.CurrencyRef);
            Assert.Equal(92m, fare.SaleValue.Amount);
            Assert.Equal("978", fare.SaleValue.CurrencyRef);
            Assert.Equal("ROE-1", fare.SourceConversionRef);

            var conversion = fare.AppliedConversion;
            Assert.NotNull(conversion);
            Assert.Equal("ROE-1", conversion.SourceConversionRef);
            Assert.Equal("840", conversion.FromCurrencyRef);
            Assert.Equal("978", conversion.ToCurrencyRef);
            Assert.Equal(0.92m, conversion.Rate);
            Assert.Equal(2, conversion.DecimalPlaces);
            Assert.Equal("100", conversion.RoundingToken);

            var document = await ProjectionAsync(harness, created.OrderId);
            var projected = document.Pricing.Single(line => line.Component == PricingComponentType.Fare);
            Assert.Equal("0.92", projected.AppliedConversion!.Rate);
            Assert.Equal("92", projected.SaleValue.Amount);
        }

        [Fact]
        public async Task Percentage_rows_record_their_calculation_kind_without_interpreting_the_rate()
        {
            var handler = new AirOfferWireFixtures.StubHandler(() => AirOfferWireFixtures.Details(percentageOrderCharge: 12m));
            await using var harness = await StartAsync(handler);

            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);
            var charge = order.PricingLines.Single(line => line.SourceLineRef.StartsWith("orderCharges/", StringComparison.Ordinal));

            Assert.Equal(PricingCalculationKind.Percentage, charge.CalculationKind);
            Assert.Equal(12m, charge.SaleValue.Amount);
            Assert.Equal(12m, charge.OriginalValue.Amount);
            Assert.Equal("IR", charge.SourceCode);
            Assert.All(order.PricingLines.Where(line => line.Id != charge.Id), line => Assert.Equal(PricingCalculationKind.Amount, line.CalculationKind));
        }

        [Fact]
        public async Task Projection_rebuild_is_byte_identical_for_the_same_accepted_state()
        {
            var handler = Handler();
            await using var harness = await StartAsync(handler);

            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));
            var first = await ProjectionJsonAsync(harness, created.OrderId);

            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);
            var rebuilt = OrderProjectionJson.Write(OrderProjectionBuilder.Build(order));

            Assert.Equal(first, rebuilt);
            Assert.Equal(OrderProjectionJson.SchemaVersion, OrderProjectionJson.Read(first).SchemaVersion);
        }

        private static AirOfferWireFixtures.StubHandler Handler()
            => new(() => AirOfferWireFixtures.Details(withTerminals: true, ticketingRestrictionMinutes: 45));

        private Task<S1Harness> StartAsync(HttpMessageHandler handler)
            => S1Harness.StartAsync(_fixture, services =>
                services.ConfigureHttpClientDefaults(client => client.ConfigurePrimaryHttpMessageHandler(() => handler)),
                useReferenceOffers: false);

        private static async Task<OrderProjectionDocument> ProjectionAsync(S1Harness harness, long orderId)
            => OrderProjectionJson.Read(await ProjectionJsonAsync(harness, orderId));

        private static Task<string> ProjectionJsonAsync(S1Harness harness, long orderId)
            => harness.InScopeAsync(services => services.GetRequiredService<OrderQueryDbContext>()
                .Set<OrderDetailsReadModel>().AsNoTracking().Where(row => row.OrderId == orderId)
                .Select(row => row.DetailsJson).SingleAsync());
    }
}
