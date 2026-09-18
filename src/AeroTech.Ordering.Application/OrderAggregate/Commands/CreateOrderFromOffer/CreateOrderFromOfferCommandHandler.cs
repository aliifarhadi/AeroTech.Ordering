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
using AeroTech.Ordering.Domain.OrderAggregate;
using AeroTech.Ordering.Domain.OrderAggregate.Arguments;
using AeroTech.Ordering.Domain.OrderAggregate.Contracts;
using AeroTech.Ordering.Domain.OrderPreparationAggregate;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Contracts;
using MediatR;
using Microsoft.Extensions.Options;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer
{
    public sealed class CreateOrderFromOfferCommandHandler : IRequestHandler<CreateOrderFromOfferCommand, CreatedOrderResult>
    {
        private readonly AuthorizedScopeResolver _scopeResolver;
        private readonly RequestDigester _digester;
        private readonly ICommandReceiptRepository _receipts;
        private readonly IOrderPreparationRepository _preparations;
        private readonly IOrderRepository _orders;
        private readonly IOrderReferenceGenerator _referenceGenerator;
        private readonly IOrderQuerySynchronizer _querySynchronizer;
        private readonly IAcceptanceProfilePolicy _acceptancePolicy;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdGenerator _ids;
        private readonly IClock _clock;
        private readonly OrderCreationOptions _options;

        public CreateOrderFromOfferCommandHandler(
            AuthorizedScopeResolver scopeResolver,
            RequestDigester digester,
            ICommandReceiptRepository receipts,
            IOrderPreparationRepository preparations,
            IOrderRepository orders,
            IOrderReferenceGenerator referenceGenerator,
            IOrderQuerySynchronizer querySynchronizer,
            IAcceptanceProfilePolicy acceptancePolicy,
            IUnitOfWork unitOfWork,
            IIdGenerator ids,
            IClock clock,
            IOptions<OrderCreationOptions> options)
        {
            _scopeResolver = scopeResolver;
            _digester = digester;
            _receipts = receipts;
            _preparations = preparations;
            _orders = orders;
            _referenceGenerator = referenceGenerator;
            _querySynchronizer = querySynchronizer;
            _acceptancePolicy = acceptancePolicy;
            _unitOfWork = unitOfWork;
            _ids = ids;
            _clock = clock;
            _options = options.Value;
        }

        public async Task<CreatedOrderResult> Handle(CreateOrderFromOfferCommand command, CancellationToken cancellationToken)
        {
            var scope = await _scopeResolver.ResolveAsync(command.Scope, cancellationToken);
            var digest = _digester.Digest(CanonicalRequest(command));
            var receiptScope = new ReceiptScope(scope.OwnerAirlineId, scope.FinancialCustomerId, scope.CallerScope,
                OrderingCommandKind.CreateOrderFromOffer, command.IdempotencyKey);

            if (await ReplayAsync(receiptScope, digest, cancellationToken) is { } replay)
                return replay;

            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    return await CommitAsync(command, scope, receiptScope, digest, cancellationToken);
                }
                catch (CommitConflictException conflict)
                    when (conflict.Kind == CommitConflictKind.OrderReference && attempt < _options.MaxReferenceAttempts)
                {
                }
                catch (CommitConflictException conflict)
                {
                    return await ResolveConflictAsync(conflict, receiptScope, digest, command.PreparationId, cancellationToken);
                }
            }
        }

        private async Task<CreatedOrderResult> CommitAsync(
            CreateOrderFromOfferCommand command,
            AuthorizedSalesScope scope,
            ReceiptScope receiptScope,
            string digest,
            CancellationToken cancellationToken)
        {
            var preparation = await _preparations.FindInScopeAsync(command.PreparationId, scope.OwnerAirlineId, scope.FinancialCustomerId, cancellationToken)
                              ?? throw ExceptionFactory.PreparationNotFound(command.PreparationId);

            EnsureSameSalesContext(preparation, scope);

            var now = _clock.GetDateTime();
            preparation.EnsureAcceptable(command.AcceptedSnapshotDigest, _acceptancePolicy, now);

            var order = Order.AcceptOriginalSale(
                new AcceptOriginalSaleArgs(
                    _ids.NewId(),
                    await _referenceGenerator.NextAsync(scope.OwnerAirlineId, cancellationToken),
                    preparation,
                    scope,
                    command.TravelerBindings,
                    command.Contacts,
                    command.AcceptedAt,
                    now,
                    command.ClientReference),
                _ids);

            preparation.Consume(order.Id, now);

            var result = CreatedOrderResult.From(order, _ids.NewId());

            _orders.Add(order);
            _receipts.Add(CommandReceipt.Completed(
                _ids.NewId(),
                result.OperationId,
                receiptScope,
                _digester.CanonicalizationVersion,
                digest,
                preparation.Id,
                order.Id,
                result.ToReceiptJson(),
                now));
            await _querySynchronizer.SyncAsync(order, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return result;
        }

        private static void EnsureSameSalesContext(OrderPreparation preparation, AuthorizedSalesScope scope)
        {
            if (preparation.FinancialCustomerId != scope.FinancialCustomerId
                || !string.Equals(preparation.Channel, scope.Channel, StringComparison.Ordinal)
                || preparation.SellingOfficeId != scope.SellingOfficeId)
                throw ExceptionFactory.SalesContextMismatch(preparation.Id);
        }

        private async Task<CreatedOrderResult> ResolveConflictAsync(
            CommitConflictException conflict,
            ReceiptScope receiptScope,
            string digest,
            long preparationId,
            CancellationToken cancellationToken)
        {
            if (await ReplayAsync(receiptScope, digest, cancellationToken) is { } replay)
                return replay;

            var current = await _preparations.FindInScopeAsync(preparationId, receiptScope.OwnerAirlineId, receiptScope.FinancialCustomerId, cancellationToken);

            if (current?.ConsumedByOrderId is { } existingOrderId)
                throw ExceptionFactory.PreparationAlreadyConsumed(preparationId, existingOrderId);

            throw conflict;
        }

        private async Task<CreatedOrderResult?> ReplayAsync(ReceiptScope receiptScope, string digest, CancellationToken cancellationToken)
        {
            var receipt = await _receipts.FindAsync(receiptScope, cancellationToken);

            if (receipt is null)
                return null;

            receipt.EnsureSameRequest(digest, receiptScope.FinancialCustomerId);
            return CreatedOrderResult.FromReceiptJson(receipt.ResultJson, receipt.OperationId);
        }

        private static IReadOnlyDictionary<string, object?> CanonicalRequest(CreateOrderFromOfferCommand command)
            => new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["command"] = nameof(OrderingCommandKind.CreateOrderFromOffer),
                ["preparationId"] = CanonicalJson.Identifier(command.PreparationId),
                ["acceptedSnapshotDigest"] = command.AcceptedSnapshotDigest,
                ["acceptedAt"] = CanonicalJson.Instant(command.AcceptedAt),
                ["travelerBindings"] = command.TravelerBindings
                    .OrderBy(binding => binding.SourceTravellerRef, StringComparer.Ordinal)
                    .Select(binding => (object?)new Dictionary<string, object?>
                    {
                        ["sourceTravellerRef"] = binding.SourceTravellerRef,
                        ["clientTravelerRef"] = binding.ClientTravelerRef,
                        ["givenName"] = binding.GivenName,
                        ["surname"] = binding.Surname,
                        ["passengerTypeCode"] = binding.PassengerTypeCode,
                        ["dateOfBirth"] = binding.DateOfBirth.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                        ["guardianClientTravelerRef"] = binding.GuardianClientTravelerRef
                    })
                    .ToList(),
                ["contacts"] = command.Contacts
                    .Select(contact => (object?)new Dictionary<string, object?>
                    {
                        ["role"] = contact.Role.ToString(),
                        ["email"] = contact.Email,
                        ["phone"] = contact.Phone
                    })
                    .ToList(),
                ["clientReference"] = command.ClientReference
            };
    }
}
