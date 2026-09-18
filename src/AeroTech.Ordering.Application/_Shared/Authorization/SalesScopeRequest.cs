using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Application._Shared.Authorization
{
    public sealed record SalesScopeRequest(
        OrderingApiSurface Surface,
        long? FinancialCustomerId,
        long? SellingOfficeId);
}
