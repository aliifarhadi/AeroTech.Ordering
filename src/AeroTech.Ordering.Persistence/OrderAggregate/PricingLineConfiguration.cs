using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class PricingLineConfiguration : IEntityTypeConfiguration<PricingLine>
    {
        public void Configure(EntityTypeBuilder<PricingLine> builder)
        {
            builder.ToTable("PricingLines", PersistenceSchemas.Commercial, table =>
            {
                table.HasCheckConstraint("CK_PricingLines_OriginalMagnitude", "[OriginalValueAmount] >= 0");
                table.HasCheckConstraint("CK_PricingLines_SaleMagnitude", "[SaleValueAmount] >= 0");
                table.HasCheckConstraint("CK_PricingLines_TaxNotSettlement", $"NOT ([Component] = {(int)PricingComponentType.Tax} AND [Effect] = {(int)PricingEffect.SettlementOnly})");
                table.HasCheckConstraint("CK_PricingLines_CommissionNotCustomer", $"NOT ([Component] = {(int)PricingComponentType.Commission} AND [Effect] = {(int)PricingEffect.CustomerBalance})");
                table.HasCheckConstraint("CK_PricingLines_OtherInformational", $"[Component] <> {(int)PricingComponentType.Other} OR [Effect] = {(int)PricingEffect.Informational}");
                table.HasCheckConstraint("CK_PricingLines_Direction", $"[Direction] IN ({(int)OrderPricingLineDirection.Debit}, {(int)OrderPricingLineDirection.Credit})");
                table.HasCheckConstraint("CK_PricingLines_ReversalReference", $"[Role] <> {(int)PricingLineRole.Reversal} OR [OriginalPricingLineId] IS NOT NULL");
            });
            builder.HasKey(line => line.Id);
            builder.Property(line => line.Id).ValueGeneratedNever();
            builder.Property(line => line.SourceLineRef).HasMaxLength(512).IsRequired();
            builder.Property(line => line.CandidateLineRef).HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();
            builder.Property(line => line.SourceBasisRef).HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();
            builder.Property(line => line.SourceConversionRef).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.OwnsOne(line => line.OriginalValue, money => money.MapMoney("OriginalValue"));
            builder.OwnsOne(line => line.SaleValue, money => money.MapMoney("SaleValue"));
            builder.HasOne<PriceChangeSet>().WithMany().HasForeignKey(line => line.PriceChangeSetId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<OrderItem>().WithMany().HasForeignKey(line => line.OrderItemId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<PricingLine>().WithMany().HasForeignKey(line => line.OriginalPricingLineId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(line => new { line.PriceChangeSetId, line.CandidateLineRef }).IsUnique();
            builder.HasIndex(line => new { line.OrderId, line.OrderItemId });
        }
    }
}
