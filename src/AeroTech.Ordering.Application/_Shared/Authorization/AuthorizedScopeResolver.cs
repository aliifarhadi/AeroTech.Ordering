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

        public async Task<AuthorizedSalesScope> ResolveAsync(SalesScopeRequest request, CancellationToken cancellationToken)
        {
            var ownerAirlineId = await _homeOperatorProvider.GetOwnerAirlineIdAsync(cancellationToken);

            return request.Surface switch
            {
                OrderingApiSurface.Backoffice => await BackofficeAsync(ownerAirlineId, request, cancellationToken),
                OrderingApiSurface.OtaPanel => await OtaPanelAsync(ownerAirlineId, request, cancellationToken),
                OrderingApiSurface.Ota => await OtaAsync(ownerAirlineId, request, cancellationToken),
                OrderingApiSurface.Service => await ServiceAsync(ownerAirlineId, request, cancellationToken),
                _ => throw ExceptionFactory.AuthorizedScopeRequired("a sale surface")
            };
        }

        public async Task<AdministrativeScope> ResolveAsync(AdministrativeScopeRequest request, CancellationToken cancellationToken)
        {
            var ownerAirlineId = await _homeOperatorProvider.GetOwnerAirlineIdAsync(cancellationToken);

            return new AdministrativeScope(
                ownerAirlineId,
                CallerScopeKey.ForAdministration(request.Surface, CallerScopeKey.None),
                BusinessContextType.Service,
                _caller.IsAuthenticated ? _caller.ActorId : null);
        }

        public async Task<AuthorizedReadScope> ResolveReadScopeAsync(OrderingApiSurface surface, CancellationToken cancellationToken)
        {
            var ownerAirlineId = await _homeOperatorProvider.GetOwnerAirlineIdAsync(cancellationToken);

            switch (surface)
            {
                case OrderingApiSurface.Backoffice:
                    RequireSurface(AuthorizationSurface.Backoffice, surface);
                    return new AuthorizedReadScope(ownerAirlineId, null, true);

                case OrderingApiSurface.OtaPanel:
                {
                    RequireSurface(AuthorizationSurface.OtaPanel, surface);
                    var travelAgencyId = Required(_caller.TravelAgencyId, "travel agency");
                    var customer = await _customers.FindByTravelAgencyAsync(travelAgencyId, cancellationToken);

                    if (customer is null || !customer.IsActive || !customer.IsTravelAgency)
                        throw ExceptionFactory.AuthorizedScopeRequired("financial customer");

                    return new AuthorizedReadScope(ownerAirlineId, customer.CustomerId, true);
                }

                case OrderingApiSurface.Ota:
                {
                    RequireSurface(AuthorizationSurface.Api, surface);
                    var financialCustomerId = Required(_caller.CustomerId, "financial customer");
                    await EnsureActiveCustomerAsync(financialCustomerId, cancellationToken);
                    return new AuthorizedReadScope(ownerAirlineId, financialCustomerId, true);
                }

                case OrderingApiSurface.Service:
                case OrderingApiSurface.Internal:
                    return new AuthorizedReadScope(ownerAirlineId, null, false);

                default:
                    throw ExceptionFactory.AuthorizedScopeRequired("an unknown api surface");
            }
        }

        public Task<long> OwnerAirlineIdAsync(CancellationToken cancellationToken)
            => _homeOperatorProvider.GetOwnerAirlineIdAsync(cancellationToken);

        private async Task<AuthorizedSalesScope> BackofficeAsync(long ownerAirlineId, SalesScopeRequest request, CancellationToken cancellationToken)
        {
            RequireSurface(AuthorizationSurface.Backoffice, OrderingApiSurface.Backoffice);

            var financialCustomerId = Required(request.FinancialCustomerId, "financial customer");
            var sellingOfficeId = Required(request.SellingOfficeId, "selling office");
            var permittedOfficeId = Required(_caller.AirlineOfficeId, "selling office");

            if (sellingOfficeId != permittedOfficeId)
                throw ExceptionFactory.AuthorizedScopeRequired("selling office");

            await EnsureActiveCustomerAsync(financialCustomerId, cancellationToken);

            return new AuthorizedSalesScope(
                ownerAirlineId,
                financialCustomerId,
                SalesChannel.BackOffice.ToString(),
                sellingOfficeId,
                CallerScopeKey.ForSale(OrderingApiSurface.Backoffice, financialCustomerId, sellingOfficeId, CallerScopeKey.None),
                BusinessContextType.Airline,
                _caller.AirlineUserId);
        }

        private async Task<AuthorizedSalesScope> OtaPanelAsync(long ownerAirlineId, SalesScopeRequest request, CancellationToken cancellationToken)
        {
            RequireSurface(AuthorizationSurface.OtaPanel, OrderingApiSurface.OtaPanel);

            var travelAgencyId = Required(_caller.TravelAgencyId, "travel agency");
            var sellingOfficeId = Required(_caller.TravelAgencyOfficeId, "selling office");

            var customer = await _customers.FindByTravelAgencyAsync(travelAgencyId, cancellationToken);

            if (customer is null || !customer.IsActive || !customer.IsTravelAgency)
                throw ExceptionFactory.AuthorizedScopeRequired("financial customer");

            EnsureNotDelegated(request, customer.CustomerId, sellingOfficeId);

            return new AuthorizedSalesScope(
                ownerAirlineId,
                customer.CustomerId,
                SalesChannel.AgencyPanel.ToString(),
                sellingOfficeId,
                CallerScopeKey.ForSale(OrderingApiSurface.OtaPanel, customer.CustomerId, sellingOfficeId, CallerScopeKey.Agency(travelAgencyId)),
                BusinessContextType.TravelAgency,
                _caller.TravelAgencyUserId);
        }

        private async Task<AuthorizedSalesScope> OtaAsync(long ownerAirlineId, SalesScopeRequest request, CancellationToken cancellationToken)
        {
            RequireSurface(AuthorizationSurface.Api, OrderingApiSurface.Ota);

            var financialCustomerId = Required(_caller.CustomerId, "financial customer");
            var sellingOfficeId = _caller.TravelAgencyOfficeId;

            await EnsureActiveCustomerAsync(financialCustomerId, cancellationToken);
            EnsureNotDelegated(request, financialCustomerId, sellingOfficeId);

            var principalScope = _caller.PartnerApiAccessProfileId is { } profileId
                ? CallerScopeKey.Partner(profileId)
                : CallerScopeKey.None;

            return new AuthorizedSalesScope(
                ownerAirlineId,
                financialCustomerId,
                SalesChannel.PartnerAPI.ToString(),
                sellingOfficeId,
                CallerScopeKey.ForSale(OrderingApiSurface.Ota, financialCustomerId, sellingOfficeId, principalScope),
                BusinessContextType.PartnerApi,
                _caller.PartnerApiAccessProfileId);
        }

        private async Task<AuthorizedSalesScope> ServiceAsync(long ownerAirlineId, SalesScopeRequest request, CancellationToken cancellationToken)
        {
            var financialCustomerId = Required(request.FinancialCustomerId, "financial customer");
            var sellingOfficeId = request.SellingOfficeId;

            await EnsureActiveCustomerAsync(financialCustomerId, cancellationToken);

            return new AuthorizedSalesScope(
                ownerAirlineId,
                financialCustomerId,
                SalesChannel.System.ToString(),
                sellingOfficeId,
                CallerScopeKey.ForSale(OrderingApiSurface.Service, financialCustomerId, sellingOfficeId, CallerScopeKey.None),
                BusinessContextType.Service,
                _caller.IsAuthenticated ? _caller.ActorId : null);
        }

        private async Task EnsureActiveCustomerAsync(long financialCustomerId, CancellationToken cancellationToken)
        {
            var customer = await _customers.FindAsync(financialCustomerId, cancellationToken);

            if (customer is null || !customer.IsActive)
                throw ExceptionFactory.AuthorizedScopeRequired("financial customer");
        }

        private void RequireSurface(AuthorizationSurface expected, OrderingApiSurface surface)
        {
            if (!_caller.IsAuthenticated || _caller.AuthorizationSurface != expected)
                throw ExceptionFactory.AuthorizedScopeRequired($"the {CallerScopeKey.Surface(surface)} surface");
        }

        private static void EnsureNotDelegated(SalesScopeRequest request, long financialCustomerId, long? sellingOfficeId)
        {
            if (request.FinancialCustomerId is { } requested && requested != financialCustomerId)
                throw ExceptionFactory.AuthorizedScopeRequired("financial customer");

            if (request.SellingOfficeId is { } requestedOffice && requestedOffice != sellingOfficeId)
                throw ExceptionFactory.AuthorizedScopeRequired("selling office");
        }

        private static long Required(long? value, string subject)
            => value is { } resolved && resolved > 0 ? resolved : throw ExceptionFactory.AuthorizedScopeRequired(subject);
    }
}
