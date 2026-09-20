using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Aegis.Enums;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Policies;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;
using AeroTech.Ordering.Domain.Tests._Shared;
using Xunit;

namespace AeroTech.Ordering.Domain.Tests.OrderAggregate
{
    public sealed class AcceptedScopeAndLiabilityTests
    {
        private const int ContractMismatch = 20272;
        private const int FundingObligationScopeInvalid = 20295;

        private static readonly DateTimeOffset Now = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

        private static readonly BuyerSnapshot BuyerA = new(BusinessContextType.TravelAgency, 4242);
        private static readonly BuyerSnapshot BuyerB = new(BusinessContextType.TravelAgency, 9999);

        [Fact]
        public void A_candidate_with_no_buyer_is_accepted_under_a_scope_with_no_buyer()
        {
            var scope = CandidateBuilder.Scope();

            Assert.False(scope.Buyer.IsSupplied);
            Assert.Null(Record.Exception(() => CandidateValidator.EnsureValid(Sale(scope).Build(), scope)));
        }

        [Fact]
        public void A_candidate_buyer_is_accepted_only_under_the_same_buyer()
        {
            var scope = CandidateBuilder.Scope(buyer: BuyerA);

            Assert.Null(Record.Exception(() => CandidateValidator.EnsureValid(Sale(scope).Build(), scope)));
        }

        [Fact]
        public void A_candidate_buyer_cannot_be_sold_under_a_different_buyer()
        {
            var captured = CandidateBuilder.Scope(buyer: BuyerA);
            var other = CandidateBuilder.Scope(buyer: BuyerB);

            var exception = Assert.Throws<BusinessException>(
                () => CandidateValidator.EnsureValid(Sale(captured).Build(), other));

            Assert.Equal(ContractMismatch, exception.Code);
            Assert.Contains("candidate buyer differs from the authorized sales scope", exception.Message);
        }

        [Fact]
        public void A_candidate_buyer_cannot_be_sold_under_a_scope_that_supplies_none()
        {
            var captured = CandidateBuilder.Scope(buyer: BuyerA);

            Assert.Contains(
                "candidate buyer differs",
                Assert.Throws<BusinessException>(
                    () => CandidateValidator.EnsureValid(Sale(captured).Build(), CandidateBuilder.Scope())).Message);
        }

        [Fact]
        public void A_candidate_without_a_buyer_cannot_be_sold_under_a_scope_that_supplies_one()
        {
            var captured = CandidateBuilder.Scope();

            Assert.Contains(
                "candidate buyer differs",
                Assert.Throws<BusinessException>(
                    () => CandidateValidator.EnsureValid(Sale(captured).Build(), CandidateBuilder.Scope(buyer: BuyerA))).Message);
        }

        [Fact]
        public void A_buyer_is_never_inferred_from_another_role()
        {
            var scope = CandidateBuilder.Scope(customerId: 100, buyer: BuyerA);
            var order = AcceptOriginalSaleTests.Accept(Sale(scope), AcceptOriginalSaleTests.Bind("PAX-A"));

            Assert.Equal(4242, order.Buyer.BuyerId);
            Assert.NotEqual(order.FinancialCustomerId, order.Buyer.BuyerId);
            Assert.NotEqual(order.SalesContext.SellerId, order.Buyer.BuyerId);
            Assert.NotEqual(order.InitiatingActor.ActorId, order.Buyer.BuyerId);
            Assert.DoesNotContain(order.Travellers.Select(traveller => (long?)traveller.Id), id => id == order.Buyer.BuyerId);
        }

        [Fact]
        public void A_service_scope_whose_original_sale_net_is_negative_is_rejected_before_any_order()
        {
            var builder = new CandidateBuilder(Now)
                .Traveller("PAX-A")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .Package("ITEM-A", "S-A")
                .Line("fare", "ITEM-A", PricingComponentType.Fare, 100m, "S-A")
                .Line("service-discount", null, PricingComponentType.Discount, 10m, "S-A");

            var candidate = builder.Build();

            Assert.Equal(90m, candidate.CustomerTotal.Amount);

            var exception = Assert.Throws<BusinessException>(() => CandidateValidator.EnsureValid(candidate, builder.SalesScope));

            Assert.Equal(ContractMismatch, exception.Code);
            Assert.Contains("negative original-sale liability", exception.Message);
            Assert.Contains("an original sale is not a refund", exception.Message);
        }

        [Fact]
        public void A_negative_original_sale_customer_total_is_rejected_before_any_order()
        {
            var builder = new CandidateBuilder(Now)
                .Traveller("PAX-A")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .Package("ITEM-A", "S-A")
                .Line("fare", "ITEM-A", PricingComponentType.Fare, 10m, "S-A")
                .Line("promo", null, PricingComponentType.Discount, 25m, "S-A");

            var candidate = builder.Build();

            Assert.Equal(10m, Assert.Single(candidate.Items).AcceptedTotal.Amount);
            Assert.Equal(-15m, candidate.CustomerTotal.Amount);

            var exception = Assert.Throws<BusinessException>(() => CandidateValidator.EnsureValid(candidate, builder.SalesScope));

            Assert.Equal(ContractMismatch, exception.Code);
            Assert.Contains("is negative; an original sale is not a refund", exception.Message);
        }

        [Fact]
        public void A_positive_item_less_service_scoped_liability_remains_valid()
        {
            var order = AcceptOriginalSaleTests.Accept(
                new CandidateBuilder(Now)
                    .Traveller("PAX-A")
                    .Segment("SEG-1")
                    .AirService("S-A", "PAX-A", "SEG-1")
                    .Package("ITEM-A", "S-A")
                    .Line("fare", "ITEM-A", PricingComponentType.Fare, 100m, "S-A")
                    .Line("service-fee", null, PricingComponentType.Fee, 20m, "S-A")
                    .Line("service-discount", null, PricingComponentType.Discount, 5m, "S-A"),
                AcceptOriginalSaleTests.Bind("PAX-A"));

            var serviceScoped = order.FundingObligations.Single(obligation => obligation.OrderServiceId is not null);

            Assert.Equal(115m, order.CustomerTotal.Amount);
            Assert.Equal(15m, serviceScoped.Amount.Amount);
            Assert.False(serviceScoped.Amount.IsNegative);
            Assert.Equal(order.CustomerTotal.Amount, order.FundingObligations.Sum(obligation => obligation.Amount.Amount));
        }

        [Fact]
        public void A_negative_obligation_can_never_be_materialised_by_the_aggregate()
        {
            var exception = Assert.Throws<BusinessException>(() => AcceptOriginalSaleTests.Accept(
                new CandidateBuilder(Now)
                    .Traveller("PAX-A")
                    .Segment("SEG-1")
                    .AirService("S-A", "PAX-A", "SEG-1")
                    .Package("ITEM-A", "S-A")
                    .Line("fare", "ITEM-A", PricingComponentType.Fare, 100m, "S-A")
                    .Line("service-discount", null, PricingComponentType.Discount, 10m, "S-A"),
                AcceptOriginalSaleTests.Bind("PAX-A")));

            Assert.Contains(exception.Code, new[] { ContractMismatch, FundingObligationScopeInvalid });
        }

        [Fact]
        public void A_fulfillment_profile_supplies_its_identity_and_version_together_or_not_at_all()
        {
            Assert.Null(Record.Exception(() => Validate(Profile("PROFILE-A", "1", FulfillmentProfileAssurance.Certified))));
            Assert.Null(Record.Exception(() => Validate(CandidateFulfillmentProfile.Unresolved)));

            Assert.Contains(
                "identity and version are supplied together",
                Assert.Throws<BusinessException>(() => Validate(Profile("PROFILE-A", null, FulfillmentProfileAssurance.Certified))).Message);

            Assert.Contains(
                "identity and version are supplied together",
                Assert.Throws<BusinessException>(() => Validate(Profile(null, "1", FulfillmentProfileAssurance.Certified))).Message);

            Assert.Contains(
                "identity and version are supplied together",
                Assert.Throws<BusinessException>(() => Validate(Profile("PROFILE-A", null, FulfillmentProfileAssurance.NotCertified))).Message);

            Assert.Contains(
                "without naming its identity and version",
                Assert.Throws<BusinessException>(() => Validate(Profile(null, null, FulfillmentProfileAssurance.Certified))).Message);
        }

        [Fact]
        public void A_supplied_fulfillment_profile_pair_round_trips_unchanged()
        {
            var order = AcceptOriginalSaleTests.Accept(
                new CandidateBuilder(Now)
                    .Traveller("PAX-A")
                    .Segment("SEG-1")
                    .AirService("S-A", "PAX-A", "SEG-1", fulfillmentProfile: Profile("PROFILE-A", "2026.1", FulfillmentProfileAssurance.Certified))
                    .Package("ITEM-A", "S-A")
                    .Line("fare", "ITEM-A", PricingComponentType.Fare, 100m, "S-A"),
                AcceptOriginalSaleTests.Bind("PAX-A"));

            var snapshot = Assert.Single(order.Services).FulfillmentProfile;

            Assert.Equal("PROFILE-A", snapshot.ProfileRef);
            Assert.Equal("2026.1", snapshot.ProfileVersion);
            Assert.True(snapshot.IsCertified);
        }

        [Fact]
        public void The_unresolved_live_profile_stays_unresolved()
        {
            var order = AcceptOriginalSaleTests.Accept(CandidateBuilder.OneWayFare100Tax20(Now), AcceptOriginalSaleTests.Bind("PAX-A"));
            var snapshot = Assert.Single(order.Services).FulfillmentProfile;

            Assert.Null(snapshot.ProfileRef);
            Assert.Null(snapshot.ProfileVersion);
            Assert.Equal(FulfillmentProfileAssurance.NotCertified, snapshot.Assurance);
            Assert.Equal(ReservationRequirement.Unresolved, snapshot.ReservationRequirement);
            Assert.Equal(FulfillmentDocumentKind.Unresolved, snapshot.DocumentKind);
            Assert.Equal(FundingRequirement.Unresolved, snapshot.FundingRequirement);
        }

        private static CandidateBuilder Sale(AuthorizedSalesScope scope)
            => new CandidateBuilder(Now, scope)
                .Traveller("PAX-A")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .Package("ITEM-A", "S-A")
                .Line("fare", "ITEM-A", PricingComponentType.Fare, 100m, "S-A");

        private static CandidateFulfillmentProfile Profile(string? profileRef, string? profileVersion, FulfillmentProfileAssurance assurance)
            => CandidateFulfillmentProfile.Unresolved with
            {
                ProfileRef = profileRef,
                ProfileVersion = profileVersion,
                Assurance = assurance
            };

        private static void Validate(CandidateFulfillmentProfile profile)
        {
            var builder = new CandidateBuilder(Now)
                .Traveller("PAX-A")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1", fulfillmentProfile: profile)
                .Package("ITEM-A", "S-A")
                .Line("fare", "ITEM-A", PricingComponentType.Fare, 100m, "S-A");

            CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope);
        }
    }
}
