using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer;

namespace AeroTech.Ordering.RestApi.V1.OrderAggregate.Requests
{
    public static class OrderRequestMapper
    {
        public static IReadOnlyList<CreateOrderTravellerInput> ToInputs(this IReadOnlyList<OrderTravellerRequest> travellers)
            => travellers
                .Select(traveller => new CreateOrderTravellerInput(
                    traveller.OfferTravellerRef,
                    traveller.TravellerRef,
                    traveller.FirstName,
                    traveller.SurName,
                    traveller.PassengerType,
                    traveller.DateOfBirth,
                    traveller.GuardianTravellerRef))
                .ToList();

        public static IReadOnlyList<CreateOrderContactInput> ToInputs(this IReadOnlyList<OrderContactRequest> contacts)
            => contacts
                .Select(contact => new CreateOrderContactInput(contact.Role, contact.Email, contact.Phone))
                .ToList();
    }
}
