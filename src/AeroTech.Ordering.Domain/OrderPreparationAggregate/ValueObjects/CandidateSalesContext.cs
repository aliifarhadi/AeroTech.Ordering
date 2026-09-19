using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Messages.Shared.Enums;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record CandidateSalesContext(
        long OwnerAirlineId,
        long FinancialCustomerId,
        SalesContextSnapshot Sales,
        BuyerSnapshot Buyer)
    {
        public SalesChannel Channel => Sales.Channel;

        public long? SellingOfficeId => Sales.SellingOfficeId;

        public SellingOfficeKind? SellingOfficeKind => Sales.SellingOfficeKind;
    }
}
