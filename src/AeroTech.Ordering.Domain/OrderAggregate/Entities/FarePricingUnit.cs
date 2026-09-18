using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class FarePricingUnit : Entity<long>
    {
        private readonly List<FarePricingUnitCoveredBound> _coveredBounds = new();
        private readonly List<FareComponent> _components = new();

        private FarePricingUnit()
        {
        }

        internal FarePricingUnit(
            long id,
            long fareConstructionId,
            long? pricingGroupId,
            int sequence,
            CandidatePricingUnit source,
            IReadOnlyDictionary<string, long> serviceIds,
            IReadOnlyDictionary<string, long> segmentIds,
            Func<long> newId)
        {
            Id = id;
            FareConstructionId = fareConstructionId;
            PricingGroupId = pricingGroupId;
            Sequence = sequence;
            SourceUnitRef = source.SourceUnitRef;
            SourceKindRaw = source.SourceKindRaw;
            Type = source.Type;
            CombinationMethod = source.CombinationMethod;

            foreach (var boundRef in source.CoveredSourceBoundRefs)
                _coveredBounds.Add(new FarePricingUnitCoveredBound(newId(), id, boundRef));

            for (var index = 0; index < source.Components.Count; index++)
                _components.Add(new FareComponent(newId(), id, index + 1, source.Components[index], serviceIds, segmentIds, newId));
        }

        public long FareConstructionId { get; private set; }

        public long? PricingGroupId { get; private set; }

        public int Sequence { get; private set; }

        public string SourceUnitRef { get; private set; } = null!;

        public string? SourceKindRaw { get; private set; }

        public FarePricingUnitType Type { get; private set; }

        public FareCombinationMethod CombinationMethod { get; private set; }

        public IReadOnlyCollection<FarePricingUnitCoveredBound> CoveredBounds => _coveredBounds.AsReadOnly();

        public IReadOnlyCollection<FareComponent> Components => _components.AsReadOnly();
    }
}
