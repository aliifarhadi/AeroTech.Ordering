namespace AeroTech.Ordering.Domain.CommandReceiptAggregate.Contracts
{
    public interface ICommandReceiptRepository
    {
        Task<CommandReceipt?> FindAsync(ReceiptScope scope, CancellationToken cancellationToken = default);

        void Add(CommandReceipt receipt);
    }
}
