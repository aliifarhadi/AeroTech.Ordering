using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class FareConstruction : Entity<long>
    {
        private FareConstruction()
        {
        }

        internal FareConstruction(
            long id,
            long orderIdAtCreation,
            long createdByChangeId,
            FareConstructionAssurance assurance,
            string sourceContextRef,
            string pricingUnitsJson)
        {
            Id = id;
            OrderIdAtCreation = orderIdAtCreation;
            CreatedByChangeId = createdByChangeId;
            Assurance = assurance;
            SourceContextRef = sourceContextRef;
            PricingUnitsJson = pricingUnitsJson;
        }

        public long OrderIdAtCreation { get; private set; }

        public long CreatedByChangeId { get; private set; }

        public long? SupersededByConstructionId { get; private set; }

        public FareConstructionAssurance Assurance { get; private set; }

        public string SourceContextRef { get; private set; } = null!;

        public string PricingUnitsJson { get; private set; } = null!;
    }
}
