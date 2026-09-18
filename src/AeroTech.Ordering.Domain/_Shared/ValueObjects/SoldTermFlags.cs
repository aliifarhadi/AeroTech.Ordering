namespace AeroTech.Ordering.Domain._Shared.ValueObjects
{
    public sealed record SoldTermFlags(bool? Refundable, bool? Changeable, bool? Upgradable)
    {
        public bool SuppliesNothing => Refundable is null && Changeable is null && Upgradable is null;
    }
}
