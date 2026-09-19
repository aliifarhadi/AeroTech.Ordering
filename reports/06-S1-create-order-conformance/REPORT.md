# S1 Create-Order Domain Benchmark & Conformance Audit

**Revision 3** — closure plan corrected after the second independent review. Revision 1's executive conclusion stays withdrawn; revision 2's diagnosis survives, but five of its recommendations and its scenario arithmetic do not.

## 1. Repository state

Two states must be distinguished, because revision 2's wording no longer holds.

**During the revision-2 audit run:** branch `k8s-stg`, HEAD `506cceeb4971dedce5d0905641f3250cd2e25459` ("reports"), worktree clean. That run committed nothing, and the statement "nothing committed" was accurate when it was written.

**Current, and the baseline for this run:**

- **Branch:** `k8s-stg`
- **HEAD:** `4e48447f51ab2ecc1829f30ec1c14d759f2659f5` ("Reports Added")
- **Parent:** `506cceeb4971dedce5d0905641f3250cd2e25459`
- **What `4e48447` contains:** exactly the ten Stage-06 report files — eight modified, two added, 784 insertions, all under `reports/06-S1-create-order-conformance/`. Verified with `git diff --stat 506ccee HEAD`: **zero** changes under `src/`, `Contracts/`, `tests/` or `Migrations/`. The branch state is report-only.
- **Worktree at the start of revision 3:** clean.
- **Changed in this run:** `reports/06-S1-create-order-conformance/` only. **No file under `src/`, `Contracts/`, `tests/` or `Migrations/`; no package reference; no public API code; no migration.** Nothing committed in this run.
- **Production/domain baseline:** `e86171103e9fef0b6a57a7895b32ccc87b0e5773` ("S1-Domain Repair") — unchanged since it landed.
- **Historical evidence repository:** `E:\Projects\DotAir\Ordering` @ `077a851`, read only.

## 2. Authority and benchmark sources

Authority order applied: owner decisions → Pack 3.8 → actual AeroTech contracts and observed wire → IATA standards and AIDM → Amadeus/Sabre/Navitaire → historical repository → engineering preference last.

Revision 3 re-read `DOMAIN/02` §19 and §23, `DOMAIN/03` §11 and §13–§20, `DOMAIN/06` in full and `DOMAIN/07`, and traced `Order.SellingOfficeId`, `CandidateSalesContext`, `NormalizedCandidateJson`, `FundingObligation` with its EF configuration, `DocumentAuthority`, `FulfillmentDocumentKind`, `OrderProjectionJson.SchemaVersion` and both commercial-status enums line by line.

**23 benchmark sources**, 17 carrying a positive claim. Revision 3 adds **model maturity** beside retrieval confidence, which revision 2 omitted:

| AIDM row | Page status | Used as |
|---|---|---|
| M1 Order Item, M2 Service, M3 Commission, M5 Price | **Approved** | normative corroboration |
| M4 Distribution Chain Role Code, M6 Distribution Chain Link | **Proposed** | corroboration only — no AeroTech shape rests on these |

Two limits stand: Sabre returns **HTTP 403** to this agent, so Sabre claims are capped at MEDIUM and no Sabre field is asserted; and IATA's Implementation Guides remain behind the developer portal, though AIDM does not.

## 3. Executive result

**`S1_CREATE_ORDER_DOMAIN_GAPS_FOUND`**

Revision 2's substantive findings are all confirmed by the second review and stand: **4 blockers**, **3 domain-shape debts**, **1 governance contradiction**, `FIX_DOMAIN_DESIGN` = **7** rows. The core architecture remains sound and a rewrite is not justified.

What revision 3 changes is the **closure plan**, not the diagnosis:

- **Scenario arithmetic was broken.** The table said 9 incorrectly unsupported, the prose said 11, the enumeration listed 12, and the population compared against revision 1 was not the same population. Corrected to `61 business/domain + 12 reliability = 73`, one primary disposition per scenario, primary counts summing to 61, **12** incorrectly unsupported.
- **One blocker was wrongly treated as free.** The lifecycle vocabulary correction is a shared-contract and public-wire change and needs owner approval — now **`OD-CLOSE-09`**. Only **A1** is unambiguous today.
- **The office collision is a current public bug**, not only a provenance debt — traced through to `OrderDto.AirlineOfficeId`.
- **Two plan items were missing entirely:** binding the repaired sales context into the accepted candidate digest, and the projection schema-4 transition.
- **Four recommended shapes were wrong** and are corrected: component totals, funding scope, fulfillment document authority, and the settlement benchmark wording.

## 4. Critical S1 gaps

### 4.1 An AirOffer response for a different offer is accepted silently

`AirOfferSourceAdapter.Interpret` calls `Map(request.OfferId, envelope.Data, …)` and never compares `envelope.Data.OfferId`. Receiver-qualified search confirms it is read nowhere. The Order is accepted with the other offer's money, journeys, services and fare construction while `AcceptedSourceOfferId`, `ProductSourceOfferId` and the public `offerId` all claim the requested offer. This defeats INV-006's "no silent repricing".

Closure is strict on **both** missing and mismatched: `CONTRACTS/02-AIROFFER` line 13 makes `LastTicketingDate?` the only optional root field. **This is the one item that needs no owner answer.**

### 4.2 A settlement line can be accepted with no counterparty and no category

`DOMAIN/03` §13 — "SettlementOnly requires party/category/currency." Neither exists in any layer. The owner kept the settlement half of SC-S1-013 in S1, and there is no `SC_S1_013` test at any layer.

Shape: `SettlementAttribution(PartyRef, CategoryCode)` — both **opaque source/contract strings**, required iff `Effect == SettlementOnly`, a SQL check against half-population, settlement currency = the committed `SaleValue` currency.

**Benchmark wording corrected.** Revision 2 claimed AIDM "models the category as a Code". That overstates M3, which proves only that **Commission** carries codes and may be an amount or a percentage. `CategoryCode` is an opaque code required by **Pack 3.8**; AIDM corroborates, it does not authorise. And settlement attribution is **not** commission-specific — `DOMAIN/03` §19–20 give Fee, Markup, Penalty and Discount their own settlement bases — so closure requires at least one **non-commission** `SettlementOnly` test (scenario 21).

### 4.3 No immutable `SalesContext`, no `BuyerSnapshot`, seller organisation discarded, office namespace already public

`DOMAIN/01` §2 requires an immutable `SalesContext` **and** a `BuyerSnapshot` at accepted creation. `DOMAIN/04` §5: "Buyer, financial Customer, Traveler, payer, agency/seller and actor are separate references … SalesContext … remain historical."

- `BuyerActorContextType` / `BuyerActorId` hold the **initiating actor**. `SalesContext` does not satisfy `BuyerSnapshot`; the Pack requires both, and **no surface supplies an independent buyer fact today**. Buyer must never be inferred from FinancialCustomer, Actor, Traveler or Seller.
- On OtaPanel, `TravelAgencyId` is resolved to a `FinancialCustomerId` and dropped. It survives only inside the `agency:{id}` fragment of the `CallerScope` **string** — an idempotency key, never to be parsed into a snapshot.
- **`SellingOfficeId` is a current public semantic bug.** Traced: `Order.cs:47` → `OrderProjectionBuilder.cs:19` → `OrderProjectionDocument.cs:15`, where the position is renamed **`AirlineOfficeId`** because both are positional records → `OrderProjectionMapper.cs:22` → `OrderDto.cs:14`. **A `GET` on an OtaPanel order returns a travel-agency office id under the field name `airlineOfficeId`.** Scope: the S1 outbox writes `IntegrationEvents.V2.OrderCreated`, which has no office field, so no integration event is affected.
- **The repair must reach the accepted candidate.** `CandidateSalesContext` already participates in `NormalizedCandidateJson` under the `salesContext` key and therefore in the acceptance digest. Changing only `Order` would leave the accepted sales context outside the digest that proves it — the hole revision 2's plan left open.

Needs `OD-CLOSE-05`, which now also carries the public-contract choice. Do **not** build a distribution-chain engine; no surface supplies a chain.

### 4.4 Commercial lifecycle vocabulary does not match the Pack — and is a public-contract decision

| Enum | Current | `DOMAIN/02` requires |
|---|---|---|
| `OrderServiceCommercialStatus` | `Pending=1, Active=2, Cancelled=3, Exchanged=4, Suspended=5` | §48: Pending, Active, Cancelled, **Replaced**, **Expired** |
| `OrderItemCommercialStatus` | `Active=1, Replaced=2, Cancelled=3` | §52: Active, **PartiallyChanged**, Cancelled, Replaced, **Partitioned**, **Expired**, **Inactive** |

`Exchanged` is an operation outcome; `Suspended` belongs to the delivery axis, where `OrderServiceDeliveryStatus` already exists. AIDM M2 separates `Status Code` from `Delivery Status Code`.

**Revision 2 recorded this as "not a decision". That was wrong on process.** Both enums are in shared `Contracts/AeroTech.Messages`, both are in the public `OrderDto`, and both serialize by name — a shared-contract and public-wire change under `GOVERNANCE/05`. It is now **`OD-CLOSE-09`**, with a recommended numbering that preserves every existing value: `Replaced = 4` and `Expired = 5` take the unused service slots, the item enum appends 4–7, and item `Replaced`/`Cancelled` are **not** renumbered to match prose order.

Blast radius measured: `Exchanged` and `Suspended` are referenced nowhere in `src` or `tests`, and only `Active` is ever written. This is the cheapest moment to correct it; after S5 it would mean migrating accepted rows.

## 5. Important but non-blocking gaps — domain shape decided now, code later

| # | Debt | Pack text | Corrected position |
|---|---|---|---|
| 5.1 | `FulfillmentProfileSnapshot` | `DOMAIN/02` §23 names eight semantics | **Revision 2's analysis was wrong twice.** Coverage is **five of eight**, not "4½": `FulfillmentDocumentKind` (`None` / specific kind / `Unresolved`) already expresses document **requirement and type**, so no second flag should be added. And document **authority** is *not* an undefined vocabulary — `DOMAIN/07` defines LOCAL-AIRLINE / EXTERNAL and `DocumentAuthority { Local, External }` already exists in Contracts. Add `DocumentAuthority?` and a **nullable** `PartialFulfillmentSupported`; a non-nullable `false` would assert that partial fulfillment is unsupported, which no source has said. The three genuinely undefined policy semantics stay as source-owned opaque refs. `OD-CLOSE-07` |
| 5.2 | `FundingObligation` scope and disposition | `DOMAIN/06`: "Service/**Item**/PricingLine scope … and **current disposition**" | **Revision 2's shape was wrong.** `ScopeKind + ScopeId` is a polymorphic FK that SQL Server cannot enforce, inside an aggregate whose other three references are real FKs. Corrected to `OrderItemId?` + `OrderServiceId?` + `PricingLineId?`, real FKs, CHECK exactly one non-null, typed `FundingObligationScope` in the Domain, no discriminator. "Every fee-only `MonetaryCharge` needs line scope" is withdrawn as overstated. S1 stays item-scoped; the **disposition vocabulary is not enumerated anywhere in `DOMAIN/06`** and must be asked. `OD-CLOSE-08` |
| 5.3 | No persisted current component totals | `DOMAIN/01` §2: "complete current component totals" | **Revision 2's shape was insufficient.** A single unsigned `Amount` cannot hold a component with both debit and credit lines, and a signed net would contradict `DOMAIN/03` §11 — "Direction alone supplies sign". Corrected to `(OrderId, Component, Effect, DebitAmount, CreditAmount, CurrencyRef)`, net derived, all effects included, sale currency only, order level only, a deterministic summary and not a second source of truth. `OD-CLOSE-06` |

## 6. False alarms and concepts correctly deferred

**Seat — unchanged across all three revisions.** `DOMAIN/02`'s typed-details table gives Seat its own row, separate from the AirTransport row; `VERTICAL-SLICE-PLAN` puts it in **S6**. A sold seat product, a requested seat, a reserved inventory seat and a delivered/assigned DCS seat are four facts with four lifecycles. `AirTransportDetail` carries **no** seat field; the historical repository's `RequestedSeat` on the air detail is the precedent avoided. AIDM M2 and Navitaire N3 corroborate seat merchandising as distinct from the flight. **`STRUCTURALLY_READY`, no S6 code.**

**Interline settlement is *not* a current S1 gap — corrected in revision 3.** Revision 2 counted it as a second `CURRENT_S1_GAP`. Interline behaviour is not an S1 requirement and the owner never kept it in S1. Reclassified to **`FUTURE_DOMAIN_DEPENDENCY` + `BLOCKED_OWNER_CONTRACT` (BD-012)**. The only obligation it places on S1 is that `SettlementAttribution` be decided **generically** — which is why a non-commission test is now required. `CURRENT_S1_GAP` accordingly falls from 3 to **2**.

Also still correct: the ETKT-coupon-entity rejection (the Pack calls AirOffer's coupons a priced projection), the route-shape fare-inference rejection, the generic-Order-TTL rejection, and the finding that the S1 path is not over-abstracted.

## 7. Pricing and settlement verdict

| Pack rule | Enforced | Verdict |
|---|---|---|
| CustomerBalance alone contributes to `CustomerTotal` | `PricingArithmetic.CustomerTotal` filters on `Effect == CustomerBalance`, so SettlementOnly **and** Informational are both excluded | **PASS** |
| Commission is not a customer charge | `PricingLineMatrix` + SQL `CK_PricingLines_CommissionNotCustomer` + `CandidateValidatorTests` | **PASS** |
| Tax cannot be settlement-only | matrix + SQL + `PackExamples.SettlementTax` negative fixture | **PASS** |
| Direction alone supplies sign | non-negative magnitudes + `PricingLineMatrix.Sign` | **PASS** |
| **SettlementOnly requires party / category / currency** | **nowhere** | **FAIL** |

**Agency commission.** `DOMAIN/03` §19–20 make settlement a property of Fee, Markup, Penalty and Discount as well as Commission, so the S1 fix must be generic rather than commission-shaped. AIDM M3 corroborates that commission is remuneration "paid to an agent" and may be amount or percentage; `PricingCalculationKind` is already the seam for the percentage case, and `OD-P-18` stays binding — AirOffer's `Amount` is never read as a percentage value.

**SC-S1-013.** The owner split it: the reversal half is deferred, the settlement/commission half "stays proven". Status of that retained half — arithmetic proven, but only on `PricedAmount` arrays in a pure domain test; matrix and SQL rules proven; **representation incomplete**; **end-to-end unproven**, with no `SC_S1_013` test at any layer and an AirOffer mapper that only ever emits `CustomerBalance`.

Scope boundary reaffirmed: Ordering preserves the accepted commercial/settlement fact and nothing more. No Commission aggregate, no commission calculation, no agency master, no BSP engine, and no assumption that the settlement party equals the seller.

## 8. AirOffer information-loss verdict

Unchanged.

| Outcome | Count |
|---|---|
| Wire fields audited | **105** |
| Read and preserved, or provably derivable | **99** |
| Deliberately ignored with a recorded reason | **3** — `CouponId`, `Coupon.Sequence`, `TravellerIndex` |
| Blocked by an owner-contract gap | **2** — `Flight.Stop`, `Leg.Stop` (`OD-P-12`, handoff `OR-002`) |
| Lost | **1** — `Details.OfferId`, lost as a **validation**, strict on missing and mismatched |
| Silently lost with no reason recorded | **0** |

The rate-of-exchange limit stands: the recorded live response references a period whose own currencies (`71 → 70`) do not correspond to the line's (`155 → 70`). Evidence is preserved verbatim; nothing interprets it; the meaning sits inside `OR-002`.

## 9. Scenario coverage verdict

**Accounting corrected.** Revision 2's totals did not close, and its growth comparison mixed two populations.

`61 business/domain (1–61) + 12 reliability (R1–R12) = 73 total.` Revision 1's 43 was 31 business/domain + 12 reliability, so like-for-like growth is **31 → 61**, not 43 → 61. Every business/domain scenario now has exactly **one primary disposition**, and the primary counts sum to 61 with no overlap subtraction anywhere.

| Primary disposition | Count |
|---|---|
| `TESTED` | **24** |
| `UNTESTED` — representable, but no test asserts the scenario's own claim | **24** |
| `UNSUPPORTED` — cannot be represented today and should be | **12** |
| `DEFERRED` — content belongs to a later slice | **1** |
| **Total business/domain** | **61** |
| Reliability (R1–R12), all proven | 12 |
| **Total** | **73** — 36 proven |

The 12 `UNSUPPORTED`: 4 party/role (8, 9, 11, 12), 5 settlement (17–21), 1 component totals (38), 2 contract fail-closed (54, 55). That matches the reviewer's own enumeration; revision 2's "9" and "11" were both wrong.

`TESTED` fell 26 → 24 because revision 3 applies a stricter rule — a scenario counts as tested only when a named test asserts **its own** claim, not an adjacent rule — which moved scenarios 25, 27, 44 and 58 to `UNTESTED`. **No test was removed, disabled or weakened.**

Four owner-blocked facts (infant/BD-002, `Stop`, bound `direction`, root `journeyType`) are now **secondary tags on their host scenarios**, not a sixth primary bucket. Counting them as a bucket is why revision 2's columns did not sum.

Pack reconciliation unchanged: 19 of 21 `SC-S1-*` have named tests; `SC-S1-014` is absent by ratified owner decision; `SC-S1-013` is absent and only half-ratified.

## 10. Full-domain future readiness

Six classes, one primary per family, fifteen families:

| Classification | Families |
|---|---|
| `STRUCTURALLY_READY` | **5** — seat/ancillary, exchange fare graph, disruption, delivery, traveler correction |
| `DOMAIN_SHAPE_GAP` | **5** — reservation snapshot, funding scope/disposition, document authority, cancellation vocabulary, split status |
| `ADDITIVE_FUTURE_EXTENSION` | **2** — refund, group (plus split's lineage half) |
| `FUTURE_DOMAIN_DEPENDENCY` *(new class)* | **1** — interline/partner settlement |
| `CURRENT_S1_GAP` | **2** — agency settlement, accepted sales provenance *(was 3)* |
| **Total** | **15** |

`BLOCKED_OWNER_CONTRACT` is a **secondary** tag on 5 families (BD-002/003, BD-006, BD-010, BD-012) and is never added into the primary totals.

Three S1 decisions that genuinely improved future readiness still stand: seat stayed out of `AirTransportDetail`; the fare-construction graph became typed; `ScopeAtAssociation` was added to the item–service link.

## 11. Code simplicity and readability findings

Unchanged. Finding 1's fix stays scoped so the **canonical candidate JSON and every accepted digest remain byte-identical** — only the in-memory model becomes typed — and it stays a post-closure item, which is exactly how `OD-C-09` splits.

Deliberately retained so nobody "simplifies" them later: the single-entry `ServiceDetailSchemaRegistry` (the SC-S1-021 gate), the three mirrored model families (the Pack requires the separation), the four per-surface command triplets (a ratified owner decision), and the `20288` throw in `OrderProjectionMapper`.

The inverted result holds: the real problems in this path are **missing checks and missing accepted facts**, not excess machinery.

## 12. Open owner decisions

| ID | Title | State in revision 3 |
|---|---|---|
| `OD-CLOSE-01` | Settlement attribution on a pricing line | **benchmark wording narrowed**; shape unchanged; non-commission test added |
| `OD-CLOSE-02` | Fail-closed check on `data.offerId` | unchanged — strict on missing and mismatched |
| `OD-CLOSE-03` | Reconcile the stage-04 cleanup decisions | reconciliation table; `OD-C-04` contradiction; a recommended answer per row |
| `OD-CLOSE-04` | Public exposure of pricing code and name | unchanged; explicitly **not** a domain blocker |
| `OD-CLOSE-05` | SalesContext / BuyerSnapshot / InitiatingActor | **expanded** — three separate snapshots, candidate-digest binding, public office-contract choice |
| `OD-CLOSE-06` | Component totals | **shape corrected** — debit/credit pair |
| `OD-CLOSE-07` | FulfillmentProfileSnapshot | **analysis corrected** — authority vocabulary already exists; nullable partial-fulfillment |
| `OD-CLOSE-08` | FundingObligation scope and disposition | **shape corrected** — three real FKs with an exactly-one CHECK |
| **`OD-CLOSE-09`** | **Commercial lifecycle vocabulary — shared contract and public wire** | **new** |

Every `Answer:` line is empty. Nothing anywhere in this bundle is an accepted decision; everything under a **Recommendation** heading is a recommendation.

Open elsewhere and not duplicated: `OD-P-12` (AirOffer `Stop`, `direction`, `journeyType` — handoff `OR-002`) and BD-001 … BD-013.

**`OD-C-04` contradiction, unchanged:** `S1-API-READABILITY-OWNER-DECISION` line 60 says `GetOperation` "remains reachable on the internal surface only"; `OD-C-04` says the query, read model and routes were deleted entirely; the code has **no** `GetOperation` on any surface. The implementation followed the report, not the decision.

## 13. Recommended repair scope

Detail in `IMPLEMENTATION-PLAN.md`.

**A — before S1 approval.** A1 `offerId` fail-closed *(the only unambiguous item; can start now)*; A2 `SettlementAttribution`; A3 sales context + buyer + actor + office kind, **including the canonical candidate and its digest**; A4 lifecycle vocabulary *(now gated on `OD-CLOSE-09`)*; A5 component totals; A6 the missing tests *(20 of the 24 need no new field and can start now)*; A7 the `OD-C-04` reconciliation; **A8 projection schema 4** *(new)*.

**A8 in brief:** schema 2 and schema 3 stay readable; schema 4 becomes current for all new writes and deterministic rebuilds; no facts are fabricated when reading 2 or 3; the deterministic rebuild is the upgrade path, so schema 3 is never mutated in place; stale-rebuild concurrency is unchanged; R13 covers 2→4 and 3→4.

**B — shape decided now, implementation later.** B1 fulfillment profile (`DocumentAuthority?` and a nullable `PartialFulfillmentSupported` now; three opaque policy refs with the slices that need them); B2 funding-scope FKs now, disposition vocabulary asked rather than invented.

**C — explicitly deferred behaviour.** Reservation, payment, documents, cancellation, ancillary add, seat assignment, allocation and reversal, refund, exchange, disruption, delivery, split, group, **interline** — no code, no tables, no handlers.

**D — post-closure simplifications.** Typed in-memory candidate detail with an unchanged canonical digest; the candidate-member reflection guard; the `Map` method extraction.

**Migration strategy — revision 2's "one migration for the whole batch" is withdrawn.** These changes have different authorities, different owner answers, different backfill semantics and different verification needs; batching them makes each un-revertable without the others. Replaced by: one migration per coherent decision; honest backfill (`NotSupplied` / null / unresolved — never back-derived from today's ReferenceData and never parsed out of a `CallerScope` string); every new reference a **real FK** and every conditional-presence rule a **SQL CHECK**; each migration naming the decision that authorised it, with guard migrations failing loudly rather than coercing a value; and an upgrade test per migration asserting that accepted rows survive without invented facts, plus the digest (R14) and rebuild (R13) transition tests.

## 14. S1 status

`S1_NOT_READY_FOR_APPROVAL`

Four blockers must close (4.1–4.4), **nine** owner decisions must be answered, and the scenario closure plan must be executed. Only **A1** and the 20 field-independent tests can proceed today; A4 is no longer free. None of this requires a rewrite; the architecture is sound.

## 15. S2 status

`S2_NOT_STARTED`
