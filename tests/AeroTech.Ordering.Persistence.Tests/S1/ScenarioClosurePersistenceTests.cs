using System.Net.Http;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;
using AeroTech.Ordering.Domain.Tests._Shared;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Persistence.Tests._Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class ScenarioClosurePersistenceTests
    {
        private const int ContractMismatch = 20272;

        private readonly OrderingDatabaseFixture _fixture;

        public ScenarioClosurePersistenceTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task SC_01_two_independently_priced_items_survive_acceptance_and_sql()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, (now, scope) => new CandidateBuilder(now, scope)
                .Traveller("PAX-A")
                .Segment("SEG-1")
                .Segment("SEG-2")
                .AirService("S-1", "PAX-A", "SEG-1")
                .AirService("S-2", "PAX-A", "SEG-2")
                .Package("ITEM-1", "S-1")
                .Package("ITEM-2", "S-2")
                .Line("out/fare", "ITEM-1", PricingComponentType.Fare, 100m, "S-1")
                .Line("in/fare", "ITEM-2", PricingComponentType.Fare, 140m, "S-2"));

            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);

            Assert.Equal(2, order.Items.Count);
            Assert.Equal([100m, 140m], order.Items.Select(item => item.AcceptedTotal.Amount).Order());
            Assert.Equal(240m, order.CustomerTotal.Amount);
            Assert.Equal(2, order.Services.Select(service => service.OrderItemId).Distinct().Count());
            Assert.Equal(2, order.ItemServiceLinks.Count);
        }

        [Fact]
        public async Task SC_02_an_offer_package_and_a_product_item_keep_their_kinds_in_sql()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, (now, scope) => new CandidateBuilder(now, scope)
                .Traveller("PAX-A")
                .Segment("SEG-1")
                .Segment("SEG-2")
                .AirService("S-AIR", "PAX-A", "SEG-1")
                .AirService("S-EXTRA", "PAX-A", "SEG-2")
                .Package("ITEM-AIR", "S-AIR")
                .Item(new CandidateItem("ITEM-PRODUCT", OrderItemKind.Product, ["S-EXTRA"], Zero))
                .Line("air/fare", "ITEM-AIR", PricingComponentType.Fare, 100m, "S-AIR")
                .Line("product/charge", "ITEM-PRODUCT", PricingComponentType.ProductCharge, 35m, "S-EXTRA"));

            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);

            Assert.Equal(
                [OrderItemKind.OfferPackage, OrderItemKind.Product],
                order.Items.OrderBy(item => item.Kind).Select(item => item.Kind));
            Assert.Equal(135m, order.CustomerTotal.Amount);
        }

        [Fact]
        public async Task SC_03_a_fee_only_item_is_persisted_with_no_service_and_its_own_total()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, (now, scope) => new CandidateBuilder(now, scope)
                .Traveller("PAX-A")
                .Segment("SEG-1")
                .AirService("S-1", "PAX-A", "SEG-1")
                .Package("ITEM-AIR", "S-1")
                .Item(new CandidateItem("ITEM-FEE", OrderItemKind.MonetaryCharge, [], Zero))
                .Line("air/fare", "ITEM-AIR", PricingComponentType.Fare, 100m, "S-1")
                .Line("service/fee", "ITEM-FEE", PricingComponentType.Fee, 25m, "ITEM-FEE", basisType: PricingBasisType.OrderItem));

            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);
            var fee = order.Items.Single(item => item.Kind == OrderItemKind.MonetaryCharge);

            Assert.Equal(25m, fee.AcceptedTotal.Amount);
            Assert.DoesNotContain(order.Services, service => service.OrderItemId == fee.Id);
            Assert.Single(order.Services);
            Assert.Equal(125m, order.CustomerTotal.Amount);
        }

        [Fact]
        public async Task SC_07_a_construction_spanning_two_items_keeps_both_item_rows()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, (now, scope) => new CandidateBuilder(now, scope)
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
                .ConstructionFor("ITEM-OUT", "ITEM-IN"));

            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);
            var construction = Assert.Single(order.FareConstructions);

            Assert.Equal(2, construction.Items.Count);
            Assert.Equal(
                order.Items.Select(item => item.Id).Order(),
                construction.Items.Select(item => item.OrderItemId).Order());
            Assert.Equal(FarePricingUnitType.RoundTrip, Assert.Single(construction.PricingUnits).Type);
        }

        [Theory]
        [InlineData(PricingComponentType.CarrierSurcharge, 30, 150)]
        [InlineData(PricingComponentType.Fee, 15, 135)]
        [InlineData(PricingComponentType.Markup, 12, 132)]
        [InlineData(PricingComponentType.Penalty, 40, 160)]
        public async Task SC_26_28_a_debit_component_is_persisted_as_its_own_line_and_component_total(
            PricingComponentType component,
            decimal amount,
            decimal expectedTotal)
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, (now, scope) =>
                CandidateBuilder.OneWayFare100Tax20(now, scope)
                    .Line("extra", "ITEM-A", component, amount, "S-A"));

            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);
            var line = order.PricingLines.Single(candidate => candidate.SourceOccurrencePath == "extra");
            var total = order.ComponentTotals.Single(candidate => candidate.Component == component);

            Assert.Equal(component, line.Component);
            Assert.Equal(OrderPricingLineDirection.Debit, line.Direction);
            Assert.Equal(amount, line.SaleValue.Amount);
            Assert.Equal(amount, total.DebitAmount);
            Assert.Equal(0m, total.CreditAmount);
            Assert.Equal(expectedTotal, order.CustomerTotal.Amount);
        }

        [Fact]
        public async Task SC_27_a_discount_credit_is_persisted_on_the_credit_side_of_its_component_total()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, (now, scope) =>
                CandidateBuilder.OneWayFare100Tax20(now, scope)
                    .Line("promo", "ITEM-A", PricingComponentType.Discount, 30m, "S-A"));

            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);
            var line = order.PricingLines.Single(candidate => candidate.SourceOccurrencePath == "promo");
            var total = order.ComponentTotals.Single(candidate => candidate.Component == PricingComponentType.Discount);

            Assert.Equal(OrderPricingLineDirection.Credit, line.Direction);
            Assert.Equal(30m, line.SaleValue.Amount);
            Assert.Equal(0m, total.DebitAmount);
            Assert.Equal(30m, total.CreditAmount);
            Assert.Equal(90m, order.CustomerTotal.Amount);
            Assert.Equal(90m, Assert.Single(order.Items).AcceptedTotal.Amount);
        }

        [Fact]
        public async Task SC_29_an_adjustment_debit_and_credit_are_kept_on_opposite_sides_of_one_component_total()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, (now, scope) =>
                CandidateBuilder.OneWayFare100Tax20(now, scope)
                    .Line("adjust/up", "ITEM-A", PricingComponentType.Adjustment, 25m, "S-A", direction: OrderPricingLineDirection.Debit)
                    .Line("adjust/down", "ITEM-A", PricingComponentType.Adjustment, 10m, "S-A", direction: OrderPricingLineDirection.Credit));

            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);
            var total = order.ComponentTotals.Single(candidate => candidate.Component == PricingComponentType.Adjustment);

            Assert.Equal(25m, total.DebitAmount);
            Assert.Equal(10m, total.CreditAmount);
            Assert.Equal(15m, total.Net);
            Assert.Equal(135m, order.CustomerTotal.Amount);
        }

        [Fact]
        public async Task SC_31_a_zero_value_line_is_persisted_as_a_line_not_dropped()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, (now, scope) =>
                CandidateBuilder.OneWayFare100Tax20(now, scope)
                    .Line("waived", "ITEM-A", PricingComponentType.Fee, 0m, "S-A"));

            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);

            Assert.Equal(0m, order.PricingLines.Single(line => line.SourceOccurrencePath == "waived").SaleValue.Amount);
            Assert.Equal(120m, order.CustomerTotal.Amount);
            Assert.Contains(order.ComponentTotals, total => total.Component == PricingComponentType.Fee);
        }

        [Fact]
        public async Task SC_10_the_financial_customer_the_actor_and_the_traveller_stay_three_separate_facts_in_sql()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);

            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);
            var traveller = Assert.Single(order.Travellers);

            Assert.Equal(S1Harness.CustomerId, order.FinancialCustomerId);
            Assert.Equal(S1Harness.AirlineUserId, order.InitiatingActor.ActorId);
            Assert.NotEqual(order.FinancialCustomerId, order.InitiatingActor.ActorId);
            Assert.NotEqual(order.FinancialCustomerId, traveller.Id);
            Assert.NotEqual(order.SalesContext.SellerId, order.FinancialCustomerId);
            Assert.False(order.Buyer.IsSupplied);
        }

        [Fact]
        public async Task SC_54_a_blank_responded_offer_identity_is_a_contract_mismatch()
        {
            var handler = new AirOfferWireFixtures.StubHandler(() => AirOfferWireFixtures.Details(offerId: " "));
            await using var harness = await StartAsync(handler);

            var failure = await Assert.ThrowsAsync<BusinessException>(
                () => harness.SendAsync(S1Commands.Backoffice("SYNTHETIC-PRICED-OFFER", travellers: S1Commands.Travellers("T1"))));

            Assert.Equal(ContractMismatch, failure.Code);
        }

        [Fact]
        public async Task SC_55_a_mismatched_responded_offer_identity_is_a_contract_mismatch_naming_both_identities()
        {
            var handler = new AirOfferWireFixtures.StubHandler(() => AirOfferWireFixtures.Details(offerId: "SOME-OTHER-OFFER"));
            await using var harness = await StartAsync(handler);

            var failure = await Assert.ThrowsAsync<BusinessException>(
                () => harness.SendAsync(S1Commands.Backoffice("SYNTHETIC-PRICED-OFFER", travellers: S1Commands.Travellers("T1"))));

            Assert.Equal(ContractMismatch, failure.Code);
            Assert.Contains("SOME-OTHER-OFFER", failure.Message);
            Assert.Contains("SYNTHETIC-PRICED-OFFER", failure.Message);
        }

        private static Money Zero => new(0m, CandidateBuilder.SaleCurrencyId);

        private Task<S1Harness> StartAsync(HttpMessageHandler handler)
            => S1Harness.StartAsync(_fixture, services =>
                services.ConfigureHttpClientDefaults(client => client.ConfigurePrimaryHttpMessageHandler(() => handler)),
                useReferenceOffers: false);
    }
}
