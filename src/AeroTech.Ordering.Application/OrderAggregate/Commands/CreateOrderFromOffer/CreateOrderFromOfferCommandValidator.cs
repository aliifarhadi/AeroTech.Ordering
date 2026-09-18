using FluentValidation;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer
{
    public sealed class CreateOrderFromOfferCommandValidator : AbstractValidator<CreateOrderFromOfferCommand>
    {
        public CreateOrderFromOfferCommandValidator()
        {
            RuleFor(command => command.Scope).NotNull();
            RuleFor(command => command.IdempotencyKey).NotEmpty();
            RuleFor(command => command.PreparationId).GreaterThan(0);
            RuleFor(command => command.AcceptedSnapshotDigest).NotEmpty().Matches("^[a-f0-9]{64}$");
            RuleFor(command => command.AcceptedAt).NotEqual(default(DateTimeOffset));
            RuleFor(command => command.TravelerBindings).NotEmpty();
            RuleFor(command => command.Contacts).NotNull();
            RuleFor(command => command.ClientReference).NotEmpty().When(command => command.ClientReference is not null);
        }
    }
}
