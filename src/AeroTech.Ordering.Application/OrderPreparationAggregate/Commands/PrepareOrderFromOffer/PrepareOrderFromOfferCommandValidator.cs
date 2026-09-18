using FluentValidation;

namespace AeroTech.Ordering.Application.OrderPreparationAggregate.Commands.PrepareOrderFromOffer
{
    public sealed class PrepareOrderFromOfferCommandValidator : AbstractValidator<PrepareOrderFromOfferCommand>
    {
        public PrepareOrderFromOfferCommandValidator()
        {
            RuleFor(command => command.Scope).NotNull();
            RuleFor(command => command.IdempotencyKey).NotEmpty();
            RuleFor(command => command.OfferId).NotEmpty();
            RuleFor(command => command.ClientReference).NotEmpty().When(command => command.ClientReference is not null);
        }
    }
}
