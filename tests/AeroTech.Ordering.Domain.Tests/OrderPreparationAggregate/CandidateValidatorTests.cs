using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Policies;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Serialization;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;
using AeroTech.Ordering.Domain.Tests._Shared;
using Xunit;
using AeroTech.Messages.Shared.Enums;

namespace AeroTech.Ordering.Domain.Tests.OrderPreparationAggregate
{
    public sealed class CandidateValidatorTests
    {
        private const int ContractMismatch = 20272;
        private const int UnsupportedCapability = 20273;
        private const int PricingRuleViolated = 20279;
        private const int RepresentationOverflow = 20278;

        private static readonly DateTimeOffset Now = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

        [Fact]
        public void Reference_one_way_candidate_is_valid()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now);

            CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope);
        }

        [Fact]
        public void Unregistered_detail_schema_version_is_unsupported_before_acceptance()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now);
            var candidate = builder.Build();
            var unregistered = candidate with
            {
                Services = [candidate.Services[0] with { DetailSchemaVersion = 3 }]
            };

            AssertCode(UnsupportedCapability, () => CandidateValidator.EnsureValid(unregistered, builder.SalesScope));
        }

        [Fact]
        public void Unregistered_service_type_is_unsupported_before_acceptance()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now);
            var candidate = builder.Build();
            var seat = candidate with
            {
                Services = [candidate.Services[0] with { Type = OrderServiceType.SeatAssignment, DetailSchema = "Seat" }]
            };

            AssertCode(UnsupportedCapability, () => CandidateValidator.EnsureValid(seat, builder.SalesScope));
        }

        [Fact]
        public void Pack_negative_examples_are_rejected_with_their_rule()
        {
            var incorrectTotal = PackCandidate(PackExamples.IncorrectTotal);
            var missingBeneficiary = PackCandidate(PackExamples.MissingBeneficiary);
            var settlementTax = PackCandidate(PackExamples.SettlementTax);

            AssertMessage("differs from the pricing lines", () => CandidateValidator.EnsureValid(incorrectTotal.Candidate, incorrectTotal.Scope));
            AssertMessage("is not a candidate traveler", () => CandidateValidator.EnsureValid(missingBeneficiary.Candidate, missingBeneficiary.Scope));
            AssertCode(PricingRuleViolated, () => CandidateValidator.EnsureValid(settlementTax.Candidate, settlementTax.Scope));
        }

        [Fact]
        public void Air_service_with_two_segments_is_rejected()
        {
            var builder = new CandidateBuilder(Now)
                .Traveller("PAX-A")
                .Segment("SEG-A")
                .Segment("SEG-B")
                .Package("ITEM-A", "SERVICE-A")
                .Line("FARE", "ITEM-A", PricingComponentType.Fare, 100m, "SERVICE-A");
            var candidate = builder.AirService("SERVICE-A", "PAX-A", "SEG-A").Build();
            var widened = candidate with { Services = [candidate.Services[0] with { SegmentRefs = ["SEG-A", "SEG-B"] }] };

            AssertCode(ContractMismatch, () => CandidateValidator.EnsureValid(widened, builder.SalesScope));
        }

        [Fact]
        public void Opaque_construction_cannot_claim_component_links()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now)
                .OpaqueUnits(new CandidatePricingUnit("PU-1", null, FarePricingUnitType.OneWay, FareCombinationMethod.Unspecified, ["BOUND-1"], null,
                    [new CandidateFareComponent("FARE-1", "YOW", null, "Published", null, null, "Y", null, null, null, null, null, ["SERVICE-A"], [])]));

            AssertMessage("opaque construction", () => CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope));
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
                .Line("FARE", "ITEM-A", PricingComponentType.Fare, 1.123456789m, "SERVICE-A");

            AssertCode(RepresentationOverflow, () => CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope));
        }

        [Fact]
        public void Commission_cannot_be_a_customer_charge()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now)
                .Line("COMMISSION", "ITEM-A", PricingComponentType.Commission, 5m, "SERVICE-A");

            AssertCode(PricingRuleViolated, () => CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope));
        }

        [Fact]
        public void Two_values_in_one_currency_are_a_contract_mismatch()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now)
                .Line("FARE-2", "ITEM-A", PricingComponentType.Fare, 10m, "SERVICE-A", original: new Money(11m, CandidateBuilder.SaleCurrencyId));

            AssertMessage("two different values in one currency", () => CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope));
        }

        private static (NormalizedCandidate Candidate, AuthorizedSalesScope Scope) PackCandidate(string packJson)
        {
            var legacy = NormalizedCandidateJson.Read(packJson);
            var candidate = legacy with { SchemaVersion = NormalizedCandidate.CurrentSchemaVersion };

            var scope = new AuthorizedSalesScope(
                candidate.SalesContext.OwnerAirlineId,
                candidate.SalesContext.FinancialCustomerId,
                candidate.SalesContext.Sales,
                candidate.SalesContext.Buyer,
                "pack-example",
                new InitiatingActorSnapshot(AeroTech.Messages.Aegis.Enums.BusinessContextType.Airline, 1));

            return (candidate, scope);
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
