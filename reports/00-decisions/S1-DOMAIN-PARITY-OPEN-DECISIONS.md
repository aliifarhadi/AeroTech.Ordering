# S1 Domain Parity — decisions OD-P-01 … OD-P-21

Raised by: `reports/05-S1-domain-parity-audit/DOMAIN-PARITY-MATRIX.md`
Owner answers recorded: 2026-09-19, on repository HEAD `deaf6b0` (branch `k8s-stg`, clean tree).
Status: OD-P-01 … OD-P-21 are **answered**. `OD-P-12` stays blocked by the owner's own answer and is the only remaining domain-parity blocker.
The approved repairs were implemented on 2026-09-19; see `reports/05-S1-domain-parity-audit/REPORT.md`.

Legend: **Recommendation** was my position as architect when the item was raised. **Answer** is the owner's decision and is binding. Where the owner overruled me the correction is stated in the answer, and the parity matrix was corrected accordingly.

---

## Group 1 — monetary and currency truth

### OD-P-01 — source currency code is discarded
`Order.SaleCurrencyRef` and `Money.CurrencyRef` hold the stringified AirOffer `CurrencyId`. `details.CurrencyCode` is on the wire and never stored. DOMAIN/03 §31 requires "source CurrencyId **and** source CurrencyCode snapshot".
**Recommendation:** store both on the accepted sale, next to the currency id, on the Order and on each accepted monetary row.
**Answer: APPROVED WITH CORRECTION.** Preserve both the source currency identity and the source currency code **only where the accepted source actually supplies the code**.
- `Order` retains the accepted sale `CurrencyId`/ref **and** the `CurrencyCode` snapshot.
- An accepted monetary value may carry a source currency code snapshot only when the source supplied it, or unambiguously supplied it, for that value.
- Never query current ReferenceData and call the result an accepted historical source snapshot.
- A foreign/original monetary row that supplies only a `CurrencyId` keeps the id and records the code as not supplied (null). Do not invent it.
- ReferenceData stays the current display/enrichment authority, never historical accepted truth.
Disposition: `REDESIGN`.

### OD-P-16 — rates of exchange are discarded
`AirOfferRateOfExchangeWire` (from currency, to currency, rate, decimal places, rounding factor) is parsed and thrown away; only `RateOfExchangePeriodId` survives as `PricingLine.SourceConversionRef`.
**Recommendation:** restore an `ExchangeRate` value object on the pricing line.
**Answer: APPROVED WITH CORRECTION.** Preserve the applied conversion evidence for each accepted pricing line when the source supplies it: source conversion/rate reference, from-currency identity, to-currency identity, accepted rate, decimal-places metadata, supplied rounding evidence, original value, sale/equivalent value.
- Implement it as a small immutable typed applied-conversion snapshot owned by `PricingLine`, resolved from the source `RateOfExchangePeriodId`.
- Do not build an FX engine or a rate-table subsystem.
- Do not recompute accepted money.
- Preserve the source rounding token losslessly, but do not interpret or use it in any calculation until its semantics are owner-confirmed.
- Do not mechanically copy the historical `ExchangeRate` implementation if a smaller typed snapshot satisfies Pack 3.8.
Disposition: `RESTORE` (as a new typed snapshot, not the legacy type).

### OD-P-18 — percentage rows lose the fact that they were percentages
Per OD-S1-08 a percentage row is materialised as sale = original = the equivalent amount, and `IsPercentage` is dropped.
**Recommendation:** persist the source basis of the row, including the percentage value when supplied.
**Answer: PARTIALLY APPROVED; THE SEMANTIC INTERPRETATION REMAINS BLOCKED.** The source fact `IsPercentage` must no longer be discarded: persist enough canonical provenance to distinguish a fixed/amount-based source row from a percentage-based source row.
- Do **not** assert that `AirOfferPricingLineWire.Amount` is the percentage rate or value unless an owner contract explicitly confirms that meaning.
- Do not turn the source flag into a local pricing formula.
- Do not recompute the accepted equivalent monetary amount.
- The raw source evidence already preserves the untouched row.
Disposition: `RESTORE` for the flag/provenance. The numeric percentage basis stays uninterpreted and needs no code.

---

## Group 2 — the fare construction graph

### OD-P-19 — replace `FareConstruction.PricingUnitsJson` with the Pack's relational graph
DOMAIN/13 names the table group `FareConstructions / Groups / Units / Components / Bindings`; DOMAIN/03 §47–51 requires pricing groups, units and components with explicit coverage. Today the whole graph is one JSON column, components cover nothing, and unit type and combination method are hard-coded constants.
**Recommendation:** restore the typed graph and delete `PricingUnitsJson`.
**Answer: APPROVED, WITH AN OPAQUE-SOURCE CORRECTION.** Remove the canonical `PricingUnitsJson` shortcut and restore typed fare-construction structure able to represent: FareConstruction, item binding, PricingGroup and traveler binding when actually known, PricingUnit, FareComponent, service/segment bindings when actually known, source fare identity, FareBasis, FareFamily/brand where supplied, FareType, cabin, RBD, booking class, fare owner/rule/routing/tariff refs when supplied, `TicketingRestrictionMinutes`, source unit/component references, source covered-bound references.
**Do not fabricate missing structure for AirOffer**, which is explicitly an opaque/incomplete construction source:
- if AirOffer does not provide traveler pricing groups, do not invent PricingGroups from equal PTC or ticket order;
- if AirOffer does not prove per-FareComponent service/segment coverage, do not fabricate component bindings;
- preserve `CoveredBoundOfferIds` at the level actually supplied;
- preserve `PricingUnit.Kind` as a source fact;
- map a canonical `FarePricingUnitType` only when its vocabulary is contractually known, otherwise `Unspecified`;
- `FareCombinationMethod` must be `Unspecified` when the source did not establish it — do **not** hard-code `ProviderDefined` merely because a provider returned the row;
- `FareConstructionAssurance.Opaque` remains correct for current AirOffer.
The relational model must support partial/opaque construction without forcing fake rows to satisfy a hierarchy.
Disposition: `REDESIGN` plus the required `RESTORE` substructures.

---

## Group 3 — item snapshots required by DOMAIN/02 §9

### OD-P-03 — `OrderItem` has no `ProductSnapshot`, no product code/name, no quantity
**Recommendation:** restore `ProductSnapshot`, and add product code/name and quantity/unit of measure to the item itself.
**Answer: APPROVED IN PART; ITEM QUANTITY/UOM REJECTED.** Restore the Pack-required `ProductSnapshot` / accepted product context on `OrderItem`, preserving only facts actually supplied or justified by the accepted normalization. For the current AirOffer package that is: source system/owner, source OfferId, source OfferItemRef only when supplied, item kind / accepted package identity, and source product code/name/brand/version only when supplied.
Do not:
- invent a source product id, code or name;
- collapse per-segment marketing/operating carriers into one package-level carrier;
- copy historical singular carrier fields onto a multi-flight package unless the source explicitly defines that item-level value.
**Item quantity and unit of measure are not added.** Pack 3.8 does not require them for this S1 package and the current source supplies no meaningful single item quantity.
Disposition: `ProductSnapshot` → `RESTORE`; item quantity/UOM → rejected.

### OD-P-04 — refundable / changeable / upgradable are read and thrown away
**Recommendation:** restore an item-level `CommercialTermsSnapshot`, and treat disagreement between coupons as `NotSupplied`.
**Answer: APPROVED WITH A GRANULARITY CORRECTION.** Preserve the exact source flags supplied per AirOffer coupon/service — `IsRefundable`, `IsChangeable`, `IsUpgradable` — as display/source facts that authorize no servicing behavior.
1. Preserve the exact per-service accepted source flags at the service / typed-air-detail level, or in an equally explicit typed sale snapshot, so their original granularity is not lost.
2. Restore the Pack-required item-level `OrderItem.CommercialTermsSnapshot` as a display summary.
3. Item-level summary mapping: all supplied members true → `Permitted`; all supplied members false → `Prohibited`; a mixture → `Conditional`; no supplied fact → `Unknown`.
4. `NotSupplied` must **not** represent disagreement — disagreement is not missing data. (`CommercialTermState` has exactly `Unknown / Prohibited / Permitted / Conditional`, so the corrected mapping is directly implementable.)
5. Do not implement refund/change/upgrade eligibility from these display summaries.
Retain source policy/version references only when actually supplied.
Disposition: `RESTORE`.

---

## Group 4 — services

### OD-P-06 — `OrderService` is missing Pack-required fields
**Recommendation:** restore `ServiceCode`, `Name` and `PriceTreatment` now and leave supplier/delivery refs out.
**Answer: APPROVED STRUCTURALLY; NO INVENTED VALUES.** Bring the accepted `OrderService` shape back into conformance with DOMAIN/02 and plan for `ServiceCode`, `Name`, `PriceTreatment`, `SupplierPartyRef` and `DeliveryProviderRef`. For current AirOffer:
- `ServiceCode` and `Name` stay null / not-supplied unless the owner supplies them;
- do not use `FlightNumber` as a fake `ServiceCode`;
- do not infer `SupplierPartyRef` or `DeliveryProviderRef` from marketing/operating carrier identity;
- AirOffer package/coupon allocations do not prove independently sold service pricing, so the service is **not** labelled `SeparatelyPriced`;
- use `ServicePriceTreatment.SupplierOpaque` as the AirOffer S1 price treatment unless better Pack/owner evidence is found during planning.
No new behavior follows from these fields.
Disposition: `RESTORE`.

### OD-P-07 — the fulfillment profile is invented by the adapter
**Recommendation:** keep the constants but bind them to an options-bound profile definition with a version.
**Answer: RECOMMENDATION REJECTED. REDESIGN REQUIRED.** Configuration does not turn an unsupported assumption into owner truth. Current AirOffer certifies none of `ReservationRequirement = FlightCapacity`, `DocumentKind = Etkt`, `RequiresFunding = true`, `CapacityUnits = 1`.
Required direction:
- distinguish a fully authoritative **reference/simulator fulfillment profile** from the **live AirOffer candidate sandbox** profile;
- the reference simulator may state exact target semantics because it owns its test contract;
- the live AirOffer profile must explicitly represent unresolved / not-certified requirement semantics and must not silently claim FlightFlow, ETKT or funding authority;
- store the profile version and the assurance/source needed to know which fulfillment definition was accepted;
- no live AirOffer Order becomes Reserve/Issue eligible from these assumptions;
- BD-002 / BD-005 / BD-006 remain the production authority gates;
- do not solve this by building a generic profile engine.
Disposition: `REDESIGN`; the live profile's requirement semantics remain owner-blocked while the reference profile is implementable.

### OD-P-08 — baggage allowance is discarded
**Answer: APPROVED.** Preserve the checked and cabin baggage allowance supplied per coupon on the accepted air-transport service/detail, as typed immutable sold-allowance snapshots containing only supplied facts (pieces, weight, weight unit).
- This is included/sold air-product context.
- Do not create a separately sold `Baggage` OrderService from an allowance.
- Do not treat an allowance as consumption state.
- Do not infer missing pieces or weight.
- Preserve checked and cabin allowance separately.
Disposition: `RESTORE`.

---

## Group 5 — journeys, segments, legs

### OD-P-09 — there is no Journey
**Answer: APPROVED.** Restore explicit Journey grouping, preserving source `BoundId`, source sequence, the source direction fact, origin, destination, the Order/Journey relationship and the root AirOffer `JourneyType` source fact. Do not infer outbound/inbound or trip type from segment order. Where the exact source direction / journey-type vocabulary is not contractually known: preserve the raw source value losslessly, map to a canonical enum only where known, otherwise retain an explicit unknown/unmapped state without guessing.
Disposition: `RESTORE`.

### OD-P-10 — the sold segment lost its operational identity
**Recommendation:** restore terminals, duration, aircraft, capacity id, flight version, carriers, cabin and RBD on the segment.
**Answer: APPROVED WITH CORRECTION.** Restore the accepted sold-segment / source-flight facts AirOffer supplies: origin/destination, origin/destination terminal refs, sold departure/arrival, duration, aircraft ref, external `FlightId`, `FlightVersion`, marketing carrier ref, operating carrier ref, and the source `FlightCapacityId` as a retained source capacity/resource reference.
- Preserving `FlightCapacityId` does **not** certify it as a valid FlightFlow mutation handle; BD-002 / BD-003 still govern that. Model and name it so it stays a source reference, not locally owned capacity truth.
- **Do not duplicate cabin, RBD or booking class onto the segment** just because the legacy model did. Those are passenger/service-specific accepted air-transport facts and stay on the typed air-transport service detail unless the owner explicitly defines a segment-level value.
Disposition: `RESTORE`.

### OD-P-11 — legs keep nothing but a reference
**Answer: APPROVED.** Restore typed leg source facts: `SourceLegRef`, sequence, origin/destination airport refs, origin/destination terminal refs, departure/arrival times. Do not infer connection, protection or stop semantics.
Disposition: `RESTORE`.

### OD-P-12 — the `Stop` vocabulary is undocumented
**Answer: REMAINS BLOCKED.** Do not guess the `Stop` JSON vocabulary and do not create canonical Connection/Stopover/Protected semantics from an unread `JsonElement`. Until owner evidence exists: preserve the full raw AirOffer source evidence S1 already retains, never silently drop the raw source payload, and **do not create a fake canonical `Unknown` connection row** merely because a `Stop` node existed.
Prepare a precise AirOffer-owner handoff asking for the exact `Stop` JSON shape, the enum/value vocabulary, the semantics of each field, whether it describes a technical stop, a passenger connection, protection or another concept, and the versioning/compatibility expectations.
(The Pack's own AirOffer contract agrees: "Exact optional stop shape requires a captured wire fixture before it is consumed.")
Disposition: `BLOCKED_DECISION`. No code is written for it.

---

## Group 6 — pricing line identity

### OD-P-15 — pricing lines lose Code, Name and Reference
**Answer: APPROVED.** Restore typed canonical pricing-line identity/provenance for the source `Code`, source `Name`/description, source `Reference` and the exact `SourceLineRef` / occurrence path, preserved from AirOffer without reinterpretation. Do not assume `Code` is universally a tax code; its meaning follows `ComponentType` and the source contract. The canonical persistence and read model must retain these values.
Changing the public HTTP response is a separate API-contract choice: the plan must state whether public exposure is needed and must not silently change the ratified API contract.
Disposition: `RESTORE`.

### OD-P-17 — quantity / unit of measure / unit price on `PricingLine`
**Recommendation:** restore all three as nullable source facts.
**Answer: NOT APPROVED FOR S1.** Do not restore these fields merely because the legacy model had them. Pack 3.8's canonical line requirements and the current AirOffer S1 contract supply no meaningful pricing-line quantity, unit of measure or unit price. Preserve them when a real accepted pricing source supplies them, or when a later approved scenario requires them; do not add empty speculative columns now.
Disposition corrected from `RESTORE` to `DEFER-BEHAVIOR / NOT REQUIRED BY CURRENT S1 SOURCE`.

### OD-P-14 — `PriceChangeSet` source split
**Recommendation:** split `SourceDecisionRef` back into source, offer id and pricing ref.
**Answer: NOT APPROVED. KEEP CURRENT PACK 3.8 SEMANTICS.** DOMAIN/03 §9 defines `PriceChangeSet(SetId, OrderId, ChangeId, FinancialSequence, Reason, SourceDecisionRef, BaseCommercialVersion, CommittedAt)`. Legacy `Source`, `SourceOfferId` and `SourcePricingRef` are not restored merely because they existed historically: the Order already retains accepted source/offer identity, and `SourceDecisionRef` identifies the accepted monetary decision/context. Keep `SourceDecisionRef` opaque and stable, and never require callers to parse it for business meaning. Later AirPrice servicing decisions carry their own exact owner decision reference.
Disposition corrected to `KEEP`.

### OD-P-20 — the numeric pricing-category map is inferred
**Recommendation:** handoff to the AirOffer owner and record the raw category.
**Answer: CLOSED — NOT BLOCKED BY THE CURRENT PACK.** `docs/ORDERING-DESIGN-PACK-v3.8/CONTRACTS/02-AIROFFER.md` records the observed AirOffer CLR category values `Fare = 0, Tax = 1, Fee = 2, Surcharge = 3`, so S1 keeps the existing mapping under the current AirOffer contract/profile. Safeguards: bind and test the mapping as part of the current AirOffer adapter contract/profile; unknown numeric or string values stay `ContractMismatch`; never silently default; a new AirOffer version with different values requires an explicit profile update. No extra raw-category column is required merely out of fear of future renumbering — the raw source evidence is already retained.
Disposition corrected to `KEEP`.

---

## Group 7 — smaller items

### OD-P-02 — the ticketing deadline survives only as prose
**Answer: APPROVED WITH AUTHORITY SEPARATION.** Persist AirOffer `LastTicketingDate` as a typed observed source time fact with its actual source label. Do **not** turn it into an authoritative `TicketingDeadline` while BD-004 is unresolved: the authoritative/effective ticketing deadline stays unresolved/not supplied until its owner and composition are approved. Do not hide the supplied instant inside a prose `reason`.
Disposition: `REDESIGN`.

### OD-P-05 — `OrderItemServiceLink` does not record when the link was made
**Recommendation:** add `LinkedAt`.
**Answer: `LinkedAt` REJECTED AS THE PRIMARY FIX. REDESIGN FOR `ScopeAtAssociation`.** DOMAIN/02 §13 defines `OrderItemServiceLink(LinkId, OrderIdAtAssociation, OrderItemId, OrderServiceId, ScopeAtAssociation, LinkedByChangeId)`; the implementation is missing `ScopeAtAssociation`, and that is the actual semantic gap. Capture the immutable association scope at acceptance so later reparenting or split does not force historical reconstruction from current service state; include the beneficiary/coverage scope needed to reconstruct the original association; use typed structure and do not hide known scope in arbitrary JSON. `LinkedByChangeId` already gives the business change and its occurrence time, so `LinkedAt` is not added unless a concrete independent requirement is demonstrated.
Disposition: `REDESIGN`.

### OD-P-13 — contacts are flatter than the historical model
**Answer: CLOSED FOR S1 — KEEP THE CURRENT MODEL.** Legacy `ContactPoint` cardinality is not restored in S1; the current role + email/phone representation satisfies the S1 contract and the Pack minimum. No placeholder values; keep the PII protections and redaction; revisit multiple contact points and country-code structure only when an approved product or document scenario requires it.
Disposition corrected from `BLOCKED_DECISION` to `KEEP`.

---

## New, raised while planning

### OD-P-21 — where flight-level facts live once the segment is restored — **ANSWERED**
`OD-P-10` puts marketing carrier ref, operating carrier ref and `FlightVersion` on the sold segment. All three are **already** carried per passenger on the typed air-transport service detail (`AirTransportDetail.MarketingCarrierRef`, `.OperatingCarrierRef`, `.FlightVersion`, registered in `ServiceDetailSchemaRegistry`) and two of them are published in the ratified public projection as `airTransport.marketingAirlineRef` and `.operatingAirlineRef`.
Applying OD-P-10 as written therefore creates two stores of the same source fact for the same flight. Removing them from the service detail removes fields from a ratified public response, which I will not do without an answer; keeping both leaves duplicated truth that a later servicing slice can desynchronise.
`CabinRef`, `RbdRef` and `BookingClass` are not affected — OD-P-10 explicitly leaves them on the service detail.
**Recommendation:** put the flight-level facts on the **segment** as the single accepted store, keep the service detail to passenger-specific facts, and keep the public fields by reading them from the segment.
**Answer: APPROVED WITH EXPANSION.** `OrderSegment` is the single canonical accepted store for **flight-level** sold facts: external/source `FlightId`, `FlightNumber`, `FlightVersion`, `MarketingCarrierRef`, `OperatingCarrierRef`, origin/destination airport refs, origin/destination terminal refs, sold departure/arrival, duration, aircraft ref, and the source `FlightCapacityId` / capacity reference.
`AirTransportDetail` is the traveler/service-level sold detail and retains cabin ref, RBD ref, booking class, checked baggage allowance, cabin baggage allowance, and the exact source refundable/changeable/upgradable display flags; other future facts only when they are truly traveler/service-level and approved.
- **`FlightNumber` is part of this decision**: it was duplicated at the service-detail level and is a flight-level fact, so its canonical storage moves to `OrderSegment` too.
- **Current-model correction:** `OrderSegment.FlightRef` already holds the AirOffer `FlightId`. Do **not** add a second `SourceFlightRef` column for the same value — keep the existing property/column and document that it is the external/source FlightId.
- **Public API compatibility:** `OrderAirTransportDto.FlightNumber`, `.MarketingAirlineRef` and `.OperatingAirlineRef` stay in the ratified response, populated in the read mapping from the **single covered OrderSegment**. That is denormalized presentation, not duplicate domain persistence. `FlightVersion` is not public and is not added.
- **Required invariant:** an accepted `AirTransportation` service has exactly one beneficiary traveler and exactly one covered passenger segment, enforced in candidate/domain validation. A projection that meets an air service without exactly one covered segment fails deterministically instead of choosing one.
Disposition: `RESTORE/REDESIGN`, fully decided. OD-P-21 is **not** blocked.

---

## Correction: OD-C-02 was never open

The revision-2 matrix counted `OD-C-02` as a blocker. That was wrong. `reports/00-decisions/S1-CLEANUP-OPEN-DECISIONS.md` already carries the owner's answer:

> `Messages.Shared.Enums.SalesChannel` is allowed

So `SalesChannel` stays in the Domain under an explicit owner exception to the GOVERNANCE/05 C1 allowlist, the allowlist test keeps asserting exactly that exception, and no alternative channel type is invented. Matrix row A3 is `KEEP`, not `BLOCKED_DECISION`.

## Still open from stage 04 (unchanged, repeated so nothing is lost)

- **OD-C-01b** — schema for `CommandReceipts` (order tables are `Order`, answered).
- **OD-C-03 … OD-C-10** — as recorded in `S1-CLEANUP-OPEN-DECISIONS.md`.
