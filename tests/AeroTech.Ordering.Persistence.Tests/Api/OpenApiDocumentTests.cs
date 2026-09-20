using System.Text.Json;
using AeroTech.Ordering.Persistence.Tests._Shared;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.Api
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class OpenApiDocumentTests
    {
        public const string DocumentOutputVariable = "ORDERING_OPENAPI_OUT";

        private readonly OrderingDatabaseFixture _fixture;

        public OpenApiDocumentTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Published_document_exposes_exactly_the_s1_operations()
        {
            await using var host = await OrderingApiHost.StartAsync(_fixture, Environments.Development);

            var document = await host.Client.GetStringAsync("swagger/v1/swagger.json");

            if (Environment.GetEnvironmentVariable(DocumentOutputVariable) is { Length: > 0 } path)
                await File.WriteAllTextAsync(path, document);

            using var parsed = JsonDocument.Parse(document);

            var operations = parsed.RootElement.GetProperty("paths").EnumerateObject()
                .SelectMany(path => path.Value.EnumerateObject().Select(method => $"{method.Name.ToUpperInvariant()} {path.Name}"))
                .OrderBy(operation => operation, StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(
                [
                    "GET /Api/v1/Bookings/{id}",
                    "GET /Backoffice/v1/Orders/{id}",
                    "GET /OtaPanel/v1/Bookings/{id}",
                    "GET /Service/v1/Bookings/{id}",
                    "GET /api/v1/Ping",
                    "POST /Api/v1/Bookings/FlightOffers",
                    "POST /Backoffice/v1/Orders/FlightOffers",
                    "POST /Internal/v1/Orders/{id}/ProjectionRebuilds",
                    "POST /OtaPanel/v1/Bookings/FlightOffers",
                    "POST /Service/v1/Bookings/FlightOffers",
                    "POST /Syncer/v1/Airlines",
                    "POST /Syncer/v1/Airports",
                    "POST /Syncer/v1/Cities",
                    "POST /Syncer/v1/Currencies",
                    "POST /Syncer/v1/Customers",
                    "POST /Syncer/v1/OperatorSettings"
                ],
                operations);
        }

        [Fact]
        public async Task The_create_surfaces_name_the_financial_customer_by_its_role()
        {
            await using var host = await OrderingApiHost.StartAsync(_fixture, Environments.Development);

            using var parsed = JsonDocument.Parse(await host.Client.GetStringAsync("swagger/v1/swagger.json"));
            var schemas = parsed.RootElement.GetProperty("components").GetProperty("schemas");

            foreach (var schema in new[] { "BackofficeCreateOrderFromOfferRequest", "ServiceCreateOrderFromOfferRequest" })
            {
                var properties = schemas.GetProperty(schema).GetProperty("properties").EnumerateObject()
                    .Select(property => property.Name)
                    .ToList();

                Assert.Contains("financialCustomerId", properties);
                Assert.DoesNotContain("customerId", properties);
            }

            var officeKind = schemas.GetProperty("SellingOfficeKind").GetProperty("enum").EnumerateArray()
                .Select(value => value.ToString())
                .ToList();

            Assert.DoesNotContain("NotRecorded", officeKind);
        }
    }
}
