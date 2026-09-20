using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Aegis.Enums;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;
using AeroTech.Ordering.Domain.Tests._Shared;
using Xunit;

namespace AeroTech.Ordering.Domain.Tests.OrderAggregate
{
    public sealed class AcceptedShapeTests
    {
        private const int FundingCoverageIncomplete = 20296;
        private const int FundingObligationScopeInvalid = 20295;
        private const int ContractMismatch = 20272;

        private static readonly DateTimeOffset Now = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

        [Fact]
        public void An_accepted_air_service_declares_its_service_type_and_owns_its_air_facts()
        {
            var order = AcceptOriginalSaleTests.Accept(CandidateBuilder.OneWayFare100Tax20(Now), AcceptOriginalSaleTests.Bind("PAX-A"));

            var service = Assert.Single(order.Services);

            Assert.Equal(OrderServiceType.AirTransportation, service.ServiceType);

            var air = Assert.IsType<OrderAirTransportService>(service);

            Assert.Equal(order.Travellers.Single().Id, air.TravellerId);
            Assert.Equal(order.Segments.Single().Id, air.SegmentId);
            Assert.Equal(1, air.CabinClassId);
            Assert.Equal(25L, air.RbdId);
            Assert.Equal("Y", air.BookingClass);
            Assert.NotNull(air.SoldTerms);
        }

        [Fact]
        public void An_accepted_service_keeps_an_honest_fulfillment_profile_without_inventing_one()
        {
            var order = AcceptOriginalSaleTests.Accept(CandidateBuilder.OneWayFare100Tax20(Now), AcceptOriginalSaleTests.Bind("PAX-A"));

            var profile = Assert.Single(order.Services).FulfillmentProfile;

            Assert.Null(profile.ProfileRef);
            Assert.Null(profile.ProfileVersion);
            Assert.False(profile.IsCertified);
            Assert.Equal(FulfillmentProfileAssurance.NotCertified, profile.Assurance);
            Assert.Equal(ReservationRequirement.Unresolved, profile.ReservationRequirement);
            Assert.Equal(FulfillmentDocumentKind.Unresolved, profile.DocumentKind);
            Assert.Equal(FundingRequirement.Unresolved, profile.FundingRequirement);
            Assert.Null(profile.DocumentAuthority);
            Assert.Null(profile.CapacityUnits);
            Assert.Null(profile.PartialFulfillmentSupported);
        }

        [Fact]
        public void A_certified_fulfillment_profile_must_name_itself()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now);
            var candidate = builder.Build();
            var certified = candidate with
            {
                Services =
                [
                    candidate.Services[0] with
                    {
                        FulfillmentProfile = CandidateFulfillmentProfile.Unresolved with
                        {
                            Assurance = FulfillmentProfileAssurance.Certified
                        }
                    }
                ]
            };

            var exception = Assert.Throws<BusinessException>(
                () => Domain.OrderPreparationAggregate.Policies.CandidateValidator.EnsureValid(certified, builder.SalesScope));

            Assert.Equal(ContractMismatch, exception.Code);
            Assert.Contains("certified fulfillment profile without naming it", exception.Message);
        }

        [Fact]
        public void An_accepted_buyer_is_not_supplied_and_is_never_inferred_from_another_role()
        {
            var scope = CandidateBuilder.Scope(customerId: 100);
            var order = AcceptOriginalSaleTests.Accept(
                CandidateBuilder.OneWayFare100Tax20(Now, scope),
                AcceptOriginalSaleTests.Bind("PAX-A"));

            Assert.False(order.Buyer.IsSupplied);
            Assert.Null(order.Buyer.BuyerId);
            Assert.Null(order.Buyer.ContextType);
            Assert.NotEqual(order.FinancialCustomerId, order.Buyer.BuyerId ?? 0);
            Assert.NotNull(order.SalesContext.SellerId);
            Assert.NotEqual(order.SalesContext.SellerId, order.Buyer.BuyerId);
            Assert.NotEqual(order.InitiatingActor.ActorId, order.Buyer.BuyerId);
        }

        [Fact]
        public void A_supplied_buyer_survives_acceptance_as_its_own_party()
        {
            var scope = CandidateBuilder.Scope(buyer: new BuyerSnapshot(BusinessContextType.TravelAgency, 4242));
            var order = AcceptOriginalSaleTests.Accept(
                CandidateBuilder.OneWayFare100Tax20(Now, scope),
                AcceptOriginalSaleTests.Bind("PAX-A"));

            Assert.True(order.Buyer.IsSupplied);
            Assert.Equal(4242, order.Buyer.BuyerId);
            Assert.Equal(BusinessContextType.TravelAgency, order.Buyer.ContextType);
            Assert.NotEqual(order.Buyer.BuyerId, order.FinancialCustomerId);
        }

        [Fact]
        public void A_buyer_is_never_half_supplied()
        {
            Assert.Throws<BusinessException>(() => new BuyerSnapshot(BusinessContextType.TravelAgency, null));
            Assert.Throws<BusinessException>(() => new BuyerSnapshot(null, 4242));
            Assert.Throws<BusinessException>(() => new BuyerSnapshot(BusinessContextType.TravelAgency, 0));
        }

        [Fact]
        public void Component_totals_are_committed_once_per_component_and_effect()
        {
            var order = AcceptOriginalSaleTests.Accept(CandidateBuilder.OneWayFare100Tax20(Now), AcceptOriginalSaleTests.Bind("PAX-A"));

            Assert.Equal(2, order.ComponentTotals.Count);
            Assert.Equal(
                order.ComponentTotals.Select(total => (total.Component, total.Effect)).Distinct().Count(),
                order.ComponentTotals.Count);

            foreach (var total in order.ComponentTotals)
            {
                var lines = order.PricingLines.Where(line => line.Component == total.Component && line.Effect == total.Effect).ToList();

                Assert.Equal(lines.Where(line => line.Direction == OrderPricingLineDirection.Debit).Sum(line => line.SaleValue.Amount), total.DebitAmount);
                Assert.Equal(lines.Where(line => line.Direction == OrderPricingLineDirection.Credit).Sum(line => line.SaleValue.Amount), total.CreditAmount);
                Assert.Equal(order.CurrencyId, total.CurrencyId);
            }

            Assert.Equal(
                order.CustomerTotal.Amount,
                order.ComponentTotals.Where(total => total.Effect == PricingEffect.CustomerBalance).Sum(total => total.Net));
        }

        [Fact]
        public void Every_accepted_pricing_line_records_its_role()
        {
            var order = AcceptOriginalSaleTests.Accept(CandidateBuilder.OneWayFare100Tax20(Now), AcceptOriginalSaleTests.Bind("PAX-A"));

            Assert.NotEmpty(order.PricingLines);
            Assert.All(order.PricingLines, line => Assert.Equal(PricingLineRole.Original, line.Role));
        }

        [Fact]
        public void Every_customer_balance_line_is_covered_by_a_funding_obligation()
        {
            var order = AcceptOriginalSaleTests.Accept(CandidateBuilder.OneWayFare100Tax20(Now), AcceptOriginalSaleTests.Bind("PAX-A"));

            var customerLines = order.PricingLines.Where(line => line.Effect == PricingEffect.CustomerBalance).ToList();

            Assert.NotEmpty(customerLines);
            Assert.Equal(order.CustomerTotal.Amount, order.FundingObligations.Sum(obligation => obligation.Amount.Amount));
            Assert.All(order.FundingObligations, obligation => Assert.Equal(
                1,
                new object?[] { obligation.OrderItemId, obligation.OrderServiceId, obligation.PricingLineId }.Count(value => value is not null)));
            Assert.All(order.FundingObligations, obligation => Assert.Equal(order.PriceChangeSets.Single().Id, obligation.PriceChangeSetId));
        }

        [Fact]
        public void A_customer_balance_line_that_cannot_be_scoped_is_refused_at_the_candidate_boundary()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now);
            var candidate = builder.Build();
            var unscoped = candidate with
            {
                PricingLines = candidate.PricingLines
                    .Select(line => line with { ItemKey = null, BasisType = PricingBasisType.Order, BasisKey = "order" })
                    .ToList()
            };

            var exception = Assert.Throws<BusinessException>(
                () => Domain.OrderPreparationAggregate.Policies.CandidateValidator.EnsureValid(unscoped, builder.SalesScope));

            Assert.Equal(ContractMismatch, exception.Code);
            Assert.Contains("funding liability cannot be scoped", exception.Message);
        }

        [Fact]
        public void A_funding_obligation_names_exactly_one_scope()
        {
            Assert.Equal(20295, Assert.Throws<BusinessException>(
                () => Domain.OrderAggregate.ValueObjects.FundingObligationScope.ForItem(0)).Code);

            Assert.Equal(FundingObligationScopeInvalid, Assert.Throws<BusinessException>(
                () => Domain.OrderAggregate.ValueObjects.FundingObligationScope.ForService(-1)).Code);

            Assert.Null(Domain.OrderAggregate.ValueObjects.FundingObligationScope.ForItem(7).OrderServiceId);
            Assert.Equal(7, Domain.OrderAggregate.ValueObjects.FundingObligationScope.ForItem(7).OrderItemId);
        }

        [Fact]
        public void An_original_order_is_its_own_root()
        {
            var order = AcceptOriginalSaleTests.Accept(CandidateBuilder.OneWayFare100Tax20(Now), AcceptOriginalSaleTests.Bind("PAX-A"));

            Assert.Equal(order.Id, order.RootOrderId);
            Assert.True(order.RootOrderId > 0);
        }

        [Fact]
        public void The_sale_currency_keeps_its_identity_and_the_source_code_snapshot()
        {
            var order = AcceptOriginalSaleTests.Accept(
                CandidateBuilder.OneWayFare100Tax20(Now).SaleCurrencyCode("EUR"),
                AcceptOriginalSaleTests.Bind("PAX-A"));

            Assert.Equal(CandidateBuilder.SaleCurrencyId, order.CurrencyId);
            Assert.Equal("EUR", order.SaleCurrencyCode);
            Assert.Equal(CandidateBuilder.SaleCurrencyId, order.CustomerTotal.CurrencyId);
        }

        [Fact]
        public void A_source_that_supplies_no_currency_code_records_none()
        {
            var order = AcceptOriginalSaleTests.Accept(CandidateBuilder.OneWayFare100Tax20(Now), AcceptOriginalSaleTests.Bind("PAX-A"));

            Assert.Null(order.SaleCurrencyCode);
        }

        [Fact]
        public void The_original_sale_records_one_item_service_link_per_service()
        {
            var order = AcceptOriginalSaleTests.Accept(CandidateBuilder.OneWayFare100Tax20(Now), AcceptOriginalSaleTests.Bind("PAX-A"));

            var link = Assert.Single(order.ItemServiceLinks);
            var service = Assert.Single(order.Services);

            Assert.Equal(order.Id, link.OrderIdAtAssociation);
            Assert.Equal(service.OrderItemId, link.OrderItemId);
            Assert.Equal(service.Id, link.OrderServiceId);
            Assert.Equal(order.Changes.Single().Id, link.LinkedByChangeId);
        }

        [Fact]
        public void A_candidate_without_a_supplied_fare_construction_commits_none()
        {
            var order = AcceptOriginalSaleTests.Accept(CandidateBuilder.OneWayFare100Tax20(Now), AcceptOriginalSaleTests.Bind("PAX-A"));

            Assert.Empty(order.FareConstructions);
        }

        [Fact]
        public void A_fare_construction_links_only_the_items_the_source_named()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now)
                .OneWayConstruction("BOUND-1")
                .ConstructionFor("ITEM-A");

            var order = AcceptOriginalSaleTests.Accept(builder, AcceptOriginalSaleTests.Bind("PAX-A"));

            var construction = Assert.Single(order.FareConstructions);

            Assert.Equal(FareConstructionAssurance.SourceProvided, construction.Assurance);
            Assert.Equal(order.Items.Single().Id, Assert.Single(construction.Items).OrderItemId);
            Assert.Equal(["BOUND-1"], Assert.Single(construction.PricingUnits).CoveredBounds.Select(bound => bound.CoveredBoundOfferId));
        }

        [Fact]
        public void A_fare_construction_naming_an_unknown_item_is_refused()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now)
                .OneWayConstruction("BOUND-1")
                .ConstructionFor("ITEM-UNKNOWN");

            var exception = Assert.Throws<BusinessException>(
                () => Domain.OrderPreparationAggregate.Policies.CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope));

            Assert.Equal(ContractMismatch, exception.Code);
            Assert.Contains("is not a candidate item", exception.Message);
        }

        [Fact]
        public void The_candidate_builder_invents_no_covered_bound_offer_ids()
        {
            var candidate = CandidateBuilder.OneWayFare100Tax20(Now).Build();

            Assert.Null(candidate.FareConstruction);
        }

        [Fact]
        public void A_pricing_unit_keeps_the_owner_construction_type_it_was_sold_under()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now)
                .Units(CandidateBuilder.Unit(
                    1,
                    FarePricingUnitType.RoundTrip,
                    AirFareConstructionType.RoundTripFromOneWays,
                    ["BOUND-1"],
                    CandidateBuilder.Component(4242)))
                .ConstructionFor("ITEM-A");

            var order = AcceptOriginalSaleTests.Accept(builder, AcceptOriginalSaleTests.Bind("PAX-A"));
            var unit = Assert.Single(Assert.Single(order.FareConstructions).PricingUnits);

            Assert.Equal(FarePricingUnitType.RoundTrip, unit.Type);
            Assert.Equal(AirFareConstructionType.RoundTripFromOneWays, unit.SourceConstructionType);
        }
    }
}
