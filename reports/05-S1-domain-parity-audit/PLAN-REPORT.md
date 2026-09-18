# S1 Domain Parity — Plan Report

## 1. Repository state

- **Branch:** `k8s-stg`
- **Starting HEAD:** `deaf6b0031960d10a3cd0d756ddc19849447b305` ("Matrix") — exactly the commit the reviewer observed; the working tree was clean, so no reconciliation of the audit files was needed and nothing had to be inspected as "changes since".
- **Ending HEAD:** `deaf6b0031960d10a3cd0d756ddc19849447b305` — unchanged. Nothing was committed; the owner asks for commits explicitly.
- **Files changed (working tree, documentation only):**
  - `M reports/00-decisions/S1-DOMAIN-PARITY-OPEN-DECISIONS.md`
  - `M reports/05-S1-domain-parity-audit/DOMAIN-PARITY-MATRIX.md`
  - `?? reports/05-S1-domain-parity-audit/IMPLEMENTATION-PLAN.md`
  - `?? reports/05-S1-domain-parity-audit/PLAN-REPORT.md` (this file)
- **No production, domain, persistence, provider or test code was changed.** No migration was added, no database was touched, no test was run.

## 2. Owner decisions applied

| Decision | Final answer | Matrix effect |
|---|---|---|
| OD-P-01 source currency code | Approved with correction — preserve code **only where the source supplies it**; never from ReferenceData; null when absent | A5, E4 stay `REDESIGN`; per-value currency-code columns are explicitly **not** added |
| OD-P-02 `LastTicketingDate` | Approved with authority separation — typed observed source fact; the authoritative deadline stays unresolved under BD-004 | A7 stays `REDESIGN`, wording changed to name the separation |
| OD-P-03 `ProductSnapshot` / item | Approved in part — snapshot restored from supplied facts only; **item quantity/UOM rejected** | B2 `RESTORE`; **B1 corrected from `REDESIGN` to `KEEP`** |
| OD-P-04 `CommercialTermsSnapshot` | Approved with granularity correction — exact per-service flags **and** an item summary `Permitted / Prohibited / Conditional / Unknown`; disagreement is `Conditional`, never `NotSupplied` | B3 stays `RESTORE`; my proposed `NotSupplied`-on-disagreement rule removed |
| OD-P-05 `OrderItemServiceLink` | `LinkedAt` rejected — the real gap is **`ScopeAtAssociation`** | B7 stays `REDESIGN`, restated against DOMAIN/02 §13 |
| OD-P-06 `OrderService` base fields | Approved structurally, no invented values; `PriceTreatment = SupplierOpaque`; no fake `ServiceCode`, no inferred supplier/delivery ref, never `SeparatelyPriced` | C1 stays `RESTORE`, with all five fields planned and only one populated |
| OD-P-07 fulfillment profile | Options-binding rejected; reference-vs-live redesign required, with profile version and assurance | C2 stays `REDESIGN`; **H1 moves from `BLOCKED_DECISION` to `REDESIGN`**; live requirement semantics remain owner-blocked under BD-002/005/006 |
| OD-P-08 baggage allowance | Approved — typed sold allowance, checked and cabin separate, nothing inferred, no separate baggage service | C3 stays `RESTORE` |
| OD-P-09 Journey | Approved — bound id, sequence, direction fact, origin, destination, root `JourneyType`; raw preserved, canonical mapped only where known | D1 stays `RESTORE` |
| OD-P-10 sold segment | Approved with correction — restore the flight facts; **cabin/RBD/booking class not duplicated onto the segment**; `FlightCapacityId` retained as a source reference only | D2 stays `RESTORE`, scope narrowed |
| OD-P-11 legs | Approved — airports, terminals, times; no inferred stop semantics | D3 stays `RESTORE` |
| OD-P-12 `Stop` vocabulary | Remains blocked — no guessing, no fake canonical `Unknown` row, raw evidence retained, handoff prepared | D4 stays `BLOCKED_DECISION`; the "persist as `Unknown`" part of my recommendation is withdrawn |
| OD-P-13 contacts | Closed for S1 — keep the current model | **D8 corrected from `BLOCKED_DECISION` to `KEEP`** |
| OD-P-14 `PriceChangeSet` split | **Not approved** — Pack 3.8 defines the field set deliberately | **E1 corrected from `REDESIGN` to `KEEP`** |
| OD-P-15 pricing code / name / reference | Approved — canonical persistence and read model retain them; public exposure is a separate contract choice | E3 stays `RESTORE`; no silent API change |
| OD-P-16 rates of exchange | Approved with correction — a small typed applied-conversion snapshot, not the legacy `ExchangeRate` type; rounding token preserved but uninterpreted; no FX engine | E5 stays `RESTORE`, restated |
| OD-P-17 line quantity / UoM / unit price | **Not approved for S1** | **E6 corrected from `RESTORE` to `DEFER-BEHAVIOR`** |
| OD-P-18 percentage rows | Partially approved — provenance preserved; numeric interpretation still not invented | **E7 corrected from `BLOCKED_DECISION` to `RESTORE`**, with the interpretation excluded |
| OD-P-19 fare-construction graph | Approved with an opaque-source correction — typed graph, but no fabricated groups or component bindings; `CombinationMethod = Unspecified`; source `Kind` preserved | F1 `REDESIGN`, F2–F6 `RESTORE`, each restated as "structure only, rows only where the source proves them" |
| OD-P-20 numeric category map | **Closed** — CONTRACTS/02-AIROFFER records `Fare=0, Tax=1, Fee=2, Surcharge=3` | **H4 corrected from `BLOCKED_DECISION` to `KEEP`**; no raw-category column |
| OD-P-21 (new) carrier / flight version placement | **Open** | no disposition change; the plan carries both paths |

## 3. Audit corrections

- **OD-P-20 closure.** The audit called the numeric pricing-category map an owner blocker. It is not: `docs/ORDERING-DESIGN-PACK-v3.8/CONTRACTS/02-AIROFFER.md` states "Pricing category CLR values are Fare=0, Tax=1, Fee=2, Surcharge=3. Unknown enum value fails mapping." The mapping is contractual under the current AirOffer profile, the existing throw already satisfies the safeguard, and the extra raw-category column the audit proposed is unnecessary because the raw payload is already retained in `PreparationSourceEvidence`. H4 is now `KEEP`.
- **OD-P-14 rejection.** The audit used the legacy entity's richness to argue that `SourceDecisionRef` is too opaque. Pack 3.8 DOMAIN/03 §9 defines the field set exactly as implemented, the Order already retains the accepted source and offer identity, and requiring callers to parse a decision reference for business meaning would be worse than the opacity complained of. E1 is now `KEEP`. This is the clearest instance of the standing rule that the legacy implementation is evidence, not authority.
- **OD-P-17 deferral.** Neither Pack 3.8's canonical line nor the AirOffer S1 contract supplies a pricing-line quantity, unit of measure or unit price. Adding three nullable columns on the strength of the legacy model would have been exactly the speculative persistence the audit criticised elsewhere. E6 is now `DEFER-BEHAVIOR`.
- **OD-P-05 `ScopeAtAssociation` correction.** The audit asked for `LinkedAt` and missed the field the Pack actually names: DOMAIN/02 §13 defines `OrderItemServiceLink(LinkId, OrderIdAtAssociation, OrderItemId, OrderServiceId, ScopeAtAssociation, LinkedByChangeId)`. `LinkedByChangeId` already carries the change and its committed instant, so the timestamp was redundant while the association scope — the thing that makes a historical item's sold contents reconstructable after reparenting — was the real omission. B7 is restated around `ScopeAtAssociation`, and the plan models it as typed child rows, not JSON.
- **Opaque FareConstruction correction.** The audit implied a full hierarchy should be populated. AirOffer is an explicitly opaque construction source, and CONTRACTS/02-AIROFFER already says so ("Incomplete fare-construction mapping is retained as OpaquePricingContext with available source unit/component data … No inferred PU grouping by route shape"). The typed model must therefore hold a construction with units, components and covered bounds but **zero** pricing groups and **zero** component-coverage rows without that being a defect. The plan also stops writing `FareCombinationMethod = ProviderDefined`, which was a fabricated value, and preserves the source `PricingUnit.Kind` verbatim instead.

## 4. Revised matrix counts

| Disposition | Revision 1 | Revision 2 |
|---|---|---|
| KEEP | 24 | **32** |
| RESTORE | 10 | **15** |
| REDESIGN | 8 | **7** |
| REMOVE | 0 | **0** |
| DEFER-BEHAVIOR | 13 | **13** |
| BLOCKED_DECISION | 6 | **2** |
| **Rows** | 61 stated / 69 actual | **69** |

Revision 1's stated totals did not add up to its own row count; revision 2 counts every row exactly once, and the per-section breakdown is printed in the matrix. The two remaining blocked rows are **A3** (`OD-C-02`, the `SalesChannel` C1 allowlist conflict, open since stage 04) and **D4** (`OD-P-12`, the `Stop` vocabulary, blocked by the owner's own answer). No approved item is still marked blocked, and no rejected legacy-only field is still counted as `RESTORE`.

## 5. Implementation scope

Fifteen approved repairs, all additive to the accepted record and none of them behavioral:

1. **Order sale currency** gains the source currency code where supplied (`CurrencySnapshot`); `Money` is deliberately left alone.
2. **Applied conversion evidence** as a small typed snapshot on `PricingLine`, resolved from the source rate period; rounding token preserved and never calculated with.
3. **Percentage provenance** as a new `PricingCalculationKind` on the line; the numeric basis stays uninterpreted.
4. **`OrderItem.ProductSnapshot`** from supplied facts only, with no invented product identity and no package-level carrier.
5. **Commercial terms** preserved twice: the exact three flags per air service, and an item-level summary with the owner's `Permitted / Prohibited / Conditional / Unknown` rule.
6. **`OrderService`** gains `ServiceCode`, `Name`, `PriceTreatment`, `SupplierPartyRef`, `DeliveryProviderRef`; AirOffer populates only `SupplierOpaque`.
7. **Fulfillment profile** redesigned into a certified reference profile and an explicitly not-certified live profile with a version, `Unresolved` requirement semantics and a nullable capacity unit.
8. **Baggage allowance**, checked and cabin kept separate, on the typed air-transport detail.
9. **Journey** restored as an entity between Order and Segment, with the raw direction and journey type preserved and the canonical enums left unmapped.
10. **Sold segment** regains terminals, duration, aircraft, flight id and version, carriers and the source capacity reference — without cabin, RBD or booking class.
11. **Legs** regain airports, terminals and times, with no stop or connection semantics.
12. **Pricing lines** regain the source code, name and reference.
13. **The fare-construction graph** becomes eight typed tables that support partial/opaque source structure, and `PricingUnitsJson` is retired in a gated three-step migration.
14. **`LastTicketingDate`** becomes a typed observed fact separate from the still-unresolved authoritative deadline.
15. **`OrderItemServiceLink`** gains `ScopeAtAssociation` as typed child rows.

Roughly 30 new files and 30 changed files, concentrated in Domain and Persistence; the fare graph is more than half of it. The public API contract is unchanged; three additive exposures are offered for the owner to accept or decline.

## 6. Explicitly excluded work

- Any interpretation of the AirOffer `Stop` node — **no code is written for it at all** (OD-P-12).
- Percentage-rate interpretation without a contract: `AirOfferPricingLineWire.Amount` is never read as a rate (OD-P-18).
- S2 capacity behavior — no reservation, no FlightFlow mutation, no eligibility from `FlightCapacityId`.
- Payment behavior.
- Document issue behavior.
- Servicing, refund and exchange behavior.
- `PricingLine` quantity, unit of measure and unit price (OD-P-17).
- The `PriceChangeSet` legacy source split (OD-P-14).
- `ContactPoint` redesign (OD-P-13).
- Item quantity and unit of measure (OD-P-03).
- A raw pricing-category column (OD-P-20).
- Any generic fulfillment-profile engine, and any FX engine or rate-table subsystem.
- Any change to the ratified public API contract.

## 7. Migration approach

One additive migration, one data migration, one gated drop.

- **`S1DomainParityTypedStructures`** adds every new column and table, nullable wherever the fact may be absent. Three non-nullable additions get a deterministic, non-inventing backfill: `OrderSegments.JourneyId` points at one synthetic `UNKNOWN` journey per existing order with a null direction; `PricingLines.SourceCalculationKind` is `Amount`; `OrderServices.PriceTreatment` is `SupplierOpaque`. Everything else stays `NULL` for existing rows, and the item terms summary is written as `Unknown / Unknown / Unknown`, which is literally true of those rows.
- **`S1DomainParityFareGraphBackfill`** reads each stored `PricingUnitsJson` and writes typed units and components using **only** the members that JSON actually holds. No pricing group and no component-coverage row is created, because the blob proves neither. `FareCombinationMethod` is rewritten from the fabricated `ProviderDefined` to `Unspecified`.
- **`S1DomainParityDropPricingUnitsJson`** removes the blob in a separate commit, and only after the equivalence test proves every migrated construction reproduces its blob's unit and component facts with matching row counts.
- No destructive reset, no data loss, and the raw source evidence is never touched, trimmed or re-parsed — re-deriving accepted facts from the raw wire would be a second acceptance.
- `MigrationUpgradeTests` covers both paths: fresh database with no pending model changes, and an upgrade from an existing S1 database with accepted rows that must survive with exactly the backfill values above.

## 8. Test plan

- A new `InformationPreservationTests` traces **AirOffer wire → NormalizedCandidate → Order → SQL → projection → rebuild** for 31 named vectors covering currency identity and code, pricing code/name/reference, original and sale values, conversion evidence, `IsPercentage`, fare basis/family/type, ticketing restriction minutes, source unit kind, covered bound ids, checked and cabin baggage, the three term flags, bound id, raw direction, journey type, flight id and version, capacity reference, terminals, duration, aircraft, carriers, leg detail, the typed observed ticketing date, and the historical item–service link scope.
- Three assertions carry the point: **no owner call is needed after acceptance** to read any vector back; **nothing is silently dropped** — every wire member is either mapped or named in an explicit "intentionally not consumed" set, so a new member added upstream fails the suite until classified; and the **projection rebuild stays byte-identical**.
- Every existing reliability guarantee keeps its existing test unchanged: idempotency and replay, payload conflict, single consumption of the accepted source, no remote call inside the SQL transaction, one atomic local transaction, one projector, deterministic rebuild, concurrency uniqueness, evidence retention, pricing sign and total invariants, PII redaction, authorization boundaries, live-vs-reference separation, layer and Messages-allowlist boundaries, and decimal precision for every new column. Nothing is weakened, disabled or deleted to fit the new model.
- Two tests legitimately need new expected values because the canonical candidate form moves to schema 3.0: `CandidateCanonicalFormTests` and any fixture asserting a literal digest. Old and new vectors are kept side by side so the schema move is visible rather than silent.

## 9. New blockers discovered

**OD-P-21 — where marketing/operating carrier and flight version live once the segment is restored.** OD-P-10 puts marketing carrier, operating carrier and `FlightVersion` on the sold segment. All three already exist per passenger on the typed air-transport service detail, and two are published in the ratified public projection as `airTransport.marketingAirlineRef` and `.operatingAirlineRef`. Applying OD-P-10 literally creates two stores of the same source fact for the same flight; removing them from the service detail removes fields from a ratified response. My recommendation is to make the segment the single accepted store for flight-level facts, keep the service detail for passenger-level facts, and keep the public fields by having the projector read them from the segment — the wire contract does not change, only the source of the value. Recorded with an `Answer:` line in the decisions register. Planning continued around it: both paths are carried and §10 of the plan depends on the answer only for where the writes land, not for whether they happen.

No other new semantic question arose. `OD-C-02` (the `SalesChannel` C1 allowlist conflict) is still open from stage 04 and is unaffected by this work.

## 10. Status

`S1_DOMAIN_REPAIR_PLAN_READY_FOR_OWNER_REVIEW`

`S2_NOT_STARTED`
