using AeroTech.Ordering.Query.OrderAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Query.OrderAggregate.Configurations
{
    public sealed class OrderTravellerReadModelConfiguration : IEntityTypeConfiguration<OrderTravellerReadModel>
    {
        public const string Schema = "Order";
        public const string Table = "OrderTravellers";

        public void Configure(EntityTypeBuilder<OrderTravellerReadModel> builder)
        {
            builder.ToTable(Table, Schema, table => table.ExcludeFromMigrations());
            builder.HasKey(traveller => traveller.Id);
        }
    }
}
