using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class OrderServiceCoverageConfiguration : IEntityTypeConfiguration<OrderServiceCoverage>
    {
        public void Configure(EntityTypeBuilder<OrderServiceCoverage> builder)
        {
            builder.ToTable("OrderServiceCoverage", PersistenceSchemas.Order);
            builder.HasKey(coverage => coverage.Id);
            builder.Property(coverage => coverage.Id).ValueGeneratedNever();
            builder.HasOne<OrderSegment>().WithMany().HasForeignKey(coverage => coverage.SegmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(coverage => new { coverage.ServiceId, coverage.SegmentId }).IsUnique();
        }
    }
}
