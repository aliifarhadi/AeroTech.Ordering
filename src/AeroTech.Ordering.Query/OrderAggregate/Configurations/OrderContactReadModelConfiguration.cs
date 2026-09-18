using AeroTech.Ordering.Query.OrderAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Query.OrderAggregate.Configurations
{
    public sealed class OrderContactReadModelConfiguration : IEntityTypeConfiguration<OrderContactReadModel>
    {
        public const string Schema = "Commercial";
        public const string Table = "OrderContacts";

        public void Configure(EntityTypeBuilder<OrderContactReadModel> builder)
        {
            builder.ToTable(Table, Schema, table => table.ExcludeFromMigrations());
            builder.HasKey(contact => contact.Id);
        }
    }
}
