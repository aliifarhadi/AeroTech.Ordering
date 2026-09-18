using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Serialization;
using AeroTech.Ordering.Domain.Tests._Shared;
using Xunit;

namespace AeroTech.Ordering.Domain.Tests.OrderPreparationAggregate
{
    public sealed class CandidateCanonicalFormTests
    {
        private const string PackExampleDigest = "2732898652d05598776a1811a5546c55aa45f8af2e782f9d1ee674c9ff9d1adb";

        [Fact]
        public void Pack_example_candidate_digest_is_reproduced_by_the_canonical_form()
        {
            var candidate = NormalizedCandidateJson.Read(PackExamples.NormalizedCandidate);

            Assert.Equal(PackExampleDigest, NormalizedCandidateJson.Digest(candidate));
        }

        [Fact]
        public void Canonical_form_round_trips_exact_decimal_strings_and_instants()
        {
            var candidate = NormalizedCandidateJson.Read(PackExamples.NormalizedCandidate);
            var written = NormalizedCandidateJson.Write(candidate);
            var reread = NormalizedCandidateJson.Read(written);

            Assert.Equal(written, NormalizedCandidateJson.Write(reread));
            Assert.Contains("\"amount\":\"120.00\"", written);
            Assert.Contains("\"value\":\"2026-10-01T10:10:00Z\"", written);
            Assert.Equal(120.00m, reread.CustomerTotal.Amount);
            Assert.Equal(2, decimal.GetBits(reread.CustomerTotal.Amount)[3] >> 16 & 0x7F);
        }

        [Fact]
        public void Property_outside_the_candidate_contract_is_rejected()
        {
            var tampered = PackExamples.NormalizedCandidate.Replace("\"schemaVersion\": \"3.0\",", "\"schemaVersion\": \"3.0\", \"unexpected\": true,");

            var exception = Assert.Throws<BusinessException>(() => NormalizedCandidateJson.Read(tampered));

            Assert.Contains("unexpected", exception.Message);
        }

        [Fact]
        public void Floating_point_amount_is_rejected()
        {
            var tampered = PackExamples.NormalizedCandidate.Replace("\"amount\": \"120.00\"", "\"amount\": 120.0");

            Assert.Throws<BusinessException>(() => NormalizedCandidateJson.Read(tampered));
        }

        [Fact]
        public void Unregistered_extension_type_is_an_unsupported_capability()
        {
            var tampered = PackExamples.NormalizedCandidate.Replace("\"type\": \"AirTransport\"", "\"type\": \"RegisteredExtension\"");

            var exception = Assert.Throws<BusinessException>(() => NormalizedCandidateJson.Read(tampered));

            Assert.Equal(20273, exception.Code);
        }
    }
}
