using System.Net.Http;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Application.OrderAggregate.Commands.RebuildOrderProjection;
using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence.Tests._Shared;
using AeroTech.Ordering.Query._Shared.DbContexts;
using AeroTech.Ordering.Query.OrderAggregate.Models;
using AeroTech.Ordering.Query.OrderAggregate.Projection;
using AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrderById.Backoffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class AcceptedShapePersistenceTests
    {
        private const string OfferId = "SYNTHETIC-PRICED-OFFER";

        private readonly OrderingDatabaseFixture _fixture;

        public AcceptedShapePersistenceTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task The_service_type_survives_sql_the_projection_and_the_public_order()
        {
            await using var harness = await StartAsync(Handler());

            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);

            var air = Assert.IsType<OrderAirTransportService>(Assert.Single(order.Services));

            Assert.Equal(OrderServiceType.AirTransportation, air.ServiceType);
            Assert.True(air.TravellerId > 0);
            Assert.True(air.SegmentId > 0);

            var discriminator = await harness.InScopeAsync(async services =>
            {
                var context = services.GetRequiredService<OrderingDbContext>();
                await using var command = context.Database.GetDbConnection().CreateCommand();
                command.CommandText = $"SELECT [ServiceType] FROM [Order].[OrderServices] WHERE [Id] = {air.Id}";

                if (command.Connection!.State != System.Data.ConnectionState.Open)
                    await command.Connection.OpenAsync();

                return Convert.ToInt32(await command.ExecuteScalarAsync());
            });

            Assert.Equal((int)OrderServiceType.AirTransportation, discriminator);

            var document = await ProjectionAsync(harness, created.OrderId);
            var projected = Assert.Single(Assert.Single(document.Items).Services);

            Assert.Equal(OrderServiceType.AirTransportation, projected.ServiceType);

            var publicOrder = await harness.SendAsync(new BackofficeGetOrderByIdQuery(created.OrderId));

            Assert.Equal(OrderServiceType.AirTransportation, Assert.Single(Assert.Single(publicOrder.Items).Services).ServiceType);
        }

        [Fact]
        public async Task The_fulfillment_snapshot_persists_honest_unresolved_values_with_no_invented_profile()
        {
            await using var harness = await StartAsync(Handler());

            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);
            var profile = Assert.Single(order.Services).FulfillmentProfile;

            Assert.Null(profile.ProfileRef);
            Assert.Null(profile.ProfileVersion);
            Assert.Equal(FulfillmentProfileAssurance.NotCertified, profile.Assurance);
            Assert.Equal(ReservationRequirement.Unresolved, profile.ReservationRequirement);
            Assert.Equal(FulfillmentDocumentKind.Unresolved, profile.DocumentKind);
            Assert.Equal(FundingRequirement.Unresolved, profile.FundingRequirement);

            var invented = await CreateOrderFromOfferTests.CountAsync<OrderAirTransportService>(
                harness,
                service => service.OrderId == created.OrderId && service.FulfillmentProfile.ProfileRef != null);

            Assert.Equal(0, invented);
        }

        [Fact]
        public async Task The_sale_currency_code_snapshot_survives_sql_and_the_public_order()
        {
            await using var harness = await StartAsync(Handler());

            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);

            Assert.Equal(978, order.CurrencyId);
            Assert.Equal("EUR", order.SaleCurrencyCode);

            var publicOrder = await harness.SendAsync(new BackofficeGetOrderByIdQuery(created.OrderId));

            Assert.Equal("EUR", publicOrder.SaleCurrencyCode);
            Assert.Equal(978, publicOrder.CustomerTotal.CurrencyId);
        }

        [Fact]
        public async Task The_original_sale_persists_one_item_service_link_per_service()
        {
            await using var harness = await StartAsync(Handler());

            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);

            var link = Assert.Single(order.ItemServiceLinks);
            var service = Assert.Single(order.Services);

            Assert.Equal(created.OrderId, link.OrderIdAtAssociation);
            Assert.Equal(service.OrderItemId, link.OrderItemId);
            Assert.Equal(service.Id, link.OrderServiceId);
            Assert.Equal(1, await CreateOrderFromOfferTests.CountAsync<OrderItemServiceLink>(
                harness,
                row => row.OrderIdAtAssociation == created.OrderId));
        }

        [Fact]
        public async Task Every_persisted_pricing_line_records_the_original_role()
        {
            await using var harness = await StartAsync(Handler());

            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);

            Assert.NotEmpty(order.PricingLines);
            Assert.All(order.PricingLines, line => Assert.Equal(PricingLineRole.Original, line.Role));

            var document = await ProjectionAsync(harness, created.OrderId);

            Assert.All(document.Pricing, line => Assert.Equal(PricingLineRole.Original, line.Role));
        }

        [Fact]
        public async Task A_projection_rebuild_returns_the_receipt_identifier_under_its_own_name()
        {
            await using var harness = await StartAsync(Handler());

            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));
            var key = S1Commands.NewKey("rebuild-receipt");

            var rebuilt = await harness.SendAsync(new RebuildOrderProjectionCommand(created.OrderId, key));
            var replayed = await harness.SendAsync(new RebuildOrderProjectionCommand(created.OrderId, key));

            Assert.Equal(rebuilt.ReceiptId, replayed.ReceiptId);
            Assert.True(replayed.ReplayedFromReceipt);
            Assert.Equal(1, await CreateOrderFromOfferTests.CountAsync<Domain.CommandReceiptAggregate.CommandReceipt>(
                harness,
                receipt => receipt.Id == rebuilt.ReceiptId));
            Assert.DoesNotContain("OperationId", typeof(RebuildOrderProjectionResult).GetProperties().Select(property => property.Name));
        }

        [Theory]
        [InlineData("OneWay", FarePricingUnitType.OneWay, AirFareConstructionType.OneWay)]
        [InlineData("RoundTripFromOneWays", FarePricingUnitType.RoundTrip, AirFareConstructionType.RoundTripFromOneWays)]
        [InlineData("RoundTripFare", FarePricingUnitType.RoundTrip, AirFareConstructionType.RoundTrip)]
        public async Task Every_owner_pricing_unit_kind_maps_to_its_own_construction_type(
            string kind,
            FarePricingUnitType expectedType,
            AirFareConstructionType expectedSourceType)
        {
            await using var harness = await StartAsync(new AirOfferWireFixtures.StubHandler(
                () => AirOfferWireFixtures.Details(pricingUnitKind: kind)));

            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);
            var construction = Assert.Single(order.FareConstructions);
            var unit = Assert.Single(construction.PricingUnits);

            Assert.Equal(FareConstructionAssurance.SourceProvided, construction.Assurance);
            Assert.Equal(expectedType, unit.Type);
            Assert.Equal(expectedSourceType, unit.SourceConstructionType);
            Assert.Equal(order.Items.Single().Id, Assert.Single(construction.Items).OrderItemId);
        }

        [Fact]
        public async Task An_unknown_owner_pricing_unit_kind_fails_closed()
        {
            await using var harness = await StartAsync(new AirOfferWireFixtures.StubHandler(
                () => AirOfferWireFixtures.Details(pricingUnitKind: "RoundTrip")));

            var exception = await Assert.ThrowsAsync<BusinessException>(() =>
                harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1"))));

            Assert.Equal(20272, exception.Code);
            Assert.Contains("pricing unit kind RoundTrip", exception.Message);
        }

        [Fact]
        public async Task An_owner_that_supplies_no_pricing_units_commits_no_fare_construction()
        {
            await using var harness = await StartAsync(new AirOfferWireFixtures.StubHandler(
                () => AirOfferWireFixtures.Details(withPricingUnits: false)));

            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);
            var candidate = await AirOfferLiveCandidateBridgeTests.AcceptedCandidateAsync(harness, created.OrderId);

            Assert.Null(candidate.FareConstruction);
            Assert.Empty(order.FareConstructions);
        }

        private static AirOfferWireFixtures.StubHandler Handler()
            => new(() => AirOfferWireFixtures.Details(withTerminals: true, ticketingRestrictionMinutes: 45));

        private Task<S1Harness> StartAsync(HttpMessageHandler handler)
            => S1Harness.StartAsync(_fixture, services =>
                services.ConfigureHttpClientDefaults(client => client.ConfigurePrimaryHttpMessageHandler(() => handler)),
                useReferenceOffers: false);

        private static async Task<OrderProjectionDocument> ProjectionAsync(S1Harness harness, long orderId)
            => OrderProjectionJson.Read(await harness.InScopeAsync(services => services.GetRequiredService<OrderQueryDbContext>()
                .Set<OrderDetailsReadModel>().AsNoTracking()
                .Where(row => row.OrderId == orderId)
                .Select(row => row.DetailsJson)
                .SingleAsync()));
    }
}
