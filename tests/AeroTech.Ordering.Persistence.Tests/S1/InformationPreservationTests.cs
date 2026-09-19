using System.Net.Http;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Query._Shared.DbContexts;
using AeroTech.Ordering.Persistence.Tests._Shared;
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

            Assert.Equal(978, candidate.CurrencyId);
            Assert.Equal(978, order.CurrencyId);
            Assert.Equal(978, order.CustomerTotal.CurrencyId);
            Assert.Equal(978, document.CurrencyId);

            var fare = order.PricingLines.Single(line => line.Component == PricingComponentType.Fare);
            var tax = order.PricingLines.Single(line => line.Component == PricingComponentType.Tax);
            Assert.Equal("YOW", fare.Code);
            Assert.Equal("Fare", fare.Name);
            Assert.Equal("9001", fare.Reference);
            Assert.Equal("AT", tax.Code);
            Assert.Equal("Airport tax", tax.Name);
            Assert.Null(tax.Reference);
            Assert.Equal(PricingCalculationKind.Amount, fare.CalculationKind);
            Assert.Equal(100m, fare.OriginalValue.Amount);
            Assert.Equal(100m, fare.SaleValue.Amount);
            Assert.Equal("tickets/0/coupons/0/pricings/0", fare.SourceOccurrencePath);
            Assert.Contains(document.Pricing, line => line.Code == "YOW" && line.Name == "Fare" && line.Reference == "9001");

            var journey = Assert.Single(order.Journeys);
            Assert.Equal("BOUND-1", journey.BoundId);
            Assert.Equal(BoundDirection.Outbound, journey.Direction);
            Assert.Equal(11, journey.OriginAirportId);
            Assert.Equal(22, journey.DestinationAirportId);
            Assert.Equal(JourneyType.OneWay, order.JourneyType);
            Assert.Equal("BOUND-1", Assert.Single(document.Journeys).BoundId);

            var segment = Assert.Single(order.Segments);
            Assert.Equal(journey.Id, segment.JourneyId);
            Assert.Equal(7001L, segment.FlightId);
            Assert.Equal("AT101", segment.FlightNumber);
            Assert.Equal(4, segment.FlightVersion);
            Assert.Equal(5001L, segment.FlightCapacityId);
            Assert.Equal(1, segment.MarketingAirlineId);
            Assert.Equal(1, segment.OperatingAirlineId);
            Assert.Equal(61, segment.OriginAirportTerminalId);
            Assert.Equal(62, segment.DestinationAirportTerminalId);
            Assert.Equal(270, segment.Duration);
            Assert.Equal(9, segment.AircraftId);

            var projectedSegment = Assert.Single(document.Segments);
            Assert.Equal("AT101", projectedSegment.FlightNumber);
            Assert.Equal(5001L, projectedSegment.FlightCapacityId);
            Assert.Equal(270, projectedSegment.Duration);

            var legs = segment.Legs.OrderBy(leg => leg.Sequence).ToList();
            Assert.Equal(2, legs.Count);
            Assert.Equal([81L, 82L], legs.Select(leg => leg.LegId));
            Assert.Equal(11, legs[0].OriginAirportId);
            Assert.Equal(33, legs[0].DestinationAirportId);
            Assert.Equal(33, legs[1].OriginAirportId);
            Assert.Equal(22, legs[1].DestinationAirportId);
            Assert.NotNull(legs[0].DepartureDateTime);
            Assert.NotNull(legs[1].ArrivalDateTime);
            Assert.Equal(2, projectedSegment.Legs.Count);

            var service = Assert.Single(order.Services);
            Assert.True(service.SoldTerms.Refundable);
            Assert.True(service.SoldTerms.Changeable);
            Assert.False(service.SoldTerms.Upgradable);
            Assert.Equal(1, service.CheckedBaggage!.Pieces);
            Assert.Equal(20m, service.CheckedBaggage.Weight);
            Assert.Equal(BaggageWeightUnit.Kg, service.CheckedBaggage.WeightUnit);
            Assert.Equal(1, service.CabinBaggage!.Pieces);
            Assert.Equal(7m, service.CabinBaggage.Weight);
            Assert.Equal(3, service.CabinClassId);
            Assert.Equal(44L, service.RbdId);
            Assert.Equal("Y", service.BookingClass);
            Assert.Equal(Assert.Single(order.Travellers).Id, service.TravellerId);
            Assert.Equal(segment.Id, service.SegmentId);

            var item = Assert.Single(order.Items);
            Assert.Equal(OrderItemKind.OfferPackage, item.Kind);
            Assert.Equal(item.Id, service.OrderItemId);
            Assert.Equal(order.CustomerTotal.Amount, item.AcceptedTotal.Amount);
            Assert.Equal(OfferId, order.SourceOfferId);

            var unit = Assert.Single(order.FareConstructions.Single().PricingUnits);
            Assert.Equal(FarePricingUnitType.OneWay, unit.Type);
            Assert.Equal(1, unit.Sequence);
            Assert.Equal("BOUND-1", Assert.Single(unit.CoveredBounds).CoveredBoundOfferId);

            var component = Assert.Single(unit.Components);
            Assert.Equal(9001L, component.AirFareId);
            Assert.Equal("YOW", component.FareBasis);
            Assert.Equal("FLEX", component.FareFamily);
            Assert.Equal("Published", component.FareType);
            Assert.Equal(3, component.CabinClassId);
            Assert.Equal(44L, component.RbdId);
            Assert.Equal(45, component.TicketingRestrictionMinutes);

            Assert.Equal(new DateTimeOffset(2026, 9, 10, 12, 0, 0, TimeSpan.FromHours(3.5)), order.LastTicketingDate);
            Assert.Equal(order.LastTicketingDate, document.LastTicketingDate);

            Assert.Equal(callsAfterCreate, handler.Calls);
        }

        [Fact]
        public async Task A_service_carries_only_its_own_facts_and_never_repeats_the_flight()
        {
            var handler = Handler();
            await using var harness = await StartAsync(handler);

            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));
            var candidate = await AirOfferLiveCandidateBridgeTests.AcceptedCandidateAsync(harness, created.OrderId);
            var candidateService = Assert.Single(candidate.Services);

            Assert.Equal(3, candidateService.CabinClassId);
            Assert.Equal(44L, candidateService.RbdId);
            Assert.Equal("Y", candidateService.BookingClass);

            var document = await ProjectionAsync(harness, created.OrderId);
            var projectedSegment = Assert.Single(document.Segments);
            var projectedService = Assert.Single(Assert.Single(document.Items).Services);

            Assert.Equal(3, projectedService.CabinClassId);
            Assert.Equal(projectedSegment.SegmentId, projectedService.SegmentId);
            Assert.Equal("AT101", projectedSegment.FlightNumber);
            Assert.Equal(1, projectedSegment.MarketingAirlineId);
            Assert.Equal(1, projectedSegment.OperatingAirlineId);
            Assert.Equal(4, projectedSegment.FlightVersion);

            var publicOrder = OrderProjectionMapper.ToPublicOrder(document);
            var publicSegment = Assert.Single(publicOrder.Itinerary);
            var publicService = Assert.Single(Assert.Single(publicOrder.Items).Services);

            Assert.Equal("AT101", publicSegment.FlightNumber);
            Assert.Equal(1, publicSegment.MarketingAirlineId);
            Assert.Equal(1, publicSegment.OperatingAirlineId);
            Assert.Equal(publicSegment.SegmentId, publicService.SegmentId);
            Assert.Equal(3, publicService.CabinClassId);
            Assert.Equal(44L, publicService.RbdId);
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
            Assert.Equal(840, fare.OriginalValue.CurrencyId);
            Assert.Equal(92m, fare.SaleValue.Amount);
            Assert.Equal(978, fare.SaleValue.CurrencyId);
            Assert.Equal("ROE-1", fare.SourceConversionRef);

            var conversion = fare.AppliedConversion;
            Assert.NotNull(conversion);
            Assert.Equal("ROE-1", conversion.SourceConversionRef);
            Assert.Equal(840, conversion.FromCurrencyId);
            Assert.Equal(978, conversion.ToCurrencyId);
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
            var charge = order.PricingLines.Single(line => line.SourceOccurrencePath.StartsWith("orderCharges/", StringComparison.Ordinal));

            Assert.Equal(PricingCalculationKind.Percentage, charge.CalculationKind);
            Assert.Equal(12m, charge.SaleValue.Amount);
            Assert.Equal(12m, charge.OriginalValue.Amount);
            Assert.Equal("IR", charge.Code);
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
