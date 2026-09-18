using AeroTech.Ordering.Query.OrderAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Query.OrderAggregate.Configurations
{
    public sealed class OrderDetailsReadModelConfiguration : IEntityTypeConfiguration<OrderDetailsReadModel>
    {
        public const string Table = "OrderDetails";

        public void Configure(EntityTypeBuilder<OrderDetailsReadModel> builder)
        {
            builder.ToTable(Table, table =>
            {
                table.HasCheckConstraint("CK_OrderDetails_Revision", "[OrderRevision] >= 1 AND [ProjectionSchemaVersion] >= 1");
                table.HasCheckConstraint("CK_OrderDetails_Json", "ISJSON([DetailsJson]) = 1");
            });
            builder.HasKey(details => details.OrderId);
            builder.Property(details => details.OrderId).ValueGeneratedNever();
            builder.Property(details => details.OrderReference).HasMaxLength(64).IsRequired();
            builder.Property(details => details.DetailsJson).HasColumnType("nvarchar(max)").IsRequired();
            builder.HasIndex(details => new { details.OwnerAirlineId, details.OrderReference }).IsUnique();
            builder.HasIndex(details => new { details.OwnerAirlineId, details.FinancialCustomerId, details.CreatedAt });
        }
    }
}
