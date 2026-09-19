using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class FareConstructionItemConfiguration : IEntityTypeConfiguration<FareConstructionItem>
    {
        public void Configure(EntityTypeBuilder<FareConstructionItem> builder)
        {
            builder.ToTable("FareConstructionItems", PersistenceSchemas.Order);
            builder.HasKey(item => new { item.FareConstructionId, item.OrderItemId });
            builder.HasOne<OrderItem>().WithMany().HasForeignKey(item => item.OrderItemId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
