# Source contract matrix — AirOffer

Stage: 08-S1-authoritative-final-closure · 2026-09-21 · branch `k8s-stg`

Source of truth: `docs/ORDERING-DESIGN-PACK-v3.8/CONTRACTS/02-AIROFFER.md` (observed wire),
`POST /Service/v1/FlightOffers/Details`.

Every field the owner supplies is in exactly one of four states. There is no fifth state and no field is unaccounted
for.

| State | Meaning |
|---|---|
| `MODELLED` | Reaches a typed column of the Ordering model. |
| `EVIDENCE` | Not modelled; retained byte-for-byte in `PreparationSourceEvidence.Payload` and recoverable. |
| `RECONCILED` | Consumed as a check against other supplied facts, not stored as its own column. |
| `FAIL-CLOSED` | Supplying it stops the sale with a named error, because consuming it is not approved. |

## 1. Root

| Field | State | Where |
|---|---|---|
| `OfferId` | `MODELLED` | `Order.SourceOfferId`; must equal the requested id — `EnsureRespondedOffer`, scenarios 54/55 |
| `PricedAt` | `MODELLED` | `OrderPreparation.PricedAt` |
| `LastTicketingDate?` | `MODELLED` | `Order.LastTicketingDate` — kept as a source fact, never substituted for the absent validity values (R12) |
| `CurrencyId` | `MODELLED` | `Order.CurrencyId`, and the sale currency of every `Money` |
| `CurrencyCode` | `MODELLED` | `Order.SaleCurrencyCode` (display only; ReferenceData owns the currency) |
| `JourneyType` | `MODELLED` | `Order.JourneyType`, typed; an unknown value fails closed and `CK_Orders_JourneyType_Enum` makes that a storage guarantee |
| `BaseAmount`, `ChargeAmount`, `TotalAmount` | `RECONCILED` | Checked against coupon and ticket totals plus `OrderCharges`; a mismatch is `ContractMismatch` (scenario 40, `SC_S1_015`). Not stored — `Order.CustomerTotal` is computed from the committed lines, not copied. |
| `AirTransports[]`, `PricingUnits[]`, `Tickets[]`, `OrderCharges[]`, `RatesOfExchange[]` | see below | |

## 2. AirTransport / Flight / Leg

| Field | State | Where |
|---|---|---|
| `BoundId`, `Direction`, `Sequence`, bound origin/destination | `MODELLED` | `OrderJourney.BoundId`, `.Direction`, `.Sequence`, `.OriginAirportId`, `.DestinationAirportId` |
| Flight `Sequence`, `FlightId`, `FlightVersion`, `FlightCapacityId`, `FlightNumber` | `MODELLED` | `OrderSegment.*` — all required for a `ScheduledAir` segment (`EnsureScheduledAir`) |
| Flight `OperatingAirlineId`, `MarketingAirlineId`, `AircraftId`, `Duration` | `MODELLED` | `OrderSegment.*` — all required for `ScheduledAir` |
| Flight origin/destination airport and optional terminal ids | `MODELLED` | `OrderSegment.*AirportId` (required), `*AirportTerminalId` (nullable) |
| Flight `DepartureDateTime`, `ArrivalDateTime` | `MODELLED` | `OrderSegment.SoldDeparture`, `.SoldArrival`; arrival before departure is rejected |
| Flight `CabinClassId?`, `RbdId?`, `BookingClass?` | `MODELLED` | On `OrderAirTransportService`, not on the segment — they are per-traveller sold terms |
| Leg `Sequence`, `LegId`, origin/destination airports, `DepartureDateTime`, `ArrivalDateTime` | `MODELLED` | `OrderSegmentLeg.*` — **non-nullable as of this stage**; leg identity and sequence must be unique and positive (`EnsureLegs`) |
| Leg optional terminal ids | `MODELLED` | `OrderSegmentLeg.*AirportTerminalId`, nullable |
| **Flight `Stop?` and Leg `Stop?`** | **`FAIL-CLOSED`** + `EVIDENCE` | See §5 |

## 3. Ticket / Coupon / Pricing

| Field | State | Where |
|---|---|---|
| `TravellerRef` | `MODELLED` | `OrderTraveller.SourceTravellerRef`; a blank, missing or repeated ref fails closed at the ACL |
| `TravellerIndex` | `EVIDENCE` | An array position, not an identity. Proven recoverable by `Coupon_identity_and_sequence_dropped_from_the_domain_model_stay_readable_in_raw_evidence` |
| `PassengerTypeCode` | `MODELLED` | `OrderTraveller.PassengerTypeCode`, typed, guarded by `CK_OrderTravellers_PassengerTypeCode_Enum` (116 members) |
| Ticket `BaseAmount`, `ChargeAmount`, `TotalAmount` | `RECONCILED` | Checked against the coupon rows and the root |
| `CouponId`, coupon `Sequence` | `EVIDENCE` | The Pack states explicitly that Ticket/Coupon "describe a priced projection, NOT issued ETKT/coupons"; storing them as Ordering identities would claim a document that does not exist. Recoverable from evidence — same test as above. |
| Coupon `BoundId`, `FlightId` | `RECONCILED` | Used to bind the service to its segment; the binding itself is `OrderAirTransportService.SegmentId` |
| Coupon baggage / cabin-baggage pieces, weight, unit | `MODELLED` | `CheckedBaggage*`, `CabinBaggage*` owned columns; the unit is typed and guarded |
| `IsRefundable`, `IsChangeable`, `IsUpgradable` | `MODELLED` | `SoldTermRefundable`, `SoldTermChangeable`, `SoldTermUpgradable` |
| Coupon `BaseAmount`, `ChargeAmount`, `TotalAmount` | `RECONCILED` | |
| Pricing `Category` | `MODELLED` | `PricingLine.Component` — Fare/Tax/Fee/Surcharge map explicitly; an unknown category fails mapping (scenario 56) |
| Pricing `Name?`, `Code?`, `Reference?` | `MODELLED` | `PricingLine.Name`, `.Code`, `.Reference` |
| Pricing `Amount`, `CurrencyId`, `EquivalentAmount`, `EquivalentCurrencyId` | `MODELLED` | `PricingLine.OriginalValue` and `.SaleValue`; never added together, never converted locally (scenarios 32–34) |
| Pricing `IsPercentage` | `MODELLED` | `PricingLine.CalculationKind` — the rate is recorded, never interpreted (scenario 34) |
| `RateOfExchangePeriodId?` | `MODELLED` | `PricingLine.SourceConversionRef` and `AppliedConversion` |
| the source occurrence position | `MODELLED` | `PricingLine.SourceOccurrencePath`, e.g. `tickets/0/coupons/0/pricings/1`, unique per set — this is how identical tax codes stay separate occurrences (scenarios 25, 30) |

## 4. PricingUnit / FareComponent / Rate

| Field | State | Where |
|---|---|---|
| `Kind` | `MODELLED` | `FarePricingUnit.Type` + `.SourceConstructionType`; all three real contract values (`OneWay`, `RoundTripFromOneWays`, `RoundTripFare`) map to distinct pairs without collapsing (scenario 51) |
| `CoveredBoundOfferIds[]` | `MODELLED` | `FarePricingUnitCoveredBound.CoveredBoundOfferId`, kept under the owner's exact name and left opaque. **Recorded contract doubt:** the observed live value is a `flightId`, not a `boundId` (`FIELD-INVENTORY.md`, `BLOCKED_REAL_CONTRACT`). S1 stores what the owner sends and does not reinterpret it. |
| `AirFareId`, `FareBasis?`, `FareFamily?`, `FareType`, `CabinClassId?`, `RbdId?`, `BookingClass?`, `TicketingRestrictionMinutes?` | `MODELLED` | `FareComponent.*` |
| Rate `RateOfExchangePeriodId`, `FromCurrencyId`, `ToCurrencyId`, `Rate`, `DecimalPlaces`, `RoundingFactor` | `MODELLED` | `AppliedConversion.*` on the line that used the rate; evidence only, money is never recomputed (scenario 33) |
| `OrderCharges[]` | `MODELLED` | Order-level `PricingLine` rows with `PricingBasisType.OrderItem` (scenario 39) |

## 5. `Stop` — the one FAIL-CLOSED field

Pack `CONTRACTS/02-AIROFFER.md`, Stop row:

> Preserve source stop metadata; it never determines commercial segment or coupon count. **Exact optional stop shape
> requires a captured wire fixture before it is consumed.**

Before this stage the mapper ignored `stop` on both the flight and the leg. That satisfied "never determines
commercial segment or coupon count" but violated the second half: an unapproved shape was being silently dropped from
the commercial model while the sale completed.

As of this stage, a supplied `stop` on a flight or a leg raises `AirOfferUnsupportedException`, which the adapter
turns into `OfferResolutionOutcome.UnsupportedCapability` and the application into `UnsupportedCapability` (20273)
naming `BD-006`. An explicit JSON `null` is not a supplied stop and is accepted.

- "Preserve source stop metadata" is satisfied: the entire owner response, including `stop`, is retained in
  `PreparationSourceEvidence.Payload` and proven byte-identical with its hash.
- **This is a deliberate restriction, not a neutral change.** An offer that supplies a stop can no longer be sold at
  all, where before it was sold with the stop dropped. That is the intended reading of "before it is consumed", and it
  is reversible the moment the owner supplies a captured wire fixture: the guard is one method
  (`AirOfferCandidateMapper.EnsureNoUnsupportedStop`) and the fixture parameters already exist.

Evidence: `SourceEvidenceRetentionTests.A_flight_that_supplies_a_stop_payload_is_an_unsupported_capability_and_persists_no_order`,
`A_leg_that_supplies_a_stop_payload_…`, `An_explicit_json_null_stop_is_not_a_supplied_stop`. Red-first: with the guard
disabled both tests fail (see `TEST-RESULTS.md` §4).

## 6. Explicitly not supplied by the source

The Pack lists these as absent from the DTO; S1 does not fabricate any of them:

authoritative `OfferExpiresAt`, `PriceValidUntil`, an immutable owner snapshot/version token, complete source
sales-context verification evidence, and stable individually priced OfferItem ids.

Consequences carried in the model rather than papered over:

- `OrderPreparation.OfferExpiresAt` / `.PriceValidUntil` are nullable and hold only what the local acceptance profile
  supplies. `LastTicketingDate` is **not** substituted for them (R12).
- There is no owner binding token, so `AcceptanceAssurance` is `LiveCandidateSandbox` for the live path and the Order
  is sandbox-scoped. `BD-001` remains open; production startup rejects the sandbox policy.
- Because there is no stable OfferItem id, S1 uses one local `OfferPackage` item, labelled as a local conservative
  normalization and never as a claimed source OfferItem id (scenario 52,
  `SC_S1_010_observed_details_become_one_conservative_package_with_opaque_construction`).

## 7. Completeness

Every node and field in the Pack's DTO mirror table appears above. The two fields the model deliberately does not
carry (`TravellerIndex`, `CouponId`+coupon `Sequence`) have a test proving they are still recoverable from evidence.
The one field that stops the sale (`Stop`) has three tests and a named blocked decision.
