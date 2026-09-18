using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class FareComponentServiceConfiguration : IEntityTypeConfiguration<FareComponentService>
    {
        public void Configure(EntityTypeBuilder<FareComponentService> builder)
        {
            builder.ToTable("FareComponentServices", PersistenceSchemas.Order);
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasOne<OrderService>().WithMany().HasForeignKey(row => row.OrderServiceId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(row => new { row.FareComponentId, row.OrderServiceId }).IsUnique();
        }
    }
}
