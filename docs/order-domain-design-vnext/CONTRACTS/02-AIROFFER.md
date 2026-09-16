# AirOffer - observed wire and accepted-source target contract

## Owner and current evidence

AirOffer owns offer composition/bundling/offer validity. AirPrice remains the desired pricing decision owner. Source inspection of AirOffer's service details implementation shows its current response is built by invoking `IOfferPricePipeline.PriceAsync` using the commercial context decoded from the priced OfferId. That is not an immutable read-back of an earlier accepted price.

Observed route: `POST /Service/v1/FlightOffers/Details`. Body: `{ "offerId": "<opaque priced offer id>" }`. Controller returns `BaseResult<ServiceFlightOfferDetailDto>`; CLR wrapper properties are Data and Errors, error fields Code/Title/Detail. JSON casing/enum serializer settings must be captured in the live wire fixture; tolerate documented casing in the adapter, not arbitrary semantic variants. Pricing category CLR values are Fare=0, Tax=1, Fee=2, Surcharge=3. Unknown enum value fails mapping.

## DTO mirror sufficient for implementation

| Node | Observed fields |
|---|---|
| Root | OfferId, PricedAt, LastTicketingDate?, CurrencyId, CurrencyCode, JourneyType, BaseAmount, ChargeAmount, TotalAmount, AirTransports[], PricingUnits[], Tickets[], OrderCharges[], RatesOfExchange[] |
| AirTransport | BoundId, Direction, Sequence, OriginAirportId, DestinationAirportId, Flights[] |
| Flight | Sequence, CabinClassId?, RbdId?, BookingClass?, FlightCapacityId, FlightId, FlightVersion, FlightNumber, origin/destination airport and optional terminal IDs, OperatingAirlineId, MarketingAirlineId, DepartureDateTime, ArrivalDateTime, Duration, AircraftId, Stop?, Legs[] |
| Leg | Sequence, LegId, origin/destination airport and optional terminal IDs, DepartureDateTime, ArrivalDateTime, Stop? |
| Stop | Preserve source stop metadata; it never determines commercial segment or coupon count. Exact optional stop shape requires a captured wire fixture before it is consumed. |
| Ticket projection | TravellerRef, TravellerIndex, PassengerTypeCode, BaseAmount, ChargeAmount, TotalAmount, Coupons[] |
| Coupon projection | CouponId, Sequence, BoundId, FlightId, baggage/cabin-baggage pieces/weight/unit, IsRefundable, IsChangeable, IsUpgradable, BaseAmount, ChargeAmount, TotalAmount, Pricings[] |
| PricingUnit | Kind, CoveredBoundOfferIds[], FareComponents[] |
| FareComponent | AirFareId, CabinClassId?, RbdId?, BookingClass?, FareBasis?, FareFamily?, FareType, TicketingRestrictionMinutes? |
| PricingLine | Category, Name?, Code?, Reference? (fare/charge reference), Amount, CurrencyId, IsPercentage, EquivalentAmount, EquivalentCurrencyId, RateOfExchangePeriodId? |
| Rate | RateOfExchangePeriodId, FromCurrencyId, ToCurrencyId, Rate, DecimalPlaces, RoundingFactor |

Some numeric identity fields permit strings via JsonNumberHandling. Implement lossless string/number ID parsing within this known contract. Keep money decimal, not binary floating point. The Ticket/Coupon names describe a priced projection, NOT issued ETKT/coupons.

Not supplied by this DTO: authoritative OfferExpiresAt, PriceValidUntil, immutable owner snapshot/version token, complete source sales-context verification evidence, or stable individually priced OfferItem IDs. LastTicketingDate is retained as a source fact and is not substituted for those absent values. Do not infer its ultimate ownership from its field name.

## Required semantic methods

`IOfferSourcePort.ResolveCandidate` is a pre-acceptance query/price-resolution operation: it may yield a different candidate than an earlier shopping screen. `ReadBoundCandidate` is a separate required capability only when an owner supplies immutable accepted binding/read-back. It must not be implemented by calling current Details and pretending the result is the original.

S1 introduces `OrderPreparation`, a LOCAL record containing source payload/normalized candidate, actor/customer context, local digest/canonicalization version, priced/captured instants, all provided and not-supplied validity facts and binding assurance. Display the result and obtain explicit acceptance of this exact digest before Create. Create does no AirOffer/AirPrice call and cannot substitute a newer snapshot. No inventory/payment mutation occurs in either standard preparation or Create.

## Conservative normalization profile

For the observed full projection use one local `OfferPackage` item containing all selected air services and package/order charges. Label this as a local conservative normalization, NOT a claimed source OfferItem ID or PricingUnit. Do not infer independent sale contracts from per-traveler ticket totals. Each air service maps a unique TravellerRef plus passenger Flight/Bound occurrence; physical Legs remain nested itinerary detail.

Retain each coupon pricing-row occurrence with a stable local occurrence path/reference; identical tax/fare codes do not imply one charge. Fare/Tax/Fee/Surcharge map explicitly to Fare/Tax/Fee/CarrierSurcharge. Choose the supplied sale-currency valuation: Amount when CurrencyId equals root sale currency, or EquivalentAmount when its currency matches; when both match require consistency. Never add both valuations, multiply group quantity again or convert locally. Reconcile coupon, traveler and root totals including OrderCharges. Ambiguous/missing valuation is ContractMismatch, not zero.

Keep source allocations as `SourceAllocatedFact` provenance. They do not establish refund entitlement or independent fare components. Incomplete fare-construction mapping is retained as OpaquePricingContext with available source unit/component data; granular pricing requiring more information remains blocked. No inferred PU grouping by route shape.

## Assurance and real-offer stage gate

The target fully authoritative path requires an owner token or explicitly approved immutable snapshot/validation contract binding owner/customer/channel, scope, accepted price and applicable validity. BD-001 covers the missing production binding. A locally calculated SHA256 proves only local snapshot identity.

The `LIVE-CANDIDATE-SANDBOX` profile permits end-to-end verification with a REAL AirOffer response before owner binding is certified. Its preparations and resulting Orders are explicitly sandbox-scoped and not eligible for production Reserve/Issue. This is not silently treated as completed real-sale certification. The Domain sees assurance/profile facts, not a simulator flag; production startup rejects this sandbox policy.

S1 must produce a live captured (redacted) candidate, exact local Create/Get/replay/restart results and `LIVE_CANDIDATE_PROVEN`, then separately show `LIVE_ACCEPTANCE_BLOCKED(BD-001)` until the owner contract is available. No test-only backdoor may mark an unverified source authoritative. A fully capable reference offer simulator supplies authoritative target binding for local stage progression.
