using AeroTech.Ordering.Query.OrderAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Query.OrderAggregate.Configurations
{
    public sealed class OrderTravellerIdentityReadModelConfiguration : IEntityTypeConfiguration<OrderTravellerIdentityReadModel>
    {
        public const string Schema = "Order";
        public const string Table = "OrderTravellerIdentities";

        public void Configure(EntityTypeBuilder<OrderTravellerIdentityReadModel> builder)
        {
            builder.ToTable(Table, Schema, table => table.ExcludeFromMigrations());
            builder.HasKey(identity => identity.TravellerId);
        }
    }
}
