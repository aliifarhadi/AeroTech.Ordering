using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Ordering.Domain._Shared.Contracts;

namespace AeroTech.Ordering.ServiceHost.Composition
{
    public static class SingleRegistrationGuard
    {
        private const string PortsNamespace = "AeroTech.Ordering.Domain.Ports";

        private static readonly Type[] SingleBindingServices =
        {
            typeof(IUnitOfWork),
            typeof(ICallerContext),
            typeof(IHomeOperatorProvider)
        };

        public static IServiceCollection EnsureSingleRegistrations(this IServiceCollection services)
        {
            var duplicates = services
                .Where(descriptor => RequiresSingleBinding(descriptor.ServiceType))
                .GroupBy(descriptor => descriptor.ServiceType)
                .Where(group => group.Count() > 1)
                .Select(group => $"{group.Key.FullName} -> [{string.Join(", ", group.Select(Describe))}]")
                .ToList();

            if (duplicates.Count > 0)
                throw new InvalidOperationException(
                    $"Duplicate service registrations: {string.Join("; ", duplicates)}");

            return services;
        }

        private static bool RequiresSingleBinding(Type serviceType)
            => SingleBindingServices.Contains(serviceType)
               || (serviceType.Namespace?.StartsWith(PortsNamespace, StringComparison.Ordinal) ?? false);

        private static string Describe(ServiceDescriptor descriptor)
            => descriptor.ImplementationType?.FullName
               ?? descriptor.ImplementationInstance?.GetType().FullName
               ?? "factory";
    }
}
