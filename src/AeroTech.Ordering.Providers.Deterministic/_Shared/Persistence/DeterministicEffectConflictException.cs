namespace AeroTech.Ordering.Providers.Deterministic._Shared.Persistence
{
    public sealed class DeterministicEffectConflictException : Exception
    {
        public DeterministicEffectConflictException(string owner, string effectKey)
            : base($"Deterministic owner '{owner}' already holds effect '{effectKey}' with a different request hash.")
        {
            Owner = owner;
            EffectKey = effectKey;
        }

        public string Owner { get; }

        public string EffectKey { get; }
    }
}
