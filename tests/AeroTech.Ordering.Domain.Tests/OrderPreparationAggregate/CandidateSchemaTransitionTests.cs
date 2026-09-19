using System.Text.Json;
using AeroTech.Messages.Aegis.Enums;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Messages.Shared.Enums;
using AeroTech.Ordering.Domain._Shared.Serialization;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Serialization;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;
using AeroTech.Ordering.Domain.Tests._Shared;
using Xunit;

namespace AeroTech.Ordering.Domain.Tests.OrderPreparationAggregate
{
    public sealed class CandidateSchemaTransitionTests
    {
        private static readonly DateTimeOffset Now = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

        [Fact]
        public void The_canonicalization_algorithm_version_did_not_change_with_the_candidate_schema()
        {
            Assert.Equal("ordering-canonical-json-v1", CanonicalJson.Version);
            Assert.Equal("4.0", NormalizedCandidate.CurrentSchemaVersion);
            Assert.Equal("3.0", NormalizedCandidate.LegacySchemaVersion);
        }

        [Theory]
        [InlineData(nameof(PackExamples.NormalizedCandidate))]
        [InlineData(nameof(PackExamples.IncorrectTotal))]
        [InlineData(nameof(PackExamples.MissingBeneficiary))]
        [InlineData(nameof(PackExamples.SettlementTax))]
        public void A_retained_schema_three_candidate_keeps_the_exact_schema_three_canonical_shape(string exampleName)
        {
            var candidate = NormalizedCandidateJson.Read(Example(exampleName));
            var canonical = NormalizedCandidateJson.Write(candidate);

            Assert.Equal(NormalizedCandidate.LegacySchemaVersion, candidate.SchemaVersion);
            Assert.Equal(canonical, NormalizedCandidateJson.Write(NormalizedCandidateJson.Read(canonical)));

            using var document = JsonDocument.Parse(canonical);
            var root = document.RootElement;

            Assert.Equal(
                ["acceptanceAssurance", "capturedAt", "customerTotal", "fareConstruction", "items", "journeyType", "journeys",
                 "pricedAt", "pricingLines", "saleCurrencyCode", "salesContext", "schemaVersion", "segments", "services",
                 "source", "sourceJourneyTypeRaw", "travelers", "validity"],
                Keys(root));

            Assert.Equal(
                ["channel", "financialCustomerId", "ownerAirlineId", "sellingOfficeId"],
                Keys(root.GetProperty("salesContext")));

            foreach (var line in root.GetProperty("pricingLines").EnumerateArray())
                Assert.Equal(
                    ["appliedConversion", "basisRef", "basisType", "calculationKind", "component", "direction", "effect",
                     "itemRef", "lineRef", "lineRole", "originalValue", "saleValue", "sourceCode", "sourceConversionRef",
                     "sourceLineRef", "sourceName", "sourceReference"],
                    Keys(line));

            foreach (var service in root.GetProperty("services").EnumerateArray())
                Assert.Equal(
                    ["assurance", "capacityUnits", "documentKind", "fundingRequirement", "profileRef", "profileVersion",
                     "reservationRequirement"],
                    Keys(service.GetProperty("fulfillmentProfile")));
        }

        private static string[] Keys(JsonElement element)
            => element.EnumerateObject().Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal).ToArray();

        [Fact]
        public void A_schema_three_candidate_gains_no_invented_facts_when_it_is_read()
        {
            var candidate = NormalizedCandidateJson.Read(Example(nameof(PackExamples.NormalizedCandidate)));

            Assert.False(candidate.SalesContext.Sales.HasSeller);
            Assert.False(candidate.SalesContext.Buyer.IsSupplied);
            Assert.All(candidate.PricingLines, line => Assert.Null(line.SettlementAttribution));
            Assert.All(candidate.Services, service => Assert.Null(service.FulfillmentProfile.DocumentAuthority));
            Assert.All(candidate.Services, service => Assert.Null(service.FulfillmentProfile.PartialFulfillmentSupported));
            Assert.All(candidate.Services, service => Assert.Null(service.FulfillmentProfile.ResourceUnitPolicyRef));
        }

        [Fact]
        public void A_schema_three_office_kind_is_inferred_only_from_the_immutable_channel()
        {
            var candidate = NormalizedCandidateJson.Read(Example(nameof(PackExamples.NormalizedCandidate)));

            Assert.Equal(SalesChannel.BackOffice, candidate.SalesContext.Channel);
            Assert.Equal(SellingOfficeKind.AirlineOffice, candidate.SalesContext.SellingOfficeKind);
        }

        [Fact]
        public void A_schema_four_candidate_round_trips_and_is_byte_stable()
        {
            var candidate = Builder().Build();

            var first = NormalizedCandidateJson.Write(candidate);
            var second = NormalizedCandidateJson.Write(NormalizedCandidateJson.Read(first));

            Assert.Equal(NormalizedCandidate.CurrentSchemaVersion, candidate.SchemaVersion);
            Assert.Equal(first, second);
            Assert.Equal(NormalizedCandidateJson.Digest(candidate), CanonicalJson.Sha256Hex(second));
            Assert.Contains("\"settlementAttribution\"", first);
            Assert.Contains("\"sellingOffice\"", first);
        }

        [Fact]
        public void A_different_seller_office_or_buyer_produces_a_different_accepted_digest()
        {
            var baseline = Builder().Build();

            var otherSeller = Builder(new SalesContextSnapshot(
                SalesChannel.BackOffice, BusinessContextType.TravelAgency, 99, SellingOfficeKind.TravelAgencyOffice, 10)).Build();

            var otherOffice = Builder(new SalesContextSnapshot(
                SalesChannel.BackOffice, BusinessContextType.Airline, 1, SellingOfficeKind.AirlineOffice, 11)).Build();

            var withBuyer = Builder(buyer: new BuyerSnapshot(BusinessContextType.Individual, 5150)).Build();

            var digest = NormalizedCandidateJson.Digest(baseline);

            Assert.NotEqual(digest, NormalizedCandidateJson.Digest(otherSeller));
            Assert.NotEqual(digest, NormalizedCandidateJson.Digest(otherOffice));
            Assert.NotEqual(digest, NormalizedCandidateJson.Digest(withBuyer));
        }

        [Fact]
        public void A_settlement_attribution_changes_the_accepted_digest()
        {
            var without = Builder().Build();
            var with = Builder()
                .Line("COMM", "ITEM-A", PricingComponentType.Commission, 20m, "S-A",
                    effect: PricingEffect.SettlementOnly,
                    settlementAttribution: new SettlementAttribution("agency:77", "COMMISSION"))
                .Build();

            Assert.NotEqual(NormalizedCandidateJson.Digest(without), NormalizedCandidateJson.Digest(with));
        }

        private static CandidateBuilder Builder(SalesContextSnapshot? salesContext = null, BuyerSnapshot? buyer = null)
            => new CandidateBuilder(Now, CandidateBuilder.Scope(salesContext: salesContext, buyer: buyer))
                .Traveler("PAX-A")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1")
                .Package("ITEM-A", "S-A")
                .Line("FARE", "ITEM-A", PricingComponentType.Fare, 100m, "S-A");

        private static string Example(string name) => name switch
        {
            nameof(PackExamples.NormalizedCandidate) => PackExamples.NormalizedCandidate,
            nameof(PackExamples.IncorrectTotal) => PackExamples.IncorrectTotal,
            nameof(PackExamples.MissingBeneficiary) => PackExamples.MissingBeneficiary,
            nameof(PackExamples.SettlementTax) => PackExamples.SettlementTax,
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, "unknown pack example")
        };
    }
}
