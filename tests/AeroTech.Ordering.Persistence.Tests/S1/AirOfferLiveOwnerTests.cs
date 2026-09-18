using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderPreparationAggregate;
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
        public async Task Recorded_live_owner_response_is_normalized_and_prepared()
        {
            var body = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, RecordedResponse));
            var handler = new AirOfferWireFixtures.StubHandler(() => body);

            await using var harness = await S1Harness.StartAsync(
                _fixture,
                services => services.ConfigureHttpClientDefaults(client => client.ConfigurePrimaryHttpMessageHandler(() => handler)),
                useReferenceOffers: false);

            var preparation = await harness.SendAsync(S1Commands.Prepare("LIVE-RECORDED-OFFER"));
            var candidate = preparation.Candidate;
            var charge = candidate.PricingLines.Single(line => line.SourceLineRef == "orderCharges/0");

            Assert.Equal(AcceptanceAssurance.LocalCandidateOnly, candidate.AcceptanceAssurance);
            Assert.Equal(AirOfferProfile.LiveCandidateSandbox, preparation.PermittedAcceptanceProfile);
            Assert.Contains(OrderPreparation.LiveAcceptanceBlocked, preparation.BlockingReasons);

            Assert.Equal(264005012m, candidate.CustomerTotal.Amount);
            Assert.Equal("70", candidate.CustomerTotal.CurrencyRef);
            Assert.Single(candidate.Items);
            Assert.Equal(2, candidate.Services.Count);
            Assert.Equal(2, candidate.Segments.Count);
            Assert.Single(candidate.Travelers);

            Assert.Equal(24000000m, charge.SaleValue.Amount);
            Assert.Equal(24000000m, charge.OriginalValue.Amount);
            Assert.Equal("70", charge.SaleValue.CurrencyRef);
            Assert.Equal(PricingComponentType.Tax, charge.Component);

            var equivalentFare = candidate.PricingLines.First(line => line.SourceConversionRef == "1533121255006273536");
            Assert.Equal(120m, equivalentFare.OriginalValue.Amount);
            Assert.Equal("155", equivalentFare.OriginalValue.CurrencyRef);
            Assert.Equal(120000000m, equivalentFare.SaleValue.Amount);

            Assert.All(candidate.Services, service => Assert.Equal(AirOfferCandidateMapper.FulfillmentProfileRef, service.FulfillmentProfile.ProfileRef));
        }

        [Trait("Category", "Live")]
        [Fact]
        public async Task Live_owner_details_resolve_into_an_acceptable_candidate()
        {
            var baseUrl = Environment.GetEnvironmentVariable(LiveBaseUrlVariable);
            var offerId = Environment.GetEnvironmentVariable(LiveOfferVariable);

            Assert.False(string.IsNullOrWhiteSpace(baseUrl), $"{LiveBaseUrlVariable} must point at the live AirOffer service.");
            Assert.False(string.IsNullOrWhiteSpace(offerId), $"{LiveOfferVariable} must carry a currently priced offer id.");

            await using var harness = await S1Harness.StartAsync(_fixture, useReferenceOffers: false, offerBaseUrl: baseUrl);

            var preparation = await harness.SendAsync(S1Commands.Prepare(offerId!));

            _output.WriteLine($"offerId: {offerId}");
            _output.WriteLine($"preparationId: {preparation.PreparationId}");
            _output.WriteLine($"acceptedSnapshotDigest: {preparation.AcceptedSnapshotDigest}");
            _output.WriteLine($"acceptanceAssurance: {preparation.AcceptanceAssurance}");
            _output.WriteLine($"permittedAcceptanceProfile: {preparation.PermittedAcceptanceProfile}");
            _output.WriteLine($"blockingReasons: {string.Join(", ", preparation.BlockingReasons)}");
            _output.WriteLine($"customerTotal: {preparation.Candidate.CustomerTotal.Amount} {preparation.Candidate.CustomerTotal.CurrencyRef}");
            _output.WriteLine($"travelers: {preparation.Candidate.Travelers.Count}, services: {preparation.Candidate.Services.Count}, lines: {preparation.Candidate.PricingLines.Count}");
            _output.WriteLine($"candidate: {NormalizedCandidateJson.Write(preparation.Candidate)}");

            Assert.Equal(offerId, preparation.SourceOfferId);
            Assert.Equal(AcceptanceAssurance.LocalCandidateOnly, preparation.AcceptanceAssurance);
            Assert.Contains(OrderPreparation.LiveAcceptanceBlocked, preparation.BlockingReasons);
            Assert.NotEmpty(preparation.Candidate.Services);
            Assert.True(preparation.Candidate.CustomerTotal.Amount > 0);
        }
    }
}
