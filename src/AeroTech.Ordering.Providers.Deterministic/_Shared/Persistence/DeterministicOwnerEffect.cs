namespace AeroTech.Ordering.Providers.Deterministic._Shared.Persistence
{
    public sealed class DeterministicOwnerEffect
    {
        public long Id { get; set; }

        public string Owner { get; set; } = null!;

        public string EffectKey { get; set; } = null!;

        public string RequestHash { get; set; } = null!;

        public string ResultPayload { get; set; } = null!;

        public DateTimeOffset RecordedOn { get; set; }
    }
}
