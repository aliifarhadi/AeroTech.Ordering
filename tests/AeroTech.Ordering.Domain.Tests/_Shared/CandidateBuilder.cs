using AeroTech.Messages.Aegis.Enums;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Messages.Shared.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.Tests._Shared
{
    public sealed class CandidateBuilder
    {
        public const string ReferenceProfile = "REFERENCE-OFFER-2.0";
        public const int SaleCurrencyId = 978;

        private readonly DateTimeOffset _now;
        private readonly List<CandidateTraveller> _travellers = new();
        private readonly List<CandidateJourney> _journeys = new();
        private readonly List<CandidateSegment> _segments = new();
        private readonly List<CandidateService> _services = new();
        private readonly List<CandidateItem> _items = new();
        private readonly List<CandidatePricingLine> _lines = new();
        private readonly List<CandidatePricingUnit> _units = new();
        private readonly List<string> _constructionItemKeys = new();

        private AuthorizedSalesScope _scope;
        private string _offerId = "OFFER-1";
        private string _profile = ReferenceProfile;
        private AcceptanceAssurance _assurance = AcceptanceAssurance.OwnerBound;
        private JourneyType _journeyType = JourneyType.OneWay;
        private DateTimeOffset? _offerExpiresAt;
        private DateTimeOffset? _priceValidUntil;
        private DateTimeOffset? _lastTicketingDate;
        private decimal? _customerTotalOverride;
        private string? _saleCurrencyCode;

        public CandidateBuilder(DateTimeOffset now, AuthorizedSalesScope? scope = null)
        {
            _now = now;
            _scope = scope ?? Scope();
        }

        public static AuthorizedSalesScope Scope(
            long customerId = 100,
            string callerScope = "customer:100/actor:1",
            long ownerAirlineId = 1,
            SalesChannel channel = SalesChannel.BackOffice,
            SalesContextSnapshot? salesContext = null,
            BuyerSnapshot? buyer = null)
            => new(
                ownerAirlineId,
                customerId,
                salesContext ?? new SalesContextSnapshot(
                    channel,
                    BusinessContextType.Airline,
                    ownerAirlineId,
                    SellingOfficeKind.AirlineOffice,
                    10),
                buyer ?? BuyerSnapshot.NotSupplied,
                callerScope,
                new InitiatingActorSnapshot(BusinessContextType.Airline, 1));

        public AuthorizedSalesScope SalesScope => _scope;

        public CandidateBuilder ForScope(AuthorizedSalesScope scope)
        {
            _scope = scope;
            return this;
        }

        public CandidateBuilder Offer(string offerId)
        {
            _offerId = offerId;
            return this;
        }

        public CandidateBuilder LocalCandidateOnly(string profile)
        {
            _assurance = AcceptanceAssurance.LocalCandidateOnly;
            _profile = profile;
            return this;
        }

        public CandidateBuilder Validity(DateTimeOffset? offerExpiresAt, DateTimeOffset? priceValidUntil, DateTimeOffset? lastTicketingDate)
        {
            _offerExpiresAt = offerExpiresAt;
            _priceValidUntil = priceValidUntil;
            _lastTicketingDate = lastTicketingDate;
            return this;
        }

        public CandidateBuilder JourneyKind(JourneyType journeyType)
        {
            _journeyType = journeyType;
            return this;
        }

        public CandidateBuilder Traveller(string reference, PassengerTypeCode passengerType = PassengerTypeCode.ADT)
        {
            _travellers.Add(new CandidateTraveller(reference, passengerType));
            return this;
        }

        public CandidateBuilder Journey(string boundId, BoundDirection direction = BoundDirection.Outbound)
        {
            _journeys.Add(new CandidateJourney(boundId, _journeys.Count + 1, direction, 1001, 1002));
            return this;
        }

        public CandidateBuilder Segment(string segmentKey, params long[] legs)
            => Segment(segmentKey, null, legs);

        public CandidateBuilder Segment(string segmentKey, CandidateSegment? replacement, params long[] legs)
        {
            if (_journeys.Count == 0)
                Journey("BOUND-1");

            var boundId = _journeys[^1].BoundId;
            var sequence = _segments.Count(segment => segment.BoundId == boundId) + 1;
            var legIds = legs.Length == 0 ? [9000L + _segments.Count] : legs;

            _segments.Add(replacement ?? new CandidateSegment(
                segmentKey,
                boundId,
                sequence,
                SegmentKind.ScheduledAir,
                1001,
                null,
                1002,
                null,
                _now.AddDays(10),
                _now.AddDays(10).AddHours(2),
                7000 + _segments.Count,
                "XX100",
                1,
                21,
                21,
                8000 + _segments.Count,
                120,
                1,
                legIds.Select((legId, index) => new CandidateSegmentLeg(
                    legId,
                    index + 1,
                    1001,
                    null,
                    1002,
                    null,
                    _now.AddDays(10),
                    _now.AddDays(10).AddHours(2))).ToList()));

            return this;
        }

        public CandidateBuilder AirService(
            string serviceKey,
            string travellerRef,
            string segmentKey,
            int? cabinClassId = 1,
            long? rbdId = 25,
            string? bookingClass = "Y",
            BaggageAllowance? checkedBaggage = null,
            BaggageAllowance? cabinBaggage = null,
            SoldTermFlags? soldTerms = null,
            CandidateFulfillmentProfile? fulfillmentProfile = null)
        {
            _services.Add(new CandidateService(
                serviceKey,
                travellerRef,
                segmentKey,
                cabinClassId,
                rbdId,
                bookingClass,
                checkedBaggage,
                cabinBaggage,
                soldTerms ?? new SoldTermFlags(null, null, null),
                fulfillmentProfile ?? CandidateFulfillmentProfile.Unresolved));
            return this;
        }

        public CandidateBuilder Service(CandidateService service)
        {
            _services.Add(service);
            return this;
        }

        public CandidateBuilder Package(string itemKey, params string[] services)
        {
            _items.Add(new CandidateItem(itemKey, OrderItemKind.OfferPackage, services.ToList(), new Money(0m, SaleCurrencyId)));
            return this;
        }

        public CandidateBuilder Item(CandidateItem item)
        {
            _items.Add(item);
            return this;
        }

        public CandidateBuilder Line(
            string occurrencePath,
            string item,
            PricingComponentType component,
            decimal amount,
            string basisKey,
            PricingEffect effect = PricingEffect.CustomerBalance,
            OrderPricingLineDirection? direction = null,
            Money? original = null,
            PricingBasisType basisType = PricingBasisType.OrderService,
            string? code = null,
            string? name = null,
            string? reference = null,
            PricingCalculationKind calculationKind = PricingCalculationKind.Amount,
            AppliedConversion? appliedConversion = null,
            SettlementAttribution? settlementAttribution = null,
            PricingLineRole role = PricingLineRole.Original)
        {
            var originalValue = original ?? new Money(amount, SaleCurrencyId);
            var conversionRef = appliedConversion?.SourceConversionRef
                ?? (originalValue.CurrencyId == SaleCurrencyId ? null : $"ROE-{occurrencePath}");

            _lines.Add(new CandidatePricingLine(
                occurrencePath,
                item,
                component,
                effect,
                direction ?? (component == PricingComponentType.Discount ? OrderPricingLineDirection.Credit : OrderPricingLineDirection.Debit),
                role,
                code,
                name,
                reference,
                calculationKind,
                originalValue,
                new Money(amount, SaleCurrencyId),
                basisType,
                basisKey,
                conversionRef,
                appliedConversion,
                settlementAttribution));
            return this;
        }

        public CandidateBuilder Units(params CandidatePricingUnit[] units)
        {
            _units.AddRange(units);
            return this;
        }

        public CandidateBuilder CustomerTotal(decimal amount)
        {
            _customerTotalOverride = amount;
            return this;
        }

        public CandidateBuilder SaleCurrencyCode(string? code)
        {
            _saleCurrencyCode = code;
            return this;
        }

        public CandidateBuilder ConstructionFor(params string[] itemKeys)
        {
            _constructionItemKeys.AddRange(itemKeys);
            return this;
        }

        public CandidateBuilder OneWayConstruction(params string[] coveredBoundOfferIds)
            => Units(Unit(1, FarePricingUnitType.OneWay, coveredBoundOfferIds, Component(4242)));

        public static CandidatePricingUnit Unit(
            int sequence,
            FarePricingUnitType type,
            IReadOnlyList<string> coveredBoundOfferIds,
            params CandidateFareComponent[] components)
            => Unit(sequence, type, SourceTypeOf(type), coveredBoundOfferIds, components);

        public static CandidatePricingUnit Unit(
            int sequence,
            FarePricingUnitType type,
            AirFareConstructionType sourceConstructionType,
            IReadOnlyList<string> coveredBoundOfferIds,
            params CandidateFareComponent[] components)
            => new(sequence, type, sourceConstructionType, coveredBoundOfferIds, components.ToList());

        private static AirFareConstructionType SourceTypeOf(FarePricingUnitType type) => type switch
        {
            FarePricingUnitType.OneWay => AirFareConstructionType.OneWay,
            FarePricingUnitType.RoundTrip => AirFareConstructionType.RoundTrip,
            _ => AirFareConstructionType.Unspecified
        };

        public static CandidateFareComponent Component(
            long airFareId,
            string? fareBasis = "YOW",
            int? cabinClassId = 1,
            long? rbdId = 25,
            string? bookingClass = "Y")
            => new(airFareId, fareBasis, "FLEX", "Published", cabinClassId, rbdId, bookingClass, null);

        public NormalizedCandidate Build()
        {
            var items = _items.Select(item => item with
            {
                AcceptedTotal = new Money(
                    _lines.Where(line => line.ItemKey == item.ItemKey && line.Effect == PricingEffect.CustomerBalance)
                        .Sum(line => (line.Direction == OrderPricingLineDirection.Debit ? 1 : -1) * line.SaleValue.Amount),
                    SaleCurrencyId)
            }).ToList();

            var total = _customerTotalOverride ?? _lines
                .Where(line => line.Effect == PricingEffect.CustomerBalance)
                .Sum(line => (line.Direction == OrderPricingLineDirection.Debit ? 1 : -1) * line.SaleValue.Amount);

            var construction = _units.Count == 0
                ? null
                : new CandidateFareConstruction(
                    FareConstructionAssurance.SourceProvided,
                    _constructionItemKeys.Count > 0 ? _constructionItemKeys : _items.Select(item => item.ItemKey).ToList(),
                    _units);

            return new NormalizedCandidate(
                NormalizedCandidate.CurrentSchemaVersion,
                new CandidateSource(
                    "AirOffer",
                    _offerId,
                    _profile,
                    _assurance == AcceptanceAssurance.OwnerBound ? $"BINDING-{_offerId}" : null,
                    new string('a', 64)),
                _assurance,
                _now,
                _now,
                _offerExpiresAt ?? _now.AddHours(2),
                _priceValidUntil ?? _now.AddHours(1),
                _lastTicketingDate,
                new CandidateSalesContext(_scope.OwnerAirlineId, _scope.FinancialCustomerId, _scope.SalesContext, _scope.Buyer),
                _journeyType,
                _travellers,
                _journeys,
                _segments,
                items,
                _services,
                _lines,
                new Money(total, SaleCurrencyId),
                _saleCurrencyCode,
                construction);
        }

        public static CandidateBuilder OneWayFare100Tax20(DateTimeOffset now, AuthorizedSalesScope? scope = null)
            => new CandidateBuilder(now, scope)
                .Traveller("PAX-A")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .Package("ITEM-A", "S-A")
                .Line("fare", "ITEM-A", PricingComponentType.Fare, 100m, "S-A")
                .Line("tax", "ITEM-A", PricingComponentType.Tax, 20m, "S-A");
    }
}
