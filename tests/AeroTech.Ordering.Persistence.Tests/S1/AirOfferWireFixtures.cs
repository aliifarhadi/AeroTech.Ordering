using System.Net;
using System.Text;
using System.Text.Json;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    public static class AirOfferWireFixtures
    {
        public static string Details(
            decimal couponFare = 100m,
            decimal couponTax = 20m,
            decimal? rootTotal = null,
            object? category = null,
            bool twoLegs = true,
            string passengerType = "ADT",
            bool percentage = false,
            int lineCurrencyId = 978,
            decimal? equivalent = null,
            decimal? percentageOrderCharge = null,
            bool withTerminals = false,
            int? ticketingRestrictionMinutes = null,
            decimal? conversionRate = null,
            string? offerId = "SYNTHETIC-PRICED-OFFER")
        {
            var couponTotal = couponFare + couponTax;
            var saleFare = lineCurrencyId == 978 ? couponFare : equivalent ?? couponFare;
            var orderChargeTotal = percentageOrderCharge ?? 0m;

            var body = new
            {
                data = new
                {
                    offerId,
                    pricedAt = "2026-09-08T09:59:00+00:00",
                    lastTicketingDate = "2026-09-10T12:00:00+03:30",
                    currencyId = 978,
                    currencyCode = "EUR",
                    journeyType = "OneWay",
                    baseAmount = couponFare,
                    chargeAmount = couponTax + orderChargeTotal,
                    totalAmount = rootTotal ?? couponTotal + orderChargeTotal,
                    airTransports = new[]
                    {
                        new
                        {
                            boundId = "BOUND-1",
                            direction = "Outbound",
                            sequence = 1,
                            originAirportId = "11",
                            destinationAirportId = "22",
                            flights = new[]
                            {
                                new
                                {
                                    sequence = 1,
                                    cabinClassId = (int?)3,
                                    rbdId = "44",
                                    bookingClass = "Y",
                                    flightCapacityId = "5001",
                                    flightId = "7001",
                                    flightVersion = 4,
                                    flightNumber = "AT101",
                                    originAirportId = 11,
                                    originAirportTerminalId = withTerminals ? (int?)61 : null,
                                    destinationAirportId = 22,
                                    destinationAirportTerminalId = withTerminals ? (int?)62 : null,
                                    operatingAirlineId = 1,
                                    marketingAirlineId = 1,
                                    departureDateTime = "2026-09-20T08:00:00+03:30",
                                    arrivalDateTime = "2026-09-20T12:30:00+03:30",
                                    duration = 270,
                                    aircraftId = 9,
                                    stop = (object?)null,
                                    legs = twoLegs
                                        ? new object[]
                                        {
                                            new { sequence = 1, legId = "81", originAirportId = 11, destinationAirportId = 33, departureDateTime = "2026-09-20T08:00:00+03:30", arrivalDateTime = "2026-09-20T09:30:00+03:30" },
                                            new { sequence = 2, legId = "82", originAirportId = 33, destinationAirportId = 22, departureDateTime = "2026-09-20T10:30:00+03:30", arrivalDateTime = "2026-09-20T12:30:00+03:30" }
                                        }
                                        : new object[]
                                        {
                                            new { sequence = 1, legId = "81", originAirportId = 11, destinationAirportId = 22, departureDateTime = "2026-09-20T08:00:00+03:30", arrivalDateTime = "2026-09-20T12:30:00+03:30" }
                                        }
                                }
                            }
                        }
                    },
                    pricingUnits = new[]
                    {
                        new
                        {
                            kind = "OneWay",
                            coveredBoundOfferIds = new[] { "BOUND-1" },
                            fareComponents = new[]
                            {
                                new { airFareId = "9001", cabinClassId = (int?)3, rbdId = "44", bookingClass = "Y", fareBasis = "YOW", fareFamily = "FLEX", fareType = "Published", ticketingRestrictionMinutes = ticketingRestrictionMinutes }
                            }
                        }
                    },
                    tickets = new[]
                    {
                        new
                        {
                            travellerRef = "T1",
                            travellerIndex = 0,
                            passengerTypeCode = passengerType,
                            baseAmount = couponFare,
                            chargeAmount = couponTax,
                            totalAmount = couponTotal,
                            coupons = new[]
                            {
                                new
                                {
                                    couponId = "T1-7001",
                                    sequence = 1,
                                    boundId = "BOUND-1",
                                    flightId = "7001",
                                    baggagePieces = 1,
                                    baggageWeight = 20,
                                    baggageUnit = "KG",
                                    cabinBaggagePieces = 1,
                                    cabinBaggageWeight = 7,
                                    cabinBaggageUnit = "KG",
                                    isRefundable = true,
                                    isChangeable = true,
                                    isUpgradable = false,
                                    baseAmount = couponFare,
                                    chargeAmount = couponTax,
                                    totalAmount = couponTotal,
                                    pricings = new object[]
                                    {
                                        new { category = category ?? "Fare", name = "Fare", code = "YOW", reference = "9001", amount = couponFare, currencyId = lineCurrencyId, isPercentage = percentage, equivalentAmount = saleFare, equivalentCurrencyId = 978, rateOfExchangePeriodId = lineCurrencyId == 978 ? null : "ROE-1" },
                                        new { category = "Tax", name = "Airport tax", code = "AT", reference = (string?)null, amount = couponTax, currencyId = 978, isPercentage = false, equivalentAmount = couponTax, equivalentCurrencyId = 978, rateOfExchangePeriodId = (string?)null }
                                    }
                                }
                            }
                        }
                    },
                    orderCharges = percentageOrderCharge is { } charge
                        ? new object[]
                        {
                            new { category = 1, name = "IR", code = "IR", reference = "ROE-70", amount = 10.0m, currencyId = 0, isPercentage = true, equivalentAmount = charge, equivalentCurrencyId = 978, rateOfExchangePeriodId = "70" }
                        }
                        : Array.Empty<object>(),
                    ratesOfExchange = lineCurrencyId == 978 && percentageOrderCharge is null
                        ? Array.Empty<object>()
                        : new object[]
                        {
                            new { rateOfExchangePeriodId = "ROE-1", fromCurrencyId = lineCurrencyId, toCurrencyId = 978, rate = conversionRate ?? 1.0m, decimalPlaces = 2, roundingFactor = 100 },
                            new { rateOfExchangePeriodId = "70", fromCurrencyId = 978, toCurrencyId = 978, rate = 1.0m, decimalPlaces = 2, roundingFactor = 100 }
                        }
                },
                errors = (object?)null
            };

            return JsonSerializer.Serialize(body);
        }

        public sealed class StubHandler : HttpMessageHandler
        {
            private readonly Func<HttpStatusCode> _status;
            private readonly Func<string> _body;

            public StubHandler(Func<string> body, Func<HttpStatusCode>? status = null)
            {
                _body = body;
                _status = status ?? (() => HttpStatusCode.OK);
            }

            public int Calls { get; private set; }

            public string? LastRequestPath { get; private set; }

            public string? LastRequestBody { get; private set; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Calls++;
                LastRequestPath = request.RequestUri!.AbsolutePath;
                LastRequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

                return new HttpResponseMessage(_status()) { Content = new StringContent(_body(), Encoding.UTF8, "application/json") };
            }
        }
    }
}
