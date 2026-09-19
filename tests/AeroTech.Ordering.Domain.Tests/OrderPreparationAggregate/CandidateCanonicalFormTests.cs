using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Ordering.Domain._Shared.Serialization;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Serialization;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;
using AeroTech.Ordering.Domain.Tests._Shared;
using Xunit;

namespace AeroTech.Ordering.Domain.Tests.OrderPreparationAggregate
{
    public sealed class CandidateCanonicalFormTests
    {
        private const int ContractMismatch = 20272;

        private static readonly DateTimeOffset Now = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

        [Fact]
        public void The_canonicalization_algorithm_version_is_stable_and_the_schema_starts_at_one()
        {
            Assert.Equal("ordering-canonical-json-v1", CanonicalJson.Version);
            Assert.Equal("1.0", NormalizedCandidate.CurrentSchemaVersion);
        }

        [Fact]
        public void Canonical_form_round_trips_and_reproduces_the_same_digest()
        {
            var candidate = CandidateBuilder.OneWayFare100Tax20(Now).Build();

            var first = NormalizedCandidateJson.Write(candidate);
            var second = NormalizedCandidateJson.Write(NormalizedCandidateJson.Read(first));

            Assert.Equal(first, second);
            Assert.Equal(NormalizedCandidateJson.Digest(candidate), CanonicalJson.Sha256Hex(second));
        }

        [Fact]
        public void Identities_stay_numeric_in_the_canonical_form()
        {
            var canonical = NormalizedCandidateJson.Write(CandidateBuilder.OneWayFare100Tax20(Now).Build());

            Assert.Contains("\"currencyId\":978", canonical);
            Assert.Contains("\"originAirportId\":1001", canonical);
            Assert.Contains("\"destinationAirportId\":1002", canonical);
            Assert.DoesNotContain("\"currencyRef\"", canonical);
            Assert.DoesNotContain("\"originRef\"", canonical);
            Assert.DoesNotContain("\"sourceJourneyTypeRaw\"", canonical);
            Assert.DoesNotContain("\"sourceDirectionRaw\"", canonical);
            Assert.DoesNotContain("\"detailSchema\"", canonical);
        }

        [Fact]
        public void Amounts_stay_exact_decimal_strings()
        {
            var canonical = NormalizedCandidateJson.Write(CandidateBuilder.OneWayFare100Tax20(Now).Build());

            Assert.Contains("\"amount\":\"100\"", canonical);
            Assert.Contains("\"amount\":\"20\"", canonical);
        }

        [Fact]
        public void A_property_outside_the_candidate_contract_is_rejected()
        {
            var canonical = NormalizedCandidateJson.Write(CandidateBuilder.OneWayFare100Tax20(Now).Build());
            var extended = canonical.Replace("{\"acceptanceAssurance\"", "{\"unexpected\":1,\"acceptanceAssurance\"");

            Assert.NotEqual(canonical, extended);

            var exception = Assert.Throws<BusinessException>(() => NormalizedCandidateJson.Read(extended));

            Assert.Equal(ContractMismatch, exception.Code);
            Assert.Contains("is not part of the candidate contract", exception.Message);
        }

        [Fact]
        public void A_floating_point_amount_is_rejected()
        {
            var canonical = NormalizedCandidateJson.Write(CandidateBuilder.OneWayFare100Tax20(Now).Build());
            var floating = canonical.Replace("\"amount\":\"100\"", "\"amount\":100.0");

            Assert.NotEqual(canonical, floating);

            Assert.Equal(ContractMismatch, Assert.Throws<BusinessException>(() => NormalizedCandidateJson.Read(floating)).Code);
        }

        [Fact]
        public void An_unsupported_schema_version_is_rejected()
        {
            var canonical = NormalizedCandidateJson.Write(CandidateBuilder.OneWayFare100Tax20(Now).Build());
            var legacy = canonical.Replace("\"schemaVersion\":\"1.0\"", "\"schemaVersion\":\"3.0\"");

            Assert.NotEqual(canonical, legacy);

            var exception = Assert.Throws<BusinessException>(() => NormalizedCandidateJson.Read(legacy));

            Assert.Equal(ContractMismatch, exception.Code);
            Assert.Contains("is not supported", exception.Message);
        }
    }
}
