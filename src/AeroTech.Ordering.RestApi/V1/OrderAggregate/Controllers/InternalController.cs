using AeroTech.Framework.Presentation.Responses;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Application._Shared.Authorization;
using AeroTech.Ordering.Application.OrderAggregate.Commands.RebuildOrderProjection;
using AeroTech.Ordering.RestApi._Shared;
using AeroTech.Ordering.RestApi.V1.OrderAggregate.Responses;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ordering.RestApi.V1.OrderAggregate.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [AllowAnonymous]
    [Route(ApiSurfaceRoutes.Internal)]
    public sealed class InternalController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InternalController(IMediator mediator) => _mediator = mediator;

        [HttpPost(ApiSurfaceRoutes.ProjectionRebuilds)]
        [ProducesResponseType(typeof(ApiResult<ProjectionRebuildResponse>), StatusCodes.Status201Created)]
        public async Task<ActionResult<ProjectionRebuildResponse>> RebuildProjectionAsync(
            [FromRoute] long orderId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new RebuildOrderProjectionCommand(
                    new AdministrativeScopeRequest(OrderingApiSurface.Internal),
                    IdempotencyKey.Require(Request),
                    orderId),
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, ProjectionRebuildResponse.From(result));
        }
    }
}
