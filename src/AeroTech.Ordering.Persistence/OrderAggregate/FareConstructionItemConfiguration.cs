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
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasOne<OrderItem>().WithMany().HasForeignKey(row => row.OrderItemId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(row => new { row.FareConstructionId, row.OrderItemId }).IsUnique();
        }
    }
}
