using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderAggregate.Policies;
using Xunit;

namespace AeroTech.Ordering.Domain.Tests.OrderAggregate
{
    public sealed class PricingArithmeticTests
    {
        private const string Eur = "EUR";

        private static readonly ReversibleLine Discount = Line(PricingComponentType.Discount, PricingEffect.CustomerBalance, OrderPricingLineDirection.Credit, PricingLineRole.Original, 45m);

        [Fact]
        public void Sale_with_settlement_commission_totals_405_and_explicit_discount_reversal_totals_450()
        {
            var sale = new[]
            {
                Priced(PricingEffect.CustomerBalance, OrderPricingLineDirection.Debit, 400m),
                Priced(PricingEffect.CustomerBalance, OrderPricingLineDirection.Debit, 50m),
                Priced(PricingEffect.CustomerBalance, OrderPricingLineDirection.Credit, 45m),
                Priced(PricingEffect.SettlementOnly, OrderPricingLineDirection.Debit, 20m)
            };

            Assert.Equal(405m, PricingArithmetic.CustomerTotal(sale, Eur).Amount);

            var reversal = Line(PricingComponentType.Discount, PricingEffect.CustomerBalance, OrderPricingLineDirection.Debit, PricingLineRole.Reversal, 45m);
            PricingReversalPolicy.EnsureReversible(Discount, reversal, []);

            var afterReversal = sale.Append(Priced(reversal.Effect, reversal.Direction, reversal.SaleValue.Amount));

            Assert.Equal(450m, PricingArithmetic.CustomerTotal(afterReversal, Eur).Amount);
        }

        [Fact]
        public void Commission_as_settlement_only_is_allowed_and_as_customer_charge_is_not()
        {
            PricingLineMatrix.EnsureAllowed(PricingComponentType.Commission, PricingEffect.SettlementOnly, OrderPricingLineDirection.Debit, PricingLineRole.Original);

            Assert.Throws<BusinessException>(() =>
                PricingLineMatrix.EnsureAllowed(PricingComponentType.Commission, PricingEffect.CustomerBalance, OrderPricingLineDirection.Debit, PricingLineRole.Original));
        }

        [Fact]
        public void Reversal_beyond_the_original_or_with_same_direction_is_rejected()
        {
            var first = Line(PricingComponentType.Discount, PricingEffect.CustomerBalance, OrderPricingLineDirection.Debit, PricingLineRole.Reversal, 30m);
            var second = Line(PricingComponentType.Discount, PricingEffect.CustomerBalance, OrderPricingLineDirection.Debit, PricingLineRole.Reversal, 20m);
            var sameDirection = Line(PricingComponentType.Discount, PricingEffect.CustomerBalance, OrderPricingLineDirection.Credit, PricingLineRole.Reversal, 5m);

            PricingReversalPolicy.EnsureReversible(Discount, first, []);

            Assert.Throws<BusinessException>(() => PricingReversalPolicy.EnsureReversible(Discount, second, [first]));
            Assert.Throws<BusinessException>(() => PricingReversalPolicy.EnsureReversible(Discount, sameDirection, []));
            Assert.Throws<BusinessException>(() => PricingReversalPolicy.EnsureReversible(first, sameDirection, []));
        }

        [Fact]
        public void Allocation_attributes_value_and_never_adds_money()
        {
            var parent = new Money(100m, Eur);
            var commercial = new AllocationProposal(PricingAllocationPurpose.CommercialValue, 1, PricingAllocationMethod.SourceProvided, PricingAllocationCompleteness.Complete,
                [Share("S-1", 60m), Share("S-2", 40m)]);
            var reporting = new AllocationProposal(PricingAllocationPurpose.Reporting, 1, PricingAllocationMethod.EqualSplit, PricingAllocationCompleteness.Complete,
                [Share("S-1", 50m), Share("S-2", 50m)]);

            AllocationPolicy.EnsureConsistent(parent, parent, commercial);
            AllocationPolicy.EnsureConsistent(parent, parent, reporting);

            Assert.Equal(100m, PricingArithmetic.CustomerTotal([Priced(PricingEffect.CustomerBalance, OrderPricingLineDirection.Debit, 100m)], Eur).Amount);
            Assert.Equal(60m, AllocationPolicy.Select([commercial, reporting], PricingAllocationPurpose.CommercialValue, 1).Rows[0].SaleValue.Amount);
            Assert.Throws<BusinessException>(() => AllocationPolicy.Select([commercial, reporting], PricingAllocationPurpose.Servicing, 1));
        }

        [Fact]
        public void Complete_allocation_must_reconcile_and_partial_cannot_exceed()
        {
            var parent = new Money(100m, Eur);

            Assert.Throws<BusinessException>(() => AllocationPolicy.EnsureConsistent(parent, parent,
                new AllocationProposal(PricingAllocationPurpose.CommercialValue, 1, PricingAllocationMethod.ProRata, PricingAllocationCompleteness.Complete, [Share("S-1", 60m)])));

            AllocationPolicy.EnsureConsistent(parent, parent,
                new AllocationProposal(PricingAllocationPurpose.CommercialValue, 1, PricingAllocationMethod.ProRata, PricingAllocationCompleteness.Partial, [Share("S-1", 60m)]));

            Assert.Throws<BusinessException>(() => AllocationPolicy.EnsureConsistent(parent, parent,
                new AllocationProposal(PricingAllocationPurpose.CommercialValue, 1, PricingAllocationMethod.ProRata, PricingAllocationCompleteness.Partial, [Share("S-1", 60m), Share("S-2", 60m)])));

            Assert.Throws<BusinessException>(() => AllocationPolicy.EnsureConsistent(parent, parent,
                new AllocationProposal(PricingAllocationPurpose.CommercialValue, 1, PricingAllocationMethod.ProRata, PricingAllocationCompleteness.Unavailable, [Share("S-1", 0m)])));
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
            Assert.Equal(12.345m, PricingArithmetic.CustomerTotal([Priced(PricingEffect.CustomerBalance, OrderPricingLineDirection.Debit, 12.345m, "KWD")], "KWD").Amount);
            Assert.Equal(1500m, PricingArithmetic.CustomerTotal([Priced(PricingEffect.CustomerBalance, OrderPricingLineDirection.Debit, 1500m, "JPY")], "JPY").Amount);
            Assert.Throws<BusinessException>(() => PricingArithmetic.CustomerTotal([Priced(PricingEffect.CustomerBalance, OrderPricingLineDirection.Debit, 1m, "USD")], Eur));
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

        private static PricedAmount Priced(PricingEffect effect, OrderPricingLineDirection direction, decimal amount, string currency = Eur)
            => new(effect, direction, new Money(amount, currency));

        private static ReversibleLine Line(PricingComponentType component, PricingEffect effect, OrderPricingLineDirection direction, PricingLineRole role, decimal amount)
            => new(component, effect, direction, role, new Money(amount, Eur), new Money(amount, Eur));

        private static AllocationShare Share(string service, decimal amount)
            => new(PricingBasisType.OrderService, service, new Money(amount, Eur), new Money(amount, Eur));
    }
}
