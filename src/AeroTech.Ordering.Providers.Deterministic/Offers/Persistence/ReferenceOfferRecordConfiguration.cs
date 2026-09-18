using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Providers.Deterministic.Offers.Persistence
{
    public sealed class ReferenceOfferRecordConfiguration : IEntityTypeConfiguration<ReferenceOfferRecord>
    {
        public void Configure(EntityTypeBuilder<ReferenceOfferRecord> builder)
        {
            builder.ToTable("ReferenceOffers");
            builder.HasKey(offer => offer.Id);
            builder.Property(offer => offer.OfferId).HasMaxLength(256);
            builder.Property(offer => offer.OwnerBindingRef).HasMaxLength(256);
            builder.Property(offer => offer.Channel).HasMaxLength(64);
            builder.Property(offer => offer.CandidateJson).HasColumnType("nvarchar(max)");
            builder.HasIndex(offer => new { offer.OfferId, offer.Revision }).IsUnique();
            builder.HasIndex(offer => offer.OwnerBindingRef).IsUnique();
            builder.HasIndex(offer => offer.OfferId).IsUnique().HasFilter("[IsCurrent] = 1");
        }
    }
}
