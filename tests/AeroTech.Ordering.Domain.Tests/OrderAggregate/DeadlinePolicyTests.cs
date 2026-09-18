using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderAggregate.Policies;
using Xunit;

namespace AeroTech.Ordering.Domain.Tests.OrderAggregate
{
    public sealed class DeadlinePolicyTests
    {
        private static readonly DateTimeOffset T = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

        private static readonly ValidityFact Offer = new(ValidityState.Known, T.AddMinutes(10), "AirOffer", "OFFER-1", null);
        private static readonly ValidityFact Price = new(ValidityState.Known, T.AddMinutes(5), "AirPrice", "PRICE-1", null);
        private static readonly ValidityFact Hold = new(ValidityState.Known, T.AddMinutes(20), "FlightFlow", "HOLD-1", null);
        private static readonly ValidityFact Ticketing = new(ValidityState.NotSupplied, null, "Unresolved owner", null, "BD-004");

        [Fact]
        public void Independent_deadlines_keep_their_owners_and_the_earliest_is_only_a_display()
        {
            ValidityFact[] facts = [Offer, Price, Hold, Ticketing];

            var earliest = DeadlinePolicy.EarliestKnown(facts);

            Assert.Same(Price, earliest);
            Assert.Equal(["AirOffer", "AirPrice", "FlightFlow", "Unresolved owner"], facts.Select(fact => fact.Owner));
            Assert.Single(DeadlinePolicy.Unestablished(facts));
            Assert.Null(Ticketing.Value);
        }

        [Fact]
        public void Each_fact_expires_at_its_own_instant()
        {
            ValidityFact[] facts = [Offer, Price, Hold, Ticketing];

            Assert.Empty(DeadlinePolicy.ExpiredAt(facts, T.AddMinutes(5).AddTicks(-1)));
            Assert.Equal([Price], DeadlinePolicy.ExpiredAt(facts, T.AddMinutes(5)));
            Assert.Equal([Offer, Price], DeadlinePolicy.ExpiredAt(facts, T.AddMinutes(10)));
            Assert.Equal([Offer, Price, Hold], DeadlinePolicy.ExpiredAt(facts, T.AddYears(1)));
        }

        [Fact]
        public void Not_supplied_validity_never_carries_a_value_or_expires()
        {
            Assert.False(Ticketing.IsExpiredAt(DateTimeOffset.MaxValue));
            Assert.Throws<AeroTech.Framework.Core.Domain.Exceptions.BusinessException>(() =>
                new ValidityFact(ValidityState.NotSupplied, T, "AirOffer", null, "invented"));
        }
    }
}
