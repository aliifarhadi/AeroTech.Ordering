using AeroTech.Ordering.Domain.OrderPreparationAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderPreparationAggregate
{
    public sealed class PreparationSourceEvidenceConfiguration : IEntityTypeConfiguration<PreparationSourceEvidence>
    {
        public void Configure(EntityTypeBuilder<PreparationSourceEvidence> builder)
        {
            builder.ToTable("PreparationSourceEvidence", PersistenceSchemas.Commercial);
            builder.HasKey(evidence => evidence.Id);
            builder.Property(evidence => evidence.Id).ValueGeneratedNever();
            builder.Property(evidence => evidence.EvidenceRef).HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();
            builder.Property(evidence => evidence.PayloadHash).HasMaxLength(PersistenceSchemas.DigestLength).IsRequired();
            builder.Property(evidence => evidence.ContentType).HasMaxLength(PersistenceSchemas.OwnerNameLength).IsRequired();
            builder.Property(evidence => evidence.Payload).HasColumnType("nvarchar(max)").IsRequired();
            builder.HasIndex(evidence => new { evidence.PreparationId, evidence.EvidenceRef }).IsUnique();
        }
    }
}
