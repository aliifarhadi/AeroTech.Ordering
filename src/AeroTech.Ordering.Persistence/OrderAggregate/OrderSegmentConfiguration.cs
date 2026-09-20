using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class OrderSegmentConfiguration : IEntityTypeConfiguration<OrderSegment>
    {
        public void Configure(EntityTypeBuilder<OrderSegment> builder)
        {
            builder.ToTable("OrderSegments", PersistenceSchemas.Order, table =>
                table.HasCheckConstraint("CK_OrderSegments_Sequence", "[Sequence] >= 1"));
            builder.HasKey(segment => segment.Id);
            builder.Property(segment => segment.Id).ValueGeneratedNever();
            builder.Property(segment => segment.FlightNumber).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.HasOne<OrderJourney>().WithMany().HasForeignKey(segment => segment.JourneyId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(segment => segment.Legs).WithOne().HasForeignKey(leg => leg.SegmentId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(segment => segment.Legs).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasAlternateKey(segment => new { segment.OrderId, segment.Id });
            builder.HasIndex(segment => new { segment.JourneyId, segment.Sequence }).IsUnique();
        }
    }
}
