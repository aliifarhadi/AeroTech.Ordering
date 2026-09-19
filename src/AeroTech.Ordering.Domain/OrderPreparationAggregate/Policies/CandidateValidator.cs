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

            var travelers = UniqueIndex(candidate.Travelers, traveler => traveler.SourceTravellerRef, "traveler");
            var journeys = UniqueIndex(candidate.Journeys, journey => journey.JourneyRef, "journey");
            var segments = UniqueIndex(candidate.Segments, segment => segment.SegmentRef, "segment");
            var services = UniqueIndex(candidate.Services, service => service.ServiceRef, "service");
            var items = UniqueIndex(candidate.Items, item => item.ItemRef, "item");
            UniqueIndex(candidate.PricingLines, line => line.LineRef, "pricing line");

            if (candidate.Items.Count == 0)
                throw Mismatch("a candidate needs at least one item");

            EnsureJourneys(candidate.Journeys);

            foreach (var segment in candidate.Segments)
                EnsureSegment(segment, journeys);

            EnsureServices(candidate.Services, travelers, segments);
            EnsureItems(candidate.Items, services);
            EnsurePricing(candidate, items, services, segments);
            EnsureFareConstruction(candidate.FareConstruction, travelers, services, segments);
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
                || context.Sales != scope.SalesContext
                || context.Buyer != scope.Buyer)
                throw Mismatch("candidate sales context differs from the authorized sales scope");
        }

        private static void EnsureJourneys(IReadOnlyList<CandidateJourney> journeys)
        {
            if (journeys.Count == 0)
                throw Mismatch("a candidate needs at least one journey");

            var sequences = new HashSet<int>();

            foreach (var journey in journeys)
            {
                if (journey.Sequence < 1)
                    throw Mismatch($"journey {journey.JourneyRef} sequence must be positive");

                if (!sequences.Add(journey.Sequence))
                    throw Mismatch($"journey sequence {journey.Sequence} is repeated");

                Require(journey.OriginRef, $"journey {journey.JourneyRef} origin");
                Require(journey.DestinationRef, $"journey {journey.JourneyRef} destination");

                if (journey.Direction is { } direction && !Enum.IsDefined(direction))
                    throw Mismatch($"journey {journey.JourneyRef} direction is not defined");
            }
        }

        private static void EnsureSegment(CandidateSegment segment, IReadOnlyDictionary<string, CandidateJourney> journeys)
        {
            if (!journeys.ContainsKey(segment.JourneyRef))
                throw Mismatch($"segment {segment.SegmentRef} journey {segment.JourneyRef} is not a candidate journey");

            if (!Enum.IsDefined(segment.Kind))
                throw Mismatch($"segment {segment.SegmentRef} kind is not defined");

            Require(segment.OriginRef, $"segment {segment.SegmentRef} origin");
            Require(segment.DestinationRef, $"segment {segment.SegmentRef} destination");

            var legRefs = segment.Legs.Select(leg => leg.SourceLegRef).ToList();

            if (legRefs.Count != legRefs.Distinct(StringComparer.Ordinal).Count())
                throw Mismatch($"segment {segment.SegmentRef} repeats an operational leg");

            var legSequences = new HashSet<int>();

            foreach (var leg in segment.Legs)
                if (leg.Sequence < 1 || !legSequences.Add(leg.Sequence))
                    throw Mismatch($"segment {segment.SegmentRef} leg sequence {leg.Sequence} is not a unique positive sequence");

            switch (segment.Kind)
            {
                case SegmentKind.ScheduledAir:
                    if (segment.SoldDeparture is null || segment.SoldArrival is null || string.IsNullOrWhiteSpace(segment.FlightRef))
                        throw Mismatch($"scheduled segment {segment.SegmentRef} needs a dated flight");
                    break;

                case SegmentKind.OpenAir:
                    if (segment.SoldDeparture is not null || segment.SoldArrival is not null || segment.FlightRef is not null)
                        throw Mismatch($"open segment {segment.SegmentRef} cannot carry a dated flight");
                    break;
            }
        }

        private static void EnsureServices(
            IReadOnlyList<CandidateService> services,
            IReadOnlyDictionary<string, CandidateTraveler> travelers,
            IReadOnlyDictionary<string, CandidateSegment> segments)
        {
            var airCoverage = new HashSet<(string Traveler, string Segment)>();

            foreach (var service in services)
            {
                if (!Enum.IsDefined(service.Type))
                    throw ExceptionFactory.UnsupportedCapability($"service {service.ServiceRef} type is not defined");

                ServiceDetailSchemaRegistry.EnsureRegistered(service.Type, service.DetailSchema, service.DetailSchemaVersion, service.Details);

                if (service.BeneficiaryRefs.Count == 0)
                    throw Mismatch($"service {service.ServiceRef} needs a beneficiary");

                if (service.BeneficiaryRefs.Distinct(StringComparer.Ordinal).Count() != service.BeneficiaryRefs.Count)
                    throw Mismatch($"service {service.ServiceRef} repeats a beneficiary");

                foreach (var beneficiary in service.BeneficiaryRefs)
                    if (!travelers.ContainsKey(beneficiary))
                        throw Mismatch($"service {service.ServiceRef} beneficiary {beneficiary} is not a candidate traveler");

                foreach (var segment in service.SegmentRefs)
                    if (!segments.ContainsKey(segment))
                        throw Mismatch($"service {service.ServiceRef} segment {segment} is not a candidate segment");

                DecimalRepresentation.EnsureQuantity(service.Quantity, $"service {service.ServiceRef} quantity");

                if (service.Quantity <= 0)
                    throw Mismatch($"service {service.ServiceRef} quantity must be positive");

                EnsureFulfillmentProfile(service);

                if (service.Type != OrderServiceType.AirTransportation)
                    continue;

                if (service.BeneficiaryRefs.Count != 1 || service.SegmentRefs.Count != 1 || service.Quantity != 1)
                    throw Mismatch($"air service {service.ServiceRef} is exactly one traveler on one passenger segment");

                if (segments[service.SegmentRefs[0]].Kind == SegmentKind.Surface)
                    throw Mismatch($"air service {service.ServiceRef} cannot cover a surface segment");

                if (!airCoverage.Add((service.BeneficiaryRefs[0], service.SegmentRefs[0])))
                    throw Mismatch($"traveler {service.BeneficiaryRefs[0]} has two air services on segment {service.SegmentRefs[0]}");
            }
        }

        private static void EnsureFulfillmentProfile(CandidateService service)
        {
            var profile = service.FulfillmentProfile;

            Require(profile.ProfileRef, $"service {service.ServiceRef} fulfillment profile");

            Require(profile.ProfileVersion, $"service {service.ServiceRef} fulfillment profile version");

            if (!Enum.IsDefined(profile.Assurance) || !Enum.IsDefined(profile.ReservationRequirement)
                || !Enum.IsDefined(profile.DocumentKind) || !Enum.IsDefined(profile.FundingRequirement))
                throw Mismatch($"service {service.ServiceRef} fulfillment profile values are not defined");

            if (profile.CapacityUnits < 0)
                throw Mismatch($"service {service.ServiceRef} capacity units cannot be negative");

            var claimsRequirement = profile.ReservationRequirement != ReservationRequirement.Unresolved
                || profile.DocumentKind != FulfillmentDocumentKind.Unresolved
                || profile.FundingRequirement != FundingRequirement.Unresolved
                || profile.CapacityUnits is not null;

            if (profile.Assurance == FulfillmentProfileAssurance.NotCertified && claimsRequirement)
                throw Mismatch($"service {service.ServiceRef} fulfillment profile is not certified and cannot claim fulfillment requirements");

            if (profile.Assurance == FulfillmentProfileAssurance.Certified && !claimsRequirement)
                throw Mismatch($"service {service.ServiceRef} certified fulfillment profile must state its fulfillment requirements");
        }

        private static void EnsureItems(IReadOnlyList<CandidateItem> items, IReadOnlyDictionary<string, CandidateService> services)
        {
            var owner = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var item in items)
            {
                if (!Enum.IsDefined(item.ItemKind))
                    throw Mismatch($"item {item.ItemRef} kind is not defined");

                if (item.ItemKind == OrderItemKind.MonetaryCharge && item.ServiceRefs.Count > 0)
                    throw Mismatch($"monetary charge item {item.ItemRef} cannot own services");

                if (item.ItemKind != OrderItemKind.MonetaryCharge && item.ServiceRefs.Count == 0)
                    throw Mismatch($"item {item.ItemRef} needs at least one service");

                foreach (var serviceRef in item.ServiceRefs)
                {
                    if (!services.ContainsKey(serviceRef))
                        throw Mismatch($"item {item.ItemRef} service {serviceRef} is not a candidate service");

                    if (!owner.TryAdd(serviceRef, item.ItemRef))
                        throw Mismatch($"service {serviceRef} belongs to items {owner[serviceRef]} and {item.ItemRef}");
                }

                if (item.AcceptedTotal.IsNegative)
                    throw Mismatch($"item {item.ItemRef} accepted total is negative");

                DecimalRepresentation.EnsureAmount(item.AcceptedTotal.Amount, $"item {item.ItemRef} accepted total");
            }

            foreach (var serviceRef in services.Keys)
                if (!owner.ContainsKey(serviceRef))
                    throw Mismatch($"service {serviceRef} belongs to no item");
        }

        private static void EnsurePricing(
            NormalizedCandidate candidate,
            IReadOnlyDictionary<string, CandidateItem> items,
            IReadOnlyDictionary<string, CandidateService> services,
            IReadOnlyDictionary<string, CandidateSegment> segments)
        {
            var saleCurrency = candidate.CustomerTotal.CurrencyRef;

            foreach (var line in candidate.PricingLines)
            {
                PricingLineMatrix.EnsureAllowed(line.Component, line.Effect, line.Direction, line.LineRole, line.SettlementAttribution);

                if (line.LineRole != PricingLineRole.Original)
                    throw Mismatch($"pricing line {line.LineRef} of an original sale must have role Original");

                if (line.ItemRef is not null && !items.ContainsKey(line.ItemRef))
                    throw Mismatch($"pricing line {line.LineRef} item {line.ItemRef} is not a candidate item");

                if (line.CalculationKind == PricingCalculationKind.NotRecorded)
                    throw Mismatch($"pricing line {line.LineRef} must record whether its source row was an amount or a percentage");

                EnsureConversion(line);

                Require(line.SourceLineRef, $"pricing line {line.LineRef} source reference");
                Require(line.BasisRef, $"pricing line {line.LineRef} basis reference");
                EnsureBasis(line, items, services, segments);

                PricingArithmetic.EnsureMagnitude(line.OriginalValue);
                PricingArithmetic.EnsureMagnitude(line.SaleValue);
                DecimalRepresentation.EnsureAmount(line.OriginalValue.Amount, $"pricing line {line.LineRef} original value");
                DecimalRepresentation.EnsureAmount(line.SaleValue.Amount, $"pricing line {line.LineRef} sale value");

                if (!string.Equals(line.SaleValue.CurrencyRef, saleCurrency, StringComparison.Ordinal))
                    throw Mismatch($"pricing line {line.LineRef} sale currency differs from the sale currency");

                if (line.OriginalValue.SameCurrencyAs(line.SaleValue) && line.OriginalValue.Amount != line.SaleValue.Amount)
                    throw Mismatch($"pricing line {line.LineRef} carries two different values in one currency");
            }

            DecimalRepresentation.EnsureAmount(candidate.CustomerTotal.Amount, "customer total");

            var total = PricingArithmetic.CustomerTotal(
                candidate.PricingLines.Select(line => new PricedAmount(line.Effect, line.Direction, line.SaleValue)),
                saleCurrency);

            if (total.Amount != candidate.CustomerTotal.Amount)
                throw Mismatch($"customer total {candidate.CustomerTotal.Amount} differs from the pricing lines {total.Amount}");

            foreach (var item in candidate.Items)
            {
                if (!item.AcceptedTotal.SameCurrencyAs(candidate.CustomerTotal))
                    throw Mismatch($"item {item.ItemRef} currency differs from the sale currency");

                var itemTotal = PricingArithmetic.CustomerTotal(
                    candidate.PricingLines
                        .Where(line => line.ItemRef == item.ItemRef)
                        .Select(line => new PricedAmount(line.Effect, line.Direction, line.SaleValue)),
                    saleCurrency);

                if (itemTotal.Amount != item.AcceptedTotal.Amount)
                    throw Mismatch($"item {item.ItemRef} accepted total {item.AcceptedTotal.Amount} differs from its pricing lines {itemTotal.Amount}");
            }
        }

        private static void EnsureConversion(CandidatePricingLine line)
        {
            if (line.AppliedConversion is not { } conversion)
                return;

            if (!string.Equals(conversion.SourceConversionRef, line.SourceConversionRef, StringComparison.Ordinal))
                throw Mismatch($"pricing line {line.LineRef} conversion reference differs from its source conversion reference");
        }

        private static void EnsureBasis(
            CandidatePricingLine line,
            IReadOnlyDictionary<string, CandidateItem> items,
            IReadOnlyDictionary<string, CandidateService> services,
            IReadOnlyDictionary<string, CandidateSegment> segments)
        {
            var known = line.BasisType switch
            {
                PricingBasisType.OrderItem => items.ContainsKey(line.BasisRef),
                PricingBasisType.OrderService => services.ContainsKey(line.BasisRef),
                PricingBasisType.Segment => segments.ContainsKey(line.BasisRef),
                _ => Enum.IsDefined(line.BasisType)
            };

            if (!known)
                throw Mismatch($"pricing line {line.LineRef} basis {line.BasisType}:{line.BasisRef} is not in the candidate");

            if (line.BasisType == PricingBasisType.OrderService && line.ItemRef is not null
                && !items[line.ItemRef].ServiceRefs.Contains(line.BasisRef, StringComparer.Ordinal))
                throw Mismatch($"pricing line {line.LineRef} basis service is outside its item");
        }

        private static void EnsureFareConstruction(
            CandidateFareConstruction construction,
            IReadOnlyDictionary<string, CandidateTraveler> travelers,
            IReadOnlyDictionary<string, CandidateService> services,
            IReadOnlyDictionary<string, CandidateSegment> segments)
        {
            if (!Enum.IsDefined(construction.Assurance))
                throw Mismatch("fare construction assurance is not defined");

            Require(construction.SourceContextRef, "fare construction source context");
            UniqueIndex(construction.PricingUnits, unit => unit.SourceUnitRef, "pricing unit");

            foreach (var unit in construction.PricingUnits)
            {
                if (!Enum.IsDefined(unit.Type) || !Enum.IsDefined(unit.CombinationMethod))
                    throw Mismatch($"pricing unit {unit.SourceUnitRef} type or combination method is not defined");

                if (unit.PricingGroup is { } group)
                {
                    if (group.TravelerRefs.Count == 0 || group.Quantity != group.TravelerRefs.Count
                        || group.TravelerRefs.Distinct(StringComparer.Ordinal).Count() != group.TravelerRefs.Count)
                        throw Mismatch($"pricing unit {unit.SourceUnitRef} group quantity must equal its distinct travelers");

                    foreach (var traveler in group.TravelerRefs)
                    {
                        if (!travelers.TryGetValue(traveler, out var known))
                            throw Mismatch($"pricing unit {unit.SourceUnitRef} traveler {traveler} is not a candidate traveler");

                        if (known.PassengerTypeCode != group.PassengerTypeCode)
                            throw Mismatch($"pricing unit {unit.SourceUnitRef} group passenger type differs from traveler {traveler}");
                    }
                }

                foreach (var component in unit.Components)
                {
                    Require(component.SourceFareRef, $"pricing unit {unit.SourceUnitRef} fare reference");

                    foreach (var segmentRef in component.CoveredSegmentRefs)
                        if (!segments.ContainsKey(segmentRef))
                            throw Mismatch($"fare component {component.SourceFareRef} covers {segmentRef}, which is not a candidate segment");

                    if (construction.Assurance == FareConstructionAssurance.Opaque)
                    {
                        if (component.CoveredServiceRefs.Count > 0 || component.CoveredSegmentRefs.Count > 0)
                            throw Mismatch("an opaque construction cannot claim fare component coverage");

                        continue;
                    }

                    if (component.CoveredServiceRefs.Count == 0)
                        throw Mismatch($"source-provided fare component {component.SourceFareRef} needs covered services");

                    foreach (var serviceRef in component.CoveredServiceRefs)
                        if (!services.TryGetValue(serviceRef, out var service) || service.Type != OrderServiceType.AirTransportation)
                            throw Mismatch($"fare component {component.SourceFareRef} covers {serviceRef}, which is not a candidate air service");
                }
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
