namespace AeroTech.Ordering.Providers.AirOffer.Services
{
    public sealed class AirOfferContractMismatchException : Exception
    {
        public AirOfferContractMismatchException(string message)
            : base(message)
        {
        }
    }
}
