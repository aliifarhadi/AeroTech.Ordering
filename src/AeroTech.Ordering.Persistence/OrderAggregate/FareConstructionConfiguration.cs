using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class FareConstructionConfiguration : IEntityTypeConfiguration<FareConstruction>
    {
        public void Configure(EntityTypeBuilder<FareConstruction> builder)
        {
            builder.ToTable("FareConstructions", PersistenceSchemas.Order);
            builder.HasKey(construction => construction.Id);
            builder.Property(construction => construction.Id).ValueGeneratedNever();
            builder.Property(construction => construction.SourceContextRef).HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();
            builder.HasOne<OrderChange>().WithMany().HasForeignKey(construction => construction.CreatedByChangeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<FareConstruction>().WithMany().HasForeignKey(construction => construction.SupersededByConstructionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(construction => construction.Items).WithOne().HasForeignKey(item => item.FareConstructionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(construction => construction.PricingGroups).WithOne().HasForeignKey(group => group.FareConstructionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(construction => construction.PricingUnits).WithOne().HasForeignKey(unit => unit.FareConstructionId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(construction => construction.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(construction => construction.PricingGroups).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(construction => construction.PricingUnits).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Ignore(construction => construction.FareComponents);
            builder.HasIndex(construction => construction.OrderIdAtCreation)
                .IsUnique()
                .HasFilter("[SupersededByConstructionId] IS NULL")
                .HasDatabaseName("UX_FareConstructions_CurrentPerOrder");
        }
    }
}
