# AirOffer Wire-to-Domain Loss Audit

Stage: 06-S1-create-order-conformance · **revision 2** · 2026-09-19 · HEAD `506ccee`

Field enumeration and destinations are unchanged from revision 1 and were re-verified. Only the §9 closure target changed.

## Method

Every serialized property of every class under `src/AeroTech.Ordering.Providers/AirOffer/Wire/` was enumerated mechanically from source (regex over `public T Name { get; set; }`), then each was checked for a **receiver-qualified** read in `AirOfferCandidateMapper.cs` and `AirOfferSourceAdapter.cs`. Receiver-qualified matching matters: a naive `.OfferId` grep reports `AirOfferDetailsWire.OfferId` as read because the mapper uses `request.OfferId`. It does not read `details.OfferId`.

**Total serialized wire fields: 105.** Read: **99**. Not read: **6**.

## 1. Root — `AirOfferDetailsWire`

| Wire class.field | Type | Mapper reads? | Candidate destination | Order destination | SQL destination | Projection destination | Public API? | Raw evidence retained? | Semantics known? | Loss? | Decision |
|---|---|---|---|---|---|---|---|---|---|---|---|
| `.OfferId` | `string?` | **NO** | — | — | — | — | no | yes | yes | **YES — integrity** | **FIX_S1_SEMANTICS** — see §9 |
| `.PricedAt` | `DateTimeOffset` | yes | `NormalizedCandidate.PricedAt` | `AcceptedSource.PricedAt` | `Orders.SourcePricedAt` | — | no | yes | yes | no | KEEP |
| `.LastTicketingDate` | `DateTimeOffset?` | yes | `CandidateValidity.ObservedTicketingDeadline` | `Order.ObservedTicketingDeadline` | `Orders.ObservedTicketingDeadline*` | `ObservedTicketingDeadline` | no | yes | observed only; ownership BD-004 | no | KEEP |
| `.CurrencyId` | `int` | yes | `CustomerTotal.CurrencyRef` | `Order.SaleCurrency.CurrencyRef` | `Orders.SaleCurrencyRef` | `SaleCurrencyRef` | yes (`grandTotal.currencyRef`) | yes | yes | no | KEEP |
| `.CurrencyCode` | `string?` | yes | `SaleCurrencyCode` | `Order.SaleCurrency.CurrencyCode` | `Orders.SaleCurrencyCode` | `SaleCurrencyCode` | no | yes | yes | no | KEEP |
| `.JourneyType` | `string?` | yes | `SourceJourneyTypeRaw` | `Order.SourceJourneyTypeRaw` | `Orders.SourceJourneyTypeRaw` | `SourceJourneyTypeRaw` | no | yes | **no vocabulary** | raw kept, canonical `NULL` | **BLOCKED_OWNER_CONTRACT** (`OR-002`) |
| `.BaseAmount` | `decimal` | yes | reconciliation only | — | — | — | no | yes | yes | derivable | KEEP |
| `.ChargeAmount` | `decimal` | yes | reconciliation only | — | — | — | no | yes | yes | derivable | KEEP |
| `.TotalAmount` | `decimal` | yes | `CustomerTotal.Amount`, package `AcceptedTotal` | `Order.CustomerTotal`, `OrderItem.AcceptedTotal` | `Orders.CustomerTotalAmount`, `OrderItems.AcceptedTotalAmount` | `GrandTotal` | yes | yes | yes | no | KEEP |
| `.AirTransports[]` | list | yes | `Journeys` + `Segments` | `OrderJourney`, `OrderSegment` | `OrderJourneys`, `OrderSegments` | yes | partly | yes | yes | no | KEEP |
| `.PricingUnits[]` | list | yes | `FareConstruction.PricingUnits` | `FarePricingUnit` | `FarePricingUnits` | yes | no | yes | yes | no | KEEP |
| `.Tickets[]` | list | yes | `Travelers`, `Services`, `PricingLines` | traveler/service/line graph | those tables | yes | partly | yes | yes | no | KEEP |
| `.OrderCharges[]` | list | yes | `PricingLines` (basis `OrderItem`) | `PricingLine` | `PricingLines` | `Pricing` | yes (amount) | yes | yes | no | KEEP |
| `.RatesOfExchange[]` | list | yes | `AppliedConversion` | `PricingLine.AppliedConversion` | `PricingLines.Conversion*` | `AppliedConversion` | no | yes | partly — see §10 | no | KEEP |

## 2. `AirOfferEnvelopeWire` / `AirOfferErrorWire`

| Field | Read? | Destination | Loss? | Decision |
|---|---|---|---|---|
| `Envelope.Data` | yes | the whole candidate | no | KEEP |
| `Envelope.Errors` | yes | failure reason text on `CandidateResolution` | no | KEEP |
| `Error.Code` | yes | failure reason | no | KEEP |
| `Error.Title` | yes | failure reason | no | KEEP |
| `Error.Detail` | yes | failure reason | no | KEEP |

## 3. `AirOfferAirTransportWire` (bound)

| Field | Read? | Candidate | Order | SQL | Projection | Public | Loss? | Decision |
|---|---|---|---|---|---|---|---|---|
| `.BoundId` | yes | `CandidateJourney.JourneyRef` | `OrderJourney.SourceBoundRef` | `OrderJourneys.SourceBoundRef` | `Journeys[].SourceBoundRef` | no | no | KEEP |
| `.Direction` | yes (raw) | `SourceDirectionRaw` | `OrderJourney.SourceDirectionRaw` | `OrderJourneys.SourceDirectionRaw` | yes | no | raw kept, canonical `NULL` | **BLOCKED_OWNER_CONTRACT** (`OR-002`) |
| `.Sequence` | yes | `CandidateJourney.Sequence` | `OrderJourney.Sequence` | `OrderJourneys.Sequence` | yes | no | no | KEEP |
| `.OriginAirportId` | yes | `CandidateJourney.OriginRef` | `OrderJourney.OriginRef` | `OrderJourneys.OriginRef` | yes | no | no | KEEP |
| `.DestinationAirportId` | yes | `CandidateJourney.DestinationRef` | `OrderJourney.DestinationRef` | `OrderJourneys.DestinationRef` | yes | no | no | KEEP |
| `.Flights[]` | yes | `CandidateSegment[]` | `OrderSegment` | `OrderSegments` | yes | partly | no | KEEP |

## 4. `AirOfferFlightWire` (passenger segment)

| Field | Read? | Candidate | Order | SQL | Projection | Public | Loss? | Decision |
|---|---|---|---|---|---|---|---|---|
| `.Sequence` | yes | ordering of `Segments` | `OrderSegment.Sequence` | `OrderSegments.Sequence` | yes | yes | no | KEEP |
| `.CabinClassId` | yes | service `Details[cabinRef]` | `AirTransportDetail.CabinRef` | `AirTransportServiceDetails.CabinRef` | yes | yes | no | KEEP |
| `.RbdId` | yes | `Details[rbdRef]` | `AirTransportDetail.RbdRef` | `.RbdRef` | yes | yes | no | KEEP |
| `.BookingClass` | yes | `Details[bookingClass]` | `AirTransportDetail.BookingClass` | `.BookingClass` | yes | yes | no | KEEP |
| `.FlightCapacityId` | yes | `SourceCapacityRef` | `OrderSegment.SourceCapacityRef` | `OrderSegments.SourceCapacityRef` | yes | no | retained as a source reference only; BD-002/BD-003 govern any use | KEEP |
| `.FlightId` | yes | `CandidateSegment.FlightRef` | `OrderSegment.FlightRef` | `OrderSegments.FlightRef` | yes | yes | no | KEEP |
| `.FlightVersion` | yes | `CandidateSegment.FlightVersion` | `OrderSegment.FlightVersion` | `OrderSegments.FlightVersion` | yes | **no** (deliberate, OD-P-21) | no | KEEP |
| `.FlightNumber` | yes | `CandidateSegment.FlightNumber` | `OrderSegment.FlightNumber` | `OrderSegments.FlightNumber` | yes | yes (via segment) | no | KEEP |
| `.OriginAirportId` | yes | `OriginRef` | `OrderSegment.OriginRef` | `.OriginRef` | yes | yes | no | KEEP |
| `.OriginAirportTerminalId` | yes | `OriginTerminalRef` | `OrderSegment.OriginTerminalRef` | `.OriginTerminalRef` | yes | no | no | KEEP |
| `.DestinationAirportId` | yes | `DestinationRef` | `OrderSegment.DestinationRef` | `.DestinationRef` | yes | yes | no | KEEP |
| `.DestinationAirportTerminalId` | yes | `DestinationTerminalRef` | `.DestinationTerminalRef` | `.DestinationTerminalRef` | yes | no | no | KEEP |
| `.OperatingAirlineId` | yes | `OperatingCarrierRef` | `OrderSegment.OperatingCarrierRef` | `.OperatingCarrierRef` | yes | yes (via segment) | no | KEEP |
| `.MarketingAirlineId` | yes | `MarketingCarrierRef` | `OrderSegment.MarketingCarrierRef` | `.MarketingCarrierRef` | yes | yes (via segment) | no | KEEP |
| `.DepartureDateTime` | yes | `SoldDeparture` | `OrderSegment.SoldDeparture` | `.SoldDeparture` | yes | yes | no | KEEP |
| `.ArrivalDateTime` | yes | `SoldArrival` | `OrderSegment.SoldArrival` | `.SoldArrival` | yes | yes | no | KEEP |
| `.Duration` | yes | `CandidateSegment.Duration` | `OrderSegment.Duration` | `.Duration` | yes | no | no | KEEP |
| `.AircraftId` | yes | `AircraftRef` | `OrderSegment.AircraftRef` | `.AircraftRef` | yes | no | no | KEEP |
| `.Stop` | **NO** | — | — | — | — | no | yes | **no vocabulary** | **BLOCKED_OWNER_CONTRACT** (`OD-P-12`, handoff `OR-002`) |
| `.Legs[]` | yes | `CandidateSegmentLeg[]` | `OrderSegmentLeg` | `OrderSegmentLegs` | yes | partly | no | KEEP |

## 5. `AirOfferLegWire`

| Field | Read? | Candidate | Order | SQL | Projection | Public | Loss? | Decision |
|---|---|---|---|---|---|---|---|---|
| `.Sequence` | yes | `CandidateSegmentLeg.Sequence` | `OrderSegmentLeg.Sequence` | `OrderSegmentLegs.Sequence` | yes | yes | no | KEEP |
| `.LegId` | yes | `SourceLegRef` | `OrderSegmentLeg.SourceLegRef` | `.SourceLegRef` | yes | yes | no | KEEP |
| `.OriginAirportId` | yes | `OriginRef` | `.OriginRef` | `.OriginRef` | yes | no | no | KEEP |
| `.OriginAirportTerminalId` | yes | `OriginTerminalRef` | `.OriginTerminalRef` | `.OriginTerminalRef` | yes | no | no | KEEP |
| `.DestinationAirportId` | yes | `DestinationRef` | `.DestinationRef` | `.DestinationRef` | yes | no | no | KEEP |
| `.DestinationAirportTerminalId` | yes | `DestinationTerminalRef` | `.DestinationTerminalRef` | `.DestinationTerminalRef` | yes | no | no | KEEP |
| `.DepartureDateTime` | yes | `Departure` | `.Departure` | `.Departure` | yes | no | no | KEEP |
| `.ArrivalDateTime` | yes | `Arrival` | `.Arrival` | `.Arrival` | yes | no | no | KEEP |
| `.Stop` | **NO** | — | — | — | — | no | yes | **no vocabulary** | **BLOCKED_OWNER_CONTRACT** (`OD-P-12`) |

## 6. `AirOfferTicketWire` (priced traveler projection)

| Field | Read? | Candidate | Order | SQL | Projection | Public | Loss? | Decision |
|---|---|---|---|---|---|---|---|---|
| `.TravellerRef` | yes | `CandidateTraveler.SourceTravellerRef`, service ref, line basis ref | `OrderTraveler.SourceTravellerRef` | `OrderTravelers.SourceTravellerRef` | `Travellers[].OfferTravellerRef` | yes | no | KEEP |
| `.TravellerIndex` | **NO** | — | — | — | — | no | yes | positional only | **KEEP** — see §11 |
| `.PassengerTypeCode` | yes | `CandidateTraveler.PassengerTypeCode` | `OrderTraveler.PassengerTypeCode` | `.PassengerTypeCode` | yes | yes | no | KEEP |
| `.BaseAmount` | yes | reconciliation only | — | — | — | no | yes | derivable by summing that traveler's lines | KEEP |
| `.ChargeAmount` | yes | reconciliation only | — | — | — | no | yes | derivable | KEEP |
| `.TotalAmount` | yes | reconciliation only | — | — | — | no | yes | derivable | KEEP |
| `.Coupons[]` | yes | `CandidateService[]`, `CandidatePricingLine[]` | `OrderService`, `PricingLine` | those tables | yes | partly | no | KEEP |

## 7. `AirOfferCouponWire` (priced coupon projection — **not** an issued ETKT coupon)

| Field | Read? | Candidate | Order | SQL | Projection | Public | Loss? | Decision |
|---|---|---|---|---|---|---|---|---|
| `.CouponId` | **NO** | — | — | — | — | no | yes | owner occurrence id | **KEEP** — see §11 |
| `.Sequence` | **NO** | — | — | — | — | no | yes | ordinal recoverable from `SourceLineRef` path | **KEEP** — see §11 |
| `.BoundId` | yes | segment ref resolution | `OrderServiceCoverage` | `OrderServiceCoverage` | `SegmentIds` | yes | no | KEEP |
| `.FlightId` | yes | segment ref resolution | same | same | same | yes | no | KEEP |
| `.BaggagePieces` | yes | `CheckedBaggage.Pieces` | `AirTransportDetail.CheckedBaggage` | `AirTransportServiceDetails.CheckedBaggagePieces` | yes | no | no | KEEP |
| `.BaggageWeight` | yes | `CheckedBaggage.Weight` | same | `.CheckedBaggageWeight` | yes | no | no | KEEP |
| `.BaggageUnit` | yes | `CheckedBaggage.WeightUnit` | same | `.CheckedBaggageWeightUnit` | yes | no | `KG`/`LB`/`LBS` mapped; anything else is `ContractMismatch` | KEEP |
| `.CabinBaggagePieces` | yes | `CabinBaggage.Pieces` | `.CabinBaggage` | `.CabinBaggagePieces` | yes | no | no | KEEP |
| `.CabinBaggageWeight` | yes | `CabinBaggage.Weight` | same | `.CabinBaggageWeight` | yes | no | no | KEEP |
| `.CabinBaggageUnit` | yes | `CabinBaggage.WeightUnit` | same | `.CabinBaggageWeightUnit` | yes | no | no | KEEP |
| `.IsRefundable` | yes | `SoldTermFlags.Refundable` | `OrderService.SoldTerms` + `OrderItem.CommercialTerms` | `OrderServices.SoldTermRefundable`, `OrderItems.TermsRefundability` | yes | no | display only, authorizes nothing | KEEP |
| `.IsChangeable` | yes | `SoldTermFlags.Changeable` | same | `.SoldTermChangeable`, `.TermsChangeability` | yes | no | same | KEEP |
| `.IsUpgradable` | yes | `SoldTermFlags.Upgradable` | same | `.SoldTermUpgradable`, `.TermsUpgradeEligibility` | yes | no | same | KEEP |
| `.BaseAmount` | yes | reconciliation only | — | — | — | no | yes | derivable | KEEP |
| `.ChargeAmount` | yes | reconciliation only | — | — | — | no | yes | derivable | KEEP |
| `.TotalAmount` | yes | reconciliation only | — | — | — | no | yes | derivable | KEEP |
| `.Pricings[]` | yes | `CandidatePricingLine[]` | `PricingLine` | `PricingLines` | `Pricing` | yes | no | KEEP |

## 8. Pricing, fare construction and exchange

### `AirOfferPricingLineWire`

| Field | Read? | Candidate | Order | SQL | Projection | Public | Loss? | Decision |
|---|---|---|---|---|---|---|---|---|
| `.Category` | yes | `Component` | `PricingLine.Component` | `PricingLines.Component` | yes | yes | mapped `0..3 → Fare/Tax/Fee/CarrierSurcharge` per `CONTRACTS/02-AIROFFER`; unknown value is `ContractMismatch` | KEEP |
| `.Name` | yes | `SourceName` | `PricingLine.SourceName` | `.SourceName` | yes | **no** | no | KEEP (public exposure is a separate owner choice) |
| `.Code` | yes | `SourceCode` | `.SourceCode` | `.SourceCode` | yes | **no** | no | KEEP (same) |
| `.Reference` | yes | `SourceReference` | `.SourceReference` | `.SourceReference` | yes | no | no | KEEP |
| `.Amount` | yes | `OriginalValue.Amount` | `.OriginalValue` | `.OriginalValueAmount` | yes | yes | no | KEEP |
| `.CurrencyId` | yes | `OriginalValue.CurrencyRef` | same | `.OriginalValueCurrencyRef` | yes | yes | no | KEEP |
| `.IsPercentage` | yes | `CalculationKind` | `PricingLine.CalculationKind` | `.CalculationKind` | yes | no | flag preserved; the numeric basis is **not** interpreted (OD-P-18) | KEEP |
| `.EquivalentAmount` | yes | `SaleValue.Amount` when the equivalent is the sale valuation | `.SaleValue` | `.SaleValueAmount` | yes | yes | no | KEEP |
| `.EquivalentCurrencyId` | yes | `SaleValue.CurrencyRef` | same | `.SaleValueCurrencyRef` | yes | yes | no | KEEP |
| `.RateOfExchangePeriodId` | yes | `SourceConversionRef` + conversion lookup | `.SourceConversionRef`, `.AppliedConversion` | `.SourceConversionRef`, `.Conversion*` | yes | no | no | KEEP |

### `AirOfferPricingUnitWire`

| Field | Read? | Candidate | Order | SQL | Projection | Loss? | Decision |
|---|---|---|---|---|---|---|---|
| `.Kind` | yes | `SourceKindRaw` | `FarePricingUnit.SourceKindRaw` | `FarePricingUnits.SourceKindRaw` | yes | raw kept; canonical `Type` stays `Unspecified` because the vocabulary is not contractual | KEEP |
| `.CoveredBoundOfferIds[]` | yes | `CoveredSourceBoundRefs` | `FarePricingUnitCoveredBound` | `FarePricingUnitCoveredBounds` | yes | no | KEEP |
| `.FareComponents[]` | yes | `CandidateFareComponent[]` | `FareComponent` | `FareComponents` | yes | no | KEEP |

### `AirOfferFareComponentWire`

| Field | Read? | Candidate | Order | SQL | Projection | Loss? | Decision |
|---|---|---|---|---|---|---|---|
| `.AirFareId` | yes | `SourceFareRef` | `FareComponent.SourceFareRef` | `FareComponents.SourceFareRef` | yes | no | KEEP |
| `.CabinClassId` | yes | `CabinRef` | `.CabinRef` | `.CabinRef` | yes | no | KEEP |
| `.RbdId` | yes | `RbdRef` | `.RbdRef` | `.RbdRef` | yes | no | KEEP |
| `.BookingClass` | yes | `BookingClass` | `.BookingClass` | `.BookingClass` | yes | no | KEEP |
| `.FareBasis` | yes | `FareBasis` | `.FareBasis` | `.FareBasis` | yes | no | KEEP |
| `.FareFamily` | yes | `FareFamily` | `.FareFamily` | `.FareFamily` | yes | no | KEEP |
| `.FareType` | yes | `FareType` | `.FareType` | `.FareType` | yes | no | KEEP |
| `.TicketingRestrictionMinutes` | yes | `TicketingRestrictionMinutes` | `.TicketingRestrictionMinutes` | `.TicketingRestrictionMinutes` | yes | no | KEEP |

### `AirOfferRateOfExchangeWire`

| Field | Read? | Candidate | Order | SQL | Projection | Loss? | Decision |
|---|---|---|---|---|---|---|---|
| `.RateOfExchangePeriodId` | yes | `AppliedConversion.SourceConversionRef` | same | `PricingLines.ConversionSourceRef` | yes | no | KEEP |
| `.FromCurrencyId` | yes | `.FromCurrencyRef` | same | `.ConversionFromCurrencyRef` | yes | see §10 | KEEP |
| `.ToCurrencyId` | yes | `.ToCurrencyRef` | same | `.ConversionToCurrencyRef` | yes | see §10 | KEEP |
| `.Rate` | yes | `.Rate` | same | `.ConversionRate` (28,12) | yes | no | KEEP |
| `.DecimalPlaces` | yes | `.DecimalPlaces` | same | `.ConversionDecimalPlaces` | yes | no | KEEP |
| `.RoundingFactor` | yes (raw text) | `.RoundingToken` | same | `.ConversionRoundingToken` | yes | preserved verbatim, used by **no** calculation | KEEP |

## 9. Critical integrity check — `data.offerId != requestedOfferId`

**Question:** can a successful AirOffer Details response whose `data.offerId` differs from the requested offer id currently be accepted?

**Answer: yes, and it is accepted silently.**

Evidence:

- `AirOfferSourceAdapter.Interpret` calls `AirOfferCandidateMapper.Map(request.OfferId, envelope.Data, …)`. It never compares `envelope.Data.OfferId` to `request.OfferId`.
- Receiver-qualified search for `details.OfferId` / `Data.OfferId` across the mapper and the adapter returns **no match**.
- `AirOfferCandidateMapper` writes `requestedOfferId` into `CandidateSource.OfferId` and into `ProductSnapshot.SourceOfferId`.

**Consequence.** If AirOffer answers 200 for a different offer, the Order is accepted with the priced content of offer B while its accepted provenance — `Orders.AcceptedSourceOfferId`, `OrderItems.ProductSourceOfferId` and the public `offerId` on the response — all claim offer A. Money, journeys, services and fare construction come from B. The raw payload in `PreparationSourceEvidence` still holds B's id, so the mismatch is detectable afterwards, but the accepted record is wrong and the client is told it bought A.

This defeats INV-006 in substance ("Create consumes exactly one previously captured, explicitly accepted preparation; no external calls or **silent repricing**") — a silently substituted offer is the strongest form of silent repricing.

**Classification: S1 contract-integrity defect.**

**Smallest fail-closed validation** (documented, not implemented in this run): in `AirOfferCandidateMapper.Map`, before any other work,

> if `details.OfferId` is null, empty, or not ordinally equal to `requestedOfferId`, throw `AirOfferContractMismatchException`.

That is one comparison inside the existing mismatch path, so the adapter already maps it to `OfferResolutionOutcome.ContractMismatch` and nothing is persisted. No alias, equivalence, normalisation or case-folding semantics are invented.

**Revision 2 correction.** Revision 1 recommended tolerating a missing `data.offerId` "because the observed contract marks it optional (`string?`)". That reasoning used our own C# mirror class as contract authority, which it is not. `CONTRACTS/02-AIROFFER.md` line 13 lists the root fields as `OfferId, PricedAt, LastTicketingDate?, CurrencyId, CurrencyCode, JourneyType, …` — `LastTicketingDate?` is the **only** root field the Pack marks optional, and `OfferId` carries no `?`. A response with no offer id is therefore already off-contract, and accepting it would leave an Order whose provenance rests on nothing the owner returned. Missing and mismatched are both `ContractMismatch`, and both need a negative test.

## 10. Rate-of-exchange semantics — known limit

The recorded live AirOffer response contains a fare line in currency `155` with an equivalent in `70` that references rate period `1533121255006273536`, whose own row reads `fromCurrencyId 71 → toCurrencyId 70`. The referenced rate's currencies therefore do **not** correspond to the line's currencies, and no published contract explains the relationship.

The adapter consequently preserves the rate row exactly as the source states it and correlates nothing; a referenced period absent from `ratesOfExchange` leaves `AppliedConversion` null while `SourceConversionRef` is still preserved. `RoundingFactor` is stored as raw text and read by no calculation.

**Semantics known: partly.** The evidence is complete; the *meaning* of the from/to pair relative to the line is `NOT PUBLICLY PROVEN` and is included in handoff `OR-002` scope. No loss.

## 11. The four unread non-`Stop` fields

| Field | Why it is unread | Is the fact lost? | Decision |
|---|---|---|---|
| `AirOfferDetailsWire.OfferId` | never compared — neither for absence nor for disagreement | **the check is missing**, not the fact | **FIX_S1_SEMANTICS** (§9, closure target corrected in revision 2) |
| `AirOfferCouponWire.CouponId` | the service identity is synthesised as `{TravellerRef}\|{BoundId}\|{FlightId}` | in the normalized record yes; in raw evidence no | **KEEP** — justified in `DOMAIN-BENCHMARK-MATRIX.md` §C4 |
| `AirOfferCouponWire.Sequence` | the coupon ordinal survives positionally inside every `SourceLineRef` (`tickets/{i}/coupons/{j}/pricings/{k}`) | no — recoverable from the line path | **KEEP** |
| `AirOfferTicketWire.TravellerIndex` | `TravellerRef` is the identity the caller binds against and it is persisted | in the normalized record yes; ordering is reconstructable from the raw payload | **KEEP** |

## 12. Totals

| Outcome | Count |
|---|---|
| Wire fields audited | **105** |
| Read and preserved (or provably derivable) | **99** |
| Deliberately not consumed, with a recorded reason | **3** (`CouponId`, `Coupon.Sequence`, `TravellerIndex`) |
| Blocked by an owner-contract gap | **2** (`Flight.Stop`, `Leg.Stop`) |
| Lost through a defect | **1** (`Details.OfferId` — not lost as data, lost as a **validation**) |
| Silently lost with no reason recorded | **0** |
