using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class PriceChangeSetConfiguration : IEntityTypeConfiguration<PriceChangeSet>
    {
        public void Configure(EntityTypeBuilder<PriceChangeSet> builder)
        {
            builder.ToTable("PriceChangeSets", PersistenceSchemas.Order, table =>
                {
                    table.HasCheckConstraint("CK_PriceChangeSets_FinancialSequence", "[FinancialSequence] >= 1");
                    table.RequiredEnum<PriceChangeReason>("PriceChangeSets", "Reason");
                });
            builder.HasKey(set => set.Id);
            builder.Property(set => set.Id).ValueGeneratedNever();
            builder.HasAlternateKey(set => new { set.OrderId, set.Id });
            builder.HasOne<OrderChange>().WithMany()
                .HasForeignKey(set => new { set.OrderId, set.ChangeId })
                .HasPrincipalKey(change => new { change.OrderId, change.Id })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(set => new { set.OrderId, set.FinancialSequence }).IsUnique();
            builder.HasIndex(set => set.ChangeId).IsUnique();
        }
    }
}
