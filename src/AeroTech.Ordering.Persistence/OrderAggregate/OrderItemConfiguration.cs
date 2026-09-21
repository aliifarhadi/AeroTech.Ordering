using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems", PersistenceSchemas.Order, table =>
            {
                table.HasCheckConstraint("CK_OrderItems_AcceptedTotal", "[AcceptedTotalAmount] >= 0");
                table.RequiredEnum<OrderItemKind>("OrderItems", "Kind");
                table.RequiredEnum<OrderItemCommercialStatus>("OrderItems", "CommercialStatus");
            });
            builder.HasKey(item => item.Id);
            builder.Property(item => item.Id).ValueGeneratedNever();
            builder.OwnsOne(item => item.AcceptedTotal, money => money.MapMoney("AcceptedTotal"));
            builder.HasAlternateKey(item => new { item.OrderId, item.Id });
            builder.HasOne<OrderChange>().WithMany()
                .HasForeignKey(item => new { item.OrderId, item.CreatedByChangeId })
                .HasPrincipalKey(change => new { change.OrderId, change.Id })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(item => new { item.OrderId, item.CommercialStatus });
        }
    }
}
