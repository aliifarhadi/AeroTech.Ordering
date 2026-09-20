using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderAggregate.Policies;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.Policies
{
    public static class CandidateValidator
    {
        public static void EnsureValid(NormalizedCandidate candidate, AuthorizedSalesScope scope)
        {
            if (candidate.SchemaVersion != NormalizedCandidate.CurrentSchemaVersion)
                throw Mismatch($"schema version {candidate.SchemaVersion} is not {NormalizedCandidate.CurrentSchemaVersion}");

            EnsureSource(candidate);
            EnsureSalesContext(candidate.SalesContext, scope);

            var travellers = UniqueIndex(candidate.Travellers, traveller => traveller.TravellerRef, "traveller");
            var journeys = UniqueIndex(candidate.Journeys, journey => journey.BoundId, "journey");
            var segments = UniqueIndex(candidate.Segments, segment => segment.SegmentKey, "segment");
            var services = UniqueIndex(candidate.Services, service => service.ServiceKey, "service");
            var items = UniqueIndex(candidate.Items, item => item.ItemKey, "item");
            UniqueIndex(candidate.PricingLines, line => line.SourceOccurrencePath, "pricing line");

            if (candidate.Items.Count == 0)
                throw Mismatch("a candidate needs at least one item");

            if (!Enum.IsDefined(candidate.JourneyType))
                throw Mismatch("journey type is not defined");

            EnsureJourneys(candidate.Journeys);

            foreach (var segment in candidate.Segments)
                EnsureSegment(segment, journeys);

            EnsureServices(candidate.Services, travellers, segments);
            EnsureFulfillmentProfiles(candidate.Services);
            EnsureItems(candidate.Items, services);
            EnsurePricing(candidate, items, services, segments);
            EnsureFareConstruction(candidate.FareConstruction, items);
        }

        private static void EnsureSource(NormalizedCandidate candidate)
        {
            Require(candidate.Source.Owner, "source owner");
            Require(candidate.Source.OfferId, "source offer id");
            Require(candidate.Source.ProviderProfileId, "provider profile");
            Require(candidate.Source.SourcePayloadHash, "source payload hash");

            if (!Enum.IsDefined(candidate.AcceptanceAssurance))
                throw Mismatch("acceptance assurance is not defined");

            if (candidate.AcceptanceAssurance == AcceptanceAssurance.OwnerBound && string.IsNullOrWhiteSpace(candidate.Source.OwnerBindingRef))
                throw Mismatch("an owner-bound candidate requires an owner binding reference");

            if (candidate.AcceptanceAssurance == AcceptanceAssurance.LocalCandidateOnly && candidate.Source.OwnerBindingRef is not null)
                throw Mismatch("a local-only candidate cannot claim an owner binding reference");
        }

        private static void EnsureSalesContext(CandidateSalesContext context, AuthorizedSalesScope scope)
        {
            if (context.OwnerAirlineId != scope.OwnerAirlineId
                || context.FinancialCustomerId != scope.FinancialCustomerId
                || context.Sales != scope.SalesContext)
                throw Mismatch("candidate sales context differs from the authorized sales scope");

            if (context.Buyer != scope.Buyer)
                throw Mismatch("candidate buyer differs from the authorized sales scope");
        }

        private static void EnsureJourneys(IReadOnlyList<CandidateJourney> journeys)
        {
            if (journeys.Count == 0)
                throw Mismatch("a candidate needs at least one journey");

            var sequences = new HashSet<int>();

            foreach (var journey in journeys)
            {
                if (journey.Sequence < 1 || !sequences.Add(journey.Sequence))
                    throw Mismatch($"journey sequence {journey.Sequence} is not a unique positive sequence");

                if (!Enum.IsDefined(journey.Direction))
                    throw Mismatch($"journey {journey.BoundId} direction is not defined");

                if (journey.OriginAirportId <= 0 || journey.DestinationAirportId <= 0)
                    throw Mismatch($"journey {journey.BoundId} requires origin and destination airports");
            }
        }

        private static void EnsureSegment(CandidateSegment segment, IReadOnlyDictionary<string, CandidateJourney> journeys)
        {
            if (!journeys.ContainsKey(segment.BoundId))
                throw Mismatch($"segment {segment.SegmentKey} bound {segment.BoundId} is not a candidate journey");

            if (!Enum.IsDefined(segment.Kind))
                throw Mismatch($"segment {segment.SegmentKey} kind is not defined");

            if (segment.Sequence < 1)
                throw Mismatch($"segment {segment.SegmentKey} sequence must be positive");

            if (segment.OriginAirportId <= 0 || segment.DestinationAirportId <= 0)
                throw Mismatch($"segment {segment.SegmentKey} requires origin and destination airports");

            var legIds = segment.Legs.Select(leg => leg.LegId).ToList();

            if (legIds.Count != legIds.Distinct().Count())
                throw Mismatch($"segment {segment.SegmentKey} repeats an operational leg");

            var legSequences = new HashSet<int>();

            foreach (var leg in segment.Legs)
                if (leg.Sequence < 1 || !legSequences.Add(leg.Sequence))
                    throw Mismatch($"segment {segment.SegmentKey} leg sequence {leg.Sequence} is not a unique positive sequence");

            switch (segment.Kind)
            {
                case SegmentKind.ScheduledAir:
                    if (segment.SoldDeparture is null || segment.SoldArrival is null || segment.FlightId is null)
                        throw Mismatch($"scheduled segment {segment.SegmentKey} needs a dated flight");
                    break;

                case SegmentKind.OpenAir:
                    if (segment.SoldDeparture is not null || segment.SoldArrival is not null || segment.FlightId is not null)
                        throw Mismatch($"open segment {segment.SegmentKey} cannot carry a dated flight");
                    break;
            }
        }

        private static void EnsureServices(
            IReadOnlyList<CandidateService> services,
            IReadOnlyDictionary<string, CandidateTraveller> travellers,
            IReadOnlyDictionary<string, CandidateSegment> segments)
        {
            var coverage = new HashSet<(string Traveller, string Segment)>();

            foreach (var service in services)
            {
                if (!travellers.ContainsKey(service.TravellerRef))
                    throw Mismatch($"service {service.ServiceKey} traveller {service.TravellerRef} is not a candidate traveller");

                if (!segments.TryGetValue(service.SegmentKey, out var segment))
                    throw Mismatch($"service {service.ServiceKey} segment {service.SegmentKey} is not a candidate segment");

                if (segment.Kind == SegmentKind.Surface)
                    throw Mismatch($"air service {service.ServiceKey} cannot cover a surface segment");

                if (!coverage.Add((service.TravellerRef, service.SegmentKey)))
                    throw Mismatch($"traveller {service.TravellerRef} has two air services on segment {service.SegmentKey}");
            }
        }

        private static void EnsureFulfillmentProfiles(IReadOnlyList<CandidateService> services)
        {
            foreach (var service in services)
            {
                var profile = service.FulfillmentProfile;

                if (!Enum.IsDefined(profile.Assurance)
                    || !Enum.IsDefined(profile.ReservationRequirement)
                    || !Enum.IsDefined(profile.DocumentKind)
                    || !Enum.IsDefined(profile.FundingRequirement))
                    throw Mismatch($"service {service.ServiceKey} fulfillment profile carries an undefined vocabulary value");

                if (profile.DocumentAuthority is { } authority && !Enum.IsDefined(authority))
                    throw Mismatch($"service {service.ServiceKey} fulfillment profile document authority is not defined");

                var hasProfileRef = !string.IsNullOrWhiteSpace(profile.ProfileRef);
                var hasProfileVersion = !string.IsNullOrWhiteSpace(profile.ProfileVersion);

                if (hasProfileRef != hasProfileVersion)
                    throw Mismatch($"service {service.ServiceKey} fulfillment profile identity and version are supplied together or not at all");

                if (profile.Assurance == FulfillmentProfileAssurance.Certified && !hasProfileRef)
                    throw Mismatch($"service {service.ServiceKey} claims a certified fulfillment profile without naming its identity and version");

                if (profile.CapacityUnits is <= 0)
                    throw Mismatch($"service {service.ServiceKey} fulfillment profile capacity units must be positive when supplied");
            }
        }

        private static void EnsureItems(IReadOnlyList<CandidateItem> items, IReadOnlyDictionary<string, CandidateService> services)
        {
            var owner = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var item in items)
            {
                if (!Enum.IsDefined(item.ItemKind))
                    throw Mismatch($"item {item.ItemKey} kind is not defined");

                if (item.ItemKind == OrderItemKind.MonetaryCharge && item.ServiceKeys.Count > 0)
                    throw Mismatch($"monetary charge item {item.ItemKey} cannot own services");

                if (item.ItemKind != OrderItemKind.MonetaryCharge && item.ServiceKeys.Count == 0)
                    throw Mismatch($"item {item.ItemKey} needs at least one service");

                foreach (var serviceKey in item.ServiceKeys)
                {
                    if (!services.ContainsKey(serviceKey))
                        throw Mismatch($"item {item.ItemKey} service {serviceKey} is not a candidate service");

                    if (!owner.TryAdd(serviceKey, item.ItemKey))
                        throw Mismatch($"service {serviceKey} belongs to items {owner[serviceKey]} and {item.ItemKey}");
                }

                if (item.AcceptedTotal.IsNegative)
                    throw Mismatch($"item {item.ItemKey} accepted total is negative");

                DecimalRepresentation.EnsureAmount(item.AcceptedTotal.Amount, $"item {item.ItemKey} accepted total");
            }

            foreach (var serviceKey in services.Keys)
                if (!owner.ContainsKey(serviceKey))
                    throw Mismatch($"service {serviceKey} belongs to no item");
        }

        private static void EnsurePricing(
            NormalizedCandidate candidate,
            IReadOnlyDictionary<string, CandidateItem> items,
            IReadOnlyDictionary<string, CandidateService> services,
            IReadOnlyDictionary<string, CandidateSegment> segments)
        {
            var currencyId = candidate.CustomerTotal.CurrencyId;

            foreach (var line in candidate.PricingLines)
            {
                PricingLineMatrix.EnsureAllowed(line.Component, line.Effect, line.Direction, line.SettlementAttribution);

                if (line.ItemKey is not null && !items.ContainsKey(line.ItemKey))
                    throw Mismatch($"pricing line {line.SourceOccurrencePath} item {line.ItemKey} is not a candidate item");

                if (line.CalculationKind == PricingCalculationKind.NotRecorded)
                    throw Mismatch($"pricing line {line.SourceOccurrencePath} must record whether its source row was an amount or a percentage");

                if (!Enum.IsDefined(line.Role))
                    throw Mismatch($"pricing line {line.SourceOccurrencePath} role is not defined");

                if (line.Effect == PricingEffect.CustomerBalance && line.ItemKey is null && line.BasisType != PricingBasisType.OrderService)
                    throw Mismatch($"pricing line {line.SourceOccurrencePath} carries a customer balance with no item and no service basis, so its funding liability cannot be scoped");

                EnsureConversion(line);
                Require(line.BasisKey, $"pricing line {line.SourceOccurrencePath} basis reference");
                EnsureBasis(line, items, services, segments);

                PricingArithmetic.EnsureMagnitude(line.OriginalValue);
                PricingArithmetic.EnsureMagnitude(line.SaleValue);
                DecimalRepresentation.EnsureAmount(line.OriginalValue.Amount, $"pricing line {line.SourceOccurrencePath} original value");
                DecimalRepresentation.EnsureAmount(line.SaleValue.Amount, $"pricing line {line.SourceOccurrencePath} sale value");

                if (line.SaleValue.CurrencyId != currencyId)
                    throw Mismatch($"pricing line {line.SourceOccurrencePath} sale currency differs from the sale currency");

                if (line.OriginalValue.SameCurrencyAs(line.SaleValue) && line.OriginalValue.Amount != line.SaleValue.Amount)
                    throw Mismatch($"pricing line {line.SourceOccurrencePath} carries two different values in one currency");
            }

            DecimalRepresentation.EnsureAmount(candidate.CustomerTotal.Amount, "customer total");

            var total = PricingArithmetic.CustomerTotal(
                candidate.PricingLines.Select(line => new PricedAmount(line.Effect, line.Direction, line.SaleValue)),
                currencyId);

            if (total.Amount != candidate.CustomerTotal.Amount)
                throw Mismatch($"customer total {candidate.CustomerTotal.Amount} differs from the pricing lines {total.Amount}");

            if (candidate.CustomerTotal.IsNegative)
                throw Mismatch($"original sale customer total {candidate.CustomerTotal.Amount} is negative; an original sale is not a refund");

            foreach (var item in candidate.Items)
            {
                if (!item.AcceptedTotal.SameCurrencyAs(candidate.CustomerTotal))
                    throw Mismatch($"item {item.ItemKey} currency differs from the sale currency");

                var itemTotal = PricingArithmetic.CustomerTotal(
                    candidate.PricingLines
                        .Where(line => line.ItemKey == item.ItemKey)
                        .Select(line => new PricedAmount(line.Effect, line.Direction, line.SaleValue)),
                    currencyId);

                if (itemTotal.Amount != item.AcceptedTotal.Amount)
                    throw Mismatch($"item {item.ItemKey} accepted total {item.AcceptedTotal.Amount} differs from its pricing lines {itemTotal.Amount}");
            }

            EnsureServiceScopedLiability(candidate, currencyId);
        }

        private static void EnsureServiceScopedLiability(NormalizedCandidate candidate, int currencyId)
        {
            var serviceScopes = candidate.PricingLines
                .Where(line => line.Effect == PricingEffect.CustomerBalance
                    && line.ItemKey is null
                    && line.BasisType == PricingBasisType.OrderService)
                .GroupBy(line => line.BasisKey, StringComparer.Ordinal);

            foreach (var scope in serviceScopes)
            {
                var net = PricingArithmetic.CustomerTotal(
                    scope.Select(line => new PricedAmount(line.Effect, line.Direction, line.SaleValue)),
                    currencyId);

                if (net.IsNegative)
                    throw Mismatch($"service {scope.Key} carries a negative original-sale liability {net.Amount}; an original sale is not a refund");
            }
        }

        private static void EnsureConversion(CandidatePricingLine line)
        {
            if (line.AppliedConversion is not { } conversion)
                return;

            if (!string.Equals(conversion.SourceConversionRef, line.SourceConversionRef, StringComparison.Ordinal))
                throw Mismatch($"pricing line {line.SourceOccurrencePath} conversion reference differs from its source conversion reference");
        }

        private static void EnsureBasis(
            CandidatePricingLine line,
            IReadOnlyDictionary<string, CandidateItem> items,
            IReadOnlyDictionary<string, CandidateService> services,
            IReadOnlyDictionary<string, CandidateSegment> segments)
        {
            var known = line.BasisType switch
            {
                PricingBasisType.OrderItem => items.ContainsKey(line.BasisKey),
                PricingBasisType.OrderService => services.ContainsKey(line.BasisKey),
                PricingBasisType.Segment => segments.ContainsKey(line.BasisKey),
                _ => Enum.IsDefined(line.BasisType)
            };

            if (!known)
                throw Mismatch($"pricing line {line.SourceOccurrencePath} basis {line.BasisType}:{line.BasisKey} is not in the candidate");

            if (line.BasisType == PricingBasisType.OrderService && line.ItemKey is not null
                && !items[line.ItemKey].ServiceKeys.Contains(line.BasisKey, StringComparer.Ordinal))
                throw Mismatch($"pricing line {line.SourceOccurrencePath} basis service is outside its item");
        }

        private static void EnsureFareConstruction(
            CandidateFareConstruction? construction,
            IReadOnlyDictionary<string, CandidateItem> items)
        {
            if (construction is null)
                return;

            if (!Enum.IsDefined(construction.Assurance))
                throw Mismatch("fare construction assurance is not defined");

            if (construction.ItemKeys.Count == 0)
                throw Mismatch("a fare construction must name the items it prices");

            if (construction.ItemKeys.Distinct(StringComparer.Ordinal).Count() != construction.ItemKeys.Count)
                throw Mismatch("a fare construction repeats an item");

            foreach (var itemKey in construction.ItemKeys)
                if (!items.ContainsKey(itemKey))
                    throw Mismatch($"fare construction item {itemKey} is not a candidate item");

            if (construction.PricingUnits.Count == 0)
                throw Mismatch("a fare construction needs at least one pricing unit");

            var sequences = new HashSet<int>();

            foreach (var unit in construction.PricingUnits)
            {
                if (unit.Sequence < 1 || !sequences.Add(unit.Sequence))
                    throw Mismatch($"pricing unit sequence {unit.Sequence} is not a unique positive sequence");

                if (!Enum.IsDefined(unit.Type))
                    throw Mismatch($"pricing unit {unit.Sequence} type is not defined");

                if (!Enum.IsDefined(unit.SourceConstructionType))
                    throw Mismatch($"pricing unit {unit.Sequence} source construction type is not defined");

                if (unit.CoveredBoundOfferIds.Distinct(StringComparer.Ordinal).Count() != unit.CoveredBoundOfferIds.Count)
                    throw Mismatch($"pricing unit {unit.Sequence} repeats a covered bound offer id");

                foreach (var component in unit.Components)
                    if (component.AirFareId <= 0)
                        throw Mismatch($"pricing unit {unit.Sequence} fare component requires an air fare id");
            }
        }

        private static IReadOnlyDictionary<string, T> UniqueIndex<T>(IEnumerable<T> values, Func<T, string> key, string kind)
        {
            var index = new Dictionary<string, T>(StringComparer.Ordinal);

            foreach (var value in values)
            {
                var reference = key(value);
                Require(reference, $"{kind} reference");

                if (!index.TryAdd(reference, value))
                    throw Mismatch($"{kind} reference {reference} is repeated");
            }

            return index;
        }

        private static void Require(string? value, string field)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw Mismatch($"{field} is required");
        }

        private static Exception Mismatch(string reason) => ExceptionFactory.CandidateContractMismatch(reason);
    }
}
