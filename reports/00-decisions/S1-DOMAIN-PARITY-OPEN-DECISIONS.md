# S1 Domain Parity — open decisions (OD-P-01 … OD-P-20)

Raised by: `reports/05-S1-domain-parity-audit/DOMAIN-PARITY-MATRIX.md`
Repository HEAD: `43df88a` — no domain change has been made.
Every item below changes semantics, persistence identity/cardinality or a public contract, so none is implemented before the owner writes an answer on its **Answer:** line.

Legend for my line: **Recommendation** is my position as architect, with the reason. It is not a decision.

---

## Group 1 — monetary and currency truth

### OD-P-01 — source currency code is discarded
`Order.SaleCurrencyRef` and `Money.CurrencyRef` hold the stringified AirOffer `CurrencyId`. `details.CurrencyCode` is on the wire and never stored. DOMAIN/03 §31 requires "source CurrencyId **and** source CurrencyCode snapshot".
**Recommendation:** store both on the accepted sale — keep `CurrencyRef` as the id and add the accepted `CurrencyCode` next to it on the Order and on each `Money`-bearing accepted row. ReferenceData stays the display authority for anything current; this is the accepted historical snapshot, which reference data may never rewrite.
**Answer:**

### OD-P-16 — rates of exchange are discarded
`AirOfferRateOfExchangeWire` (from currency, to currency, rate, decimal places, rounding factor) is parsed and thrown away; only `RateOfExchangePeriodId` survives as `PricingLine.SourceConversionRef`. The historical model had an `ExchangeRate` value object on both line and allocation.
**Recommendation:** restore an `ExchangeRate` value object on `PricingLine` carrying the accepted rate, its decimal places and rounding factor, plus the period id. Without it a converted line cannot be re-explained to revenue accounting.
**Answer:**

### OD-P-18 — percentage rows lose the fact that they were percentages
Per OD-S1-08 a percentage row is materialised as sale = original = the equivalent amount. The `IsPercentage` flag itself is dropped, so a 5 % fee and a flat fee of the same value are indistinguishable afterwards.
**Recommendation:** persist the source basis of the row (flat vs percentage, plus the percentage value when supplied) on the pricing line. It is a source fact, not a derived one.
**Answer:**

---

## Group 2 — the fare construction graph (the largest divergence)

### OD-P-19 — replace `FareConstruction.PricingUnitsJson` with the Pack's relational graph
DOMAIN/13 names the table group `FareConstructions / Groups / Units / Components / Bindings`. DOMAIN/03 §47–51 requires pricing groups with exact grouped travelers and PTC, units with source identity and combination method, and components with **explicit covered ServiceIds or traveler+segment pairs** plus fare owner, rule, routing and tariff references. Today the entire graph is one JSON column produced inside `Order.AcceptOriginalSale`, components cover nothing, and unit type and combination method are hard-coded constants. The historical `k8s-stg` model had every one of these as a typed entity.
This single decision covers matrix rows F1–F6 and is the precondition for exchange, reissue and revenue accounting later.
**Recommendation:** restore the typed graph natively — `FareConstruction` → `FarePricingGroup` (+ traveler links) → `FarePricingUnit` → `FareComponent` (+ service links, + segment links), with an item-binding link table — and delete `PricingUnitsJson`. Where AirOffer supplies no value (unit type, combination method), persist an explicit "not supplied by source" enum member rather than a guessed constant. `TicketingRestrictionMinutes` is carried on the component.
**Answer:**

---

## Group 3 — item snapshots required by DOMAIN/02 §9

### OD-P-03 — `OrderItem` has no `ProductSnapshot`, no product code/name, no quantity
DOMAIN/02 §7 and §9 make `ProductSnapshot` a field of `OrderItem`: source product id/code/type/name, brand/family/version, supplier product reference, immutable validated attributes. None exists here; the historical model had `OrderItemProductSnapshot`.
**Recommendation:** restore it as an owned snapshot of the item, populated from the accepted candidate (source offer id, source item ref, product name, brand code/name where supplied, marketing and operating carrier, accepted-at). Add product code/name and quantity/unit of measure to the item itself.
**Answer:**

### OD-P-04 — refundable / changeable / upgradable are read and thrown away
`AirOfferCouponWire.IsRefundable`, `.IsChangeable`, `.IsUpgradable` are deserialised and never used. DOMAIN/02 §9 requires a `CommercialTermsSnapshot` holding exactly these display summaries plus policy reference/version and the capture time, and warns that a display flag is not a runnable refund algorithm.
**Recommendation:** restore `CommercialTermsSnapshot` on the item with three tri-state term summaries (`Permitted` / `NotPermitted` / `NotSupplied`), the source system, and the capture instant. Display only — no eligibility behavior in S1.
The values are per coupon on the wire but the item is the whole offer package; if the coupons of one offer disagree, the item-level summary must be recorded as `NotSupplied` rather than silently taking the first value. Confirm that rule.
**Answer:**

---

## Group 4 — services

### OD-P-06 — `OrderService` is missing Pack-required fields
DOMAIN/02 §19 lists ServiceCode, Name, PriceTreatment, SupplierPartyRef and DeliveryProviderRef among the service's fields; the historical model had all of them. None exists here.
**Recommendation:** restore `ServiceCode`, `Name` and `PriceTreatment` now, because they are decidable from the accepted source at S1. Leave `SupplierPartyRef` and `DeliveryProviderRef` out until a slice actually has a supplier or delivery provider — adding empty columns is not preservation.
**Answer:**

### OD-P-07 — the fulfillment profile is invented by the adapter
`AirOfferCandidateMapper` hard-codes `AIROFFER-OBSERVED-AIR-UNCERTIFIED`, `CapacityUnits = 1`, `ReservationRequirement.FlightCapacity`, `DocumentKind.Etkt` and `RequiresFunding = true`. DOMAIN/02 §23 additionally requires a profile **version**, and states the snapshot must not drift with the catalogue.
**Recommendation:** the profile is Ordering-owned reference data, not an adapter constant. Until an owner publishes one, keep the constant but bind it to a named options-bound profile definition with an explicit version, so the snapshot records which definition was in force. No hard-coded literal in the mapper.
**Answer:**

### OD-P-08 — baggage allowance is discarded
`AirOfferCouponWire` carries `BaggagePieces / BaggageWeight / BaggageUnit` and the three cabin-baggage equivalents. All are dropped. The historical air-transport detail carried checked and cabin baggage as a `Baggage` value object.
**Recommendation:** restore checked and cabin baggage on the accepted air-transport detail as a value object (pieces, weight, unit). This is the allowance that was sold; it cannot be re-derived later from the offer.
**Answer:**

---

## Group 5 — journeys, segments, legs

### OD-P-09 — there is no Journey
DOMAIN/04 §15 makes Journey the grouping of customer travel, and DOMAIN/13 names `Journeys` as a table group. AirOffer supplies `airTransports[].BoundId`, `Direction`, `Sequence`, `OriginAirportId`, `DestinationAirportId` and `details.JourneyType`; all are discarded and segments hang directly off the order. The historical model had `OrderItinerary` with exactly these fields.
**Recommendation:** restore a journey entity between Order and Segment (bound id, sequence, direction, origin, destination) and record the offer's journey type on the order. Outbound/inbound is not derivable from segment order alone, and every downstream surface (display, change, refund scope) needs it.
**Answer:**

### OD-P-10 — the sold segment lost its operational identity
`OrderSegment` keeps only origin/destination refs, sold times and a flight ref. Dropped from the wire: origin and destination **terminals**, `Duration`, `AircraftId`, `FlightCapacityId`, `FlightVersion`, marketing and operating carrier, cabin and RBD. DOMAIN/04 §15 requires the sold marketing/operating identity, the sold schedule snapshot and the external flight id **and version** on the segment.
**Recommendation:** restore them on the segment as the sold snapshot. `FlightCapacityId` in particular is the FlightFlow handle the reservation slice will need; losing it now means re-reading the offer later, which is not possible after expiry.
**Answer:**

### OD-P-11 — legs keep nothing but a reference
`OrderSegmentLeg` holds a sequence and `SourceLegRef`. Leg airports, terminals, departure and arrival times and `Stop` are parsed off the wire and dropped.
**Recommendation:** restore the leg's airports, terminals and times. A multi-leg segment is currently unreadable without re-calling the source.
**Answer:**

### OD-P-12 — the `Stop` vocabulary is undocumented
`AirOfferFlightWire.Stop` and `AirOfferLegWire.Stop` are `JsonElement` and never parsed. DOMAIN/04 §19 requires connection facts as explicit source facts (Connection / Stopover / SurfaceBreak / Unknown, Protected / Unprotected / Unknown) and forbids deriving protection from elapsed time.
**Blocked:** no contract in the Pack or in `CONTRACTS/02-AIROFFER` states the values this field can take. I will not guess a vocabulary.
**Recommendation:** raise a handoff to the AirOffer owner (`E:\Projects\DotAir\handoffs`) for the published `Stop` vocabulary, and until it is answered persist the stop fact as `Unknown` rather than dropping the field.
**Answer:**

---

## Group 6 — pricing line identity

### OD-P-15 — pricing lines lose Code, Name and Reference
`AirOfferPricingLineWire` supplies `Name`, `Code` and `Reference` on every row. `PricingLine` stores none of them; the only identity is `SourceLineRef`, a JSON path such as `tickets/0/coupons/1/pricings/2`. The historical line had `Code` and `Description`. A tax code is the primary key of a tax to any airline user, and to revenue accounting.
**Recommendation:** restore `Code`, `Name` and the source `Reference` on the pricing line, and surface code and name in the order projection.
**Answer:**

### OD-P-17 — no quantity, unit of measure or unit price on the line
The historical line carried all three; DOMAIN/03 §49 warns that line values are extended once and must not be multiplied again by group quantity, which presumes the extension is recorded.
**Recommendation:** restore quantity, unit of measure and unit price as nullable source facts — populated only when the source supplies them, never computed by us.
**Answer:**

### OD-P-14 — `PriceChangeSet` collapses the pricing source
The historical set kept `Source`, `SourceOfferId` and `SourcePricingRef` separately; here they are one `SourceDecisionRef` string.
**Recommendation:** split them back. A single opaque string cannot answer "which offer priced this" without parsing.
**Answer:**

### OD-P-20 — the numeric pricing-category map is inferred, not published
`AirOfferCandidateMapper` maps category values `0,1,2,3` to Fare, Tax, Fee, CarrierSurcharge. This mapping was inferred from observed payloads; no published contract states it. A silent renumbering on the AirOffer side would mis-classify money.
**Recommendation:** handoff to the AirOffer owner for the published enum, and until it is answered treat an unknown numeric category as a contract mismatch (it already throws) **and** record the observed raw category on the accepted line so a later correction is possible.
**Answer:**

---

## Group 7 — smaller items

### OD-P-02 — the ticketing deadline survives only as prose
AirOffer's `LastTicketingDate` is embedded in the free-text reason of a `NotSupplied` `ValidityFact`. The historical model had a typed `OrderTimeLimit` (type, due-at, status, settled-at). BD-004 (ticketing deadline ownership) is still open.
**Recommendation:** keep the ownership question open, but persist the observed instant in a typed field on the ticketing validity fact instead of inside a sentence. Recording an observed value is not claiming authority over it.
**Answer:**

### OD-P-05 — `OrderItemServiceLink` does not record when the link was made
DOMAIN/13 calls these tables append-only. The historical link carried `LinkedAt`.
**Recommendation:** add `LinkedAt`. One column, and the table is otherwise unauditable.
**Answer:**

### OD-P-13 — contacts are flatter than the historical model
The historical model had `OrderContact` + `OrderContactPoint` (type, value, country code, primary flag). Here a contact is a role with one optional email and one optional phone, which was fixed by the ratified stage-04 cleanup decision and is reflected in the published request bodies and OpenAPI.
**Blocked:** restoring contact points changes a public contract that the owner already ratified.
**Recommendation:** leave it as it is for S1. Revisit when a slice actually needs several contact points or a phone country code.
**Answer:**

---

## Still open from stage 04 (unchanged, repeated here so nothing is lost)

- **OD-C-01b** — schema for `CommandReceipts` (order tables are `Order`, answered).
- **OD-C-02** — `SalesChannel` is used inside Domain although GOVERNANCE/05 C1 allows only the Ordering enums plus three caller-context types. Either the allowlist gains `SalesChannel` or the Domain stops referencing it.
- **OD-C-03 … OD-C-10** — as recorded in `S1-CLEANUP-OPEN-DECISIONS.md`.
