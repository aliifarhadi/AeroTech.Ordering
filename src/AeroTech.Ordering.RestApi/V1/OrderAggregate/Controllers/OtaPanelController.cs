using AeroTech.Framework.Presentation.Responses;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.RestApi._Shared;
using AeroTech.Ordering.RestApi.V1.OrderAggregate.Requests;
using AeroTech.Ordering.RestApi.V1.OrderAggregate.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ordering.RestApi.V1.OrderAggregate.Controllers
{
    [Authorize]
    [Route(ApiSurfaceRoutes.OtaPanel)]
    public sealed class OtaPanelController : OrdersController
    {
        public OtaPanelController(IMediator mediator) : base(mediator)
        {
        }

        protected override OrderingApiSurface Surface => OrderingApiSurface.OtaPanel;

        [HttpPost(ApiSurfaceRoutes.OrdersFromOffer)]
        [ProducesResponseType(typeof(ApiResult<CreatedOrderResponse>), StatusCodes.Status201Created)]
        public Task<ActionResult<CreatedOrderResponse>> CreateOrderFromOfferAsync(
            [FromBody] CreateOrderRequest request,
            CancellationToken cancellationToken)
            => CreateAsync(request, null, null, cancellationToken);

        [HttpGet(ApiSurfaceRoutes.Orders + "/{orderId:long}")]
        [ProducesResponseType(typeof(ApiResult<OrderDetailsResponse>), StatusCodes.Status200OK)]
        public Task<ActionResult<OrderDetailsResponse>> GetOrderAsync(
            [FromRoute] long orderId,
            [FromQuery] long? minRevision,
            CancellationToken cancellationToken)
            => GetAsync(orderId, minRevision, cancellationToken);
    }
}
