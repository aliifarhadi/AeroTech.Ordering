# S1 Closure — Implementation Plan

Stage: 06-S1-create-order-conformance · **revision 3** · 2026-09-19 · HEAD `4e48447`

**No code was written in this run.** This plan is the scope that closure would execute once the owner answers `OD-CLOSE-01` … `OD-CLOSE-09`. Everything here is a recommendation; nothing is an accepted decision.

**Revision 3 corrections to the plan itself:**
- **A1 is the only item that is fully unambiguous today.** Revision 2 also listed A4 as unambiguous. That was wrong on process: the lifecycle enums are a shared contract and a public wire vocabulary, so **A4 now waits for `OD-CLOSE-09`.**
- **A3 gains the accepted-candidate half.** Revision 2 changed only `Order` and `AuthorizedSalesScope`, which would have left the repaired sales context **outside the acceptance digest**.
- **A8 is new: projection schema 4.** A2, A3 and A5 all change canonical internal projection facts; revision 2 never said what happens to schema 3.
- **A2, A5, B1, B2 carry the corrected shapes** from `OD-CLOSE-01`, `-06`, `-07`, `-08`.
- **The "one migration" optimisation is withdrawn** — see §E.

Simplicity constraints that bind every row: no Commission aggregate, no DistributionChain engine, no agency master in Ordering, no workflow engine, no generic commercial-status framework, no fulfillment rule engine, no JSON dictionary for a known canonical concept, no polymorphic foreign key, no reflection or polymorphic framework for candidate details, and no S2–S16 tables added "for completeness".

---

## A. Must implement before S1 owner approval

### A1 — Response `offerId` fail-closed · `OD-CLOSE-02` · **the only item that needs no owner answer**

| | |
|---|---|
| **Gap** | the AirOffer response `offerId` is never compared to the requested one |
| **Authority** | INV-006; `CONTRACTS/02-AIROFFER` line 13 lists root `OfferId` without the `?` that marks optional fields |
| **Source supplies?** | yes |
| **Canonical owner** | the adapter, at the boundary |
| **Files** | `Providers/AirOffer/Services/AirOfferCandidateMapper.cs` — one guard at the top of `Map` |
| **Migration / projection / public API** | none / none / none |
| **Tests** | 2 negative tests (missing-or-blank, mismatched) in `S1/AirOfferLiveCandidateBridgeTests` — scenarios 54, 55 |
| **Start now?** | **yes** — the Pack's own field list settles it. `OD-CLOSE-02` records the recommendation for the owner's file, but unlike every other A item it does not change a shape, a contract or a stored fact, so it is not gated |

### A2 — `SettlementAttribution` on a pricing line · waits for `OD-CLOSE-01`

| | |
|---|---|
| **Shape** | `SettlementAttribution(PartyRef, CategoryCode)` — both **opaque source/contract strings**, no enum, no interpretation, no assumption that `PartyRef` is the seller |
| **Rule** | required **iff** `Effect == SettlementOnly`; forbidden otherwise |
| **Currency** | the line's committed **`SaleValue`** currency is the settlement currency; `OriginalValue` stays provenance |
| **Files** | new `Domain/_Shared/ValueObjects/SettlementAttribution.cs`; `PricingLine.cs`; `CandidatePricingLine.cs`; `NormalizedCandidateJson.cs` (writer **and** reader); `CandidateVocabulary.cs`; `PricingLineMatrix.cs`; `PricingLineConfiguration.cs`; `OrderProjectionDocument.cs`; `OrderProjectionBuilder.cs`; `CandidateBuilder.cs` |
| **Migration** | two nullable columns + a CHECK that both are present iff `Effect = SettlementOnly` and both are absent otherwise |
| **Candidate contract** | this **does** enter the canonical candidate and therefore the acceptance digest — schema/canonicalization version bump, reader compatibility, digest test (see §A3's contract rules, which apply identically) |
| **Projection** | `ProjectedPricingLine` gains the pair → **schema 4** |
| **Public API** | none |
| **Tests** | `SC_S1_013` end to end (D,A,S,H) · reject with no attribution · reject with half attribution · category preserved losslessly · two counterparties stay distinct · **one non-commission `SettlementOnly` line** (`Fee` or `Markup` on an explicit source settlement basis) so the value object cannot become commission-shaped — scenarios 17–21 |

### A3 — `SalesContextSnapshot`, `BuyerSnapshot`, `InitiatingActor`, office namespace · waits for `OD-CLOSE-05`

Three separate persisted things, never collapsed into one. Buyer is **not** FinancialCustomer, **not** Actor, **not** Traveler, **not** Seller.

**The accepted-candidate half, which revision 2 omitted.** `CandidateSalesContext(OwnerAirlineId, FinancialCustomerId, Channel, SellingOfficeId)` already participates in `NormalizedCandidateJson` under the `salesContext` key and therefore in the acceptance digest. If seller identity, office kind, distributor or buyer presence were added only to `Order`, the accepted historical sale context would sit **outside** the digest that proves it. So A3 must change, in one coordinated step:

| Layer | Change |
|---|---|
| `CandidateSalesContext` | seller organisation ref + kind, office ref + **kind**, distributor ref where a surface supplies one, buyer presence/provenance |
| `NormalizedCandidateJson` | writer **and** reader, keeping ordinal key order; `Node.Only(...)` key set updated on both sides |
| canonical schema | candidate schema / canonicalization version bump — the current version's meaning is never mutated in place |
| `OrderPreparation` | persistence and read compatibility for preparations written under the previous schema |
| digest | a test that re-reads an existing pack example under the **old** schema and reproduces its recorded digest unchanged, plus a new-schema digest fixture |
| `Order` | the three snapshots as owned values |
| `AuthorizedSalesScope`, `AuthorizedScopeResolver`, `CreateOrderFromOfferService` | carry the facts instead of discarding them; `TravelAgencyId` is persisted as the seller organisation on OtaPanel |
| `OrderConfiguration` | mappings |
| projection | document + builder → **schema 4** |
| public API | `OD-CLOSE-05` option (a) or (b) — see the office trace below |

**Never derive historical seller from `CallerScope`.** The `agency:{id}` fragment is an idempotency key. It is evidence that the fact existed at the time; it is not a canonical snapshot and must not be parsed into one, not in code and not in a migration backfill.

**Office namespace, public half.** `Order.SellingOfficeId` → `OrderProjectionBuilder.cs:19` → `OrderProjectionDocument.AirlineOfficeId` → `OrderProjectionMapper.cs:22` → `OrderDto.AirlineOfficeId`. An OtaPanel order returns a travel-agency office under `airlineOfficeId` today. Option (a) renames to `sellingOfficeId` + `sellingOfficeKind`; option (b) adds those and keeps `airlineOfficeId` deprecated and populated **only** when the kind is `AirlineOffice`. The S1 outbox writes `IntegrationEvents.V2.OrderCreated`, which has no office field, so no integration event changes.

**Backfill honesty.** Pre-repair rows keep their `Channel` and office value and gain an explicit kind **only where the row's own `Channel` determines it**; seller organisation and buyer are `NotSupplied`. Nothing is back-derived from today's ReferenceData and nothing is parsed out of a scope string.

**Tests:** agency identity survives a ReferenceData change · the four roles are not conflated · office namespaces distinguished · partner profile is not a buyer · digest/schema transition (R14) — scenarios 8, 9, 11, 12.

### A4 — Commercial lifecycle vocabulary · **waits for `OD-CLOSE-09`**

| | |
|---|---|
| **Authority** | `DOMAIN/02` §48 and §52 |
| **Why gated** | both enums are in shared `Contracts/AeroTech.Messages`, both are in the public `OrderDto`, and both serialize **by name** — a public-contract change under `GOVERNANCE/05` no matter how clear the Pack text is. Revision 2 called this "not a decision"; that was wrong |
| **Files** | `OrderServiceCommercialStatus.cs`, `OrderItemCommercialStatus.cs` |
| **Numbering** | preserve every existing value; `Replaced = 4` and `Expired = 5` replace the unused `Exchanged`/`Suspended`; item enum appends 4–7. Do **not** renumber item `Replaced`/`Cancelled` to match prose order |
| **Migration** | a **guard only** — fail loudly if any row holds a value outside the approved set. Search confirms `Exchanged`(4) and `Suspended`(5) are written nowhere and only `Active` is ever persisted, so no data moves |
| **Public API** | OpenAPI enum schema changes; `OpenApiDocumentTests` must show it deliberately |
| **Tests** | migration guard test; enum-conformance test asserting the member set matches `DOMAIN/02` |
| **Behaviour** | still deferred — no transition logic is added |

### A5 — Pack-required current component totals · waits for `OD-CLOSE-06`

| | |
|---|---|
| **Shape** | `OrderComponentTotal(OrderId, Component, Effect, DebitAmount, CreditAmount, CurrencyRef)`, unique `(OrderId, Component, Effect)` |
| **Why not one amount** | `DOMAIN/03` §11 — amounts are nonnegative magnitudes and **Direction alone supplies sign**. One unsigned `Amount` cannot hold a component with both debit and credit lines, and a signed net would contradict the line model. Revision 2's single-`Amount` shape is withdrawn |
| **Rules** | net derived, never stored · sale currency only · all effects included · `CustomerTotal` stays `CustomerBalance` net only · order level only for S1 · deterministic persisted summary, not a second source of truth |
| **Files** | new `Domain/OrderAggregate/Entities/OrderComponentTotal.cs`; `Order.cs`; new configuration; projection document + builder → **schema 4** |
| **Migration** | one additive table; backfill computed deterministically from existing committed lines |
| **Tests** | component-total reconciliation · settlement excluded from `CustomerTotal` but present as a component · a component with both debit and credit lines keeps them apart · rebuild determinism — scenario 38 |

### A6 — The missing tests · 20 can start now

The 24 `UNTESTED` scenarios in `CREATE-ORDER-SCENARIO-MATRIX.md` §H. **Twenty need no new field** and can start immediately; four (10, and the settlement-adjacent ones) follow A2/A3. S1 cannot close with 19 of 21 Pack scenarios named and the settlement half untested at every layer.

### A7 — `OD-C-04` reconciliation · waits for `OD-CLOSE-03`

An owner decision and the code disagree about `GetOperation`. Either the owner confirms full deletion for S1 and the earlier internal-surface statement is amended, or QRY-002 returns on `Internal/v1`. Route-inventory test if restored.

### A8 — Projection schema 4 · **new in revision 3**

A2, A3 and A5 each add canonical facts to the internal projection. `OrderProjectionJson.SchemaVersion` is `3` today.

| Rule | Detail |
|---|---|
| **Do not mutate schema 3 in place** | a schema-3 row means what it meant when it was written |
| **Schema 4 becomes current** | new writes and all deterministic rebuilds produce 4 |
| **Schema 2 stays readable** | legacy public-DTO projection, already covered by R6 |
| **Schema 3 stays readable** | current internal projection, during the transition |
| **No fabrication** | reading a 2 or 3 row must not invent settlement attribution, sales context, buyer or component totals; absent facts read as absent |
| **Upgrade path** | the deterministic rebuild is what moves canonical SQL state to schema 4 — there is no in-place JSON rewrite |
| **Concurrency** | the existing stale-rebuild protection in `OrderProjector` is unchanged |
| **Tests** | deterministic rebuild **2 → 4** and **3 → 4** (R13), plus the existing byte-identical rebuild assertion re-run at 4 |

---

## B. Domain shape decided now, implementation later

| ID | Gap | Decide now | Implement when | Why not now |
|---|---|---|---|---|
| **B1** | `FulfillmentProfileSnapshot` — document **authority**, resource unit policy, delivery/control policy, dependency treatment, partial-fulfillment support | the **shape**, via `OD-CLOSE-07`. Add **`DocumentAuthority?`** (the enum already exists in Contracts — nothing is invented) and **`PartialFulfillmentSupported` as a nullable bool** with the A-batch migration | unit policy with S2, delivery/control with S12, dependency treatment with S6 — each as a **source/profile-owned opaque policy ref**, not an invented enum | the values are owner-blocked (BD-002/005/006), but the **shape** is not. A non-nullable `false` would assert that partial fulfillment is unsupported, which no source has stated. Revision 2 both called the authority vocabulary undefined (wrong — see `OD-CLOSE-07`) and proposed a plain bool (wrong for the same reason) |
| **B2** | `FundingObligation` — Service and PricingLine scope, current disposition, `Order.ObligationRevision` | the **scope shape** via `OD-CLOSE-08`: `OrderItemId?` + `OrderServiceId?` + `PricingLineId?`, **three real FKs** with a CHECK requiring exactly one non-null, plus a typed `FundingObligationScope` in the Domain. **No `ScopeKind` discriminator** — a polymorphic FK cannot be enforced by SQL Server and the aggregate's other three references are real FKs. Keep the S1 original-sale obligation **item**-scoped | service scope with S6, line scope when a `Fee`-purpose obligation's boundary genuinely is one line; disposition with S3; `ObligationRevision` with S3 | no payment behaviour may exist now, and the **disposition vocabulary is not enumerated anywhere in `DOMAIN/06`**, so it must be asked. What must not happen is item-only scope hardening into an invariant |
| **B3** | Item-level time limits (AIDM M1 has four) | nothing to decide yet | when a source supplies per-item limits | at S1 there is one item and one source; INV-055 already forbids collapsing order-level validity facts into one TTL |
| **B4** | Percentage commission provenance (AIDM: Percentage Percent, Percentage Applied To Amount) | nothing to decide yet | when an owner contract supplies them | `PricingCalculationKind` is already the seam. `OD-P-18` stays binding: AirOffer's `Amount` is never read as a percentage value |

---

## C. Explicitly deferred behaviour

No code, no tables, no handlers, in this closure or its repairs:

`OD-CLOSE-04` (public exposure of pricing code and name) is explicitly **not** in scope A and does not gate domain approval; if the owner wants it, it is an additive change to `OrderPriceLineDto` at any time.

Also deferred:

reservation and capacity (S2) · payment, coverage and funding application (S3) · ETKT/EMD issuance (S4, S7) · cancellation (S5) · ancillary add and typed seat/bag/meal/lounge details (S6) · seat assignment of any kind · allocation sets and reversal lines (owner-deferred) · refund (S9) · exchange and reprice (S10) · disruption (S11) · delivery and consumption observations (S12) · traveler correction (S13) · split (S14) · group and charter (S15) · **interline and partner settlement** (BD-012 — a future domain dependency that will reuse the same generic `SettlementAttribution`, not a current S1 gap) · any interpretation of AirOffer `Stop`, bound `direction` or root `journeyType` (`OD-P-12`, handoff `OR-002`) · any numeric interpretation of a percentage row (`OD-P-18`).

---

## D. Post-closure simplifications — not blockers

| ID | Item | Minimal change | Why it is not in A |
|---|---|---|---|
| **D1** | `CandidateService.Details` is a string dictionary for three typed concepts | typed `CandidateAirTransportDetail` in memory; the **canonical serializer keeps the identical `details` object, property names, ordinal key order and digest**; the reader still accepts the current schema; a test re-reads the existing pack example and reproduces the recorded digest unchanged | it changes no accepted fact and no invariant. If digest compatibility proves awkward, it stays deferred and that is recorded, not worked around. `OD-C-09` splits exactly this way |
| **D2** | No guard that every candidate member reaches the canonical form | one reflection test over `NormalizedCandidate` and its nested records | `Node.Only(...)` already catches writer/reader drift; this closes the narrower case of a member added to a record and to neither side. It becomes more valuable once A2 and A3 add members to that record |
| **D3** | `AirOfferCandidateMapper.Map` is ~120 lines | extract `Journeys(details)` and the coupon line construction into private business-named methods | cosmetic; the single-pass traversal must stay because coupon totals reconcile against ticket totals against the root |

---

## E. Sequencing and migration strategy

**Sequencing.**

1. **A1** starts immediately — the Pack settles it and no owner answer is pending. It is the **only** fully unambiguous item.
2. **A6**'s 20 field-independent tests start immediately and harden the model while the decisions are open.
3. **A4** starts when `OD-CLOSE-09` is answered.
4. **A2** → `OD-CLOSE-01` · **A3** → `OD-CLOSE-05` · **A5** → `OD-CLOSE-06` · **A7** (`GetOperation`) → `OD-CLOSE-03`.
5. **B1**'s two fields → `OD-CLOSE-07` · **B2**'s two FKs and CHECK → `OD-CLOSE-08`.
6. **A8** (schema 4) lands with whichever of A2/A3/A5 lands first and absorbs the rest.
7. **D1–D3** after closure.

**Migration strategy — the "one migration" optimisation is withdrawn.** Revision 2 recommended batching A2, A3, A4's guard, A5 and B1 into a single schema change "so there is one migration, not four". That optimises the wrong thing. These changes have different authorities, different owner answers, different backfill semantics and different verification needs, and a single migration makes each of them un-revertable without the others. Replace it with:

| Principle | What it means here |
|---|---|
| **Correctness over batching** | one migration per coherent decision — A2, A3, A5, A4's guard, B1, B2 each land on their own, in whatever order their decisions are answered |
| **Honest backfill** | pre-repair rows get explicit `NotSupplied` / null / unresolved. Nothing is back-derived from today's ReferenceData; nothing is parsed out of a `CallerScope` string; no boolean is defaulted to a business assertion |
| **FK and CHECK integrity** | every new reference is a real FK; every conditional-presence rule is a SQL CHECK, not application-only. B2's exactly-one-non-null and A2's present-iff-SettlementOnly are both enforced in the database |
| **Auditability** | each migration names the decision that authorised it; a guard migration fails loudly rather than coercing a value |
| **Upgrade verification** | each migration has an upgrade test asserting that accepted rows survive **without invented facts**, in the pattern of `S1_domain_parity_upgrade_preserves_accepted_rows_without_inventing_facts`; the canonical-candidate and projection changes additionally carry the digest (R14) and rebuild (R13) transition tests |
