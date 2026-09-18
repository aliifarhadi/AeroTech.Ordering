using System.Data.Common;

namespace AeroTech.Ordering.Persistence._Shared.Transactions
{
    public interface ICommandTransactionParticipant
    {
        Task FlushAsync(DbTransaction transaction, CancellationToken cancellationToken);

        void Complete();

        void Abandon();
    }
}
