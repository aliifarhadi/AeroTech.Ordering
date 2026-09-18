using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Providers.Deterministic.Offers.Persistence
{
    public sealed class OwnerAvailabilityRecordConfiguration : IEntityTypeConfiguration<OwnerAvailabilityRecord>
    {
        public void Configure(EntityTypeBuilder<OwnerAvailabilityRecord> builder)
        {
            builder.ToTable("OwnerAvailability");
            builder.HasKey(availability => availability.Owner);
            builder.Property(availability => availability.Owner).HasMaxLength(64);
        }
    }
}
