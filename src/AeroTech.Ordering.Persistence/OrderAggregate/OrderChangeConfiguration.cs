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
            builder.ToTable("OrderChanges", PersistenceSchemas.Commercial);
            builder.HasKey(change => change.Id);
            builder.Property(change => change.Id).ValueGeneratedNever();
            builder.Property(change => change.SourceDecisionRef).HasMaxLength(PersistenceSchemas.CallerScopeLength).IsRequired();
            builder.HasIndex(change => new { change.OrderId, change.CommercialVersion }).IsUnique();
        }
    }
}
