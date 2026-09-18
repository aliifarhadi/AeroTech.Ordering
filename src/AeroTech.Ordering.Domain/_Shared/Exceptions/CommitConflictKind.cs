namespace AeroTech.Ordering.Domain._Shared.Exceptions
{
    public enum CommitConflictKind
    {
        CommandReceiptKey = 1,
        PreparationConsumption = 2,
        OrderReference = 3,
        ProjectionRevision = 4
    }
}
