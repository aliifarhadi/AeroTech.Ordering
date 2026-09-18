using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Query.OperationAggregate.Queries.GetOperation;
using AeroTech.Ordering.RestApi.V1.OperationAggregate.Responses;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ordering.RestApi.V1.OperationAggregate.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    public abstract class OperationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        protected OperationsController(IMediator mediator) => _mediator = mediator;

        protected abstract OrderingApiSurface Surface { get; }

        protected async Task<ActionResult<OperationResponse>> GetAsync(long operationId, CancellationToken cancellationToken)
        {
            var view = await _mediator.Send(new GetOperationQuery(operationId, Surface), cancellationToken);

            return Ok(OperationResponse.From(view));
        }
    }
}
