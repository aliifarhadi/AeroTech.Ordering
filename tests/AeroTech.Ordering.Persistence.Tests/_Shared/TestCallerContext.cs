using AeroTech.Messages.Aegis.Enums;
using AeroTech.Messages.Shared.Enums;
using AeroTech.Ordering.Domain._Shared.Contracts;

namespace AeroTech.Ordering.Persistence.Tests._Shared
{
    public sealed class TestCallerContext : ICallerContext
    {
        public bool IsAuthenticated { get; set; }

        public string? Subject { get; set; }

        public string? ClientId { get; set; }

        public BusinessContextType? ContextType { get; set; }

        public PrincipalType? PrincipalType { get; set; }

        public AuthorizationSurface? AuthorizationSurface { get; set; }

        public long? AirlineUserId { get; set; }

        public long? AirlineOfficeId { get; set; }

        public long? TravelAgencyUserId { get; set; }

        public long? TravelAgencyId { get; set; }

        public long? TravelAgencyOfficeId { get; set; }

        public long? IndividualId { get; set; }

        public long? PartnerApiAccessProfileId { get; set; }

        public long? CustomerId { get; set; }

        public string? ServiceCode { get; set; }

        public long? ActorId => AirlineUserId ?? TravelAgencyUserId ?? IndividualId ?? PartnerApiAccessProfileId;

        public TestCallerContext UseBackoffice(long airlineOfficeId, long airlineUserId)
        {
            Reset();
            IsAuthenticated = true;
            AuthorizationSurface = Messages.Shared.Enums.AuthorizationSurface.Backoffice;
            ContextType = BusinessContextType.Airline;
            PrincipalType = Messages.Aegis.Enums.PrincipalType.Human;
            AirlineOfficeId = airlineOfficeId;
            AirlineUserId = airlineUserId;
            Subject = $"airline-user:{airlineUserId}";
            return this;
        }

        public TestCallerContext UseOtaPanel(long travelAgencyId, long travelAgencyOfficeId, long travelAgencyUserId)
        {
            Reset();
            IsAuthenticated = true;
            AuthorizationSurface = Messages.Shared.Enums.AuthorizationSurface.OtaPanel;
            ContextType = BusinessContextType.TravelAgency;
            PrincipalType = Messages.Aegis.Enums.PrincipalType.Human;
            TravelAgencyId = travelAgencyId;
            TravelAgencyOfficeId = travelAgencyOfficeId;
            TravelAgencyUserId = travelAgencyUserId;
            Subject = $"travel-agency-user:{travelAgencyUserId}";
            return this;
        }

        public TestCallerContext UseOta(long customerId, long? partnerApiAccessProfileId = null, long? travelAgencyOfficeId = null)
        {
            Reset();
            IsAuthenticated = true;
            AuthorizationSurface = Messages.Shared.Enums.AuthorizationSurface.Api;
            ContextType = BusinessContextType.PartnerApi;
            PrincipalType = Messages.Aegis.Enums.PrincipalType.TravelAgencyApi;
            CustomerId = customerId;
            PartnerApiAccessProfileId = partnerApiAccessProfileId;
            TravelAgencyOfficeId = travelAgencyOfficeId;
            Subject = $"partner-api:{partnerApiAccessProfileId}";
            return this;
        }

        public TestCallerContext UseAnonymous()
        {
            Reset();
            return this;
        }

        private void Reset()
        {
            IsAuthenticated = false;
            Subject = null;
            ClientId = null;
            ContextType = null;
            PrincipalType = null;
            AuthorizationSurface = null;
            AirlineUserId = null;
            AirlineOfficeId = null;
            TravelAgencyUserId = null;
            TravelAgencyId = null;
            TravelAgencyOfficeId = null;
            IndividualId = null;
            PartnerApiAccessProfileId = null;
            CustomerId = null;
            ServiceCode = null;
        }
    }
}
