using Microsoft.Extensions.DependencyInjection;

namespace AeroTech.Ordering.Synchronizer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSynchronizer(this IServiceCollection services)
        {
            return services;
        }
    }
}
