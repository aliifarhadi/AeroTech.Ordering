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
    [Route(ApiSurfaceRoutes.Backoffice)]
    public sealed class BackofficeController : OrderPreparationsController
    {
        public BackofficeController(IMediator mediator) : base(mediator)
        {
        }

        protected override OrderingApiSurface Surface => OrderingApiSurface.Backoffice;

        [HttpPost(ApiSurfaceRoutes.OrderPreparations)]
        [ProducesResponseType(typeof(ApiResult<PreparationResponse>), StatusCodes.Status201Created)]
        public Task<ActionResult<PreparationResponse>> PrepareOrderFromOfferAsync(
            [FromBody] BackofficePrepareOrderRequest request,
            CancellationToken cancellationToken)
            => PrepareAsync(request, request.FinancialCustomerId, request.SellingOfficeId, cancellationToken);
    }
}
