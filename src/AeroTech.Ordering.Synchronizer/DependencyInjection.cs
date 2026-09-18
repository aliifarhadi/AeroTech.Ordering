using AeroTech.Ordering.Domain.OrderAggregate.Contracts;
using AeroTech.Ordering.Synchronizer.OrderAggregate;
using Microsoft.Extensions.DependencyInjection;

namespace AeroTech.Ordering.Synchronizer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSynchronizer(this IServiceCollection services)
        {
            services.AddScoped<IOrderProjectionFaultInjector, NoOrderProjectionFault>();
            services.AddScoped<IOrderQuerySynchronizer, OrderProjector>();

            return services;
        }
    }
}
