using System.Reflection;
using AeroTech.Ordering.ReferenceData;
using AeroTech.Ordering.RestApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.Host
{
    public sealed class AnonymousSurfaceInventoryTests
    {
        [Fact]
        public void Current_controller_surface_is_exactly_the_characterized_inventory()
        {
            var inventory = new[] { typeof(RestApiAssembly).Assembly, typeof(ReferenceDataAssembly).Assembly }
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract)
                .SelectMany(Describe)
                .OrderBy(line => line, StringComparer.Ordinal)
                .ToArray();

            var expected = new[]
            {
                "PingController GET api/v{version:apiVersion}/[controller]/ authorize=False",
                "ReferenceSyncController POST Syncer/v{version:apiVersion}/Airlines authorize=False",
                "ReferenceSyncController POST Syncer/v{version:apiVersion}/Airports authorize=False",
                "ReferenceSyncController POST Syncer/v{version:apiVersion}/Cities authorize=False",
                "ReferenceSyncController POST Syncer/v{version:apiVersion}/Currencies authorize=False",
                "ReferenceSyncController POST Syncer/v{version:apiVersion}/Customers authorize=False",
                "ReferenceSyncController POST Syncer/v{version:apiVersion}/OperatorSettings authorize=False"
            };

            Assert.Equal(expected, inventory);
        }

        private static IEnumerable<string> Describe(Type controller)
        {
            var route = controller.GetCustomAttribute<RouteAttribute>()?.Template ?? string.Empty;
            var controllerAuthorized = controller.GetCustomAttribute<AuthorizeAttribute>() is not null;

            foreach (var action in controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                foreach (var verb in action.GetCustomAttributes<HttpMethodAttribute>())
                {
                    var authorized = controllerAuthorized || action.GetCustomAttribute<AuthorizeAttribute>() is not null;

                    yield return $"{controller.Name} {string.Join(",", verb.HttpMethods)} {route}/{verb.Template} authorize={authorized}";
                }
            }
        }
    }
}
