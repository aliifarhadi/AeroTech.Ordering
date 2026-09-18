using System.Security.Cryptography;
using System.Security.Claims;
using AeroTech.Messages.Aegis.Enums;
using AeroTech.Messages.Shared.Enums;
using AeroTech.Ordering.Providers.Deterministic;
using AeroTech.Ordering.Providers.Deterministic._Shared.Persistence;
using AeroTech.Ordering.Providers.Deterministic.Offers;
using AeroTech.Ordering.ReferenceData.Persistence;
using AeroTech.Ordering.ReferenceData.ReadModels;
using AeroTech.Ordering.ServiceHost.CallerContext;
using AeroTech.Ordering.ServiceHost.Composition;
using AeroTech.Framework.Presentation.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace AeroTech.Ordering.Persistence.Tests._Shared
{
    public sealed class OrderingApiHost : IAsyncDisposable
    {
        public const string Issuer = "https://ordering-tests.invalid/";
        public const string Audience = "pss-api";
        public const string EnvironmentName = "S1ApiTest";

        private readonly WebApplication _app;
        private readonly RsaSecurityKey _signingKey;

        private OrderingApiHost(WebApplication app, RsaSecurityKey signingKey, HttpClient client)
        {
            _app = app;
            _signingKey = signingKey;
            Client = client;
        }

        public HttpClient Client { get; }

        public IServiceProvider Services => _app.Services;

        public ReferenceOfferCatalog Catalog => _app.Services.GetRequiredService<ReferenceOfferCatalog>();

        public static async Task<OrderingApiHost> StartAsync(OrderingDatabaseFixture fixture, string? environmentName = null)
        {
            using var rsa = RSA.Create(2048);
            var signingKey = new RsaSecurityKey(rsa.ExportParameters(true)) { KeyId = "ordering-tests" };

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = environmentName ?? EnvironmentName });

            builder.Configuration.Sources.Clear();
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:CommandDbContext"] = fixture.ConnectionString,
                [$"ConnectionStrings:{DeterministicOwnerDbContext.ConnectionStringName}"] = fixture.OwnerConnectionString,
                ["Idempotency:DigestKey"] = S1Harness.DigestKey,
                ["IdGenerator:GeneratorId"] = "0",
                ["Redis:Host"] = "localhost",
                ["RabbitMq:Server"] = "localhost",
                ["RabbitMq:UserName"] = "guest",
                ["RabbitMq:Password"] = "guest",
                ["Jwt:Authority"] = Issuer,
                ["Jwt:Audience"] = Audience,
                ["Jwt:RequireHttpsMetadata"] = "false",
                ["Offer:BaseUrl"] = "http://airoffer.invalid/service/",
                ["Offer:RequestTimeout"] = "2",
                ["Offer:RetryCount"] = "0",
                ["Offer:RetryInterval"] = "0",
                [DeterministicAdapterOptions.EnabledKey] = "true"
            });

            builder.WebHost.UseSetting(WebHostDefaults.ServerUrlsKey, "http://127.0.0.1:0");
            builder.Services.AddOrderingHost(builder.Configuration, builder.Environment);

            builder.Services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Configuration = new OpenIdConnectConfiguration();
                options.TokenValidationParameters.ValidIssuer = Issuer;
                options.TokenValidationParameters.ValidAudience = Audience;
                options.TokenValidationParameters.IssuerSigningKey = signingKey;
            });

            RemoveBackgroundWorkers(builder.Services);

            var app = builder.Build();
            app.UsePresentation();
            await app.StartAsync();

            var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
            var client = new HttpClient { BaseAddress = new Uri(address) };
            var host = new OrderingApiHost(app, signingKey, client);

            await app.Services.GetRequiredService<DeterministicEffectStore>().EnsureSchemaAsync();
            await host.SeedReferenceDataAsync();

            return host;
        }

        public string BackofficeToken(long airlineOfficeId = S1Harness.AirlineOfficeId, long airlineUserId = S1Harness.AirlineUserId)
            => Token(AuthorizationSurface.Backoffice, BusinessContextType.Airline, PrincipalType.Human, new Dictionary<string, object>
            {
                [ClaimsCallerContext.AirlineOfficeIdClaim] = airlineOfficeId,
                [ClaimsCallerContext.AirlineUserIdClaim] = airlineUserId
            });

        public string OtaPanelToken(
            long travelAgencyId = S1Harness.TravelAgencyId,
            long travelAgencyOfficeId = S1Harness.AgencyOfficeId,
            long travelAgencyUserId = S1Harness.AgencyUserId)
            => Token(AuthorizationSurface.OtaPanel, BusinessContextType.TravelAgency, PrincipalType.Human, new Dictionary<string, object>
            {
                [ClaimsCallerContext.TravelAgencyIdClaim] = travelAgencyId,
                [ClaimsCallerContext.TravelAgencyOfficeIdClaim] = travelAgencyOfficeId,
                [ClaimsCallerContext.TravelAgencyUserIdClaim] = travelAgencyUserId
            });

        public string OtaToken(
            long customerId = S1Harness.OtherCustomerId,
            long partnerApiAccessProfileId = S1Harness.PartnerApiAccessProfileId)
            => Token(AuthorizationSurface.Api, BusinessContextType.PartnerApi, PrincipalType.TravelAgencyApi, new Dictionary<string, object>
            {
                [ClaimsCallerContext.CustomerIdClaim] = customerId,
                [ClaimsCallerContext.PartnerApiAccessProfileIdClaim] = partnerApiAccessProfileId
            });

        public string Token(
            AuthorizationSurface surface,
            BusinessContextType contextType,
            PrincipalType principalType,
            IDictionary<string, object> claims)
        {
            var payload = new Dictionary<string, object>(claims)
            {
                [ClaimsCallerContext.AuthorizationSurfaceClaim] = surface.ToString(),
                [ClaimsCallerContext.ContextTypeClaim] = contextType.ToString(),
                [ClaimsCallerContext.PrincipalTypeClaim] = principalType.ToString()
            };

            return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
            {
                Issuer = Issuer,
                Audience = Audience,
                Subject = new ClaimsIdentity([new Claim(ClaimsCallerContext.SubjectClaim, $"test-{surface}")]),
                Claims = payload,
                NotBefore = DateTime.UtcNow.AddMinutes(-1),
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.RsaSha256)
            });
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await _app.StopAsync();
            await _app.DisposeAsync();
        }

        private static void RemoveBackgroundWorkers(IServiceCollection services)
        {
            var background = services
                .Where(descriptor => descriptor.ServiceType == typeof(IHostedService))
                .Where(descriptor => (descriptor.ImplementationType?.FullName ?? descriptor.ImplementationFactory?.Method.DeclaringType?.FullName ?? string.Empty)
                    is var name && (name.Contains("MassTransit", StringComparison.Ordinal) || name.Contains("AeroTech.Ordering.Consumers", StringComparison.Ordinal)))
                .ToList();

            background.ForEach(descriptor => services.Remove(descriptor));
        }

        private async Task SeedReferenceDataAsync()
        {
            await using var scope = _app.Services.CreateAsyncScope();
            var reference = scope.ServiceProvider.GetRequiredService<ReferenceDbContext>();

            if (!await reference.OperatorSettings.AnyAsync(settings => settings.ScopeKey == OperatorScopeKey.HomeOperator))
            {
                reference.OperatorSettings.Add(new OperatorSettingsReadModel
                {
                    Id = 1,
                    ScopeKey = OperatorScopeKey.HomeOperator,
                    HomeAirlineId = S1Harness.OwnerAirlineId,
                    LastUpdateTime = DateTimeOffset.UtcNow
                });
            }

            if (!await reference.Customers.AnyAsync())
                reference.Customers.AddRange(
                    Customer(S1Harness.CustomerId, CustomerType.Individual, null),
                    Customer(S1Harness.OtherCustomerId, CustomerType.Individual, null),
                    Customer(S1Harness.AgencyCustomerId, CustomerType.TravelAgency, S1Harness.TravelAgencyId),
                    Customer(S1Harness.SuspendedCustomerId, CustomerType.Individual, null, CustomerStatus.Suspended));

            await reference.SaveChangesAsync();
        }

        private static CustomerReadModel Customer(long id, CustomerType type, long? travelAgencyId, CustomerStatus status = CustomerStatus.Active) => new()
        {
            Id = id,
            CustomerNumber = $"C{id}",
            Type = type,
            TravelAgencyId = travelAgencyId,
            SubjectId = id,
            SubjectName = $"Customer {id}",
            Status = status,
            LastUpdateTime = DateTimeOffset.UtcNow
        };
    }
}
