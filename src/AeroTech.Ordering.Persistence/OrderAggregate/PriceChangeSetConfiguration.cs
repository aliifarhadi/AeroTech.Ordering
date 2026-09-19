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
                table.HasCheckConstraint("CK_PriceChangeSets_FinancialSequence", "[FinancialSequence] >= 1"));
            builder.HasKey(set => set.Id);
            builder.Property(set => set.Id).ValueGeneratedNever();
            builder.HasOne<OrderChange>().WithMany().HasForeignKey(set => set.ChangeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(set => new { set.OrderId, set.FinancialSequence }).IsUnique();
        }
    }
}
