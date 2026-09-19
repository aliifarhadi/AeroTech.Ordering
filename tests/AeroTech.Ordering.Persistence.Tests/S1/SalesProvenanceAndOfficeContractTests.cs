using AeroTech.Messages.Aegis.Enums;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Messages.Shared.Enums;
using AeroTech.Ordering.Domain.Tests._Shared;
using AeroTech.Ordering.Persistence.Tests._Shared;
using AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrderById.Backoffice;
using AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrderById.OtaPanel;
using AeroTech.Ordering.ReferenceData.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class SalesProvenanceAndOfficeContractTests
    {
        private readonly OrderingDatabaseFixture _fixture;

        public SalesProvenanceAndOfficeContractTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task A_backoffice_sale_records_the_owner_airline_as_seller_in_its_own_office_namespace()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);
            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));

            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);

            Assert.Equal(SalesChannel.BackOffice, order.SalesContext.Channel);
            Assert.Equal(BusinessContextType.Airline, order.SalesContext.SellerContextType);
            Assert.Equal(S1Harness.OwnerAirlineId, order.SalesContext.SellerId);
            Assert.Equal(SellingOfficeKind.AirlineOffice, order.SalesContext.SellingOfficeKind);
            Assert.Equal(S1Harness.AirlineOfficeId, order.SalesContext.SellingOfficeId);
            Assert.Equal(BusinessContextType.Airline, order.InitiatingActor.ContextType);
            Assert.Equal(S1Harness.AirlineUserId, order.InitiatingActor.ActorId);
        }

        [Fact]
        public async Task An_agency_panel_sale_persists_the_travel_agency_as_seller_and_survives_a_reference_data_change()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await PublishAgencyOfferAsync(harness);
            harness.Caller.UseOtaPanel(S1Harness.TravelAgencyId, S1Harness.AgencyOfficeId, S1Harness.AgencyUserId);
            var created = await harness.SendAsync(S1Commands.OtaPanel(offerId));

            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);

            Assert.Equal(SalesChannel.AgencyPanel, order.SalesContext.Channel);
            Assert.Equal(BusinessContextType.TravelAgency, order.SalesContext.SellerContextType);
            Assert.Equal(S1Harness.TravelAgencyId, order.SalesContext.SellerId);
            Assert.Equal(SellingOfficeKind.TravelAgencyOffice, order.SalesContext.SellingOfficeKind);
            Assert.Equal(S1Harness.AgencyOfficeId, order.SalesContext.SellingOfficeId);
            Assert.Equal(S1Harness.AgencyCustomerId, order.FinancialCustomerId);

            await RepointAgencyAsync(harness, S1Harness.OtherCustomerId);

            try
            {
                var reread = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);

                Assert.Equal(S1Harness.TravelAgencyId, reread.SalesContext.SellerId);
                Assert.Equal(S1Harness.AgencyCustomerId, reread.FinancialCustomerId);
            }
            finally
            {
                await RepointAgencyAsync(harness, S1Harness.AgencyCustomerId);
            }
        }

        [Fact]
        public async Task The_public_order_exposes_the_selling_office_with_its_namespace()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);
            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));

            var dto = await harness.SendAsync(new BackofficeGetOrderByIdQuery(created.OrderId));

            Assert.Equal(S1Harness.AirlineOfficeId, dto.SellingOfficeId);
            Assert.Equal(SellingOfficeKind.AirlineOffice, dto.SellingOfficeKind);
        }

        [Fact]
        public async Task An_agency_office_is_never_returned_under_an_airline_office_label()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await PublishAgencyOfferAsync(harness);
            harness.Caller.UseOtaPanel(S1Harness.TravelAgencyId, S1Harness.AgencyOfficeId, S1Harness.AgencyUserId);
            var created = await harness.SendAsync(S1Commands.OtaPanel(offerId));

            var dto = await harness.SendAsync(new OtaPanelGetOrderByIdQuery(created.OrderId));

            Assert.Equal(S1Harness.AgencyOfficeId, dto.SellingOfficeId);
            Assert.Equal(SellingOfficeKind.TravelAgencyOffice, dto.SellingOfficeKind);
        }

        private static async Task<string> PublishAgencyOfferAsync(S1Harness harness)
        {
            var offerId = $"AGENCY-{Guid.NewGuid():N}";

            await harness.Catalog.PublishAsync(
                new CandidateBuilder(harness.Clock.GetDateTime(), S1Harness.OtaPanelScope())
                    .Traveller("PAX-A")
                    .Segment("SEG-1")
                    .AirService("S-A", "PAX-A", "SEG-1")
                    .Package("ITEM-A", "S-A")
                    .Line("FARE", "ITEM-A", PricingComponentType.Fare, 100m, "S-A")
                    .Offer(offerId)
                    .Build());

            return offerId;
        }

        private static Task RepointAgencyAsync(S1Harness harness, long customerId)
            => harness.InScopeAsync(async services =>
            {
                var reference = services.GetRequiredService<ReferenceDbContext>();

                await reference.Customers
                    .Where(customer => customer.TravelAgencyId == S1Harness.TravelAgencyId)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(customer => customer.TravelAgencyId, (long?)null));

                return await reference.Customers
                    .Where(customer => customer.Id == customerId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(customer => customer.TravelAgencyId, S1Harness.TravelAgencyId)
                        .SetProperty(customer => customer.Type, AeroTech.Ordering.ReferenceData.ReadModels.CustomerType.TravelAgency));
            });
    }
}
