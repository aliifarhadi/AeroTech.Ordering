using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Aegis.Enums;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Messages.Shared.Enums;
using AeroTech.Ordering.Domain._Shared.Policies;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.Tests._Shared;
using Xunit;

namespace AeroTech.Ordering.Domain.Tests.OrderAggregate
{
    public sealed class SalesProvenanceTests
    {
        private const int SalesContextIncomplete = 20289;

        private static readonly DateTimeOffset Now = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

        [Fact]
        public void An_agency_sale_freezes_the_selling_agency_and_its_office_namespace()
        {
            var order = AcceptOriginalSaleTests.Accept(AgencySale(), AcceptOriginalSaleTests.Bind("PAX-A"));

            Assert.Equal(SalesChannel.AgencyPanel, order.SalesContext.Channel);
            Assert.Equal(BusinessContextType.TravelAgency, order.SalesContext.SellerContextType);
            Assert.Equal(77, order.SalesContext.SellerId);
            Assert.Equal(SellingOfficeKind.TravelAgencyOffice, order.SalesContext.SellingOfficeKind);
            Assert.Equal(55, order.SalesContext.SellingOfficeId);
        }

        [Fact]
        public void Financial_customer_seller_buyer_and_actor_are_four_separate_facts()
        {
            var order = AcceptOriginalSaleTests.Accept(AgencySale(), AcceptOriginalSaleTests.Bind("PAX-A"));

            Assert.Equal(900, order.FinancialCustomerId);
            Assert.Equal(77, order.SalesContext.SellerId);
            Assert.False(order.Buyer.IsSupplied);
            Assert.Equal(BusinessContextType.TravelAgency, order.InitiatingActor.ContextType);
            Assert.Equal(4242, order.InitiatingActor.ActorId);
            Assert.NotEqual(order.FinancialCustomerId, order.SalesContext.SellerId);
            Assert.NotEqual(order.SalesContext.SellerId, order.InitiatingActor.ActorId);
        }

        [Fact]
        public void An_office_identifier_carries_the_namespace_it_belongs_to()
        {
            var airline = AcceptOriginalSaleTests.Accept(
                new CandidateBuilder(Now, CandidateBuilder.Scope()).Traveler("PAX-A").Segment("SEG-1")
                    .AirService("S-A", "PAX-A", "SEG-1").Package("ITEM-A", "S-A")
                    .Line("FARE", "ITEM-A", PricingComponentType.Fare, 100m, "S-A"),
                AcceptOriginalSaleTests.Bind("PAX-A"));

            var agency = AcceptOriginalSaleTests.Accept(AgencySale(), AcceptOriginalSaleTests.Bind("PAX-A"));

            Assert.Equal(SellingOfficeKind.AirlineOffice, airline.SalesContext.SellingOfficeKind);
            Assert.Equal(SellingOfficeKind.TravelAgencyOffice, agency.SalesContext.SellingOfficeKind);
            Assert.NotEqual(airline.SalesContext.SellingOfficeKind, agency.SalesContext.SellingOfficeKind);
        }

        [Fact]
        public void A_partner_api_profile_is_the_actor_and_is_neither_buyer_nor_seller()
        {
            var scope = CandidateBuilder.Scope(
                customerId: 901,
                salesContext: SalesContextSnapshot.SellerNotSupplied(SalesChannel.PartnerAPI, null, null),
                buyer: BuyerSnapshot.NotSupplied);

            var partnerScope = new AuthorizedSalesScope(
                scope.OwnerAirlineId,
                scope.FinancialCustomerId,
                scope.SalesContext,
                scope.Buyer,
                scope.CallerScope,
                new InitiatingActorSnapshot(BusinessContextType.PartnerApi, 31337));

            var order = AcceptOriginalSaleTests.Accept(Sale(partnerScope), AcceptOriginalSaleTests.Bind("PAX-A"));

            Assert.False(order.SalesContext.HasSeller);
            Assert.False(order.Buyer.IsSupplied);
            Assert.Equal(BusinessContextType.PartnerApi, order.InitiatingActor.ContextType);
            Assert.Equal(31337, order.InitiatingActor.ActorId);
        }

        [Fact]
        public void A_buyer_is_never_half_supplied()
        {
            Assert.Equal(SalesContextIncomplete, Assert.Throws<BusinessException>(() => new BuyerSnapshot(BusinessContextType.Individual, null)).Code);
            Assert.Equal(SalesContextIncomplete, Assert.Throws<BusinessException>(() => new BuyerSnapshot(null, 5)).Code);
            Assert.False(BuyerSnapshot.NotSupplied.IsSupplied);
        }

        [Fact]
        public void A_seller_or_an_office_is_never_half_supplied()
        {
            Assert.Equal(SalesContextIncomplete, Assert.Throws<BusinessException>(
                () => new SalesContextSnapshot(SalesChannel.BackOffice, BusinessContextType.Airline, null, null, null)).Code);

            Assert.Equal(SalesContextIncomplete, Assert.Throws<BusinessException>(
                () => new SalesContextSnapshot(SalesChannel.BackOffice, null, null, SellingOfficeKind.AirlineOffice, null)).Code);
        }

        [Theory]
        [InlineData(SalesChannel.BackOffice, SellingOfficeKind.AirlineOffice)]
        [InlineData(SalesChannel.AgencyPanel, SellingOfficeKind.TravelAgencyOffice)]
        [InlineData(SalesChannel.PartnerAPI, SellingOfficeKind.TravelAgencyOffice)]
        [InlineData(SalesChannel.System, SellingOfficeKind.NotRecorded)]
        [InlineData(SalesChannel.GDS, SellingOfficeKind.NotRecorded)]
        public void A_legacy_office_kind_is_only_inferred_from_the_immutable_channel(SalesChannel channel, SellingOfficeKind expected)
        {
            Assert.Equal(expected, LegacySellingOfficePolicy.KindOf(channel, 10));
            Assert.Null(LegacySellingOfficePolicy.KindOf(channel, null));
        }

        private static CandidateBuilder AgencySale()
        {
            var scope = new AuthorizedSalesScope(
                1,
                900,
                new SalesContextSnapshot(SalesChannel.AgencyPanel, BusinessContextType.TravelAgency, 77, SellingOfficeKind.TravelAgencyOffice, 55),
                BuyerSnapshot.NotSupplied,
                "otapanel|customer:900|office:55|principal:agency:77",
                new InitiatingActorSnapshot(BusinessContextType.TravelAgency, 4242));

            return Sale(scope);
        }

        private static CandidateBuilder Sale(AuthorizedSalesScope scope)
            => new CandidateBuilder(Now, scope)
                .Traveler("PAX-A")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .Package("ITEM-A", "S-A")
                .Line("FARE", "ITEM-A", PricingComponentType.Fare, 100m, "S-A");
    }
}
