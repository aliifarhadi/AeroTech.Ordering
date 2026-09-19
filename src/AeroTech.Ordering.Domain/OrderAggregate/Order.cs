using AeroTech.Framework.Core.Domain.Aggregates;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Messages.Shared.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Domain._Shared.Serialization;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderAggregate.Arguments;
using AeroTech.Ordering.Domain.OrderAggregate.DomainEvents;
using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Domain.OrderAggregate.Policies;
using AeroTech.Ordering.Domain.OrderAggregate.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate
{
    public sealed class Order : AggregateRoot<long>
    {
        private readonly List<OrderTraveller> _travellers = new();
        private readonly List<OrderContact> _contacts = new();
        private readonly List<OrderJourney> _journeys = new();
        private readonly List<OrderSegment> _segments = new();
        private readonly List<OrderItem> _items = new();
        private readonly List<OrderService> _services = new();
        private readonly List<OrderChange> _changes = new();
        private readonly List<PriceChangeSet> _priceChangeSets = new();
        private readonly List<PricingLine> _pricingLines = new();
        private readonly List<FareConstruction> _fareConstructions = new();
        private readonly List<FundingObligation> _fundingObligations = new();

        private Order()
        {
        }

        public string OrderReference { get; private set; } = null!;

        public long RootOrderId { get; private set; }

        public long OwnerAirlineId { get; private set; }

        public long FinancialCustomerId { get; private set; }

        public SalesContextSnapshot SalesContext { get; private set; } = null!;

        public InitiatingActorSnapshot InitiatingActor { get; private set; } = null!;

        public int CurrencyId { get; private set; }

        public string SourceOfferId { get; private set; } = null!;

        public long SourcePreparationId { get; private set; }

        public string AcceptedSnapshotDigest { get; private set; } = null!;

        public JourneyType JourneyType { get; private set; }

        public DateTimeOffset? LastTicketingDate { get; private set; }

        public CommercialSummary CommercialSummary { get; private set; }

        public Money CustomerTotal { get; private set; } = null!;

        public int CommercialVersion { get; private set; }

        public int FinancialSequence { get; private set; }

        public long OrderRevision { get; private set; }

        public long LastEventOrdinal { get; private set; }

        public string? ClientReference { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public SalesChannel Channel => SalesContext.Channel;

        public IReadOnlyCollection<OrderTraveller> Travellers => _travellers.AsReadOnly();

        public IReadOnlyCollection<OrderContact> Contacts => _contacts.AsReadOnly();

        public IReadOnlyCollection<OrderJourney> Journeys => _journeys.AsReadOnly();

        public IReadOnlyCollection<OrderSegment> Segments => _segments.AsReadOnly();

        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

        public IReadOnlyCollection<OrderService> Services => _services.AsReadOnly();

        public IReadOnlyCollection<OrderChange> Changes => _changes.AsReadOnly();

        public IReadOnlyCollection<PriceChangeSet> PriceChangeSets => _priceChangeSets.AsReadOnly();

        public IReadOnlyCollection<PricingLine> PricingLines => _pricingLines.AsReadOnly();

        public IReadOnlyCollection<FareConstruction> FareConstructions => _fareConstructions.AsReadOnly();

        public IReadOnlyCollection<FundingObligation> FundingObligations => _fundingObligations.AsReadOnly();

        public static Order AcceptOriginalSale(AcceptOriginalSaleArgs args, IIdGenerator ids)
        {
            var preparation = args.Preparation;
            var candidate = preparation.Candidate;

            if (string.IsNullOrWhiteSpace(args.OrderReference))
                throw ExceptionFactory.CandidateContractMismatch("an order reference is required");

            if (args.AcceptingScope.OwnerAirlineId != preparation.OwnerAirlineId
                || args.AcceptingScope.FinancialCustomerId != preparation.FinancialCustomerId)
                throw ExceptionFactory.PreparationNotFound(preparation.Id);

            TravellerBindingPolicy.EnsureComplete(candidate.Travellers, args.TravellerBindings, args.AcceptedAt);
            TravellerBindingPolicy.EnsureContacts(args.Contacts);

            var order = new Order
            {
                Id = args.OrderId,
                OrderReference = args.OrderReference,
                RootOrderId = args.OrderId,
                OwnerAirlineId = preparation.OwnerAirlineId,
                FinancialCustomerId = preparation.FinancialCustomerId,
                SalesContext = candidate.SalesContext.Sales,
                InitiatingActor = args.AcceptingScope.InitiatingActor,
                CurrencyId = candidate.CurrencyId,
                SourceOfferId = preparation.SourceOfferId,
                SourcePreparationId = preparation.Id,
                AcceptedSnapshotDigest = preparation.SnapshotDigest,
                JourneyType = candidate.JourneyType,
                LastTicketingDate = candidate.LastTicketingDate,
                ClientReference = args.ClientReference,
                CreatedAt = args.AcceptedAt,
                CommercialVersion = 1,
                FinancialSequence = 1,
                OrderRevision = 1,
                LastEventOrdinal = 1
            };

            var change = new OrderChange(ids.NewId(), order.Id, OrderChangeType.Create, order.CommercialVersion,
                args.AcceptingScope.ActorContextType, args.AcceptingScope.ActorId, args.AcceptedAt);
            order._changes.Add(change);

            var priceSet = new PriceChangeSet(ids.NewId(), order.Id, change.Id, order.FinancialSequence,
                PriceChangeReason.OriginalSale, args.AcceptedAt);
            order._priceChangeSets.Add(priceSet);

            var travellerIds = order.AddTravellers(args.TravellerBindings, ids);
            order.AddContacts(args.Contacts, ids);

            var journeyIds = order.AddJourneys(candidate, ids);
            var segmentIds = order.AddSegments(candidate, journeyIds, ids);
            var (itemIds, serviceIds) = order.AddItemsAndServices(candidate, travellerIds, segmentIds, change.Id, ids);

            order.AddPricing(candidate, priceSet.Id, itemIds, serviceIds, segmentIds, ids);
            order.AddFareConstruction(candidate, change.Id, itemIds, ids);
            order.AddOriginalSaleObligations(change.Id, ids);

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

        private IReadOnlyDictionary<string, long> AddJourneys(NormalizedCandidate candidate, IIdGenerator ids)
        {
            var journeyIds = new Dictionary<string, long>(StringComparer.Ordinal);

            foreach (var sourceJourney in candidate.Journeys.OrderBy(journey => journey.Sequence))
            {
                var journey = new OrderJourney(ids.NewId(), Id, sourceJourney);
                _journeys.Add(journey);
                journeyIds.Add(journey.BoundId, journey.Id);
            }

            return journeyIds;
        }

        private IReadOnlyDictionary<string, long> AddSegments(
            NormalizedCandidate candidate,
            IReadOnlyDictionary<string, long> journeyIds,
            IIdGenerator ids)
        {
            var segmentIds = new Dictionary<string, long>(StringComparer.Ordinal);

            foreach (var source in candidate.Segments)
            {
                var segment = new OrderSegment(ids.NewId(), Id, journeyIds[source.BoundId], source, ids.NewId);
                _segments.Add(segment);
                segmentIds.Add(source.SegmentKey, segment.Id);
            }

            return segmentIds;
        }

        private (IReadOnlyDictionary<string, long> ItemIds, IReadOnlyDictionary<string, long> ServiceIds) AddItemsAndServices(
            NormalizedCandidate candidate,
            IReadOnlyDictionary<string, long> travellerIds,
            IReadOnlyDictionary<string, long> segmentIds,
            long changeId,
            IIdGenerator ids)
        {
            var itemIds = new Dictionary<string, long>(StringComparer.Ordinal);
            var serviceIds = new Dictionary<string, long>(StringComparer.Ordinal);
            var sourceServices = candidate.Services.ToDictionary(service => service.ServiceKey, StringComparer.Ordinal);

            foreach (var sourceItem in candidate.Items)
            {
                var itemId = ids.NewId();

                foreach (var serviceKey in sourceItem.ServiceKeys)
                {
                    var source = sourceServices[serviceKey];
                    var service = new OrderService(
                        ids.NewId(),
                        Id,
                        itemId,
                        travellerIds[source.TravellerRef],
                        segmentIds[source.SegmentKey],
                        source,
                        changeId);

                    _services.Add(service);
                    serviceIds.Add(serviceKey, service.Id);
                }

                var item = new OrderItem(itemId, Id, sourceItem, changeId);
                _items.Add(item);
                itemIds.Add(sourceItem.ItemKey, item.Id);
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
                    PricingBasisType.OrderItem => itemIds[sourceLine.BasisKey],
                    PricingBasisType.OrderService => serviceIds[sourceLine.BasisKey],
                    PricingBasisType.Segment => segmentIds[sourceLine.BasisKey],
                    PricingBasisType.Order => Id,
                    _ => (long?)null
                };

                var itemId = sourceLine.ItemKey is null ? (long?)null : itemIds[sourceLine.ItemKey];
                _pricingLines.Add(new PricingLine(ids.NewId(), Id, priceChangeSetId, itemId, basisId, sourceLine));
            }

            CustomerTotal = PricingArithmetic.CustomerTotal(
                _pricingLines.Select(line => new PricedAmount(line.Effect, line.Direction, line.SaleValue)),
                CurrencyId);

            if (CustomerTotal.Amount != candidate.CustomerTotal.Amount)
                throw ExceptionFactory.PricingRuleViolated("committed lines differ from the accepted customer total");
        }

        private void AddFareConstruction(
            NormalizedCandidate candidate,
            long changeId,
            IReadOnlyDictionary<string, long> itemIds,
            IIdGenerator ids)
            => _fareConstructions.Add(new FareConstruction(ids.NewId(), Id, changeId, candidate.FareConstruction, itemIds, ids.NewId));

        private IReadOnlyDictionary<string, long> AddTravellers(IReadOnlyList<TravellerBinding> bindings, IIdGenerator ids)
        {
            var bySource = new Dictionary<string, long>(StringComparer.Ordinal);
            var byClient = new Dictionary<string, OrderTraveller>(StringComparer.Ordinal);

            foreach (var binding in bindings)
            {
                var traveller = new OrderTraveller(ids.NewId(), Id, binding);
                _travellers.Add(traveller);
                bySource.Add(binding.SourceTravellerRef, traveller.Id);
                byClient.Add(binding.ClientTravellerRef, traveller);
            }

            foreach (var binding in bindings)
                if (binding.GuardianClientTravellerRef is { } guardian)
                    byClient[binding.ClientTravellerRef].LinkGuardian(byClient[guardian].Id);

            return bySource;
        }

        private void AddContacts(IReadOnlyList<ContactDetails> contacts, IIdGenerator ids)
        {
            for (var index = 0; index < contacts.Count; index++)
                _contacts.Add(new OrderContact(ids.NewId(), Id, index + 1, contacts[index]));
        }

        private void AddOriginalSaleObligations(long changeId, IIdGenerator ids)
        {
            var customerLines = _pricingLines
                .Where(line => line.Effect == PricingEffect.CustomerBalance && line.OrderItemId is not null)
                .GroupBy(line => line.OrderItemId!.Value);

            foreach (var group in customerLines)
            {
                var amount = PricingArithmetic.CustomerTotal(
                    group.Select(line => new PricedAmount(line.Effect, line.Direction, line.SaleValue)),
                    CurrencyId);

                _fundingObligations.Add(new FundingObligation(
                    ids.NewId(), Id, FundingObligationPurpose.OriginalSale, amount, group.Key, changeId));
            }
        }
    }
}
