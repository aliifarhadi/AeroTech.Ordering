using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class OrderServiceConfiguration : IEntityTypeConfiguration<OrderService>
    {
        public void Configure(EntityTypeBuilder<OrderService> builder)
        {
            builder.ToTable("OrderServices", PersistenceSchemas.Order);
            builder.HasKey(service => service.Id);
            builder.Property(service => service.Id).ValueGeneratedNever();
            builder.Property(service => service.BookingClass).HasMaxLength(PersistenceSchemas.ReferenceLength);

            builder.OwnsOne(service => service.SoldTerms, terms =>
            {
                terms.Property(value => value.Refundable).HasColumnName("SoldTermRefundable");
                terms.Property(value => value.Changeable).HasColumnName("SoldTermChangeable");
                terms.Property(value => value.Upgradable).HasColumnName("SoldTermUpgradable");
                terms.Ignore(value => value.SuppliesNothing);
            });

            builder.OwnsOne(service => service.CheckedBaggage, baggage => baggage.MapBaggage("CheckedBaggage"));
            builder.OwnsOne(service => service.CabinBaggage, baggage => baggage.MapBaggage("CabinBaggage"));

            builder.HasOne<OrderItem>().WithMany().HasForeignKey(service => service.OrderItemId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<OrderTraveller>().WithMany().HasForeignKey(service => service.TravellerId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<OrderSegment>().WithMany().HasForeignKey(service => service.SegmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<OrderChange>().WithMany().HasForeignKey(service => service.CreatedByChangeId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(service => new { service.OrderId, service.TravellerId, service.SegmentId }).IsUnique();
            builder.HasIndex(service => new { service.OrderItemId, service.CommercialStatus });
        }
    }
}
