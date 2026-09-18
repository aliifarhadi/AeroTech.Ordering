namespace AeroTech.Messages.Ordering.IntegrationEvents.V2
{
    public record OrderCreated(
        long OrderId,
        long ChangeId,
        int CommercialVersion,
        string AcceptedSourceDigest,
        long PriceChangeSetId,
        int FinancialSequence,
        long EventOrdinal) : BaseIntegrationEvent;
}
