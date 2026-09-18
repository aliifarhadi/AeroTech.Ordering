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
                "ReferenceData.Api.ReferenceSyncController POST Syncer/v{version:apiVersion}/Airlines authorize=False",
                "ReferenceData.Api.ReferenceSyncController POST Syncer/v{version:apiVersion}/Airports authorize=False",
                "ReferenceData.Api.ReferenceSyncController POST Syncer/v{version:apiVersion}/Cities authorize=False",
                "ReferenceData.Api.ReferenceSyncController POST Syncer/v{version:apiVersion}/Currencies authorize=False",
                "ReferenceData.Api.ReferenceSyncController POST Syncer/v{version:apiVersion}/Customers authorize=False",
                "ReferenceData.Api.ReferenceSyncController POST Syncer/v{version:apiVersion}/OperatorSettings authorize=False",
                "RestApi.V1.OperationAggregate.Controllers.BackofficeController GET backoffice/v{version:apiVersion}/operations/{operationId:long} authorize=True",
                "RestApi.V1.OperationAggregate.Controllers.OtaController GET ota/v{version:apiVersion}/operations/{operationId:long} authorize=True",
                "RestApi.V1.OperationAggregate.Controllers.OtaPanelController GET otapanel/v{version:apiVersion}/operations/{operationId:long} authorize=True",
                "RestApi.V1.OperationAggregate.Controllers.ServiceController GET service/v{version:apiVersion}/operations/{operationId:long} authorize=False",
                "RestApi.V1.OrderAggregate.Controllers.BackofficeController GET backoffice/v{version:apiVersion}/orders/{orderId:long} authorize=True",
                "RestApi.V1.OrderAggregate.Controllers.BackofficeController POST backoffice/v{version:apiVersion}/orders/from-offer authorize=True",
                "RestApi.V1.OrderAggregate.Controllers.InternalController POST internal/v{version:apiVersion}/orders/{orderId}/projection-rebuilds authorize=False",
                "RestApi.V1.OrderAggregate.Controllers.OtaController GET ota/v{version:apiVersion}/orders/{orderId:long} authorize=True",
                "RestApi.V1.OrderAggregate.Controllers.OtaController POST ota/v{version:apiVersion}/orders/from-offer authorize=True",
                "RestApi.V1.OrderAggregate.Controllers.OtaPanelController GET otapanel/v{version:apiVersion}/orders/{orderId:long} authorize=True",
                "RestApi.V1.OrderAggregate.Controllers.OtaPanelController POST otapanel/v{version:apiVersion}/orders/from-offer authorize=True",
                "RestApi.V1.OrderAggregate.Controllers.ServiceController GET service/v{version:apiVersion}/orders/{orderId:long} authorize=False",
                "RestApi.V1.OrderAggregate.Controllers.ServiceController POST service/v{version:apiVersion}/orders/from-offer authorize=False",
                "RestApi.V1.OrderPreparationAggregate.Controllers.BackofficeController POST backoffice/v{version:apiVersion}/order-preparations authorize=True",
                "RestApi.V1.OrderPreparationAggregate.Controllers.OtaController POST ota/v{version:apiVersion}/order-preparations authorize=True",
                "RestApi.V1.OrderPreparationAggregate.Controllers.OtaPanelController POST otapanel/v{version:apiVersion}/order-preparations authorize=True",
                "RestApi.V1.OrderPreparationAggregate.Controllers.ServiceController POST service/v{version:apiVersion}/order-preparations authorize=False",
                "RestApi.V1._Shared.PingController GET api/v{version:apiVersion}/[controller]/ authorize=False"
            };

            Assert.Equal(expected, inventory);
        }

        private static IEnumerable<string> Describe(Type controller)
        {
            var name = controller.FullName!.Replace("AeroTech.Ordering.", string.Empty, StringComparison.Ordinal);
            var route = controller.GetCustomAttribute<RouteAttribute>()?.Template ?? string.Empty;
            var controllerAuthorized = controller.GetCustomAttribute<AuthorizeAttribute>() is not null;

            foreach (var action in controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                foreach (var verb in action.GetCustomAttributes<HttpMethodAttribute>())
                {
                    var authorized = controllerAuthorized || action.GetCustomAttribute<AuthorizeAttribute>() is not null;

                    yield return $"{name} {string.Join(",", verb.HttpMethods)} {route}/{verb.Template} authorize={authorized}";
                }
            }
        }
    }
}
