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

        internal FarePricingUnit(long id, long fareConstructionId, CandidatePricingUnit source, Func<long> newId)
        {
            Id = id;
            FareConstructionId = fareConstructionId;
            Sequence = source.Sequence;
            Type = source.Type;
            SourceConstructionType = source.SourceConstructionType;

            foreach (var bound in source.CoveredBoundOfferIds)
                _coveredBounds.Add(new FarePricingUnitCoveredBound(id, bound));

            for (var index = 0; index < source.Components.Count; index++)
                _components.Add(new FareComponent(newId(), id, index + 1, source.Components[index]));
        }

        public long FareConstructionId { get; private set; }

        public int Sequence { get; private set; }

        public FarePricingUnitType Type { get; private set; }

        public AirFareConstructionType SourceConstructionType { get; private set; }

        public IReadOnlyCollection<FarePricingUnitCoveredBound> CoveredBounds => _coveredBounds.AsReadOnly();

        public IReadOnlyCollection<FareComponent> Components => _components.AsReadOnly();
    }
}
