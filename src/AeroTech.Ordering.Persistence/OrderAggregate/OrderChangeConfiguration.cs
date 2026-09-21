using AeroTech.Messages.Aegis.Enums;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class OrderChangeConfiguration : IEntityTypeConfiguration<OrderChange>
    {
        public void Configure(EntityTypeBuilder<OrderChange> builder)
        {
            builder.ToTable("OrderChanges", PersistenceSchemas.Order, table =>
            {
                table.HasCheckConstraint("CK_OrderChanges_CommercialVersion", "[CommercialVersion] >= 1");
                table.RequiredEnum<OrderChangeType>("OrderChanges", "Type");
                table.RequiredEnum<BusinessContextType>("OrderChanges", "ActorContextType");
            });
            builder.HasKey(change => change.Id);
            builder.Property(change => change.Id).ValueGeneratedNever();
            builder.HasAlternateKey(change => new { change.OrderId, change.Id });
            builder.HasIndex(change => new { change.OrderId, change.CommercialVersion }).IsUnique();
        }
    }
}
