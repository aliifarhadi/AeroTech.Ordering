using AeroTech.Ordering.Application._Shared.Authorization;
using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer;
using MediatR;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer.Service
{
    public sealed class ServiceCreateOrderFromOfferCommandHandler : IRequestHandler<ServiceCreateOrderFromOfferCommand, CreateOrderFromOfferResult>
    {
        private readonly AuthorizedScopeResolver _scopeResolver;
        private readonly ICreateOrderFromOfferService _service;

        public ServiceCreateOrderFromOfferCommandHandler(AuthorizedScopeResolver scopeResolver, ICreateOrderFromOfferService service)
        {
            _scopeResolver = scopeResolver;
            _service = service;
        }

        public async Task<CreateOrderFromOfferResult> Handle(ServiceCreateOrderFromOfferCommand command, CancellationToken cancellationToken)
        {
            var scope = await _scopeResolver.ServiceSaleAsync(
                command.CustomerId, command.SellingOfficeId, command.SellingOfficeKind, cancellationToken);

            return await _service.ExecuteAsync(
                new CreateOrderFromOfferArgs(
                    scope,
                    command.IdempotencyKey,
                    command.OfferId,
                    command.Travellers,
                    command.Contacts,
                    command.ClientReference),
                cancellationToken);
        }
    }
}
