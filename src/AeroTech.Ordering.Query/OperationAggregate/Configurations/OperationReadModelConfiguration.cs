using AeroTech.Ordering.Query.OperationAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Query.OperationAggregate.Configurations
{
    public sealed class OperationReadModelConfiguration : IEntityTypeConfiguration<OperationReadModel>
    {
        public const string Schema = "Operations";
        public const string Table = "CommandReceipts";

        public void Configure(EntityTypeBuilder<OperationReadModel> builder)
        {
            builder.ToTable(Table, Schema, table => table.ExcludeFromMigrations());
            builder.HasKey(operation => operation.Id);
        }
    }
}
