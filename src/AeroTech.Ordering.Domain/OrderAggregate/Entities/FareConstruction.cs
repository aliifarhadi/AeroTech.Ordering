using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class FareConstruction : Entity<long>
    {
        private readonly List<FareConstructionItem> _items = new();
        private readonly List<FarePricingUnit> _pricingUnits = new();

        private FareConstruction()
        {
        }

        internal FareConstruction(
            long id,
            long orderId,
            long changeId,
            CandidateFareConstruction source,
            IReadOnlyDictionary<string, long> itemIds,
            Func<long> newId)
        {
            Id = id;
            OrderId = orderId;
            CreatedByChangeId = changeId;

            foreach (var itemId in itemIds.Values.Distinct().OrderBy(value => value))
                _items.Add(new FareConstructionItem(id, itemId));

            foreach (var unit in source.PricingUnits.OrderBy(unit => unit.Sequence))
                _pricingUnits.Add(new FarePricingUnit(newId(), id, unit, newId));
        }

        public long OrderId { get; private set; }

        public long CreatedByChangeId { get; private set; }

        public IReadOnlyCollection<FareConstructionItem> Items => _items.AsReadOnly();

        public IReadOnlyCollection<FarePricingUnit> PricingUnits => _pricingUnits.AsReadOnly();
    }
}
