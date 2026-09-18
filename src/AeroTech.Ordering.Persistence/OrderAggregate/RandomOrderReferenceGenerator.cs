using System.Security.Cryptography;
using AeroTech.Ordering.Domain.OrderAggregate.Contracts;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class RandomOrderReferenceGenerator : IOrderReferenceGenerator
    {
        public const string Alphabet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";
        public const int Length = 8;

        public Task<string> NextAsync(long ownerAirlineId, CancellationToken cancellationToken = default)
            => Task.FromResult(Next());

        public static string Next() => RandomNumberGenerator.GetString(Alphabet, Length);
    }
}
