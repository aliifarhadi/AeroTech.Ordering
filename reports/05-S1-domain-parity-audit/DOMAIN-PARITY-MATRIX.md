# S1 Domain Parity Matrix

Stage: 05-S1-domain-parity-audit
Repository HEAD audited: `43df88a` (branch `k8s-stg`, clean tree)
Date: 2026-09-19
Status: `S1_DOMAIN_REPAIR_INCOMPLETE` — matrix only, **no material domain change has been made**.

## Sources of truth used

| Source | What it is authority for | How it was read |
|---|---|---|
| `docs/ORDERING-DESIGN-PACK-v3.8/` (DOMAIN 01–15, CONTRACTS/02-AIROFFER, SLICES/S1, GOVERNANCE/05) | Ordering domain semantics, invariants, persistence dictionary | read in this repository |
| `E:\Projects\DotAir\Ordering`, branch **`k8s-stg`** | historical richer Order domain + `docs/order-domain-design-v1/` | `git show k8s-stg:<path>` — the `main` branch carries neither the design docs nor the rich entities |
| `E:\Projects\DotAir\AeroTech.Ordering` @ `43df88a` | the model under audit | working tree |
| AirOffer Details wire (`src/AeroTech.Ordering.Providers/AirOffer/Wire/*.cs`) + `AirOfferCandidateMapper` | what the only S1 source actually supplies today | working tree |

## Disposition vocabulary

- **KEEP** — present, Pack-conformant, no change.
- **RESTORE** — existed in the historical `k8s-stg` domain, is required by Pack 3.8 for S1 accepted-sale information, and is missing or degraded here.
- **REDESIGN** — the concept exists in both, but the current shape breaks a Pack rule (typed vs JSON, wrong owner, wrong cardinality).
- **REMOVE** — present here, not justified by Pack or history.
- **DEFER-BEHAVIOR** — the structure may be needed later, but no S1 behavior may be written now (S2+ servicing, fulfillment, documents, funding application).
- **BLOCKED_DECISION** — cannot be settled without the owner; carries an `OD-P-nn` id recorded in `reports/00-decisions/S1-DOMAIN-PARITY-OPEN-DECISIONS.md`.

**Nothing in the RESTORE/REDESIGN columns is implemented until the owner ratifies it.** Every RESTORE/REDESIGN row changes persistence identity or cardinality, which CLAUDE.md reserves to the owner.

---

## A. Order root and sale context

| # | Concept | Pack 3.8 requirement | Historical `k8s-stg` | Current @ `43df88a` | Gap / information loss | S1 scope | Disposition |
|---|---|---|---|---|---|---|---|
| A1 | Order identity + reference | DOMAIN/13: unique owner+reference | `Order.RecordLocator` VO, `Id` | `Order.OrderReference` (string), snowflake `Id` | none; naming differs only | yes | **KEEP** |
| A2 | Owner / financial customer / sales context | DOMAIN/01 | owner airline, customer, channel, office | `OwnerAirlineId`, `FinancialCustomerId`, `Channel`, `SellingOfficeId`, `BuyerActorContextType`, `BuyerActorId` | none | yes | **KEEP** |
| A3 | `SalesChannel` used inside Domain | GOVERNANCE/05 C1 Messages allowlist | platform enum | `SalesChannel` referenced in Domain, outside the C1 allowlist | allowlist conflict, open since stage 04 | yes | **BLOCKED_DECISION OD-C-02** (already open) |
| A4 | Root / parent order | DOMAIN/12, DOMAIN/13 (parent/root consistency) | `OrderLineage` VO (root, parent, split reason) | `RootOrderId` only | split / related-order lineage not representable | no (S2 split) | **DEFER-BEHAVIOR** |
| A5 | Sale currency | DOMAIN/03 §31: source **CurrencyId and source CurrencyCode** snapshot | amount + `CurrencyId` int | `SaleCurrencyRef` = stringified CurrencyId only | **CurrencyCode discarded** although AirOffer supplies `details.CurrencyCode` | yes | **REDESIGN OD-P-01** |
| A6 | Accepted source evidence | DOMAIN/15, SLICES/S1 | `AcceptedOrderSource` VO | `AcceptedSource` VO (owner, offer id, profile, digests, priced/captured/accepted instants) | none; current is richer | yes | **KEEP** |
| A7 | Validity facts (offer / price / ticketing) | DOMAIN/10, DOMAIN/15 | `OrderTimeLimit` entity (type, dueAt, status, settledAt) | three `ValidityFact` VOs; AirOffer `LastTicketingDate` folded into a **free-text reason string** | a supplied ticketing deadline survives only as prose, not as a typed instant | yes (information preservation) | **REDESIGN OD-P-02** |
| A8 | Commercial summary and totals | DOMAIN/01, 02 | `OrderAmount`, `OrderPaymentSummary` | `CommercialSummary`, `CustomerTotal` (`Money`) | payment summary absent — correct, it is S2 | yes | **KEEP** |
| A9 | Versioning (CV / FinancialSequence / OrderRevision / rowversion) | DOMAIN/13 | present | present, plus `LastEventOrdinal` | none | yes | **KEEP** |
| A10 | `ClientReference` | API-CONTRACTS | client reference on create | `ClientReference` | none | yes | **KEEP** |
| A11 | `OrderRemark` | not required for S1 | `OrderRemark` entity + rules | absent | none for S1 | no | **DEFER-BEHAVIOR** |
| A12 | `OrderExternalReference` | DOMAIN/13 (source references) | entity (type, system, reference, recordedAt) | absent; source refs live inside `AcceptedSource` | no loss at S1 (single source) | no | **DEFER-BEHAVIOR** |

## B. Commercial composition — items

| # | Concept | Pack 3.8 requirement | Historical `k8s-stg` | Current @ `43df88a` | Gap / information loss | S1 scope | Disposition |
|---|---|---|---|---|---|---|---|
| B1 | `OrderItem` core | DOMAIN/02 §7 | ProductType/Code/Name, Quantity, UoM, four statuses | `SourceItemRef`, `Kind`, `SourceOfferItemRef`, `AcceptedTotal`, `CommercialStatus` | product code/name and quantity/UoM absent at item level | yes | **REDESIGN OD-P-03** |
| B2 | `ProductSnapshot` | DOMAIN/02 §9 — a **required field of OrderItem** | `OrderItemProductSnapshot` (source product ref/system/offer, code, name, brand code/name, marketing + operating airline, supplier, pricing ref, acceptedAt) | **absent** | brand, marketing/operating carrier, supplier and source product identity are not snapshotted per item | yes | **RESTORE OD-P-03** |
| B3 | `CommercialTermsSnapshot` | DOMAIN/02 §9 — display refund/change/upgrade summaries, policy ref/version, captured-at | `OrderItemCommercialTermsSnapshot` (`CommercialTermState` × 3, source system, policy ref/version, capturedAt) | **absent** | AirOffer `coupon.IsRefundable / IsChangeable / IsUpgradable` are **read off the wire and thrown away** | yes | **RESTORE OD-P-04** |
| B4 | `AcceptedPriceSnapshot` (original item version) | DOMAIN/02 §7 | per-item accepted amounts | `AcceptedTotal` (`Money`) | none | yes | **KEEP** |
| B5 | `OrderItemPolicySnapshot` | not a Pack 3.8 required S1 field; overlaps `FulfillmentProfileSnapshot` | rich policy snapshot (delivery model, granularity, assignment mode, eight booleans, rule refs) | absent; `FulfillmentProfileSnapshot` on the service covers the S1 subset | none for S1 | no | **DEFER-BEHAVIOR** |
| B6 | Single `OFFER-PACKAGE` item | DOMAIN/02 + CONTRACTS/02-AIROFFER: AirOffer publishes no item decomposition | n/a (legacy had one item per product) | one item per offer, ref `OFFER-PACKAGE` | Pack-approved normalization, recorded in stage 03 | yes | **KEEP** |
| B7 | `OrderItemServiceLink` | DOMAIN/13 (append-only, occurrence OrderId) | entity carrying `LinkedAt` | entity without `LinkedAt` | link time not recorded | yes | **REDESIGN OD-P-05** |
| B8 | Item / service lineage tables | DOMAIN/13 | absent in history too | absent | none for S1 | no | **DEFER-BEHAVIOR** |

## C. Commercial composition — services

| # | Concept | Pack 3.8 requirement | Historical `k8s-stg` | Current @ `43df88a` | Gap / information loss | S1 scope | Disposition |
|---|---|---|---|---|---|---|---|
| C1 | `OrderService` core fields | DOMAIN/02 §19: ServiceId, current Order/Item, ServiceType, **ServiceCode/Name**, CommercialStatus, ServiceVersion, beneficiaries, **SupplierPartyRef**, **DeliveryProviderRef**, FulfillmentProfileSnapshot, **PriceTreatment**, coverage/detail link, CreatedByChangeId | all present | `SourceServiceRef`, `Type`, `CommercialStatus`, `ServiceVersion`, `Quantity`, `QuantityUnit`, `DetailSchema(+Version)`, `FulfillmentProfile`, `CreatedByChangeId` | **ServiceCode, Name, PriceTreatment, SupplierPartyRef, DeliveryProviderRef missing** | yes | **RESTORE OD-P-06** |
| C2 | `FulfillmentProfileSnapshot` | DOMAIN/02 §23: profile **ID + version**, reservation requirement, quantity/unit policy, document requirement/type/authority, funding requirement, delivery control, dependency treatment, partial fulfillment | equivalent booleans on the service | VO with `ProfileRef`, `ReservationRequirement`, `DocumentKind`, `RequiresFunding`, `CapacityUnits` | no profile **version**; the adapter hard-codes the profile ref and `CapacityUnits = 1` | yes | **REDESIGN OD-P-07** |
| C3 | Typed service detail — air transport | DOMAIN/02: exactly one typed detail per registered core type | `OrderAirTransportServiceDetail` (segment, fare basis, requested seat, **checked and cabin baggage**) | `AirTransportDetail` VO (cabin, RBD, booking class, flight number/version, marketing/operating carrier) | **baggage allowance (pieces/weight/unit) and cabin baggage supplied per AirOffer coupon are discarded**; no transitional fare basis | yes | **RESTORE OD-P-08** |
| C4 | Typed details — seat / baggage / meal / lounge / hotel / ground / generic | DOMAIN/02 registered types | seven further detail entities | absent | none for S1 (AirOffer sells air transport only) | no | **DEFER-BEHAVIOR** |
| C5 | Detail carried as `DetailSchema` + string dictionary | CLAUDE.md "no dynamic JSON for known concepts"; DOMAIN/02 typed details | typed entity | string dictionary in the **candidate**, materialised to a typed VO on accept | the candidate is an acceptance envelope, not the accepted record | yes | **KEEP** (candidate) / see C3 for the accepted record |
| C6 | Beneficiaries | DOMAIN/13 unique relation | `OrderServiceBeneficiary` | `OrderServiceBeneficiary` | none | yes | **KEEP** |
| C7 | Coverage (service → segment) | DOMAIN/13 covered scope | `OrderServiceCoveredSegment` | `OrderServiceCoverage` | none | yes | **KEEP** |
| C8 | Service → service dependencies | DOMAIN/13 dependencies, no `RequiresAirService` cycle | `OrderServiceCoveredService` | absent | not needed while only air transport is sold | no | **DEFER-BEHAVIOR** |
| C9 | Fulfillment / document / delivery / financial statuses | DOMAIN/02, 07, 11 | six status enums and transitions | `CommercialStatus` only | deliberate: S1 may carry no fulfillment behavior | yes | **KEEP** |
| C10 | EMD issuance snapshot, ticket/coupon links | DOMAIN/07 | present | absent | S2+ | no | **DEFER-BEHAVIOR** |

## D. Travelers, journeys, segments, contacts

| # | Concept | Pack 3.8 requirement | Historical `k8s-stg` | Current @ `43df88a` | Gap / information loss | S1 scope | Disposition |
|---|---|---|---|---|---|---|---|
| D1 | **Journey** | DOMAIN/04 §15 "Journey groups customer travel"; DOMAIN/13 table group `Travelers/Contacts/`**`Journeys`**`/Segments` | `OrderItinerary` (order, origin, destination, **BoundId**, sequence, **BoundDirection**) | **absent** | AirOffer supplies `airTransports[].BoundId / Direction / Sequence / OriginAirportId / DestinationAirportId` and `details.JourneyType` — **all discarded**; segments hang directly off the order | yes | **RESTORE OD-P-09** |
| D2 | Sold `JourneySegment` facts | DOMAIN/04 §15: sold marketing/operating identity, sold schedule snapshot, external flight ID/version, segment kind | `OrderSegment`: cabin, RBD, booking class, **FlightCapacityId**, AirFareId, FlightId + Version, number, origin/destination **and terminals**, operating/marketing airline, departure/arrival, **duration**, **aircraft** | `OrderSegment`: sequence, `SourceSegmentRef`, `Kind`, `OriginRef`, `DestinationRef`, `SoldDeparture`, `SoldArrival`, `FlightRef` | **terminals, duration, aircraft, FlightCapacityId, flight version, marketing/operating carrier, cabin and RBD are not on the sold segment**; carrier/cabin survive only inside the per-traveler service detail | yes | **RESTORE OD-P-10** |
| D3 | Segment legs | DOMAIN/04 §19 physical legs | `OrderSegmentLeg`: sequence, LegId, origin/destination **and terminals**, departure/arrival, **StopType**, **stop duration** | `OrderSegmentLeg`: sequence + `SourceLegRef` **only** | AirOffer leg airports, terminals, times and `Stop` are parsed into the wire type and **dropped entirely** | yes | **RESTORE OD-P-11** |
| D4 | Connection / protection facts | DOMAIN/04 §19: Connection / Stopover / SurfaceBreak / Unknown × Protected / Unprotected / Unknown | `FlightStopType` on the leg | absent; AirOffer `Stop` is an unparsed `JsonElement` | the stop vocabulary is never read | yes, if the source supplies it | **BLOCKED_DECISION OD-P-12** — observed `Stop` vocabulary is undocumented |
| D5 | Traveler | DOMAIN/04 | `OrderTraveller`: index, `Name` VO, PTC, age range, DOB, gender, nationality, residence, documents, parent | `OrderTraveler`: `SourceTravellerRef`, `ClientTravelerRef`, `PassengerTypeCode`, `InfantParentTravelerId`, `TravelerIdentity` | gender, nationality, residence, age range and index absent — **not supplied by AirOffer at S1**; they belong to a later slice | yes | **KEEP** (no S1 loss) |
| D6 | Traveler documents | DOMAIN/04 protected data | `OrderTravellerDocument` | absent | not supplied at S1 | no | **DEFER-BEHAVIOR** |
| D7 | PII handling | DOMAIN/04 protected refs; OD-S1-04 | names stored in the domain | names stored in the domain, **null in the projection** | none | yes | **KEEP** |
| D8 | Contacts | DOMAIN/13 | `OrderContact` + `OrderContactPoint` (type, value, country code, primary) | `OrderContact` with `Role`, `Email`, `Phone` scalars | one email/phone pair per contact; no country code, no multiple points | yes | **BLOCKED_DECISION OD-P-13** — the historical shape is richer, but the S1 request bodies were fixed by the ratified stage-04 cleanup decision |

## E. Pricing

| # | Concept | Pack 3.8 requirement | Historical `k8s-stg` | Current @ `43df88a` | Gap / information loss | S1 scope | Disposition |
|---|---|---|---|---|---|---|---|
| E1 | `PriceChangeSet` | DOMAIN/13 unique Order+FinancialSequence | reason, **source, SourceOfferId, SourcePricingRef**, committedAt | reason, `SourceDecisionRef`, `BaseCommercialVersion`, `CommittedAt` | pricing source, offer id and pricing reference collapsed into one opaque string | yes | **REDESIGN OD-P-14** |
| E2 | Magnitudes, direction, effect, role | DOMAIN/03 sign contract | non-negative magnitudes + direction | non-negative `Money` original and sale, direction/effect/role, matrix policy | none | yes | **KEEP** |
| E3 | Line identity: **Code / Name / Reference** | DOMAIN/03; AirOffer supplies `Name`, `Code`, `Reference` on every row | `Code`, `Description` | **absent** — only `SourceLineRef`, a JSON path | an observed tax code (`YQ`) or fee name is **preserved nowhere** | yes | **RESTORE OD-P-15** |
| E4 | Source-currency amount and code | DOMAIN/03 §31 | `OriginalAmount` + `OriginalCurrencyId`, `SaleAmount` + `SaleCurrencyId` | `Money` original and sale, currency as a stringified id | currency **code** never stored (see A5) | yes | **REDESIGN OD-P-01** |
| E5 | FX evidence | DOMAIN/03 | `ExchangeRate` VO on line and allocation | `SourceConversionRef` (period id string) only | AirOffer `ratesOfExchange[]` (from/to currency, rate, decimal places, rounding factor) is parsed and **discarded** | yes | **RESTORE OD-P-16** |
| E6 | Quantity / unit of measure / unit price | DOMAIN/03 (extended once, not multiplied again) | `Quantity`, `UnitOfMeasure`, `UnitPrice` | absent on the line (present on the service) | a per-unit charge cannot be shown as "2 × 15.00" | yes | **RESTORE OD-P-17** |
| E7 | Percentage rows | AirOffer `IsPercentage`; OD-S1-08 | n/a | mapper sets sale = original = equivalent amount and drops the flag | the fact that the row was a percentage is lost | yes | **BLOCKED_DECISION OD-P-18** |
| E8 | `Refundability`, `ApplicationLevel`, `TaxDetails`, `SettlementPartyRef/Category`, `OccurrenceKey`, `TransferGroupId`, `RelatedOperationId`, `CalculationSnapshot` | DOMAIN/03 + DOMAIN/09 | all present on `OrderPricingLine` | absent | not supplied by AirOffer at S1; these are servicing and settlement facts | no | **DEFER-BEHAVIOR** |
| E9 | Allocation sets and allocations | DOMAIN/03, DOMAIN/13 (unique line+purpose+version, reconciliation) | `OrderPricingAllocationSet` + `OrderPricingAllocation` with full reconciliation | **removed in the stage-04 cleanup** | S1 has no allocation source; the removal was ratified | no | **DEFER-BEHAVIOR** — removal stands; re-add with the slice that needs it |
| E10 | Per-traveler / per-coupon attribution | DOMAIN/03 | line → allocation per traveler/segment | lines carry `PricingBasisType.OrderService` with basis ref `traveller\|bound\|flight` | attribution is preserved through the basis ref instead of an allocation | yes | **KEEP** |
| E11 | Ticket-level base/charge/total | source facts | per-ticket amounts | used to reconcile, then dropped | traveler subtotal is derivable by summing that traveler's lines | yes | **KEEP** (no loss) |

## F. Fare construction

| # | Concept | Pack 3.8 requirement | Historical `k8s-stg` | Current @ `43df88a` | Gap / information loss | S1 scope | Disposition |
|---|---|---|---|---|---|---|---|
| F1 | `AirFareConstruction` | DOMAIN/03 §47: immutable Order-owned snapshot with construction id, OrderIdAtCreation, created change, supersession, **item refs**, construction type, source reference, **pricing groups**; DOMAIN/13 table group `FareConstructions/Groups/Units/Components/Bindings` | `OrderAirFareConstruction` + `OrderAirFareConstructionItem` | `FareConstruction` whose whole graph is one column, **`PricingUnitsJson`** | **the Pack-mandated relational graph does not exist**; units and components are an opaque blob assembled inside `Order.AcceptOriginalSale` | yes | **REDESIGN OD-P-19** — the single largest divergence |
| F2 | `PricingGroup` | DOMAIN/03 §49: exact source-grouped travelers, PTC, quantity | `OrderFarePricingGroup` + `OrderFarePricingGroupTraveller` | absent from the blob | traveler ↔ fare grouping is unrepresentable | yes | **RESTORE OD-P-19** |
| F3 | `PricingUnit` | DOMAIN/03 §51: source identity, type (OneWay/RoundTrip/OpenJaw/CircleTrip/Other), combination method, components | `OrderFarePricingUnit` (type, combination method, sequence, source ref) | JSON node with hard-coded `Unspecified` / `ProviderDefined` | unit type and combination method are constants, not source facts | yes | **RESTORE OD-P-19** |
| F4 | `FareComponent` | DOMAIN/03 §51: explicit covered ServiceIds **or traveler+segment pairs**, source fare id, fare basis/family/brand, cabin/RBD, fare owner/rule/routing/tariff references | `OrderFareComponent` + `OrderFareComponentService` + `OrderFareComponentSegment` | JSON node with fare id, basis, family, type, cabin, RBD, booking class and an **empty coverage list** | components cover nothing — the Pack's explicit coupling is absent; tariff, rule, routing and fare-owner references absent | yes | **RESTORE OD-P-19** |
| F5 | Current bindings | DOMAIN/13 "current-binding uniqueness" | `OrderAirFareConstructionItem` | absent | the construction is not bound to items | yes | **RESTORE OD-P-19** |
| F6 | `TicketingRestrictionMinutes` | source fact on the fare component | n/a | read into `AirOfferFareComponentWire`, never mapped | discarded | yes | **RESTORE OD-P-19** |
| F7 | `FareConstructionAssurance.Opaque` | SLICES/S1 | n/a | hard-coded by the adapter | correct while AirOffer publishes no certified construction | yes | **KEEP** |

## G. Funding, receipts, durability

| # | Concept | Pack 3.8 requirement | Historical `k8s-stg` | Current @ `43df88a` | Gap / information loss | S1 scope | Disposition |
|---|---|---|---|---|---|---|---|
| G1 | `FundingObligation` | DOMAIN/06, DOMAIN/13 (unique obligation+version) | payment summary / documents | `FundingObligation` (version, purpose, amount, item, change, decision ref, supersession) | none | yes | **KEEP** |
| G2 | Funding applications and evidence | DOMAIN/06 | n/a | absent | S2+ | no | **DEFER-BEHAVIOR** |
| G3 | `CommandReceipt` idempotency | DOMAIN/08, DOMAIN/13 unique receipt scope | n/a | `(OwnerAirlineId, CallerScope, CommandKind, IdempotencyKey)` plus the stored result | none | yes | **KEEP** |
| G4 | `OrderPreparation` + `PreparationSourceEvidence` | SLICES/S1, DOMAIN/15 | n/a | present; consumed exactly once inside the sale transaction | none — the owner requires it kept | yes | **KEEP** |
| G5 | Outbox / inbox / single projector | ARCHITECTURE/03, DOMAIN/13 | present | present; one projector, one transaction | none | yes | **KEEP** |
| G6 | One effective unit of work for create | ARCHITECTURE/03 | n/a | receipt + preparation consumption + order graph + projection + outbox in one SQL transaction, provider read outside it | none | yes | **KEEP** — must not regress |

## H. Adapter assumptions that are not source facts

Values the AirOffer adapter invents. None may be silently kept as "observed behavior".

| # | Assumption | Where | Justification today | Disposition |
|---|---|---|---|---|
| H1 | `FulfillmentProfileRef = "AIROFFER-OBSERVED-AIR-UNCERTIFIED"`, `CapacityUnits = 1`, `ReservationRequirement.FlightCapacity`, `DocumentKind.Etkt`, `RequiresFunding = true` | `AirOfferCandidateMapper.AirService` | no owner contract publishes a fulfillment profile | **BLOCKED_DECISION OD-P-07** |
| H2 | `Quantity = 1`, `OrderItemUnitOfMeasure.PassengerSegment` | same | one coupon is one passenger segment | **KEEP** (source-implied) |
| H3 | every row is `PricingEffect.CustomerBalance`, `Direction.Debit`, `Role.Original` | `AirOfferCandidateMapper.Line` | S1 sells only; a negative row throws instead of being interpreted | **KEEP**, guarded by the explicit throw |
| H4 | pricing category numeric map `0..3 → Fare / Tax / Fee / CarrierSurcharge` | same | inferred from observed payloads, **not from a published contract** | **BLOCKED_DECISION OD-P-20** |
| H5 | `AcceptanceAssurance.LocalCandidateOnly` for every AirOffer candidate | same | AirOffer publishes no binding acceptance | **KEEP** |
| H6 | `SegmentRef = "{BoundId}\|{FlightId}"`, `ServiceRef = "{TravellerRef}\|{SegmentRef}"` | same | synthetic identity, stable and reproducible | **KEEP** |
| H7 | infant (`INF`) rejected outright | same | BD-002 / OD-S1-07 unresolved | **KEEP** until the owner resolves BD-002 |

---

## Summary counts

| Disposition | Rows |
|---|---|
| KEEP | 24 |
| RESTORE | 10 |
| REDESIGN | 8 |
| REMOVE | 0 |
| DEFER-BEHAVIOR | 13 |
| BLOCKED_DECISION | 6 (`OD-P-07`, `OD-P-12`, `OD-P-13`, `OD-P-18`, `OD-P-20`, plus the already-open `OD-C-02`) |

## The five findings of the audit prompt, verified

| Finding | Verdict | Evidence |
|---|---|---|
| FareConstruction stored as a JSON blob | **Confirmed** | `FareConstruction.PricingUnitsJson`; DOMAIN/13 requires `FareConstructions/Groups/Units/Components/Bindings` |
| `PricingLine` too narrow | **Confirmed** | no Code/Name/Reference, no quantity or unit price, no FX evidence, currency code absent |
| AirOffer information loss | **Confirmed** | legs, terminals, duration, aircraft, flight-capacity id, baggage, refund/change/upgrade flags, rates of exchange, journey/bound structure and currency code are all parsed and dropped |
| `OrderItem` lacks the Pack-required snapshots | **Confirmed** | DOMAIN/02 §9 names `ProductSnapshot` and `CommercialTermsSnapshot` as item fields; neither exists |
| Hard-coded adapter assumptions | **Confirmed** | section H |

## What happens next

1. The owner ratifies or rejects each `OD-P-nn` in `reports/00-decisions/S1-DOMAIN-PARITY-OPEN-DECISIONS.md`.
2. Only then is `IMPLEMENTATION-PLAN.md` written and executed, followed by `PERSISTENCE-AUDIT.md`, `INFORMATION-PRESERVATION.md`, `TEST-RESULTS.md` and `REPORT.md`.
3. The reliability invariants proven in stages 03 and 04 — one transaction, one projector, receipt replay, projection byte-determinism, outbox ordering — must not regress; the information-preservation tests are added on top of them.

S1 status: `S1_DOMAIN_REPAIR_INCOMPLETE`. S2: `S2_NOT_STARTED`.
