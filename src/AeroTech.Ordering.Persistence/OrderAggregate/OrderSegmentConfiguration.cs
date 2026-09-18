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
                table.HasCheckConstraint("CK_OrderSegments_Duration", "[Duration] IS NULL OR [Duration] >= 0");
            });
            builder.HasKey(segment => segment.Id);
            builder.Property(segment => segment.Id).ValueGeneratedNever();
            builder.Property(segment => segment.SourceSegmentRef).HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();
            builder.Property(segment => segment.OriginRef).HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();
            builder.Property(segment => segment.OriginTerminalRef).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(segment => segment.DestinationRef).HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();
            builder.Property(segment => segment.DestinationTerminalRef).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(segment => segment.FlightRef).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(segment => segment.FlightNumber).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(segment => segment.FlightVersion).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(segment => segment.MarketingCarrierRef).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(segment => segment.OperatingCarrierRef).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(segment => segment.SourceCapacityRef).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(segment => segment.AircraftRef).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.HasOne<OrderJourney>().WithMany().HasForeignKey(segment => segment.JourneyId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(segment => segment.Legs).WithOne().HasForeignKey(leg => leg.SegmentId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(segment => segment.Legs).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasIndex(segment => new { segment.OrderId, segment.SourceSegmentRef }).IsUnique();
            builder.HasIndex(segment => new { segment.OrderId, segment.Sequence }).IsUnique();
            builder.HasIndex(segment => segment.JourneyId);
        }
    }
}
