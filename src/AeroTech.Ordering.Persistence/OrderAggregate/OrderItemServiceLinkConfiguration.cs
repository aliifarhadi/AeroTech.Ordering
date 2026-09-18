using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class OrderItemServiceLinkConfiguration : IEntityTypeConfiguration<OrderItemServiceLink>
    {
        public void Configure(EntityTypeBuilder<OrderItemServiceLink> builder)
        {
            builder.ToTable("OrderItemServiceLinks", PersistenceSchemas.Order);
            builder.HasKey(link => link.Id);
            builder.Property(link => link.Id).ValueGeneratedNever();
            builder.HasOne<OrderItem>().WithMany().HasForeignKey(link => link.OrderItemId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<OrderService>().WithMany().HasForeignKey(link => link.OrderServiceId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<OrderChange>().WithMany().HasForeignKey(link => link.LinkedByChangeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(link => link.TravelersAtAssociation).WithOne().HasForeignKey(scope => scope.LinkId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(link => link.SegmentsAtAssociation).WithOne().HasForeignKey(scope => scope.LinkId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(link => link.TravelersAtAssociation).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(link => link.SegmentsAtAssociation).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasIndex(link => new { link.OrderItemId, link.OrderServiceId, link.LinkedByChangeId }).IsUnique();
            builder.HasIndex(link => link.OrderServiceId);
        }
    }
}
