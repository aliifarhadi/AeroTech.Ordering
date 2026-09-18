using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class FarePricingGroup : Entity<long>
    {
        private readonly List<FarePricingGroupTraveler> _travelers = new();

        private FarePricingGroup()
        {
        }

        internal FarePricingGroup(
            long id,
            long fareConstructionId,
            CandidatePricingGroup source,
            IReadOnlyDictionary<string, long> travelerIds,
            Func<long> newId)
        {
            Id = id;
            FareConstructionId = fareConstructionId;
            PassengerTypeCode = source.PassengerTypeCode;
            Quantity = source.Quantity;

            foreach (var travelerRef in source.TravelerRefs)
                _travelers.Add(new FarePricingGroupTraveler(newId(), id, travelerIds[travelerRef]));
        }

        public long FareConstructionId { get; private set; }

        public PassengerTypeCode PassengerTypeCode { get; private set; }

        public int Quantity { get; private set; }

        public IReadOnlyCollection<FarePricingGroupTraveler> Travelers => _travelers.AsReadOnly();
    }
}
