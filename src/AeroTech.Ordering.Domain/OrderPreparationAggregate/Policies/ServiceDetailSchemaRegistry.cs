using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.Policies
{
    public static class ServiceDetailSchemaRegistry
    {
        public const string AirTransportSchema = "AirTransport";
        public const int AirTransportSchemaVersion = 2;

        public const string CabinRef = "cabinRef";
        public const string RbdRef = "rbdRef";
        public const string BookingClass = "bookingClass";

        private static readonly IReadOnlyDictionary<(OrderServiceType Type, string Schema, int Version), IReadOnlySet<string>> Registered =
            new Dictionary<(OrderServiceType, string, int), IReadOnlySet<string>>
            {
                [(OrderServiceType.AirTransportation, AirTransportSchema, AirTransportSchemaVersion)] = new HashSet<string>(StringComparer.Ordinal)
                {
                    CabinRef,
                    RbdRef,
                    BookingClass
                }
            };

        public static void EnsureRegistered(OrderServiceType type, string schema, int version, IReadOnlyDictionary<string, string> details)
        {
            if (!Registered.TryGetValue((type, schema, version), out var attributes))
                throw ExceptionFactory.UnsupportedCapability($"service type {type} with detail schema {schema} v{version} is not registered");

            var unknown = details.Keys.Where(key => !attributes.Contains(key)).ToList();

            if (unknown.Count > 0)
                throw ExceptionFactory.CandidateContractMismatch($"detail schema {schema} v{version} does not define {string.Join(", ", unknown)}");

            if (details.Values.Any(string.IsNullOrWhiteSpace))
                throw ExceptionFactory.CandidateContractMismatch($"detail schema {schema} v{version} attributes cannot be empty");
        }
    }
}
