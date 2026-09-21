using AeroTech.Messages.Ordering.Enums;
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
            builder.ToTable("FareConstructions", PersistenceSchemas.Order, table =>
                table.RequiredEnum<FareConstructionAssurance>("FareConstructions", "Assurance"));
            builder.HasKey(construction => construction.Id);
            builder.Property(construction => construction.Id).ValueGeneratedNever();
            builder.HasOne<OrderChange>().WithMany()
                .HasForeignKey(construction => new { construction.OrderId, construction.CreatedByChangeId })
                .HasPrincipalKey(change => new { change.OrderId, change.Id })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(construction => construction.Items).WithOne().HasForeignKey(item => item.FareConstructionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(construction => construction.PricingUnits).WithOne().HasForeignKey(unit => unit.FareConstructionId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(construction => construction.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(construction => construction.PricingUnits).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasIndex(construction => construction.OrderId);
        }
    }
}
