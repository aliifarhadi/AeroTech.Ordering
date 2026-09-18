using AeroTech.Ordering.Application._Shared.Authorization;
using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer;
using MediatR;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer.Backoffice
{
    public sealed class BackofficeCreateOrderFromOfferCommandHandler : IRequestHandler<BackofficeCreateOrderFromOfferCommand, CreateOrderFromOfferResult>
    {
        private readonly AuthorizedScopeResolver _scopeResolver;
        private readonly ICreateOrderFromOfferService _service;

        public BackofficeCreateOrderFromOfferCommandHandler(AuthorizedScopeResolver scopeResolver, ICreateOrderFromOfferService service)
        {
            _scopeResolver = scopeResolver;
            _service = service;
        }

        public async Task<CreateOrderFromOfferResult> Handle(BackofficeCreateOrderFromOfferCommand command, CancellationToken cancellationToken)
        {
            var scope = await _scopeResolver.BackofficeSaleAsync(command.CustomerId, command.AirlineOfficeId, cancellationToken);

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
