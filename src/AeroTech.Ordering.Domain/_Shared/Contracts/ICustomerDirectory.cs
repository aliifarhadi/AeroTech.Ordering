using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain._Shared.Contracts
{
    public interface ICustomerDirectory
    {
        Task<CustomerRelationship?> FindAsync(long customerId, CancellationToken cancellationToken = default);

        Task<CustomerRelationship?> FindByTravelAgencyAsync(long travelAgencyId, CancellationToken cancellationToken = default);
    }
}
