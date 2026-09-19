using AeroTech.Ordering.Domain.OrderPreparationAggregate;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderPreparationAggregate
{
    public sealed class OrderPreparationConfiguration : IEntityTypeConfiguration<OrderPreparation>
    {
        public void Configure(EntityTypeBuilder<OrderPreparation> builder)
        {
            builder.ToTable("OrderPreparations", PersistenceSchemas.Order, table =>
                table.HasCheckConstraint("CK_OrderPreparations_Digest", "LEN([SnapshotDigest]) = 64"));

            builder.HasKey(preparation => preparation.Id);
            builder.Property(preparation => preparation.Id).ValueGeneratedNever();
            builder.Ignore(preparation => preparation.Candidate);

            builder.Property(preparation => preparation.CallerScope).HasMaxLength(PersistenceSchemas.CallerScopeLength).IsRequired();
            builder.Property(preparation => preparation.SourceOwner).HasMaxLength(PersistenceSchemas.OwnerNameLength).IsRequired();
            builder.Property(preparation => preparation.SourceOfferId).HasMaxLength(2048).IsRequired();
            builder.Property(preparation => preparation.ProviderProfileId).HasMaxLength(PersistenceSchemas.ProfileLength).IsRequired();
            builder.Property(preparation => preparation.ContractVersion).HasMaxLength(PersistenceSchemas.ProfileLength).IsRequired();
            builder.Property(preparation => preparation.AcceptanceProfile).HasMaxLength(PersistenceSchemas.ProfileLength).IsRequired();
            builder.Property(preparation => preparation.OwnerBindingRef).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(preparation => preparation.SourcePayloadHash).HasMaxLength(PersistenceSchemas.DigestLength).IsRequired();
            builder.Property(preparation => preparation.CanonicalizationVersion).HasMaxLength(PersistenceSchemas.ProfileLength).IsRequired();
            builder.Property(preparation => preparation.SnapshotDigest).HasMaxLength(PersistenceSchemas.DigestLength).IsFixedLength().IsUnicode(false).IsRequired();
            builder.Property(preparation => preparation.CandidateJson).HasColumnType("nvarchar(max)").IsRequired();

            builder.HasMany(preparation => preparation.Evidence)
                .WithOne()
                .HasForeignKey(evidence => evidence.PreparationId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(preparation => preparation.Evidence).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(preparation => new { preparation.OwnerAirlineId, preparation.FinancialCustomerId, preparation.CapturedAt });
        }
    }
}
