using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Application._Shared.Authorization;
using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer;
using AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrder;
using AeroTech.Ordering.RestApi._Shared;
using AeroTech.Ordering.RestApi.V1.OrderAggregate.Requests;
using AeroTech.Ordering.RestApi.V1.OrderAggregate.Responses;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ordering.RestApi.V1.OrderAggregate.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    public abstract class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        protected OrdersController(IMediator mediator) => _mediator = mediator;

        protected abstract OrderingApiSurface Surface { get; }

        protected async Task<ActionResult<CreatedOrderResponse>> CreateAsync(
            CreateOrderRequest request,
            long? financialCustomerId,
            long? sellingOfficeId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateOrderFromOfferCommand(
                    new SalesScopeRequest(Surface, financialCustomerId, sellingOfficeId),
                    IdempotencyKey.Require(Request),
                    request.PreparationId!.Value,
                    request.AcceptedSnapshotDigest,
                    request.AcceptedAt!.Value,
                    request.TravelerBindings.Select(binding => binding.ToBinding()).ToList(),
                    request.Contacts.Select(contact => contact.ToDetails()).ToList(),
                    request.ClientReference),
                cancellationToken);

            var orderUrl = OrderUrl(result.OrderId);

            return Created(orderUrl, CreatedOrderResponse.From(result, orderUrl));
        }

        protected async Task<ActionResult<OrderDetailsResponse>> GetAsync(long orderId, long? minRevision, CancellationToken cancellationToken)
        {
            var view = await _mediator.Send(new GetOrderQuery(orderId, Surface, minRevision), cancellationToken);

            return Ok(OrderDetailsResponse.From(view));
        }

        private string OrderUrl(long orderId)
            => $"/{CallerScopeKey.Surface(Surface)}/v1/{ApiSurfaceRoutes.Orders}/{orderId}";
    }
}
