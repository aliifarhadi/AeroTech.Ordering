# S1 Closure — Implementation Plan

Stage: 06-S1-create-order-conformance · 2026-09-19 · HEAD `506ccee`

**No code was written in this run.** This plan is the scope that closure would execute once the owner answers `OD-CLOSE-01` … `OD-CLOSE-08`.

Two items (A1, A4) are unambiguous — the Pack states them exactly and no owner decision overrides them — so they can be planned in full today. Everything else in A waits on an answer.

Simplicity constraints that bind every row: no Commission aggregate, no DistributionChain engine, no agency master in Ordering, no workflow engine, no generic commercial-status framework, no fulfillment rule engine, no JSON dictionary for a known canonical concept, no reflection or polymorphic framework for candidate details, and no S2–S16 tables added "for completeness".

---

## A. Must implement before S1 owner approval

| ID | Gap | Business meaning | Authority | Source supplies? | Canonical owner | Exact files | Migration | Projection | Public API | Tests | Implement now? | Why |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| **A1** | Response `offerId` never validated | The Order must come from the offer that was asked for | INV-006; `CONTRACTS/02-AIROFFER` line 13 lists root `OfferId` without the `?` that marks optional fields | yes — AirOffer returns it | the adapter, at the boundary | `Providers/AirOffer/Services/AirOfferCandidateMapper.cs` — one guard at the top of `Map` | none | none | none | 2 new negative tests (missing, mismatched) in `S1/AirOfferLiveCandidateBridgeTests` | **yes — unambiguous** | the Pack's own field list settles it; one ordinal comparison inside a method that already throws `ContractMismatch` six times |
| **A2** | Settlement line with no counterparty or category | Who settles, under what category | `DOMAIN/03` §13; INV-013; SC-S1-013 kept in S1 by owner decision | reference source can; AirOffer never emits a settlement line | `PricingLine` | new `Domain/_Shared/ValueObjects/SettlementAttribution.cs`; `PricingLine.cs`; `CandidatePricingLine.cs`; `NormalizedCandidateJson.cs`; `CandidateVocabulary.cs`; `PricingLineMatrix.cs`; `PricingLineConfiguration.cs`; `OrderProjectionDocument.cs`; `OrderProjectionBuilder.cs`; `CandidateBuilder.cs` | one additive migration: two nullable columns + a SQL check that both are present iff `Effect = SettlementOnly` | add to `ProjectedPricingLine` | none | `SC_S1_013` end to end (D,A,S,H); reject with no attribution; reject with half attribution; category preserved losslessly; two counterparties stay distinct | **after `OD-CLOSE-01`** | both members are source-owned **strings**, not a new enum — AIDM models the category as a Code |
| **A3** | No immutable `SalesContext` / `BuyerSnapshot`; seller organisation discarded; one office column, two namespaces | Who sold it, through which channel and office, on whose behalf — frozen at acceptance | `DOMAIN/01` §2 (required at accepted creation); `DOMAIN/04` §5 | per surface — see `OD-CLOSE-05` table; **no surface supplies a buyer today** | `Order`, as one owned value | new `Domain/OrderAggregate/ValueObjects/SalesContext.cs`; `Order.cs`; `AcceptOriginalSaleArgs.cs`; `AuthorizedSalesScope.cs`; `AuthorizedScopeResolver.cs`; `CreateOrderFromOfferService.cs`; `OrderConfiguration.cs`; projection document + builder | one additive migration; existing rows keep `Channel`/`SellingOfficeId` values and gain explicit kinds; seller organisation is `NotSupplied` for pre-repair rows — **never back-derived from today's ReferenceData** | add `SalesContext` to the internal document | none | agency identity survives a ReferenceData change; the four roles are not conflated; office namespaces distinguished; partner profile is not a buyer | **after `OD-CLOSE-05`** | the mapping differs per surface and no owner decision defines it; `BuyerActor*` is renamed to what it is — the initiating actor |
| **A4** | Commercial lifecycle vocabulary does not match the Pack | The canonical current commercial state of a service and an item | `DOMAIN/02` §48 (Pending, Active, Cancelled, **Replaced**, **Expired**) and §52 (Active, **PartiallyChanged**, Cancelled, Replaced, **Partitioned**, **Expired**, **Inactive**) | n/a — these are local states | `Contracts/AeroTech.Messages/Ordering/Enums` | `OrderServiceCommercialStatus.cs`, `OrderItemCommercialStatus.cs` | a migration **guard** that fails if any row holds an unexpected value; search confirms `Exchanged`(4) and `Suspended`(5) are written nowhere and only `Active` is ever persisted | enum names appear in the internal document and the public DTO — see note | **`OrderItemCommercialStatus` and `OrderServiceCommercialStatus` are serialized by name in the public `OrderDto`**, so additions are backward compatible but a rename is not. `Exchanged` → `Replaced` at value 4 is a wire-visible rename of a value **no order has ever held** | migration guard test; enum-conformance test asserting the member set matches `DOMAIN/02` | **yes — unambiguous vocabulary**, behavior still deferred | correcting it now means S5/S10/S14 never migrate accepted rows; `Suspended` moves off the commercial axis, where AIDM also separates `Status Code` from `Delivery Status Code` |
| **A5** | Pack-required current component totals | Base/tax/fee/surcharge/discount summary beside the grand total | `DOMAIN/01` §2; AIDM Price (Base Amount, Total Amount, Fee/Markup/Tax Summary/Discount/Surcharge) | derivable from the accepted lines | `Order`, as a deterministic persisted summary | new `Domain/OrderAggregate/Entities/OrderComponentTotal.cs`; `Order.cs`; new configuration; projection document + builder | one additive table `Order.OrderComponentTotals`; backfill computed deterministically from existing lines | add to the internal document | none | component-total reconciliation; settlement excluded from `CustomerTotal` but present as a component; rebuild determinism | **after `OD-CLOSE-06`** | the shape is clear; the **semantics** (customer-effective only? keyed by effect? canonical or summary?) are the owner's |
| **A6** | `SC_S1_013` and the untested boundaries | Proof, not claim | SLICES/S1 required scenario IDs | — | tests only | `tests/AeroTech.Ordering.Domain.Tests`, `tests/AeroTech.Ordering.Persistence.Tests` | none | none | none | the 24 "representable but untested" scenarios in `CREATE-ORDER-SCENARIO-MATRIX.md` §H | **yes for the 20 that need no new field**; the rest follow A2/A3/A5 | S1 cannot close with 19 of 21 Pack scenarios named and two of the gaps untested |
| **A7** | `OD-C-04` contradiction | An owner decision and the code disagree about `GetOperation` | `BLOCKED-DECISIONS.md` "Empty Answer lines … remain unanswered" | — | governance | `reports/00-decisions/S1-CLEANUP-OPEN-DECISIONS.md`; possibly restore QRY-002 on `Internal/v1` | none | none | internal surface only | route-inventory test if restored | **after `OD-CLOSE-03`** | either the owner confirms deletion and the earlier decision is amended, or the internal route returns |

### Note on A4 and the public contract

`OrderDto` serializes `OrderItemCommercialStatus` and `OrderServiceCommercialStatus` **by name** through `JsonStringEnumConverter`. Adding members is backward compatible. Renaming `Exchanged` to `Replaced` changes a wire string — but no accepted order has ever held that value, and `OpenApiDocumentTests` will show the enum-schema change deliberately rather than silently. This is the cheapest moment in the project's life to correct it; after S5 it would require migrating accepted rows.

---

## B. Domain shape must be decided now, implementation can remain later

| ID | Gap | Decide now | Implement when | Why not now |
|---|---|---|---|---|
| **B1** | `FulfillmentProfileSnapshot` — resource unit policy, document authority, delivery/control policy, dependency treatment, partial-fulfillment support | the **shape**, via `OD-CLOSE-07`; add `PartialFulfillmentSupported` (a boolean the Pack defines without needing a vocabulary) with the A-batch migration | the other four with the slice that first needs them — S2 (unit policy), S4 (document authority), S6 (dependency treatment), S12 (delivery/control) | the values are owner-blocked (BD-002/005/006) and inventing enums for four undefined vocabularies is exactly what `GOVERNANCE/05` forbids. Deciding the shape now is what stops S2 migrating accepted snapshots. |
| **B2** | `FundingObligation` — Service and PricingLine scope, current disposition, `Order.ObligationRevision` | the **scope shape** via `OD-CLOSE-08`: a scope-kind discriminator plus one reference, rather than three nullable FKs. Keep the S1 original-sale obligation **item**-scoped — that is genuinely what it is | service/line scope with S6 and S9; disposition with S3; `ObligationRevision` with S3 | no payment behavior may exist now, and the **disposition vocabulary is not enumerated anywhere in `DOMAIN/06`** — inventing `Open/Settled/Released` would be guessing. What must not happen is item-only scope hardening into an invariant. |
| **B3** | Item-level time limits (AIDM M1 has four) | nothing to decide yet | when a source supplies per-item limits | at S1 there is one item and one source; order-level validity facts are sufficient and INV-055 already forbids collapsing them into one TTL |
| **B4** | Percentage commission provenance (AIDM: Percentage Percent, Percentage Applied To Amount) | nothing to decide yet | when an owner contract supplies them | `PricingCalculationKind` is already the seam. `OD-P-18` stays binding: AirOffer's `Amount` is never read as a percentage value. |

---

## C. Explicitly deferred behavior

No code, no tables, no handlers, in this closure or its repairs:

reservation and capacity (S2) · payment, coverage and funding application (S3) · ETKT/EMD issuance (S4, S7) · cancellation (S5) · ancillary add and typed seat/bag/meal/lounge details (S6) · seat assignment of any kind · allocation sets and reversal lines (owner-deferred) · refund (S9) · exchange and reprice (S10) · disruption (S11) · delivery and consumption observations (S12) · traveler correction (S13) · split (S14) · group and charter (S15) · interline and partner settlement (BD-012) · any interpretation of AirOffer `Stop`, bound `direction` or root `journeyType` (`OD-P-12`, handoff `OR-002`) · any numeric interpretation of a percentage row (`OD-P-18`).

---

## D. Post-closure simplifications — not blockers

| ID | Item | Minimal change | Why it is not in A |
|---|---|---|---|
| **D1** | `CandidateService.Details` is a string dictionary for three typed concepts | typed `CandidateAirTransportDetail` in memory; the **canonical serializer keeps the identical `details` object, property names, ordinal key order and digest**; the reader still accepts the current schema; a test re-reads the existing pack example and reproduces the recorded digest unchanged | it changes no accepted fact and no invariant. If digest compatibility proves awkward, it stays deferred and that is recorded, not worked around. |
| **D2** | No guard that every candidate member reaches the canonical form | one reflection test over `NormalizedCandidate` and its nested records | `Node.Only(...)` already catches writer/reader drift; this closes the narrower case of a member added to a record and to neither side |
| **D3** | `AirOfferCandidateMapper.Map` is ~120 lines | extract `Journeys(details)` and the coupon line construction into private business-named methods | cosmetic; the single-pass traversal must stay because coupon totals reconcile against ticket totals against the root |

---

## E. Sequencing

1. **A1** and **A4** can start immediately — the Pack settles both and no owner answer is pending.
2. **A6**'s 20 field-independent tests can start immediately and will harden the model while the decisions are open.
3. **A2**, **A3**, **A5**, **A7** start when `OD-CLOSE-01`, `-05`, `-06`, `-03` are answered.
4. **B1**'s `PartialFulfillmentSupported` rides the same migration as A2/A3/A5 so there is one schema change, not four.
5. **D1–D3** after closure.

One migration for the whole A+B1 batch, additive, with the A4 guard failing loudly if any row holds an unexpected status. Pre-repair rows get honest `NotSupplied` values — never a back-derived identity from today's ReferenceData.
