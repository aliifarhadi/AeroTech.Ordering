using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class OrderComponentTotalConfiguration : IEntityTypeConfiguration<OrderComponentTotal>
    {
        public void Configure(EntityTypeBuilder<OrderComponentTotal> builder)
        {
            builder.ToTable("OrderComponentTotals", PersistenceSchemas.Order, table =>
            {
                table.HasCheckConstraint("CK_OrderComponentTotals_Magnitudes", "[DebitAmount] >= 0 AND [CreditAmount] >= 0");
            });
            builder.HasKey(total => total.Id);
            builder.Property(total => total.Id).ValueGeneratedNever();
            builder.Property(total => total.DebitAmount).HasAmountPrecision().IsRequired();
            builder.Property(total => total.CreditAmount).HasAmountPrecision().IsRequired();
            builder.Property(total => total.CurrencyRef).HasMaxLength(PersistenceSchemas.CurrencyRefLength).IsRequired();
            builder.Ignore(total => total.Net);
            builder.HasIndex(total => new { total.OrderId, total.Component, total.Effect }).IsUnique();
        }
    }
}
