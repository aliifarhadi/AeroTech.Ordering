using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
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
using AeroTech.Ordering.Domain.OrderAggregate.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Arguments;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Contracts;
using AeroTech.Ordering.Domain.Ports.Offers;
using Microsoft.Extensions.Options;

namespace AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer
{
    public sealed class CreateOrderFromOfferService : ICreateOrderFromOfferService
    {
        private readonly RequestDigester _digester;
        private readonly ICommandReceiptRepository _receipts;
        private readonly IOrderPreparationRepository _preparations;
        private readonly IOrderRepository _orders;
        private readonly IOfferSourcePort _offerSource;
        private readonly IOrderReferenceGenerator _referenceGenerator;
        private readonly IOrderQuerySynchronizer _querySynchronizer;
        private readonly IAcceptanceProfilePolicy _acceptancePolicy;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdGenerator _ids;
        private readonly IClock _clock;
        private readonly OrderCreationOptions _options;

        public CreateOrderFromOfferService(
            RequestDigester digester,
            ICommandReceiptRepository receipts,
            IOrderPreparationRepository preparations,
            IOrderRepository orders,
            IOfferSourcePort offerSource,
            IOrderReferenceGenerator referenceGenerator,
            IOrderQuerySynchronizer querySynchronizer,
            IAcceptanceProfilePolicy acceptancePolicy,
            IUnitOfWork unitOfWork,
            IIdGenerator ids,
            IClock clock,
            IOptions<OrderCreationOptions> options)
        {
            _digester = digester;
            _receipts = receipts;
            _preparations = preparations;
            _orders = orders;
            _offerSource = offerSource;
            _referenceGenerator = referenceGenerator;
            _querySynchronizer = querySynchronizer;
            _acceptancePolicy = acceptancePolicy;
            _unitOfWork = unitOfWork;
            _ids = ids;
            _clock = clock;
            _options = options.Value;
        }

        public async Task<CreateOrderFromOfferResult> ExecuteAsync(CreateOrderFromOfferArgs args, CancellationToken cancellationToken = default)
        {
            var receiptScope = new ReceiptScope(
                args.Scope.OwnerAirlineId,
                args.Scope.FinancialCustomerId,
                args.Scope.CallerScope,
                OrderingCommandKind.CreateOrderFromOffer,
                args.IdempotencyKey);
            var digest = _digester.Digest(CanonicalRequest(args));

            if (await ReplayAsync(receiptScope, digest, cancellationToken) is { } replay)
                return replay;

            var resolution = await ResolveCandidateAsync(args, cancellationToken);

            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    return await CommitAsync(args, receiptScope, digest, resolution, cancellationToken);
                }
                catch (CommitConflictException conflict)
                    when (conflict.Kind == CommitConflictKind.OrderReference && attempt < _options.MaxReferenceAttempts)
                {
                }
                catch (CommitConflictException conflict)
                {
                    return await ReplayAsync(receiptScope, digest, cancellationToken) ?? throw conflict;
                }
            }
        }

        private async Task<CandidateResolution> ResolveCandidateAsync(CreateOrderFromOfferArgs args, CancellationToken cancellationToken)
        {
            var resolution = await _offerSource.ResolveCandidateAsync(new ResolveCandidateRequest(args.OfferId, args.Scope, []), cancellationToken);
            var reasons = string.Join("; ", resolution.Reasons);

            return resolution.Outcome switch
            {
                OfferResolutionOutcome.Resolved when resolution.Candidate is not null && resolution.Evidence is not null => resolution,
                OfferResolutionOutcome.NotFound => throw ExceptionFactory.OfferNotFound(args.OfferId),
                OfferResolutionOutcome.UnsupportedCapability => throw ExceptionFactory.UnsupportedCapability(reasons),
                OfferResolutionOutcome.Unavailable => throw ExceptionFactory.OfferSourceUnavailable(reasons),
                _ => throw ExceptionFactory.CandidateContractMismatch(reasons.Length == 0 ? "offer source returned no candidate" : reasons)
            };
        }

        private async Task<CreateOrderFromOfferResult> CommitAsync(
            CreateOrderFromOfferArgs args,
            ReceiptScope receiptScope,
            string digest,
            CandidateResolution resolution,
            CancellationToken cancellationToken)
        {
            var now = _clock.GetDateTime();
            var preparation = CaptureAcceptedSource(args, resolution, now);

            preparation.EnsureAcceptable(_acceptancePolicy, now);

            var order = Order.AcceptOriginalSale(
                new AcceptOriginalSaleArgs(
                    _ids.NewId(),
                    await _referenceGenerator.NextAsync(args.Scope.OwnerAirlineId, cancellationToken),
                    preparation,
                    args.Scope,
                    Travelers(args),
                    Contacts(args),
                    now,
                    now,
                    args.ClientReference),
                _ids);

            preparation.Consume(order.Id, now);

            var result = CreateOrderFromOfferResult.From(order);

            _preparations.Add(preparation);
            _orders.Add(order);
            _receipts.Add(CommandReceipt.Completed(
                _ids.NewId(),
                _ids.NewId(),
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

        private OrderPreparation CaptureAcceptedSource(CreateOrderFromOfferArgs args, CandidateResolution resolution, DateTimeOffset now)
            => OrderPreparation.Capture(new CaptureOrderPreparationArgs(
                _ids.NewId(),
                _ids.NewId(),
                args.Scope,
                resolution.Candidate!,
                resolution.Profile,
                resolution.Evidence!,
                args.ClientReference,
                now));

        private async Task<CreateOrderFromOfferResult?> ReplayAsync(ReceiptScope receiptScope, string digest, CancellationToken cancellationToken)
        {
            var receipt = await _receipts.FindAsync(receiptScope, cancellationToken);

            if (receipt is null)
                return null;

            receipt.EnsureSameRequest(digest, receiptScope.FinancialCustomerId);

            return CreateOrderFromOfferResult.FromReceiptJson(receipt.ResultJson);
        }

        private static IReadOnlyList<TravelerBinding> Travelers(CreateOrderFromOfferArgs args)
            => args.Travellers
                .Select(traveller => new TravelerBinding(
                    traveller.OfferTravellerRef,
                    traveller.TravellerRef,
                    traveller.FirstName,
                    traveller.SurName,
                    traveller.PassengerType,
                    traveller.DateOfBirth,
                    traveller.GuardianTravellerRef))
                .ToList();

        private static IReadOnlyList<ContactDetails> Contacts(CreateOrderFromOfferArgs args)
            => args.Contacts
                .Select(contact => new ContactDetails(contact.Role, contact.Email, contact.Phone))
                .ToList();

        private static IReadOnlyDictionary<string, object?> CanonicalRequest(CreateOrderFromOfferArgs args)
            => new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["command"] = nameof(OrderingCommandKind.CreateOrderFromOffer),
                ["offerId"] = args.OfferId,
                ["clientReference"] = args.ClientReference,
                ["travellers"] = args.Travellers
                    .OrderBy(traveller => traveller.OfferTravellerRef, StringComparer.Ordinal)
                    .Select(traveller => (object?)new Dictionary<string, object?>
                    {
                        ["offerTravellerRef"] = traveller.OfferTravellerRef,
                        ["travellerRef"] = traveller.TravellerRef,
                        ["firstName"] = traveller.FirstName,
                        ["surName"] = traveller.SurName,
                        ["passengerType"] = traveller.PassengerType.ToString(),
                        ["dateOfBirth"] = traveller.DateOfBirth.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                        ["guardianTravellerRef"] = traveller.GuardianTravellerRef
                    })
                    .ToList(),
                ["contacts"] = args.Contacts
                    .Select(contact => (object?)new Dictionary<string, object?>
                    {
                        ["role"] = contact.Role.ToString(),
                        ["email"] = contact.Email,
                        ["phone"] = contact.Phone
                    })
                    .ToList()
            };
    }
}
