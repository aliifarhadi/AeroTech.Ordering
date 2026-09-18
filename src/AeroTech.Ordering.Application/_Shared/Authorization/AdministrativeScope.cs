using AeroTech.Messages.Aegis.Enums;

namespace AeroTech.Ordering.Application._Shared.Authorization
{
    public sealed record AdministrativeScope(
        long OwnerAirlineId,
        string CallerScope,
        BusinessContextType ActorContextType,
        long? ActorId);
}
