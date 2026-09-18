using System.Security.Cryptography;
using System.Text;
using AeroTech.Ordering.Domain._Shared.Serialization;
using Microsoft.Extensions.Options;

namespace AeroTech.Ordering.Application._Shared.Idempotency
{
    public sealed class RequestDigester
    {
        private readonly byte[] _key;

        public RequestDigester(IOptions<IdempotencyOptions> options)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(options.Value.DigestKey, $"{IdempotencyOptions.SectionName}:{nameof(IdempotencyOptions.DigestKey)}");
            _key = Encoding.UTF8.GetBytes(options.Value.DigestKey);
        }

        public string CanonicalizationVersion => CanonicalJson.Version;

        public string Digest(IReadOnlyDictionary<string, object?> canonicalRequest)
            => Convert.ToHexString(HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes(CanonicalJson.Write(canonicalRequest)))).ToLowerInvariant();
    }
}
