using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace AeroTech.Ordering.Domain._Shared.Serialization
{
    public static class CanonicalJson
    {
        public const string Version = "ordering-canonical-json-v1";

        private static readonly JsonWriterOptions WriterOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = false
        };

        public static string Write(object? node)
        {
            using var buffer = new MemoryStream();

            using (var writer = new Utf8JsonWriter(buffer, WriterOptions))
                WriteNode(writer, node);

            return Encoding.UTF8.GetString(buffer.ToArray());
        }

        public static string Sha256Hex(string canonical)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();

        public static string Amount(decimal value) => value.ToString(CultureInfo.InvariantCulture);

        public static string Instant(DateTimeOffset value)
        {
            var fraction = value.ToString("fffffff", CultureInfo.InvariantCulture).TrimEnd('0');
            var seconds = value.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);
            var offset = value.Offset == TimeSpan.Zero
                ? "Z"
                : value.ToString("zzz", CultureInfo.InvariantCulture);

            return fraction.Length == 0 ? $"{seconds}{offset}" : $"{seconds}.{fraction}{offset}";
        }

        public static string? Instant(DateTimeOffset? value) => value is null ? null : Instant(value.Value);

        public static string Identifier(long value) => value.ToString(CultureInfo.InvariantCulture);

        public static string? Identifier(long? value) => value is null ? null : Identifier(value.Value);

        private static void WriteNode(Utf8JsonWriter writer, object? node)
        {
            switch (node)
            {
                case null:
                    writer.WriteNullValue();
                    break;

                case string text:
                    writer.WriteStringValue(text);
                    break;

                case bool flag:
                    writer.WriteBooleanValue(flag);
                    break;

                case int number:
                    writer.WriteNumberValue(number);
                    break;

                case long number:
                    writer.WriteNumberValue(number);
                    break;

                case IReadOnlyDictionary<string, object?> map:
                    writer.WriteStartObject();

                    foreach (var pair in map.OrderBy(pair => pair.Key, StringComparer.Ordinal))
                    {
                        writer.WritePropertyName(pair.Key);
                        WriteNode(writer, pair.Value);
                    }

                    writer.WriteEndObject();
                    break;

                case IEnumerable<object?> list:
                    writer.WriteStartArray();

                    foreach (var element in list)
                        WriteNode(writer, element);

                    writer.WriteEndArray();
                    break;

                default:
                    throw new ArgumentException($"Canonical JSON does not support {node.GetType().Name}.", nameof(node));
            }
        }
    }
}
