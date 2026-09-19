using System.Globalization;
using System.Text.Json;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Policies;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;
using AeroTech.Ordering.Providers.AirOffer.Wire;

namespace AeroTech.Ordering.Providers.AirOffer.Services
{
    public static class AirOfferCandidateMapper
    {
        public const string PackageItemRef = "OFFER-PACKAGE";
        public const string FulfillmentProfileRef = "AIROFFER-OBSERVED-AIR-UNCERTIFIED";
        public const string FulfillmentProfileVersion = "1";
        public const string SourceContextRef = "airoffer:details:pricingUnits";
        public const string TicketingDeadlineSourceRef = "details.lastTicketingDate";

        private static readonly IReadOnlyDictionary<string, PricingComponentType> Categories = new Dictionary<string, PricingComponentType>(StringComparer.OrdinalIgnoreCase)
        {
            ["Fare"] = PricingComponentType.Fare,
            ["Tax"] = PricingComponentType.Tax,
            ["Fee"] = PricingComponentType.Fee,
            ["Surcharge"] = PricingComponentType.CarrierSurcharge
        };

        private static readonly IReadOnlyDictionary<int, PricingComponentType> CategoryValues = new Dictionary<int, PricingComponentType>
        {
            [0] = PricingComponentType.Fare,
            [1] = PricingComponentType.Tax,
            [2] = PricingComponentType.Fee,
            [3] = PricingComponentType.CarrierSurcharge
        };

        public static NormalizedCandidate Map(
            string requestedOfferId,
            AirOfferDetailsWire details,
            AuthorizedSalesScope scope,
            string sourcePayloadHash,
            DateTimeOffset capturedAt)
        {
            EnsureRespondedOffer(requestedOfferId, details.OfferId);

            var saleCurrency = Currency(details.CurrencyId);
            var mismatches = new List<string>();

            if (details.Tickets.Count == 0)
                throw new AirOfferContractMismatchException("details contain no priced traveler tickets");

            if (details.Tickets.Any(ticket => PassengerType(ticket.PassengerTypeCode) == PassengerTypeCode.INF))
                throw new AirOfferUnsupportedException("infant seat/resource requirement is unresolved (BD-002, OD-S1-07)");

            var journeys = new List<CandidateJourney>();
            var segments = new List<CandidateSegment>();

            foreach (var bound in details.AirTransports.OrderBy(bound => bound.Sequence))
            {
                journeys.Add(new CandidateJourney(
                    bound.BoundId,
                    bound.Sequence,
                    RawValue(bound.Direction),
                    null,
                    Id(bound.OriginAirportId),
                    Id(bound.DestinationAirportId)));

                foreach (var flight in bound.Flights.OrderBy(flight => flight.Sequence))
                    segments.Add(Segment(bound.BoundId, flight));
            }

            var flights = details.AirTransports
                .SelectMany(bound => bound.Flights.Select(flight => (Bound: bound.BoundId, Flight: flight)))
                .ToDictionary(entry => SegmentRef(entry.Bound, entry.Flight.FlightId), entry => entry.Flight, StringComparer.Ordinal);

            var rates = RateIndex(details.RatesOfExchange);
            var services = new List<CandidateService>();
            var lines = new List<CandidatePricingLine>();
            var rootTotal = 0m;

            for (var ticketIndex = 0; ticketIndex < details.Tickets.Count; ticketIndex++)
            {
                var ticket = details.Tickets[ticketIndex];
                var ticketTotal = 0m;

                Reconcile($"tickets/{ticketIndex}", ticket.BaseAmount, ticket.ChargeAmount, ticket.TotalAmount, mismatches);

                for (var couponIndex = 0; couponIndex < ticket.Coupons.Count; couponIndex++)
                {
                    var coupon = ticket.Coupons[couponIndex];
                    var path = $"tickets/{ticketIndex}/coupons/{couponIndex}";
                    var segmentRef = SegmentRef(coupon.BoundId, coupon.FlightId);

                    if (!flights.TryGetValue(segmentRef, out var flight))
                        throw new AirOfferContractMismatchException($"{path} references bound {coupon.BoundId} flight {coupon.FlightId}, which is not in airTransports");

                    var serviceRef = $"{ticket.TravellerRef}|{segmentRef}";
                    services.Add(AirService(serviceRef, ticket.TravellerRef, segmentRef, flight, coupon, path));

                    Reconcile(path, coupon.BaseAmount, coupon.ChargeAmount, coupon.TotalAmount, mismatches);
                    var couponTotal = 0m;

                    for (var pricingIndex = 0; pricingIndex < coupon.Pricings.Count; pricingIndex++)
                    {
                        var line = Line($"{path}/pricings/{pricingIndex}", coupon.Pricings[pricingIndex], details.CurrencyId, rates, PricingBasisType.OrderService, serviceRef);
                        lines.Add(line);
                        couponTotal += line.SaleValue.Amount;
                    }

                    if (couponTotal != coupon.TotalAmount)
                        mismatches.Add($"{path} total {coupon.TotalAmount} differs from its pricing rows {couponTotal}");

                    ticketTotal += coupon.TotalAmount;
                }

                if (ticketTotal != ticket.TotalAmount)
                    mismatches.Add($"tickets/{ticketIndex} total {ticket.TotalAmount} differs from its coupons {ticketTotal}");

                rootTotal += ticket.TotalAmount;
            }

            for (var chargeIndex = 0; chargeIndex < details.OrderCharges.Count; chargeIndex++)
            {
                var line = Line($"orderCharges/{chargeIndex}", details.OrderCharges[chargeIndex], details.CurrencyId, rates, PricingBasisType.OrderItem, PackageItemRef);
                lines.Add(line);
                rootTotal += line.SaleValue.Amount;
            }

            Reconcile("offer", details.BaseAmount, details.ChargeAmount, details.TotalAmount, mismatches);

            if (rootTotal != details.TotalAmount)
                mismatches.Add($"offer total {details.TotalAmount} differs from tickets and order charges {rootTotal}");

            if (mismatches.Count > 0)
                throw new AirOfferContractMismatchException(string.Join("; ", mismatches));

            var package = new CandidateItem(
                PackageItemRef,
                OrderItemKind.OfferPackage,
                null,
                services.Select(service => service.ServiceRef).ToList(),
                new Money(details.TotalAmount, saleCurrency),
                new ProductSnapshot(AirOfferProfile.Owner, requestedOfferId, null, null, null, null, null, null));

            return new NormalizedCandidate(
                NormalizedCandidate.CurrentSchemaVersion,
                new CandidateSource(AirOfferProfile.Owner, requestedOfferId, AirOfferProfile.LiveCandidateSandbox, null, sourcePayloadHash),
                AcceptanceAssurance.LocalCandidateOnly,
                details.PricedAt,
                capturedAt,
                new CandidateValidity(
                    new ValidityFact(ValidityState.NotSupplied, null, AirOfferProfile.Owner, null, "Details response supplies no OfferExpiresAt (BD-001)"),
                    new ValidityFact(ValidityState.NotSupplied, null, "AirPrice", null, "Details response supplies no PriceValidUntil (BD-001)"),
                    new ValidityFact(ValidityState.NotSupplied, null, "Unresolved owner", null, "Ticketing deadline ownership is unresolved (BD-004)"),
                    details.LastTicketingDate is { } lastTicketingDate
                        ? new ObservedTimeFact(lastTicketingDate, AirOfferProfile.Owner, TicketingDeadlineSourceRef)
                        : null),
                new CandidateSalesContext(scope.OwnerAirlineId, scope.FinancialCustomerId, scope.SalesContext, scope.Buyer),
                details.Tickets.Select(ticket => new CandidateTraveler(ticket.TravellerRef, PassengerType(ticket.PassengerTypeCode))).ToList(),
                journeys,
                segments,
                [package],
                services,
                lines.Select(line => line with { ItemRef = PackageItemRef }).ToList(),
                new Money(details.TotalAmount, saleCurrency),
                Text(details.CurrencyCode),
                Text(details.JourneyType),
                null,
                new CandidateFareConstruction(
                    FareConstructionAssurance.Opaque,
                    SourceContextRef,
                    details.PricingUnits.Select((unit, index) => new CandidatePricingUnit(
                        $"pricingUnits/{index}",
                        Text(unit.Kind),
                        FarePricingUnitType.Unspecified,
                        FareCombinationMethod.Unspecified,
                        unit.CoveredBoundOfferIds.ToList(),
                        null,
                        unit.FareComponents.Select(component => new CandidateFareComponent(
                            Id(component.AirFareId),
                            component.FareBasis,
                            component.FareFamily,
                            component.FareType,
                            component.CabinClassId is null ? null : Id(component.CabinClassId.Value),
                            component.RbdId is null ? null : Id(component.RbdId.Value),
                            component.BookingClass,
                            component.TicketingRestrictionMinutes,
                            null,
                            null,
                            null,
                            null,
                            [],
                            [])).ToList())).ToList()));
        }

        private static CandidateSegment Segment(string boundId, AirOfferFlightWire flight)
            => new(
                SegmentRef(boundId, flight.FlightId),
                boundId,
                SegmentKind.ScheduledAir,
                Id(flight.OriginAirportId),
                flight.OriginAirportTerminalId is null ? null : Id(flight.OriginAirportTerminalId.Value),
                Id(flight.DestinationAirportId),
                flight.DestinationAirportTerminalId is null ? null : Id(flight.DestinationAirportTerminalId.Value),
                flight.DepartureDateTime,
                flight.ArrivalDateTime,
                Id(flight.FlightId),
                Text(flight.FlightNumber),
                Id(flight.FlightVersion),
                Id(flight.MarketingAirlineId),
                Id(flight.OperatingAirlineId),
                Id(flight.FlightCapacityId),
                flight.Duration,
                Id(flight.AircraftId),
                flight.Legs.OrderBy(leg => leg.Sequence).Select(Leg).ToList());

        private static CandidateSegmentLeg Leg(AirOfferLegWire leg)
            => new(
                Id(leg.LegId),
                leg.Sequence,
                Id(leg.OriginAirportId),
                leg.OriginAirportTerminalId is null ? null : Id(leg.OriginAirportTerminalId.Value),
                Id(leg.DestinationAirportId),
                leg.DestinationAirportTerminalId is null ? null : Id(leg.DestinationAirportTerminalId.Value),
                leg.DepartureDateTime,
                leg.ArrivalDateTime);

        private static CandidateService AirService(
            string serviceRef,
            string travellerRef,
            string segmentRef,
            AirOfferFlightWire flight,
            AirOfferCouponWire coupon,
            string path)
        {
            var details = new Dictionary<string, string>(StringComparer.Ordinal);
            Add(details, ServiceDetailSchemaRegistry.CabinRef, flight.CabinClassId is null ? null : Id(flight.CabinClassId.Value));
            Add(details, ServiceDetailSchemaRegistry.RbdRef, flight.RbdId is null ? null : Id(flight.RbdId.Value));
            Add(details, ServiceDetailSchemaRegistry.BookingClass, flight.BookingClass);

            return new CandidateService(
                serviceRef,
                OrderServiceType.AirTransportation,
                null,
                null,
                ServicePriceTreatment.SupplierOpaque,
                null,
                null,
                [travellerRef],
                [segmentRef],
                1,
                OrderItemUnitOfMeasure.PassengerSegment,
                ServiceDetailSchemaRegistry.AirTransportSchema,
                ServiceDetailSchemaRegistry.AirTransportSchemaVersion,
                details,
                Baggage($"{path} checked baggage", coupon.BaggagePieces, coupon.BaggageWeight, coupon.BaggageUnit),
                Baggage($"{path} cabin baggage", coupon.CabinBaggagePieces, coupon.CabinBaggageWeight, coupon.CabinBaggageUnit),
                new SoldTermFlags(coupon.IsRefundable, coupon.IsChangeable, coupon.IsUpgradable),
                new CandidateFulfillmentProfile(
                    FulfillmentProfileRef,
                    FulfillmentProfileVersion,
                    FulfillmentProfileAssurance.NotCertified,
                    ReservationRequirement.Unresolved,
                    FulfillmentDocumentKind.Unresolved,
                    null,
                    FundingRequirement.Unresolved,
                    null,
                    null,
                    null,
                    null,
                    null));
        }

        private static void EnsureRespondedOffer(string requestedOfferId, string? respondedOfferId)
        {
            if (string.IsNullOrWhiteSpace(respondedOfferId))
                throw new AirOfferContractMismatchException("details supply no offer id; the priced offer cannot be proven");

            if (!string.Equals(requestedOfferId, respondedOfferId, StringComparison.Ordinal))
                throw new AirOfferContractMismatchException(
                    $"details price offer {respondedOfferId} but offer {requestedOfferId} was requested");
        }

        private static BaggageAllowance? Baggage(string path, int pieces, int weight, string? unit)
        {
            if (pieces < 0 || weight < 0)
                throw new AirOfferContractMismatchException($"{path} carries a negative allowance");

            var hasWeight = weight > 0;

            if (hasWeight && string.IsNullOrWhiteSpace(unit))
                throw new AirOfferContractMismatchException($"{path} supplies a weight without its unit");

            if (pieces == 0 && !hasWeight)
                return null;

            return new BaggageAllowance(
                pieces > 0 ? pieces : null,
                hasWeight ? weight : null,
                hasWeight ? WeightUnit(path, unit!) : null);
        }

        private static BaggageWeightUnit WeightUnit(string path, string unit) => unit.Trim().ToUpperInvariant() switch
        {
            "KG" => BaggageWeightUnit.Kg,
            "LB" or "LBS" => BaggageWeightUnit.Lbs,
            _ => throw new AirOfferContractMismatchException($"{path} carries an unknown weight unit {unit}")
        };

        private static IReadOnlyDictionary<string, AirOfferRateOfExchangeWire> RateIndex(IEnumerable<AirOfferRateOfExchangeWire> rates)
        {
            var index = new Dictionary<string, AirOfferRateOfExchangeWire>(StringComparer.Ordinal);

            foreach (var rate in rates)
            {
                if (string.IsNullOrWhiteSpace(rate.RateOfExchangePeriodId))
                    throw new AirOfferContractMismatchException("a rate of exchange has no period identity");

                if (!index.TryAdd(rate.RateOfExchangePeriodId, rate))
                    throw new AirOfferContractMismatchException($"rate of exchange period {rate.RateOfExchangePeriodId} is repeated");
            }

            return index;
        }

        private static CandidatePricingLine Line(
            string path,
            AirOfferPricingLineWire row,
            int saleCurrencyId,
            IReadOnlyDictionary<string, AirOfferRateOfExchangeWire> rates,
            PricingBasisType basisType,
            string basisRef)
        {
            var component = Component(path, row.Category);
            var amountInSale = row.CurrencyId == saleCurrencyId;
            var equivalentInSale = row.EquivalentCurrencyId == saleCurrencyId;

            if (row.IsPercentage)
            {
                if (!equivalentInSale)
                    throw new AirOfferContractMismatchException($"{path} is a percentage row without a sale-currency valuation");

                if (row.EquivalentAmount < 0)
                    throw new AirOfferContractMismatchException($"{path} carries a negative amount; its direction is not defined by the observed contract");

                return new CandidatePricingLine(
                    path,
                    null,
                    component,
                    PricingEffect.CustomerBalance,
                    OrderPricingLineDirection.Debit,
                    PricingLineRole.Original,
                    Text(row.Code),
                    Text(row.Name),
                    Text(row.Reference),
                    PricingCalculationKind.Percentage,
                    new Money(row.EquivalentAmount, Currency(saleCurrencyId)),
                    new Money(row.EquivalentAmount, Currency(saleCurrencyId)),
                    path,
                    basisType,
                    basisRef,
                    Text(row.RateOfExchangePeriodId),
                    null,
                    null);
            }

            if (!amountInSale && !equivalentInSale)
                throw new AirOfferContractMismatchException($"{path} has no valuation in the sale currency");

            if (amountInSale && equivalentInSale && row.Amount != row.EquivalentAmount)
                throw new AirOfferContractMismatchException($"{path} amount {row.Amount} and equivalent {row.EquivalentAmount} disagree in the sale currency");

            var sale = amountInSale ? row.Amount : row.EquivalentAmount;

            if (row.Amount < 0 || sale < 0)
                throw new AirOfferContractMismatchException($"{path} carries a negative amount; its direction is not defined by the observed contract");

            var conversionRef = amountInSale ? null : Text(row.RateOfExchangePeriodId);

            return new CandidatePricingLine(
                path,
                null,
                component,
                PricingEffect.CustomerBalance,
                OrderPricingLineDirection.Debit,
                PricingLineRole.Original,
                Text(row.Code),
                Text(row.Name),
                Text(row.Reference),
                PricingCalculationKind.Amount,
                new Money(row.Amount, Currency(row.CurrencyId)),
                new Money(sale, Currency(saleCurrencyId)),
                path,
                basisType,
                basisRef,
                conversionRef,
                conversionRef is null ? null : Conversion(path, conversionRef, rates),
                null);
        }

        private static AppliedConversion? Conversion(
            string path,
            string conversionRef,
            IReadOnlyDictionary<string, AirOfferRateOfExchangeWire> rates)
        {
            if (!rates.TryGetValue(conversionRef, out var rate))
                return null;

            if (rate.Rate <= 0m)
                throw new AirOfferContractMismatchException($"{path} rate of exchange period {conversionRef} has no positive rate");

            return new AppliedConversion(
                conversionRef,
                Currency(rate.FromCurrencyId),
                Currency(rate.ToCurrencyId),
                rate.Rate,
                rate.DecimalPlaces,
                RawValue(rate.RoundingFactor));
        }

        private static PricingComponentType Component(string path, JsonElement category) => category.ValueKind switch
        {
            JsonValueKind.Number when category.TryGetInt32(out var value) && CategoryValues.TryGetValue(value, out var mapped) => mapped,
            JsonValueKind.String when Categories.TryGetValue(category.GetString()!, out var mapped) => mapped,
            _ => throw new AirOfferContractMismatchException($"{path} has an unknown pricing category {category}")
        };

        private static void Reconcile(string path, decimal baseAmount, decimal chargeAmount, decimal totalAmount, List<string> mismatches)
        {
            if (baseAmount + chargeAmount != totalAmount)
                mismatches.Add($"{path} base {baseAmount} plus charge {chargeAmount} differs from total {totalAmount}");
        }

        private static string? RawValue(JsonElement value) => value.ValueKind switch
        {
            JsonValueKind.Undefined or JsonValueKind.Null => null,
            JsonValueKind.String => Text(value.GetString()),
            _ => value.GetRawText()
        };

        private static string? RawValue(JsonElement? value) => value is null ? null : RawValue(value.Value);

        private static void Add(IDictionary<string, string> details, string key, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                details[key] = value;
        }

        private static string SegmentRef(string boundId, long flightId) => $"{boundId}|{Id(flightId)}";

        private static PassengerTypeCode PassengerType(string code)
            => Enum.TryParse<PassengerTypeCode>(code, ignoreCase: false, out var parsed) && Enum.IsDefined(parsed)
                ? parsed
                : throw new AirOfferContractMismatchException($"passenger type code {code} is not a known passenger type");

        private static string Currency(int currencyId) => Id(currencyId);

        private static string? Text(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

        private static string Id(long value) => value.ToString(CultureInfo.InvariantCulture);
    }
}
