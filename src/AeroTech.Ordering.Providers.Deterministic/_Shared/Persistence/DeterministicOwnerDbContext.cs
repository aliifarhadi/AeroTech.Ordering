using AeroTech.Ordering.Providers.Deterministic.Offers.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ordering.Providers.Deterministic._Shared.Persistence
{
    public sealed class DeterministicOwnerDbContext : DbContext
    {
        public const string ConnectionStringName = "DeterministicOwnerDbContext";
        public const string Schema = "DeterministicOwner";
        public const string EffectKeyName = "UX_OwnerEffects_Owner_EffectKey";

        public DeterministicOwnerDbContext(DbContextOptions<DeterministicOwnerDbContext> options)
            : base(options)
        {
        }

        public DbSet<DeterministicOwnerEffect> OwnerEffects => Set<DeterministicOwnerEffect>();

        public DbSet<ReferenceOfferRecord> ReferenceOffers => Set<ReferenceOfferRecord>();

        public DbSet<OwnerReadRecord> OwnerReads => Set<OwnerReadRecord>();

        public DbSet<OwnerAvailabilityRecord> OwnerAvailability => Set<OwnerAvailabilityRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeterministicOwnerDbContext).Assembly);

            modelBuilder.Entity<DeterministicOwnerEffect>(effect =>
            {
                effect.ToTable("OwnerEffects");
                effect.HasKey(item => item.Id);
                effect.Property(item => item.Owner).HasMaxLength(64);
                effect.Property(item => item.EffectKey).HasMaxLength(200);
                effect.Property(item => item.RequestHash).HasMaxLength(128);
                effect.Property(item => item.ResultPayload).HasColumnType("nvarchar(max)");
                effect.HasIndex(item => new { item.Owner, item.EffectKey }).IsUnique().HasDatabaseName(EffectKeyName);
            });
        }
    }
}
