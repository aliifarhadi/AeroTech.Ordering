using AeroTech.Ordering.Domain.Ports.Offers;
using AeroTech.Ordering.Providers.Deterministic._Shared.Persistence;
using AeroTech.Ordering.Providers.Deterministic.Offers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AeroTech.Ordering.Providers.Deterministic
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDeterministicProviders(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(DeterministicOwnerDbContext.ConnectionStringName);

            ArgumentException.ThrowIfNullOrWhiteSpace(
                connectionString,
                $"ConnectionStrings:{DeterministicOwnerDbContext.ConnectionStringName}");

            services.AddDbContextFactory<DeterministicOwnerDbContext>(options => options.UseSqlServer(connectionString));
            services.AddSingleton<DeterministicEffectStore>();
            services.AddSingleton<ReferenceOfferCatalog>();

            services.RemoveAll<IOfferSourcePort>();
            services.AddScoped<IOfferSourcePort, ReferenceOfferSourceAdapter>();

            return services;
        }
    }
}
