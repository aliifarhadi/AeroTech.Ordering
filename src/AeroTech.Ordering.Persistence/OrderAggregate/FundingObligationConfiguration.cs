using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class FundingObligationConfiguration : IEntityTypeConfiguration<FundingObligation>
    {
        public void Configure(EntityTypeBuilder<FundingObligation> builder)
        {
            builder.ToTable("FundingObligations", PersistenceSchemas.Order, table =>
            {
                table.HasCheckConstraint("CK_FundingObligations_Version", "[Version] >= 1");
                table.HasCheckConstraint("CK_FundingObligations_Amount", "[AmountAmount] >= 0");
            });
            builder.HasKey(obligation => obligation.Id);
            builder.Property(obligation => obligation.Id).ValueGeneratedNever();
            builder.OwnsOne(obligation => obligation.Amount, money => money.MapMoney("Amount"));
            builder.HasOne<OrderChange>().WithMany().HasForeignKey(obligation => obligation.ChangeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<OrderItem>().WithMany().HasForeignKey(obligation => obligation.OrderItemId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(obligation => new { obligation.OrderId, obligation.Id, obligation.Version }).IsUnique();
        }
    }
}
