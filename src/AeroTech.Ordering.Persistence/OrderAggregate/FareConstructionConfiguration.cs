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
            builder.ToTable("FareConstructions", PersistenceSchemas.Order);
            builder.HasKey(construction => construction.Id);
            builder.Property(construction => construction.Id).ValueGeneratedNever();
            builder.Property(construction => construction.SourceContextRef).HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();
            builder.Property(construction => construction.PricingUnitsJson).HasColumnType("nvarchar(max)").IsRequired();
            builder.HasOne<OrderChange>().WithMany().HasForeignKey(construction => construction.CreatedByChangeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<FareConstruction>().WithMany().HasForeignKey(construction => construction.SupersededByConstructionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(construction => construction.OrderIdAtCreation)
                .IsUnique()
                .HasFilter("[SupersededByConstructionId] IS NULL")
                .HasDatabaseName("UX_FareConstructions_CurrentPerOrder");
        }
    }
}
