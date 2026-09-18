using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Application._Shared.Authorization;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Query._Shared.DbContexts;
using AeroTech.Ordering.Query.OperationAggregate.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ordering.Query.OperationAggregate.Queries.GetOperation
{
    public sealed class GetOperationQueryHandler : IRequestHandler<GetOperationQuery, OperationDetailsView>
    {
        private readonly OrderQueryDbContext _dbContext;
        private readonly AuthorizedScopeResolver _scopeResolver;

        public GetOperationQueryHandler(OrderQueryDbContext dbContext, AuthorizedScopeResolver scopeResolver)
        {
            _dbContext = dbContext;
            _scopeResolver = scopeResolver;
        }

        public async Task<OperationDetailsView> Handle(GetOperationQuery query, CancellationToken cancellationToken)
        {
            var scope = await _scopeResolver.ResolveReadScopeAsync(query.Surface, cancellationToken);

            var operation = await _dbContext.Set<OperationReadModel>()
                .AsNoTracking()
                .Where(row => row.OperationId == query.OperationId && row.OwnerAirlineId == scope.OwnerAirlineId)
                .Where(row => scope.RestrictToFinancialCustomerId == null || row.FinancialCustomerId == scope.RestrictToFinancialCustomerId)
                .SingleOrDefaultAsync(cancellationToken)
                ?? throw ExceptionFactory.OperationNotFound(query.OperationId);

            var (phase, outcome) = operation.Status switch
            {
                CommandReceiptStatus.Completed => ("Completed", "Succeeded"),
                CommandReceiptStatus.Rejected => ("Rejected", "Rejected"),
                CommandReceiptStatus.Unknown => ("AwaitingExternal", "Unknown"),
                CommandReceiptStatus.NeedsReconciliation => ("NeedsReconciliation", "Unknown"),
                _ => ("Executing", "Pending")
            };

            return new OperationDetailsView(
                operation.OperationId,
                operation.OrderId,
                operation.PreparationId,
                operation.CommandKind.ToString(),
                phase,
                outcome,
                [],
                [],
                operation.Status == CommandReceiptStatus.NeedsReconciliation,
                operation.CreatedAt,
                operation.CompletedAt);
        }
    }
}
