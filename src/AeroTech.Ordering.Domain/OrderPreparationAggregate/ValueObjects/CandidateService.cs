using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateService(
        string ServiceRef,
        OrderServiceType Type,
        IReadOnlyList<string> BeneficiaryRefs,
        IReadOnlyList<string> SegmentRefs,
        decimal Quantity,
        OrderItemUnitOfMeasure QuantityUnit,
        string DetailSchema,
        int DetailSchemaVersion,
        IReadOnlyDictionary<string, string> Details,
        CandidateFulfillmentProfile FulfillmentProfile);
}
