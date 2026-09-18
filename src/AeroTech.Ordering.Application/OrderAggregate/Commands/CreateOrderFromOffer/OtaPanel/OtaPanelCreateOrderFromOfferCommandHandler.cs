using AeroTech.Ordering.Application._Shared.Authorization;
using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer;
using MediatR;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer.OtaPanel
{
    public sealed class OtaPanelCreateOrderFromOfferCommandHandler : IRequestHandler<OtaPanelCreateOrderFromOfferCommand, CreateOrderFromOfferResult>
    {
        private readonly AuthorizedScopeResolver _scopeResolver;
        private readonly ICreateOrderFromOfferService _service;

        public OtaPanelCreateOrderFromOfferCommandHandler(AuthorizedScopeResolver scopeResolver, ICreateOrderFromOfferService service)
        {
            _scopeResolver = scopeResolver;
            _service = service;
        }

        public async Task<CreateOrderFromOfferResult> Handle(OtaPanelCreateOrderFromOfferCommand command, CancellationToken cancellationToken)
        {
            var scope = await _scopeResolver.OtaPanelSaleAsync(cancellationToken);

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
