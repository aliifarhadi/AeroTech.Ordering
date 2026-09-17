using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.Inbox
{
    public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
    {
        public const string KeyName = "PK_InboxMessages";
        public const int SourceSystemMaxLength = 64;
        public const int EventIdMaxLength = 64;
        public const int ConsumerMaxLength = 256;
        public const int PayloadHashLength = 64;

        public void Configure(EntityTypeBuilder<InboxMessage> builder)
        {
            builder.ToTable("InboxMessages", "dbo");
            builder
                .HasKey(message => new { message.OwnerAirlineId, message.SourceSystem, message.EventId, message.Consumer })
                .HasName(KeyName);
            builder.Property(message => message.SourceSystem).HasMaxLength(SourceSystemMaxLength);
            builder.Property(message => message.EventId).HasMaxLength(EventIdMaxLength);
            builder.Property(message => message.Consumer).HasMaxLength(ConsumerMaxLength);
            builder.Property(message => message.MessageType).HasMaxLength(500);
            builder.Property(message => message.PayloadHash).HasMaxLength(PayloadHashLength).IsFixedLength().IsUnicode(false);
            builder.HasIndex(message => message.ReceivedOn);
        }
    }
}
