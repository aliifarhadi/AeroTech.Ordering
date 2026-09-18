using AeroTech.Ordering.Domain.Ports.Offers;
using AeroTech.Ordering.Providers.AirOffer.Options;
using AeroTech.Ordering.Providers.AirOffer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AeroTech.Ordering.Providers
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddProviders(this IServiceCollection services, IConfiguration configuration)
        {
            var offer = configuration.GetSection(AirOfferOptions.SectionName).Get<AirOfferOptions>() ?? new AirOfferOptions();

            services.Configure<AirOfferOptions>(configuration.GetSection(AirOfferOptions.SectionName));
            services.AddHttpClient<IOfferSourcePort, AirOfferSourceAdapter>(client =>
            {
                if (!string.IsNullOrWhiteSpace(offer.BaseUrl))
                    client.BaseAddress = new Uri(offer.BaseUrl);
            });

            return services;
        }
    }
}
