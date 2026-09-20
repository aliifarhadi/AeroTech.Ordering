using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Application.OrderAggregate.Commands.RebuildOrderProjection;
using AeroTech.Ordering.Domain.Tests._Shared;
using AeroTech.Ordering.Persistence.Tests._Shared;
using AeroTech.Ordering.Query._Shared.DbContexts;
using AeroTech.Ordering.Query.OrderAggregate.Models;
using AeroTech.Ordering.Query.OrderAggregate.Projection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class MultipleTaxOccurrenceTests
    {
        private readonly OrderingDatabaseFixture _fixture;

        public MultipleTaxOccurrenceTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Every_tax_occurrence_on_one_ticket_survives_acceptance_sql_and_rebuild()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);

            var offerId = await CreateOrderFromOfferTests.PublishAsync(harness, (now, scope) => new CandidateBuilder(now, scope)
                .Traveller("PAX-A")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .Package("ITEM-A", "S-A")
                .Line("tickets/0/coupons/0/pricings/0", "ITEM-A", PricingComponentType.Fare, 100m, "S-A", code: "YOW", name: "Fare", reference: "9001")
                .Line("tickets/0/coupons/0/pricings/1", "ITEM-A", PricingComponentType.Tax, 12m, "S-A", code: "AT", name: "Airport tax", reference: "AT-1")
                .Line("tickets/0/coupons/0/pricings/2", "ITEM-A", PricingComponentType.Tax, 7m, "S-A", code: "YQ", name: "Carrier imposed", reference: "YQ-1")
                .Line("tickets/0/coupons/0/pricings/3", "ITEM-A", PricingComponentType.Tax, 3m, "S-A", code: "AT", name: "Airport tax", reference: "AT-2")
                .Line("tickets/0/coupons/0/pricings/4", "ITEM-A", PricingComponentType.Tax, 1.5m, "S-A", code: "XT", name: "Other tax", reference: null));

            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));
            var candidate = await AirOfferLiveCandidateBridgeTests.AcceptedCandidateAsync(harness, created.OrderId);
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);

            var taxes = order.PricingLines.Where(line => line.Component == PricingComponentType.Tax).ToList();

            Assert.Equal(4, candidate.PricingLines.Count(line => line.Component == PricingComponentType.Tax));
            Assert.Equal(4, taxes.Count);
            Assert.Equal(5, order.PricingLines.Count);

            Assert.Equal(
                ["tickets/0/coupons/0/pricings/1", "tickets/0/coupons/0/pricings/2", "tickets/0/coupons/0/pricings/3", "tickets/0/coupons/0/pricings/4"],
                taxes.Select(line => line.SourceOccurrencePath).OrderBy(path => path, StringComparer.Ordinal));

            Assert.Equal(taxes.Count, taxes.Select(line => line.SourceOccurrencePath).Distinct(StringComparer.Ordinal).Count());
            Assert.Equal(2, taxes.Count(line => line.Code == "AT"));
            Assert.Equal(
                ["AT-1", "AT-2"],
                taxes.Where(line => line.Code == "AT").Select(line => line.Reference).OrderBy(value => value, StringComparer.Ordinal));
            Assert.Equal([1.5m, 3m, 7m, 12m], taxes.Select(line => line.SaleValue.Amount).OrderBy(amount => amount));

            Assert.Equal(123.5m, order.CustomerTotal.Amount);
            Assert.Equal(123.5m, Assert.Single(order.Items).AcceptedTotal.Amount);

            var persistedTaxRows = await CreateOrderFromOfferTests.CountAsync<Domain.OrderAggregate.Entities.PricingLine>(
                harness,
                line => line.OrderId == created.OrderId && line.Component == PricingComponentType.Tax);

            Assert.Equal(4, persistedTaxRows);

            var before = await ProjectionAsync(harness, created.OrderId);
            var projectedTaxTotal = before.ComponentTotals.Single(total => total.Component == PricingComponentType.Tax);

            Assert.Equal(4, before.Pricing.Count(line => line.Component == PricingComponentType.Tax));
            Assert.Equal("23.5", projectedTaxTotal.DebitAmount);
            Assert.Equal(
                taxes.Sum(line => line.SaleValue.Amount),
                decimal.Parse(projectedTaxTotal.DebitAmount, System.Globalization.CultureInfo.InvariantCulture));

            await harness.SendAsync(new RebuildOrderProjectionCommand(created.OrderId, S1Commands.NewKey("many-taxes")));

            var after = await ProjectionAsync(harness, created.OrderId);

            Assert.Equal(
                before.Pricing.Select(line => line.SourceOccurrencePath).OrderBy(path => path, StringComparer.Ordinal),
                after.Pricing.Select(line => line.SourceOccurrencePath).OrderBy(path => path, StringComparer.Ordinal));
            Assert.Equal("23.5", after.ComponentTotals.Single(total => total.Component == PricingComponentType.Tax).DebitAmount);
            Assert.Equal("123.5", after.CustomerTotal.Amount);
        }

        private static async Task<OrderProjectionDocument> ProjectionAsync(S1Harness harness, long orderId)
            => OrderProjectionJson.Read(await harness.InScopeAsync(services => services.GetRequiredService<OrderQueryDbContext>()
                .Set<OrderDetailsReadModel>().AsNoTracking()
                .Where(row => row.OrderId == orderId)
                .Select(row => row.DetailsJson)
                .SingleAsync()));
    }
}
