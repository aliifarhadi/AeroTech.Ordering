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
            builder.HasOne<OrderItem>().WithMany()
                .HasForeignKey(link => new { link.OrderIdAtAssociation, link.OrderItemId })
                .HasPrincipalKey(item => new { item.OrderId, item.Id })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<OrderService>().WithMany()
                .HasForeignKey(link => new { link.OrderIdAtAssociation, link.OrderServiceId })
                .HasPrincipalKey(service => new { service.OrderId, service.Id })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<OrderChange>().WithMany()
                .HasForeignKey(link => new { link.OrderIdAtAssociation, link.LinkedByChangeId })
                .HasPrincipalKey(change => new { change.OrderId, change.Id })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(link => new { link.OrderItemId, link.OrderServiceId }).IsUnique();
        }
    }
}
