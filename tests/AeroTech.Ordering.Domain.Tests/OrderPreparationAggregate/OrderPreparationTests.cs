using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Arguments;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Contracts;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;
using AeroTech.Ordering.Domain.Ports.Offers;
using AeroTech.Ordering.Domain.Tests._Shared;
using Xunit;

namespace AeroTech.Ordering.Domain.Tests.OrderPreparationAggregate
{
    public sealed class OrderPreparationTests
    {
        private const string SandboxProfile = "LIVE-CANDIDATE-SANDBOX";
        private static readonly DateTimeOffset Now = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

        [Fact]
        public void Sandbox_candidate_keeps_not_supplied_validity_and_reports_live_acceptance_blocked()
        {
            var preparation = Capture(CandidateBuilder.OneWayFare100Tax20(Now).LocalCandidateOnly(SandboxProfile), SandboxProfile);

            Assert.Equal(ValidityState.NotSupplied, preparation.OfferValidity.State);
            Assert.Equal(ValidityState.NotSupplied, preparation.PriceValidity.State);
            Assert.Null(preparation.OfferValidity.Value);
            Assert.Equal(AcceptanceAssurance.LocalCandidateOnly, preparation.AcceptanceAssurance);
        }

        [Fact]
        public void Sandbox_profile_is_refused_where_the_deployment_does_not_permit_it()
        {
            var preparation = Capture(CandidateBuilder.OneWayFare100Tax20(Now).LocalCandidateOnly(SandboxProfile), SandboxProfile);
            var production = new Policy();

            AssertCode(20269, () => preparation.EnsureAcceptable(production, Now));
            preparation.EnsureAcceptable(new Policy(SandboxProfile), Now.AddDays(30));
        }

        [Fact]
        public void Owner_bound_acceptance_needs_known_validity()
        {
            var builder = CandidateBuilder.OneWayFare100Tax20(Now).Validity(
                new ValidityFact(ValidityState.NotSupplied, null, "AirOffer", null, "missing"),
                new ValidityFact(ValidityState.Known, Now.AddMinutes(5), "AirPrice", "P", null),
                new ValidityFact(ValidityState.NotSupplied, null, "Unresolved owner", null, "missing"));
            var preparation = Capture(builder, CandidateBuilder.ReferenceProfile);

            AssertCode(20271, () => preparation.EnsureAcceptable(new Policy(CandidateBuilder.ReferenceProfile), Now));
        }

        [Fact]
        public void Expiry_is_evaluated_at_now_greater_or_equal_due()
        {
            var preparation = Capture(CandidateBuilder.OneWayFare100Tax20(Now), CandidateBuilder.ReferenceProfile);
            var policy = new Policy(CandidateBuilder.ReferenceProfile);

            preparation.EnsureAcceptable(policy, Now.AddMinutes(5).AddTicks(-1));
            AssertCode(20270, () => preparation.EnsureAcceptable(policy, Now.AddMinutes(5)));
        }

        [Fact]
        public void A_second_consumption_of_the_same_accepted_source_is_a_conflict()
        {
            var preparation = Capture(CandidateBuilder.OneWayFare100Tax20(Now), CandidateBuilder.ReferenceProfile);
            var policy = new Policy(CandidateBuilder.ReferenceProfile);

            preparation.Consume(5001, Now);

            AssertCode(20267, () => preparation.EnsureAcceptable(policy, Now));
            AssertCode(20267, () => preparation.Consume(5002, Now));
            Assert.Equal(5001, preparation.ConsumedByOrderId);
        }

        [Fact]
        public void Digest_binds_the_authorized_scope_as_well_as_the_candidate()
        {
            var first = Capture(CandidateBuilder.OneWayFare100Tax20(Now), CandidateBuilder.ReferenceProfile);
            var otherActor = CandidateBuilder.Scope(callerScope: "customer:100/actor:2");
            var second = Capture(CandidateBuilder.OneWayFare100Tax20(Now, otherActor), CandidateBuilder.ReferenceProfile);

            Assert.Equal(first.CandidateJson, second.CandidateJson);
            Assert.NotEqual(first.SnapshotDigest, second.SnapshotDigest);
            Assert.Matches("^[a-f0-9]{64}$", first.SnapshotDigest);
        }

        public static OrderPreparation Capture(CandidateBuilder builder, string profile)
        {
            var candidate = builder.Build();

            return OrderPreparation.Capture(new CaptureOrderPreparationArgs(
                1001,
                1002,
                builder.SalesScope,
                candidate,
                new OfferSourceProfile(profile, "contract-1", profile),
                new SourceEvidence("evidence-1", candidate.Source.SourcePayloadHash, "application/json", "{}"),
                null,
                Now));
        }

        private static void AssertCode(int code, Action action)
        {
            var exception = Assert.Throws<BusinessException>(action);
            Assert.Equal(code, exception.Code);
        }

        private sealed class Policy : IAcceptanceProfilePolicy
        {
            private readonly HashSet<string> _permitted;

            public Policy(params string[] permitted) => _permitted = new HashSet<string>(permitted);

            public string EnvironmentClass => "Test";

            public bool Permits(string acceptanceProfile) => _permitted.Contains(acceptanceProfile);
        }
    }
}
