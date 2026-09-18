using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems", PersistenceSchemas.Order);
            builder.HasKey(item => item.Id);
            builder.Property(item => item.Id).ValueGeneratedNever();
            builder.Property(item => item.SourceItemRef).HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();
            builder.Property(item => item.SourceOfferItemRef).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.OwnsOne(item => item.AcceptedTotal, money => money.MapMoney("AcceptedTotal"));
            builder.OwnsOne(item => item.Product, product =>
            {
                product.Property(value => value.SourceSystem).HasColumnName("ProductSourceSystem").HasMaxLength(PersistenceSchemas.OwnerNameLength).IsRequired();
                product.Property(value => value.SourceOfferId).HasColumnName("ProductSourceOfferId").HasMaxLength(2048).IsRequired();
                product.Property(value => value.SourceOfferItemRef).HasColumnName("ProductSourceOfferItemRef").HasMaxLength(PersistenceSchemas.ReferenceLength);
                product.Property(value => value.ProductCode).HasColumnName("ProductCode").HasMaxLength(PersistenceSchemas.ReferenceLength);
                product.Property(value => value.ProductName).HasColumnName("ProductName").HasMaxLength(PersistenceSchemas.ReferenceLength);
                product.Property(value => value.BrandCode).HasColumnName("ProductBrandCode").HasMaxLength(PersistenceSchemas.ReferenceLength);
                product.Property(value => value.BrandName).HasColumnName("ProductBrandName").HasMaxLength(PersistenceSchemas.ReferenceLength);
                product.Property(value => value.ProductVersion).HasColumnName("ProductVersion").HasMaxLength(PersistenceSchemas.ReferenceLength);
            });
            builder.OwnsOne(item => item.CommercialTerms, terms =>
            {
                terms.Property(value => value.Refundability).HasColumnName("TermsRefundability");
                terms.Property(value => value.Changeability).HasColumnName("TermsChangeability");
                terms.Property(value => value.UpgradeEligibility).HasColumnName("TermsUpgradeEligibility");
                terms.Property(value => value.SourceSystem).HasColumnName("TermsSourceSystem").HasMaxLength(PersistenceSchemas.OwnerNameLength).IsRequired();
                terms.Property(value => value.SourcePolicyRef).HasColumnName("TermsSourcePolicyRef").HasMaxLength(PersistenceSchemas.ReferenceLength);
                terms.Property(value => value.SourcePolicyVersion).HasColumnName("TermsSourcePolicyVersion").HasMaxLength(PersistenceSchemas.ReferenceLength);
                terms.Property(value => value.TermsCapturedAt).HasColumnName("TermsCapturedAt");
            });
            builder.HasOne<OrderChange>().WithMany().HasForeignKey(item => item.CreatedByChangeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(item => new { item.OrderId, item.SourceItemRef }).IsUnique();
            builder.HasIndex(item => new { item.OrderId, item.CommercialStatus });
        }
    }
}
