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
            builder.ToTable("OrderItems", PersistenceSchemas.Commercial);
            builder.HasKey(item => item.Id);
            builder.Property(item => item.Id).ValueGeneratedNever();
            builder.Property(item => item.SourceItemRef).HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();
            builder.Property(item => item.SourceOfferItemRef).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.OwnsOne(item => item.AcceptedTotal, money => money.MapMoney("AcceptedTotal"));
            builder.HasOne<OrderChange>().WithMany().HasForeignKey(item => item.CreatedByChangeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(item => new { item.OrderId, item.SourceItemRef }).IsUnique();
            builder.HasIndex(item => new { item.OrderId, item.CommercialStatus });
        }
    }
}
