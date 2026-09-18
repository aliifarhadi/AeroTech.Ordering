using AeroTech.Ordering.Domain.OrderPreparationAggregate.Contracts;
using AeroTech.Ordering.Providers.AirOffer;
using AeroTech.Ordering.Providers.Deterministic.Offers;

namespace AeroTech.Ordering.ServiceHost.Composition
{
    public sealed class AcceptanceProfilePolicy : IAcceptanceProfilePolicy
    {
        private static readonly IReadOnlySet<string> NonProductionProfiles = new HashSet<string>(StringComparer.Ordinal)
        {
            AirOfferProfile.LiveCandidateSandbox,
            ReferenceOfferProfile.ProfileId
        };

        private readonly bool _isProduction;

        public AcceptanceProfilePolicy(IHostEnvironment environment)
        {
            _isProduction = environment.IsProduction();
            EnvironmentClass = environment.EnvironmentName;
        }

        public string EnvironmentClass { get; }

        public bool Permits(string acceptanceProfile) => !_isProduction && NonProductionProfiles.Contains(acceptanceProfile);
    }
}
