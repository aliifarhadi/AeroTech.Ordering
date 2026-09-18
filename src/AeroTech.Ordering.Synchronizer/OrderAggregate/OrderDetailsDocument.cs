using AeroTech.Ordering.Domain._Shared.Serialization;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderAggregate;

namespace AeroTech.Ordering.Synchronizer.OrderAggregate
{
    public static class OrderDetailsDocument
    {
        public const int SchemaVersion = 1;

        public static string Build(Order order) => CanonicalJson.Write(new Dictionary<string, object?>
        {
            ["projectionSchemaVersion"] = SchemaVersion,
            ["orderId"] = Id(order.Id),
            ["orderReference"] = order.OrderReference,
            ["ownerAirlineId"] = Id(order.OwnerAirlineId),
            ["financialCustomerId"] = Id(order.FinancialCustomerId),
            ["commercialVersion"] = order.CommercialVersion,
            ["financialSequence"] = order.FinancialSequence,
            ["orderRevision"] = order.OrderRevision,
            ["acceptedSourceDigest"] = order.AcceptedSource.SnapshotDigest,
            ["acceptanceAssurance"] = order.AcceptedSource.AcceptanceAssurance.ToString(),
            ["commercialSummary"] = order.CommercialSummary.ToString(),
            ["customerTotal"] = MoneyNode(order.CustomerTotal),
            ["salesContext"] = Map(
                ("channel", order.Channel),
                ("sellingOfficeId", CanonicalJson.Identifier(order.SellingOfficeId))),
            ["acceptedSource"] = Map(
                ("preparationId", Id(order.AcceptedSource.PreparationId)),
                ("sourceOwner", order.AcceptedSource.SourceOwner),
                ("sourceOfferId", order.AcceptedSource.SourceOfferId),
                ("providerProfileId", order.AcceptedSource.ProviderProfileId),
                ("contractVersion", order.AcceptedSource.ContractVersion),
                ("acceptanceProfile", order.AcceptedSource.AcceptanceProfile),
                ("sandboxScoped", order.AcceptedSource.IsSandboxScoped),
                ("ownerBindingRef", order.AcceptedSource.OwnerBindingRef),
                ("pricedAt", CanonicalJson.Instant(order.AcceptedSource.PricedAt)),
                ("capturedAt", CanonicalJson.Instant(order.AcceptedSource.CapturedAt)),
                ("clientAcceptedAt", CanonicalJson.Instant(order.AcceptedSource.ClientAcceptedAt)),
                ("acceptedAt", CanonicalJson.Instant(order.AcceptedSource.AcceptedAt)),
                ("validity", Map(
                    ("offer", Validity(order.OfferValidity)),
                    ("price", Validity(order.PriceValidity)),
                    ("ticketing", Validity(order.TicketingValidity))))),
            ["travelers"] = order.Travelers.OrderBy(traveler => traveler.SourceTravellerRef, StringComparer.Ordinal).Select(traveler => (object?)Map(
                ("travelerId", Id(traveler.Id)),
                ("sourceTravellerRef", traveler.SourceTravellerRef),
                ("clientTravelerRef", traveler.ClientTravelerRef),
                ("passengerTypeCode", traveler.PassengerTypeCode),
                ("infantParentTravelerId", CanonicalJson.Identifier(traveler.InfantParentTravelerId)))).ToList(),
            ["items"] = order.Items.OrderBy(item => item.Id).Select(item => (object?)Map(
                ("itemId", Id(item.Id)),
                ("sourceItemRef", item.SourceItemRef),
                ("kind", item.Kind.ToString()),
                ("sourceOfferItemRef", item.SourceOfferItemRef),
                ("commercialStatus", item.CommercialStatus.ToString()),
                ("acceptedTotal", MoneyNode(item.AcceptedTotal)),
                ("serviceIds", order.ItemServiceLinks.Where(link => link.OrderItemId == item.Id).OrderBy(link => link.OrderServiceId)
                    .Select(link => (object?)Id(link.OrderServiceId)).ToList()))).ToList(),
            ["services"] = order.Services.OrderBy(service => service.Id).Select(service => (object?)Map(
                ("serviceId", Id(service.Id)),
                ("itemId", Id(service.OrderItemId)),
                ("sourceServiceRef", service.SourceServiceRef),
                ("type", service.Type.ToString()),
                ("commercialStatus", service.CommercialStatus.ToString()),
                ("serviceVersion", service.ServiceVersion),
                ("quantity", Normalized(service.Quantity)),
                ("quantityUnit", service.QuantityUnit),
                ("detailSchema", service.DetailSchema),
                ("detailSchemaVersion", service.DetailSchemaVersion),
                ("beneficiaryTravelerIds", service.Beneficiaries.OrderBy(beneficiary => beneficiary.TravelerId).Select(beneficiary => (object?)Id(beneficiary.TravelerId)).ToList()),
                ("coveredSegmentIds", service.Coverage.OrderBy(coverage => coverage.SegmentId).Select(coverage => (object?)Id(coverage.SegmentId)).ToList()),
                ("fulfillmentProfile", Map(
                    ("profileRef", service.FulfillmentProfile.ProfileRef),
                    ("reservationRequirement", service.FulfillmentProfile.ReservationRequirement.ToString()),
                    ("documentKind", service.FulfillmentProfile.DocumentKind.ToString()),
                    ("requiresFunding", service.FulfillmentProfile.RequiresFunding),
                    ("capacityUnits", service.FulfillmentProfile.CapacityUnits))),
                ("airTransport", service.AirTransport is null ? null : Map(
                    ("cabinRef", service.AirTransport.CabinRef),
                    ("rbdRef", service.AirTransport.RbdRef),
                    ("bookingClass", service.AirTransport.BookingClass),
                    ("flightNumber", service.AirTransport.FlightNumber),
                    ("flightVersion", service.AirTransport.FlightVersion),
                    ("marketingCarrierRef", service.AirTransport.MarketingCarrierRef),
                    ("operatingCarrierRef", service.AirTransport.OperatingCarrierRef))))).ToList(),
            ["soldItinerary"] = order.Segments.OrderBy(segment => segment.Sequence).Select(segment => (object?)Map(
                ("segmentId", Id(segment.Id)),
                ("sequence", segment.Sequence),
                ("sourceSegmentRef", segment.SourceSegmentRef),
                ("kind", segment.Kind.ToString()),
                ("originRef", segment.OriginRef),
                ("destinationRef", segment.DestinationRef),
                ("soldDeparture", CanonicalJson.Instant(segment.SoldDeparture)),
                ("soldArrival", CanonicalJson.Instant(segment.SoldArrival)),
                ("flightRef", segment.FlightRef),
                ("legs", segment.Legs.OrderBy(leg => leg.Sequence).Select(leg => (object?)Map(
                    ("sequence", leg.Sequence),
                    ("sourceLegRef", leg.SourceLegRef))).ToList()))).ToList(),
            ["operationalItinerary"] = new List<object?>(),
            ["pricing"] = Map(
                ("priceChangeSets", order.PriceChangeSets.OrderBy(set => set.FinancialSequence).Select(set => (object?)Map(
                    ("setId", Id(set.Id)),
                    ("changeId", Id(set.ChangeId)),
                    ("financialSequence", set.FinancialSequence),
                    ("reason", set.Reason.ToString()),
                    ("committedAt", CanonicalJson.Instant(set.CommittedAt)))).ToList()),
                ("lines", order.PricingLines.OrderBy(line => line.Id).Select(line => (object?)Map(
                    ("lineId", Id(line.Id)),
                    ("setId", Id(line.PriceChangeSetId)),
                    ("itemId", CanonicalJson.Identifier(line.OrderItemId)),
                    ("component", line.Component.ToString()),
                    ("effect", line.Effect.ToString()),
                    ("direction", line.Direction.ToString()),
                    ("role", line.Role.ToString()),
                    ("originalValue", MoneyNode(line.OriginalValue)),
                    ("saleValue", MoneyNode(line.SaleValue)),
                    ("basisType", line.BasisType.ToString()),
                    ("basisId", CanonicalJson.Identifier(line.BasisId)),
                    ("sourceLineRef", line.SourceLineRef),
                    ("sourceConversionRef", line.SourceConversionRef))).ToList()),
                ("fareConstructions", order.FareConstructions.OrderBy(construction => construction.Id).Select(construction => (object?)Map(
                    ("constructionId", Id(construction.Id)),
                    ("assurance", construction.Assurance.ToString()),
                    ("sourceContextRef", construction.SourceContextRef))).ToList())),
            ["facets"] = Map(
                ("fundingObligations", order.FundingObligations.OrderBy(obligation => obligation.Id).Select(obligation => (object?)Map(
                    ("obligationId", Id(obligation.Id)),
                    ("version", obligation.Version),
                    ("purpose", obligation.Purpose.ToString()),
                    ("itemId", CanonicalJson.Identifier(obligation.OrderItemId)),
                    ("amount", MoneyNode(obligation.Amount)))).ToList()),
                ("capacity", new List<object?>()),
                ("documents", new List<object?>()),
                ("delivery", new List<object?>())),
            ["activeOperations"] = new List<object?>(),
            ["eligibility"] = Map(),
            ["asOf"] = CanonicalJson.Instant(order.Changes.Max(change => change.CommittedAt))
        });

        private static string Id(long value) => CanonicalJson.Identifier(value);

        private static string Normalized(decimal value) => CanonicalJson.Amount(value / 1.0000000000000000000000000000m);

        private static IReadOnlyDictionary<string, object?> MoneyNode(Money money) => Map(
            ("amount", Normalized(money.Amount)),
            ("currencyRef", money.CurrencyRef));

        private static IReadOnlyDictionary<string, object?> Validity(ValidityFact fact) => Map(
            ("state", fact.State.ToString()),
            ("value", CanonicalJson.Instant(fact.Value)),
            ("owner", fact.Owner),
            ("sourceRef", fact.SourceRef),
            ("reason", fact.Reason));

        private static IReadOnlyDictionary<string, object?> Map(params (string Key, object? Value)[] pairs)
            => pairs.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
    }
}
