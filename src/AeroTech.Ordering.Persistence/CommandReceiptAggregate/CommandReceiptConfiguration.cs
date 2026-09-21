using AeroTech.Ordering.Domain.CommandReceiptAggregate;
using AeroTech.Ordering.Domain.OrderAggregate;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.CommandReceiptAggregate
{
    public sealed class CommandReceiptConfiguration : IEntityTypeConfiguration<CommandReceipt>
    {
        public const string Table = "CommandReceipts";
        public const string ScopeKeyIndex = "UX_CommandReceipts_Scope_Key";

        public void Configure(EntityTypeBuilder<CommandReceipt> builder)
        {
            builder.ToTable(Table, PersistenceSchemas.Operations, table =>
                table.RequiredEnum<OrderingCommandKind>(Table, "CommandKind"));
            builder.HasKey(receipt => receipt.Id);
            builder.Property(receipt => receipt.Id).ValueGeneratedNever();
            builder.Property(receipt => receipt.CallerScope).HasMaxLength(PersistenceSchemas.CallerScopeLength).IsRequired();
            builder.Property(receipt => receipt.IdempotencyKey).HasMaxLength(PersistenceSchemas.IdempotencyKeyLength).IsRequired();
            builder.Property(receipt => receipt.CanonicalizationVersion).HasMaxLength(PersistenceSchemas.ProfileLength).IsRequired();
            builder.Property(receipt => receipt.RequestDigest).HasMaxLength(PersistenceSchemas.DigestLength).IsFixedLength().IsUnicode(false).IsRequired();
            builder.Property(receipt => receipt.ResultJson).HasColumnType("nvarchar(max)").IsRequired();

            builder.HasIndex(receipt => new { receipt.OwnerAirlineId, receipt.CallerScope, receipt.CommandKind, receipt.IdempotencyKey })
                .IsUnique()
                .HasDatabaseName(ScopeKeyIndex);
            builder.HasOne<Order>()
                .WithMany()
                .HasForeignKey(receipt => new { receipt.OwnerAirlineId, receipt.OrderId })
                .HasPrincipalKey(order => new { order.OwnerAirlineId, order.Id })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(receipt => new { receipt.OwnerAirlineId, receipt.OrderId });
        }
    }
}
