using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ordering.Domain.OrderAggregate.Contracts
{
    public interface IOrderQuerySynchronizer : IQueryDbSynchronizer
    {
        Task SyncAsync(Order order, CancellationToken cancellationToken = default);

        Task RebuildAsync(Order order, CancellationToken cancellationToken = default);
    }
}
