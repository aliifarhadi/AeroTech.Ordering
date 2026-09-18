using AeroTech.Ordering.Query.OrderAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Query.OrderAggregate.Configurations
{
    public sealed class OrderTravelerReadModelConfiguration : IEntityTypeConfiguration<OrderTravelerReadModel>
    {
        public const string Schema = "Commercial";
        public const string Table = "OrderTravelers";

        public void Configure(EntityTypeBuilder<OrderTravelerReadModel> builder)
        {
            builder.ToTable(Table, Schema, table => table.ExcludeFromMigrations());
            builder.HasKey(traveler => traveler.Id);
        }
    }
}
