using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderAggregate;
using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Domain.Tests._Shared;
using Xunit;

namespace AeroTech.Ordering.Domain.Tests.OrderAggregate
{
    public sealed class ComponentTotalTests
    {
        private static readonly DateTimeOffset Now = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

        [Fact]
        public void Fare_tax_fee_surcharge_and_discount_each_get_their_own_total()
        {
            var order = AcceptOriginalSaleTests.Accept(MixedSale(), AcceptOriginalSaleTests.Bind("PAX-A"));

            Assert.Equal(400.00m, Net(order, PricingComponentType.Fare));
            Assert.Equal(20.00m, Net(order, PricingComponentType.Tax));
            Assert.Equal(50.00m, Net(order, PricingComponentType.Fee));
            Assert.Equal(30.00m, Net(order, PricingComponentType.CarrierSurcharge));
            Assert.Equal(-45.00m, Net(order, PricingComponentType.Discount));
        }

        [Fact]
        public void A_component_with_both_debit_and_credit_lines_keeps_the_two_magnitudes_apart()
        {
            var builder = new CandidateBuilder(Now)
                .Traveler("PAX-A")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .Package("ITEM-A", "S-A")
                .Line("FARE", "ITEM-A", PricingComponentType.Fare, 300m, "S-A")
                .Line("ADJ-UP", "ITEM-A", PricingComponentType.Adjustment, 40m, "S-A", direction: OrderPricingLineDirection.Debit)
                .Line("ADJ-DOWN", "ITEM-A", PricingComponentType.Adjustment, 15m, "S-A", direction: OrderPricingLineDirection.Credit);

            var order = AcceptOriginalSaleTests.Accept(builder, AcceptOriginalSaleTests.Bind("PAX-A"));
            var adjustment = Total(order, PricingComponentType.Adjustment);

            Assert.Equal(40.00m, adjustment.DebitAmount);
            Assert.Equal(15.00m, adjustment.CreditAmount);
            Assert.Equal(25.00m, adjustment.Net);
            Assert.Equal(325.00m, order.CustomerTotal.Amount);
        }

        [Fact]
        public void A_settlement_component_is_totalled_but_stays_outside_the_customer_total()
        {
            var order = AcceptOriginalSaleTests.Accept(
                SettlementAttributionTests.CommissionSale(),
                AcceptOriginalSaleTests.Bind("PAX-A"));

            var commission = Total(order, PricingComponentType.Commission);

            Assert.Equal(PricingEffect.SettlementOnly, commission.Effect);
            Assert.Equal(20.00m, commission.DebitAmount);
            Assert.Equal(405.00m, order.CustomerTotal.Amount);
            Assert.DoesNotContain(order.ComponentTotals, total => total.Effect == PricingEffect.CustomerBalance && total.Component == PricingComponentType.Commission);
        }

        [Fact]
        public void An_informational_component_is_totalled_but_never_payable()
        {
            var builder = new CandidateBuilder(Now)
                .Traveler("PAX-A")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .Package("ITEM-A", "S-A")
                .Line("FARE", "ITEM-A", PricingComponentType.Fare, 100m, "S-A")
                .Line("NOTE", "ITEM-A", PricingComponentType.Other, 7m, "S-A", effect: PricingEffect.Informational);

            var order = AcceptOriginalSaleTests.Accept(builder, AcceptOriginalSaleTests.Bind("PAX-A"));

            Assert.Equal(7.00m, Total(order, PricingComponentType.Other).DebitAmount);
            Assert.Equal(100.00m, order.CustomerTotal.Amount);
        }

        [Fact]
        public void Every_total_is_keyed_by_component_and_effect_in_the_sale_currency()
        {
            var order = AcceptOriginalSaleTests.Accept(MixedSale(), AcceptOriginalSaleTests.Bind("PAX-A"));

            Assert.All(order.ComponentTotals, total => Assert.Equal(order.SaleCurrency.CurrencyRef, total.CurrencyRef));
            Assert.All(order.ComponentTotals, total => Assert.True(total.DebitAmount >= 0 && total.CreditAmount >= 0));
            Assert.Equal(
                order.ComponentTotals.Select(total => (total.Component, total.Effect)).Distinct().Count(),
                order.ComponentTotals.Count);
        }

        [Fact]
        public void Component_totals_reconcile_with_the_committed_lines()
        {
            var order = AcceptOriginalSaleTests.Accept(MixedSale(), AcceptOriginalSaleTests.Bind("PAX-A"));

            foreach (var total in order.ComponentTotals)
            {
                var lines = order.PricingLines.Where(line => line.Component == total.Component && line.Effect == total.Effect).ToList();

                Assert.Equal(lines.Where(line => line.Direction == OrderPricingLineDirection.Debit).Sum(line => line.SaleValue.Amount), total.DebitAmount);
                Assert.Equal(lines.Where(line => line.Direction == OrderPricingLineDirection.Credit).Sum(line => line.SaleValue.Amount), total.CreditAmount);
            }

            var customerNet = order.ComponentTotals
                .Where(total => total.Effect == PricingEffect.CustomerBalance)
                .Sum(total => total.Net);

            Assert.Equal(order.CustomerTotal.Amount, customerNet);
        }

        private static decimal Net(Order order, PricingComponentType component) => Total(order, component).Net;

        private static OrderComponentTotal Total(Order order, PricingComponentType component)
            => order.ComponentTotals.Single(total => total.Component == component);

        private static CandidateBuilder MixedSale()
            => new CandidateBuilder(Now)
                .Traveler("PAX-A")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .Package("ITEM-A", "S-A")
                .Line("FARE", "ITEM-A", PricingComponentType.Fare, 400m, "S-A")
                .Line("TAX", "ITEM-A", PricingComponentType.Tax, 20m, "S-A")
                .Line("FEE", "ITEM-A", PricingComponentType.Fee, 50m, "S-A")
                .Line("YQ", "ITEM-A", PricingComponentType.CarrierSurcharge, 30m, "S-A")
                .Line("PROMO", "ITEM-A", PricingComponentType.Discount, 45m, "S-A");
    }
}
