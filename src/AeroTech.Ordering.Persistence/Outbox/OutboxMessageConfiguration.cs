using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.Outbox
{
    public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public const string Table = "OutboxMessages";
        public const string Schema = "dbo";
        public const int EventIdMaxLength = 64;
        public const int LeaseOwnerMaxLength = 128;
        public const int LastErrorMaxLength = 2000;

        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable(Table, Schema);
            builder.HasKey(message => message.Id);
            builder.Property(message => message.Id).ValueGeneratedOnAdd();
            builder.Property(message => message.EventId).HasMaxLength(EventIdMaxLength);
            builder.Property(message => message.MessageType).HasMaxLength(500);
            builder.Property(message => message.Payload).HasColumnType("nvarchar(max)");
            builder.Property(message => message.LastError).HasMaxLength(LastErrorMaxLength);
            builder.Property(message => message.LeaseOwner).HasMaxLength(LeaseOwnerMaxLength);
            builder.HasIndex(message => message.EventId).IsUnique();
            builder.HasIndex(message => new { message.ProcessedOn, message.LeaseExpiresOn });
        }
    }
}
