using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using AeroTech.Messages.Aegis.Enums;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Messages.Shared.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Domain._Shared.Serialization;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.Serialization
{
    public static partial class NormalizedCandidateJson
    {
        public static string Write(NormalizedCandidate candidate) => CanonicalJson.Write(ToNode(candidate));

        public static string Digest(NormalizedCandidate candidate) => CanonicalJson.Sha256Hex(Write(candidate));

        public static IReadOnlyDictionary<string, object?> ToNode(NormalizedCandidate candidate) => Map(
            ("schemaVersion", candidate.SchemaVersion),
            ("source", Map(
                ("owner", candidate.Source.Owner),
                ("offerId", candidate.Source.OfferId),
                ("providerProfileId", candidate.Source.ProviderProfileId),
                ("ownerBindingRef", candidate.Source.OwnerBindingRef),
                ("sourcePayloadHash", candidate.Source.SourcePayloadHash))),
            ("acceptanceAssurance", CandidateVocabulary.Name(candidate.AcceptanceAssurance)),
            ("pricedAt", CanonicalJson.Instant(candidate.PricedAt)),
            ("capturedAt", CanonicalJson.Instant(candidate.CapturedAt)),
            ("offerExpiresAt", CanonicalJson.Instant(candidate.OfferExpiresAt)),
            ("priceValidUntil", CanonicalJson.Instant(candidate.PriceValidUntil)),
            ("lastTicketingDate", CanonicalJson.Instant(candidate.LastTicketingDate)),
            ("salesContext", Map(
                ("ownerAirlineId", candidate.SalesContext.OwnerAirlineId),
                ("financialCustomerId", candidate.SalesContext.FinancialCustomerId),
                ("channel", CandidateVocabulary.Name(candidate.SalesContext.Channel)),
                ("seller", PartyNode(candidate.SalesContext.Sales.SellerContextType, candidate.SalesContext.Sales.SellerId)),
                ("sellingOffice", candidate.SalesContext.Sales.HasSellingOffice
                    ? Map(
                        ("kind", CandidateVocabulary.Name(candidate.SalesContext.Sales.SellingOfficeKind!.Value)),
                        ("officeId", candidate.SalesContext.Sales.SellingOfficeId!.Value))
                    : null),
                ("buyer", PartyNode(candidate.SalesContext.Buyer.ContextType, candidate.SalesContext.Buyer.BuyerId)))),
            ("journeyType", CandidateVocabulary.Name(candidate.JourneyType)),
            ("travellers", candidate.Travellers.Select(traveller => (object?)Map(
                ("travellerRef", traveller.TravellerRef),
                ("passengerTypeCode", CandidateVocabulary.Name(traveller.PassengerTypeCode)))).ToList()),
            ("journeys", candidate.Journeys.Select(journey => (object?)Map(
                ("boundId", journey.BoundId),
                ("sequence", journey.Sequence),
                ("direction", CandidateVocabulary.Name(journey.Direction)),
                ("originAirportId", journey.OriginAirportId),
                ("destinationAirportId", journey.DestinationAirportId))).ToList()),
            ("segments", candidate.Segments.Select(segment => (object?)Map(
                ("segmentKey", segment.SegmentKey),
                ("boundId", segment.BoundId),
                ("sequence", segment.Sequence),
                ("kind", CandidateVocabulary.Name(segment.Kind)),
                ("originAirportId", segment.OriginAirportId),
                ("originAirportTerminalId", segment.OriginAirportTerminalId),
                ("destinationAirportId", segment.DestinationAirportId),
                ("destinationAirportTerminalId", segment.DestinationAirportTerminalId),
                ("soldDeparture", CanonicalJson.Instant(segment.SoldDeparture)),
                ("soldArrival", CanonicalJson.Instant(segment.SoldArrival)),
                ("flightId", segment.FlightId),
                ("flightNumber", segment.FlightNumber),
                ("flightVersion", segment.FlightVersion),
                ("marketingAirlineId", segment.MarketingAirlineId),
                ("operatingAirlineId", segment.OperatingAirlineId),
                ("flightCapacityId", segment.FlightCapacityId),
                ("duration", segment.Duration),
                ("aircraftId", segment.AircraftId),
                ("legs", segment.Legs.Select(leg => (object?)Map(
                    ("legId", leg.LegId),
                    ("sequence", leg.Sequence),
                    ("originAirportId", leg.OriginAirportId),
                    ("originAirportTerminalId", leg.OriginAirportTerminalId),
                    ("destinationAirportId", leg.DestinationAirportId),
                    ("destinationAirportTerminalId", leg.DestinationAirportTerminalId),
                    ("departureDateTime", CanonicalJson.Instant(leg.DepartureDateTime)),
                    ("arrivalDateTime", CanonicalJson.Instant(leg.ArrivalDateTime)))).ToList()))).ToList()),
            ("items", candidate.Items.Select(item => (object?)Map(
                ("itemKey", item.ItemKey),
                ("itemKind", CandidateVocabulary.Name(item.ItemKind)),
                ("serviceKeys", Strings(item.ServiceKeys)),
                ("acceptedTotal", MoneyNode(item.AcceptedTotal)))).ToList()),
            ("services", candidate.Services.Select(service => (object?)Map(
                ("serviceKey", service.ServiceKey),
                ("travellerRef", service.TravellerRef),
                ("segmentKey", service.SegmentKey),
                ("cabinClassId", service.CabinClassId),
                ("rbdId", service.RbdId),
                ("bookingClass", service.BookingClass),
                ("checkedBaggage", BaggageNode(service.CheckedBaggage)),
                ("cabinBaggage", BaggageNode(service.CabinBaggage)),
                ("soldTerms", Map(
                    ("refundable", service.SoldTerms.Refundable),
                    ("changeable", service.SoldTerms.Changeable),
                    ("upgradable", service.SoldTerms.Upgradable))),
                ("fulfillmentProfile", FulfillmentNode(service.FulfillmentProfile)))).ToList()),
            ("pricingLines", candidate.PricingLines.Select(line => (object?)Map(
                ("sourceOccurrencePath", line.SourceOccurrencePath),
                ("itemKey", line.ItemKey),
                ("component", CandidateVocabulary.Name(line.Component)),
                ("effect", CandidateVocabulary.Name(line.Effect)),
                ("direction", CandidateVocabulary.Name(line.Direction)),
                ("role", CandidateVocabulary.Name(line.Role)),
                ("code", line.Code),
                ("name", line.Name),
                ("reference", line.Reference),
                ("calculationKind", CandidateVocabulary.Name(line.CalculationKind)),
                ("originalValue", MoneyNode(line.OriginalValue)),
                ("saleValue", MoneyNode(line.SaleValue)),
                ("basisType", CandidateVocabulary.Name(line.BasisType)),
                ("basisKey", line.BasisKey),
                ("sourceConversionRef", line.SourceConversionRef),
                ("appliedConversion", ConversionNode(line.AppliedConversion)),
                ("settlementAttribution", AttributionNode(line.SettlementAttribution)))).ToList()),
            ("customerTotal", MoneyNode(candidate.CustomerTotal)),
            ("saleCurrencyCode", candidate.SaleCurrencyCode),
            ("fareConstruction", candidate.FareConstruction is not { } construction ? null : Map(
                ("assurance", CandidateVocabulary.Name(construction.Assurance)),
                ("itemKeys", Strings(construction.ItemKeys)),
                ("pricingUnits", construction.PricingUnits.Select(unit => (object?)Map(
                    ("sequence", unit.Sequence),
                    ("type", CandidateVocabulary.Name(unit.Type)),
                    ("sourceConstructionType", CandidateVocabulary.Name(unit.SourceConstructionType)),
                    ("coveredBoundOfferIds", Strings(unit.CoveredBoundOfferIds)),
                    ("components", unit.Components.Select(component => (object?)Map(
                        ("airFareId", component.AirFareId),
                        ("fareBasis", component.FareBasis),
                        ("fareFamily", component.FareFamily),
                        ("fareType", component.FareType),
                        ("cabinClassId", component.CabinClassId),
                        ("rbdId", component.RbdId),
                        ("bookingClass", component.BookingClass),
                        ("ticketingRestrictionMinutes", component.TicketingRestrictionMinutes))).ToList()))).ToList()))));

        public static NormalizedCandidate Read(string json)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                return ReadCandidate(new Node(document.RootElement, "candidate"));
            }
            catch (JsonException exception)
            {
                throw ExceptionFactory.CandidateContractMismatch($"candidate is not valid JSON ({exception.Message})");
            }
        }

        private static NormalizedCandidate ReadCandidate(Node root)
        {
            root.Only("schemaVersion", "source", "acceptanceAssurance", "pricedAt", "capturedAt", "offerExpiresAt",
                "priceValidUntil", "lastTicketingDate", "salesContext", "journeyType", "travellers", "journeys",
                "segments", "items", "services", "pricingLines", "customerTotal", "saleCurrencyCode", "fareConstruction");

            var schemaVersion = root.String("schemaVersion");

            if (schemaVersion != NormalizedCandidate.CurrentSchemaVersion)
                throw Mismatch($"candidate schema version {schemaVersion} is not supported");

            var source = root.Object("source");
            source.Only("owner", "offerId", "providerProfileId", "ownerBindingRef", "sourcePayloadHash");

            var construction = root.NullableObject("fareConstruction");
            construction?.Only("assurance", "itemKeys", "pricingUnits");

            return new NormalizedCandidate(
                schemaVersion,
                new CandidateSource(
                    source.String("owner"),
                    source.String("offerId"),
                    source.String("providerProfileId"),
                    source.NullableString("ownerBindingRef"),
                    source.String("sourcePayloadHash")),
                root.Enum<AcceptanceAssurance>("acceptanceAssurance"),
                root.Instant("pricedAt"),
                root.Instant("capturedAt"),
                root.NullableInstant("offerExpiresAt"),
                root.NullableInstant("priceValidUntil"),
                root.NullableInstant("lastTicketingDate"),
                ReadSalesContext(root.Object("salesContext")),
                root.Enum<JourneyType>("journeyType"),
                root.Array("travellers").Select(traveller =>
                {
                    traveller.Only("travellerRef", "passengerTypeCode");
                    return new CandidateTraveller(
                        traveller.String("travellerRef"),
                        CandidateVocabulary.Parse<PassengerTypeCode>(traveller.String("passengerTypeCode"), "traveller.passengerTypeCode"));
                }).ToList(),
                root.Array("journeys").Select(ReadJourney).ToList(),
                root.Array("segments").Select(ReadSegment).ToList(),
                root.Array("items").Select(ReadItem).ToList(),
                root.Array("services").Select(ReadService).ToList(),
                root.Array("pricingLines").Select(ReadPricingLine).ToList(),
                ReadMoney(root.Object("customerTotal")),
                root.NullableString("saleCurrencyCode"),
                construction is null
                    ? null
                    : new CandidateFareConstruction(
                        construction.Value.Enum<FareConstructionAssurance>("assurance"),
                        construction.Value.Strings("itemKeys"),
                        construction.Value.Array("pricingUnits").Select(ReadPricingUnit).ToList()));
        }

        private static CandidateSalesContext ReadSalesContext(Node sales)
        {
            sales.Only("ownerAirlineId", "financialCustomerId", "channel", "seller", "sellingOffice", "buyer");

            var seller = sales.NullableObject("seller");
            var office = sales.NullableObject("sellingOffice");
            var buyer = sales.NullableObject("buyer");

            seller?.Only("contextType", "partyId");
            office?.Only("kind", "officeId");
            buyer?.Only("contextType", "partyId");

            return new CandidateSalesContext(
                sales.Identifier("ownerAirlineId"),
                sales.Identifier("financialCustomerId"),
                new SalesContextSnapshot(
                    CandidateVocabulary.Parse<SalesChannel>(sales.String("channel"), "salesContext.channel"),
                    seller?.Enum<BusinessContextType>("contextType"),
                    seller?.Identifier("partyId"),
                    office?.Enum<SellingOfficeKind>("kind"),
                    office?.Identifier("officeId")),
                new BuyerSnapshot(buyer?.Enum<BusinessContextType>("contextType"), buyer?.Identifier("partyId")));
        }

        private static CandidateJourney ReadJourney(Node journey)
        {
            journey.Only("boundId", "sequence", "direction", "originAirportId", "destinationAirportId");

            return new CandidateJourney(
                journey.String("boundId"),
                journey.Integer("sequence"),
                journey.Enum<BoundDirection>("direction"),
                journey.Integer("originAirportId"),
                journey.Integer("destinationAirportId"));
        }

        private static CandidateSegment ReadSegment(Node segment)
        {
            segment.Only("segmentKey", "boundId", "sequence", "kind", "originAirportId", "originAirportTerminalId",
                "destinationAirportId", "destinationAirportTerminalId", "soldDeparture", "soldArrival", "flightId",
                "flightNumber", "flightVersion", "marketingAirlineId", "operatingAirlineId", "flightCapacityId",
                "duration", "aircraftId", "legs");

            return new CandidateSegment(
                segment.String("segmentKey"),
                segment.String("boundId"),
                segment.Integer("sequence"),
                segment.Enum<SegmentKind>("kind"),
                segment.Integer("originAirportId"),
                segment.NullableInteger("originAirportTerminalId"),
                segment.Integer("destinationAirportId"),
                segment.NullableInteger("destinationAirportTerminalId"),
                segment.NullableInstant("soldDeparture"),
                segment.NullableInstant("soldArrival"),
                segment.NullableIdentifier("flightId"),
                segment.NullableString("flightNumber"),
                segment.NullableInteger("flightVersion"),
                segment.NullableInteger("marketingAirlineId"),
                segment.NullableInteger("operatingAirlineId"),
                segment.NullableIdentifier("flightCapacityId"),
                segment.NullableInteger("duration"),
                segment.NullableInteger("aircraftId"),
                segment.Array("legs").Select(ReadLeg).ToList());
        }

        private static CandidateSegmentLeg ReadLeg(Node leg)
        {
            leg.Only("legId", "sequence", "originAirportId", "originAirportTerminalId", "destinationAirportId",
                "destinationAirportTerminalId", "departureDateTime", "arrivalDateTime");

            return new CandidateSegmentLeg(
                leg.Identifier("legId"),
                leg.Integer("sequence"),
                leg.NullableInteger("originAirportId"),
                leg.NullableInteger("originAirportTerminalId"),
                leg.NullableInteger("destinationAirportId"),
                leg.NullableInteger("destinationAirportTerminalId"),
                leg.NullableInstant("departureDateTime"),
                leg.NullableInstant("arrivalDateTime"));
        }

        private static CandidateItem ReadItem(Node item)
        {
            item.Only("itemKey", "itemKind", "serviceKeys", "acceptedTotal");

            return new CandidateItem(
                item.String("itemKey"),
                item.Enum<OrderItemKind>("itemKind"),
                item.Strings("serviceKeys"),
                ReadMoney(item.Object("acceptedTotal")));
        }

        private static CandidateService ReadService(Node service)
        {
            service.Only("serviceKey", "travellerRef", "segmentKey", "cabinClassId", "rbdId", "bookingClass",
                "checkedBaggage", "cabinBaggage", "soldTerms", "fulfillmentProfile");

            var terms = service.Object("soldTerms");
            terms.Only("refundable", "changeable", "upgradable");

            return new CandidateService(
                service.String("serviceKey"),
                service.String("travellerRef"),
                service.String("segmentKey"),
                service.NullableInteger("cabinClassId"),
                service.NullableIdentifier("rbdId"),
                service.NullableString("bookingClass"),
                ReadBaggage(service.NullableObject("checkedBaggage")),
                ReadBaggage(service.NullableObject("cabinBaggage")),
                new SoldTermFlags(
                    terms.NullableBoolean("refundable"),
                    terms.NullableBoolean("changeable"),
                    terms.NullableBoolean("upgradable")),
                ReadFulfillmentProfile(service.Object("fulfillmentProfile")));
        }

        private static CandidatePricingLine ReadPricingLine(Node line)
        {
            line.Only("sourceOccurrencePath", "itemKey", "component", "effect", "direction", "role", "code", "name",
                "reference", "calculationKind", "originalValue", "saleValue", "basisType", "basisKey",
                "sourceConversionRef", "appliedConversion", "settlementAttribution");

            return new CandidatePricingLine(
                line.String("sourceOccurrencePath"),
                line.NullableString("itemKey"),
                line.Enum<PricingComponentType>("component"),
                line.Enum<PricingEffect>("effect"),
                line.Enum<OrderPricingLineDirection>("direction"),
                line.Enum<PricingLineRole>("role"),
                line.NullableString("code"),
                line.NullableString("name"),
                line.NullableString("reference"),
                line.Enum<PricingCalculationKind>("calculationKind"),
                ReadMoney(line.Object("originalValue")),
                ReadMoney(line.Object("saleValue")),
                line.Enum<PricingBasisType>("basisType"),
                line.String("basisKey"),
                line.NullableString("sourceConversionRef"),
                ReadConversion(line.NullableObject("appliedConversion")),
                ReadAttribution(line.NullableObject("settlementAttribution")));
        }

        private static CandidatePricingUnit ReadPricingUnit(Node unit)
        {
            unit.Only("sequence", "type", "sourceConstructionType", "coveredBoundOfferIds", "components");

            return new CandidatePricingUnit(
                unit.Integer("sequence"),
                unit.Enum<FarePricingUnitType>("type"),
                unit.Enum<AirFareConstructionType>("sourceConstructionType"),
                unit.Strings("coveredBoundOfferIds"),
                unit.Array("components").Select(ReadFareComponent).ToList());
        }

        private static CandidateFareComponent ReadFareComponent(Node component)
        {
            component.Only("airFareId", "fareBasis", "fareFamily", "fareType", "cabinClassId", "rbdId",
                "bookingClass", "ticketingRestrictionMinutes");

            return new CandidateFareComponent(
                component.Identifier("airFareId"),
                component.NullableString("fareBasis"),
                component.NullableString("fareFamily"),
                component.NullableString("fareType"),
                component.NullableInteger("cabinClassId"),
                component.NullableIdentifier("rbdId"),
                component.NullableString("bookingClass"),
                component.NullableInteger("ticketingRestrictionMinutes"));
        }

        private static BaggageAllowance? ReadBaggage(Node? node)
        {
            if (node is not { } baggage)
                return null;

            baggage.Only("pieces", "weight", "weightUnit");

            return new BaggageAllowance(
                baggage.NullableInteger("pieces"),
                baggage.NullableDecimal("weight", QuantityPattern()),
                baggage.NullableEnum<BaggageWeightUnit>("weightUnit"));
        }

        private static AppliedConversion? ReadConversion(Node? node)
        {
            if (node is not { } conversion)
                return null;

            conversion.Only("sourceConversionRef", "fromCurrencyId", "toCurrencyId", "rate", "decimalPlaces", "roundingToken");

            return new AppliedConversion(
                conversion.String("sourceConversionRef"),
                conversion.Integer("fromCurrencyId"),
                conversion.Integer("toCurrencyId"),
                conversion.Decimal("rate", RatePattern()),
                conversion.Integer("decimalPlaces"),
                conversion.NullableString("roundingToken"));
        }

        private static SettlementAttribution? ReadAttribution(Node? node)
        {
            if (node is not { } attribution)
                return null;

            attribution.Only("partyRef", "categoryCode");

            return new SettlementAttribution(attribution.String("partyRef"), attribution.String("categoryCode"));
        }

        private static Money ReadMoney(Node node)
        {
            node.Only("amount", "currencyId");
            return new Money(node.Decimal("amount", AmountPattern()), node.Integer("currencyId"));
        }

        private static IReadOnlyDictionary<string, object?>? PartyNode(BusinessContextType? contextType, long? partyId)
            => contextType is null || partyId is null
                ? null
                : Map(("contextType", CandidateVocabulary.Name(contextType.Value)), ("partyId", partyId.Value));

        private static IReadOnlyDictionary<string, object?>? BaggageNode(BaggageAllowance? allowance) => allowance is null ? null : Map(
            ("pieces", allowance.Pieces),
            ("weight", allowance.Weight is null ? null : CanonicalJson.Amount(allowance.Weight.Value)),
            ("weightUnit", OptionalName(allowance.WeightUnit)));

        private static IReadOnlyDictionary<string, object?>? ConversionNode(AppliedConversion? conversion) => conversion is null ? null : Map(
            ("sourceConversionRef", conversion.SourceConversionRef),
            ("fromCurrencyId", conversion.FromCurrencyId),
            ("toCurrencyId", conversion.ToCurrencyId),
            ("rate", CanonicalJson.Amount(conversion.Rate)),
            ("decimalPlaces", conversion.DecimalPlaces),
            ("roundingToken", conversion.RoundingToken));

        private static IReadOnlyDictionary<string, object?>? AttributionNode(SettlementAttribution? attribution) => attribution is null ? null : Map(
            ("partyRef", attribution.PartyRef),
            ("categoryCode", attribution.CategoryCode));

        private static CandidateFulfillmentProfile ReadFulfillmentProfile(Node node)
        {
            node.Only("profileRef", "profileVersion", "assurance", "reservationRequirement", "documentKind",
                "documentAuthority", "fundingRequirement", "capacityUnits", "resourceUnitPolicyRef",
                "deliveryControlPolicyRef", "dependencyTreatmentPolicyRef", "partialFulfillmentSupported");

            return new CandidateFulfillmentProfile(
                node.NullableString("profileRef"),
                node.NullableString("profileVersion"),
                node.Enum<FulfillmentProfileAssurance>("assurance"),
                node.Enum<ReservationRequirement>("reservationRequirement"),
                node.Enum<FulfillmentDocumentKind>("documentKind"),
                node.NullableEnum<DocumentAuthority>("documentAuthority"),
                node.Enum<FundingRequirement>("fundingRequirement"),
                node.NullableInteger("capacityUnits"),
                node.NullableString("resourceUnitPolicyRef"),
                node.NullableString("deliveryControlPolicyRef"),
                node.NullableString("dependencyTreatmentPolicyRef"),
                node.NullableBoolean("partialFulfillmentSupported"));
        }

        private static IReadOnlyDictionary<string, object?> FulfillmentNode(CandidateFulfillmentProfile profile) => Map(
            ("profileRef", profile.ProfileRef),
            ("profileVersion", profile.ProfileVersion),
            ("assurance", CandidateVocabulary.Name(profile.Assurance)),
            ("reservationRequirement", CandidateVocabulary.Name(profile.ReservationRequirement)),
            ("documentKind", CandidateVocabulary.Name(profile.DocumentKind)),
            ("documentAuthority", OptionalName(profile.DocumentAuthority)),
            ("fundingRequirement", CandidateVocabulary.Name(profile.FundingRequirement)),
            ("capacityUnits", profile.CapacityUnits),
            ("resourceUnitPolicyRef", profile.ResourceUnitPolicyRef),
            ("deliveryControlPolicyRef", profile.DeliveryControlPolicyRef),
            ("dependencyTreatmentPolicyRef", profile.DependencyTreatmentPolicyRef),
            ("partialFulfillmentSupported", profile.PartialFulfillmentSupported));

        private static IReadOnlyDictionary<string, object?> MoneyNode(Money money) => Map(
            ("amount", CanonicalJson.Amount(money.Amount)),
            ("currencyId", money.CurrencyId));

        private static string? OptionalName<TEnum>(TEnum? value) where TEnum : struct, Enum
            => value is null ? null : CandidateVocabulary.Name(value.Value);

        private static List<object?> Strings(IEnumerable<string> values) => values.Select(value => (object?)value).ToList();

        private static IReadOnlyDictionary<string, object?> Map(params (string Key, object? Value)[] pairs)
            => pairs.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

        [GeneratedRegex(@"^(0|[1-9][0-9]*)(\.[0-9]{1,8})?$")]
        private static partial Regex AmountPattern();

        [GeneratedRegex(@"^[0-9]+(\.[0-9]{1,6})?$")]
        private static partial Regex QuantityPattern();

        [GeneratedRegex(@"^(0|[1-9][0-9]*)(\.[0-9]{1,12})?$")]
        private static partial Regex RatePattern();

        private readonly struct Node
        {
            private readonly JsonElement _element;
            private readonly string _path;

            public Node(JsonElement element, string path)
            {
                if (element.ValueKind != JsonValueKind.Object)
                    throw Mismatch($"{path} must be an object");

                _element = element;
                _path = path;
            }

            public void Only(params string[] names)
            {
                var allowed = new HashSet<string>(names, StringComparer.Ordinal);

                foreach (var property in _element.EnumerateObject())
                    if (!allowed.Contains(property.Name))
                        throw Mismatch($"{_path}.{property.Name} is not part of the candidate contract");

                foreach (var name in names)
                    if (!_element.TryGetProperty(name, out _))
                        throw Mismatch($"{_path}.{name} is required");
            }

            public Node Object(string name) => new(Get(name), $"{_path}.{name}");

            public Node? NullableObject(string name)
            {
                var value = Get(name);
                return value.ValueKind == JsonValueKind.Null ? null : new Node(value, $"{_path}.{name}");
            }

            public IEnumerable<Node> Array(string name)
            {
                var value = Get(name);

                if (value.ValueKind != JsonValueKind.Array)
                    throw Mismatch($"{_path}.{name} must be an array");

                var path = $"{_path}.{name}";
                return value.EnumerateArray().Select((element, index) => new Node(element, $"{path}[{index}]")).ToList();
            }

            public string String(string name)
            {
                var value = NullableString(name);
                return string.IsNullOrEmpty(value) ? throw Mismatch($"{_path}.{name} must be a non-empty string") : value;
            }

            public string? NullableString(string name)
            {
                var value = Get(name);

                return value.ValueKind switch
                {
                    JsonValueKind.Null => null,
                    JsonValueKind.String when value.GetString()!.Length > 0 => value.GetString(),
                    _ => throw Mismatch($"{_path}.{name} must be a non-empty string or null")
                };
            }

            public IReadOnlyList<string> Strings(string name)
            {
                var value = Get(name);

                if (value.ValueKind != JsonValueKind.Array)
                    throw Mismatch($"{_path}.{name} must be an array");

                var path = $"{_path}.{name}";

                return value.EnumerateArray()
                    .Select(element => element.ValueKind == JsonValueKind.String && element.GetString()!.Length > 0
                        ? element.GetString()!
                        : throw Mismatch($"{path} must contain non-empty strings"))
                    .ToList();
            }

            public TEnum Enum<TEnum>(string name) where TEnum : struct, Enum
                => CandidateVocabulary.Parse<TEnum>(String(name), $"{_path}.{name}");

            public TEnum? NullableEnum<TEnum>(string name) where TEnum : struct, Enum
            {
                var text = NullableString(name);
                return text is null ? null : CandidateVocabulary.Parse<TEnum>(text, $"{_path}.{name}");
            }

            public DateTimeOffset Instant(string name)
                => NullableInstant(name) ?? throw Mismatch($"{_path}.{name} must be a date-time");

            public DateTimeOffset? NullableInstant(string name)
            {
                var text = NullableString(name);

                if (text is null)
                    return null;

                return DateTimeOffset.TryParseExact(text, "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
                    && (text.EndsWith('Z') || text.Length > 6 && (text[^6] == '+' || text[^6] == '-'))
                    ? value
                    : throw Mismatch($"{_path}.{name} must be an ISO-8601 date-time with offset");
            }

            public decimal Decimal(string name, Regex pattern)
                => NullableDecimal(name, pattern) ?? throw Mismatch($"{_path}.{name} must be an exact decimal string");

            public decimal? NullableDecimal(string name, Regex pattern)
            {
                var text = NullableString(name);

                if (text is null)
                    return null;

                if (!pattern.IsMatch(text) || !decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value))
                    throw Mismatch($"{_path}.{name} must be an exact decimal string");

                return value;
            }

            public long Identifier(string name)
                => NullableIdentifier(name) ?? throw Mismatch($"{_path}.{name} must be a positive identifier");

            public long? NullableIdentifier(string name)
            {
                var value = Get(name);

                return value.ValueKind switch
                {
                    JsonValueKind.Null => null,
                    JsonValueKind.Number when value.TryGetInt64(out var number) && number > 0 => number,
                    _ => throw Mismatch($"{_path}.{name} must be a positive identifier or null")
                };
            }

            public int Integer(string name)
                => NullableInteger(name) ?? throw Mismatch($"{_path}.{name} must be an integer");

            public int? NullableInteger(string name)
            {
                var value = Get(name);

                return value.ValueKind switch
                {
                    JsonValueKind.Null => null,
                    JsonValueKind.Number when value.TryGetInt32(out var number) => number,
                    _ => throw Mismatch($"{_path}.{name} must be an integer or null")
                };
            }

            public bool? NullableBoolean(string name)
            {
                var value = Get(name);

                return value.ValueKind switch
                {
                    JsonValueKind.Null => null,
                    JsonValueKind.True or JsonValueKind.False => value.GetBoolean(),
                    _ => throw Mismatch($"{_path}.{name} must be a boolean or null")
                };
            }

            private JsonElement Get(string name)
                => _element.TryGetProperty(name, out var value) ? value : throw Mismatch($"{_path}.{name} is required");
        }

        private static Exception Mismatch(string reason) => ExceptionFactory.CandidateContractMismatch(reason);
    }
}
