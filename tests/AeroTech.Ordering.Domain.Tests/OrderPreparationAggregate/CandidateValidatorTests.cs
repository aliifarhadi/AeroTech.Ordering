using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Policies;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Serialization;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;
using AeroTech.Ordering.Domain.Tests._Shared;
using Xunit;

namespace AeroTech.Ordering.Domain.Tests.OrderPreparationAggregate
{
    public sealed class CandidateValidatorTests
    {
        private const int ContractMismatch = 20272;
        private const int PricingRuleViolated = 20279;
        private const int RepresentationOverflow = 20278;

        private static readonly DateTimeOffset Now = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

        [Fact]
        public void Pack_one_way_reference_candidate_is_valid()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now);
            var candidate = builder.Build();

            CandidateValidator.EnsureValid(candidate, builder.SalesScope);

            Assert.Equal(120m, candidate.CustomerTotal.Amount);
            Assert.Equal(CandidateBuilder.SaleCurrencyId, candidate.CustomerTotal.CurrencyId);
            Assert.Single(candidate.Items);
            Assert.Single(candidate.Services);
        }

        [Fact]
        public void Pack_incorrect_total_is_rejected_against_its_pricing_lines()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now).CustomerTotal(119m);

            AssertMessage("differs from the pricing lines", () => CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope));
        }

        [Fact]
        public void Pack_settlement_tax_is_rejected_by_the_component_matrix()
        {
            var builder = new CandidateBuilder(Now)
                .Traveller("PAX-A")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .Package("ITEM-A", "S-A")
                .Line("fare", "ITEM-A", PricingComponentType.Fare, 100m, "S-A")
                .Line("tax", "ITEM-A", PricingComponentType.Tax, 20m, "S-A",
                    effect: PricingEffect.SettlementOnly,
                    settlementAttribution: new SettlementAttribution("agency:77", "TAX"));

            var exception = Assert.Throws<BusinessException>(() => CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope));

            Assert.Equal(PricingRuleViolated, exception.Code);
            Assert.Contains("Tax cannot be settlement-only", exception.Message);
        }

        [Fact]
        public void Pack_missing_beneficiary_is_rejected_at_the_candidate_boundary()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now);
            var candidate = builder.Build();
            var orphaned = candidate with
            {
                Services = [candidate.Services[0] with { TravellerRef = "PAX-UNKNOWN" }]
            };

            AssertMessage("is not a candidate traveller", () => CandidateValidator.EnsureValid(orphaned, builder.SalesScope));
        }

        [Fact]
        public void A_candidate_service_that_names_no_traveller_cannot_be_read()
        {
            var candidate = NormalizedCandidateJson.Write(CandidateBuilder.OneWayFare100Tax20(Now).Build());
            var withoutTraveller = candidate.Replace(",\"travellerRef\":\"PAX-A\"}", "}");

            Assert.NotEqual(candidate, withoutTraveller);

            var exception = Assert.Throws<BusinessException>(() => NormalizedCandidateJson.Read(withoutTraveller));

            Assert.Equal(ContractMismatch, exception.Code);
            Assert.Contains("travellerRef", exception.Message);
        }

        [Fact]
        public void An_air_service_cannot_cover_two_segments()
        {
            var builder = new CandidateBuilder(Now)
                .Traveller("PAX-A")
                .Segment("SEG-A")
                .Segment("SEG-B")
                .AirService("SERVICE-A", "PAX-A", "SEG-A")
                .AirService("SERVICE-B", "PAX-A", "SEG-A")
                .Package("ITEM-A", "SERVICE-A", "SERVICE-B")
                .Line("fare", "ITEM-A", PricingComponentType.Fare, 100m, "SERVICE-A");

            AssertMessage("two air services on segment", () => CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope));
        }

        [Fact]
        public void Candidate_for_another_customer_is_rejected()
        {
            var candidate = CandidateBuilder.OneWayFare100Tax20(Now).Build();

            AssertMessage("authorized sales scope", () => CandidateValidator.EnsureValid(candidate, CandidateBuilder.Scope(customerId: 200)));
        }

        [Fact]
        public void Amount_beyond_storage_scale_is_rejected_not_rounded()
        {
            var builder = new CandidateBuilder(Now)
                .Traveller("PAX-A")
                .Segment("SEG-A")
                .AirService("SERVICE-A", "PAX-A", "SEG-A")
                .Package("ITEM-A", "SERVICE-A")
                .Line("fare", "ITEM-A", PricingComponentType.Fare, 1.123456789m, "SERVICE-A");

            AssertCode(RepresentationOverflow, () => CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope));
        }

        [Fact]
        public void Commission_cannot_be_a_customer_charge()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now)
                .Line("commission", "ITEM-A", PricingComponentType.Commission, 5m, "S-A");

            AssertCode(PricingRuleViolated, () => CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope));
        }

        [Fact]
        public void Two_values_in_one_currency_are_a_contract_mismatch()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now)
                .Line("fare-2", "ITEM-A", PricingComponentType.Fare, 10m, "S-A", original: new Money(11m, CandidateBuilder.SaleCurrencyId));

            AssertMessage("two different values in one currency", () => CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope));
        }

        [Fact]
        public void An_unknown_pricing_unit_type_is_not_representable()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now)
                .Units(CandidateBuilder.Unit(1, (FarePricingUnitType)77, ["BOUND-1"], CandidateBuilder.Component(4242)));

            AssertMessage("type is not defined", () => CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope));
        }

        private static void AssertCode(int code, Action action)
        {
            var exception = Assert.Throws<BusinessException>(action);
            Assert.Equal(code, exception.Code);
        }

        private static void AssertMessage(string fragment, Action action)
        {
            var exception = Assert.Throws<BusinessException>(action);
            Assert.Contains(fragment, exception.Message);
        }
    }
}
