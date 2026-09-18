using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
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
            ("validity", Map(
                ("offer", Validity(candidate.Validity.Offer)),
                ("price", Validity(candidate.Validity.Price)),
                ("ticketing", Validity(candidate.Validity.Ticketing)))),
            ("salesContext", Map(
                ("ownerAirlineId", CanonicalJson.Identifier(candidate.SalesContext.OwnerAirlineId)),
                ("financialCustomerId", CanonicalJson.Identifier(candidate.SalesContext.FinancialCustomerId)),
                ("channel", CandidateVocabulary.Name(candidate.SalesContext.Channel)),
                ("sellingOfficeId", CanonicalJson.Identifier(candidate.SalesContext.SellingOfficeId)))),
            ("travelers", candidate.Travelers.Select(traveler => (object?)Map(
                ("sourceTravellerRef", traveler.SourceTravellerRef),
                ("passengerTypeCode", CandidateVocabulary.Name(traveler.PassengerTypeCode)))).ToList()),
            ("segments", candidate.Segments.Select(segment => (object?)Map(
                ("segmentRef", segment.SegmentRef),
                ("kind", CandidateVocabulary.Name(segment.Kind)),
                ("originRef", segment.OriginRef),
                ("destinationRef", segment.DestinationRef),
                ("soldDeparture", CanonicalJson.Instant(segment.SoldDeparture)),
                ("soldArrival", CanonicalJson.Instant(segment.SoldArrival)),
                ("flightRef", segment.FlightRef),
                ("operationalLegRefs", Strings(segment.OperationalLegRefs)))).ToList()),
            ("items", candidate.Items.Select(item => (object?)Map(
                ("itemRef", item.ItemRef),
                ("itemKind", CandidateVocabulary.Name(item.ItemKind)),
                ("sourceOfferItemRef", item.SourceOfferItemRef),
                ("serviceRefs", Strings(item.ServiceRefs)),
                ("acceptedTotal", MoneyNode(item.AcceptedTotal)))).ToList()),
            ("services", candidate.Services.Select(service => (object?)Map(
                ("serviceRef", service.ServiceRef),
                ("type", CandidateVocabulary.Name(service.Type)),
                ("beneficiaryRefs", Strings(service.BeneficiaryRefs)),
                ("segmentRefs", Strings(service.SegmentRefs)),
                ("quantity", CanonicalJson.Amount(service.Quantity)),
                ("quantityUnit", CandidateVocabulary.Name(service.QuantityUnit)),
                ("detailSchema", service.DetailSchema),
                ("detailSchemaVersion", service.DetailSchemaVersion),
                ("details", service.Details.ToDictionary(pair => pair.Key, pair => (object?)pair.Value)),
                ("fulfillmentProfile", Map(
                    ("profileRef", service.FulfillmentProfile.ProfileRef),
                    ("reservationRequirement", CandidateVocabulary.Name(service.FulfillmentProfile.ReservationRequirement)),
                    ("documentKind", CandidateVocabulary.Name(service.FulfillmentProfile.DocumentKind)),
                    ("requiresFunding", service.FulfillmentProfile.RequiresFunding),
                    ("capacityUnits", service.FulfillmentProfile.CapacityUnits))))).ToList()),
            ("pricingLines", candidate.PricingLines.Select(line => (object?)Map(
                ("lineRef", line.LineRef),
                ("itemRef", line.ItemRef),
                ("component", CandidateVocabulary.Name(line.Component)),
                ("effect", CandidateVocabulary.Name(line.Effect)),
                ("direction", CandidateVocabulary.Name(line.Direction)),
                ("lineRole", CandidateVocabulary.Name(line.LineRole)),
                ("originalValue", MoneyNode(line.OriginalValue)),
                ("saleValue", MoneyNode(line.SaleValue)),
                ("sourceLineRef", line.SourceLineRef),
                ("basisType", CandidateVocabulary.Name(line.BasisType)),
                ("basisRef", line.BasisRef),
                ("sourceConversionRef", line.SourceConversionRef))).ToList()),
            ("customerTotal", MoneyNode(candidate.CustomerTotal)),
            ("fareConstruction", Map(
                ("assurance", CandidateVocabulary.Name(candidate.FareConstruction.Assurance)),
                ("sourceContextRef", candidate.FareConstruction.SourceContextRef),
                ("pricingUnits", candidate.FareConstruction.PricingUnits.Select(unit => (object?)Map(
                    ("sourceUnitRef", unit.SourceUnitRef),
                    ("type", CandidateVocabulary.Name(unit.Type)),
                    ("combinationMethod", CandidateVocabulary.Name(unit.CombinationMethod)),
                    ("coveredSourceBoundRefs", Strings(unit.CoveredSourceBoundRefs)),
                    ("pricingGroup", unit.PricingGroup is null ? null : Map(
                        ("travelerRefs", Strings(unit.PricingGroup.TravelerRefs)),
                        ("passengerTypeCode", CandidateVocabulary.Name(unit.PricingGroup.PassengerTypeCode)),
                        ("quantity", unit.PricingGroup.Quantity))),
                    ("components", unit.Components.Select(component => (object?)Map(
                        ("sourceFareRef", component.SourceFareRef),
                        ("fareBasis", component.FareBasis),
                        ("fareFamily", component.FareFamily),
                        ("fareType", component.FareType),
                        ("cabinRef", component.CabinRef),
                        ("rbdRef", component.RbdRef),
                        ("bookingClass", component.BookingClass),
                        ("coveredServiceRefs", Strings(component.CoveredServiceRefs)))).ToList()))).ToList()))));

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
            root.Only("schemaVersion", "source", "acceptanceAssurance", "pricedAt", "capturedAt", "validity", "salesContext",
                "travelers", "segments", "items", "services", "pricingLines", "customerTotal", "fareConstruction");

            var source = root.Object("source");
            source.Only("owner", "offerId", "providerProfileId", "ownerBindingRef", "sourcePayloadHash");

            var validity = root.Object("validity");
            validity.Only("offer", "price", "ticketing");

            var sales = root.Object("salesContext");
            sales.Only("ownerAirlineId", "financialCustomerId", "channel", "sellingOfficeId");

            var construction = root.Object("fareConstruction");
            construction.Only("assurance", "sourceContextRef", "pricingUnits");

            return new NormalizedCandidate(
                root.String("schemaVersion"),
                new CandidateSource(
                    source.String("owner"),
                    source.String("offerId"),
                    source.String("providerProfileId"),
                    source.NullableString("ownerBindingRef"),
                    source.String("sourcePayloadHash")),
                root.Enum<AcceptanceAssurance>("acceptanceAssurance"),
                root.Instant("pricedAt"),
                root.Instant("capturedAt"),
                new CandidateValidity(
                    ReadValidity(validity.Object("offer")),
                    ReadValidity(validity.Object("price")),
                    ReadValidity(validity.Object("ticketing"))),
                new CandidateSalesContext(
                    sales.Identifier("ownerAirlineId"),
                    sales.Identifier("financialCustomerId"),
                    CandidateVocabulary.Parse<SalesChannel>(sales.String("channel"), "salesContext.channel"),
                    sales.NullableIdentifier("sellingOfficeId")),
                root.Array("travelers").Select(traveler =>
                {
                    traveler.Only("sourceTravellerRef", "passengerTypeCode");
                    return new CandidateTraveler(
                        traveler.String("sourceTravellerRef"),
                        CandidateVocabulary.Parse<PassengerTypeCode>(traveler.String("passengerTypeCode"), "traveler.passengerTypeCode"));
                }).ToList(),
                root.Array("segments").Select(segment =>
                {
                    segment.Only("segmentRef", "kind", "originRef", "destinationRef", "soldDeparture", "soldArrival", "flightRef", "operationalLegRefs");
                    return new CandidateSegment(
                        segment.String("segmentRef"),
                        segment.Enum<SegmentKind>("kind"),
                        segment.String("originRef"),
                        segment.String("destinationRef"),
                        segment.NullableInstant("soldDeparture"),
                        segment.NullableInstant("soldArrival"),
                        segment.NullableString("flightRef"),
                        segment.Strings("operationalLegRefs"));
                }).ToList(),
                root.Array("items").Select(item =>
                {
                    item.Only("itemRef", "itemKind", "sourceOfferItemRef", "serviceRefs", "acceptedTotal");
                    return new CandidateItem(
                        item.String("itemRef"),
                        item.Enum<OrderItemKind>("itemKind"),
                        item.NullableString("sourceOfferItemRef"),
                        item.Strings("serviceRefs"),
                        ReadMoney(item.Object("acceptedTotal")));
                }).ToList(),
                root.Array("services").Select(ReadService).ToList(),
                root.Array("pricingLines").Select(ReadPricingLine).ToList(),
                ReadMoney(root.Object("customerTotal")),
                new CandidateFareConstruction(
                    construction.Enum<FareConstructionAssurance>("assurance"),
                    construction.String("sourceContextRef"),
                    construction.Array("pricingUnits").Select(ReadPricingUnit).ToList()));
        }

        private static CandidateService ReadService(Node service)
        {
            service.Only("serviceRef", "type", "beneficiaryRefs", "segmentRefs", "quantity", "quantityUnit", "detailSchema",
                "detailSchemaVersion", "details", "fulfillmentProfile");

            var profile = service.Object("fulfillmentProfile");
            profile.Only("profileRef", "reservationRequirement", "documentKind", "requiresFunding", "capacityUnits");

            return new CandidateService(
                service.String("serviceRef"),
                service.Enum<OrderServiceType>("type"),
                service.Strings("beneficiaryRefs"),
                service.Strings("segmentRefs"),
                service.Decimal("quantity", QuantityPattern()),
                CandidateVocabulary.Parse<OrderItemUnitOfMeasure>(service.String("quantityUnit"), "service.quantityUnit"),
                service.String("detailSchema"),
                service.Integer("detailSchemaVersion"),
                service.Object("details").StringMap(),
                new CandidateFulfillmentProfile(
                    profile.String("profileRef"),
                    profile.Enum<ReservationRequirement>("reservationRequirement"),
                    profile.Enum<FulfillmentDocumentKind>("documentKind"),
                    profile.Boolean("requiresFunding"),
                    profile.Integer("capacityUnits")));
        }

        private static CandidatePricingLine ReadPricingLine(Node line)
        {
            line.Only("lineRef", "itemRef", "component", "effect", "direction", "lineRole", "originalValue", "saleValue",
                "sourceLineRef", "basisType", "basisRef", "sourceConversionRef");

            return new CandidatePricingLine(
                line.String("lineRef"),
                line.NullableString("itemRef"),
                line.Enum<PricingComponentType>("component"),
                line.Enum<PricingEffect>("effect"),
                line.Enum<OrderPricingLineDirection>("direction"),
                line.Enum<PricingLineRole>("lineRole"),
                ReadMoney(line.Object("originalValue")),
                ReadMoney(line.Object("saleValue")),
                line.String("sourceLineRef"),
                line.Enum<PricingBasisType>("basisType"),
                line.String("basisRef"),
                line.NullableString("sourceConversionRef"));
        }

        private static CandidatePricingUnit ReadPricingUnit(Node unit)
        {
            unit.Only("sourceUnitRef", "type", "combinationMethod", "coveredSourceBoundRefs", "pricingGroup", "components");

            CandidatePricingGroup? group = null;

            if (unit.NullableObject("pricingGroup") is { } groupNode)
            {
                groupNode.Only("travelerRefs", "passengerTypeCode", "quantity");
                group = new CandidatePricingGroup(
                    groupNode.Strings("travelerRefs"),
                    CandidateVocabulary.Parse<PassengerTypeCode>(groupNode.String("passengerTypeCode"), "pricingGroup.passengerTypeCode"),
                    groupNode.Integer("quantity"));
            }

            return new CandidatePricingUnit(
                unit.String("sourceUnitRef"),
                unit.Enum<FarePricingUnitType>("type"),
                unit.Enum<FareCombinationMethod>("combinationMethod"),
                unit.Strings("coveredSourceBoundRefs"),
                group,
                unit.Array("components").Select(component =>
                {
                    component.Only("sourceFareRef", "fareBasis", "fareFamily", "fareType", "cabinRef", "rbdRef", "bookingClass", "coveredServiceRefs");
                    return new CandidateFareComponent(
                        component.String("sourceFareRef"),
                        component.NullableString("fareBasis"),
                        component.NullableString("fareFamily"),
                        component.NullableString("fareType"),
                        component.NullableString("cabinRef"),
                        component.NullableString("rbdRef"),
                        component.NullableString("bookingClass"),
                        component.Strings("coveredServiceRefs"));
                }).ToList());
        }

        private static ValidityFact ReadValidity(Node node)
        {
            node.Only("state", "value", "owner", "sourceRef", "reason");

            return new ValidityFact(
                node.Enum<ValidityState>("state"),
                node.NullableInstant("value"),
                node.String("owner"),
                node.NullableString("sourceRef"),
                node.NullableString("reason"));
        }

        private static Money ReadMoney(Node node)
        {
            node.Only("amount", "currencyRef");
            return new Money(node.Decimal("amount", AmountPattern()), node.String("currencyRef"));
        }

        private static IReadOnlyDictionary<string, object?> Validity(ValidityFact fact) => Map(
            ("state", CandidateVocabulary.Name(fact.State)),
            ("value", CanonicalJson.Instant(fact.Value)),
            ("owner", fact.Owner),
            ("sourceRef", fact.SourceRef),
            ("reason", fact.Reason));

        private static IReadOnlyDictionary<string, object?> MoneyNode(Money money) => Map(
            ("amount", CanonicalJson.Amount(money.Amount)),
            ("currencyRef", money.CurrencyRef));

        private static List<object?> Strings(IEnumerable<string> values) => values.Select(value => (object?)value).ToList();

        private static IReadOnlyDictionary<string, object?> Map(params (string Key, object? Value)[] pairs)
            => pairs.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

        [GeneratedRegex(@"^(0|[1-9][0-9]*)(\.[0-9]{1,8})?$")]
        private static partial Regex AmountPattern();

        [GeneratedRegex(@"^[0-9]+(\.[0-9]{1,6})?$")]
        private static partial Regex QuantityPattern();

        [GeneratedRegex(@"^[1-9][0-9]*$")]
        private static partial Regex IdentifierPattern();

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

            public IReadOnlyDictionary<string, string> StringMap()
            {
                var path = _path;

                return _element.EnumerateObject().ToDictionary(
                    property => property.Name,
                    property => property.Value.ValueKind == JsonValueKind.String
                        ? property.Value.GetString()!
                        : throw Mismatch($"{path}.{property.Name} must be a string"),
                    StringComparer.Ordinal);
            }

            public TEnum Enum<TEnum>(string name) where TEnum : struct, Enum
                => CandidateVocabulary.Parse<TEnum>(String(name), $"{_path}.{name}");

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
            {
                var text = String(name);

                if (!pattern.IsMatch(text) || !decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value))
                    throw Mismatch($"{_path}.{name} must be an exact decimal string");

                return value;
            }

            public long Identifier(string name)
                => NullableIdentifier(name) ?? throw Mismatch($"{_path}.{name} must be an identifier string");

            public long? NullableIdentifier(string name)
            {
                var text = NullableString(name);

                if (text is null)
                    return null;

                return IdentifierPattern().IsMatch(text) && long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var value)
                    ? value
                    : throw Mismatch($"{_path}.{name} must be an identifier string");
            }

            public int Integer(string name)
            {
                var value = Get(name);
                return value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
                    ? number
                    : throw Mismatch($"{_path}.{name} must be an integer");
            }

            public bool Boolean(string name)
            {
                var value = Get(name);
                return value.ValueKind is JsonValueKind.True or JsonValueKind.False
                    ? value.GetBoolean()
                    : throw Mismatch($"{_path}.{name} must be a boolean");
            }

            private JsonElement Get(string name)
                => _element.TryGetProperty(name, out var value) ? value : throw Mismatch($"{_path}.{name} is required");
        }

        private static Exception Mismatch(string reason) => ExceptionFactory.CandidateContractMismatch(reason);
    }
}
