using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Application._Shared.Authorization;
using AeroTech.Ordering.Application._Shared.Idempotency;
using AeroTech.Ordering.Domain._Shared.Exceptions;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Domain._Shared.Serialization;
using AeroTech.Ordering.Domain.CommandReceiptAggregate;
using AeroTech.Ordering.Domain.CommandReceiptAggregate.Contracts;
using AeroTech.Ordering.Domain.OrderAggregate.Contracts;
using MediatR;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.RebuildOrderProjection
{
    public sealed class RebuildOrderProjectionCommandHandler : IRequestHandler<RebuildOrderProjectionCommand, RebuildOrderProjectionResult>
    {
        private readonly AuthorizedScopeResolver _scopeResolver;
        private readonly RequestDigester _digester;
        private readonly ICommandReceiptRepository _receipts;
        private readonly IOrderRepository _orders;
        private readonly IOrderQuerySynchronizer _querySynchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdGenerator _ids;
        private readonly IClock _clock;

        public RebuildOrderProjectionCommandHandler(
            AuthorizedScopeResolver scopeResolver,
            RequestDigester digester,
            ICommandReceiptRepository receipts,
            IOrderRepository orders,
            IOrderQuerySynchronizer querySynchronizer,
            IUnitOfWork unitOfWork,
            IIdGenerator ids,
            IClock clock)
        {
            _scopeResolver = scopeResolver;
            _digester = digester;
            _receipts = receipts;
            _orders = orders;
            _querySynchronizer = querySynchronizer;
            _unitOfWork = unitOfWork;
            _ids = ids;
            _clock = clock;
        }

        public async Task<RebuildOrderProjectionResult> Handle(RebuildOrderProjectionCommand command, CancellationToken cancellationToken)
        {
            var administrativeScope = await _scopeResolver.InternalAsync(cancellationToken);
            var ownerAirlineId = administrativeScope.OwnerAirlineId;
            var order = await _orders.LoadSnapshotAsync(command.OrderId, ownerAirlineId, cancellationToken)
                        ?? throw ExceptionFactory.OrderNotFound(command.OrderId);

            var digest = _digester.Digest(new Dictionary<string, object?>
            {
                ["command"] = nameof(OrderingCommandKind.RebuildOrderProjection),
                ["orderId"] = CanonicalJson.Identifier(command.OrderId)
            });
            var receiptScope = new ReceiptScope(ownerAirlineId, order.FinancialCustomerId, administrativeScope.CallerScope,
                OrderingCommandKind.RebuildOrderProjection, command.IdempotencyKey);

            var existing = await _receipts.FindAsync(receiptScope, cancellationToken);

            if (existing is not null)
            {
                existing.EnsureSameRequest(digest, receiptScope.FinancialCustomerId);
                return RebuildOrderProjectionResult.FromReceiptJson(existing.ResultJson, existing.Id);
            }

            await _querySynchronizer.RebuildAsync(order, cancellationToken);

            var result = new RebuildOrderProjectionResult(order.Id, _ids.NewId(), order.OrderRevision, order.CommercialVersion, false);

            _receipts.Add(CommandReceipt.Completed(
                result.ReceiptId,
                receiptScope,
                _digester.CanonicalizationVersion,
                digest,
                order.Id,
                result.ToReceiptJson(),
                _clock.GetDateTime()));

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (CommitConflictException conflict) when (conflict.Kind == CommitConflictKind.CommandReceiptKey)
            {
                var winner = await _receipts.FindAsync(receiptScope, cancellationToken) ?? throw conflict;
                winner.EnsureSameRequest(digest, receiptScope.FinancialCustomerId);
                return RebuildOrderProjectionResult.FromReceiptJson(winner.ResultJson, winner.Id);
            }
            catch (CommitConflictException conflict) when (conflict.Kind == CommitConflictKind.ProjectionRevision)
            {
                throw ExceptionFactory.ProjectionRebuildConflict(command.OrderId);
            }

            return result;
        }
    }
}
