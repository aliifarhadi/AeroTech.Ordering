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
            SalesContextSnapshot salesContext,
            BuyerSnapshot buyer,
            string callerScope,
            InitiatingActorSnapshot initiatingActor)
        {
            if (ownerAirlineId <= 0)
                throw ExceptionFactory.AuthorizedScopeRequired("owner airline");

            if (financialCustomerId <= 0)
                throw ExceptionFactory.AuthorizedScopeRequired("financial customer");

            if (string.IsNullOrWhiteSpace(callerScope))
                throw ExceptionFactory.AuthorizedScopeRequired("caller scope");

            OwnerAirlineId = ownerAirlineId;
            FinancialCustomerId = financialCustomerId;
            SalesContext = salesContext;
            Buyer = buyer;
            CallerScope = callerScope;
            InitiatingActor = initiatingActor;
        }

        public long OwnerAirlineId { get; }

        public long FinancialCustomerId { get; }

        public SalesContextSnapshot SalesContext { get; }

        public BuyerSnapshot Buyer { get; }

        public string CallerScope { get; }

        public InitiatingActorSnapshot InitiatingActor { get; }

        public SalesChannel Channel => SalesContext.Channel;

        public long? SellingOfficeId => SalesContext.SellingOfficeId;

        public BusinessContextType ActorContextType => InitiatingActor.ContextType;

        public long? ActorId => InitiatingActor.ActorId;
    }
}
