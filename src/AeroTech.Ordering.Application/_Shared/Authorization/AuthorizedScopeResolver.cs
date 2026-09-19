using AeroTech.Messages.Aegis.Enums;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Messages.Shared.Enums;
using AeroTech.Ordering.Domain._Shared.Contracts;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Application._Shared.Authorization
{
    public sealed class AuthorizedScopeResolver
    {
        private readonly IHomeOperatorProvider _homeOperatorProvider;
        private readonly ICustomerDirectory _customers;
        private readonly ICallerContext _caller;

        public AuthorizedScopeResolver(
            IHomeOperatorProvider homeOperatorProvider,
            ICustomerDirectory customers,
            ICallerContext caller)
        {
            _homeOperatorProvider = homeOperatorProvider;
            _customers = customers;
            _caller = caller;
        }

        public async Task<AuthorizedSalesScope> BackofficeSaleAsync(long financialCustomerId, long sellingOfficeId, CancellationToken cancellationToken)
        {
            RequireSurface(AuthorizationSurface.Backoffice, CallerScopeKey.Backoffice);

            if (sellingOfficeId != Required(_caller.AirlineOfficeId, "selling office"))
                throw ExceptionFactory.AuthorizedScopeRequired("selling office");

            await EnsureActiveCustomerAsync(financialCustomerId, cancellationToken);

            var ownerAirlineId = await OwnerAirlineIdAsync(cancellationToken);

            return new AuthorizedSalesScope(
                ownerAirlineId,
                financialCustomerId,
                new SalesContextSnapshot(
                    SalesChannel.BackOffice,
                    BusinessContextType.Airline,
                    ownerAirlineId,
                    SellingOfficeKind.AirlineOffice,
                    sellingOfficeId),
                CallerScopeKey.ForSale(CallerScopeKey.Backoffice, financialCustomerId, sellingOfficeId, CallerScopeKey.None),
                new InitiatingActorSnapshot(BusinessContextType.Airline, _caller.AirlineUserId));
        }

        public async Task<AuthorizedSalesScope> OtaPanelSaleAsync(CancellationToken cancellationToken)
        {
            RequireSurface(AuthorizationSurface.OtaPanel, CallerScopeKey.OtaPanel);

            var travelAgencyId = Required(_caller.TravelAgencyId, "travel agency");
            var sellingOfficeId = Required(_caller.TravelAgencyOfficeId, "selling office");
            var financialCustomerId = await TravelAgencyCustomerIdAsync(travelAgencyId, cancellationToken);

            return new AuthorizedSalesScope(
                await OwnerAirlineIdAsync(cancellationToken),
                financialCustomerId,
                new SalesContextSnapshot(
                    SalesChannel.AgencyPanel,
                    BusinessContextType.TravelAgency,
                    travelAgencyId,
                    SellingOfficeKind.TravelAgencyOffice,
                    sellingOfficeId),
                CallerScopeKey.ForSale(CallerScopeKey.OtaPanel, financialCustomerId, sellingOfficeId, CallerScopeKey.Agency(travelAgencyId)),
                new InitiatingActorSnapshot(BusinessContextType.TravelAgency, _caller.TravelAgencyUserId));
        }

        public async Task<AuthorizedSalesScope> OtaSaleAsync(CancellationToken cancellationToken)
        {
            RequireSurface(AuthorizationSurface.Api, CallerScopeKey.Ota);

            var financialCustomerId = Required(_caller.CustomerId, "financial customer");
            var sellingOfficeId = _caller.TravelAgencyOfficeId;

            await EnsureActiveCustomerAsync(financialCustomerId, cancellationToken);

            var principalScope = _caller.PartnerApiAccessProfileId is { } profileId
                ? CallerScopeKey.Partner(profileId)
                : CallerScopeKey.None;

            return new AuthorizedSalesScope(
                await OwnerAirlineIdAsync(cancellationToken),
                financialCustomerId,
                SalesContextSnapshot.SellerNotSupplied(
                    SalesChannel.PartnerAPI,
                    sellingOfficeId is null ? null : SellingOfficeKind.TravelAgencyOffice,
                    sellingOfficeId),
                CallerScopeKey.ForSale(CallerScopeKey.Ota, financialCustomerId, sellingOfficeId, principalScope),
                new InitiatingActorSnapshot(BusinessContextType.PartnerApi, _caller.PartnerApiAccessProfileId));
        }

        public async Task<AuthorizedSalesScope> ServiceSaleAsync(
            long financialCustomerId,
            long? sellingOfficeId,
            SellingOfficeKind? sellingOfficeKind,
            CancellationToken cancellationToken)
        {
            if (sellingOfficeId is null != sellingOfficeKind is null)
                throw ExceptionFactory.AuthorizedScopeRequired("a selling office identifier together with its kind");

            if (sellingOfficeKind == SellingOfficeKind.NotRecorded)
                throw ExceptionFactory.AuthorizedScopeRequired("a recorded selling office kind");

            await EnsureActiveCustomerAsync(financialCustomerId, cancellationToken);

            return new AuthorizedSalesScope(
                await OwnerAirlineIdAsync(cancellationToken),
                financialCustomerId,
                SalesContextSnapshot.SellerNotSupplied(SalesChannel.System, sellingOfficeKind, sellingOfficeId),
                CallerScopeKey.ForSale(CallerScopeKey.Service, financialCustomerId, sellingOfficeId, CallerScopeKey.None),
                new InitiatingActorSnapshot(BusinessContextType.Service, _caller.IsAuthenticated ? _caller.ActorId : null));
        }

        public async Task<AuthorizedReadScope> BackofficeReadAsync(CancellationToken cancellationToken)
        {
            RequireSurface(AuthorizationSurface.Backoffice, CallerScopeKey.Backoffice);

            return new AuthorizedReadScope(await OwnerAirlineIdAsync(cancellationToken), null, true);
        }

        public async Task<AuthorizedReadScope> OtaPanelReadAsync(CancellationToken cancellationToken)
        {
            RequireSurface(AuthorizationSurface.OtaPanel, CallerScopeKey.OtaPanel);

            var travelAgencyId = Required(_caller.TravelAgencyId, "travel agency");
            var financialCustomerId = await TravelAgencyCustomerIdAsync(travelAgencyId, cancellationToken);

            return new AuthorizedReadScope(await OwnerAirlineIdAsync(cancellationToken), financialCustomerId, true);
        }

        public async Task<AuthorizedReadScope> OtaReadAsync(CancellationToken cancellationToken)
        {
            RequireSurface(AuthorizationSurface.Api, CallerScopeKey.Ota);

            var financialCustomerId = Required(_caller.CustomerId, "financial customer");
            await EnsureActiveCustomerAsync(financialCustomerId, cancellationToken);

            return new AuthorizedReadScope(await OwnerAirlineIdAsync(cancellationToken), financialCustomerId, true);
        }

        public async Task<AuthorizedReadScope> ServiceReadAsync(CancellationToken cancellationToken)
            => new(await OwnerAirlineIdAsync(cancellationToken), null, false);

        public async Task<AdministrativeScope> InternalAsync(CancellationToken cancellationToken)
            => new(
                await OwnerAirlineIdAsync(cancellationToken),
                CallerScopeKey.ForAdministration(CallerScopeKey.Internal),
                BusinessContextType.Service,
                _caller.IsAuthenticated ? _caller.ActorId : null);

        public Task<long> OwnerAirlineIdAsync(CancellationToken cancellationToken)
            => _homeOperatorProvider.GetOwnerAirlineIdAsync(cancellationToken);

        private async Task<long> TravelAgencyCustomerIdAsync(long travelAgencyId, CancellationToken cancellationToken)
        {
            var customer = await _customers.FindByTravelAgencyAsync(travelAgencyId, cancellationToken);

            if (customer is null || !customer.IsActive || !customer.IsTravelAgency)
                throw ExceptionFactory.AuthorizedScopeRequired("financial customer");

            return customer.CustomerId;
        }

        private async Task EnsureActiveCustomerAsync(long financialCustomerId, CancellationToken cancellationToken)
        {
            var customer = await _customers.FindAsync(financialCustomerId, cancellationToken);

            if (customer is null || !customer.IsActive)
                throw ExceptionFactory.AuthorizedScopeRequired("financial customer");
        }

        private void RequireSurface(AuthorizationSurface expected, string surface)
        {
            if (!_caller.IsAuthenticated || _caller.AuthorizationSurface != expected)
                throw ExceptionFactory.AuthorizedScopeRequired($"the {surface} surface");
        }

        private static long Required(long? value, string subject)
            => value is { } resolved && resolved > 0 ? resolved : throw ExceptionFactory.AuthorizedScopeRequired(subject);
    }
}
