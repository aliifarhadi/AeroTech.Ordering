using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Application._Shared.Authorization;
using AeroTech.Ordering.Application._Shared.Idempotency;
using AeroTech.Ordering.Domain._Shared.Exceptions;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Domain._Shared.Serialization;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.CommandReceiptAggregate;
using AeroTech.Ordering.Domain.CommandReceiptAggregate.Contracts;
using AeroTech.Ordering.Domain.OrderPreparationAggregate;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Arguments;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Contracts;
using AeroTech.Ordering.Domain.Ports.Offers;
using MediatR;

namespace AeroTech.Ordering.Application.OrderPreparationAggregate.Commands.PrepareOrderFromOffer
{
    public sealed class PrepareOrderFromOfferCommandHandler : IRequestHandler<PrepareOrderFromOfferCommand, PreparationResult>
    {
        private readonly AuthorizedScopeResolver _scopeResolver;
        private readonly RequestDigester _digester;
        private readonly ICommandReceiptRepository _receipts;
        private readonly IOrderPreparationRepository _preparations;
        private readonly IOfferSourcePort _offerSource;
        private readonly IAcceptanceProfilePolicy _acceptancePolicy;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdGenerator _ids;
        private readonly IClock _clock;

        public PrepareOrderFromOfferCommandHandler(
            AuthorizedScopeResolver scopeResolver,
            RequestDigester digester,
            ICommandReceiptRepository receipts,
            IOrderPreparationRepository preparations,
            IOfferSourcePort offerSource,
            IAcceptanceProfilePolicy acceptancePolicy,
            IUnitOfWork unitOfWork,
            IIdGenerator ids,
            IClock clock)
        {
            _scopeResolver = scopeResolver;
            _digester = digester;
            _receipts = receipts;
            _preparations = preparations;
            _offerSource = offerSource;
            _acceptancePolicy = acceptancePolicy;
            _unitOfWork = unitOfWork;
            _ids = ids;
            _clock = clock;
        }

        public async Task<PreparationResult> Handle(PrepareOrderFromOfferCommand command, CancellationToken cancellationToken)
        {
            var scope = await _scopeResolver.ResolveAsync(command.Scope, cancellationToken);
            var digest = _digester.Digest(new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["command"] = nameof(OrderingCommandKind.PrepareOrderFromOffer),
                ["offerId"] = command.OfferId,
                ["clientReference"] = command.ClientReference,
                ["requestedSelection"] = new List<object?>()
            });
            var receiptScope = new ReceiptScope(scope.OwnerAirlineId, scope.FinancialCustomerId, scope.CallerScope,
                OrderingCommandKind.PrepareOrderFromOffer, command.IdempotencyKey);

            if (await ReplayAsync(receiptScope, digest, cancellationToken) is { } replay)
                return replay;

            var resolution = await _offerSource.ResolveCandidateAsync(new ResolveCandidateRequest(command.OfferId, scope, []), cancellationToken);
            var candidate = EnsureResolved(command.OfferId, resolution);
            var now = _clock.GetDateTime();

            var preparation = OrderPreparation.Capture(new CaptureOrderPreparationArgs(
                _ids.NewId(),
                _ids.NewId(),
                scope,
                candidate,
                resolution.Profile,
                resolution.Evidence ?? throw ExceptionFactory.CandidateContractMismatch("resolved candidate has no source evidence"),
                command.ClientReference,
                now));

            var operationId = _ids.NewId();
            _preparations.Add(preparation);
            _receipts.Add(CommandReceipt.Completed(
                _ids.NewId(),
                operationId,
                receiptScope,
                _digester.CanonicalizationVersion,
                digest,
                preparation.Id,
                null,
                CanonicalJson.Write(new Dictionary<string, object?> { ["preparationId"] = CanonicalJson.Identifier(preparation.Id) }),
                now));

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (CommitConflictException conflict) when (conflict.Kind == CommitConflictKind.CommandReceiptKey)
            {
                return await ReplayAsync(receiptScope, digest, cancellationToken) ?? throw conflict;
            }

            return PreparationResult.From(preparation, operationId, _acceptancePolicy, replayed: false);
        }

        private async Task<PreparationResult?> ReplayAsync(ReceiptScope receiptScope, string digest, CancellationToken cancellationToken)
        {
            var receipt = await _receipts.FindAsync(receiptScope, cancellationToken);

            if (receipt is null)
                return null;

            receipt.EnsureSameRequest(digest, receiptScope.FinancialCustomerId);

            var preparation = await _preparations.FindInScopeAsync(receipt.PreparationId!.Value, receiptScope.OwnerAirlineId, receiptScope.FinancialCustomerId, cancellationToken)
                              ?? throw ExceptionFactory.PreparationNotFound(receipt.PreparationId);

            return PreparationResult.From(preparation, receipt.OperationId, _acceptancePolicy, replayed: true);
        }

        private static Domain.OrderPreparationAggregate.ValueObjects.NormalizedCandidate EnsureResolved(string offerId, CandidateResolution resolution)
        {
            var reasons = string.Join("; ", resolution.Reasons);

            return resolution.Outcome switch
            {
                OfferResolutionOutcome.Resolved when resolution.Candidate is not null => resolution.Candidate,
                OfferResolutionOutcome.NotFound => throw ExceptionFactory.OfferNotFound(offerId),
                OfferResolutionOutcome.UnsupportedCapability => throw ExceptionFactory.UnsupportedCapability(reasons),
                OfferResolutionOutcome.Unavailable => throw ExceptionFactory.OfferSourceUnavailable(reasons),
                _ => throw ExceptionFactory.CandidateContractMismatch(reasons.Length == 0 ? "offer source returned no candidate" : reasons)
            };
        }
    }
}
