using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderAggregate;
using AeroTech.Ordering.Domain.OrderAggregate.Arguments;
using AeroTech.Ordering.Domain.OrderAggregate.DomainEvents;
using AeroTech.Ordering.Domain.OrderAggregate.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Arguments;
using AeroTech.Ordering.Domain.Ports.Offers;
using AeroTech.Ordering.Domain.Tests._Shared;
using Xunit;

namespace AeroTech.Ordering.Domain.Tests.OrderAggregate
{
    public sealed class AcceptOriginalSaleTests
    {
        private const int AirServiceScopeInvalid = 20293;
        private const int TravellerBindingInvalid = 20276;

        private static readonly DateTimeOffset Now = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

        [Fact]
        public void One_traveller_one_segment_package_creates_one_item_one_service_total_120_versions_1()
        {
            var order = Accept(CandidateBuilder.OneWayFare100Tax20(Now), Bind("PAX-A"));

            Assert.Single(order.Items);
            Assert.Single(order.Services);
            Assert.Equal(120.00m, order.CustomerTotal.Amount);
            Assert.Equal(CandidateBuilder.SaleCurrencyId, order.CustomerTotal.CurrencyId);
            Assert.Equal(CandidateBuilder.SaleCurrencyId, order.CurrencyId);
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
        public void An_accepted_service_is_bound_to_exactly_one_traveller_and_one_segment()
        {
            var order = Accept(CandidateBuilder.OneWayFare100Tax20(Now), Bind("PAX-A"));

            var service = order.Services.Single();

            Assert.Equal(order.Travellers.Single().Id, service.TravellerId);
            Assert.Equal(order.Segments.Single().Id, service.SegmentId);
            Assert.True(service.TravellerId > 0);
            Assert.True(service.SegmentId > 0);
        }

        [Fact]
        public void Technical_stop_keeps_one_service_and_two_legs()
        {
            var builder = new CandidateBuilder(Now)
                .Traveller("PAX-A")
                .Segment("SEG-A", 9101L, 9102L)
                .AirService("SERVICE-A", "PAX-A", "SEG-A")
                .Package("ITEM-A", "SERVICE-A")
                .Line("fare", "ITEM-A", PricingComponentType.Fare, 100m, "SERVICE-A");

            var order = Accept(builder, Bind("PAX-A"));

            Assert.Single(order.Services);
            Assert.Equal([9101L, 9102L], order.Segments.Single().Legs.OrderBy(leg => leg.Sequence).Select(leg => leg.LegId));
        }

        [Fact]
        public void Source_priced_package_for_two_travellers_and_two_segments_is_one_item_with_four_services()
        {
            var builder = new CandidateBuilder(Now)
                .Traveller("PAX-A")
                .Traveller("PAX-B")
                .Segment("SEG-1")
                .Segment("SEG-2")
                .AirService("S-A1", "PAX-A", "SEG-1")
                .AirService("S-A2", "PAX-A", "SEG-2")
                .AirService("S-B1", "PAX-B", "SEG-1")
                .AirService("S-B2", "PAX-B", "SEG-2")
                .Package("PACKAGE", "S-A1", "S-A2", "S-B1", "S-B2")
                .Line("fare", "PACKAGE", PricingComponentType.Fare, 400m, "PACKAGE", basisType: PricingBasisType.OrderItem);

            var order = Accept(builder, Bind("PAX-A"), Bind("PAX-B"));

            Assert.Single(order.Items);
            Assert.Equal(4, order.Services.Count);
            Assert.Equal(2, order.Travellers.Count);
            Assert.Equal(2, order.Segments.Count);
            Assert.Equal(400.00m, order.CustomerTotal.Amount);
        }

        [Fact]
        public void The_source_flight_sequence_is_preserved_per_bound()
        {
            var builder = new CandidateBuilder(Now)
                .Traveller("PAX-A")
                .Journey("OUT")
                .Segment("OUT-1")
                .Segment("OUT-2")
                .Journey("IN", BoundDirection.Inbound)
                .Segment("IN-1")
                .AirService("S-1", "PAX-A", "OUT-1")
                .AirService("S-2", "PAX-A", "OUT-2")
                .AirService("S-3", "PAX-A", "IN-1")
                .Package("PACKAGE", "S-1", "S-2", "S-3")
                .Line("fare", "PACKAGE", PricingComponentType.Fare, 300m, "PACKAGE", basisType: PricingBasisType.OrderItem)
                .JourneyKind(JourneyType.RoundTrip);

            var order = Accept(builder, Bind("PAX-A"));

            var outbound = order.Journeys.Single(journey => journey.BoundId == "OUT");
            var inbound = order.Journeys.Single(journey => journey.BoundId == "IN");

            Assert.Equal(BoundDirection.Outbound, outbound.Direction);
            Assert.Equal(BoundDirection.Inbound, inbound.Direction);
            Assert.Equal(JourneyType.RoundTrip, order.JourneyType);
            Assert.Equal([1, 2], order.Segments.Where(segment => segment.JourneyId == outbound.Id).OrderBy(segment => segment.Sequence).Select(segment => segment.Sequence));
            Assert.Equal([1], order.Segments.Where(segment => segment.JourneyId == inbound.Id).Select(segment => segment.Sequence));
        }

        [Fact]
        public void Sale_equivalent_is_the_customer_value_and_the_original_currency_is_retained()
        {
            var builder = new CandidateBuilder(Now)
                .Traveller("PAX-A")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .Package("ITEM-A", "S-A")
                .Line("fare", "ITEM-A", PricingComponentType.Fare, 92m, "S-A",
                    original: new Money(100m, 840),
                    appliedConversion: new AppliedConversion("ROE-1", 840, CandidateBuilder.SaleCurrencyId, 0.92m, 6, null));

            var order = Accept(builder, Bind("PAX-A"));
            var line = order.PricingLines.Single();

            Assert.Equal(92m, line.SaleValue.Amount);
            Assert.Equal(CandidateBuilder.SaleCurrencyId, line.SaleValue.CurrencyId);
            Assert.Equal(100m, line.OriginalValue.Amount);
            Assert.Equal(840, line.OriginalValue.CurrencyId);
            Assert.Equal(840, line.AppliedConversion!.FromCurrencyId);
            Assert.Equal(CandidateBuilder.SaleCurrencyId, line.AppliedConversion.ToCurrencyId);
            Assert.Equal(92m, order.CustomerTotal.Amount);
        }

        [Fact]
        public void Repeated_source_traveller_binding_is_rejected_before_any_order()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now);

            var exception = Assert.Throws<BusinessException>(() => Accept(builder, Bind("PAX-A"), Bind("PAX-A", "CLIENT-B")));

            Assert.Equal(TravellerBindingInvalid, exception.Code);
        }

        [Theory]
        [InlineData("missing")]
        [InlineData("foreign")]
        [InlineData("type")]
        public void Incomplete_or_inconsistent_traveller_binding_is_rejected(string defect)
        {
            var builder = new CandidateBuilder(Now)
                .Traveller("PAX-A")
                .Traveller("PAX-I", PassengerTypeCode.CHD)
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .AirService("S-I", "PAX-I", "SEG-1")
                .Package("PACKAGE", "S-A", "S-I")
                .Line("fare", "PACKAGE", PricingComponentType.Fare, 120m, "PACKAGE", basisType: PricingBasisType.OrderItem);

            TravellerBinding[] bindings = defect switch
            {
                "missing" => [Bind("PAX-A")],
                "foreign" => [Bind("PAX-A"), Bind("PAX-Z", "CLIENT-Z", PassengerTypeCode.CHD)],
                _ => [Bind("PAX-A"), Bind("PAX-I", "CLIENT-I", PassengerTypeCode.ADT)]
            };

            var exception = Assert.Throws<BusinessException>(() => Accept(builder, bindings));

            Assert.Equal(TravellerBindingInvalid, exception.Code);
        }

        [Fact]
        public void Infant_guardian_is_linked_to_the_same_order_traveller()
        {
            var builder = new CandidateBuilder(Now)
                .Traveller("PAX-A")
                .Traveller("PAX-I", PassengerTypeCode.INF)
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .AirService("S-I", "PAX-I", "SEG-1")
                .Package("PACKAGE", "S-A", "S-I")
                .Line("fare", "PACKAGE", PricingComponentType.Fare, 120m, "PACKAGE", basisType: PricingBasisType.OrderItem);

            var order = Accept(builder, Bind("PAX-A", "CLIENT-A"), Bind("PAX-I", "CLIENT-I", PassengerTypeCode.INF, "CLIENT-A"));

            var adult = order.Travellers.Single(traveller => traveller.SourceTravellerRef == "PAX-A");
            var infant = order.Travellers.Single(traveller => traveller.SourceTravellerRef == "PAX-I");

            Assert.Equal(adult.Id, infant.InfantParentTravellerId);
        }

        [Fact]
        public void An_accepted_order_records_its_source_and_sales_provenance()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now);
            var order = Accept(builder, Bind("PAX-A"));

            Assert.Equal(builder.Build().Source.OfferId, order.SourceOfferId);
            Assert.Equal(64, order.AcceptedSnapshotDigest.Length);
            Assert.Equal(order.Id, order.RootOrderId);
            Assert.Equal(builder.SalesScope.SalesContext, order.SalesContext);
            Assert.Equal(builder.SalesScope.InitiatingActor, order.InitiatingActor);
        }

        public static Order Accept(CandidateBuilder builder, params TravellerBinding[] bindings)
        {
            var preparation = CapturePreparation(builder);

            return Order.AcceptOriginalSale(
                new AcceptOriginalSaleArgs(
                    9001,
                    "REF-TEST",
                    preparation,
                    builder.SalesScope,
                    bindings,
                    [new ContactDetails(ContactRole.Primary, "traveller@example.invalid", null)],
                    Now,
                    Now,
                    null),
                new SequentialIdGenerator());
        }

        internal static OrderPreparation CapturePreparation(CandidateBuilder builder)
            => OrderPreparation.Capture(new CaptureOrderPreparationArgs(
                7001,
                7002,
                builder.SalesScope,
                builder.Build(),
                new OfferSourceProfile(CandidateBuilder.ReferenceProfile, "1.0", CandidateBuilder.ReferenceProfile),
                new SourceEvidence("evidence", new string('a', 64), "application/json", "{}"),
                Now));

        public static TravellerBinding Bind(string sourceRef, string? clientRef = null, PassengerTypeCode passengerType = PassengerTypeCode.ADT, string? guardian = null)
            => new(sourceRef, clientRef ?? $"CLIENT-{sourceRef}", "Sample", "Traveller", passengerType,
                passengerType == PassengerTypeCode.INF ? new DateOnly(2026, 1, 1) : new DateOnly(1990, 1, 1), guardian);
    }
}
