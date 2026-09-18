namespace AeroTech.Ordering.Application._Shared.Authorization
{
    public sealed record AuthorizedReadScope(
        long OwnerAirlineId,
        long? RestrictToFinancialCustomerId,
        bool MayReadProtectedPayloads);
}
