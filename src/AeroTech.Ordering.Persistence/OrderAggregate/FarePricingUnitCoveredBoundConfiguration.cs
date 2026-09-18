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
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.Property(row => row.SourceBoundRef).HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();
            builder.HasIndex(row => new { row.PricingUnitId, row.SourceBoundRef }).IsUnique();
        }
    }
}
