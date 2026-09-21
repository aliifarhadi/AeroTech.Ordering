using AeroTech.Messages.Ordering.Enums;
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
            {
                table.HasCheckConstraint("CK_OrderSegments_Sequence", "[Sequence] >= 1");
                table.RequiredEnum<SegmentKind>("OrderSegments", "Kind");
            });
            builder.HasKey(segment => segment.Id);
            builder.Property(segment => segment.Id).ValueGeneratedNever();
            builder.Property(segment => segment.FlightNumber).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.HasMany(segment => segment.Legs).WithOne().HasForeignKey(leg => leg.SegmentId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(segment => segment.Legs).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasAlternateKey(segment => new { segment.OrderId, segment.Id });
            builder.HasOne<OrderJourney>().WithMany()
                .HasForeignKey(segment => new { segment.OrderId, segment.JourneyId })
                .HasPrincipalKey(journey => new { journey.OrderId, journey.Id })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(segment => new { segment.JourneyId, segment.Sequence }).IsUnique();
        }
    }
}
