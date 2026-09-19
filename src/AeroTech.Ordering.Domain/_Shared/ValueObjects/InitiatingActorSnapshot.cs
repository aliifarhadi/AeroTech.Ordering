using AeroTech.Messages.Aegis.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain._Shared.ValueObjects
{
    public sealed record InitiatingActorSnapshot
    {
        public InitiatingActorSnapshot(BusinessContextType contextType, long? actorId)
        {
            if (actorId is <= 0)
                throw ExceptionFactory.SalesContextIncomplete("initiating actor identifier");

            ContextType = contextType;
            ActorId = actorId;
        }

        public BusinessContextType ContextType { get; }

        public long? ActorId { get; }
    }
}
