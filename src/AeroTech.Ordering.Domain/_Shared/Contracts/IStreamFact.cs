namespace AeroTech.Ordering.Domain._Shared.Contracts
{
    public interface IStreamFact
    {
        string StreamKind { get; }

        long StreamId { get; }

        long EventOrdinal { get; }
    }
}
