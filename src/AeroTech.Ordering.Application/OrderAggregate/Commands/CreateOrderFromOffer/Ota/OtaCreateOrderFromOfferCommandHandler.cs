using AeroTech.Ordering.Application._Shared.Authorization;
using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer;
using MediatR;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer.Ota
{
    public sealed class OtaCreateOrderFromOfferCommandHandler : IRequestHandler<OtaCreateOrderFromOfferCommand, CreateOrderFromOfferResult>
    {
        private readonly AuthorizedScopeResolver _scopeResolver;
        private readonly ICreateOrderFromOfferService _service;

        public OtaCreateOrderFromOfferCommandHandler(AuthorizedScopeResolver scopeResolver, ICreateOrderFromOfferService service)
        {
            _scopeResolver = scopeResolver;
            _service = service;
        }

        public async Task<CreateOrderFromOfferResult> Handle(OtaCreateOrderFromOfferCommand command, CancellationToken cancellationToken)
        {
            var scope = await _scopeResolver.OtaSaleAsync(cancellationToken);

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
