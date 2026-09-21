using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Policies;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;
using AeroTech.Ordering.Domain.Tests._Shared;
using Xunit;

namespace AeroTech.Ordering.Domain.Tests.OrderPreparationAggregate
{
    public sealed class SegmentStructureTests
    {
        private static readonly DateTimeOffset Now = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

        [Fact]
        public void A_scheduled_segment_with_complete_operational_facts_is_accepted()
        {
            var builder = Priced(Scheduled());

            CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope);

            var segment = Assert.Single(builder.Build().Segments);
            Assert.Equal(SegmentKind.ScheduledAir, segment.Kind);
            Assert.Equal(2, segment.Legs.Count);
        }

        [Theory]
        [InlineData("flightId")]
        [InlineData("flightVersion")]
        [InlineData("flightCapacityId")]
        [InlineData("marketingAirlineId")]
        [InlineData("operatingAirlineId")]
        [InlineData("aircraftId")]
        [InlineData("duration")]
        [InlineData("soldDeparture")]
        [InlineData("soldArrival")]
        public void A_scheduled_segment_missing_one_owner_operational_fact_is_a_contract_mismatch(string missing)
        {
            var segment = Scheduled() with
            {
                FlightId = missing == "flightId" ? null : 7001,
                FlightVersion = missing == "flightVersion" ? null : 4,
                FlightCapacityId = missing == "flightCapacityId" ? null : 5001,
                MarketingAirlineId = missing == "marketingAirlineId" ? null : 21,
                OperatingAirlineId = missing == "operatingAirlineId" ? null : 21,
                AircraftId = missing == "aircraftId" ? null : 9,
                Duration = missing == "duration" ? null : 120,
                SoldDeparture = missing == "soldDeparture" ? null : Now.AddDays(10),
                SoldArrival = missing == "soldArrival" ? null : Now.AddDays(10).AddHours(2)
            };

            AssertMismatch("scheduled segment SEG-1", Priced(segment));
        }

        [Fact]
        public void A_scheduled_segment_that_arrives_before_it_departs_is_a_contract_mismatch()
        {
            var segment = Scheduled() with
            {
                SoldDeparture = Now.AddDays(10).AddHours(2),
                SoldArrival = Now.AddDays(10)
            };

            AssertMismatch("scheduled segment SEG-1 arrives before it departs", Priced(segment));
        }

        [Fact]
        public void An_open_segment_cannot_carry_a_dated_flight()
        {
            var segment = Scheduled() with { Kind = SegmentKind.OpenAir };

            AssertMismatch("open segment SEG-1 cannot carry a dated flight", Priced(segment));
        }

        [Fact]
        public void A_leg_identity_must_be_positive()
        {
            AssertMismatch(
                "leg identity 0 is not a unique positive operational leg",
                Priced(WithLegs(Leg(0, 1), Leg(82, 2))));
        }

        [Fact]
        public void A_leg_identity_must_be_unique_inside_its_segment()
        {
            AssertMismatch(
                "leg identity 81 is not a unique positive operational leg",
                Priced(WithLegs(Leg(81, 1), Leg(81, 2))));
        }

        [Fact]
        public void A_leg_sequence_must_be_positive()
        {
            AssertMismatch(
                "leg sequence 0 is not a unique positive sequence",
                Priced(WithLegs(Leg(81, 0), Leg(82, 2))));
        }

        [Fact]
        public void A_leg_sequence_must_be_unique_inside_its_segment()
        {
            AssertMismatch(
                "leg sequence 1 is not a unique positive sequence",
                Priced(WithLegs(Leg(81, 1), Leg(82, 1))));
        }

        [Fact]
        public void A_leg_requires_a_positive_origin_airport()
        {
            AssertMismatch(
                "leg 81 requires origin and destination airports",
                Priced(WithLegs(Leg(81, 1) with { OriginAirportId = 0 })));
        }

        [Fact]
        public void A_leg_requires_a_positive_destination_airport()
        {
            AssertMismatch(
                "leg 81 requires origin and destination airports",
                Priced(WithLegs(Leg(81, 1) with { DestinationAirportId = 0 })));
        }

        [Fact]
        public void A_leg_cannot_arrive_before_it_departs()
        {
            AssertMismatch(
                "leg 81 arrives before it departs",
                Priced(WithLegs(Leg(81, 1) with
                {
                    DepartureDateTime = Now.AddDays(10).AddHours(3),
                    ArrivalDateTime = Now.AddDays(10)
                })));
        }

        [Fact]
        public void A_leg_may_omit_its_terminals()
        {
            var builder = Priced(WithLegs(Leg(81, 1) with
            {
                OriginAirportTerminalId = null,
                DestinationAirportTerminalId = null
            }));

            CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope);

            var leg = Assert.Single(Assert.Single(builder.Build().Segments).Legs);
            Assert.Null(leg.OriginAirportTerminalId);
            Assert.Null(leg.DestinationAirportTerminalId);
        }

        [Fact]
        public void A_leg_that_lands_exactly_when_it_departs_is_accepted()
        {
            var instant = Now.AddDays(10);
            var builder = Priced(WithLegs(Leg(81, 1) with { DepartureDateTime = instant, ArrivalDateTime = instant }));

            CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope);

            Assert.Single(Assert.Single(builder.Build().Segments).Legs);
        }

        [Fact]
        public void A_scheduled_segment_of_zero_duration_is_accepted()
        {
            var builder = Priced(Scheduled() with { Duration = 0 });

            CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope);

            Assert.Equal(0, Assert.Single(builder.Build().Segments).Duration);
        }

        private static CandidateSegment Scheduled()
            => new(
                "SEG-1",
                "BOUND-1",
                1,
                SegmentKind.ScheduledAir,
                11,
                null,
                22,
                null,
                Now.AddDays(10),
                Now.AddDays(10).AddHours(2),
                7001,
                "XX100",
                4,
                21,
                21,
                5001,
                120,
                9,
                [Leg(81, 1), Leg(82, 2)]);

        private static CandidateSegment WithLegs(params CandidateSegmentLeg[] legs)
            => Scheduled() with { Legs = legs };

        private static CandidateSegmentLeg Leg(long legId, int sequence)
            => new(legId, sequence, 11, 61, 22, 62, Now.AddDays(10), Now.AddDays(10).AddHours(2));

        private static CandidateBuilder Priced(CandidateSegment segment)
            => new CandidateBuilder(Now)
                .Traveller("PAX-A")
                .Segment("SEG-1", segment)
                .AirService("S-A", "PAX-A", "SEG-1")
                .Package("ITEM-A", "S-A")
                .Line("fare", "ITEM-A", PricingComponentType.Fare, 100m, "S-A");

        private static void AssertMismatch(string fragment, CandidateBuilder builder)
        {
            var exception = Assert.Throws<BusinessException>(() => CandidateValidator.EnsureValid(builder.Build(), builder.SalesScope));

            Assert.Contains(fragment, exception.Message);
        }
    }
}
