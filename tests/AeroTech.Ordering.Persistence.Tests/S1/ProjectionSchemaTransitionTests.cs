using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Application.OrderAggregate.Commands.RebuildOrderProjection;
using AeroTech.Ordering.Domain.Tests._Shared;
using AeroTech.Ordering.Persistence.Tests._Shared;
using AeroTech.Ordering.Query._Shared.DbContexts;
using AeroTech.Ordering.Query.OrderAggregate.Dto;
using AeroTech.Ordering.Query.OrderAggregate.Models;
using AeroTech.Ordering.Query.OrderAggregate.Projection;
using AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrderById.Backoffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class ProjectionSchemaTransitionTests
    {
        private readonly OrderingDatabaseFixture _fixture;

        public ProjectionSchemaTransitionTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task A_schema_two_projection_row_stays_readable_and_a_rebuild_moves_it_to_schema_three()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);
            var orderId = (await harness.SendAsync(S1Commands.Backoffice(offerId))).OrderId;

            var current = await harness.InScopeAsync(services => services.GetRequiredService<OrderQueryDbContext>()
                .Set<OrderDetailsReadModel>().AsNoTracking().SingleAsync(row => row.OrderId == orderId));

            Assert.Equal(OrderProjectionJson.SchemaVersion, current.ProjectionSchemaVersion);

            var legacy = OrderDtoJson.Write(OrderProjectionMapper.ToPublicOrder(OrderProjectionJson.Read(current.DetailsJson)));

            await harness.InScopeAsync(services => services.GetRequiredService<OrderQueryDbContext>()
                .Set<OrderDetailsReadModel>().Where(row => row.OrderId == orderId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(row => row.ProjectionSchemaVersion, OrderDtoJson.SchemaVersion)
                    .SetProperty(row => row.DetailsJson, legacy)));

            var readBack = await harness.SendAsync(new BackofficeGetOrderByIdQuery(orderId));
            Assert.Equal(orderId, readBack.OrderId);
            Assert.Equal(CommercialSummary.Active, readBack.Status);
            Assert.Equal("120", readBack.GrandTotal.Amount);

            await harness.SendAsync(new RebuildOrderProjectionCommand(orderId, S1Commands.NewKey("schema3")));

            var rebuilt = await harness.InScopeAsync(services => services.GetRequiredService<OrderQueryDbContext>()
                .Set<OrderDetailsReadModel>().AsNoTracking().SingleAsync(row => row.OrderId == orderId));

            Assert.Equal(OrderProjectionJson.SchemaVersion, rebuilt.ProjectionSchemaVersion);
            Assert.Equal(current.DetailsJson, rebuilt.DetailsJson);
        }
    }
}
