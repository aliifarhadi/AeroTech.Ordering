using System.Globalization;
using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain._Shared.ValueObjects
{
    public static class DecimalRepresentation
    {
        public const int AmountPrecision = 28;
        public const int AmountScale = 8;
        public const int RatePrecision = 28;
        public const int RateScale = 12;
        public const int QuantityPrecision = 18;
        public const int QuantityScale = 6;

        public static decimal EnsureAmount(decimal value, string field) => Ensure(value, field, AmountPrecision, AmountScale);

        public static decimal Normalize(decimal value) => value / 1.0000000000000000000000000000m;

        public static string Text(decimal value) => Normalize(value).ToString(CultureInfo.InvariantCulture);

        public static decimal EnsureRate(decimal value, string field) => Ensure(value, field, RatePrecision, RateScale);

        public static decimal EnsureQuantity(decimal value, string field) => Ensure(value, field, QuantityPrecision, QuantityScale);

        public static int ScaleOf(decimal value) => (decimal.GetBits(value)[3] >> 16) & 0x7F;

        private static decimal Ensure(decimal value, string field, int precision, int scale)
        {
            var normalized = value / 1.0000000000000000000000000000m;
            var integerDigits = IntegerDigits(decimal.Truncate(Math.Abs(normalized)));

            if (ScaleOf(normalized) > scale || integerDigits > precision - scale)
                throw ExceptionFactory.RepresentationOverflow(value, field, precision, scale);

            return value;
        }

        private static int IntegerDigits(decimal integral)
        {
            var digits = 0;

            while (integral >= 1)
            {
                integral = decimal.Truncate(integral / 10);
                digits++;
            }

            return digits;
        }
    }
}
