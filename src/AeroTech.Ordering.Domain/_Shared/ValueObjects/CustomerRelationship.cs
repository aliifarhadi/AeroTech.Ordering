namespace AeroTech.Ordering.Domain._Shared.ValueObjects
{
    public sealed record CustomerRelationship(
        long CustomerId,
        long? TravelAgencyId,
        bool IsTravelAgency,
        bool IsActive);
}
