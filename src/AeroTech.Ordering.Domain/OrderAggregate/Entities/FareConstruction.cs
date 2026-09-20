using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;
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
            Assurance = source.Assurance;

            if (source.ItemKeys.Count == 0)
                throw ExceptionFactory.CandidateContractMismatch("a fare construction must name the items it prices");

            foreach (var itemKey in source.ItemKeys)
            {
                if (!itemIds.TryGetValue(itemKey, out var itemId))
                    throw ExceptionFactory.CandidateContractMismatch($"fare construction item {itemKey} is not a candidate item");

                _items.Add(new FareConstructionItem(id, itemId));
            }

            foreach (var unit in source.PricingUnits.OrderBy(unit => unit.Sequence))
                _pricingUnits.Add(new FarePricingUnit(newId(), id, unit, newId));
        }

        public long OrderId { get; private set; }

        public long CreatedByChangeId { get; private set; }

        public FareConstructionAssurance Assurance { get; private set; }

        public IReadOnlyCollection<FareConstructionItem> Items => _items.AsReadOnly();

        public IReadOnlyCollection<FarePricingUnit> PricingUnits => _pricingUnits.AsReadOnly();
    }
}
