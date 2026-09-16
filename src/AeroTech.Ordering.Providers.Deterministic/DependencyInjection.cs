using Microsoft.Extensions.DependencyInjection;

namespace AeroTech.Ordering.Providers.Deterministic
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDeterministicProviders(this IServiceCollection services)
        {
            return services;
        }
    }
}
