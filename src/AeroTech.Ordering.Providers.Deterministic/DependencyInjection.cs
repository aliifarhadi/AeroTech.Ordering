using AeroTech.Ordering.Providers.Deterministic._Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            return services;
        }
    }
}
