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
    [AllowAnonymous]
    [Route(ApiSurfaceRoutes.Service)]
    public sealed class ServiceController : OrderPreparationsController
    {
        public ServiceController(IMediator mediator) : base(mediator)
        {
        }

        protected override OrderingApiSurface Surface => OrderingApiSurface.Service;

        [HttpPost(ApiSurfaceRoutes.OrderPreparations)]
        [ProducesResponseType(typeof(ApiResult<PreparationResponse>), StatusCodes.Status201Created)]
        public Task<ActionResult<PreparationResponse>> PrepareOrderFromOfferAsync(
            [FromBody] ServicePrepareOrderRequest request,
            CancellationToken cancellationToken)
            => PrepareAsync(request, request.FinancialCustomerId, request.SellingOfficeId, cancellationToken);
    }
}
