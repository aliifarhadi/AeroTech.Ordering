using AeroTech.Framework.Infrastructure;
using AeroTech.Framework.Presentation.Extensions;
using AeroTech.Ordering.Application;
using AeroTech.Ordering.Consumers;
using AeroTech.Ordering.Domain._Shared.Contracts;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Contracts;
using AeroTech.Ordering.Persistence;
using AeroTech.Ordering.Providers;
using AeroTech.Ordering.Providers.Deterministic;
using AeroTech.Ordering.Query;
using AeroTech.Ordering.ReferenceData;
using AeroTech.Ordering.RestApi;
using AeroTech.Ordering.ServiceHost.CallerContext;
using AeroTech.Ordering.Synchronizer;

namespace AeroTech.Ordering.ServiceHost.Composition
{
    public static class OrderingHostComposition
    {
        public static IServiceCollection AddOrderingHost(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            if (environment.IsProduction() && configuration.GetValue<bool>(DeterministicAdapterOptions.EnabledKey))
                throw new InvalidOperationException($"'{DeterministicAdapterOptions.EnabledKey}' cannot be enabled in the Production environment.");

            services.AddScoped<ICallerContext, ClaimsCallerContext>();
            services.AddSingleton<IAcceptanceProfilePolicy, AcceptanceProfilePolicy>();

            services
                .AddFrameworkInfrastructure(configuration)
                .AddPersistence(configuration)
                .AddProviders(configuration)
                .AddDeterministicProvidersWhenEnabled(configuration)
                .AddQuery(configuration)
                .AddSynchronizer()
                .AddConsumers(configuration)
                .AddApplication(configuration)
                .AddReferenceData(configuration)
                .AddPresentation(configuration, typeof(RestApiAssembly).Assembly, typeof(ReferenceDataAssembly).Assembly);

            return services.EnsureSingleRegistrations();
        }
    }
}
