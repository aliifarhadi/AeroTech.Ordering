using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateSalesContext(
        long OwnerAirlineId,
        long FinancialCustomerId,
        string Channel,
        long? SellingOfficeId);
}
