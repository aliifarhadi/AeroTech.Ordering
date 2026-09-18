using FluentValidation;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer.OtaPanel
{
    public sealed class OtaPanelCreateOrderFromOfferCommandValidator : AbstractValidator<OtaPanelCreateOrderFromOfferCommand>
    {
        public OtaPanelCreateOrderFromOfferCommandValidator()
        {
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
