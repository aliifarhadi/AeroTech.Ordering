using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Serialization;
using AeroTech.Ordering.Persistence.Tests._Shared;
using AeroTech.Ordering.Providers.AirOffer;
using AeroTech.Ordering.Providers.AirOffer.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class AirOfferLiveOwnerTests
    {
        public const string RecordedResponse = "S1/LiveFixtures/airoffer-live-details.json";
        public const string LiveBaseUrlVariable = "ORDERING_LIVE_AIROFFER_BASEURL";
        public const string LiveOfferVariable = "ORDERING_LIVE_AIROFFER_OFFERID";

        private readonly OrderingDatabaseFixture _fixture;
        private readonly ITestOutputHelper _output;

        public AirOfferLiveOwnerTests(OrderingDatabaseFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task Recorded_live_owner_response_is_normalized_and_sold()
        {
            var body = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, RecordedResponse));
            var handler = new AirOfferWireFixtures.StubHandler(() => body);

            await using var harness = await S1Harness.StartAsync(
                _fixture,
                services => services.ConfigureHttpClientDefaults(client => client.ConfigurePrimaryHttpMessageHandler(() => handler)),
                useReferenceOffers: false);

            var created = await harness.SendAsync(S1Commands.Backoffice("LIVE-RECORDED-OFFER", travellers: S1Commands.Travellers("ADT-1")));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);
            var candidate = await AirOfferLiveCandidateBridgeTests.AcceptedCandidateAsync(harness, created.OrderId);
            var charge = candidate.PricingLines.Single(line => line.SourceLineRef == "orderCharges/0");

            Assert.Equal(AcceptanceAssurance.LocalCandidateOnly, candidate.AcceptanceAssurance);
            Assert.Equal(AirOfferProfile.LiveCandidateSandbox, order.AcceptedSource.AcceptanceProfile);
            Assert.Equal(264005012m, order.CustomerTotal.Amount);
            Assert.Equal("70", order.CustomerTotal.CurrencyRef);
            Assert.Single(order.Items);
            Assert.Equal(2, order.Services.Count);
            Assert.Equal(2, order.Segments.Count);
            Assert.Single(order.Travelers);

            Assert.Equal(24000000m, charge.SaleValue.Amount);
            Assert.Equal(24000000m, charge.OriginalValue.Amount);
            Assert.Equal(PricingComponentType.Tax, charge.Component);

            var equivalentFare = candidate.PricingLines.First(line => line.SourceConversionRef == "1533121255006273536");
            Assert.Equal(120m, equivalentFare.OriginalValue.Amount);
            Assert.Equal("155", equivalentFare.OriginalValue.CurrencyRef);
            Assert.Equal(120000000m, equivalentFare.SaleValue.Amount);

            Assert.All(candidate.Services, service => Assert.Equal(AirOfferCandidateMapper.FulfillmentProfileRef, service.FulfillmentProfile.ProfileRef));
        }

        [Trait("Category", "Live")]
        [Fact]
        public async Task Live_owner_details_are_sold_into_a_local_order()
        {
            var baseUrl = Environment.GetEnvironmentVariable(LiveBaseUrlVariable);
            var offerId = Environment.GetEnvironmentVariable(LiveOfferVariable);

            Assert.False(string.IsNullOrWhiteSpace(baseUrl), $"{LiveBaseUrlVariable} must point at the live AirOffer service.");
            Assert.False(string.IsNullOrWhiteSpace(offerId), $"{LiveOfferVariable} must carry a currently priced offer id.");

            await using var harness = await S1Harness.StartAsync(_fixture, useReferenceOffers: false, offerBaseUrl: baseUrl);

            var created = await harness.SendAsync(S1Commands.Backoffice(offerId!, travellers: S1Commands.Travellers("ADT-1")));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);
            var candidate = await AirOfferLiveCandidateBridgeTests.AcceptedCandidateAsync(harness, created.OrderId);

            _output.WriteLine($"offerId: {offerId}");
            _output.WriteLine($"orderId: {created.OrderId}");
            _output.WriteLine($"orderReference: {created.OrderReference}");
            _output.WriteLine($"grandTotal: {created.GrandTotal} {created.CurrencyRef}");
            _output.WriteLine($"acceptanceProfile: {order.AcceptedSource.AcceptanceProfile}");
            _output.WriteLine($"acceptanceAssurance: {order.AcceptedSource.AcceptanceAssurance}");
            _output.WriteLine($"services: {order.Services.Count}, segments: {order.Segments.Count}, pricingLines: {order.PricingLines.Count}");
            _output.WriteLine($"candidate: {NormalizedCandidateJson.Write(candidate)}");

            Assert.Equal(offerId, order.AcceptedSource.SourceOfferId);
            Assert.Equal(AcceptanceAssurance.LocalCandidateOnly, order.AcceptedSource.AcceptanceAssurance);
            Assert.NotEmpty(order.Services);
            Assert.True(order.CustomerTotal.Amount > 0);
        }
    }
}
