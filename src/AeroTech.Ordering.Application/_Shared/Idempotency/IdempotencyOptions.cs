namespace AeroTech.Ordering.Application._Shared.Idempotency
{
    public sealed class IdempotencyOptions
    {
        public const string SectionName = "Idempotency";

        public string DigestKey { get; set; } = default!;
    }
}
