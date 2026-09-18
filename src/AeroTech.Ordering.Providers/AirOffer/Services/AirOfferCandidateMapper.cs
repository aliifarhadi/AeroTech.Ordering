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
        public const string SourceContextRef = "airoffer:details:pricingUnits";


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
            var saleCurrency = Currency(details.CurrencyId);
            var mismatches = new List<string>();

            if (details.Tickets.Count == 0)
                throw new AirOfferContractMismatchException("details contain no priced traveler tickets");

            if (details.Tickets.Any(ticket => PassengerType(ticket.PassengerTypeCode) == PassengerTypeCode.INF))
                throw new AirOfferUnsupportedException("infant seat/resource requirement is unresolved (BD-002, OD-S1-07)");

            var segments = new List<CandidateSegment>();

            foreach (var bound in details.AirTransports.OrderBy(bound => bound.Sequence))
                foreach (var flight in bound.Flights.OrderBy(flight => flight.Sequence))
                    segments.Add(new CandidateSegment(
                        SegmentRef(bound.BoundId, flight.FlightId),
                        SegmentKind.ScheduledAir,
                        Id(flight.OriginAirportId),
                        Id(flight.DestinationAirportId),
                        flight.DepartureDateTime,
                        flight.ArrivalDateTime,
                        Id(flight.FlightId),
                        flight.Legs.OrderBy(leg => leg.Sequence).Select(leg => Id(leg.LegId)).ToList()));

            var flights = details.AirTransports
                .SelectMany(bound => bound.Flights.Select(flight => (Bound: bound.BoundId, Flight: flight)))
                .ToDictionary(entry => SegmentRef(entry.Bound, entry.Flight.FlightId), entry => entry.Flight, StringComparer.Ordinal);

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
                    services.Add(AirService(serviceRef, ticket.TravellerRef, segmentRef, flight));
                    Reconcile(path, coupon.BaseAmount, coupon.ChargeAmount, coupon.TotalAmount, mismatches);

                    var couponTotal = 0m;

                    for (var pricingIndex = 0; pricingIndex < coupon.Pricings.Count; pricingIndex++)
                    {
                        var line = Line($"{path}/pricings/{pricingIndex}", coupon.Pricings[pricingIndex], details.CurrencyId, PricingBasisType.OrderService, serviceRef);
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
                var line = Line($"orderCharges/{chargeIndex}", details.OrderCharges[chargeIndex], details.CurrencyId, PricingBasisType.OrderItem, PackageItemRef);
                lines.Add(line);
                rootTotal += line.SaleValue.Amount;
            }

            Reconcile("offer", details.BaseAmount, details.ChargeAmount, details.TotalAmount, mismatches);

            if (rootTotal != details.TotalAmount)
                mismatches.Add($"offer total {details.TotalAmount} differs from tickets and order charges {rootTotal}");

            if (mismatches.Count > 0)
                throw new AirOfferContractMismatchException(string.Join("; ", mismatches));

            return new NormalizedCandidate(
                NormalizedCandidate.CurrentSchemaVersion,
                new CandidateSource(AirOfferProfile.Owner, requestedOfferId, AirOfferProfile.LiveCandidateSandbox, null, sourcePayloadHash),
                AcceptanceAssurance.LocalCandidateOnly,
                details.PricedAt,
                capturedAt,
                new CandidateValidity(
                    new ValidityFact(ValidityState.NotSupplied, null, AirOfferProfile.Owner, null, "Details response supplies no OfferExpiresAt (BD-001)"),
                    new ValidityFact(ValidityState.NotSupplied, null, "AirPrice", null, "Details response supplies no PriceValidUntil (BD-001)"),
                    new ValidityFact(ValidityState.NotSupplied, null, "Unresolved owner", null, TicketingReason(details.LastTicketingDate))),
                new CandidateSalesContext(scope.OwnerAirlineId, scope.FinancialCustomerId, scope.Channel, scope.SellingOfficeId),
                details.Tickets.Select(ticket => new CandidateTraveler(ticket.TravellerRef, PassengerType(ticket.PassengerTypeCode))).ToList(),
                segments,
                [new CandidateItem(PackageItemRef, OrderItemKind.OfferPackage, null, services.Select(service => service.ServiceRef).ToList(), new Money(details.TotalAmount, saleCurrency))],
                services,
                lines.Select(line => line with { ItemRef = PackageItemRef }).ToList(),
                new Money(details.TotalAmount, saleCurrency),
                new CandidateFareConstruction(
                    FareConstructionAssurance.Opaque,
                    SourceContextRef,
                    details.PricingUnits.Select((unit, index) => new CandidatePricingUnit(
                        $"pricingUnits/{index}",
                        FarePricingUnitType.Unspecified,
                        FareCombinationMethod.ProviderDefined,
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
                            [])).ToList())).ToList()));
        }

        private static CandidateService AirService(string serviceRef, string travellerRef, string segmentRef, AirOfferFlightWire flight)
        {
            var details = new Dictionary<string, string>(StringComparer.Ordinal);

            Add(details, ServiceDetailSchemaRegistry.CabinRef, flight.CabinClassId is null ? null : Id(flight.CabinClassId.Value));
            Add(details, ServiceDetailSchemaRegistry.RbdRef, flight.RbdId is null ? null : Id(flight.RbdId.Value));
            Add(details, ServiceDetailSchemaRegistry.BookingClass, flight.BookingClass);
            Add(details, ServiceDetailSchemaRegistry.FlightNumber, flight.FlightNumber);
            Add(details, ServiceDetailSchemaRegistry.FlightVersion, Id(flight.FlightVersion));
            Add(details, ServiceDetailSchemaRegistry.MarketingCarrierRef, Id(flight.MarketingAirlineId));
            Add(details, ServiceDetailSchemaRegistry.OperatingCarrierRef, Id(flight.OperatingAirlineId));

            return new CandidateService(
                serviceRef,
                OrderServiceType.AirTransportation,
                [travellerRef],
                [segmentRef],
                1,
                OrderItemUnitOfMeasure.PassengerSegment,
                ServiceDetailSchemaRegistry.AirTransportSchema,
                ServiceDetailSchemaRegistry.AirTransportSchemaVersion,
                details,
                new CandidateFulfillmentProfile(FulfillmentProfileRef, ReservationRequirement.FlightCapacity, FulfillmentDocumentKind.Etkt, true, 1));
        }

        private static CandidatePricingLine Line(string path, AirOfferPricingLineWire row, int saleCurrencyId, PricingBasisType basisType, string basisRef)
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
                    new Money(row.EquivalentAmount, Currency(saleCurrencyId)),
                    new Money(row.EquivalentAmount, Currency(saleCurrencyId)),
                    path,
                    basisType,
                    basisRef,
                    row.RateOfExchangePeriodId);
            }

            if (!amountInSale && !equivalentInSale)
                throw new AirOfferContractMismatchException($"{path} has no valuation in the sale currency");

            if (amountInSale && equivalentInSale && row.Amount != row.EquivalentAmount)
                throw new AirOfferContractMismatchException($"{path} amount {row.Amount} and equivalent {row.EquivalentAmount} disagree in the sale currency");

            var sale = amountInSale ? row.Amount : row.EquivalentAmount;

            if (row.Amount < 0 || sale < 0)
                throw new AirOfferContractMismatchException($"{path} carries a negative amount; its direction is not defined by the observed contract");

            return new CandidatePricingLine(
                path,
                null,
                component,
                PricingEffect.CustomerBalance,
                OrderPricingLineDirection.Debit,
                PricingLineRole.Original,
                new Money(row.Amount, Currency(row.CurrencyId)),
                new Money(sale, Currency(saleCurrencyId)),
                path,
                basisType,
                basisRef,
                amountInSale ? null : row.RateOfExchangePeriodId);
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

        private static string TicketingReason(DateTimeOffset? lastTicketingDate)
            => lastTicketingDate is { } value
                ? $"AirOffer LastTicketingDate {value.ToString("O", CultureInfo.InvariantCulture)} observed; ticketing deadline ownership unresolved (BD-004)"
                : "Not supplied by source; ticketing deadline ownership unresolved (BD-004)";

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

        private static string Id(long value) => value.ToString(CultureInfo.InvariantCulture);
    }
}
