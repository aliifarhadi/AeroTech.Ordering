using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ordering.Persistence.Inbox
{
    public static class InboxDuplicateClassifier
    {
        private const int UniqueIndexViolation = 2601;
        private const int UniqueConstraintViolation = 2627;

        public static bool IsDuplicateMarker(DbUpdateException exception)
        {
            if (exception.InnerException is not SqlException sqlException)
                return false;

            foreach (SqlError error in sqlException.Errors)
            {
                var isUniqueViolation = error.Number is UniqueIndexViolation or UniqueConstraintViolation;

                if (isUniqueViolation && error.Message.Contains(InboxMessageConfiguration.KeyName, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }
    }
}
