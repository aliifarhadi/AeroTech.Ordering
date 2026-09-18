using AeroTech.Messages.Aegis.Enums;
using AeroTech.Messages.Shared.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain._Shared.ValueObjects
{
    public sealed record AuthorizedSalesScope
    {
        public AuthorizedSalesScope(
            long ownerAirlineId,
            long financialCustomerId,
            SalesChannel channel,
            long? sellingOfficeId,
            string callerScope,
            BusinessContextType actorContextType,
            long? actorId)
        {
            if (ownerAirlineId <= 0)
                throw ExceptionFactory.AuthorizedScopeRequired("owner airline");

            if (financialCustomerId <= 0)
                throw ExceptionFactory.AuthorizedScopeRequired("financial customer");

            if (string.IsNullOrWhiteSpace(callerScope))
                throw ExceptionFactory.AuthorizedScopeRequired("caller scope");

            if (sellingOfficeId is <= 0)
                throw ExceptionFactory.AuthorizedScopeRequired("selling office");

            if (actorId is <= 0)
                throw ExceptionFactory.AuthorizedScopeRequired("actor");

            OwnerAirlineId = ownerAirlineId;
            FinancialCustomerId = financialCustomerId;
            Channel = channel;
            SellingOfficeId = sellingOfficeId;
            CallerScope = callerScope;
            ActorContextType = actorContextType;
            ActorId = actorId;
        }

        public long OwnerAirlineId { get; }

        public long FinancialCustomerId { get; }

        public SalesChannel Channel { get; }

        public long? SellingOfficeId { get; }

        public string CallerScope { get; }

        public BusinessContextType ActorContextType { get; }

        public long? ActorId { get; }
    }
}
