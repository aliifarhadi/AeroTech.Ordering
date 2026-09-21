using System.Net.Http;
using System.Text.Json;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Ordering.Domain.OrderPreparationAggregate;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Entities;
using AeroTech.Ordering.Persistence.Tests._Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class SourceEvidenceRetentionTests
    {
        private const string OfferId = "SYNTHETIC-PRICED-OFFER";
        private const int UnsupportedCapability = 20273;

        private readonly OrderingDatabaseFixture _fixture;

        public SourceEvidenceRetentionTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Coupon_identity_and_sequence_dropped_from_the_domain_model_stay_readable_in_raw_evidence()
        {
            var handler = new AirOfferWireFixtures.StubHandler(() => AirOfferWireFixtures.Details());
            await using var harness = await StartAsync(handler);

            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));
            var evidence = await EvidenceAsync(harness, created.OrderId);
            var coupon = SingleCoupon(evidence.Payload);

            Assert.Equal("T1-7001", coupon.GetProperty("couponId").GetString());
            Assert.Equal(1, coupon.GetProperty("sequence").GetInt32());
            Assert.Equal(0, SingleTicket(evidence.Payload).GetProperty("travellerIndex").GetInt32());
        }

        [Fact]
        public async Task The_retained_payload_is_the_exact_owner_response_and_matches_its_recorded_hash()
        {
            var handler = new AirOfferWireFixtures.StubHandler(() => AirOfferWireFixtures.Details());
            await using var harness = await StartAsync(handler);

            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));
            var evidence = await EvidenceAsync(harness, created.OrderId);

            Assert.Equal(AirOfferWireFixtures.Details(), evidence.Payload);
            Assert.Equal(Sha256(evidence.Payload), evidence.PayloadHash);
            Assert.Equal(64, evidence.PayloadHash.Length);
        }

        [Fact]
        public async Task Evidence_only_facts_are_not_promoted_into_the_normalized_candidate()
        {
            var handler = new AirOfferWireFixtures.StubHandler(() => AirOfferWireFixtures.Details());
            await using var harness = await StartAsync(handler);

            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));
            var preparation = await AirOfferLiveCandidateBridgeTests.AcceptedPreparationAsync(harness, created.OrderId);

            Assert.DoesNotContain("couponId", preparation.CandidateJson);
            Assert.DoesNotContain("travellerIndex", preparation.CandidateJson);
            Assert.Contains("\"couponId\"", (await EvidenceAsync(harness, created.OrderId)).Payload);
        }

        [Fact]
        public async Task A_flight_that_supplies_a_stop_payload_is_an_unsupported_capability_and_persists_no_order()
        {
            var handler = new AirOfferWireFixtures.StubHandler(() => AirOfferWireFixtures.Details(
                flightStop: new { count = 1, airportId = 33 }));
            await using var harness = await StartAsync(handler);
            var ordersBefore = await OrderCountAsync(harness);

            var failure = await Assert.ThrowsAsync<BusinessException>(
                () => harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1"))));

            Assert.Equal(UnsupportedCapability, failure.Code);
            Assert.Contains("stop payload", failure.Message);
            Assert.Contains("BD-006", failure.Message);
            Assert.Equal(ordersBefore, await OrderCountAsync(harness));
        }

        [Fact]
        public async Task A_leg_that_supplies_a_stop_payload_is_an_unsupported_capability_and_persists_no_order()
        {
            var handler = new AirOfferWireFixtures.StubHandler(() => AirOfferWireFixtures.Details(
                legStop: new { count = 1, airportId = 33 }));
            await using var harness = await StartAsync(handler);
            var ordersBefore = await OrderCountAsync(harness);

            var failure = await Assert.ThrowsAsync<BusinessException>(
                () => harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1"))));

            Assert.Equal(UnsupportedCapability, failure.Code);
            Assert.Contains("leg 81", failure.Message);
            Assert.Contains("stop payload", failure.Message);
            Assert.Equal(ordersBefore, await OrderCountAsync(harness));
        }

        [Fact]
        public async Task An_explicit_json_null_stop_is_not_a_supplied_stop()
        {
            var handler = new AirOfferWireFixtures.StubHandler(() => AirOfferWireFixtures.Details());
            await using var harness = await StartAsync(handler);

            Assert.Contains("\"stop\":null", AirOfferWireFixtures.Details());

            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));

            Assert.True(created.OrderId > 0);
        }

        private static Task<int> OrderCountAsync(S1Harness harness)
            => CreateOrderFromOfferTests.CountAsync<Domain.OrderAggregate.Order>(harness, order => order.Id > 0);

        private static async Task<PreparationSourceEvidence> EvidenceAsync(S1Harness harness, long orderId)
        {
            var preparationId = (await CreateOrderFromOfferTests.LoadOrderAsync(harness, orderId)).SourcePreparationId;

            var rows = await harness.InScopeAsync(services => services.GetRequiredService<OrderingDbContext>()
                .Set<PreparationSourceEvidence>().AsNoTracking()
                .Where(evidence => evidence.PreparationId == preparationId)
                .ToListAsync());

            return Assert.Single(rows);
        }

        private static JsonElement SingleTicket(string payload)
            => JsonDocument.Parse(payload).RootElement.GetProperty("data").GetProperty("tickets").EnumerateArray().Single();

        private static JsonElement SingleCoupon(string payload)
            => SingleTicket(payload).GetProperty("coupons").EnumerateArray().Single();

        private static string Sha256(string payload)
            => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();

        private Task<S1Harness> StartAsync(HttpMessageHandler handler)
            => S1Harness.StartAsync(_fixture, services =>
                services.ConfigureHttpClientDefaults(client => client.ConfigurePrimaryHttpMessageHandler(() => handler)),
                useReferenceOffers: false);
    }
}
