using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderAggregate.Policies;
using Xunit;

namespace AeroTech.Ordering.Domain.Tests.OrderAggregate
{
    public sealed class PricingArithmeticTests
    {
        private const int Eur = 978;

        [Fact]
        public void Sale_with_settlement_commission_charges_the_customer_405()
        {
            var sale = new[]
            {
                Priced(PricingEffect.CustomerBalance, OrderPricingLineDirection.Debit, 400m),
                Priced(PricingEffect.CustomerBalance, OrderPricingLineDirection.Debit, 50m),
                Priced(PricingEffect.CustomerBalance, OrderPricingLineDirection.Credit, 45m),
                Priced(PricingEffect.SettlementOnly, OrderPricingLineDirection.Debit, 20m)
            };

            Assert.Equal(405m, PricingArithmetic.CustomerTotal(sale, Eur).Amount);
        }

        [Fact]
        public void Commission_as_settlement_only_is_allowed_and_as_customer_charge_is_not()
        {
            PricingLineMatrix.EnsureAllowed(PricingComponentType.Commission, PricingEffect.SettlementOnly, OrderPricingLineDirection.Debit, new SettlementAttribution("agency:77", "COMMISSION"));

            Assert.Throws<BusinessException>(() =>
                PricingLineMatrix.EnsureAllowed(PricingComponentType.Commission, PricingEffect.CustomerBalance, OrderPricingLineDirection.Debit, null));
        }

        [Theory]
        [InlineData(new[] { "400.00", "50.00" }, new[] { "45.00" }, new[] { "20.00" }, "405.00")]
        [InlineData(new[] { "400.00", "50.00", "45.00" }, new[] { "45.00" }, new[] { "20.00" }, "450.00")]
        [InlineData(new[] { "120.00", "10.00" }, new[] { "50.00" }, new string[0], "80.00")]
        [InlineData(new[] { "120.00", "30.00" }, new string[0], new string[0], "150.00")]
        [InlineData(new[] { "120.00", "150.00" }, new[] { "120.00" }, new string[0], "150.00")]
        public void Pack_pricing_vectors(string[] debits, string[] credits, string[] settlementDebits, string expected)
        {
            var lines = debits.Select(amount => Priced(PricingEffect.CustomerBalance, OrderPricingLineDirection.Debit, decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture)))
                .Concat(credits.Select(amount => Priced(PricingEffect.CustomerBalance, OrderPricingLineDirection.Credit, decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture))))
                .Concat(settlementDebits.Select(amount => Priced(PricingEffect.SettlementOnly, OrderPricingLineDirection.Debit, decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture))));

            Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture), PricingArithmetic.CustomerTotal(lines, Eur).Amount);
        }

        [Fact]
        public void Three_and_zero_decimal_currencies_keep_their_scale()
        {
            Assert.Equal(12.345m, PricingArithmetic.CustomerTotal([Priced(PricingEffect.CustomerBalance, OrderPricingLineDirection.Debit, 12.345m, 414)], 414).Amount);
            Assert.Equal(1500m, PricingArithmetic.CustomerTotal([Priced(PricingEffect.CustomerBalance, OrderPricingLineDirection.Debit, 1500m, 392)], 392).Amount);
            Assert.Throws<BusinessException>(() => PricingArithmetic.CustomerTotal([Priced(PricingEffect.CustomerBalance, OrderPricingLineDirection.Debit, 1m, 840)], Eur));
        }

        [Fact]
        public void Representation_overflow_is_rejected()
        {
            Assert.Equal(1.23456789m, DecimalRepresentation.EnsureAmount(1.23456789m, "amount"));
            Assert.Equal(0.123456789123m, DecimalRepresentation.EnsureRate(0.123456789123m, "rate"));
            Assert.Throws<BusinessException>(() => DecimalRepresentation.EnsureAmount(1.234567891m, "amount"));
            Assert.Throws<BusinessException>(() => DecimalRepresentation.EnsureAmount(100000000000000000000m, "amount"));
            Assert.Equal(99999999999999999999.99999999m, DecimalRepresentation.EnsureAmount(99999999999999999999.99999999m, "amount"));
            Assert.Equal(100.00000000m, DecimalRepresentation.EnsureAmount(100.00000000m, "amount"));
        }

        private static PricedAmount Priced(PricingEffect effect, OrderPricingLineDirection direction, decimal amount, int currency = Eur)
            => new(effect, direction, new Money(amount, currency));
    }
}
