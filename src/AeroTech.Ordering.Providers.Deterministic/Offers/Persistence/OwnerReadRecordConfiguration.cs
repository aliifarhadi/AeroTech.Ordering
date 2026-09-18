using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Providers.Deterministic.Offers.Persistence
{
    public sealed class OwnerReadRecordConfiguration : IEntityTypeConfiguration<OwnerReadRecord>
    {
        public void Configure(EntityTypeBuilder<OwnerReadRecord> builder)
        {
            builder.ToTable("OwnerReads");
            builder.HasKey(read => read.Id);
            builder.Property(read => read.Owner).HasMaxLength(64);
            builder.Property(read => read.Operation).HasMaxLength(64);
            builder.Property(read => read.Reference).HasMaxLength(256);
            builder.Property(read => read.Outcome).HasMaxLength(64);
            builder.HasIndex(read => new { read.Owner, read.Operation, read.ObservedAt });
        }
    }
}
