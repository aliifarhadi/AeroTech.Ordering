using System.Globalization;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence._Shared.Mapping
{
    public static class EnumCheckConstraints
    {
        public static TableBuilder RequiredEnum<TEnum>(this TableBuilder table, string tableName, string column)
            where TEnum : struct, Enum
        {
            table.HasCheckConstraint(NameFor(tableName, column), $"[{column}] IN ({Values<TEnum>()})");
            return table;
        }

        public static TableBuilder OptionalEnum<TEnum>(this TableBuilder table, string tableName, string column)
            where TEnum : struct, Enum
        {
            table.HasCheckConstraint(NameFor(tableName, column), $"[{column}] IS NULL OR [{column}] IN ({Values<TEnum>()})");
            return table;
        }

        public static string NameFor(string tableName, string column) => $"CK_{tableName}_{column}_Enum";

        private static string Values<TEnum>()
            where TEnum : struct, Enum
            => string.Join(", ", Enum.GetValues<TEnum>()
                .Select(value => Convert.ToInt32(value, CultureInfo.InvariantCulture))
                .Distinct()
                .Order()
                .Select(value => value.ToString(CultureInfo.InvariantCulture)));
    }
}
