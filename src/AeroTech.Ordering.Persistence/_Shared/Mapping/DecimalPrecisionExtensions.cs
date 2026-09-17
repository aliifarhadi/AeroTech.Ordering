using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence._Shared.Mapping
{
    public static class DecimalPrecisionExtensions
    {
        public const int AmountPrecision = 28;
        public const int AmountScale = 8;
        public const int RatePrecision = 28;
        public const int RateScale = 12;
        public const int QuantityPrecision = 18;
        public const int QuantityScale = 6;

        public static PropertyBuilder<decimal> HasAmountPrecision(this PropertyBuilder<decimal> property)
            => property.HasPrecision(AmountPrecision, AmountScale);

        public static PropertyBuilder<decimal?> HasAmountPrecision(this PropertyBuilder<decimal?> property)
            => property.HasPrecision(AmountPrecision, AmountScale);

        public static PropertyBuilder<decimal> HasRatePrecision(this PropertyBuilder<decimal> property)
            => property.HasPrecision(RatePrecision, RateScale);

        public static PropertyBuilder<decimal?> HasRatePrecision(this PropertyBuilder<decimal?> property)
            => property.HasPrecision(RatePrecision, RateScale);

        public static PropertyBuilder<decimal> HasQuantityPrecision(this PropertyBuilder<decimal> property)
            => property.HasPrecision(QuantityPrecision, QuantityScale);

        public static PropertyBuilder<decimal?> HasQuantityPrecision(this PropertyBuilder<decimal?> property)
            => property.HasPrecision(QuantityPrecision, QuantityScale);
    }
}
