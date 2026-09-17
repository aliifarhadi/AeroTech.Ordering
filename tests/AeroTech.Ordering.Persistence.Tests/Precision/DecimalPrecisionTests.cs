using AeroTech.Ordering.Persistence._Shared.Mapping;
using AeroTech.Ordering.Persistence.Tests._Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.Precision
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class DecimalPrecisionTests
    {
        private const decimal Amount = 1.23456789m;
        private const decimal Rate = 0.123456789123m;
        private const decimal Quantity = 12.345678m;

        private readonly OrderingDatabaseFixture _fixture;

        public DecimalPrecisionTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Amount_rate_and_quantity_round_trip_unchanged_on_sql_server()
        {
            await using (var setup = NewContext())
                await setup.GetService<IRelationalDatabaseCreator>().CreateTablesAsync();

            await using (var write = NewContext())
            {
                write.Samples.Add(new PrecisionSample { Amount = Amount, NullableAmount = Amount, Rate = Rate, Quantity = Quantity });
                await write.SaveChangesAsync();
            }

            await using var read = NewContext();
            var stored = await read.Samples.AsNoTracking().SingleAsync();

            Assert.Equal(Amount, stored.Amount);
            Assert.Equal(Amount, stored.NullableAmount);
            Assert.Equal(Rate, stored.Rate);
            Assert.Equal(Quantity, stored.Quantity);
        }

        [Fact]
        public void The_removed_blanket_precision_would_have_truncated_the_values()
        {
            Assert.NotEqual(Amount, Math.Round(Amount, 2));
            Assert.NotEqual(Rate, Math.Round(Rate, 2));
        }

        private PrecisionContext NewContext()
            => new(new DbContextOptionsBuilder<PrecisionContext>().UseSqlServer(_fixture.ConnectionString).Options);

        private sealed class PrecisionSample
        {
            public long Id { get; set; }

            public decimal Amount { get; set; }

            public decimal? NullableAmount { get; set; }

            public decimal Rate { get; set; }

            public decimal Quantity { get; set; }
        }

        private sealed class PrecisionContext : DbContext
        {
            public PrecisionContext(DbContextOptions<PrecisionContext> options)
                : base(options)
            {
            }

            public DbSet<PrecisionSample> Samples => Set<PrecisionSample>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<PrecisionSample>(sample =>
                {
                    sample.ToTable("PrecisionSamples", "B0Test");
                    sample.Property(item => item.Amount).HasAmountPrecision();
                    sample.Property(item => item.NullableAmount).HasAmountPrecision();
                    sample.Property(item => item.Rate).HasRatePrecision();
                    sample.Property(item => item.Quantity).HasQuantityPrecision();
                });
        }
    }
}
