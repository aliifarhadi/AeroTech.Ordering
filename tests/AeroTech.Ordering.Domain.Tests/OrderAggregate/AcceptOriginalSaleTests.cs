using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderAggregate;
using AeroTech.Ordering.Domain.OrderAggregate.Arguments;
using AeroTech.Ordering.Domain.OrderAggregate.DomainEvents;
using AeroTech.Ordering.Domain.OrderAggregate.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;
using AeroTech.Ordering.Domain.Tests._Shared;
using AeroTech.Ordering.Domain.Tests.OrderPreparationAggregate;
using Xunit;

namespace AeroTech.Ordering.Domain.Tests.OrderAggregate
{
    public sealed class AcceptOriginalSaleTests
    {
        private static readonly DateTimeOffset Now = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

        [Fact]
        public void One_traveler_one_segment_package_creates_one_item_one_service_total_120_versions_1()
        {
            var order = Accept(CandidateBuilder.OneWayFare100Tax20(Now), Bind("PAX-A"));

            Assert.Single(order.Items);
            Assert.Single(order.Services);
            Assert.Equal(120.00m, order.CustomerTotal.Amount);
            Assert.Equal("EUR", order.CustomerTotal.CurrencyRef);
            Assert.Equal(1, order.CommercialVersion);
            Assert.Equal(1, order.FinancialSequence);
            Assert.Equal(1, order.OrderRevision);
            Assert.Single(order.PriceChangeSets);
            Assert.Equal(2, order.PricingLines.Count);
            Assert.Equal(CommercialSummary.Active, order.CommercialSummary);
            Assert.Equal(OrderServiceCommercialStatus.Active, order.Services.Single().CommercialStatus);
            Assert.Equal(120.00m, order.FundingObligations.Single().Amount.Amount);
            Assert.Equal(FundingObligationPurpose.OriginalSale, order.FundingObligations.Single().Purpose);

            var created = Assert.IsType<OrderCreatedDomainEvent>(Assert.Single(order.GetEvents()));
            Assert.Equal(1, created.CommercialVersion);
            Assert.Equal(1, created.FinancialSequence);
            Assert.Equal(1, created.EventOrdinal);
            Assert.Equal(order.PriceChangeSets.Single().Id, created.PriceChangeSetId);
        }

        [Fact]
        public void Technical_stop_keeps_one_service_and_two_legs()
        {
            var builder = new CandidateBuilder(Now)
                .Traveler("PAX-A")
                .Segment("SEG-A", "LEG-1", "LEG-2")
                .AirService("SERVICE-A", "PAX-A", "SEG-A")
                .Package("ITEM-A", "SERVICE-A")
                .Line("FARE", "ITEM-A", PricingComponentType.Fare, 100m, "SERVICE-A");

            var order = Accept(builder, Bind("PAX-A"));

            Assert.Single(order.Services);
            Assert.Equal(["LEG-1", "LEG-2"], order.Segments.Single().Legs.OrderBy(leg => leg.Sequence).Select(leg => leg.SourceLegRef));
            Assert.Single(order.Services.Single().Coverage);
        }

        [Fact]
        public void Source_priced_package_for_two_travelers_and_two_segments_is_one_item_with_four_services()
        {
            var builder = new CandidateBuilder(Now)
                .Traveler("PAX-A")
                .Traveler("PAX-B")
                .Segment("SEG-1")
                .Segment("SEG-2")
                .AirService("S-A1", "PAX-A", "SEG-1")
                .AirService("S-A2", "PAX-A", "SEG-2")
                .AirService("S-B1", "PAX-B", "SEG-1")
                .AirService("S-B2", "PAX-B", "SEG-2")
                .Package("PACKAGE", "S-A1", "S-A2", "S-B1", "S-B2")
                .Line("FARE-A", "PACKAGE", PricingComponentType.Fare, 150m, "S-A1")
                .Line("FARE-B", "PACKAGE", PricingComponentType.Fare, 150m, "S-B1");

            var order = Accept(builder, Bind("PAX-A"), Bind("PAX-B"));

            var item = Assert.Single(order.Items);
            Assert.Equal(4, order.Services.Count);
            Assert.All(order.Services, service => Assert.Equal(item.Id, service.OrderItemId));
            Assert.Equal(4, order.ItemServiceLinks.Count);
            Assert.All(order.Services, service => Assert.Single(service.Beneficiaries));
            Assert.Equal(2, order.Services.Select(service => service.Beneficiaries.Single().TravelerId).Distinct().Count());
        }

        [Fact]
        public void Round_trip_and_two_one_way_constructions_are_retained_as_supplied()
        {
            var roundTrip = Accept(RoundTripBuilder(
                new CandidatePricingUnit("PU-RT", FarePricingUnitType.RoundTrip, FareCombinationMethod.FiledFare, ["OUT", "IN"], null,
                    [new CandidateFareComponent("FC-RT", "YRT", null, "Published", null, null, "Y", ["S-OUT", "S-IN"])])), Bind("PAX-A"));

            var twoOneWays = Accept(RoundTripBuilder(
                new CandidatePricingUnit("PU-OUT", FarePricingUnitType.OneWay, FareCombinationMethod.FiledFare, ["OUT"], null,
                    [new CandidateFareComponent("FC-OUT", "YOW", null, "Published", null, null, "Y", ["S-OUT"])]),
                new CandidatePricingUnit("PU-IN", FarePricingUnitType.OneWay, FareCombinationMethod.FiledFare, ["IN"], null,
                    [new CandidateFareComponent("FC-IN", "YOW", null, "Published", null, null, "Y", ["S-IN"])])), Bind("PAX-A"));

            Assert.Equal(twoOneWays.Segments.Select(segment => segment.SourceSegmentRef), roundTrip.Segments.Select(segment => segment.SourceSegmentRef));
            Assert.Equal(FareConstructionAssurance.SourceProvided, roundTrip.FareConstructions.Single().Assurance);
            Assert.Contains("\"type\":\"RoundTrip\"", roundTrip.FareConstructions.Single().PricingUnitsJson);
            Assert.DoesNotContain("\"type\":\"OneWay\"", roundTrip.FareConstructions.Single().PricingUnitsJson);
            Assert.Equal(2, CountOccurrences(twoOneWays.FareConstructions.Single().PricingUnitsJson, "\"type\":\"OneWay\""));
        }

        [Fact]
        public void Opaque_context_is_retained_without_invented_links()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now)
                .OpaqueUnits(new CandidatePricingUnit("PU-1", FarePricingUnitType.Unspecified, FareCombinationMethod.ProviderDefined, ["BOUND-1"], null,
                    [new CandidateFareComponent("AIRFARE-77", "YOW", "FLEX", "Published", "CABIN-1", "RBD-1", "Y", [])]));

            var order = Accept(builder, Bind("PAX-A"));
            var construction = order.FareConstructions.Single();

            Assert.Equal(FareConstructionAssurance.Opaque, construction.Assurance);
            Assert.Contains("\"sourceFareRef\":\"AIRFARE-77\"", construction.PricingUnitsJson);
            Assert.Contains("\"coveredServiceRefs\":[]", construction.PricingUnitsJson);
        }

        [Fact]
        public void Extended_group_line_is_stored_once_with_quantity_metadata()
        {
            var builder = new CandidateBuilder(Now)
                .Traveler("PAX-A")
                .Traveler("PAX-B")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .AirService("S-B", "PAX-B", "SEG-1")
                .Package("PACKAGE", "S-A", "S-B")
                .Line("GROUP-FARE", "PACKAGE", PricingComponentType.Fare, 200m, "PACKAGE", basisType: PricingBasisType.OrderItem)
                .SourceProvidedConstruction(new CandidatePricingUnit("PU-1", FarePricingUnitType.OneWay, FareCombinationMethod.FiledFare, ["OUT"],
                    new CandidatePricingGroup(["PAX-A", "PAX-B"], "ADT", 2),
                    [new CandidateFareComponent("FC-1", "YOW", null, "Published", null, null, "Y", ["S-A", "S-B"])]));

            var order = Accept(builder, Bind("PAX-A"), Bind("PAX-B"));

            Assert.Equal(200m, order.CustomerTotal.Amount);
            Assert.Equal(200m, order.PricingLines.Single().SaleValue.Amount);
            Assert.Contains("\"quantity\":2", order.FareConstructions.Single().PricingUnitsJson);
        }

        [Fact]
        public void Sale_equivalent_is_the_customer_value_and_original_currency_is_retained()
        {
            var builder = new CandidateBuilder(Now)
                .Traveler("PAX-A")
                .Segment("SEG-A")
                .AirService("SERVICE-A", "PAX-A", "SEG-A")
                .Package("ITEM-A", "SERVICE-A")
                .Line("FARE", "ITEM-A", PricingComponentType.Fare, 92m, "SERVICE-A", original: new Money(100m, "USD"));

            var order = Accept(builder, Bind("PAX-A"));
            var line = order.PricingLines.Single();

            Assert.Equal(92m, order.CustomerTotal.Amount);
            Assert.Equal("EUR", order.CustomerTotal.CurrencyRef);
            Assert.Equal(new Money(100m, "USD"), line.OriginalValue);
            Assert.Equal(new Money(92m, "EUR"), line.SaleValue);
        }

        [Fact]
        public void Repeated_source_traveler_binding_is_rejected_before_any_order()
        {
            var builder = new CandidateBuilder(Now)
                .Traveler("PAX-A")
                .Traveler("PAX-B")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .AirService("S-B", "PAX-B", "SEG-1")
                .Package("PACKAGE", "S-A", "S-B")
                .Line("FARE", "PACKAGE", PricingComponentType.Fare, 200m, "PACKAGE", basisType: PricingBasisType.OrderItem);

            var exception = Assert.Throws<BusinessException>(() => Accept(builder, Bind("PAX-A", "CLIENT-1"), Bind("PAX-A", "CLIENT-2")));

            Assert.Equal(20276, exception.Code);
            Assert.Contains("more than once", exception.Message);
        }

        [Theory]
        [InlineData("missing")]
        [InlineData("foreign")]
        [InlineData("type")]
        [InlineData("guardian-cycle")]
        public void Incomplete_or_inconsistent_traveler_binding_is_rejected(string defect)
        {
            var builder = new CandidateBuilder(Now)
                .Traveler("PAX-A")
                .Traveler("PAX-I", "INF")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .AirService("S-I", "PAX-I", "SEG-1")
                .Package("PACKAGE", "S-A", "S-I")
                .Line("FARE", "PACKAGE", PricingComponentType.Fare, 120m, "PACKAGE", basisType: PricingBasisType.OrderItem);

            TravelerBinding[] bindings = defect switch
            {
                "missing" => [Bind("PAX-A")],
                "foreign" => [Bind("PAX-A"), Bind("PAX-Z", "CLIENT-Z", "INF")],
                "type" => [Bind("PAX-A"), Bind("PAX-I", "CLIENT-I", "ADT")],
                _ => [Bind("PAX-A", "CLIENT-A", guardian: "CLIENT-I"), Bind("PAX-I", "CLIENT-I", "INF", "CLIENT-A")]
            };

            var exception = Assert.Throws<BusinessException>(() => Accept(builder, bindings));

            Assert.Equal(20276, exception.Code);
        }

        [Fact]
        public void Infant_guardian_is_linked_to_the_same_order_traveler()
        {
            var builder = new CandidateBuilder(Now)
                .Traveler("PAX-A")
                .Traveler("PAX-I", "INF")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .AirService("S-I", "PAX-I", "SEG-1")
                .Package("PACKAGE", "S-A", "S-I")
                .Line("FARE", "PACKAGE", PricingComponentType.Fare, 120m, "PACKAGE", basisType: PricingBasisType.OrderItem);

            var order = Accept(builder, Bind("PAX-A", "CLIENT-A"), Bind("PAX-I", "CLIENT-I", "INF", "CLIENT-A"));

            var adult = order.Travelers.Single(traveler => traveler.SourceTravellerRef == "PAX-A");
            var infant = order.Travelers.Single(traveler => traveler.SourceTravellerRef == "PAX-I");
            Assert.Equal(adult.Id, infant.InfantParentTravelerId);
        }

        private static CandidateBuilder RoundTripBuilder(params CandidatePricingUnit[] units)
            => new CandidateBuilder(Now)
                .Traveler("PAX-A")
                .Segment("OUT-1")
                .Segment("IN-1")
                .AirService("S-OUT", "PAX-A", "OUT-1")
                .AirService("S-IN", "PAX-A", "IN-1")
                .Package("PACKAGE", "S-OUT", "S-IN")
                .Line("FARE", "PACKAGE", PricingComponentType.Fare, 300m, "PACKAGE", basisType: PricingBasisType.OrderItem)
                .SourceProvidedConstruction(units);

        public static Order Accept(CandidateBuilder builder, params TravelerBinding[] bindings)
        {
            var preparation = OrderPreparationTests.Capture(builder, CandidateBuilder.ReferenceProfile);

            return Order.AcceptOriginalSale(
                new AcceptOriginalSaleArgs(
                    9001,
                    "REF-TEST",
                    preparation,
                    builder.SalesScope,
                    bindings,
                    [new ContactDetails(ContactRole.Primary, "traveler@example.invalid", null)],
                    Now,
                    Now,
                    null),
                new SequentialIdGenerator());
        }

        public static TravelerBinding Bind(string sourceRef, string? clientRef = null, string passengerType = "ADT", string? guardian = null)
            => new(sourceRef, clientRef ?? $"CLIENT-{sourceRef}", "Sample", "Traveler", passengerType,
                passengerType == "INF" ? new DateOnly(2026, 1, 1) : new DateOnly(1990, 1, 1), guardian);

        private static int CountOccurrences(string text, string fragment)
            => (text.Length - text.Replace(fragment, string.Empty).Length) / fragment.Length;
    }
}
