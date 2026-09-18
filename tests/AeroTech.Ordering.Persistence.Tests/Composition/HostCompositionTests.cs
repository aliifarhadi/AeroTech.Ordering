using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Ordering.Persistence.Tests._Shared;
using AeroTech.Ordering.Providers.Deterministic;
using AeroTech.Ordering.Providers.Deterministic._Shared.Persistence;
using AeroTech.Ordering.ServiceHost.Composition;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.Composition
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class HostCompositionTests
    {
        private readonly OrderingDatabaseFixture _fixture;

        public HostCompositionTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Host_composes_with_real_bindings_and_one_unit_of_work()
        {
            await using var app = Build(Settings(deterministic: false));
            await using var scope = app.Services.CreateAsyncScope();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();

            Assert.Same(dbContext, unitOfWork);
            Assert.Single(scope.ServiceProvider.GetServices<IUnitOfWork>());
            Assert.Null(app.Services.GetService<DeterministicEffectStore>());
        }

        [Fact]
        public async Task Host_composes_with_deterministic_adapters_only_when_the_existing_flag_is_true()
        {
            var settings = Settings(deterministic: true);
            settings[$"ConnectionStrings:{DeterministicOwnerDbContext.ConnectionStringName}"] = _fixture.OwnerConnectionString;

            await using var app = Build(settings);

            Assert.NotNull(app.Services.GetService<DeterministicEffectStore>());
        }

        [Fact]
        public void Deterministic_flag_without_its_own_database_fails_closed()
        {
            var exception = Assert.ThrowsAny<ArgumentException>(() => Build(Settings(deterministic: true)));

            Assert.Contains(DeterministicOwnerDbContext.ConnectionStringName, exception.Message);
        }

        [Fact]
        public void Missing_broker_configuration_fails_startup()
        {
            var settings = Settings(deterministic: false);
            settings.Remove("RabbitMq:Server");

            var exception = Assert.ThrowsAny<ArgumentException>(() => Build(settings));

            Assert.Contains("RabbitMq:Server", exception.Message);
        }

        [Fact]
        public void Missing_identity_authority_fails_startup()
        {
            var settings = Settings(deterministic: false);
            settings.Remove("Jwt:Authority");

            var exception = Assert.ThrowsAny<ArgumentException>(() => Build(settings));

            Assert.Contains("Jwt:Authority", exception.Message);
        }

        [Fact]
        public async Task Bearer_validation_follows_the_identity_authority_and_audience()
        {
            await using var app = Build(Settings(deterministic: false));

            var options = app.Services
                .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
                .Get(JwtBearerDefaults.AuthenticationScheme);

            Assert.Equal("http://localhost:5050/", options.Authority);
            Assert.Equal("pss-api", options.Audience);
            Assert.False(options.RequireHttpsMetadata);
            Assert.True(options.TokenValidationParameters.ValidateIssuer);
            Assert.True(options.TokenValidationParameters.ValidateAudience);
            Assert.True(options.TokenValidationParameters.ValidateLifetime);
            Assert.True(options.TokenValidationParameters.ValidateIssuerSigningKey);
            Assert.Null(options.TokenValidationParameters.IssuerSigningKey);
            Assert.Equal("http://localhost:5050/", options.TokenValidationParameters.ValidIssuer);
        }

        private Dictionary<string, string?> Settings(bool deterministic)
            => new()
            {
                ["ConnectionStrings:CommandDbContext"] = _fixture.ConnectionString,
                ["IdGenerator:GeneratorId"] = "0",
                ["Redis:Host"] = "localhost",
                ["RabbitMq:Server"] = "localhost",
                ["RabbitMq:UserName"] = "guest",
                ["RabbitMq:Password"] = "guest",
                ["Jwt:Authority"] = "http://localhost:5050/",
                ["Jwt:Audience"] = "pss-api",
                ["Jwt:RequireHttpsMetadata"] = "false",
                [DeterministicAdapterOptions.EnabledKey] = deterministic ? "true" : "false"
            };

        private static WebApplication Build(Dictionary<string, string?> settings)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "B0Composition" });

            builder.Configuration.Sources.Clear();
            builder.Configuration.AddInMemoryCollection(settings);
            builder.Host.UseDefaultServiceProvider(options =>
            {
                options.ValidateOnBuild = true;
                options.ValidateScopes = true;
            });

            builder.Services.AddOrderingHost(builder.Configuration, builder.Environment);

            return builder.Build();
        }
    }
}
