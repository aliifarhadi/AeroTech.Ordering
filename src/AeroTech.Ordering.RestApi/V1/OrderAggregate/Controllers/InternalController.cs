using AeroTech.Framework.Presentation.Responses;
using AeroTech.Ordering.Application.OrderAggregate.Commands.RebuildOrderProjection;
using AeroTech.Ordering.RestApi._Shared;
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
    [Tags("Internal")]
    [Route($"Internal/v{{version:apiVersion}}/Orders")]
    public sealed class InternalController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InternalController(IMediator mediator) => _mediator = mediator;

        [HttpPost("{id:long}/ProjectionRebuilds")]
        [ProducesResponseType(typeof(ApiResult<RebuildOrderProjectionResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RebuildProjection(long id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RebuildOrderProjectionCommand(id, IdempotencyKey.Require(Request)), cancellationToken);

            return Ok(result);
        }
    }
}
