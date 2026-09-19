using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using AeroTech.Messages.Aegis.Enums;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Messages.Shared.Enums;
using AeroTech.Ordering.Domain._Shared.Policies;
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

        public static IReadOnlyDictionary<string, object?> ToNode(NormalizedCandidate candidate)
            => ToNode(candidate, IsCurrentSchema(candidate.SchemaVersion));

        private static bool IsCurrentSchema(string schemaVersion) => schemaVersion switch
        {
            NormalizedCandidate.CurrentSchemaVersion => true,
            NormalizedCandidate.LegacySchemaVersion => false,
            _ => throw Mismatch($"candidate schema version {schemaVersion} is not supported")
        };

        private static IReadOnlyDictionary<string, object?> ToNode(NormalizedCandidate candidate, bool current) => Map(
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
                ("ticketing", Validity(candidate.Validity.Ticketing)),
                ("observedTicketingDeadline", ObservedNode(candidate.Validity.ObservedTicketingDeadline)))),
            ("salesContext", SalesContextNode(candidate.SalesContext, current)),
            ("travelers", candidate.Travelers.Select(traveler => (object?)Map(
                ("sourceTravellerRef", traveler.SourceTravellerRef),
                ("passengerTypeCode", CandidateVocabulary.Name(traveler.PassengerTypeCode)))).ToList()),
            ("journeys", candidate.Journeys.Select(journey => (object?)Map(
                ("journeyRef", journey.JourneyRef),
                ("sequence", journey.Sequence),
                ("sourceDirectionRaw", journey.SourceDirectionRaw),
                ("direction", OptionalName(journey.Direction)),
                ("originRef", journey.OriginRef),
                ("destinationRef", journey.DestinationRef))).ToList()),
            ("segments", candidate.Segments.Select(segment => (object?)Map(
                ("segmentRef", segment.SegmentRef),
                ("journeyRef", segment.JourneyRef),
                ("kind", CandidateVocabulary.Name(segment.Kind)),
                ("originRef", segment.OriginRef),
                ("originTerminalRef", segment.OriginTerminalRef),
                ("destinationRef", segment.DestinationRef),
                ("destinationTerminalRef", segment.DestinationTerminalRef),
                ("soldDeparture", CanonicalJson.Instant(segment.SoldDeparture)),
                ("soldArrival", CanonicalJson.Instant(segment.SoldArrival)),
                ("flightRef", segment.FlightRef),
                ("flightNumber", segment.FlightNumber),
                ("flightVersion", segment.FlightVersion),
                ("marketingCarrierRef", segment.MarketingCarrierRef),
                ("operatingCarrierRef", segment.OperatingCarrierRef),
                ("sourceCapacityRef", segment.SourceCapacityRef),
                ("duration", segment.Duration),
                ("aircraftRef", segment.AircraftRef),
                ("legs", segment.Legs.Select(leg => (object?)Map(
                    ("sourceLegRef", leg.SourceLegRef),
                    ("sequence", leg.Sequence),
                    ("originRef", leg.OriginRef),
                    ("originTerminalRef", leg.OriginTerminalRef),
                    ("destinationRef", leg.DestinationRef),
                    ("destinationTerminalRef", leg.DestinationTerminalRef),
                    ("departure", CanonicalJson.Instant(leg.Departure)),
                    ("arrival", CanonicalJson.Instant(leg.Arrival)))).ToList()))).ToList()),
            ("items", candidate.Items.Select(item => (object?)Map(
                ("itemRef", item.ItemRef),
                ("itemKind", CandidateVocabulary.Name(item.ItemKind)),
                ("sourceOfferItemRef", item.SourceOfferItemRef),
                ("serviceRefs", Strings(item.ServiceRefs)),
                ("acceptedTotal", MoneyNode(item.AcceptedTotal)),
                ("product", Map(
                    ("sourceSystem", item.Product.SourceSystem),
                    ("sourceOfferId", item.Product.SourceOfferId),
                    ("sourceOfferItemRef", item.Product.SourceOfferItemRef),
                    ("productCode", item.Product.ProductCode),
                    ("productName", item.Product.ProductName),
                    ("brandCode", item.Product.BrandCode),
                    ("brandName", item.Product.BrandName),
                    ("productVersion", item.Product.ProductVersion))))).ToList()),
            ("services", candidate.Services.Select(service => (object?)Map(
                ("serviceRef", service.ServiceRef),
                ("type", CandidateVocabulary.Name(service.Type)),
                ("serviceCode", service.ServiceCode),
                ("name", service.Name),
                ("priceTreatment", CandidateVocabulary.Name(service.PriceTreatment)),
                ("supplierPartyRef", service.SupplierPartyRef),
                ("deliveryProviderRef", service.DeliveryProviderRef),
                ("beneficiaryRefs", Strings(service.BeneficiaryRefs)),
                ("segmentRefs", Strings(service.SegmentRefs)),
                ("quantity", CanonicalJson.Amount(service.Quantity)),
                ("quantityUnit", CandidateVocabulary.Name(service.QuantityUnit)),
                ("detailSchema", service.DetailSchema),
                ("detailSchemaVersion", service.DetailSchemaVersion),
                ("details", service.Details.ToDictionary(pair => pair.Key, pair => (object?)pair.Value)),
                ("checkedBaggage", BaggageNode(service.CheckedBaggage)),
                ("cabinBaggage", BaggageNode(service.CabinBaggage)),
                ("soldTerms", Map(
                    ("refundable", service.SoldTerms.Refundable),
                    ("changeable", service.SoldTerms.Changeable),
                    ("upgradable", service.SoldTerms.Upgradable))),
                ("fulfillmentProfile", FulfillmentProfileNode(service.FulfillmentProfile, current)))).ToList()),
            ("pricingLines", candidate.PricingLines.Select(line => (object?)PricingLineNode(line, current)).ToList()),
            ("customerTotal", MoneyNode(candidate.CustomerTotal)),
            ("saleCurrencyCode", candidate.SaleCurrencyCode),
            ("sourceJourneyTypeRaw", candidate.SourceJourneyTypeRaw),
            ("journeyType", OptionalName(candidate.JourneyType)),
            ("fareConstruction", Map(
                ("assurance", CandidateVocabulary.Name(candidate.FareConstruction.Assurance)),
                ("sourceContextRef", candidate.FareConstruction.SourceContextRef),
                ("pricingUnits", candidate.FareConstruction.PricingUnits.Select(unit => (object?)Map(
                    ("sourceUnitRef", unit.SourceUnitRef),
                    ("sourceKindRaw", unit.SourceKindRaw),
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
                        ("ticketingRestrictionMinutes", component.TicketingRestrictionMinutes),
                        ("fareOwnerRef", component.FareOwnerRef),
                        ("tariffRef", component.TariffRef),
                        ("ruleRef", component.RuleRef),
                        ("routingRef", component.RoutingRef),
                        ("coveredServiceRefs", Strings(component.CoveredServiceRefs)),
                        ("coveredSegmentRefs", Strings(component.CoveredSegmentRefs)))).ToList()))).ToList()))));

        public static NormalizedCandidate Read(string json)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                var root = new Node(document.RootElement, "candidate");
                return ReadCandidate(root, IsCurrentSchema(root.String("schemaVersion")));
            }
            catch (JsonException exception)
            {
                throw ExceptionFactory.CandidateContractMismatch($"candidate is not valid JSON ({exception.Message})");
            }
        }

        private static NormalizedCandidate ReadCandidate(Node root, bool current)
        {
            root.Only("schemaVersion", "source", "acceptanceAssurance", "pricedAt", "capturedAt", "validity", "salesContext",
                "travelers", "journeys", "segments", "items", "services", "pricingLines", "customerTotal", "saleCurrencyCode",
                "sourceJourneyTypeRaw", "journeyType", "fareConstruction");

            var source = root.Object("source");
            source.Only("owner", "offerId", "providerProfileId", "ownerBindingRef", "sourcePayloadHash");

            var validity = root.Object("validity");
            validity.Only("offer", "price", "ticketing", "observedTicketingDeadline");

            var sales = root.Object("salesContext");

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
                    ReadValidity(validity.Object("ticketing")),
                    ReadObserved(validity.NullableObject("observedTicketingDeadline"))),
                ReadSalesContext(sales, current),
                root.Array("travelers").Select(traveler =>
                {
                    traveler.Only("sourceTravellerRef", "passengerTypeCode");
                    return new CandidateTraveler(
                        traveler.String("sourceTravellerRef"),
                        CandidateVocabulary.Parse<PassengerTypeCode>(traveler.String("passengerTypeCode"), "traveler.passengerTypeCode"));
                }).ToList(),
                root.Array("journeys").Select(ReadJourney).ToList(),
                root.Array("segments").Select(ReadSegment).ToList(),
                root.Array("items").Select(ReadItem).ToList(),
                root.Array("services").Select(service => ReadService(service, current)).ToList(),
                root.Array("pricingLines").Select(line => ReadPricingLine(line, current)).ToList(),
                ReadMoney(root.Object("customerTotal")),
                root.NullableString("saleCurrencyCode"),
                root.NullableString("sourceJourneyTypeRaw"),
                root.NullableEnum<JourneyType>("journeyType"),
                new CandidateFareConstruction(
                    construction.Enum<FareConstructionAssurance>("assurance"),
                    construction.String("sourceContextRef"),
                    construction.Array("pricingUnits").Select(ReadPricingUnit).ToList()));
        }

        private static CandidateJourney ReadJourney(Node journey)
        {
            journey.Only("journeyRef", "sequence", "sourceDirectionRaw", "direction", "originRef", "destinationRef");

            return new CandidateJourney(
                journey.String("journeyRef"),
                journey.Integer("sequence"),
                journey.NullableString("sourceDirectionRaw"),
                journey.NullableEnum<BoundDirection>("direction"),
                journey.String("originRef"),
                journey.String("destinationRef"));
        }

        private static CandidateSegment ReadSegment(Node segment)
        {
            segment.Only("segmentRef", "journeyRef", "kind", "originRef", "originTerminalRef", "destinationRef",
                "destinationTerminalRef", "soldDeparture", "soldArrival", "flightRef", "flightNumber", "flightVersion",
                "marketingCarrierRef", "operatingCarrierRef", "sourceCapacityRef", "duration", "aircraftRef", "legs");

            return new CandidateSegment(
                segment.String("segmentRef"),
                segment.String("journeyRef"),
                segment.Enum<SegmentKind>("kind"),
                segment.String("originRef"),
                segment.NullableString("originTerminalRef"),
                segment.String("destinationRef"),
                segment.NullableString("destinationTerminalRef"),
                segment.NullableInstant("soldDeparture"),
                segment.NullableInstant("soldArrival"),
                segment.NullableString("flightRef"),
                segment.NullableString("flightNumber"),
                segment.NullableString("flightVersion"),
                segment.NullableString("marketingCarrierRef"),
                segment.NullableString("operatingCarrierRef"),
                segment.NullableString("sourceCapacityRef"),
                segment.NullableInteger("duration"),
                segment.NullableString("aircraftRef"),
                segment.Array("legs").Select(ReadLeg).ToList());
        }

        private static CandidateSegmentLeg ReadLeg(Node leg)
        {
            leg.Only("sourceLegRef", "sequence", "originRef", "originTerminalRef", "destinationRef", "destinationTerminalRef",
                "departure", "arrival");

            return new CandidateSegmentLeg(
                leg.String("sourceLegRef"),
                leg.Integer("sequence"),
                leg.NullableString("originRef"),
                leg.NullableString("originTerminalRef"),
                leg.NullableString("destinationRef"),
                leg.NullableString("destinationTerminalRef"),
                leg.NullableInstant("departure"),
                leg.NullableInstant("arrival"));
        }

        private static CandidateItem ReadItem(Node item)
        {
            item.Only("itemRef", "itemKind", "sourceOfferItemRef", "serviceRefs", "acceptedTotal", "product");

            var product = item.Object("product");
            product.Only("sourceSystem", "sourceOfferId", "sourceOfferItemRef", "productCode", "productName", "brandCode",
                "brandName", "productVersion");

            return new CandidateItem(
                item.String("itemRef"),
                item.Enum<OrderItemKind>("itemKind"),
                item.NullableString("sourceOfferItemRef"),
                item.Strings("serviceRefs"),
                ReadMoney(item.Object("acceptedTotal")),
                new ProductSnapshot(
                    product.String("sourceSystem"),
                    product.String("sourceOfferId"),
                    product.NullableString("sourceOfferItemRef"),
                    product.NullableString("productCode"),
                    product.NullableString("productName"),
                    product.NullableString("brandCode"),
                    product.NullableString("brandName"),
                    product.NullableString("productVersion")));
        }

        private static CandidateService ReadService(Node service, bool current)
        {
            service.Only("serviceRef", "type", "serviceCode", "name", "priceTreatment", "supplierPartyRef", "deliveryProviderRef",
                "beneficiaryRefs", "segmentRefs", "quantity", "quantityUnit", "detailSchema", "detailSchemaVersion", "details",
                "checkedBaggage", "cabinBaggage", "soldTerms", "fulfillmentProfile");

            var terms = service.Object("soldTerms");
            terms.Only("refundable", "changeable", "upgradable");

            var profile = service.Object("fulfillmentProfile");

            return new CandidateService(
                service.String("serviceRef"),
                service.Enum<OrderServiceType>("type"),
                service.NullableString("serviceCode"),
                service.NullableString("name"),
                service.Enum<ServicePriceTreatment>("priceTreatment"),
                service.NullableString("supplierPartyRef"),
                service.NullableString("deliveryProviderRef"),
                service.Strings("beneficiaryRefs"),
                service.Strings("segmentRefs"),
                service.Decimal("quantity", QuantityPattern()),
                CandidateVocabulary.Parse<OrderItemUnitOfMeasure>(service.String("quantityUnit"), "service.quantityUnit"),
                service.String("detailSchema"),
                service.Integer("detailSchemaVersion"),
                service.Object("details").StringMap(),
                ReadBaggage(service.NullableObject("checkedBaggage")),
                ReadBaggage(service.NullableObject("cabinBaggage")),
                new SoldTermFlags(
                    terms.NullableBoolean("refundable"),
                    terms.NullableBoolean("changeable"),
                    terms.NullableBoolean("upgradable")),
                ReadFulfillmentProfile(profile, current));
        }

        private static CandidatePricingLine ReadPricingLine(Node line, bool current)
        {
            string[] legacyNames = ["lineRef", "itemRef", "component", "effect", "direction", "lineRole", "sourceCode", "sourceName",
                "sourceReference", "calculationKind", "originalValue", "saleValue", "sourceLineRef", "basisType", "basisRef",
                "sourceConversionRef", "appliedConversion"];

            line.Only(current ? [.. legacyNames, "settlementAttribution"] : legacyNames);

            return new CandidatePricingLine(
                line.String("lineRef"),
                line.NullableString("itemRef"),
                line.Enum<PricingComponentType>("component"),
                line.Enum<PricingEffect>("effect"),
                line.Enum<OrderPricingLineDirection>("direction"),
                line.Enum<PricingLineRole>("lineRole"),
                line.NullableString("sourceCode"),
                line.NullableString("sourceName"),
                line.NullableString("sourceReference"),
                line.Enum<PricingCalculationKind>("calculationKind"),
                ReadMoney(line.Object("originalValue")),
                ReadMoney(line.Object("saleValue")),
                line.String("sourceLineRef"),
                line.Enum<PricingBasisType>("basisType"),
                line.String("basisRef"),
                line.NullableString("sourceConversionRef"),
                ReadConversion(line.NullableObject("appliedConversion")),
                current ? ReadAttribution(line.NullableObject("settlementAttribution")) : null);
        }

        private static CandidatePricingUnit ReadPricingUnit(Node unit)
        {
            unit.Only("sourceUnitRef", "sourceKindRaw", "type", "combinationMethod", "coveredSourceBoundRefs", "pricingGroup", "components");

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
                unit.NullableString("sourceKindRaw"),
                unit.Enum<FarePricingUnitType>("type"),
                unit.Enum<FareCombinationMethod>("combinationMethod"),
                unit.Strings("coveredSourceBoundRefs"),
                group,
                unit.Array("components").Select(ReadFareComponent).ToList());
        }

        private static CandidateFareComponent ReadFareComponent(Node component)
        {
            component.Only("sourceFareRef", "fareBasis", "fareFamily", "fareType", "cabinRef", "rbdRef", "bookingClass",
                "ticketingRestrictionMinutes", "fareOwnerRef", "tariffRef", "ruleRef", "routingRef", "coveredServiceRefs",
                "coveredSegmentRefs");

            return new CandidateFareComponent(
                component.String("sourceFareRef"),
                component.NullableString("fareBasis"),
                component.NullableString("fareFamily"),
                component.NullableString("fareType"),
                component.NullableString("cabinRef"),
                component.NullableString("rbdRef"),
                component.NullableString("bookingClass"),
                component.NullableInteger("ticketingRestrictionMinutes"),
                component.NullableString("fareOwnerRef"),
                component.NullableString("tariffRef"),
                component.NullableString("ruleRef"),
                component.NullableString("routingRef"),
                component.Strings("coveredServiceRefs"),
                component.Strings("coveredSegmentRefs"));
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

            conversion.Only("sourceConversionRef", "fromCurrencyRef", "toCurrencyRef", "rate", "decimalPlaces", "roundingToken");

            return new AppliedConversion(
                conversion.String("sourceConversionRef"),
                conversion.String("fromCurrencyRef"),
                conversion.String("toCurrencyRef"),
                conversion.Decimal("rate", RatePattern()),
                conversion.Integer("decimalPlaces"),
                conversion.NullableString("roundingToken"));
        }

        private static ObservedTimeFact? ReadObserved(Node? node)
        {
            if (node is not { } observed)
                return null;

            observed.Only("value", "sourceOwner", "sourceRef");

            return new ObservedTimeFact(
                observed.Instant("value"),
                observed.String("sourceOwner"),
                observed.String("sourceRef"));
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

        private static IReadOnlyDictionary<string, object?> PricingLineNode(CandidatePricingLine line, bool current)
        {
            var common = new (string Key, object? Value)[]
            {
                ("lineRef", line.LineRef),
                ("itemRef", line.ItemRef),
                ("component", CandidateVocabulary.Name(line.Component)),
                ("effect", CandidateVocabulary.Name(line.Effect)),
                ("direction", CandidateVocabulary.Name(line.Direction)),
                ("lineRole", CandidateVocabulary.Name(line.LineRole)),
                ("sourceCode", line.SourceCode),
                ("sourceName", line.SourceName),
                ("sourceReference", line.SourceReference),
                ("calculationKind", CandidateVocabulary.Name(line.CalculationKind)),
                ("originalValue", MoneyNode(line.OriginalValue)),
                ("saleValue", MoneyNode(line.SaleValue)),
                ("sourceLineRef", line.SourceLineRef),
                ("basisType", CandidateVocabulary.Name(line.BasisType)),
                ("basisRef", line.BasisRef),
                ("sourceConversionRef", line.SourceConversionRef),
                ("appliedConversion", ConversionNode(line.AppliedConversion))
            };

            return current
                ? Map([.. common, ("settlementAttribution", (object?)AttributionNode(line.SettlementAttribution))])
                : Map(common);
        }

        private static IReadOnlyDictionary<string, object?> SalesContextNode(CandidateSalesContext context, bool current)
        {
            if (!current)
                return Map(
                    ("ownerAirlineId", CanonicalJson.Identifier(context.OwnerAirlineId)),
                    ("financialCustomerId", CanonicalJson.Identifier(context.FinancialCustomerId)),
                    ("channel", CandidateVocabulary.Name(context.Channel)),
                    ("sellingOfficeId", CanonicalJson.Identifier(context.SellingOfficeId)));

            return Map(
                ("ownerAirlineId", CanonicalJson.Identifier(context.OwnerAirlineId)),
                ("financialCustomerId", CanonicalJson.Identifier(context.FinancialCustomerId)),
                ("channel", CandidateVocabulary.Name(context.Channel)),
                ("seller", PartyNode(context.Sales.SellerContextType, context.Sales.SellerId)),
                ("sellingOffice", context.Sales.HasSellingOffice
                    ? Map(
                        ("kind", CandidateVocabulary.Name(context.Sales.SellingOfficeKind!.Value)),
                        ("officeId", CanonicalJson.Identifier(context.Sales.SellingOfficeId!.Value)))
                    : null),
                ("buyer", PartyNode(context.Buyer.ContextType, context.Buyer.BuyerId)));
        }

        private static IReadOnlyDictionary<string, object?>? PartyNode(BusinessContextType? contextType, long? partyId)
            => partyId is null || contextType is null
                ? null
                : Map(
                    ("contextType", CandidateVocabulary.Name(contextType.Value)),
                    ("partyId", CanonicalJson.Identifier(partyId.Value)));

        private static CandidateSalesContext ReadSalesContext(Node sales, bool current)
        {
            if (!current)
            {
                sales.Only("ownerAirlineId", "financialCustomerId", "channel", "sellingOfficeId");

                var legacyChannel = CandidateVocabulary.Parse<SalesChannel>(sales.String("channel"), "salesContext.channel");
                var legacyOfficeId = sales.NullableIdentifier("sellingOfficeId");

                return new CandidateSalesContext(
                    sales.Identifier("ownerAirlineId"),
                    sales.Identifier("financialCustomerId"),
                    new SalesContextSnapshot(
                        legacyChannel,
                        null,
                        null,
                        LegacySellingOfficePolicy.KindOf(legacyChannel, legacyOfficeId),
                        legacyOfficeId),
                    BuyerSnapshot.NotSupplied);
            }

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
                new BuyerSnapshot(
                    buyer?.Enum<BusinessContextType>("contextType"),
                    buyer?.Identifier("partyId")));
        }

        private static IReadOnlyDictionary<string, object?> FulfillmentProfileNode(CandidateFulfillmentProfile profile, bool current)
        {
            if (!current)
                return Map(
                    ("profileRef", profile.ProfileRef),
                    ("profileVersion", profile.ProfileVersion),
                    ("assurance", CandidateVocabulary.Name(profile.Assurance)),
                    ("reservationRequirement", CandidateVocabulary.Name(profile.ReservationRequirement)),
                    ("documentKind", CandidateVocabulary.Name(profile.DocumentKind)),
                    ("fundingRequirement", CandidateVocabulary.Name(profile.FundingRequirement)),
                    ("capacityUnits", profile.CapacityUnits));

            return Map(
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
        }

        private static CandidateFulfillmentProfile ReadFulfillmentProfile(Node profile, bool current)
        {
            if (!current)
            {
                profile.Only("profileRef", "profileVersion", "assurance", "reservationRequirement", "documentKind",
                    "fundingRequirement", "capacityUnits");

                return new CandidateFulfillmentProfile(
                    profile.String("profileRef"),
                    profile.String("profileVersion"),
                    profile.Enum<FulfillmentProfileAssurance>("assurance"),
                    profile.Enum<ReservationRequirement>("reservationRequirement"),
                    profile.Enum<FulfillmentDocumentKind>("documentKind"),
                    null,
                    profile.Enum<FundingRequirement>("fundingRequirement"),
                    profile.NullableInteger("capacityUnits"),
                    null,
                    null,
                    null,
                    null);
            }

            profile.Only("profileRef", "profileVersion", "assurance", "reservationRequirement", "documentKind",
                "documentAuthority", "fundingRequirement", "capacityUnits", "resourceUnitPolicyRef",
                "deliveryControlPolicyRef", "dependencyTreatmentPolicyRef", "partialFulfillmentSupported");

            return new CandidateFulfillmentProfile(
                profile.String("profileRef"),
                profile.String("profileVersion"),
                profile.Enum<FulfillmentProfileAssurance>("assurance"),
                profile.Enum<ReservationRequirement>("reservationRequirement"),
                profile.Enum<FulfillmentDocumentKind>("documentKind"),
                profile.NullableEnum<DocumentAuthority>("documentAuthority"),
                profile.Enum<FundingRequirement>("fundingRequirement"),
                profile.NullableInteger("capacityUnits"),
                profile.NullableString("resourceUnitPolicyRef"),
                profile.NullableString("deliveryControlPolicyRef"),
                profile.NullableString("dependencyTreatmentPolicyRef"),
                profile.NullableBoolean("partialFulfillmentSupported"));
        }

        private static IReadOnlyDictionary<string, object?>? AttributionNode(SettlementAttribution? attribution) => attribution is null ? null : Map(
            ("partyRef", attribution.PartyRef),
            ("categoryCode", attribution.CategoryCode));

        private static SettlementAttribution? ReadAttribution(Node? node)
        {
            if (node is not { } attribution)
                return null;

            attribution.Only("partyRef", "categoryCode");

            return new SettlementAttribution(attribution.String("partyRef"), attribution.String("categoryCode"));
        }

        private static IReadOnlyDictionary<string, object?>? ObservedNode(ObservedTimeFact? fact) => fact is null ? null : Map(
            ("value", CanonicalJson.Instant(fact.Value)),
            ("sourceOwner", fact.SourceOwner),
            ("sourceRef", fact.SourceRef));

        private static IReadOnlyDictionary<string, object?>? BaggageNode(BaggageAllowance? allowance) => allowance is null ? null : Map(
            ("pieces", allowance.Pieces),
            ("weight", allowance.Weight is null ? null : CanonicalJson.Amount(allowance.Weight.Value)),
            ("weightUnit", OptionalName(allowance.WeightUnit)));

        private static IReadOnlyDictionary<string, object?>? ConversionNode(AppliedConversion? conversion) => conversion is null ? null : Map(
            ("sourceConversionRef", conversion.SourceConversionRef),
            ("fromCurrencyRef", conversion.FromCurrencyRef),
            ("toCurrencyRef", conversion.ToCurrencyRef),
            ("rate", CanonicalJson.Amount(conversion.Rate)),
            ("decimalPlaces", conversion.DecimalPlaces),
            ("roundingToken", conversion.RoundingToken));

        private static IReadOnlyDictionary<string, object?> MoneyNode(Money money) => Map(
            ("amount", CanonicalJson.Amount(money.Amount)),
            ("currencyRef", money.CurrencyRef));

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

            public bool Boolean(string name)
                => NullableBoolean(name) ?? throw Mismatch($"{_path}.{name} must be a boolean");

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
