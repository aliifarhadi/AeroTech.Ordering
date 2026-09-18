using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class FarePricingUnitCoveredBound : Entity<long>
    {
        private FarePricingUnitCoveredBound()
        {
        }

        internal FarePricingUnitCoveredBound(long id, long pricingUnitId, string sourceBoundRef)
        {
            Id = id;
            PricingUnitId = pricingUnitId;
            SourceBoundRef = sourceBoundRef;
        }

        public long PricingUnitId { get; private set; }

        public string SourceBoundRef { get; private set; } = null!;
    }
}
