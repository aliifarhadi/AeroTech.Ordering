using FluentValidation;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.RebuildOrderProjection
{
    public sealed class RebuildOrderProjectionCommandValidator : AbstractValidator<RebuildOrderProjectionCommand>
    {
        public RebuildOrderProjectionCommandValidator()
        {
            RuleFor(command => command.Scope).NotNull();
            RuleFor(command => command.IdempotencyKey).NotEmpty();
            RuleFor(command => command.OrderId).GreaterThan(0);
        }
    }
}
