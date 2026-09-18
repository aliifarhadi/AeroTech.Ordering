using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AeroTech.Messages.Aegis.Enums;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Messages.Shared.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.Tests._Shared;
using AeroTech.Ordering.Persistence.Tests._Shared;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.Api
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class OrderingApiSurfaceTests : IAsyncLifetime
    {
        private readonly OrderingDatabaseFixture _fixture;
        private OrderingApiHost _host = null!;

        public OrderingApiSurfaceTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task InitializeAsync() => _host = await OrderingApiHost.StartAsync(_fixture);

        public async Task DisposeAsync() => await _host.DisposeAsync();

        [Fact]
        public async Task Backoffice_token_prepares_creates_and_reads_protected_travelers()
        {
            var offerId = await PublishAsync(S1Harness.Scope());
            var token = _host.BackofficeToken();

            var preparation = await PostAsync("backoffice/v1/order-preparations", PrepareBody(offerId, S1Harness.CustomerId, S1Harness.AirlineOfficeId), token);
            Assert.Equal(HttpStatusCode.Created, preparation.Status);

            var created = await PostAsync("backoffice/v1/orders/from-offer", CreateBody(preparation.Data, S1Harness.CustomerId, S1Harness.AirlineOfficeId), token);
            Assert.Equal(HttpStatusCode.Created, created.Status);
            Assert.Matches("^[23456789ABCDEFGHJKLMNPQRSTUVWXYZ]{8}$", created.Data.GetProperty("orderReference").GetString());
            Assert.Equal("1", created.Data.GetProperty("commercialVersionAtCommit").ToString());

            var orderId = created.Data.GetProperty("orderId").GetString();
            var details = await GetAsync($"backoffice/v1/orders/{orderId}", token);

            Assert.Equal(HttpStatusCode.OK, details.Status);
            Assert.Equal(orderId, details.Data.GetProperty("details").GetProperty("orderId").GetString());

            var traveler = Assert.Single(details.Data.GetProperty("protectedTravelers").EnumerateArray());
            Assert.Equal("Sample", traveler.GetProperty("givenName").GetString());
            Assert.Single(details.Data.GetProperty("protectedContacts").EnumerateArray());
        }

        [Fact]
        public async Task Service_surface_needs_no_token_and_never_returns_protected_payloads()
        {
            var offerId = await PublishAsync(S1Harness.ServiceScope());

            var preparation = await PostAsync("service/v1/order-preparations", PrepareBody(offerId, S1Harness.CustomerId, null), token: null);
            Assert.Equal(HttpStatusCode.Created, preparation.Status);

            var created = await PostAsync("service/v1/orders/from-offer", CreateBody(preparation.Data, S1Harness.CustomerId, null), token: null);
            Assert.Equal(HttpStatusCode.Created, created.Status);

            var orderId = created.Data.GetProperty("orderId").GetString();
            var details = await GetAsync($"service/v1/orders/{orderId}", token: null);

            Assert.Equal(HttpStatusCode.OK, details.Status);
            Assert.Empty(details.Data.GetProperty("protectedTravelers").EnumerateArray());
            Assert.Empty(details.Data.GetProperty("protectedContacts").EnumerateArray());
            Assert.DoesNotContain("Sample", details.Raw, StringComparison.Ordinal);
            Assert.DoesNotContain("traveler@example.invalid", details.Raw, StringComparison.Ordinal);
        }

        [Fact]
        public async Task OtaPanel_token_sells_for_the_customer_of_its_travel_agency()
        {
            var offerId = await PublishAsync(S1Harness.OtaPanelScope());
            var token = _host.OtaPanelToken();

            var preparation = await PostAsync("otapanel/v1/order-preparations", PrepareBody(offerId, null, null), token);
            Assert.Equal(HttpStatusCode.Created, preparation.Status);

            var created = await PostAsync("otapanel/v1/orders/from-offer", CreateBody(preparation.Data, null, null), token);
            Assert.Equal(HttpStatusCode.Created, created.Status);

            var operationId = created.Data.GetProperty("operationId").GetString();
            var operation = await GetAsync($"otapanel/v1/operations/{operationId}", token);

            Assert.Equal(HttpStatusCode.OK, operation.Status);
            Assert.Equal("Completed", operation.Data.GetProperty("phase").GetString());
        }

        [Fact]
        public async Task Ota_token_sells_for_its_token_customer_and_cannot_read_another_customer_order()
        {
            var backofficeOffer = await PublishAsync(S1Harness.Scope());
            var backofficeToken = _host.BackofficeToken();
            var otherPreparation = await PostAsync("backoffice/v1/order-preparations", PrepareBody(backofficeOffer, S1Harness.CustomerId, S1Harness.AirlineOfficeId), backofficeToken);
            var otherOrder = await PostAsync("backoffice/v1/orders/from-offer", CreateBody(otherPreparation.Data, S1Harness.CustomerId, S1Harness.AirlineOfficeId), backofficeToken);

            var offerId = await PublishAsync(S1Harness.OtaScope());
            var token = _host.OtaToken();

            var preparation = await PostAsync("ota/v1/order-preparations", PrepareBody(offerId, null, null), token);
            Assert.Equal(HttpStatusCode.Created, preparation.Status);

            var created = await PostAsync("ota/v1/orders/from-offer", CreateBody(preparation.Data, null, null), token);
            Assert.Equal(HttpStatusCode.Created, created.Status);

            var own = await GetAsync($"ota/v1/orders/{created.Data.GetProperty("orderId").GetString()}", token);
            var foreign = await GetAsync($"ota/v1/orders/{otherOrder.Data.GetProperty("orderId").GetString()}", token);

            Assert.Equal(HttpStatusCode.OK, own.Status);
            Assert.Equal(HttpStatusCode.NotFound, foreign.Status);
        }

        [Theory]
        [InlineData("backoffice/v1/order-preparations")]
        [InlineData("otapanel/v1/order-preparations")]
        [InlineData("ota/v1/order-preparations")]
        public async Task Authenticated_sale_surfaces_reject_an_anonymous_caller(string route)
        {
            var response = await PostAsync(route, PrepareBody("any-offer", S1Harness.CustomerId, S1Harness.AirlineOfficeId), token: null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.Status);
        }

        [Fact]
        public async Task A_token_of_another_surface_is_rejected_on_every_authenticated_sale_surface()
        {
            var backoffice = _host.BackofficeToken();
            var otaPanel = _host.OtaPanelToken();

            var wrongPanel = await PostAsync("otapanel/v1/order-preparations", PrepareBody("any-offer", null, null), backoffice);
            var wrongOta = await PostAsync("ota/v1/order-preparations", PrepareBody("any-offer", null, null), backoffice);
            var wrongBackoffice = await PostAsync("backoffice/v1/order-preparations", PrepareBody("any-offer", S1Harness.CustomerId, S1Harness.AirlineOfficeId), otaPanel);

            Assert.Equal(HttpStatusCode.Forbidden, wrongPanel.Status);
            Assert.Equal(HttpStatusCode.Forbidden, wrongOta.Status);
            Assert.Equal(HttpStatusCode.Forbidden, wrongBackoffice.Status);
            Assert.Equal(20285, ErrorCode(wrongPanel));
        }

        [Fact]
        public async Task The_route_surface_is_bound_to_the_token_surface_even_when_every_other_claim_is_present()
        {
            var offerId = await PublishAsync(S1Harness.Scope());
            var everyClaim = new Dictionary<string, object>
            {
                ["airline_office_id"] = S1Harness.AirlineOfficeId,
                ["airline_user_id"] = S1Harness.AirlineUserId,
                ["travel_agency_id"] = S1Harness.TravelAgencyId,
                ["travel_agency_office_id"] = S1Harness.AgencyOfficeId,
                ["travel_agency_user_id"] = S1Harness.AgencyUserId,
                ["customer_id"] = S1Harness.OtherCustomerId,
                ["partner_api_access_profile_id"] = S1Harness.PartnerApiAccessProfileId
            };

            var wrongSurface = _host.Token(AuthorizationSurface.Api, BusinessContextType.Airline, PrincipalType.Human, everyClaim);
            var rightSurface = _host.Token(AuthorizationSurface.Backoffice, BusinessContextType.Airline, PrincipalType.Human, everyClaim);

            var rejected = await PostAsync("backoffice/v1/order-preparations", PrepareBody(offerId, S1Harness.CustomerId, S1Harness.AirlineOfficeId), wrongSurface);
            var accepted = await PostAsync("backoffice/v1/order-preparations", PrepareBody(offerId, S1Harness.CustomerId, S1Harness.AirlineOfficeId), rightSurface);

            Assert.Equal(HttpStatusCode.Forbidden, rejected.Status);
            Assert.Equal(20285, ErrorCode(rejected));
            Assert.Equal(HttpStatusCode.Created, accepted.Status);
        }

        [Fact]
        public async Task Backoffice_selling_office_must_match_the_token_office()
        {
            var response = await PostAsync(
                "backoffice/v1/order-preparations",
                PrepareBody("any-offer", S1Harness.CustomerId, S1Harness.AirlineOfficeId + 1),
                _host.BackofficeToken());

            Assert.Equal(HttpStatusCode.Forbidden, response.Status);
            Assert.Equal(20285, ErrorCode(response));
        }

        [Fact]
        public async Task An_unknown_or_inactive_financial_customer_is_rejected()
        {
            var response = await PostAsync(
                "service/v1/order-preparations",
                PrepareBody("any-offer", S1Harness.SuspendedCustomerId, null),
                token: null);

            Assert.Equal(HttpStatusCode.Forbidden, response.Status);
            Assert.Equal(20285, ErrorCode(response));
        }

        [Fact]
        public async Task A_preparation_cannot_be_accepted_under_another_sales_context()
        {
            var offerId = await PublishAsync(S1Harness.ServiceScope());
            var preparation = await PostAsync("service/v1/order-preparations", PrepareBody(offerId, S1Harness.CustomerId, null), token: null);

            var created = await PostAsync(
                "backoffice/v1/orders/from-offer",
                CreateBody(preparation.Data, S1Harness.CustomerId, S1Harness.AirlineOfficeId),
                _host.BackofficeToken());

            Assert.Equal(HttpStatusCode.Conflict, created.Status);
            Assert.Equal(20287, ErrorCode(created));
        }

        [Fact]
        public async Task Mutations_require_the_idempotency_key_header()
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "service/v1/order-preparations")
            {
                Content = JsonContent.Create(PrepareBody("any-offer", S1Harness.CustomerId, null))
            };

            using var response = await _host.Client.SendAsync(request);
            var payload = await ReadAsync(response);

            Assert.Equal(HttpStatusCode.BadRequest, payload.Status);
            Assert.Equal(20264, ErrorCode(payload));
        }

        [Fact]
        public async Task Internal_projection_rebuild_needs_no_token()
        {
            var offerId = await PublishAsync(S1Harness.Scope());
            var token = _host.BackofficeToken();
            var preparation = await PostAsync("backoffice/v1/order-preparations", PrepareBody(offerId, S1Harness.CustomerId, S1Harness.AirlineOfficeId), token);
            var created = await PostAsync("backoffice/v1/orders/from-offer", CreateBody(preparation.Data, S1Harness.CustomerId, S1Harness.AirlineOfficeId), token);
            var orderId = created.Data.GetProperty("orderId").GetString();

            var rebuilt = await PostAsync($"internal/v1/orders/{orderId}/projection-rebuilds", new { }, token: null);

            Assert.Equal(HttpStatusCode.Created, rebuilt.Status);
            Assert.Equal(orderId, rebuilt.Data.GetProperty("orderId").GetString());
            Assert.Equal("1", rebuilt.Data.GetProperty("orderRevision").ToString());
        }

        private async Task<string> PublishAsync(AuthorizedSalesScope scope)
        {
            var offerId = $"API-OFFER-{Guid.NewGuid():N}";

            await _host.Catalog.PublishAsync(CandidateBuilder
                .OneWayFare100Tax20(DateTimeOffset.UtcNow, scope)
                .Offer(offerId)
                .Build());

            return offerId;
        }

        private static object PrepareBody(string offerId, long? financialCustomerId, long? sellingOfficeId)
            => new Dictionary<string, object?>
            {
                ["offerId"] = offerId,
                ["financialCustomerId"] = financialCustomerId?.ToString(),
                ["sellingOfficeId"] = sellingOfficeId?.ToString()
            };

        private static object CreateBody(JsonElement preparation, long? financialCustomerId, long? sellingOfficeId)
            => new Dictionary<string, object?>
            {
                ["preparationId"] = preparation.GetProperty("preparationId").GetString(),
                ["acceptedSnapshotDigest"] = preparation.GetProperty("acceptedSnapshotDigest").GetString(),
                ["acceptedAt"] = DateTimeOffset.UtcNow.ToString("O"),
                ["financialCustomerId"] = financialCustomerId?.ToString(),
                ["sellingOfficeId"] = sellingOfficeId?.ToString(),
                ["travelerBindings"] = preparation.GetProperty("candidate").GetProperty("travelers").EnumerateArray()
                    .Select(traveler => new Dictionary<string, object?>
                    {
                        ["sourceTravellerRef"] = traveler.GetProperty("sourceTravellerRef").GetString(),
                        ["clientTravelerRef"] = $"CLIENT-{traveler.GetProperty("sourceTravellerRef").GetString()}",
                        ["givenName"] = "Sample",
                        ["surname"] = "Traveler",
                        ["passengerTypeCode"] = traveler.GetProperty("passengerTypeCode").GetString(),
                        ["dateOfBirth"] = "1990-01-01",
                        ["guardianClientTravelerRef"] = null
                    })
                    .ToList(),
                ["contacts"] = new[]
                {
                    new Dictionary<string, object?> { ["role"] = "Primary", ["email"] = "traveler@example.invalid", ["phone"] = null }
                }
            };

        private async Task<ApiResponse> PostAsync(string route, object body, string? token)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, route) { Content = JsonContent.Create(body) };
            request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("N"));

            if (token is not null)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _host.Client.SendAsync(request);
            return await ReadAsync(response);
        }

        private async Task<ApiResponse> GetAsync(string route, string? token)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, route);

            if (token is not null)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _host.Client.SendAsync(request);
            return await ReadAsync(response);
        }

        private static async Task<ApiResponse> ReadAsync(HttpResponseMessage response)
        {
            var raw = await response.Content.ReadAsStringAsync();
            var document = raw.Length == 0 ? null : JsonDocument.Parse(raw);

            return new ApiResponse(response.StatusCode, raw, document?.RootElement.Clone() ?? default);
        }

        private static int? ErrorCode(ApiResponse response)
            => response.Envelope.ValueKind == JsonValueKind.Object
               && response.Envelope.TryGetProperty("errors", out var errors)
               && errors.ValueKind == JsonValueKind.Array
                ? errors.EnumerateArray().First().GetProperty("code").GetInt32()
                : null;

        private sealed record ApiResponse(HttpStatusCode Status, string Raw, JsonElement Envelope)
        {
            public JsonElement Data => Envelope.GetProperty("data");
        }
    }
}
