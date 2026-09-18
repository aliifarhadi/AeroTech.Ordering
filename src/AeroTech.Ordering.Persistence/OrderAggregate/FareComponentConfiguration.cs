using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class FareComponentConfiguration : IEntityTypeConfiguration<FareComponent>
    {
        public void Configure(EntityTypeBuilder<FareComponent> builder)
        {
            builder.ToTable("FareComponents", PersistenceSchemas.Order, table =>
                table.HasCheckConstraint("CK_FareComponents_Sequence", "[Sequence] >= 1"));
            builder.HasKey(component => component.Id);
            builder.Property(component => component.Id).ValueGeneratedNever();
            builder.Property(component => component.SourceFareRef).HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();
            builder.Property(component => component.FareBasis).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(component => component.FareFamily).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(component => component.FareType).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(component => component.CabinRef).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(component => component.RbdRef).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(component => component.BookingClass).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(component => component.FareOwnerRef).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(component => component.TariffRef).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(component => component.RuleRef).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(component => component.RoutingRef).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.HasMany(component => component.CoveredServices).WithOne().HasForeignKey(covered => covered.FareComponentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(component => component.CoveredSegments).WithOne().HasForeignKey(covered => covered.FareComponentId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(component => component.CoveredServices).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(component => component.CoveredSegments).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasIndex(component => new { component.PricingUnitId, component.Sequence }).IsUnique();
        }
    }
}
