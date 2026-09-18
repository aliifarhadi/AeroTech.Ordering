using System.Data.Common;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Exceptions;
using AeroTech.Ordering.Domain.OrderAggregate;
using AeroTech.Ordering.Domain.OrderAggregate.Contracts;
using AeroTech.Ordering.Persistence;
using AeroTech.Ordering.Persistence._Shared.Transactions;
using AeroTech.Ordering.Query._Shared.DbContexts;
using AeroTech.Ordering.Query.OrderAggregate.Dto;
using AeroTech.Ordering.Query.OrderAggregate.Models;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ordering.Synchronizer.OrderAggregate
{
    public sealed class OrderProjector : IOrderQuerySynchronizer, ICommandTransactionParticipant, IAsyncDisposable
    {
        private readonly OrderingDbContext _commandContext;
        private readonly IOrderProjectionFaultInjector _faultInjector;
        private readonly IClock _clock;
        private readonly List<PendingProjection> _pending = new();
        private OrderQueryDbContext? _projectionContext;

        public OrderProjector(OrderingDbContext commandContext, IOrderProjectionFaultInjector faultInjector, IClock clock)
        {
            _commandContext = commandContext;
            _faultInjector = faultInjector;
            _clock = clock;
        }

        public Task SyncAsync(Order order, CancellationToken cancellationToken = default)
        {
            Stage(order, conditionalReplace: false);
            return Task.CompletedTask;
        }

        public Task RebuildAsync(Order order, CancellationToken cancellationToken = default)
        {
            Stage(order, conditionalReplace: true);
            return Task.CompletedTask;
        }

        public async Task FlushAsync(DbTransaction transaction, CancellationToken cancellationToken)
        {
            if (_pending.Count == 0)
                return;

            var context = ProjectionContext();
            await context.Database.UseTransactionAsync(transaction, cancellationToken);

            foreach (var projection in _pending)
            {
                await _faultInjector.BeforeProjectionWriteAsync(projection.Row.OrderId, cancellationToken);

                if (projection.ConditionalReplace)
                    await ReplaceAsync(context, projection.Row, cancellationToken);
                else
                    context.Set<OrderDetailsReadModel>().Add(projection.Row);
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        public void Complete() => Reset();

        public void Abandon() => Reset();

        public async ValueTask DisposeAsync()
        {
            if (_projectionContext is not null)
                await _projectionContext.DisposeAsync();
        }

        private void Stage(Order order, bool conditionalReplace)
        {
            _pending.RemoveAll(projection => projection.Row.OrderId == order.Id);
            _pending.Add(new PendingProjection(new OrderDetailsReadModel
            {
                OrderId = order.Id,
                OwnerAirlineId = order.OwnerAirlineId,
                FinancialCustomerId = order.FinancialCustomerId,
                OrderReference = order.OrderReference,
                OrderRevision = order.OrderRevision,
                CommercialVersion = order.CommercialVersion,
                ProjectionSchemaVersion = OrderDtoJson.SchemaVersion,
                DetailsJson = OrderDtoJson.Write(OrderDtoBuilder.Build(order)),
                CreatedAt = order.CreatedAt,
                ProjectedAt = _clock.GetDateTime()
            }, conditionalReplace));

            _commandContext.Enlist(this);
        }

        private static async Task ReplaceAsync(OrderQueryDbContext context, OrderDetailsReadModel row, CancellationToken cancellationToken)
        {
            var updated = await context.Set<OrderDetailsReadModel>()
                .Where(existing => existing.OrderId == row.OrderId && existing.OrderRevision <= row.OrderRevision)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(existing => existing.OrderRevision, row.OrderRevision)
                    .SetProperty(existing => existing.CommercialVersion, row.CommercialVersion)
                    .SetProperty(existing => existing.ProjectionSchemaVersion, row.ProjectionSchemaVersion)
                    .SetProperty(existing => existing.DetailsJson, row.DetailsJson)
                    .SetProperty(existing => existing.ProjectedAt, row.ProjectedAt), cancellationToken);

            if (updated == 1)
                return;

            var exists = await context.Set<OrderDetailsReadModel>().AnyAsync(existing => existing.OrderId == row.OrderId, cancellationToken);

            if (exists)
                throw new CommitConflictException(CommitConflictKind.ProjectionRevision, $"order {row.OrderId} projection is newer than revision {row.OrderRevision}", new InvalidOperationException());

            context.Set<OrderDetailsReadModel>().Add(row);
        }

        private OrderQueryDbContext ProjectionContext()
        {
            if (_projectionContext is not null)
                return _projectionContext;

            var options = new DbContextOptionsBuilder<OrderQueryDbContext>()
                .UseSqlServer(_commandContext.Database.GetDbConnection())
                .Options;

            _projectionContext = new OrderQueryDbContext(options);
            return _projectionContext;
        }

        private void Reset()
        {
            _pending.Clear();
            _projectionContext?.ChangeTracker.Clear();
        }

        private sealed record PendingProjection(OrderDetailsReadModel Row, bool ConditionalReplace);
    }
}
