using AeroTech.Messages.Aegis.Enums;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Messages.Shared.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Policies;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.Tests._Shared
{
    public sealed class CandidateBuilder
    {
        public const string ReferenceProfile = "REFERENCE-OFFER-2.0";
        public const string SaleCurrency = "EUR";
        public const string SaleCurrencyCode = "EUR";
        public const string ReferenceFulfillmentProfile = "REFERENCE-AIR-ETKT";
        public const string ReferenceFulfillmentProfileVersion = "1";

        private readonly List<CandidateTraveler> _travelers = new();
        private readonly List<CandidateJourney> _journeys = new();
        private readonly List<CandidateSegment> _segments = new();
        private readonly List<CandidateItem> _items = new();
        private readonly List<CandidateService> _services = new();
        private readonly List<CandidatePricingLine> _lines = new();
        private readonly List<CandidatePricingUnit> _units = new();
        private readonly DateTimeOffset _now;
        private AuthorizedSalesScope _scope;
        private string _offerId = "REFERENCE-PRICED-OW-001";
        private AcceptanceAssurance _assurance = AcceptanceAssurance.OwnerBound;
        private string _profile = ReferenceProfile;
        private FareConstructionAssurance _constructionAssurance = FareConstructionAssurance.Opaque;
        private ValidityFact? _offerValidity;
        private ValidityFact? _priceValidity;
        private ValidityFact? _ticketingValidity;
        private ObservedTimeFact? _observedTicketingDeadline;
        private string? _sourceJourneyTypeRaw;
        private JourneyType? _journeyType;
        private decimal? _customerTotalOverride;

        public CandidateBuilder(DateTimeOffset now, AuthorizedSalesScope? scope = null)
        {
            _now = now;
            _scope = scope ?? Scope();
        }

        public static AuthorizedSalesScope Scope(long customerId = 100, string callerScope = "customer:100/actor:1", long ownerAirlineId = 1, SalesChannel channel = SalesChannel.BackOffice)
            => new(ownerAirlineId, customerId, channel, 10, callerScope, BusinessContextType.Airline, 1);

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
            _offerValidity = new ValidityFact(ValidityState.NotSupplied, null, "AirOffer", null, "Not supplied by source");
            _priceValidity = new ValidityFact(ValidityState.NotSupplied, null, "AirPrice", null, "Not supplied by source");
            return this;
        }

        public CandidateBuilder Validity(ValidityFact offer, ValidityFact price, ValidityFact ticketing)
        {
            _offerValidity = offer;
            _priceValidity = price;
            _ticketingValidity = ticketing;
            return this;
        }

        public CandidateBuilder ObservedTicketingDeadline(ObservedTimeFact observed)
        {
            _observedTicketingDeadline = observed;
            return this;
        }

        public CandidateBuilder SourceJourneyType(string? raw, JourneyType? journeyType = null)
        {
            _sourceJourneyTypeRaw = raw;
            _journeyType = journeyType;
            return this;
        }

        public CandidateBuilder Traveler(string reference, PassengerTypeCode passengerType = PassengerTypeCode.ADT)
        {
            _travelers.Add(new CandidateTraveler(reference, passengerType));
            return this;
        }

        public CandidateBuilder Journey(string reference, string? directionRaw = null, BoundDirection? direction = null)
        {
            _journeys.Add(new CandidateJourney(reference, _journeys.Count + 1, directionRaw, direction, "AIRPORT-A", "AIRPORT-B"));
            return this;
        }

        public CandidateBuilder Segment(string reference, params string[] legs)
            => Segment(reference, null, legs);

        public CandidateBuilder Segment(string reference, CandidateSegment? replacement, params string[] legs)
        {
            if (_journeys.Count == 0)
                Journey("BOUND-1");

            var departure = _now.AddDays(14).AddHours(_segments.Count * 6);
            var arrival = departure.AddHours(2);
            var journeyRef = _journeys[^1].JourneyRef;

            var legRefs = legs.Length == 0 ? [$"LEG-{reference}"] : legs;
            var segmentLegs = legRefs
                .Select((leg, index) => new CandidateSegmentLeg(leg, index + 1, "AIRPORT-A", "T1", "AIRPORT-B", "T2", departure, arrival))
                .ToList();

            _segments.Add(replacement ?? new CandidateSegment(
                reference,
                journeyRef,
                SegmentKind.ScheduledAir,
                "AIRPORT-A",
                "T1",
                "AIRPORT-B",
                "T2",
                departure,
                arrival,
                $"FLIGHT-{reference}",
                $"XX{_segments.Count + 100}",
                "1",
                "CARRIER-M",
                "CARRIER-O",
                $"CAPACITY-{reference}",
                120,
                "AIRCRAFT-1",
                segmentLegs));

            return this;
        }

        public CandidateBuilder AirService(
            string reference,
            string traveler,
            string segment,
            IReadOnlyDictionary<string, string>? details = null,
            BaggageAllowance? checkedBaggage = null,
            BaggageAllowance? cabinBaggage = null,
            SoldTermFlags? soldTerms = null)
        {
            _services.Add(new CandidateService(
                reference,
                OrderServiceType.AirTransportation,
                null,
                null,
                ServicePriceTreatment.SupplierOpaque,
                null,
                null,
                [traveler],
                [segment],
                1,
                OrderItemUnitOfMeasure.PassengerSegment,
                ServiceDetailSchemaRegistry.AirTransportSchema,
                ServiceDetailSchemaRegistry.AirTransportSchemaVersion,
                details ?? new Dictionary<string, string> { [ServiceDetailSchemaRegistry.CabinRef] = "ECONOMY", [ServiceDetailSchemaRegistry.BookingClass] = "Y" },
                checkedBaggage,
                cabinBaggage,
                soldTerms ?? new SoldTermFlags(null, null, null),
                CertifiedProfile()));
            return this;
        }

        public static CandidateFulfillmentProfile CertifiedProfile()
            => new(
                ReferenceFulfillmentProfile,
                ReferenceFulfillmentProfileVersion,
                FulfillmentProfileAssurance.Certified,
                ReservationRequirement.FlightCapacity,
                FulfillmentDocumentKind.Etkt,
                FundingRequirement.Required,
                1);

        public static CandidateFulfillmentProfile UncertifiedProfile(string profileRef, string version = "1")
            => new(
                profileRef,
                version,
                FulfillmentProfileAssurance.NotCertified,
                ReservationRequirement.Unresolved,
                FulfillmentDocumentKind.Unresolved,
                FundingRequirement.Unresolved,
                null);

        public CandidateBuilder Service(CandidateService service)
        {
            _services.Add(service);
            return this;
        }

        public CandidateBuilder Package(string reference, params string[] services)
        {
            _items.Add(new CandidateItem(
                reference,
                OrderItemKind.OfferPackage,
                null,
                services,
                new Money(0, SaleCurrency),
                new ProductSnapshot("AirOffer", _offerId, null, null, null, null, null, null)));
            return this;
        }

        public CandidateBuilder Line(
            string reference,
            string item,
            PricingComponentType component,
            decimal amount,
            string basisRef,
            PricingEffect effect = PricingEffect.CustomerBalance,
            OrderPricingLineDirection? direction = null,
            Money? original = null,
            PricingBasisType basisType = PricingBasisType.OrderService,
            string? sourceCode = null,
            string? sourceName = null,
            string? sourceReference = null,
            PricingCalculationKind calculationKind = PricingCalculationKind.Amount,
            AppliedConversion? appliedConversion = null)
        {
            var originalValue = original ?? new Money(amount, SaleCurrency);
            var conversionRef = appliedConversion?.SourceConversionRef
                ?? (originalValue.CurrencyRef == SaleCurrency ? null : $"ROE-{reference}");

            _lines.Add(new CandidatePricingLine(
                reference,
                item,
                component,
                effect,
                direction ?? (component == PricingComponentType.Discount ? OrderPricingLineDirection.Credit : OrderPricingLineDirection.Debit),
                PricingLineRole.Original,
                sourceCode,
                sourceName,
                sourceReference,
                calculationKind,
                originalValue,
                new Money(amount, SaleCurrency),
                $"source/{reference}",
                basisType,
                basisRef,
                conversionRef,
                appliedConversion));
            return this;
        }

        public CandidateBuilder SourceProvidedConstruction(params CandidatePricingUnit[] units)
        {
            _constructionAssurance = FareConstructionAssurance.SourceProvided;
            _units.AddRange(units);
            return this;
        }

        public CandidateBuilder OpaqueUnits(params CandidatePricingUnit[] units)
        {
            _constructionAssurance = FareConstructionAssurance.Opaque;
            _units.AddRange(units);
            return this;
        }

        public CandidateBuilder CustomerTotal(decimal amount)
        {
            _customerTotalOverride = amount;
            return this;
        }

        public static CandidatePricingUnit Unit(
            string sourceUnitRef,
            CandidatePricingGroup? group,
            params CandidateFareComponent[] components)
            => new(sourceUnitRef, null, FarePricingUnitType.Unspecified, FareCombinationMethod.Unspecified, [], group, components);

        public static CandidateFareComponent Component(
            string sourceFareRef,
            IReadOnlyList<string>? coveredServiceRefs = null,
            IReadOnlyList<string>? coveredSegmentRefs = null,
            string? fareBasis = null)
            => new(sourceFareRef, fareBasis, null, null, null, null, null, null, null, null, null, null,
                coveredServiceRefs ?? [], coveredSegmentRefs ?? []);

        public NormalizedCandidate Build()
        {
            var items = _items.Select(item => item with
            {
                AcceptedTotal = new Money(
                    _lines.Where(line => line.ItemRef == item.ItemRef && line.Effect == PricingEffect.CustomerBalance)
                        .Sum(line => (line.Direction == OrderPricingLineDirection.Debit ? 1 : -1) * line.SaleValue.Amount),
                    SaleCurrency)
            }).ToList();

            var total = _customerTotalOverride ?? _lines
                .Where(line => line.Effect == PricingEffect.CustomerBalance)
                .Sum(line => (line.Direction == OrderPricingLineDirection.Debit ? 1 : -1) * line.SaleValue.Amount);

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
                new CandidateValidity(
                    _offerValidity ?? new ValidityFact(ValidityState.Known, _now.AddMinutes(10), "AirOffer", $"OFFER-{_offerId}", null),
                    _priceValidity ?? new ValidityFact(ValidityState.Known, _now.AddMinutes(5), "AirPrice", $"PRICE-{_offerId}", null),
                    _ticketingValidity ?? new ValidityFact(ValidityState.NotSupplied, null, "Unresolved owner", null, "Not supplied by source; no implied infinite validity"),
                    _observedTicketingDeadline),
                new CandidateSalesContext(_scope.OwnerAirlineId, _scope.FinancialCustomerId, _scope.Channel, _scope.SellingOfficeId),
                _travelers.ToList(),
                _journeys.ToList(),
                _segments.ToList(),
                items,
                _services.ToList(),
                _lines.ToList(),
                new Money(total, SaleCurrency),
                SaleCurrencyCode,
                _sourceJourneyTypeRaw,
                _journeyType,
                new CandidateFareConstruction(_constructionAssurance, $"CONTEXT-{_offerId}", _units.ToList()));
        }

        public static CandidateBuilder OneWayFare100Tax20(DateTimeOffset now, AuthorizedSalesScope? scope = null)
            => new CandidateBuilder(now, scope)
                .Traveler("PAX-A")
                .Segment("SEG-A")
                .AirService("SERVICE-A", "PAX-A", "SEG-A")
                .Package("ITEM-A", "SERVICE-A")
                .Line("PRICE-FARE", "ITEM-A", PricingComponentType.Fare, 100.00m, "SERVICE-A")
                .Line("PRICE-TAX", "ITEM-A", PricingComponentType.Tax, 20.00m, "SERVICE-A");
    }
}
