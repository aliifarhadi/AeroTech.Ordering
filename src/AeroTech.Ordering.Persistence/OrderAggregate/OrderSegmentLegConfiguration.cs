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
            builder.ToTable("OrderSegmentLegs", PersistenceSchemas.Order);
            builder.HasKey(leg => leg.Id);
            builder.Property(leg => leg.Id).ValueGeneratedNever();
            builder.Property(leg => leg.SourceLegRef).HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();
            builder.HasIndex(leg => new { leg.SegmentId, leg.Sequence }).IsUnique();
        }
    }
}
