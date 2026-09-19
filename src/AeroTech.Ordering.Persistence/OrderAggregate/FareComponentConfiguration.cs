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
            builder.Property(component => component.FareBasis).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(component => component.FareFamily).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(component => component.FareType).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(component => component.BookingClass).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.HasIndex(component => new { component.PricingUnitId, component.Sequence }).IsUnique();
        }
    }
}
