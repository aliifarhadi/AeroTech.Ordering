using FluentValidation;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer.Service
{
    public sealed class ServiceCreateOrderFromOfferCommandValidator : AbstractValidator<ServiceCreateOrderFromOfferCommand>
    {
        public ServiceCreateOrderFromOfferCommandValidator()
        {
            RuleFor(command => command.CustomerId).GreaterThan(0);
            RuleFor(command => command.SellingOfficeId).GreaterThan(0).When(command => command.SellingOfficeId is not null);
            RuleFor(command => command.SellingOfficeKind).NotNull().When(command => command.SellingOfficeId is not null);
            RuleFor(command => command.SellingOfficeId).NotNull().When(command => command.SellingOfficeKind is not null);
            RuleFor(command => command.SellingOfficeKind)
                .NotEqual(AeroTech.Messages.Ordering.Enums.SellingOfficeKind.NotRecorded)
                .When(command => command.SellingOfficeKind is not null);
            RuleFor(command => command.OfferId).NotEmpty();
            RuleFor(command => command.IdempotencyKey).NotEmpty();
            RuleFor(command => command.Travellers).NotEmpty();
            RuleForEach(command => command.Travellers).ChildRules(traveller =>
            {
                traveller.RuleFor(item => item.OfferTravellerRef).NotEmpty();
                traveller.RuleFor(item => item.TravellerRef).NotEmpty();
                traveller.RuleFor(item => item.FirstName).NotEmpty();
                traveller.RuleFor(item => item.SurName).NotEmpty();
                traveller.RuleFor(item => item.PassengerType).IsInEnum();
                traveller.RuleFor(item => item.DateOfBirth).NotEqual(default(DateOnly));
            });
            RuleFor(command => command.Contacts).NotNull();
            RuleFor(command => command.ClientReference).NotEmpty().When(command => command.ClientReference is not null);
        }
    }
}
