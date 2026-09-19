using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AeroTech.Messages.Aegis.Enums;
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
        private const string BackofficeOrders = "Backoffice/v1/Orders";
        private const string OtaBookings = "Api/v1/Bookings";
        private const string OtaPanelBookings = "OtaPanel/v1/Bookings";
        private const string ServiceBookings = "Service/v1/Bookings";

        private readonly OrderingDatabaseFixture _fixture;
        private OrderingApiHost _host = null!;

        public OrderingApiSurfaceTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task InitializeAsync() => _host = await OrderingApiHost.StartAsync(_fixture);

        public async Task DisposeAsync() => await _host.DisposeAsync();

        [Fact]
        public async Task Backoffice_sells_a_flight_offer_and_reads_the_order_with_traveller_names()
        {
            var offerId = await PublishAsync(S1Harness.Scope());
            var token = _host.BackofficeToken();

            var created = await PostAsync($"{BackofficeOrders}/FlightOffers", BackofficeBody(offerId), token);

            Assert.Equal(HttpStatusCode.Created, created.Status);
            Assert.Matches("^[23456789ABCDEFGHJKLMNPQRSTUVWXYZ]{8}$", created.Data.GetProperty("orderReference").GetString());
            Assert.Equal("120", created.Data.GetProperty("grandTotal").GetString());
            Assert.Equal("Active", created.Data.GetProperty("status").GetString());

            var orderId = created.Data.GetProperty("orderId").GetString();
            var order = await GetAsync($"{BackofficeOrders}/{orderId}", token);

            Assert.Equal(HttpStatusCode.OK, order.Status);
            Assert.Equal(offerId, order.Data.GetProperty("offerId").GetString());
            Assert.Equal("BackOffice", order.Data.GetProperty("channel").GetString());
            Assert.Equal("120", order.Data.GetProperty("grandTotal").GetProperty("amount").GetString());

            var traveller = Assert.Single(order.Data.GetProperty("travellers").EnumerateArray());
            Assert.Equal("Sample", traveller.GetProperty("firstName").GetString());
            Assert.Equal("ADT", traveller.GetProperty("passengerType").GetString());
            Assert.Single(order.Data.GetProperty("contacts").EnumerateArray());
            Assert.Single(order.Data.GetProperty("itinerary").EnumerateArray());
        }

        [Fact]
        public async Task Service_surface_needs_no_token_and_never_returns_traveller_names()
        {
            var offerId = await PublishAsync(S1Harness.ServiceScope());

            var created = await PostAsync($"{ServiceBookings}/FlightOffers", ServiceBody(offerId), token: null);
            Assert.Equal(HttpStatusCode.Created, created.Status);

            var orderId = created.Data.GetProperty("orderId").GetString();
            var order = await GetAsync($"{ServiceBookings}/{orderId}", token: null);

            Assert.Equal(HttpStatusCode.OK, order.Status);
            Assert.Empty(order.Data.GetProperty("contacts").EnumerateArray());
            Assert.Null(Assert.Single(order.Data.GetProperty("travellers").EnumerateArray()).GetProperty("firstName").GetString());
            Assert.DoesNotContain("Sample", order.Raw, StringComparison.Ordinal);
            Assert.DoesNotContain("traveler@example.invalid", order.Raw, StringComparison.Ordinal);
        }

        [Fact]
        public async Task OtaPanel_sells_for_the_customer_of_its_travel_agency()
        {
            var offerId = await PublishAsync(S1Harness.OtaPanelScope());
            var token = _host.OtaPanelToken();

            var created = await PostAsync($"{OtaPanelBookings}/FlightOffers", OtaBody(offerId), token);
            Assert.Equal(HttpStatusCode.Created, created.Status);

            var order = await GetAsync($"{OtaPanelBookings}/{created.Data.GetProperty("orderId").GetString()}", token);

            Assert.Equal(HttpStatusCode.OK, order.Status);
            Assert.Equal("AgencyPanel", order.Data.GetProperty("channel").GetString());
        }

        [Fact]
        public async Task Ota_sells_for_its_token_customer_and_cannot_read_another_customer_order()
        {
            var backofficeOffer = await PublishAsync(S1Harness.Scope());
            var backofficeToken = _host.BackofficeToken();
            var otherOrder = await PostAsync($"{BackofficeOrders}/FlightOffers", BackofficeBody(backofficeOffer), backofficeToken);

            var offerId = await PublishAsync(S1Harness.OtaScope());
            var token = _host.OtaToken();

            var created = await PostAsync($"{OtaBookings}/FlightOffers", OtaBody(offerId), token);
            Assert.Equal(HttpStatusCode.Created, created.Status);

            var own = await GetAsync($"{OtaBookings}/{created.Data.GetProperty("orderId").GetString()}", token);
            var foreign = await GetAsync($"{OtaBookings}/{otherOrder.Data.GetProperty("orderId").GetString()}", token);

            Assert.Equal(HttpStatusCode.OK, own.Status);
            Assert.Equal("PartnerAPI", own.Data.GetProperty("channel").GetString());
            Assert.Equal(HttpStatusCode.NotFound, foreign.Status);
        }

        [Theory]
        [InlineData(BackofficeOrders)]
        [InlineData(OtaBookings)]
        [InlineData(OtaPanelBookings)]
        public async Task Authenticated_sale_surfaces_reject_an_anonymous_caller(string root)
        {
            var response = await PostAsync($"{root}/FlightOffers", BackofficeBody("any-offer"), token: null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.Status);
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

            var rejected = await PostAsync($"{BackofficeOrders}/FlightOffers", BackofficeBody(offerId), wrongSurface);
            var accepted = await PostAsync($"{BackofficeOrders}/FlightOffers", BackofficeBody(offerId), rightSurface);

            Assert.Equal(HttpStatusCode.Forbidden, rejected.Status);
            Assert.Equal(20285, ErrorCode(rejected));
            Assert.Equal(HttpStatusCode.Created, accepted.Status);
        }

        [Fact]
        public async Task Backoffice_selling_office_must_match_the_token_office()
        {
            var response = await PostAsync(
                $"{BackofficeOrders}/FlightOffers",
                BackofficeBody("any-offer", airlineOfficeId: S1Harness.AirlineOfficeId + 1),
                _host.BackofficeToken());

            Assert.Equal(HttpStatusCode.Forbidden, response.Status);
            Assert.Equal(20285, ErrorCode(response));
        }

        [Fact]
        public async Task An_unknown_or_inactive_customer_is_rejected()
        {
            var response = await PostAsync($"{ServiceBookings}/FlightOffers", ServiceBody("any-offer", S1Harness.SuspendedCustomerId), token: null);

            Assert.Equal(HttpStatusCode.Forbidden, response.Status);
            Assert.Equal(20285, ErrorCode(response));
        }

        [Fact]
        public async Task Mutations_require_the_idempotency_key_header()
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{ServiceBookings}/FlightOffers")
            {
                Content = JsonContent.Create(ServiceBody("any-offer"))
            };

            using var response = await _host.Client.SendAsync(request);
            var payload = await ReadAsync(response);

            Assert.Equal(HttpStatusCode.BadRequest, payload.Status);
            Assert.Equal(20264, ErrorCode(payload));
        }

        [Fact]
        public async Task The_same_idempotency_key_returns_the_same_order()
        {
            var offerId = await PublishAsync(S1Harness.Scope());
            var token = _host.BackofficeToken();
            var key = Guid.NewGuid().ToString("N");

            var first = await PostAsync($"{BackofficeOrders}/FlightOffers", BackofficeBody(offerId), token, key);
            var replay = await PostAsync($"{BackofficeOrders}/FlightOffers", BackofficeBody(offerId), token, key);

            Assert.Equal(HttpStatusCode.Created, first.Status);
            Assert.Equal(first.Data.GetProperty("orderId").GetString(), replay.Data.GetProperty("orderId").GetString());
            Assert.Equal(first.Data.GetProperty("orderReference").GetString(), replay.Data.GetProperty("orderReference").GetString());
        }

        [Fact]
        public async Task Internal_projection_rebuild_needs_no_token()
        {
            var offerId = await PublishAsync(S1Harness.Scope());
            var token = _host.BackofficeToken();
            var created = await PostAsync($"{BackofficeOrders}/FlightOffers", BackofficeBody(offerId), token);
            var orderId = created.Data.GetProperty("orderId").GetString();

            var rebuilt = await PostAsync($"Internal/v1/Orders/{orderId}/ProjectionRebuilds", new { }, token: null);

            Assert.Equal(HttpStatusCode.OK, rebuilt.Status);
            Assert.Equal(orderId, rebuilt.Data.GetProperty("orderId").GetString());
        }

        [Fact]
        public async Task The_public_payloads_carry_no_internal_mechanics()
        {
            var offerId = await PublishAsync(S1Harness.Scope());
            var token = _host.BackofficeToken();

            var created = await PostAsync($"{BackofficeOrders}/FlightOffers", BackofficeBody(offerId), token);
            var order = await GetAsync($"{BackofficeOrders}/{created.Data.GetProperty("orderId").GetString()}", token);

            foreach (var forbidden in new[] { "preparationId", "acceptedSnapshotDigest", "acceptedSourceDigest", "operationId", "projectionSchemaVersion", "sourcePayloadHash", "canonicalizationVersion", "callerScope" })
            {
                Assert.DoesNotContain(forbidden, created.Raw, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(forbidden, order.Raw, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Theory]
        [InlineData("backoffice/v1/order-preparations")]
        [InlineData("backoffice/v1/orders/from-offer")]
        [InlineData("service/v1/order-preparations")]
        [InlineData("ota/v1/orders/from-offer")]
        public async Task The_retired_technical_routes_are_gone(string route)
        {
            var response = await PostAsync(route, BackofficeBody("any-offer"), _host.BackofficeToken());

            Assert.True(response.Status is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed, $"{route} answered {response.Status}");
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

        private static object BackofficeBody(string offerId, long customerId = S1Harness.CustomerId, long airlineOfficeId = S1Harness.AirlineOfficeId)
            => new Dictionary<string, object?>
            {
                ["customerId"] = customerId.ToString(),
                ["airlineOfficeId"] = airlineOfficeId.ToString(),
                ["offerId"] = offerId,
                ["travellers"] = Travellers(),
                ["contacts"] = Contacts(),
                ["clientReference"] = null
            };

        private static object ServiceBody(string offerId, long customerId = S1Harness.CustomerId)
            => new Dictionary<string, object?>
            {
                ["customerId"] = customerId.ToString(),
                ["sellingOfficeId"] = null,
                ["sellingOfficeKind"] = null,
                ["offerId"] = offerId,
                ["travellers"] = Travellers(),
                ["contacts"] = Contacts(),
                ["clientReference"] = null
            };

        private static object OtaBody(string offerId)
            => new Dictionary<string, object?>
            {
                ["offerId"] = offerId,
                ["travellers"] = Travellers(),
                ["contacts"] = Contacts(),
                ["clientReference"] = null
            };

        private static object Travellers()
            => new[]
            {
                new Dictionary<string, object?>
                {
                    ["offerTravellerRef"] = "PAX-A",
                    ["travellerRef"] = "CLIENT-PAX-A",
                    ["firstName"] = "Sample",
                    ["surName"] = "Traveler",
                    ["passengerType"] = "ADT",
                    ["dateOfBirth"] = "1990-01-01",
                    ["guardianTravellerRef"] = null
                }
            };

        private static object Contacts()
            => new[]
            {
                new Dictionary<string, object?> { ["role"] = "Primary", ["email"] = "traveler@example.invalid", ["phone"] = null }
            };

        private async Task<ApiResponse> PostAsync(string route, object body, string? token, string? idempotencyKey = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, route) { Content = JsonContent.Create(body) };
            request.Headers.Add("Idempotency-Key", idempotencyKey ?? Guid.NewGuid().ToString("N"));

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
            var document = raw.Length == 0 || !raw.StartsWith('{') ? null : JsonDocument.Parse(raw);

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
