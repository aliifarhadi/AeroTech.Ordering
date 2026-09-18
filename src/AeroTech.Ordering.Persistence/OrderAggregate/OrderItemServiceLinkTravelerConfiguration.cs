using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class OrderItemServiceLinkTravelerConfiguration : IEntityTypeConfiguration<OrderItemServiceLinkTraveler>
    {
        public void Configure(EntityTypeBuilder<OrderItemServiceLinkTraveler> builder)
        {
            builder.ToTable("OrderItemServiceLinkTravelers", PersistenceSchemas.Order);
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasOne<OrderTraveler>().WithMany().HasForeignKey(row => row.TravelerId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(row => new { row.LinkId, row.TravelerId }).IsUnique();
        }
    }
}
