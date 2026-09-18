using AeroTech.Framework.Presentation.Responses;
using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer;
using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer.Ota;
using AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrderById.Ota;
using AeroTech.Ordering.Query.OrderAggregate.Dto;
using AeroTech.Ordering.RestApi._Shared;
using AeroTech.Ordering.RestApi.V1.OrderAggregate.Requests;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ordering.RestApi.V1.OrderAggregate.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize]
    [Tags("OTA Api")]
    [Route($"Api/v{{version:apiVersion}}/Bookings")]
    public sealed class OtaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OtaController(IMediator mediator) => _mediator = mediator;

        [HttpPost("FlightOffers")]
        [ProducesResponseType(typeof(ApiResult<CreateOrderFromOfferResult>), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateFromOffer([FromBody] OtaCreateOrderFromOfferRequest request, CancellationToken cancellationToken)
        {
            var command = new OtaCreateOrderFromOfferCommand(
                request.OfferId,
                request.Travellers.ToInputs(),
                request.Contacts.ToInputs(),
                request.ClientReference,
                IdempotencyKey.Require(Request));

            var result = await _mediator.Send(command, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = result.OrderId }, result);
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(ApiResult<OrderDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new OtaGetOrderByIdQuery(id), cancellationToken);

            return Ok(result);
        }
    }
}
