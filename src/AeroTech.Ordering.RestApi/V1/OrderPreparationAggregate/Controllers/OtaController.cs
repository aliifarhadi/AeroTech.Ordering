using AeroTech.Framework.Presentation.Responses;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.RestApi._Shared;
using AeroTech.Ordering.RestApi.V1.OrderPreparationAggregate.Requests;
using AeroTech.Ordering.RestApi.V1.OrderPreparationAggregate.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ordering.RestApi.V1.OrderPreparationAggregate.Controllers
{
    [Authorize]
    [Route(ApiSurfaceRoutes.Ota)]
    public sealed class OtaController : OrderPreparationsController
    {
        public OtaController(IMediator mediator) : base(mediator)
        {
        }

        protected override OrderingApiSurface Surface => OrderingApiSurface.Ota;

        [HttpPost(ApiSurfaceRoutes.OrderPreparations)]
        [ProducesResponseType(typeof(ApiResult<PreparationResponse>), StatusCodes.Status201Created)]
        public Task<ActionResult<PreparationResponse>> PrepareOrderFromOfferAsync(
            [FromBody] PrepareOrderRequest request,
            CancellationToken cancellationToken)
            => PrepareAsync(request, null, null, cancellationToken);
    }
}
