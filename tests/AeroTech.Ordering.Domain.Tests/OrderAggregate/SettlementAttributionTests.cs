using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderAggregate.Policies;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Policies;
using AeroTech.Ordering.Domain.Tests._Shared;
using Xunit;

namespace AeroTech.Ordering.Domain.Tests.OrderAggregate
{
    public sealed class SettlementAttributionTests
    {
        private const int SettlementAttributionIncomplete = 20290;
        private const int SettlementAttributionNotAllowed = 20291;
        private const int SettlementAttributionRequired = 20292;

        private static readonly DateTimeOffset Now = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

        private static readonly SettlementAttribution AgencyCommission = new("agency:77", "COMMISSION");

        [Fact]
        public void SC_S1_013_amount_based_agency_commission_leaves_the_customer_total_at_405()
        {
            var order = AcceptOriginalSaleTests.Accept(CommissionSale(), AcceptOriginalSaleTests.Bind("PAX-A"));

            Assert.Equal(405.00m, order.CustomerTotal.Amount);

            var commission = order.PricingLines.Single(line => line.Component == PricingComponentType.Commission);

            Assert.Equal(PricingEffect.SettlementOnly, commission.Effect);
            Assert.Equal(20.00m, commission.SaleValue.Amount);
            Assert.Equal("agency:77", commission.SettlementAttribution!.PartyRef);
            Assert.Equal("COMMISSION", commission.SettlementAttribution.CategoryCode);
            Assert.Equal(commission.SaleValue.CurrencyRef, order.SaleCurrency.CurrencyRef);
        }

        [Fact]
        public void A_settlement_only_line_without_attribution_is_rejected()
        {
            var builder = CommissionSale(withAttribution: false);

            var exception = Assert.Throws<BusinessException>(() => CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope));

            Assert.Equal(SettlementAttributionRequired, exception.Code);
        }

        [Fact]
        public void A_settlement_only_line_with_only_one_attribution_member_cannot_be_expressed()
        {
            Assert.Equal(SettlementAttributionIncomplete, Assert.Throws<BusinessException>(() => new SettlementAttribution("agency:77", "  ")).Code);
            Assert.Equal(SettlementAttributionIncomplete, Assert.Throws<BusinessException>(() => new SettlementAttribution("", "COMMISSION")).Code);
        }

        [Fact]
        public void A_customer_balance_line_cannot_carry_settlement_attribution()
        {
            var builder = new CandidateBuilder(Now)
                .Traveler("PAX-A")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .Package("ITEM-A", "S-A")
                .Line("FARE", "ITEM-A", PricingComponentType.Fare, 100m, "S-A", settlementAttribution: AgencyCommission);

            var exception = Assert.Throws<BusinessException>(() => CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope));

            Assert.Equal(SettlementAttributionNotAllowed, exception.Code);
        }

        [Fact]
        public void A_non_commission_settlement_line_proves_the_model_is_generic()
        {
            var builder = new CandidateBuilder(Now)
                .Traveler("PAX-A")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .Package("ITEM-A", "S-A")
                .Line("FARE", "ITEM-A", PricingComponentType.Fare, 100m, "S-A")
                .Line("PARTNER-FEE", "ITEM-A", PricingComponentType.Fee, 15m, "S-A",
                    effect: PricingEffect.SettlementOnly,
                    settlementAttribution: new SettlementAttribution("partner:consolidator-9", "PARTNER-FEE"));

            var order = AcceptOriginalSaleTests.Accept(builder, AcceptOriginalSaleTests.Bind("PAX-A"));

            var settlement = order.PricingLines.Single(line => line.Effect == PricingEffect.SettlementOnly);

            Assert.Equal(PricingComponentType.Fee, settlement.Component);
            Assert.Equal("partner:consolidator-9", settlement.SettlementAttribution!.PartyRef);
            Assert.Equal("PARTNER-FEE", settlement.SettlementAttribution.CategoryCode);
            Assert.Equal(100.00m, order.CustomerTotal.Amount);
        }

        [Fact]
        public void Two_settlement_counterparties_stay_distinct()
        {
            var builder = new CandidateBuilder(Now)
                .Traveler("PAX-A")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .Package("ITEM-A", "S-A")
                .Line("FARE", "ITEM-A", PricingComponentType.Fare, 100m, "S-A")
                .Line("COMM-A", "ITEM-A", PricingComponentType.Commission, 10m, "S-A",
                    effect: PricingEffect.SettlementOnly,
                    settlementAttribution: new SettlementAttribution("agency:77", "COMMISSION"))
                .Line("COMM-B", "ITEM-A", PricingComponentType.Commission, 5m, "S-A",
                    effect: PricingEffect.SettlementOnly,
                    settlementAttribution: new SettlementAttribution("agency:88", "OVERRIDE"));

            var order = AcceptOriginalSaleTests.Accept(builder, AcceptOriginalSaleTests.Bind("PAX-A"));

            var parties = order.PricingLines
                .Where(line => line.Effect == PricingEffect.SettlementOnly)
                .Select(line => line.SettlementAttribution!.PartyRef)
                .OrderBy(party => party, StringComparer.Ordinal)
                .ToList();

            Assert.Equal(["agency:77", "agency:88"], parties);
            Assert.Equal(100.00m, order.CustomerTotal.Amount);
        }

        [Theory]
        [InlineData("IATA-COMM-STD")]
        [InlineData("bsp/2026/override")]
        [InlineData("x")]
        public void An_arbitrary_category_code_round_trips_without_interpretation(string categoryCode)
        {
            var order = AcceptOriginalSaleTests.Accept(
                CommissionSale(attribution: new SettlementAttribution("agency:77", categoryCode)),
                AcceptOriginalSaleTests.Bind("PAX-A"));

            var commission = order.PricingLines.Single(line => line.Component == PricingComponentType.Commission);

            Assert.Equal(categoryCode, commission.SettlementAttribution!.CategoryCode);
        }

        [Fact]
        public void Commission_cannot_be_a_customer_charge_even_with_attribution()
        {
            Assert.Throws<BusinessException>(() => PricingLineMatrix.EnsureAllowed(
                PricingComponentType.Commission,
                PricingEffect.CustomerBalance,
                OrderPricingLineDirection.Debit,
                PricingLineRole.Original,
                AgencyCommission));
        }

        internal static CandidateBuilder CommissionSale(SettlementAttribution? attribution = null, bool withAttribution = true)
            => new CandidateBuilder(Now)
                .Traveler("PAX-A")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .Package("ITEM-A", "S-A")
                .Line("FARE", "ITEM-A", PricingComponentType.Fare, 400m, "S-A")
                .Line("BAG", "ITEM-A", PricingComponentType.Fee, 50m, "S-A")
                .Line("PROMO", "ITEM-A", PricingComponentType.Discount, 45m, "S-A")
                .Line("COMM", "ITEM-A", PricingComponentType.Commission, 20m, "S-A",
                    effect: PricingEffect.SettlementOnly,
                    settlementAttribution: withAttribution ? attribution ?? AgencyCommission : null);
    }
}
