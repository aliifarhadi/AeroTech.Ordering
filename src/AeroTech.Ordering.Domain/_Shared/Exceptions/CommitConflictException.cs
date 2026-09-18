using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain._Shared.Exceptions
{
    public sealed class CommitConflictException : Exception
    {
        public CommitConflictException(CommitConflictKind kind, string detail, Exception innerException)
            : base($"Local commit conflict ({kind}): {detail}", innerException)
        {
            Kind = kind;
        }

        public CommitConflictKind Kind { get; }
    }
}
