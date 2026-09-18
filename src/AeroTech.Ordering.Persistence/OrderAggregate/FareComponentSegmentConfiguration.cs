using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class FareComponentSegmentConfiguration : IEntityTypeConfiguration<FareComponentSegment>
    {
        public void Configure(EntityTypeBuilder<FareComponentSegment> builder)
        {
            builder.ToTable("FareComponentSegments", PersistenceSchemas.Order);
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasOne<OrderSegment>().WithMany().HasForeignKey(row => row.OrderSegmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(row => new { row.FareComponentId, row.OrderSegmentId }).IsUnique();
        }
    }
}
