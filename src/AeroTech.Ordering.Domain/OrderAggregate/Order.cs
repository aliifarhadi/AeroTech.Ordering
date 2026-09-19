using AeroTech.Framework.Core.Domain.Aggregates;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Aegis.Enums;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Domain._Shared.Serialization;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderAggregate.Arguments;
using AeroTech.Ordering.Domain.OrderAggregate.DomainEvents;
using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Domain.OrderAggregate.Policies;
using AeroTech.Ordering.Domain.OrderAggregate.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;
using AeroTech.Messages.Shared.Enums;

namespace AeroTech.Ordering.Domain.OrderAggregate
{
    public sealed class Order : AggregateRoot<long>
    {
        private readonly List<OrderTraveler> _travelers = new();
        private readonly List<OrderContact> _contacts = new();
        private readonly List<OrderJourney> _journeys = new();
        private readonly List<OrderSegment> _segments = new();
        private readonly List<OrderItem> _items = new();
        private readonly List<OrderService> _services = new();
        private readonly List<OrderItemServiceLink> _itemServiceLinks = new();
        private readonly List<OrderChange> _changes = new();
        private readonly List<PriceChangeSet> _priceChangeSets = new();
        private readonly List<PricingLine> _pricingLines = new();
        private readonly List<FareConstruction> _fareConstructions = new();
        private readonly List<FundingObligation> _fundingObligations = new();
        private readonly List<OrderComponentTotal> _componentTotals = new();

        private Order()
        {
        }

        public string OrderReference { get; private set; } = null!;

        public long RootOrderId { get; private set; }

        public long OwnerAirlineId { get; private set; }

        public long FinancialCustomerId { get; private set; }

        public SalesContextSnapshot SalesContext { get; private set; } = null!;

        public BuyerSnapshot Buyer { get; private set; } = null!;

        public InitiatingActorSnapshot InitiatingActor { get; private set; } = null!;

        public SalesChannel Channel => SalesContext.Channel;

        public SellingOfficeKind? SellingOfficeKind => SalesContext.SellingOfficeKind;

        public long? SellingOfficeId => SalesContext.SellingOfficeId;

        public CurrencySnapshot SaleCurrency { get; private set; } = null!;

        public AcceptedSource AcceptedSource { get; private set; } = null!;

        public long SourcePreparationId { get; private set; }

        public ValidityFact OfferValidity { get; private set; } = null!;

        public ValidityFact PriceValidity { get; private set; } = null!;

        public ValidityFact TicketingValidity { get; private set; } = null!;

        public ObservedTimeFact? ObservedTicketingDeadline { get; private set; }

        public string? SourceJourneyTypeRaw { get; private set; }

        public JourneyType? JourneyType { get; private set; }

        public CommercialSummary CommercialSummary { get; private set; }

        public Money CustomerTotal { get; private set; } = null!;

        public int CommercialVersion { get; private set; }

        public int FinancialSequence { get; private set; }

        public long OrderRevision { get; private set; }

        public long LastEventOrdinal { get; private set; }

        public string? ClientReference { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public IReadOnlyCollection<OrderTraveler> Travelers => _travelers.AsReadOnly();

        public IReadOnlyCollection<OrderContact> Contacts => _contacts.AsReadOnly();

        public IReadOnlyCollection<OrderJourney> Journeys => _journeys.AsReadOnly();

        public IReadOnlyCollection<OrderSegment> Segments => _segments.AsReadOnly();

        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

        public IReadOnlyCollection<OrderService> Services => _services.AsReadOnly();

        public IReadOnlyCollection<OrderItemServiceLink> ItemServiceLinks => _itemServiceLinks.AsReadOnly();

        public IReadOnlyCollection<OrderChange> Changes => _changes.AsReadOnly();

        public IReadOnlyCollection<PriceChangeSet> PriceChangeSets => _priceChangeSets.AsReadOnly();

        public IReadOnlyCollection<PricingLine> PricingLines => _pricingLines.AsReadOnly();

        public IReadOnlyCollection<FareConstruction> FareConstructions => _fareConstructions.AsReadOnly();

        public IReadOnlyCollection<FundingObligation> FundingObligations => _fundingObligations.AsReadOnly();

        public IReadOnlyCollection<OrderComponentTotal> ComponentTotals => _componentTotals.AsReadOnly();

        public static Order AcceptOriginalSale(AcceptOriginalSaleArgs args, IIdGenerator ids)
        {
            var preparation = args.Preparation;
            var candidate = preparation.Candidate;

            if (string.IsNullOrWhiteSpace(args.OrderReference))
                throw ExceptionFactory.CandidateContractMismatch("an order reference is required");

            if (args.AcceptingScope.OwnerAirlineId != preparation.OwnerAirlineId
                || args.AcceptingScope.FinancialCustomerId != preparation.FinancialCustomerId)
                throw ExceptionFactory.PreparationNotFound(preparation.Id);

            TravelerBindingPolicy.EnsureComplete(candidate.Travelers, args.TravelerBindings, args.AcceptedAt);
            TravelerBindingPolicy.EnsureContacts(args.Contacts);

            var order = new Order
            {
                Id = args.OrderId,
                OrderReference = args.OrderReference,
                RootOrderId = args.OrderId,
                OwnerAirlineId = preparation.OwnerAirlineId,
                FinancialCustomerId = preparation.FinancialCustomerId,
                SalesContext = candidate.SalesContext.Sales,
                Buyer = candidate.SalesContext.Buyer,
                InitiatingActor = args.AcceptingScope.InitiatingActor,
                SaleCurrency = candidate.SaleCurrency,
                SourcePreparationId = preparation.Id,
                AcceptedSource = new AcceptedSource(
                    preparation.Id,
                    preparation.SourceOwner,
                    preparation.SourceOfferId,
                    preparation.ProviderProfileId,
                    preparation.ContractVersion,
                    preparation.AcceptanceProfile,
                    preparation.AcceptanceAssurance,
                    preparation.OwnerBindingRef,
                    preparation.SnapshotDigest,
                    preparation.SourcePayloadHash,
                    preparation.PricedAt,
                    preparation.CapturedAt,
                    args.ClientAcceptedAt,
                    args.AcceptedAt),
                OfferValidity = preparation.OfferValidity,
                PriceValidity = preparation.PriceValidity,
                TicketingValidity = preparation.TicketingValidity,
                ObservedTicketingDeadline = candidate.Validity.ObservedTicketingDeadline,
                SourceJourneyTypeRaw = candidate.SourceJourneyTypeRaw,
                JourneyType = candidate.JourneyType,
                ClientReference = args.ClientReference,
                CreatedAt = args.AcceptedAt,
                CommercialVersion = 1,
                FinancialSequence = 1,
                OrderRevision = 1,
                LastEventOrdinal = 1
            };

            var decisionRef = $"preparation:{preparation.Id}:{preparation.SnapshotDigest}";

            var change = new OrderChange(ids.NewId(), order.Id, OrderChangeType.Create, order.CommercialVersion,
                args.AcceptingScope.ActorContextType, args.AcceptingScope.ActorId, decisionRef, args.AcceptedAt);
            order._changes.Add(change);

            var priceSet = new PriceChangeSet(ids.NewId(), order.Id, change.Id, order.FinancialSequence,
                PriceChangeReason.OriginalSale, decisionRef, 0, args.AcceptedAt);
            order._priceChangeSets.Add(priceSet);

            var travelerIds = order.AddTravelers(args.TravelerBindings, ids);
            order.AddContacts(args.Contacts, ids);

            var journeyIds = order.AddJourneys(candidate, ids);
            var segmentIds = order.AddSegments(candidate, journeyIds, ids);
            var (itemIds, serviceIds) = order.AddItemsAndServices(candidate, travelerIds, segmentIds, change.Id, ids);

            order.AddPricing(candidate, priceSet.Id, itemIds, serviceIds, segmentIds, ids);
            order.AddFareConstruction(candidate, change.Id, itemIds, travelerIds, serviceIds, segmentIds, ids);

            order.AddComponentTotals(ids);
            order.AddOriginalSaleObligations(change.Id, decisionRef, ids);

            order.CommercialSummary = order._items.Any(item => item.CommercialStatus == OrderItemCommercialStatus.Active)
                ? CommercialSummary.Active
                : CommercialSummary.Inactive;

            order.Causes(new OrderCreatedDomainEvent(
                CanonicalJson.Identifier(ids.NewId()),
                CanonicalJson.Identifier(order.Id),
                args.AcceptedAt,
                order.Id,
                change.Id,
                order.CommercialVersion,
                preparation.SnapshotDigest,
                priceSet.Id,
                order.FinancialSequence,
                order.LastEventOrdinal));

            return order;
        }

        public bool IsSandboxScoped => AcceptedSource.IsSandboxScoped;

        private IReadOnlyDictionary<string, long> AddJourneys(NormalizedCandidate candidate, IIdGenerator ids)
        {
            var journeyIds = new Dictionary<string, long>(StringComparer.Ordinal);

            foreach (var sourceJourney in candidate.Journeys.OrderBy(journey => journey.Sequence))
            {
                var journey = new OrderJourney(ids.NewId(), Id, sourceJourney);
                _journeys.Add(journey);
                journeyIds.Add(journey.SourceBoundRef, journey.Id);
            }

            return journeyIds;
        }

        private IReadOnlyDictionary<string, long> AddSegments(
            NormalizedCandidate candidate,
            IReadOnlyDictionary<string, long> journeyIds,
            IIdGenerator ids)
        {
            var segmentIds = new Dictionary<string, long>(StringComparer.Ordinal);

            for (var index = 0; index < candidate.Segments.Count; index++)
            {
                var source = candidate.Segments[index];
                var segment = new OrderSegment(ids.NewId(), Id, journeyIds[source.JourneyRef], index + 1, source, ids.NewId);
                _segments.Add(segment);
                segmentIds.Add(segment.SourceSegmentRef, segment.Id);
            }

            return segmentIds;
        }

        private (IReadOnlyDictionary<string, long> ItemIds, IReadOnlyDictionary<string, long> ServiceIds) AddItemsAndServices(
            NormalizedCandidate candidate,
            IReadOnlyDictionary<string, long> travelerIds,
            IReadOnlyDictionary<string, long> segmentIds,
            long changeId,
            IIdGenerator ids)
        {
            var itemIds = new Dictionary<string, long>(StringComparer.Ordinal);
            var serviceIds = new Dictionary<string, long>(StringComparer.Ordinal);
            var sourceServices = candidate.Services.ToDictionary(service => service.ServiceRef, StringComparer.Ordinal);

            foreach (var sourceItem in candidate.Items)
            {
                var itemId = ids.NewId();
                var services = new List<OrderService>();

                foreach (var serviceRef in sourceItem.ServiceRefs)
                {
                    var service = new OrderService(ids.NewId(), Id, itemId, sourceServices[serviceRef], travelerIds, segmentIds, changeId, ids.NewId);
                    _services.Add(service);
                    services.Add(service);
                    serviceIds.Add(serviceRef, service.Id);
                    _itemServiceLinks.Add(new OrderItemServiceLink(ids.NewId(), Id, itemId, service, changeId, ids.NewId));
                }

                var item = new OrderItem(itemId, Id, sourceItem, changeId, CommercialTermsSnapshot.Summarize(
                    services.Select(service => service.SoldTerms).ToList(),
                    candidate.Source.Owner,
                    null,
                    null,
                    candidate.CapturedAt));

                _items.Add(item);
                itemIds.Add(item.SourceItemRef, item.Id);
            }

            return (itemIds, serviceIds);
        }

        private void AddPricing(
            NormalizedCandidate candidate,
            long priceChangeSetId,
            IReadOnlyDictionary<string, long> itemIds,
            IReadOnlyDictionary<string, long> serviceIds,
            IReadOnlyDictionary<string, long> segmentIds,
            IIdGenerator ids)
        {
            foreach (var sourceLine in candidate.PricingLines)
            {
                var basisId = sourceLine.BasisType switch
                {
                    PricingBasisType.OrderItem => itemIds[sourceLine.BasisRef],
                    PricingBasisType.OrderService => serviceIds[sourceLine.BasisRef],
                    PricingBasisType.Segment => segmentIds[sourceLine.BasisRef],
                    PricingBasisType.Order => Id,
                    _ => (long?)null
                };

                var itemId = sourceLine.ItemRef is null ? (long?)null : itemIds[sourceLine.ItemRef];
                _pricingLines.Add(new PricingLine(ids.NewId(), Id, priceChangeSetId, itemId, basisId, sourceLine));
            }

            CustomerTotal = PricingArithmetic.CustomerTotal(
                _pricingLines.Select(line => new PricedAmount(line.Effect, line.Direction, line.SaleValue)),
                SaleCurrency.CurrencyRef);

            if (CustomerTotal.Amount != candidate.CustomerTotal.Amount)
                throw ExceptionFactory.PricingRuleViolated("committed lines differ from the accepted customer total");
        }

        private void AddFareConstruction(
            NormalizedCandidate candidate,
            long changeId,
            IReadOnlyDictionary<string, long> itemIds,
            IReadOnlyDictionary<string, long> travelerIds,
            IReadOnlyDictionary<string, long> serviceIds,
            IReadOnlyDictionary<string, long> segmentIds,
            IIdGenerator ids)
            => _fareConstructions.Add(new FareConstruction(
                ids.NewId(),
                Id,
                changeId,
                candidate.FareConstruction,
                itemIds,
                travelerIds,
                serviceIds,
                segmentIds,
                ids.NewId));

        private IReadOnlyDictionary<string, long> AddTravelers(IReadOnlyList<TravelerBinding> bindings, IIdGenerator ids)
        {
            var bySource = new Dictionary<string, long>(StringComparer.Ordinal);
            var byClient = new Dictionary<string, OrderTraveler>(StringComparer.Ordinal);

            foreach (var binding in bindings)
            {
                var traveler = new OrderTraveler(ids.NewId(), Id, binding);
                _travelers.Add(traveler);
                bySource.Add(binding.SourceTravellerRef, traveler.Id);
                byClient.Add(binding.ClientTravelerRef, traveler);
            }

            foreach (var binding in bindings)
                if (binding.GuardianClientTravelerRef is { } guardian)
                    byClient[binding.ClientTravelerRef].LinkGuardian(byClient[guardian].Id);

            return bySource;
        }

        private void AddContacts(IReadOnlyList<ContactDetails> contacts, IIdGenerator ids)
        {
            for (var index = 0; index < contacts.Count; index++)
                _contacts.Add(new OrderContact(ids.NewId(), Id, index + 1, contacts[index]));
        }

        private void AddComponentTotals(IIdGenerator ids)
        {
            var totals = _pricingLines
                .GroupBy(line => (line.Component, line.Effect))
                .OrderBy(group => group.Key.Component)
                .ThenBy(group => group.Key.Effect);

            foreach (var group in totals)
            {
                var debit = group.Where(line => line.Direction == OrderPricingLineDirection.Debit).Sum(line => line.SaleValue.Amount);
                var credit = group.Where(line => line.Direction == OrderPricingLineDirection.Credit).Sum(line => line.SaleValue.Amount);

                _componentTotals.Add(new OrderComponentTotal(
                    ids.NewId(),
                    Id,
                    group.Key.Component,
                    group.Key.Effect,
                    debit,
                    credit,
                    SaleCurrency.CurrencyRef));
            }
        }

        private void AddOriginalSaleObligations(long changeId, string decisionRef, IIdGenerator ids)
        {
            var customerLines = _pricingLines.Where(line => line.Effect == PricingEffect.CustomerBalance).ToList();

            foreach (var group in customerLines.Where(line => line.OrderItemId is not null).GroupBy(line => line.OrderItemId!.Value))
                AddOriginalSaleObligation(FundingObligationScope.ForItem(group.Key), group, changeId, decisionRef, ids);

            foreach (var line in customerLines.Where(line => line.OrderItemId is null))
                AddOriginalSaleObligation(FundingObligationScope.ForPricingLine(line.Id), [line], changeId, decisionRef, ids);
        }

        private void AddOriginalSaleObligation(
            FundingObligationScope scope,
            IEnumerable<PricingLine> lines,
            long changeId,
            string decisionRef,
            IIdGenerator ids)
        {
            var amount = PricingArithmetic.CustomerTotal(
                lines.Select(line => new PricedAmount(line.Effect, line.Direction, line.SaleValue)),
                SaleCurrency.CurrencyRef);

            _fundingObligations.Add(new FundingObligation(
                ids.NewId(), Id, FundingObligationPurpose.OriginalSale, amount, scope, changeId, decisionRef));
        }
    }
}
