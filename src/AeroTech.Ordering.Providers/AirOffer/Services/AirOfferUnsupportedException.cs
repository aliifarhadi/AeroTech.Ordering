namespace AeroTech.Ordering.Providers.AirOffer.Services
{
    public sealed class AirOfferUnsupportedException : Exception
    {
        public AirOfferUnsupportedException(string message)
            : base(message)
        {
        }
    }
}
