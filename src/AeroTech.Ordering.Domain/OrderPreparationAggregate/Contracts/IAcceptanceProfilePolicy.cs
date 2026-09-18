namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.Contracts
{
    public interface IAcceptanceProfilePolicy
    {
        string EnvironmentClass { get; }

        bool Permits(string acceptanceProfile);
    }
}
