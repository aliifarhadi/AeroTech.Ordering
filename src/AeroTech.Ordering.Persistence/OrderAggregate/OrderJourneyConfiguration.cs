using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class OrderJourneyConfiguration : IEntityTypeConfiguration<OrderJourney>
    {
        public void Configure(EntityTypeBuilder<OrderJourney> builder)
        {
            builder.ToTable("OrderJourneys", PersistenceSchemas.Order, table =>
                table.HasCheckConstraint("CK_OrderJourneys_Sequence", "[Sequence] >= 1"));
            builder.HasKey(journey => journey.Id);
            builder.Property(journey => journey.Id).ValueGeneratedNever();
            builder.Property(journey => journey.BoundId).HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();
            builder.HasIndex(journey => new { journey.OrderId, journey.BoundId }).IsUnique();
            builder.HasIndex(journey => new { journey.OrderId, journey.Sequence }).IsUnique();
        }
    }
}
