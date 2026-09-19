using System.Text.Json;
using System.Text.Json.Serialization;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Application.OrderAggregate.Commands.RebuildOrderProjection;
using AeroTech.Ordering.Domain.Tests._Shared;
using AeroTech.Ordering.Persistence.Tests._Shared;
using AeroTech.Ordering.Query._Shared.DbContexts;
using AeroTech.Ordering.Query.OrderAggregate.Dto;
using AeroTech.Ordering.Query.OrderAggregate.Models;
using AeroTech.Ordering.Query.OrderAggregate.Projection;
using AeroTech.Ordering.Query.OrderAggregate.Projection.Compatibility;
using AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrderById.Backoffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class ProjectionSchemaTransitionTests
    {
        private static readonly JsonSerializerOptions LegacyOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() },
            WriteIndented = false
        };

        private readonly OrderingDatabaseFixture _fixture;

        public ProjectionSchemaTransitionTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task A_schema_two_projection_row_stays_readable_and_a_rebuild_moves_it_to_schema_four()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var (orderId, current) = await AcceptAsync(harness);

            var publicOrder = OrderProjectionMapper.ToPublicOrder(OrderProjectionJson.Read(current.DetailsJson));

            var legacy = JsonSerializer.Serialize(
                new OrderDtoV2Document(
                    publicOrder.OrderId,
                    publicOrder.OrderReference,
                    publicOrder.Status,
                    publicOrder.CommercialVersion,
                    publicOrder.OfferId,
                    publicOrder.Channel,
                    publicOrder.CustomerId,
                    publicOrder.SellingOfficeId,
                    publicOrder.GrandTotal,
                    publicOrder.Travellers,
                    publicOrder.Contacts,
                    publicOrder.Itinerary,
                    publicOrder.Items,
                    publicOrder.Pricing,
                    publicOrder.CreationDate),
                LegacyOptions);

            Assert.Contains("\"airlineOfficeId\"", legacy);
            Assert.DoesNotContain("\"sellingOfficeKind\"", legacy);

            await ReplaceAsync(harness, orderId, OrderDtoJson.SchemaVersion, legacy);
            await AssertRebuildsToSchemaFourAsync(harness, orderId, current.DetailsJson, "schema2");
        }

        [Fact]
        public async Task A_schema_three_projection_row_stays_readable_and_a_rebuild_moves_it_to_schema_four()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var (orderId, current) = await AcceptAsync(harness);

            var document = OrderProjectionJson.Read(current.DetailsJson);

            var legacy = JsonSerializer.Serialize(
                new OrderProjectionV3Document(
                    document.OrderId,
                    document.OrderReference,
                    document.Status,
                    document.CommercialVersion,
                    document.OfferId,
                    document.Channel,
                    document.CustomerId,
                    document.SalesContext.SellingOfficeId,
                    document.GrandTotal,
                    document.Travellers,
                    document.Segments,
                    document.Items,
                    document.Pricing,
                    document.CreationDate),
                LegacyOptions);

            Assert.Contains("\"airlineOfficeId\"", legacy);
            Assert.DoesNotContain("\"componentTotals\"", legacy);

            await ReplaceAsync(harness, orderId, OrderProjectionJson.LegacyInternalSchemaVersion, legacy);
            await AssertRebuildsToSchemaFourAsync(harness, orderId, current.DetailsJson, "schema3");
        }

        [Fact]
        public async Task A_legacy_row_never_gains_facts_that_were_not_recorded_in_it()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var (orderId, current) = await AcceptAsync(harness);

            var document = OrderProjectionJson.Read(current.DetailsJson);

            var legacy = JsonSerializer.Serialize(
                new OrderProjectionV3Document(
                    document.OrderId,
                    document.OrderReference,
                    document.Status,
                    document.CommercialVersion,
                    document.OfferId,
                    document.Channel,
                    document.CustomerId,
                    null,
                    document.GrandTotal,
                    document.Travellers,
                    document.Segments,
                    document.Items,
                    document.Pricing,
                    document.CreationDate),
                LegacyOptions);

            await ReplaceAsync(harness, orderId, OrderProjectionJson.LegacyInternalSchemaVersion, legacy);

            var readBack = await harness.SendAsync(new BackofficeGetOrderByIdQuery(orderId));

            Assert.Null(readBack.SellingOfficeId);
            Assert.Null(readBack.SellingOfficeKind);
        }

        private static async Task<(long OrderId, OrderDetailsReadModel Row)> AcceptAsync(S1Harness harness)
        {
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);
            var orderId = (await harness.SendAsync(S1Commands.Backoffice(offerId))).OrderId;

            var row = await harness.InScopeAsync(services => services.GetRequiredService<OrderQueryDbContext>()
                .Set<OrderDetailsReadModel>().AsNoTracking().SingleAsync(model => model.OrderId == orderId));

            Assert.Equal(OrderProjectionJson.SchemaVersion, row.ProjectionSchemaVersion);

            return (orderId, row);
        }

        private static Task ReplaceAsync(S1Harness harness, long orderId, int schemaVersion, string detailsJson)
            => harness.InScopeAsync(services => services.GetRequiredService<OrderQueryDbContext>()
                .Set<OrderDetailsReadModel>().Where(row => row.OrderId == orderId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(row => row.ProjectionSchemaVersion, schemaVersion)
                    .SetProperty(row => row.DetailsJson, detailsJson)));

        private static async Task AssertRebuildsToSchemaFourAsync(S1Harness harness, long orderId, string expectedJson, string key)
        {
            var readBack = await harness.SendAsync(new BackofficeGetOrderByIdQuery(orderId));

            Assert.Equal(orderId, readBack.OrderId);
            Assert.Equal(CommercialSummary.Active, readBack.Status);
            Assert.Equal("120", readBack.GrandTotal.Amount);
            Assert.Equal(SellingOfficeKind.AirlineOffice, readBack.SellingOfficeKind);
            Assert.Equal(S1Harness.AirlineOfficeId, readBack.SellingOfficeId);

            await harness.SendAsync(new RebuildOrderProjectionCommand(orderId, S1Commands.NewKey(key)));

            var rebuilt = await harness.InScopeAsync(services => services.GetRequiredService<OrderQueryDbContext>()
                .Set<OrderDetailsReadModel>().AsNoTracking().SingleAsync(row => row.OrderId == orderId));

            Assert.Equal(OrderProjectionJson.SchemaVersion, rebuilt.ProjectionSchemaVersion);
            Assert.Equal(expectedJson, rebuilt.DetailsJson);
        }
    }
}
