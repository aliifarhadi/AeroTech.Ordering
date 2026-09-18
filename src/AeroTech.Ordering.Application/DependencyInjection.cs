using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Ordering.Application._Shared.Authorization;
using AeroTech.Ordering.Application._Shared.Behaviors;
using AeroTech.Ordering.Application._Shared.Events;
using AeroTech.Ordering.Application._Shared.Idempotency;
using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AeroTech.Ordering.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            var assembly = typeof(DependencyInjection).Assembly;

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
            services.AddValidatorsFromAssembly(assembly);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

            services.Configure<IdempotencyOptions>(configuration.GetSection(IdempotencyOptions.SectionName));
            services.Configure<OrderCreationOptions>(configuration.GetSection(OrderCreationOptions.SectionName));
            services.AddSingleton<RequestDigester>();
            services.AddScoped<AuthorizedScopeResolver>();

            return services;
        }
    }
}
