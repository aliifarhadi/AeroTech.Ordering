using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class FarePricingUnitConfiguration : IEntityTypeConfiguration<FarePricingUnit>
    {
        public void Configure(EntityTypeBuilder<FarePricingUnit> builder)
        {
            builder.ToTable("FarePricingUnits", PersistenceSchemas.Order, table =>
                table.HasCheckConstraint("CK_FarePricingUnits_Sequence", "[Sequence] >= 1"));
            builder.HasKey(unit => unit.Id);
            builder.Property(unit => unit.Id).ValueGeneratedNever();
            builder.HasMany(unit => unit.CoveredBounds).WithOne().HasForeignKey(bound => bound.PricingUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(unit => unit.Components).WithOne().HasForeignKey(component => component.PricingUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(unit => unit.CoveredBounds).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(unit => unit.Components).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasIndex(unit => new { unit.FareConstructionId, unit.Sequence }).IsUnique();
        }
    }
}
