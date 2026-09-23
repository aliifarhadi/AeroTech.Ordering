using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class FundingObligationConfiguration : IEntityTypeConfiguration<FundingObligation>
    {
        public const string ExactlyOneScope = "CK_FundingObligations_ExactlyOneScope";

        public void Configure(EntityTypeBuilder<FundingObligation> builder)
        {
            builder.ToTable("FundingObligations", PersistenceSchemas.Order, table =>
            {
                table.HasCheckConstraint("CK_FundingObligations_Version", "[Version] >= 1");
                table.RequiredEnum<FundingObligationPurpose>("FundingObligations", "Purpose");
                table.HasCheckConstraint("CK_FundingObligations_Amount", "[AmountAmount] >= 0");
                table.HasCheckConstraint(
                    ExactlyOneScope,
                    "(CASE WHEN [OrderItemId] IS NULL THEN 0 ELSE 1 END)"
                    + " + (CASE WHEN [OrderServiceId] IS NULL THEN 0 ELSE 1 END)"
                    + " + (CASE WHEN [PricingLineId] IS NULL THEN 0 ELSE 1 END) = 1");
            });
            builder.HasKey(obligation => obligation.Id);
            builder.Property(obligation => obligation.Id).ValueGeneratedNever();
            builder.OwnsOne(obligation => obligation.Amount, money => money.MapMoney("Amount"));
            builder.HasOne<OrderChange>().WithMany()
                .HasForeignKey(obligation => new { obligation.OrderId, obligation.ChangeId })
                .HasPrincipalKey(change => new { change.OrderId, change.Id })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<PriceChangeSet>().WithMany()
                .HasForeignKey(obligation => new { obligation.OrderId, obligation.PriceChangeSetId })
                .HasPrincipalKey(set => new { set.OrderId, set.Id })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<OrderItem>().WithMany()
                .HasForeignKey(obligation => obligation.OrderItemId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<OrderService>().WithMany()
                .HasForeignKey(obligation => obligation.OrderServiceId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<PricingLine>().WithMany()
                .HasForeignKey(obligation => obligation.PricingLineId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(obligation => new { obligation.OrderId, obligation.Id, obligation.Version }).IsUnique();
        }
    }
}
