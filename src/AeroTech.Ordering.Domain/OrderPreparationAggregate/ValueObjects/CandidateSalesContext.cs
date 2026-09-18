using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Messages.Shared.Enums;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateSalesContext(
        long OwnerAirlineId,
        long FinancialCustomerId,
        SalesChannel Channel,
        long? SellingOfficeId);
}
