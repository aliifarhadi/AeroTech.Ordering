using AeroTech.Framework.Presentation.Responses;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.RestApi._Shared;
using AeroTech.Ordering.RestApi.V1.OperationAggregate.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ordering.RestApi.V1.OperationAggregate.Controllers
{
    [Authorize]
    [Route(ApiSurfaceRoutes.OtaPanel)]
    public sealed class OtaPanelController : OperationsController
    {
        public OtaPanelController(IMediator mediator) : base(mediator)
        {
        }

        protected override OrderingApiSurface Surface => OrderingApiSurface.OtaPanel;

        [HttpGet(ApiSurfaceRoutes.Operations + "/{operationId:long}")]
        [ProducesResponseType(typeof(ApiResult<OperationResponse>), StatusCodes.Status200OK)]
        public Task<ActionResult<OperationResponse>> GetOperationAsync(
            [FromRoute] long operationId,
            CancellationToken cancellationToken)
            => GetAsync(operationId, cancellationToken);
    }
}
