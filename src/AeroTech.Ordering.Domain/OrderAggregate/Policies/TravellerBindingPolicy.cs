using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Domain.OrderAggregate.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Policies
{
    public static class TravellerBindingPolicy
    {
        public static void EnsureComplete(
            IReadOnlyList<CandidateTraveller> sourceTravellers,
            IReadOnlyList<TravellerBinding> bindings,
            DateTimeOffset acceptedAt)
        {
            var source = sourceTravellers.ToDictionary(traveller => traveller.TravellerRef, StringComparer.Ordinal);
            var bound = new HashSet<string>(StringComparer.Ordinal);
            var clients = new Dictionary<string, TravellerBinding>(StringComparer.Ordinal);

            foreach (var binding in bindings)
            {
                if (string.IsNullOrWhiteSpace(binding.SourceTravellerRef) || !source.TryGetValue(binding.SourceTravellerRef, out var traveler))
                    throw Invalid($"source traveller reference '{binding.SourceTravellerRef}' is not in the accepted preparation");

                if (!bound.Add(binding.SourceTravellerRef))
                    throw Invalid($"source traveller reference '{binding.SourceTravellerRef}' is bound more than once");

                if (string.IsNullOrWhiteSpace(binding.ClientTravellerRef) || !clients.TryAdd(binding.ClientTravellerRef, binding))
                    throw Invalid($"client traveller reference '{binding.ClientTravellerRef}' is missing or repeated");

                if (binding.PassengerTypeCode != traveler.PassengerTypeCode)
                    throw Invalid($"traveller '{binding.SourceTravellerRef}' passenger type '{binding.PassengerTypeCode}' differs from the accepted '{traveler.PassengerTypeCode}'");

                if (string.IsNullOrWhiteSpace(binding.GivenName) || string.IsNullOrWhiteSpace(binding.Surname))
                    throw Invalid($"traveller '{binding.SourceTravellerRef}' requires given name and surname");

                if (binding.DateOfBirth > DateOnly.FromDateTime(acceptedAt.UtcDateTime))
                    throw Invalid($"traveller '{binding.SourceTravellerRef}' date of birth is after acceptance");
            }

            foreach (var reference in source.Keys)
                if (!bound.Contains(reference))
                    throw Invalid($"source traveller reference '{reference}' is not bound");

            foreach (var binding in bindings)
                EnsureGuardianChain(binding, clients);
        }

        public static void EnsureContacts(IReadOnlyList<ContactDetails> contacts)
        {
            foreach (var contact in contacts)
            {
                if (!Enum.IsDefined(contact.Role))
                    throw ExceptionFactory.ContactInvalid("role is not defined");

                if (contact.Email is not null && string.IsNullOrWhiteSpace(contact.Email))
                    throw ExceptionFactory.ContactInvalid("email cannot be blank");

                if (contact.Phone is not null && string.IsNullOrWhiteSpace(contact.Phone))
                    throw ExceptionFactory.ContactInvalid("phone cannot be blank");
            }
        }

        private static void EnsureGuardianChain(TravellerBinding binding, IReadOnlyDictionary<string, TravellerBinding> clients)
        {
            var visited = new HashSet<string>(StringComparer.Ordinal) { binding.ClientTravellerRef };
            var current = binding;

            while (current.GuardianClientTravellerRef is { } guardianRef)
            {
                if (!clients.TryGetValue(guardianRef, out var guardian))
                    throw Invalid($"guardian '{guardianRef}' of traveller '{current.ClientTravellerRef}' is not a traveller in this order");

                if (!visited.Add(guardianRef))
                    throw Invalid($"guardian relation of traveller '{binding.ClientTravellerRef}' is cyclic");

                current = guardian;
            }
        }

        private static Exception Invalid(string reason) => ExceptionFactory.TravellerBindingInvalid(reason);
    }
}
