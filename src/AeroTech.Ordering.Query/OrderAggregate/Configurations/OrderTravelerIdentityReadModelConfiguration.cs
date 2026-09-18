using AeroTech.Ordering.Query.OrderAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Query.OrderAggregate.Configurations
{
    public sealed class OrderTravelerIdentityReadModelConfiguration : IEntityTypeConfiguration<OrderTravelerIdentityReadModel>
    {
        public const string Schema = "Order";
        public const string Table = "OrderTravelerIdentities";

        public void Configure(EntityTypeBuilder<OrderTravelerIdentityReadModel> builder)
        {
            builder.ToTable(Table, Schema, table => table.ExcludeFromMigrations());
            builder.HasKey(identity => identity.TravelerId);
        }
    }
}
