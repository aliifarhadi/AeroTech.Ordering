namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class FarePricingUnitCoveredBound
    {
        private FarePricingUnitCoveredBound()
        {
        }

        internal FarePricingUnitCoveredBound(long pricingUnitId, string coveredBoundOfferId)
        {
            PricingUnitId = pricingUnitId;
            CoveredBoundOfferId = coveredBoundOfferId;
        }

        public long PricingUnitId { get; private set; }

        public string CoveredBoundOfferId { get; private set; } = null!;
    }
}
