using AeroTech.Ordering.Domain._Shared.Contracts;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.ReferenceData.Persistence;
using AeroTech.Ordering.ReferenceData.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ordering.Persistence._Shared.CustomerContext
{
    public sealed class ReferenceDataCustomerDirectory : ICustomerDirectory
    {
        private readonly ReferenceDbContext _referenceDbContext;

        public ReferenceDataCustomerDirectory(ReferenceDbContext referenceDbContext)
            => _referenceDbContext = referenceDbContext;

        public Task<CustomerRelationship?> FindAsync(long customerId, CancellationToken cancellationToken = default)
            => Project(customer => customer.Id == customerId, cancellationToken);

        public Task<CustomerRelationship?> FindByTravelAgencyAsync(long travelAgencyId, CancellationToken cancellationToken = default)
            => Project(customer => customer.Type == CustomerType.TravelAgency && customer.TravelAgencyId == travelAgencyId, cancellationToken);

        private Task<CustomerRelationship?> Project(
            System.Linq.Expressions.Expression<Func<CustomerReadModel, bool>> predicate,
            CancellationToken cancellationToken)
            => _referenceDbContext.Customers
                .AsNoTracking()
                .Where(predicate)
                .Select(customer => new CustomerRelationship(
                    customer.Id,
                    customer.TravelAgencyId,
                    customer.Type == CustomerType.TravelAgency,
                    customer.Status == CustomerStatus.Active))
                .SingleOrDefaultAsync(cancellationToken);
    }
}
