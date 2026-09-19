using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class OrderSegmentLegConfiguration : IEntityTypeConfiguration<OrderSegmentLeg>
    {
        public void Configure(EntityTypeBuilder<OrderSegmentLeg> builder)
        {
            builder.ToTable("OrderSegmentLegs", PersistenceSchemas.Order, table =>
                table.HasCheckConstraint("CK_OrderSegmentLegs_Sequence", "[Sequence] >= 1"));
            builder.HasKey(leg => leg.Id);
            builder.Property(leg => leg.Id).ValueGeneratedNever();
            builder.HasIndex(leg => new { leg.SegmentId, leg.Sequence }).IsUnique();
            builder.HasIndex(leg => new { leg.SegmentId, leg.LegId }).IsUnique();
        }
    }
}
