using AeroTech.Ordering.Domain.OrderPreparationAggregate;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderPreparationAggregate
{
    public sealed class OrderPreparationConfiguration : IEntityTypeConfiguration<OrderPreparation>
    {
        public const string ConsumptionIndex = "UX_OrderPreparations_ConsumedByOrderId";

        public void Configure(EntityTypeBuilder<OrderPreparation> builder)
        {
            builder.ToTable("OrderPreparations", PersistenceSchemas.Commercial, table =>
            {
                table.HasCheckConstraint("CK_OrderPreparations_Consumption", "([ConsumedByOrderId] IS NULL AND [ConsumedAt] IS NULL) OR ([ConsumedByOrderId] IS NOT NULL AND [ConsumedAt] IS NOT NULL)");
                table.HasCheckConstraint("CK_OrderPreparations_Digest", "LEN([SnapshotDigest]) = 64");
            });

            builder.HasKey(preparation => preparation.Id);
            builder.Property(preparation => preparation.Id).ValueGeneratedNever();
            builder.Ignore(preparation => preparation.Candidate);
            builder.Ignore(preparation => preparation.IsConsumed);
            builder.Ignore(preparation => preparation.ValidityFacts);

            builder.Property(preparation => preparation.Channel).HasMaxLength(PersistenceSchemas.OwnerNameLength).IsRequired();
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
            builder.Property(preparation => preparation.ClientReference).HasMaxLength(PersistenceSchemas.ReferenceLength);

            builder.OwnsOne(preparation => preparation.OfferValidity, validity => validity.MapValidity("OfferValidity"));
            builder.OwnsOne(preparation => preparation.PriceValidity, validity => validity.MapValidity("PriceValidity"));
            builder.OwnsOne(preparation => preparation.TicketingValidity, validity => validity.MapValidity("TicketingValidity"));

            builder.HasMany(preparation => preparation.Evidence)
                .WithOne()
                .HasForeignKey(evidence => evidence.PreparationId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(preparation => preparation.Evidence).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(preparation => preparation.ConsumedByOrderId)
                .IsUnique()
                .HasFilter("[ConsumedByOrderId] IS NOT NULL")
                .HasDatabaseName(ConsumptionIndex);
            builder.HasIndex(preparation => new { preparation.OwnerAirlineId, preparation.FinancialCustomerId, preparation.CreatedAt });
            builder.HasIndex(preparation => new { preparation.ConsumedAt, preparation.CreatedAt });
        }
    }
}
