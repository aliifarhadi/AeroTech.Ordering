using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class FareConstruction : Entity<long>
    {
        private readonly List<FareConstructionItem> _items = new();
        private readonly List<FarePricingGroup> _pricingGroups = new();
        private readonly List<FarePricingUnit> _pricingUnits = new();

        private FareConstruction()
        {
        }

        internal FareConstruction(
            long id,
            long orderIdAtCreation,
            long createdByChangeId,
            CandidateFareConstruction source,
            IReadOnlyDictionary<string, long> itemIds,
            IReadOnlyDictionary<string, long> travelerIds,
            IReadOnlyDictionary<string, long> serviceIds,
            IReadOnlyDictionary<string, long> segmentIds,
            Func<long> newId)
        {
            Id = id;
            OrderIdAtCreation = orderIdAtCreation;
            CreatedByChangeId = createdByChangeId;
            Assurance = source.Assurance;
            SourceContextRef = source.SourceContextRef;

            foreach (var itemId in itemIds.Values)
                _items.Add(new FareConstructionItem(newId(), id, itemId));

            for (var index = 0; index < source.PricingUnits.Count; index++)
            {
                var unit = source.PricingUnits[index];
                long? groupId = null;

                if (unit.PricingGroup is { } sourceGroup)
                {
                    var group = new FarePricingGroup(newId(), id, sourceGroup, travelerIds, newId);
                    _pricingGroups.Add(group);
                    groupId = group.Id;
                }

                _pricingUnits.Add(new FarePricingUnit(newId(), id, groupId, index + 1, unit, serviceIds, segmentIds, newId));
            }
        }

        public long OrderIdAtCreation { get; private set; }

        public long CreatedByChangeId { get; private set; }

        public long? SupersededByConstructionId { get; private set; }

        public FareConstructionAssurance Assurance { get; private set; }

        public string SourceContextRef { get; private set; } = null!;

        public IReadOnlyCollection<FareConstructionItem> Items => _items.AsReadOnly();

        public IReadOnlyCollection<FarePricingGroup> PricingGroups => _pricingGroups.AsReadOnly();

        public IReadOnlyCollection<FarePricingUnit> PricingUnits => _pricingUnits.AsReadOnly();

        public IEnumerable<FareComponent> FareComponents => _pricingUnits.SelectMany(unit => unit.Components);
    }
}
