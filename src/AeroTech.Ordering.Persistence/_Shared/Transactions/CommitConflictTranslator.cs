using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Exceptions;
using AeroTech.Ordering.Persistence.CommandReceiptAggregate;
using AeroTech.Ordering.Persistence.OrderAggregate;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ordering.Persistence._Shared.Transactions
{
    public static class CommitConflictTranslator
    {
        private const int UniqueIndexViolation = 2601;
        private const int UniqueConstraintViolation = 2627;

        private static readonly (string Index, CommitConflictKind Kind)[] UniqueIndexes =
        {
            (CommandReceiptConfiguration.ScopeKeyIndex, CommitConflictKind.CommandReceiptKey),
            (OrderConfiguration.SourcePreparationIndex, CommitConflictKind.PreparationConsumption),
            (OrderConfiguration.ReferenceIndex, CommitConflictKind.OrderReference)
        };

        public static CommitConflictException? Translate(DbUpdateException exception)
        {
            if (exception.InnerException is not SqlException sql)
                return null;

            foreach (SqlError error in sql.Errors)
            {
                if (error.Number is not (UniqueIndexViolation or UniqueConstraintViolation))
                    continue;

                foreach (var (index, kind) in UniqueIndexes)
                    if (error.Message.Contains(index, StringComparison.Ordinal))
                        return new CommitConflictException(kind, index, exception);
            }

            return null;
        }
    }
}
