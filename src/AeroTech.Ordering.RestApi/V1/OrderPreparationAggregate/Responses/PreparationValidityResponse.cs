namespace AeroTech.Ordering.RestApi.V1.OrderPreparationAggregate.Responses
{
    public sealed record PreparationValidityResponse(
        ValidityFactResponse Offer,
        ValidityFactResponse Price,
        ValidityFactResponse Ticketing);
}
