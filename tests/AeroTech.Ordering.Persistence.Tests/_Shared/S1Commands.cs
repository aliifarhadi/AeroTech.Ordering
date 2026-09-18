using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer;
using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer.Backoffice;
using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer.Ota;
using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer.OtaPanel;
using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer.Service;

namespace AeroTech.Ordering.Persistence.Tests._Shared
{
    public static class S1Commands
    {
        public static string NewKey(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

        public static BackofficeCreateOrderFromOfferCommand Backoffice(
            string offerId,
            long customerId = S1Harness.CustomerId,
            long airlineOfficeId = S1Harness.AirlineOfficeId,
            string? key = null,
            IReadOnlyList<CreateOrderTravellerInput>? travellers = null)
            => new(
                customerId,
                airlineOfficeId,
                offerId,
                travellers ?? Travellers("PAX-A"),
                Contacts(),
                null,
                key ?? NewKey("create"));

        public static ServiceCreateOrderFromOfferCommand Service(
            string offerId,
            long customerId = S1Harness.CustomerId,
            long? airlineOfficeId = null,
            string? key = null,
            IReadOnlyList<CreateOrderTravellerInput>? travellers = null)
            => new(
                customerId,
                airlineOfficeId,
                offerId,
                travellers ?? Travellers("PAX-A"),
                Contacts(),
                null,
                key ?? NewKey("create"));

        public static OtaCreateOrderFromOfferCommand Ota(
            string offerId,
            string? key = null,
            IReadOnlyList<CreateOrderTravellerInput>? travellers = null)
            => new(
                offerId,
                travellers ?? Travellers("PAX-A"),
                Contacts(),
                null,
                key ?? NewKey("create"));

        public static OtaPanelCreateOrderFromOfferCommand OtaPanel(
            string offerId,
            string? key = null,
            IReadOnlyList<CreateOrderTravellerInput>? travellers = null)
            => new(
                offerId,
                travellers ?? Travellers("PAX-A"),
                Contacts(),
                null,
                key ?? NewKey("create"));

        public static IReadOnlyList<CreateOrderTravellerInput> Travellers(params string[] offerTravellerRefs)
            => offerTravellerRefs
                .Select(reference => new CreateOrderTravellerInput(
                    reference,
                    $"CLIENT-{reference}",
                    "Sample",
                    "Traveler",
                    PassengerTypeCode.ADT,
                    new DateOnly(1990, 1, 1),
                    null))
                .ToList();

        public static IReadOnlyList<CreateOrderContactInput> Contacts()
            => [new CreateOrderContactInput(ContactRole.Primary, "traveler@example.invalid", null)];
    }
}
