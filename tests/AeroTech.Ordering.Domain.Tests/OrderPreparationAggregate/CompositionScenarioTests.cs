using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Policies;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;
using AeroTech.Ordering.Domain.Tests._Shared;
using Xunit;

namespace AeroTech.Ordering.Domain.Tests.OrderPreparationAggregate
{
    public sealed class CompositionScenarioTests
    {
        private static readonly DateTimeOffset Now = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

        [Fact]
        public void SC_01_two_independently_priced_items_keep_disjoint_services_and_totals()
        {
            var builder = new CandidateBuilder(Now)
                .Traveller("PAX-A")
                .Segment("SEG-1")
                .Segment("SEG-2")
                .AirService("S-1", "PAX-A", "SEG-1")
                .AirService("S-2", "PAX-A", "SEG-2")
                .Package("ITEM-1", "S-1")
                .Package("ITEM-2", "S-2")
                .Line("outbound/fare", "ITEM-1", PricingComponentType.Fare, 100m, "S-1")
                .Line("inbound/fare", "ITEM-2", PricingComponentType.Fare, 140m, "S-2");

            var candidate = Valid(builder);

            Assert.Equal(2, candidate.Items.Count);
            Assert.Equal(100m, candidate.Items.Single(item => item.ItemKey == "ITEM-1").AcceptedTotal.Amount);
            Assert.Equal(140m, candidate.Items.Single(item => item.ItemKey == "ITEM-2").AcceptedTotal.Amount);
            Assert.Equal(240m, candidate.CustomerTotal.Amount);
            Assert.Empty(candidate.Items[0].ServiceKeys.Intersect(candidate.Items[1].ServiceKeys));
        }

        [Fact]
        public void SC_01_one_service_cannot_belong_to_two_items()
        {
            var builder = new CandidateBuilder(Now)
                .Traveller("PAX-A")
                .Segment("SEG-1")
                .AirService("S-1", "PAX-A", "SEG-1")
                .Package("ITEM-1", "S-1")
                .Package("ITEM-2", "S-1")
                .Line("fare", "ITEM-1", PricingComponentType.Fare, 100m, "S-1");

            AssertMismatch("service S-1 belongs to items ITEM-1 and ITEM-2", builder);
        }

        [Fact]
        public void SC_02_an_offer_package_and_a_separate_product_item_coexist_in_one_order()
        {
            var builder = new CandidateBuilder(Now)
                .Traveller("PAX-A")
                .Segment("SEG-1")
                .Segment("SEG-2")
                .AirService("S-AIR", "PAX-A", "SEG-1")
                .AirService("S-EXTRA", "PAX-A", "SEG-2")
                .Package("ITEM-AIR", "S-AIR")
                .Item(new CandidateItem("ITEM-PRODUCT", OrderItemKind.Product, ["S-EXTRA"], Zero))
                .Line("air/fare", "ITEM-AIR", PricingComponentType.Fare, 100m, "S-AIR")
                .Line("product/charge", "ITEM-PRODUCT", PricingComponentType.ProductCharge, 35m, "S-EXTRA");

            var candidate = Valid(builder);

            Assert.Equal(
                [OrderItemKind.OfferPackage, OrderItemKind.Product],
                candidate.Items.Select(item => item.ItemKind));
            Assert.Equal(135m, candidate.CustomerTotal.Amount);
        }

        [Fact]
        public void SC_03_a_fee_only_monetary_charge_item_owns_no_service()
        {
            var builder = FeeOnlyBuilder();

            var candidate = Valid(builder);
            var charge = candidate.Items.Single(item => item.ItemKind == OrderItemKind.MonetaryCharge);

            Assert.Empty(charge.ServiceKeys);
            Assert.Equal(25m, charge.AcceptedTotal.Amount);
            Assert.Equal(125m, candidate.CustomerTotal.Amount);
        }

        [Fact]
        public void SC_03_a_monetary_charge_item_that_owns_a_service_is_rejected()
        {
            var builder = new CandidateBuilder(Now)
                .Traveller("PAX-A")
                .Segment("SEG-1")
                .AirService("S-1", "PAX-A", "SEG-1")
                .Item(new CandidateItem("ITEM-FEE", OrderItemKind.MonetaryCharge, ["S-1"], Zero))
                .Line("fee", "ITEM-FEE", PricingComponentType.Fee, 25m, "ITEM-FEE", basisType: PricingBasisType.OrderItem);

            AssertMismatch("monetary charge item ITEM-FEE cannot own services", builder);
        }

        [Fact]
        public void SC_03_a_non_charge_item_without_a_service_is_rejected()
        {
            var builder = new CandidateBuilder(Now)
                .Traveller("PAX-A")
                .Segment("SEG-1")
                .AirService("S-1", "PAX-A", "SEG-1")
                .Package("ITEM-1", "S-1")
                .Item(new CandidateItem("ITEM-EMPTY", OrderItemKind.Product, [], Zero))
                .Line("fare", "ITEM-1", PricingComponentType.Fare, 100m, "S-1");

            AssertMismatch("item ITEM-EMPTY needs at least one service", builder);
        }

        [Fact]
        public void SC_07_a_source_proven_fare_construction_may_span_two_items()
        {
            var builder = new CandidateBuilder(Now)
                .Journey("BOUND-1")
                .Traveller("PAX-A")
                .Segment("SEG-1")
                .Journey("BOUND-2", BoundDirection.Inbound)
                .Segment("SEG-2")
                .AirService("S-1", "PAX-A", "SEG-1")
                .AirService("S-2", "PAX-A", "SEG-2")
                .Package("ITEM-OUT", "S-1")
                .Package("ITEM-IN", "S-2")
                .Line("out/fare", "ITEM-OUT", PricingComponentType.Fare, 100m, "S-1")
                .Line("in/fare", "ITEM-IN", PricingComponentType.Fare, 100m, "S-2")
                .Units(CandidateBuilder.Unit(1, FarePricingUnitType.RoundTrip, ["BOUND-1", "BOUND-2"], CandidateBuilder.Component(4242)))
                .ConstructionFor("ITEM-OUT", "ITEM-IN");

            var candidate = Valid(builder);

            Assert.Equal(["ITEM-OUT", "ITEM-IN"], candidate.FareConstruction!.ItemKeys);
            Assert.Equal(FarePricingUnitType.RoundTrip, Assert.Single(candidate.FareConstruction.PricingUnits).Type);
        }

        [Fact]
        public void SC_23_an_informational_line_never_moves_the_customer_total()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now)
                .Line("note", "ITEM-A", PricingComponentType.Other, 999m, "S-A", effect: PricingEffect.Informational);

            var candidate = Valid(builder);

            Assert.Equal(120m, candidate.CustomerTotal.Amount);
            Assert.Equal(120m, Assert.Single(candidate.Items).AcceptedTotal.Amount);
            Assert.Equal(3, candidate.PricingLines.Count);
        }

        [Theory]
        [InlineData(PricingEffect.CustomerBalance)]
        [InlineData(PricingEffect.SettlementOnly)]
        public void SC_24_an_other_component_is_only_informational(PricingEffect effect)
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now)
                .Line("note", "ITEM-A", PricingComponentType.Other, 10m, "S-A",
                    effect: effect,
                    settlementAttribution: effect == PricingEffect.SettlementOnly ? new SettlementAttribution("agency:77", "OTH") : null);

            var exception = Assert.Throws<BusinessException>(() => CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope));

            Assert.Contains("Other", exception.Message);
        }

        [Theory]
        [InlineData(OrderPricingLineDirection.Debit, 145)]
        [InlineData(OrderPricingLineDirection.Credit, 95)]
        public void SC_29_an_adjustment_may_be_a_debit_or_a_credit(OrderPricingLineDirection direction, decimal expectedTotal)
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now)
                .Line("adjust", "ITEM-A", PricingComponentType.Adjustment, 25m, "S-A", direction: direction);

            var candidate = Valid(builder);

            Assert.Equal(expectedTotal, candidate.CustomerTotal.Amount);
        }

        [Fact]
        public void SC_31_a_zero_value_line_is_accepted_and_recorded()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now)
                .Line("waived-fee", "ITEM-A", PricingComponentType.Fee, 0m, "S-A");

            var candidate = Valid(builder);

            Assert.Equal(120m, candidate.CustomerTotal.Amount);
            Assert.Equal(0m, candidate.PricingLines.Single(line => line.SourceOccurrencePath == "waived-fee").SaleValue.Amount);
        }

        [Fact]
        public void SC_44_an_adult_and_a_child_are_two_travellers_with_their_own_passenger_type_codes()
        {
            var builder = new CandidateBuilder(Now)
                .Traveller("PAX-ADT")
                .Traveller("PAX-CHD", PassengerTypeCode.CHD)
                .Segment("SEG-1")
                .AirService("S-ADT", "PAX-ADT", "SEG-1")
                .AirService("S-CHD", "PAX-CHD", "SEG-1")
                .Package("ITEM-A", "S-ADT", "S-CHD")
                .Line("adt/fare", "ITEM-A", PricingComponentType.Fare, 100m, "S-ADT")
                .Line("chd/fare", "ITEM-A", PricingComponentType.Fare, 75m, "S-CHD");

            var candidate = Valid(builder);

            Assert.Equal(
                [PassengerTypeCode.ADT, PassengerTypeCode.CHD],
                candidate.Travellers.Select(traveller => traveller.PassengerTypeCode));
            Assert.Equal(175m, candidate.CustomerTotal.Amount);
            Assert.Equal(2, candidate.Services.Count);
        }

        [Fact]
        public void SC_45_two_travellers_may_carry_different_fare_groups_on_the_same_bound()
        {
            var builder = new CandidateBuilder(Now)
                .Journey("BOUND-1")
                .Traveller("PAX-A")
                .Traveller("PAX-B")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1", bookingClass: "Y")
                .AirService("S-B", "PAX-B", "SEG-1", bookingClass: "M")
                .Package("ITEM-A", "S-A")
                .Package("ITEM-B", "S-B")
                .Line("a/fare", "ITEM-A", PricingComponentType.Fare, 100m, "S-A")
                .Line("b/fare", "ITEM-B", PricingComponentType.Fare, 60m, "S-B")
                .Units(
                    CandidateBuilder.Unit(1, FarePricingUnitType.OneWay, ["BOUND-1"], CandidateBuilder.Component(4242, "YOW", bookingClass: "Y")),
                    CandidateBuilder.Unit(2, FarePricingUnitType.OneWay, ["BOUND-1"], CandidateBuilder.Component(4343, "MOW", bookingClass: "M")));

            var candidate = Valid(builder);

            Assert.Equal(
                ["YOW", "MOW"],
                candidate.FareConstruction!.PricingUnits.Select(unit => unit.Components.Single().FareBasis));
            Assert.Equal(["Y", "M"], candidate.Services.Select(service => service.BookingClass));
        }

        [Fact]
        public void SC_46_two_travellers_may_fly_different_itineraries_inside_one_order()
        {
            var builder = new CandidateBuilder(Now)
                .Journey("BOUND-1")
                .Traveller("PAX-A")
                .Traveller("PAX-B")
                .Segment("SEG-1")
                .Journey("BOUND-2", BoundDirection.Inbound)
                .Segment("SEG-2")
                .AirService("S-A", "PAX-A", "SEG-1")
                .AirService("S-B", "PAX-B", "SEG-2")
                .Package("ITEM-A", "S-A")
                .Package("ITEM-B", "S-B")
                .Line("a/fare", "ITEM-A", PricingComponentType.Fare, 100m, "S-A")
                .Line("b/fare", "ITEM-B", PricingComponentType.Fare, 120m, "S-B");

            var candidate = Valid(builder);

            Assert.Equal("SEG-1", candidate.Services.Single(service => service.TravellerRef == "PAX-A").SegmentKey);
            Assert.Equal("SEG-2", candidate.Services.Single(service => service.TravellerRef == "PAX-B").SegmentKey);
            Assert.Equal(2, candidate.Journeys.Count);
        }

        [Fact]
        public void SC_47_a_connection_is_two_passenger_segments_under_one_journey()
        {
            var builder = new CandidateBuilder(Now)
                .Journey("BOUND-1")
                .Traveller("PAX-A")
                .Segment("SEG-1")
                .Segment("SEG-2")
                .AirService("S-1", "PAX-A", "SEG-1")
                .AirService("S-2", "PAX-A", "SEG-2")
                .Package("ITEM-A", "S-1", "S-2")
                .Line("leg1/fare", "ITEM-A", PricingComponentType.Fare, 60m, "S-1")
                .Line("leg2/fare", "ITEM-A", PricingComponentType.Fare, 60m, "S-2");

            var candidate = Valid(builder);

            Assert.Single(candidate.Journeys);
            Assert.Equal(2, candidate.Segments.Count);
            Assert.All(candidate.Segments, segment => Assert.Equal("BOUND-1", segment.BoundId));
            Assert.Equal([1, 2], candidate.Segments.Select(segment => segment.Sequence));
            Assert.Equal(120m, candidate.CustomerTotal.Amount);
        }

        [Fact]
        public void SC_48_a_multi_bound_itinerary_carries_three_journeys()
        {
            var builder = new CandidateBuilder(Now).JourneyKind(JourneyType.Circle)
                .Journey("BOUND-1")
                .Traveller("PAX-A")
                .Segment("SEG-1")
                .Journey("BOUND-2")
                .Segment("SEG-2")
                .Journey("BOUND-3")
                .Segment("SEG-3")
                .AirService("S-1", "PAX-A", "SEG-1")
                .AirService("S-2", "PAX-A", "SEG-2")
                .AirService("S-3", "PAX-A", "SEG-3")
                .Package("ITEM-A", "S-1", "S-2", "S-3")
                .Line("b1/fare", "ITEM-A", PricingComponentType.Fare, 50m, "S-1")
                .Line("b2/fare", "ITEM-A", PricingComponentType.Fare, 60m, "S-2")
                .Line("b3/fare", "ITEM-A", PricingComponentType.Fare, 70m, "S-3");

            var candidate = Valid(builder);

            Assert.Equal(3, candidate.Journeys.Count);
            Assert.Equal([1, 2, 3], candidate.Journeys.Select(journey => journey.Sequence));
            Assert.Equal(JourneyType.Circle, candidate.JourneyType);
            Assert.Equal(180m, candidate.CustomerTotal.Amount);
        }

        [Fact]
        public void SC_50_the_same_flight_number_on_two_dated_segments_does_not_collapse()
        {
            var repeated = Dated("SEG-1", "BOUND-1", 1, Now.AddDays(10), 7001);
            var later = Dated("SEG-2", "BOUND-1", 2, Now.AddDays(17), 7002);

            var builder = new CandidateBuilder(Now)
                .Journey("BOUND-1")
                .Traveller("PAX-A")
                .Segment("SEG-1", repeated)
                .Segment("SEG-2", later)
                .AirService("S-1", "PAX-A", "SEG-1")
                .AirService("S-2", "PAX-A", "SEG-2")
                .Package("ITEM-A", "S-1", "S-2")
                .Line("one/fare", "ITEM-A", PricingComponentType.Fare, 60m, "S-1")
                .Line("two/fare", "ITEM-A", PricingComponentType.Fare, 60m, "S-2");

            var candidate = Valid(builder);

            Assert.Equal(2, candidate.Segments.Count);
            Assert.All(candidate.Segments, segment => Assert.Equal("XX100", segment.FlightNumber));
            Assert.Equal(2, candidate.Segments.Select(segment => segment.SoldDeparture).Distinct().Count());
            Assert.Equal(2, candidate.Segments.Select(segment => segment.FlightId).Distinct().Count());
        }

        [Fact]
        public void SC_50_the_same_segment_key_twice_is_rejected()
        {
            var builder = new CandidateBuilder(Now)
                .Journey("BOUND-1")
                .Traveller("PAX-A")
                .Segment("SEG-1", Dated("SEG-1", "BOUND-1", 1, Now.AddDays(10), 7001))
                .Segment("SEG-1", Dated("SEG-1", "BOUND-1", 2, Now.AddDays(17), 7002))
                .AirService("S-1", "PAX-A", "SEG-1")
                .Package("ITEM-A", "S-1")
                .Line("one/fare", "ITEM-A", PricingComponentType.Fare, 60m, "S-1");

            Assert.Throws<BusinessException>(() => CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope));
        }

        [Fact]
        public void SC_53_an_open_air_segment_carries_no_dated_flight_and_stays_sellable()
        {
            var open = new CandidateSegment(
                "SEG-OPEN", "BOUND-1", 1, SegmentKind.OpenAir,
                1001, null, 1002, null,
                null, null, null, null, null, null, null, null, null, null, []);

            var builder = new CandidateBuilder(Now)
                .Journey("BOUND-1")
                .Traveller("PAX-A")
                .Segment("SEG-OPEN", open)
                .AirService("S-1", "PAX-A", "SEG-OPEN")
                .Package("ITEM-A", "S-1")
                .Line("fare", "ITEM-A", PricingComponentType.Fare, 100m, "S-1");

            var candidate = Valid(builder);
            var segment = Assert.Single(candidate.Segments);

            Assert.Equal(SegmentKind.OpenAir, segment.Kind);
            Assert.Null(segment.SoldDeparture);
            Assert.Null(segment.FlightId);
            Assert.Empty(segment.Legs);
        }

        [Fact]
        public void SC_53_an_air_service_cannot_cover_a_surface_segment()
        {
            var surface = new CandidateSegment(
                "SEG-BUS", "BOUND-1", 1, SegmentKind.Surface,
                1001, null, 1002, null,
                null, null, null, null, null, null, null, null, null, null, []);

            var builder = new CandidateBuilder(Now)
                .Journey("BOUND-1")
                .Traveller("PAX-A")
                .Segment("SEG-BUS", surface)
                .AirService("S-1", "PAX-A", "SEG-BUS")
                .Package("ITEM-A", "S-1")
                .Line("fare", "ITEM-A", PricingComponentType.Fare, 100m, "S-1");

            AssertMismatch("air service S-1 cannot cover a surface segment", builder);
        }

        private static CandidateBuilder FeeOnlyBuilder()
            => new CandidateBuilder(Now)
                .Traveller("PAX-A")
                .Segment("SEG-1")
                .AirService("S-1", "PAX-A", "SEG-1")
                .Package("ITEM-AIR", "S-1")
                .Item(new CandidateItem("ITEM-FEE", OrderItemKind.MonetaryCharge, [], Zero))
                .Line("air/fare", "ITEM-AIR", PricingComponentType.Fare, 100m, "S-1")
                .Line("service/fee", "ITEM-FEE", PricingComponentType.Fee, 25m, "ITEM-FEE", basisType: PricingBasisType.OrderItem);

        private static CandidateSegment Dated(string key, string boundId, int sequence, DateTimeOffset departure, long flightId)
            => new(
                key, boundId, sequence, SegmentKind.ScheduledAir,
                1001, null, 1002, null,
                departure, departure.AddHours(2),
                flightId, "XX100", 1, 21, 21, 8000 + flightId, 120, 1,
                [new CandidateSegmentLeg(9000 + flightId, 1, 1001, null, 1002, null, departure, departure.AddHours(2))]);

        private static Money Zero => new(0m, CandidateBuilder.SaleCurrencyId);

        private static NormalizedCandidate Valid(CandidateBuilder builder)
        {
            var candidate = builder.Build();
            CandidateValidator.EnsureValid(candidate, builder.SalesScope);
            return candidate;
        }

        private static void AssertMismatch(string fragment, CandidateBuilder builder)
        {
            var exception = Assert.Throws<BusinessException>(() => CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope));

            Assert.Contains(fragment, exception.Message);
        }
    }
}
