using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.Serialization
{
    public static class CandidateVocabulary
    {
        public static readonly IReadOnlyDictionary<OrderServiceType, string> ServiceTypes = new Dictionary<OrderServiceType, string>
        {
            [OrderServiceType.AirTransportation] = "AirTransport",
            [OrderServiceType.SeatAssignment] = "Seat",
            [OrderServiceType.BaggageAllowance] = "Baggage",
            [OrderServiceType.Meal] = "Meal",
            [OrderServiceType.LoungeAccess] = "Lounge",
            [OrderServiceType.HotelStay] = "Hotel",
            [OrderServiceType.GroundTransport] = "GroundTransport"
        };

        public const string RegisteredExtension = "RegisteredExtension";

        public static readonly IReadOnlyDictionary<FulfillmentDocumentKind, string> DocumentKinds = new Dictionary<FulfillmentDocumentKind, string>
        {
            [FulfillmentDocumentKind.None] = "None",
            [FulfillmentDocumentKind.Etkt] = "ETKT",
            [FulfillmentDocumentKind.EmdA] = "EMDA",
            [FulfillmentDocumentKind.EmdS] = "EMDS"
        };

        public static string Name<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            if (!Enum.IsDefined(value))
                throw ExceptionFactory.CandidateContractMismatch($"{typeof(TEnum).Name} value {value} is not defined");

            return value switch
            {
                OrderServiceType serviceType => ServiceTypes.TryGetValue(serviceType, out var name)
                    ? name
                    : throw ExceptionFactory.UnsupportedCapability($"service type {serviceType} has no candidate vocabulary"),
                FulfillmentDocumentKind documentKind => DocumentKinds[documentKind],
                _ => value.ToString()
            };
        }

        public static TEnum Parse<TEnum>(string? name, string field) where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(name))
                throw ExceptionFactory.CandidateContractMismatch($"{field} is required");

            if (typeof(TEnum) == typeof(OrderServiceType))
            {
                if (name == RegisteredExtension)
                    throw ExceptionFactory.UnsupportedCapability($"{field} {name} is not registered");

                foreach (var pair in ServiceTypes)
                    if (pair.Value == name)
                        return (TEnum)(object)pair.Key;

                throw ExceptionFactory.UnsupportedCapability($"{field} {name} is not a known service type");
            }

            if (typeof(TEnum) == typeof(FulfillmentDocumentKind))
            {
                foreach (var pair in DocumentKinds)
                    if (pair.Value == name)
                        return (TEnum)(object)pair.Key;

                throw ExceptionFactory.CandidateContractMismatch($"{field} {name} is not defined");
            }

            if (Enum.TryParse<TEnum>(name, ignoreCase: false, out var parsed)
                && Enum.IsDefined(parsed)
                && !char.IsDigit(name[0])
                && name[0] != '-')
                return parsed;

            throw ExceptionFactory.CandidateContractMismatch($"{field} {name} is not defined");
        }
    }
}
