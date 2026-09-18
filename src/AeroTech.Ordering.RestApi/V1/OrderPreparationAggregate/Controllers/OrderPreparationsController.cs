using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Application._Shared.Authorization;
using AeroTech.Ordering.Application.OrderPreparationAggregate.Commands.PrepareOrderFromOffer;
using AeroTech.Ordering.RestApi._Shared;
using AeroTech.Ordering.RestApi.V1.OrderPreparationAggregate.Requests;
using AeroTech.Ordering.RestApi.V1.OrderPreparationAggregate.Responses;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ordering.RestApi.V1.OrderPreparationAggregate.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    public abstract class OrderPreparationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        protected OrderPreparationsController(IMediator mediator) => _mediator = mediator;

        protected abstract OrderingApiSurface Surface { get; }

        protected async Task<ActionResult<PreparationResponse>> PrepareAsync(
            PrepareOrderRequest request,
            long? financialCustomerId,
            long? sellingOfficeId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new PrepareOrderFromOfferCommand(
                    new SalesScopeRequest(Surface, financialCustomerId, sellingOfficeId),
                    IdempotencyKey.Require(Request),
                    request.OfferId,
                    request.ClientReference),
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, PreparationResponse.From(result));
        }
    }
}
