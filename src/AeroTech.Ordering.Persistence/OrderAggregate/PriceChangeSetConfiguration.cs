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
            builder.ToTable("PriceChangeSets", PersistenceSchemas.Commercial, table =>
                table.HasCheckConstraint("CK_PriceChangeSets_Sequence", "[FinancialSequence] >= 1"));
            builder.HasKey(set => set.Id);
            builder.Property(set => set.Id).ValueGeneratedNever();
            builder.Property(set => set.SourceDecisionRef).HasMaxLength(PersistenceSchemas.CallerScopeLength).IsRequired();
            builder.HasOne<OrderChange>().WithMany().HasForeignKey(set => set.ChangeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(set => new { set.OrderId, set.FinancialSequence }).IsUnique();
            builder.HasIndex(set => set.ChangeId).IsUnique();
        }
    }
}
