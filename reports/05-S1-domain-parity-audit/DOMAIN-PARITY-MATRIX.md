# S1 Domain Parity Matrix

Stage: 05-S1-domain-parity-audit
Repository HEAD audited: `deaf6b0` (branch `k8s-stg`, clean tree) — first issued at `43df88a`, revised after the owner's answers.
Date: 2026-09-19 (revision 3 — owner decisions applied, `OD-P-21` closed, `OD-C-02` corrected, repairs implemented)
Status: `S1_DOMAIN_REPAIR_READY_FOR_OWNER_REVIEW` — the approved repairs are implemented; see `REPORT.md`.

Owner answers: `reports/00-decisions/S1-DOMAIN-PARITY-OPEN-DECISIONS.md`.
Implementation plan: `IMPLEMENTATION-PLAN.md` in this folder.

## Sources of truth used

| Source | What it is authority for | How it was read |
|---|---|---|
| `docs/ORDERING-DESIGN-PACK-v3.8/` (DOMAIN 01–15, CONTRACTS/02-AIROFFER, SLICES/S1, GOVERNANCE/05) | Ordering domain semantics, invariants, persistence dictionary | read in this repository |
| `E:\Projects\DotAir\Ordering`, branch **`k8s-stg`** | historical richer Order domain + `docs/order-domain-design-v1/` | `git show k8s-stg:<path>` — `main` carries neither the design docs nor the rich entities |
| `E:\Projects\DotAir\AeroTech.Ordering` @ `deaf6b0` | the model under audit | working tree |
| AirOffer Details wire (`src/AeroTech.Ordering.Providers/AirOffer/Wire/*.cs`) + `AirOfferCandidateMapper` | what the only S1 source actually supplies | working tree |

## Disposition vocabulary

- **KEEP** — present and Pack-conformant, or explicitly settled by the owner as correct; no change.
- **RESTORE** — required by Pack 3.8 for S1 accepted-sale information and missing or degraded here.
- **REDESIGN** — the concept exists but the current shape breaks a Pack rule (typed vs JSON, wrong owner, wrong cardinality).
- **REMOVE** — present here, not justified by Pack or history.
- **DEFER-BEHAVIOR** — structure may be needed later; no S1 behavior may be written now.
- **BLOCKED_DECISION** — cannot be settled without the owner.

The historical `k8s-stg` model is evidence of what was lost, **not** an authority. Where Pack 3.8 and the legacy implementation disagree, Pack 3.8 wins: that is why `OD-P-14`, `OD-P-17` and part of `OD-P-03` were rejected even though the legacy entity carried those fields.

---

## A. Order root and sale context

| # | Concept | Pack 3.8 requirement | Historical `k8s-stg` | Current @ `deaf6b0` | Gap / information loss | S1 scope | Disposition |
|---|---|---|---|---|---|---|---|
| A1 | Order identity + reference | DOMAIN/13: unique owner+reference | `RecordLocator` VO, `Id` | `OrderReference`, snowflake `Id` | none | yes | **KEEP** |
| A2 | Owner / financial customer / sales context | DOMAIN/01 | owner, customer, channel, office | `OwnerAirlineId`, `FinancialCustomerId`, `Channel`, `SellingOfficeId`, `BuyerActorContextType`, `BuyerActorId` | none | yes | **KEEP** |
| A3 | `SalesChannel` used inside Domain | GOVERNANCE/05 C1 allowlist | platform enum | referenced in Domain under an explicit owner exception | none — `OD-C-02` was answered in `S1-CLEANUP-OPEN-DECISIONS.md`: "`Messages.Shared.Enums.SalesChannel` is allowed" | yes | **KEEP** (corrected in revision 3) |
| A4 | Root / parent order | DOMAIN/12, DOMAIN/13 | `OrderLineage` VO | `RootOrderId` only | split lineage not representable | no (S2) | **DEFER-BEHAVIOR** |
| A5 | Sale currency | DOMAIN/03 §31: source CurrencyId **and** CurrencyCode | amount + `CurrencyId` | `SaleCurrencyRef` = stringified id only | `details.CurrencyCode` discarded | yes | **REDESIGN OD-P-01** |
| A6 | Accepted source evidence | DOMAIN/15, SLICES/S1 | `AcceptedOrderSource` VO | `AcceptedSource` VO | none; current is richer | yes | **KEEP** |
| A7 | Validity facts (offer / price / ticketing) | DOMAIN/10, DOMAIN/15 | `OrderTimeLimit` entity | three `ValidityFact` VOs; `LastTicketingDate` folded into a free-text reason | the observed instant survives only as prose | yes | **REDESIGN OD-P-02** — observed source fact typed; the authoritative deadline stays unresolved under BD-004 |
| A8 | Commercial summary and totals | DOMAIN/01, 02 | `OrderAmount`, `OrderPaymentSummary` | `CommercialSummary`, `CustomerTotal` | payment summary absent — correct, it is S2 | yes | **KEEP** |
| A9 | Versioning (CV / FinancialSequence / OrderRevision / rowversion) | DOMAIN/13 | present | present, plus `LastEventOrdinal` | none | yes | **KEEP** |
| A10 | `ClientReference` | API-CONTRACTS | present | present | none | yes | **KEEP** |
| A11 | `OrderRemark` | not required for S1 | entity + rules | absent | none for S1 | no | **DEFER-BEHAVIOR** |
| A12 | `OrderExternalReference` | DOMAIN/13 | entity | absent; source refs live in `AcceptedSource` | no loss at S1 | no | **DEFER-BEHAVIOR** |

## B. Commercial composition — items

| # | Concept | Pack 3.8 requirement | Historical `k8s-stg` | Current @ `deaf6b0` | Gap / information loss | S1 scope | Disposition |
|---|---|---|---|---|---|---|---|
| B1 | `OrderItem` core | DOMAIN/02 §7 | ProductType/Code/Name, Quantity, UoM, four statuses | `SourceItemRef`, `Kind`, `SourceOfferItemRef`, `AcceptedTotal`, `CommercialStatus` | none that Pack 3.8 requires — **item quantity/UOM rejected by OD-P-03**; product identity belongs to B2 | yes | **KEEP** (corrected) |
| B2 | `ProductSnapshot` | DOMAIN/02 §9 — a required field of `OrderItem` | `OrderItemProductSnapshot` | **absent** | source system/owner, offer id, offer item ref and any supplied product code/name/brand are not snapshotted per item | yes | **RESTORE OD-P-03** — supplied facts only; no invented product id/code/name, no package-level carrier collapse |
| B3 | `CommercialTermsSnapshot` | DOMAIN/02 §9 | `OrderItemCommercialTermsSnapshot` | **absent** | `coupon.IsRefundable / IsChangeable / IsUpgradable` read off the wire and thrown away | yes | **RESTORE OD-P-04** — exact per-service flags preserved **and** an item-level summary `Permitted / Prohibited / Conditional / Unknown` |
| B4 | `AcceptedPriceSnapshot` | DOMAIN/02 §7 | per-item accepted amounts | `AcceptedTotal` (`Money`) | none | yes | **KEEP** |
| B5 | `OrderItemPolicySnapshot` | not a Pack 3.8 S1 requirement | rich policy snapshot | absent | none for S1 | no | **DEFER-BEHAVIOR** |
| B6 | Single `OFFER-PACKAGE` item | CONTRACTS/02-AIROFFER: "use one local `OfferPackage` item" | n/a | one item per offer | Pack-approved normalization | yes | **KEEP** |
| B7 | `OrderItemServiceLink` | DOMAIN/02 §13: `(LinkId, OrderIdAtAssociation, OrderItemId, OrderServiceId, ScopeAtAssociation, LinkedByChangeId)` | link + `LinkedAt` | link without `ScopeAtAssociation` | **`ScopeAtAssociation` missing** — the original beneficiary/coverage scope cannot be reconstructed after reparenting | yes | **REDESIGN OD-P-05** — `ScopeAtAssociation`, not `LinkedAt` |
| B8 | Item / service lineage tables | DOMAIN/13 | absent in history too | absent | none for S1 | no | **DEFER-BEHAVIOR** |

## C. Commercial composition — services

| # | Concept | Pack 3.8 requirement | Historical `k8s-stg` | Current @ `deaf6b0` | Gap / information loss | S1 scope | Disposition |
|---|---|---|---|---|---|---|---|
| C1 | `OrderService` core fields | DOMAIN/02 §19 | all present | ServiceCode, Name, PriceTreatment, SupplierPartyRef, DeliveryProviderRef **missing** | Pack-required fields absent | yes | **RESTORE OD-P-06** — structure only; `ServiceCode`/`Name` stay null for AirOffer, `PriceTreatment = SupplierOpaque`, no inferred supplier/delivery refs |
| C2 | `FulfillmentProfileSnapshot` | DOMAIN/02 §23 (profile ID **and version**, requirement semantics, authority) | booleans on the service | VO without a version; the adapter hard-codes `FlightCapacity` / `Etkt` / `RequiresFunding` / `CapacityUnits = 1` for a source that certifies none of them | uncertified requirement semantics presented as accepted truth | yes | **REDESIGN OD-P-07** — reference profile may state exact semantics; the live AirOffer profile must represent them as not certified; live requirement semantics stay owner-blocked (BD-002/005/006) |
| C3 | Typed air-transport detail | DOMAIN/02 | detail with checked and cabin baggage | **restored**: cabin ref, RBD ref, booking class, checked and cabin baggage allowance; the three sold term flags sit on the service itself | none | yes | **RESTORE OD-P-08** — flight-level facts moved out to the segment under `OD-P-21` |
| C4 | Seat / baggage / meal / lounge / hotel / ground / generic details | DOMAIN/02 | seven entities | absent | none for S1 | no | **DEFER-BEHAVIOR** |
| C5 | Candidate detail as schema + string map | CLAUDE.md, DOMAIN/02 | typed entity | string map in the candidate, typed VO on accept | the candidate is an acceptance envelope, not the accepted record | yes | **KEEP** |
| C6 | Beneficiaries | DOMAIN/13 | present | present | none | yes | **KEEP** |
| C7 | Coverage (service → segment) | DOMAIN/13 | present | present | none | yes | **KEEP** |
| C8 | Service → service dependencies | DOMAIN/13 | present | absent | not needed while only air transport is sold | no | **DEFER-BEHAVIOR** |
| C9 | Fulfillment / document / delivery / financial statuses | DOMAIN/02, 07, 11 | six status enums | `CommercialStatus` only | deliberate: S1 carries no fulfillment behavior | yes | **KEEP** |
| C10 | EMD issuance snapshot, ticket/coupon links | DOMAIN/07 | present | absent | S2+ | no | **DEFER-BEHAVIOR** |

## D. Travelers, journeys, segments, contacts

| # | Concept | Pack 3.8 requirement | Historical `k8s-stg` | Current @ `deaf6b0` | Gap / information loss | S1 scope | Disposition |
|---|---|---|---|---|---|---|---|
| D1 | **Journey** | DOMAIN/04 §15; DOMAIN/13 table group `…/Journeys/Segments` | `OrderItinerary` (bound id, sequence, direction, origin, destination) | **absent** | `airTransports[].BoundId / Direction / Sequence / Origin / Destination` and `details.JourneyType` discarded | yes | **RESTORE OD-P-09** — raw direction and journey type preserved losslessly; canonical enum mapped only where the vocabulary is known |
| D2 | Sold segment facts | DOMAIN/04 §15 | full sold snapshot | **restored**: terminals, duration, aircraft, `SourceCapacityRef`, `FlightNumber`, `FlightVersion`, marketing and operating carrier, with the existing `FlightRef` reused as the external FlightId | none | yes | **RESTORE OD-P-10 + OD-P-21** — the segment is now the single canonical store for flight-level facts; cabin, RBD and booking class stay on the service detail; `SourceCapacityRef` is a retained source reference, not certified capacity truth |
| D3 | Segment legs | DOMAIN/04 §19 | airports, terminals, times, stop | sequence + `SourceLegRef` only | leg airports, terminals and times dropped | yes | **RESTORE OD-P-11** — no inferred connection/protection semantics |
| D4 | Connection / protection facts | DOMAIN/04 §19 | `FlightStopType` | `Stop` is an unparsed `JsonElement` | stop facts unread | yes if supplied | **BLOCKED_DECISION OD-P-12** — raw evidence retained, **no fake canonical `Unknown` row**, handoff prepared |
| D5 | Traveler | DOMAIN/04 | index, name, PTC, age range, DOB, gender, nationality, residence, documents, parent | source/client refs, PTC, guardian link, identity | gender/nationality/residence not supplied by AirOffer at S1 | yes | **KEEP** |
| D6 | Traveler documents | DOMAIN/04 | entity | absent | not supplied at S1 | no | **DEFER-BEHAVIOR** |
| D7 | PII handling | DOMAIN/04; OD-S1-04 | names in domain | names in domain, redacted in the projection, re-hydrated per surface by `OrderDtoReader` | none | yes | **KEEP** |
| D8 | Contacts | DOMAIN/13 | contact + contact points | role + email + phone | legacy cardinality not restored — the current shape satisfies the S1 contract and Pack minimum | yes | **KEEP** (corrected from BLOCKED by OD-P-13) |

## E. Pricing

| # | Concept | Pack 3.8 requirement | Historical `k8s-stg` | Current @ `deaf6b0` | Gap / information loss | S1 scope | Disposition |
|---|---|---|---|---|---|---|---|
| E1 | `PriceChangeSet` | DOMAIN/03 §9 defines exactly `(SetId, OrderId, ChangeId, FinancialSequence, Reason, SourceDecisionRef, BaseCommercialVersion, CommittedAt)` | reason + source + offer id + pricing ref | exactly the Pack's field set | none — legacy richness does not override the Pack | yes | **KEEP** (corrected by OD-P-14) |
| E2 | Magnitudes, direction, effect, role | DOMAIN/03 sign contract | non-negative + direction | non-negative `Money` pair, direction/effect/role, matrix policy | none | yes | **KEEP** |
| E3 | Line identity: Code / Name / Reference | DOMAIN/03; AirOffer supplies all three | `Code`, `Description` | absent; only a JSON-path `SourceLineRef` | an observed tax code or fee name is preserved nowhere | yes | **RESTORE OD-P-15** — no reinterpretation; `Code` is not assumed to be a tax code |
| E4 | Source-currency amount and code | DOMAIN/03 §31 | amount + currency id pair | `Money` pair, currency as stringified id | currency code never stored | yes | **REDESIGN OD-P-01** — code only where supplied, null otherwise |
| E5 | FX evidence | DOMAIN/03 | `ExchangeRate` VO | `SourceConversionRef` string only | from/to currency, rate, decimal places and rounding factor discarded | yes | **RESTORE OD-P-16** — small typed applied-conversion snapshot; rounding token preserved but never used in calculation |
| E6 | Quantity / UoM / unit price on the line | not required by Pack 3.8's canonical line; not supplied by AirOffer | present | absent | none proven by the current source | no | **DEFER-BEHAVIOR** (corrected by OD-P-17) |
| E7 | Percentage rows | AirOffer `IsPercentage`; OD-S1-08 | n/a | flag dropped after materialisation | a percentage-based row is indistinguishable from a flat one | yes | **RESTORE OD-P-18** — provenance only; the numeric percentage basis stays uninterpreted |
| E8 | Refundability, application level, tax details, settlement refs, occurrence key, transfer group, related operation, calculation snapshot | DOMAIN/03, DOMAIN/09 | all present | absent | servicing/settlement facts not supplied at S1 | no | **DEFER-BEHAVIOR** |
| E9 | Allocation sets and allocations | DOMAIN/03, DOMAIN/13 | full reconciliation | removed in the stage-04 cleanup | S1 has no allocation source | no | **DEFER-BEHAVIOR** |
| E10 | Per-traveler / per-coupon attribution | DOMAIN/03 | line → allocation | line basis ref `traveller\|bound\|flight` with `PricingBasisType.OrderService` | attribution preserved through the basis ref | yes | **KEEP** |
| E11 | Ticket-level base/charge/total | source facts | per-ticket amounts | reconciled, then dropped | derivable by summing that traveler's lines | yes | **KEEP** |

## F. Fare construction

| # | Concept | Pack 3.8 requirement | Historical `k8s-stg` | Current @ `deaf6b0` | Gap / information loss | S1 scope | Disposition |
|---|---|---|---|---|---|---|---|
| F1 | `AirFareConstruction` | DOMAIN/03 §47; DOMAIN/13 `FareConstructions/Groups/Units/Components/Bindings` | typed construction + item links | one `PricingUnitsJson` column | the Pack-mandated relational graph does not exist | yes | **REDESIGN OD-P-19** |
| F2 | `PricingGroup` | DOMAIN/03 §49 | typed group + traveler links | absent | traveler ↔ fare grouping unrepresentable | yes | **RESTORE OD-P-19** — structure only; **no groups invented for AirOffer**, which supplies none |
| F3 | `PricingUnit` | DOMAIN/03 §51 | typed unit | JSON node with hard-coded `Unspecified` / `ProviderDefined` | source `Kind` not preserved; combination method fabricated | yes | **RESTORE OD-P-19** — preserve source `Kind`; `FareCombinationMethod = Unspecified` when the source did not establish it |
| F4 | `FareComponent` | DOMAIN/03 §51 | typed component + service and segment links | JSON node with an empty coverage list | tariff/rule/routing/fare-owner refs absent; coverage empty | yes | **RESTORE OD-P-19** — **no fabricated component bindings**; coverage rows only where the source proves them |
| F5 | Item / bound bindings | DOMAIN/13 current-binding uniqueness | item link table | absent | construction not bound to items; `CoveredBoundOfferIds` not persisted | yes | **RESTORE OD-P-19** — preserve covered bounds at the level actually supplied |
| F6 | `TicketingRestrictionMinutes` | source fact on the component | n/a | parsed, never mapped | discarded | yes | **RESTORE OD-P-19** |
| F7 | `FareConstructionAssurance.Opaque` | SLICES/S1; CONTRACTS/02-AIROFFER "OpaquePricingContext" | n/a | set by the adapter | correct for AirOffer | yes | **KEEP** |

## G. Funding, receipts, durability

| # | Concept | Pack 3.8 requirement | Historical `k8s-stg` | Current @ `deaf6b0` | Gap | S1 scope | Disposition |
|---|---|---|---|---|---|---|---|
| G1 | `FundingObligation` | DOMAIN/06, DOMAIN/13 | payment summary | full obligation with version and supersession | none | yes | **KEEP** |
| G2 | Funding applications and evidence | DOMAIN/06 | n/a | absent | S2+ | no | **DEFER-BEHAVIOR** |
| G3 | `CommandReceipt` idempotency | DOMAIN/08, DOMAIN/13 | n/a | scoped key + stored result | none | yes | **KEEP** |
| G4 | `OrderPreparation` + `PreparationSourceEvidence` | SLICES/S1, DOMAIN/15 | n/a | present; consumed once inside the sale transaction; raw payload retained | none | yes | **KEEP** |
| G5 | Outbox / inbox / single projector | ARCHITECTURE/03 | present | present | none | yes | **KEEP** |
| G6 | One effective unit of work for create | ARCHITECTURE/03 | n/a | receipt + preparation consumption + order graph + projection + outbox in one SQL transaction | none | yes | **KEEP** — must not regress |

## H. Adapter assumptions that are not source facts

| # | Assumption | Where | Status after the owner's answers | Disposition |
|---|---|---|---|---|
| H1 | `AIROFFER-OBSERVED-AIR-UNCERTIFIED`, `CapacityUnits = 1`, `FlightCapacity`, `Etkt`, `RequiresFunding = true` | `AirOfferCandidateMapper.AirService` | moving the literals into configuration was rejected; the live profile must say "not certified" | **REDESIGN OD-P-07** |
| H2 | `Quantity = 1`, `OrderItemUnitOfMeasure.PassengerSegment` | same | one coupon is one passenger segment | **KEEP** |
| H3 | every row is `CustomerBalance` / `Debit` / `Original` | `AirOfferCandidateMapper.Line` | S1 sells only; a negative row throws | **KEEP** |
| H4 | category map `0..3 → Fare / Tax / Fee / CarrierSurcharge` | same | **closed**: CONTRACTS/02-AIROFFER records exactly these CLR values; unknown values stay `ContractMismatch` | **KEEP** (corrected by OD-P-20) |
| H5 | `AcceptanceAssurance.LocalCandidateOnly` | same | AirOffer publishes no binding acceptance | **KEEP** |
| H6 | `SegmentRef = "{BoundId}\|{FlightId}"`, `ServiceRef = "{TravellerRef}\|{SegmentRef}"` | same | synthetic, stable, reproducible | **KEEP** |
| H7 | infant (`INF`) rejected outright | same | BD-002 / OD-S1-07 unresolved | **KEEP** |

---

## Disposition counts (recalculated after the owner's answers)

| Disposition | Revision 1 | Revision 2 | Revision 3 | Why it moved |
|---|---|---|---|---|
| KEEP | 24 | 32 | **33** | A3 corrected: `OD-C-02` was already answered in the stage-04 register |
| RESTORE | 10 | 15 | **15** | unchanged; `OD-P-21` changed where two `RESTORE` rows write, not whether they do |
| REDESIGN | 8 | 7 | **7** | unchanged |
| REMOVE | 0 | 0 | **0** | — |
| DEFER-BEHAVIOR | 13 | 13 | **13** | unchanged |
| BLOCKED_DECISION | 6 | 2 | **1** | only `OD-P-12` (the AirOffer `Stop` vocabulary) remains |
| **Total rows** | 61 stated / 69 actual | 69 | **69** | every row counted exactly once |

Per-section totals:

| Section | Rows | KEEP | RESTORE | REDESIGN | DEFER | BLOCKED |
|---|---|---|---|---|---|---|
| A | 12 | 7 | 0 | 2 | 3 | 0 |
| B | 8 | 3 | 2 | 1 | 2 | 0 |
| C | 10 | 4 | 2 | 1 | 3 | 0 |
| D | 8 | 3 | 3 | 0 | 1 | 1 |
| E | 11 | 4 | 3 | 1 | 3 | 0 |
| F | 7 | 1 | 5 | 1 | 0 | 0 |
| G | 6 | 5 | 0 | 0 | 1 | 0 |
| H | 7 | 6 | 0 | 1 | 0 | 0 |
| **Total** | **69** | **33** | **15** | **7** | **13** | **1** |

`OD-P-12` (D4, the AirOffer `Stop` vocabulary) is the only remaining domain-parity blocker. `OD-C-02` and `OD-P-21` are **not** blocked. The `Stop` question, together with the undocumented `direction` and `journeyType` vocabularies, is carried to the AirOffer owner as handoff `OR-002` in `E:\Projects\DotAir\handoffs`.

## The five findings of the audit prompt, after the owner's answers

| Finding | Verdict | Outcome |
|---|---|---|
| FareConstruction stored as a JSON blob | **Confirmed** | approved for repair, with the correction that opaque/partial source structure must not be padded with fabricated rows |
| `PricingLine` too narrow | **Confirmed in part** | Code/Name/Reference, conversion evidence and percentage provenance approved; quantity/UoM/unit price rejected as speculative |
| AirOffer information loss | **Confirmed** | journeys, segment operational identity, legs, baggage, term flags, rates of exchange and currency code approved for preservation; `Stop` stays unread by owner decision |
| `OrderItem` lacks the Pack-required snapshots | **Confirmed** | both snapshots approved, with per-service granularity preserved and a defined item-level summary mapping |
| Hard-coded adapter assumptions | **Confirmed, and one closed** | the fulfillment profile needs a real reference-vs-live redesign; the numeric category map is closed by the Pack's own AirOffer contract |

Two of the audit's own recommendations were wrong and are corrected above: `OD-P-14` and `OD-P-20` used legacy richness and caution to argue against the Pack's explicit text, and `OD-P-05` named `LinkedAt` when the actual missing Pack field is `ScopeAtAssociation`.

S1 status: `S1_DOMAIN_REPAIR_PLAN_READY_FOR_OWNER_REVIEW`. S2: `S2_NOT_STARTED`.
