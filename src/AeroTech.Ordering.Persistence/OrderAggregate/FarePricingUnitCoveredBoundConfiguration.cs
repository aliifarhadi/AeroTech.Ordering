using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class FarePricingUnitCoveredBoundConfiguration : IEntityTypeConfiguration<FarePricingUnitCoveredBound>
    {
        public void Configure(EntityTypeBuilder<FarePricingUnitCoveredBound> builder)
        {
            builder.ToTable("FarePricingUnitCoveredBounds", PersistenceSchemas.Order);
            builder.Property(bound => bound.CoveredBoundOfferId).HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();
            builder.HasKey(bound => new { bound.PricingUnitId, bound.CoveredBoundOfferId });
        }
    }
}
