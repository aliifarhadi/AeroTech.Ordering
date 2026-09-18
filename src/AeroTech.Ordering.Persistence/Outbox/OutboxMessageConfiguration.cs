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
        public const int StreamKindMaxLength = 32;
        public const string StreamOrdinalIndex = "UX_OutboxMessages_Stream_EventOrdinal";

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
            builder.Property(message => message.StreamKind).HasMaxLength(StreamKindMaxLength);
            builder.ToTable(table => table.HasCheckConstraint("CK_OutboxMessages_Stream",
                "([StreamKind] IS NULL AND [StreamId] IS NULL AND [EventOrdinal] IS NULL) OR ([StreamKind] IS NOT NULL AND [StreamId] IS NOT NULL AND [EventOrdinal] >= 1)"));
            builder.HasIndex(message => new { message.StreamKind, message.StreamId, message.EventOrdinal })
                .IsUnique()
                .HasFilter("[StreamKind] IS NOT NULL")
                .HasDatabaseName(StreamOrdinalIndex);
            builder.HasIndex(message => message.EventId).IsUnique();
            builder.HasIndex(message => new { message.ProcessedOn, message.LeaseExpiresOn });
        }
    }
}
