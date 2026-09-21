using System.Globalization;
using System.Text.Json;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;
using AeroTech.Ordering.Providers.AirOffer.Wire;

namespace AeroTech.Ordering.Providers.AirOffer.Services
{
    public static class AirOfferCandidateMapper
    {
        public const string PackageItemKey = "package";

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

        private static readonly IReadOnlyDictionary<string, JourneyType> JourneyTypes = new Dictionary<string, JourneyType>(StringComparer.OrdinalIgnoreCase)
        {
            ["OneWay"] = JourneyType.OneWay,
            ["RoundTrip"] = JourneyType.RoundTrip,
            ["Circle"] = JourneyType.Circle,
            ["OpenJaw"] = JourneyType.OpenJaw
        };

        private static readonly IReadOnlyDictionary<string, (FarePricingUnitType Type, AirFareConstructionType SourceType)> PricingUnitKinds =
            new Dictionary<string, (FarePricingUnitType, AirFareConstructionType)>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(AeroTech.Messages.AirOffer.Enums.PricingUnitKind.OneWay)] = (FarePricingUnitType.OneWay, AirFareConstructionType.OneWay),
                [nameof(AeroTech.Messages.AirOffer.Enums.PricingUnitKind.RoundTripFromOneWays)] = (FarePricingUnitType.RoundTrip, AirFareConstructionType.RoundTripFromOneWays),
                [nameof(AeroTech.Messages.AirOffer.Enums.PricingUnitKind.RoundTripFare)] = (FarePricingUnitType.RoundTrip, AirFareConstructionType.RoundTrip)
            };

        public static NormalizedCandidate Map(
            string requestedOfferId,
            AirOfferDetailsWire details,
            AuthorizedSalesScope scope,
            string sourcePayloadHash,
            DateTimeOffset capturedAt)
        {
            EnsureRespondedOffer(requestedOfferId, details.OfferId);

            var currencyId = details.CurrencyId;
            var mismatches = new List<string>();

            if (details.Tickets.Count == 0)
                throw new AirOfferContractMismatchException("details contain no priced traveller tickets");

            if (details.Tickets.Any(ticket => PassengerType(ticket.PassengerTypeCode) == PassengerTypeCode.INF))
                throw new AirOfferUnsupportedException("infant seat/resource requirement is unresolved (BD-002, OD-S1-07)");

            EnsureTravellerReferences(details.Tickets);
            EnsureNoUnsupportedStop(details.AirTransports);

            var journeys = new List<CandidateJourney>();
            var segments = new List<CandidateSegment>();

            foreach (var bound in details.AirTransports.OrderBy(bound => bound.Sequence))
            {
                journeys.Add(new CandidateJourney(
                    bound.BoundId,
                    bound.Sequence,
                    Direction(bound.BoundId, bound.Direction),
                    bound.OriginAirportId,
                    bound.DestinationAirportId));

                foreach (var flight in bound.Flights.OrderBy(flight => flight.Sequence))
                    segments.Add(Segment(bound.BoundId, flight));
            }

            var flights = details.AirTransports
                .SelectMany(bound => bound.Flights.Select(flight => (Bound: bound.BoundId, Flight: flight)))
                .ToDictionary(entry => SegmentKey(entry.Bound, entry.Flight.FlightId), entry => entry.Flight, StringComparer.Ordinal);

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
                    var segmentKey = SegmentKey(coupon.BoundId, coupon.FlightId);

                    if (!flights.TryGetValue(segmentKey, out var flight))
                        throw new AirOfferContractMismatchException($"{path} references bound {coupon.BoundId} flight {coupon.FlightId}, which is not in airTransports");

                    var serviceKey = $"{ticket.TravellerRef}|{segmentKey}";
                    services.Add(AirService(serviceKey, ticket.TravellerRef, segmentKey, flight, coupon, path));

                    Reconcile(path, coupon.BaseAmount, coupon.ChargeAmount, coupon.TotalAmount, mismatches);
                    var couponTotal = 0m;

                    for (var pricingIndex = 0; pricingIndex < coupon.Pricings.Count; pricingIndex++)
                    {
                        var line = Line($"{path}/pricings/{pricingIndex}", coupon.Pricings[pricingIndex], currencyId, rates, PricingBasisType.OrderService, serviceKey);
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
                var line = Line($"orderCharges/{chargeIndex}", details.OrderCharges[chargeIndex], currencyId, rates, PricingBasisType.OrderItem, PackageItemKey);
                lines.Add(line);
                rootTotal += line.SaleValue.Amount;
            }

            Reconcile("offer", details.BaseAmount, details.ChargeAmount, details.TotalAmount, mismatches);

            if (rootTotal != details.TotalAmount)
                mismatches.Add($"offer total {details.TotalAmount} differs from tickets and order charges {rootTotal}");

            if (mismatches.Count > 0)
                throw new AirOfferContractMismatchException(string.Join("; ", mismatches));

            var package = new CandidateItem(
                PackageItemKey,
                OrderItemKind.OfferPackage,
                services.Select(service => service.ServiceKey).ToList(),
                new Money(details.TotalAmount, currencyId));

            return new NormalizedCandidate(
                NormalizedCandidate.CurrentSchemaVersion,
                new CandidateSource(AirOfferProfile.Owner, requestedOfferId, AirOfferProfile.LiveCandidateSandbox, null, sourcePayloadHash),
                AcceptanceAssurance.LocalCandidateOnly,
                details.PricedAt,
                capturedAt,
                null,
                null,
                details.LastTicketingDate,
                new CandidateSalesContext(scope.OwnerAirlineId, scope.FinancialCustomerId, scope.SalesContext, scope.Buyer),
                JourneyTypeOf(details.JourneyType),
                details.Tickets.Select(ticket => new CandidateTraveller(ticket.TravellerRef, PassengerType(ticket.PassengerTypeCode))).ToList(),
                journeys,
                segments,
                [package],
                services,
                lines.Select(line => line with { ItemKey = PackageItemKey }).ToList(),
                new Money(details.TotalAmount, currencyId),
                Text(details.CurrencyCode),
                FareConstruction(details));
        }

        private static CandidateFareConstruction? FareConstruction(AirOfferDetailsWire details)
        {
            if (details.PricingUnits.Count == 0)
                return null;

            return new CandidateFareConstruction(
                FareConstructionAssurance.SourceProvided,
                [PackageItemKey],
                details.PricingUnits
                    .Select((unit, index) =>
                    {
                        var kind = PricingUnitKindOf(unit.Kind);

                        return new CandidatePricingUnit(
                            index + 1,
                            kind.Type,
                            kind.SourceType,
                            unit.CoveredBoundOfferIds.ToList(),
                            unit.FareComponents.Select(component => new CandidateFareComponent(
                                component.AirFareId,
                                Text(component.FareBasis),
                                Text(component.FareFamily),
                                Text(component.FareType),
                                component.CabinClassId,
                                component.RbdId,
                                Text(component.BookingClass),
                                component.TicketingRestrictionMinutes)).ToList());
                    })
                    .ToList());
        }

        private static CandidateSegment Segment(string boundId, AirOfferFlightWire flight)
            => new(
                SegmentKey(boundId, flight.FlightId),
                boundId,
                flight.Sequence,
                SegmentKind.ScheduledAir,
                flight.OriginAirportId,
                flight.OriginAirportTerminalId,
                flight.DestinationAirportId,
                flight.DestinationAirportTerminalId,
                flight.DepartureDateTime,
                flight.ArrivalDateTime,
                flight.FlightId,
                Text(flight.FlightNumber),
                flight.FlightVersion,
                flight.MarketingAirlineId,
                flight.OperatingAirlineId,
                flight.FlightCapacityId,
                flight.Duration,
                flight.AircraftId,
                flight.Legs.OrderBy(leg => leg.Sequence).Select(Leg).ToList());

        private static CandidateSegmentLeg Leg(AirOfferLegWire leg)
            => new(
                leg.LegId,
                leg.Sequence,
                leg.OriginAirportId,
                leg.OriginAirportTerminalId,
                leg.DestinationAirportId,
                leg.DestinationAirportTerminalId,
                leg.DepartureDateTime,
                leg.ArrivalDateTime);

        private static CandidateService AirService(
            string serviceKey,
            string travellerRef,
            string segmentKey,
            AirOfferFlightWire flight,
            AirOfferCouponWire coupon,
            string path)
            => new(
                serviceKey,
                travellerRef,
                segmentKey,
                flight.CabinClassId,
                flight.RbdId,
                Text(flight.BookingClass),
                Baggage($"{path} checked baggage", coupon.BaggagePieces, coupon.BaggageWeight, coupon.BaggageUnit),
                Baggage($"{path} cabin baggage", coupon.CabinBaggagePieces, coupon.CabinBaggageWeight, coupon.CabinBaggageUnit),
                new SoldTermFlags(coupon.IsRefundable, coupon.IsChangeable, coupon.IsUpgradable),
                CandidateFulfillmentProfile.Unresolved);

        private static void EnsureRespondedOffer(string requestedOfferId, string? respondedOfferId)
        {
            if (string.IsNullOrWhiteSpace(respondedOfferId))
                throw new AirOfferContractMismatchException("details supply no offer id; the priced offer cannot be proven");

            if (!string.Equals(requestedOfferId, respondedOfferId, StringComparison.Ordinal))
                throw new AirOfferContractMismatchException(
                    $"details price offer {respondedOfferId} but offer {requestedOfferId} was requested");
        }

        private static void EnsureNoUnsupportedStop(IReadOnlyList<AirOfferAirTransportWire> bounds)
        {
            foreach (var bound in bounds)
            {
                foreach (var flight in bound.Flights)
                {
                    if (IsSupplied(flight.Stop))
                        throw new AirOfferUnsupportedException(
                            $"bound {bound.BoundId} flight {flight.FlightId} supplies a stop payload whose connection semantics are not approved (BD-006)");

                    foreach (var leg in flight.Legs)
                        if (IsSupplied(leg.Stop))
                            throw new AirOfferUnsupportedException(
                                $"bound {bound.BoundId} flight {flight.FlightId} leg {leg.LegId} supplies a stop payload whose connection semantics are not approved (BD-006)");
                }
            }
        }

        private static bool IsSupplied(JsonElement? stop)
            => stop is { } value && value.ValueKind != JsonValueKind.Null && value.ValueKind != JsonValueKind.Undefined;

        private static void EnsureTravellerReferences(IReadOnlyList<AirOfferTicketWire> tickets)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);

            for (var index = 0; index < tickets.Count; index++)
            {
                var travellerRef = tickets[index].TravellerRef;

                if (string.IsNullOrWhiteSpace(travellerRef))
                    throw new AirOfferContractMismatchException($"tickets/{index} supplies no traveller reference; the priced traveller cannot be identified");

                if (!seen.Add(travellerRef))
                    throw new AirOfferContractMismatchException($"tickets/{index} repeats traveller reference {travellerRef}");
            }
        }

        private static JourneyType JourneyTypeOf(string? journeyType)
            => journeyType is not null && JourneyTypes.TryGetValue(journeyType, out var mapped)
                ? mapped
                : throw new AirOfferContractMismatchException($"journey type {journeyType ?? "(missing)"} is not a known journey type");

        private static (FarePricingUnitType Type, AirFareConstructionType SourceType) PricingUnitKindOf(string? kind)
            => kind is not null && PricingUnitKinds.TryGetValue(kind, out var mapped)
                ? mapped
                : throw new AirOfferContractMismatchException($"pricing unit kind {kind ?? "(missing)"} is not a known pricing unit type");

        private static BoundDirection Direction(string boundId, JsonElement direction) => direction.ValueKind switch
        {
            JsonValueKind.Number when direction.TryGetInt32(out var value) && Enum.IsDefined((BoundDirection)value) => (BoundDirection)value,
            JsonValueKind.String when Enum.TryParse<BoundDirection>(direction.GetString(), ignoreCase: true, out var parsed) && Enum.IsDefined(parsed) => parsed,
            _ => throw new AirOfferContractMismatchException($"bound {boundId} direction {direction} is not a known bound direction")
        };

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
            string basisKey)
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
                    new Money(row.EquivalentAmount, saleCurrencyId),
                    new Money(row.EquivalentAmount, saleCurrencyId),
                    basisType,
                    basisKey,
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
                new Money(row.Amount, row.CurrencyId),
                new Money(sale, saleCurrencyId),
                basisType,
                basisKey,
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
                rate.FromCurrencyId,
                rate.ToCurrencyId,
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

        private static string SegmentKey(string boundId, long flightId)
            => $"{boundId}|{flightId.ToString(CultureInfo.InvariantCulture)}";

        private static PassengerTypeCode PassengerType(string code)
            => Enum.TryParse<PassengerTypeCode>(code, ignoreCase: false, out var parsed) && Enum.IsDefined(parsed)
                ? parsed
                : throw new AirOfferContractMismatchException($"passenger type code {code} is not a known passenger type");

        private static string? Text(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
