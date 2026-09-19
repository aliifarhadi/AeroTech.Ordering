# S1 Create-Order Domain Benchmark & Conformance Audit

**Revision 2** — corrected after independent review. Revision 1's executive conclusion is withdrawn.

## 1. Repository state

- **Branch:** `k8s-stg`
- **HEAD:** `506cceeb4971dedce5d0905641f3250cd2e25459` ("reports") — exactly the reviewer-observed commit, so nothing had advanced and no diff had to be inspected.
- **Production/domain baseline parent:** `e86171103e9fef0b6a57a7895b32ccc87b0e5773` ("S1-Domain Repair").
- **Worktree at start:** clean.
- **Changed in this run:** `reports/06-S1-create-order-conformance/` only — eight documents updated, two added (`INDEPENDENT-REVIEW-CORRECTIONS.md`, `IMPLEMENTATION-PLAN.md`).
- **Code changed:** **none.** No file under `src/`, `Contracts/`, `tests/` or `Migrations/`; no package reference; no public API.
- **Historical evidence repository:** `E:\Projects\DotAir\Ordering` @ `077a851`, read only.

## 2. Authority and benchmark sources

Authority order applied: owner decisions → Pack 3.8 → actual AeroTech contracts and observed wire → IATA standards and AIDM → Amadeus/Sabre/Navitaire → historical repository → engineering preference last.

Revision 2 re-read `DOMAIN/01` §2–§3, `DOMAIN/02` §7–§23 and §46–§52, `DOMAIN/04` §5, `DOMAIN/06`, `CONTRACTS/02-AIROFFER` line 13, and `AuthorizedScopeResolver`, `CallerScopeKey`, `PricingArithmetic`, `PricingLineMatrix` and the status enums line by line — which is what revision 1 did not do.

**Benchmark sources: 15 rows in revision 1 → 23 in revision 2**, 17 carrying a positive claim. The new group is **IATA AIDM**, which revision 1 wrongly declared unavailable:

| AIDM entity | What it proves | Confidence |
|---|---|---|
| **Order Item** (25.2) | "An individually priced item within an Order, made up of one or more Services." Associates **Commission ("may be present")**, Price, Change/Cancel Restrictions, Penalty. Carries Grand Total Amount and four separate time limits. | HIGH |
| **Service** (25.2) | "**At time of order, the services should be applied to a single passenger on a single segment.**" "At time of Order Creation an Offered Service can become multiple services within the Order Item as the service is broken down per segment and passenger." Carries `Status Code` **and separately** `Delivery Status Code`. | HIGH |
| **Commission** (24.1) | "A remuneration … **paid to an agent** …" Attributes: Amount, **Code**, **Commission Code**, Percentage Applied To Amount, Percentage Percent, Remark Text, Taxable Indicator — all 0..1. | HIGH |
| **Distribution Chain Role Code** (25.2) | Carrier / Distributor / Seller, defined as roles "within the distribution chain mechanism itself, **not an entity's primary business classification**". | HIGH |
| **Price** (24.1) | Base Amount, Total Amount, Equivalent Amount; associates Fee, Markup, **Tax Summary**, Discount, Surcharge, Currency Conversion, at **Order** and **Order Item** level. | HIGH |
| **Reference Business Architecture** | Separates "**Customer Order Accounting**" from "**Partners/Suppliers Order Accounting**". | HIGH |

Two limits stand: Sabre returns **HTTP 403** to this agent, so Sabre claims are capped at MEDIUM and no Sabre field is asserted; and IATA's Implementation Guides remain behind the developer portal, though AIDM does not.

## 3. Executive result

**`S1_CREATE_ORDER_DOMAIN_GAPS_FOUND`**

Revision 1 concluded "only two small gaps" and `FIX_DOMAIN_DESIGN = 0`. **That conclusion is withdrawn.** It audited the concepts it went looking for and never walked `DOMAIN/01` §2, `DOMAIN/02` §48–52 or `DOMAIN/06` field by field against the code. Four of the five new gaps are single sentences in those three sections.

Corrected position: **4 S1 blockers**, **3 domain-shape debts**, **1 governance contradiction**, and `FIX_DOMAIN_DESIGN` = **7 rows**, not 0.

The core architecture remains sound and a rewrite is not justified: canonical Order boundary, accepted-source evidence, idempotency, SQL atomicity and concurrency, typed fare construction, segment/service ownership, offline projection rebuild, PII separation, and owner/payment/document/delivery boundaries all hold.

## 4. Critical S1 gaps

### 4.1 An AirOffer response for a different offer is accepted silently

`AirOfferSourceAdapter.Interpret` calls `Map(request.OfferId, envelope.Data, …)` and never compares `envelope.Data.OfferId`. Receiver-qualified search confirms it is read nowhere. The Order is accepted with the other offer's money, journeys, services and fare construction while `AcceptedSourceOfferId`, `ProductSourceOfferId` and the public `offerId` all claim the requested offer. This defeats INV-006's "no silent repricing".

**Correction to revision 1:** revision 1 recommended tolerating a *missing* `data.offerId` because our C# mirror types it `string?`. The Pack is authority, not the mirror: `CONTRACTS/02-AIROFFER` line 13 lists `LastTicketingDate?` as the **only** optional root field. Closure target is strict on **both** missing and mismatched.

### 4.2 A settlement line can be accepted with no counterparty and no category

`DOMAIN/03` §13 — "SettlementOnly requires party/category/currency." Neither exists in any layer. The owner kept the settlement half of SC-S1-013 in S1 (`S1-API-READABILITY-OWNER-DECISION` line 64), and there is no `SC_S1_013` test at any layer.

**Correction to revision 1:** it recommended a new `SettlementCategory` **enum**. AIDM models the category as a **Code** (Commission has `Code` and `Commission Code`, both 0..1), and inventing a closed vocabulary the owner has not defined is exactly what `GOVERNANCE/05` forbids. Corrected shape: `SettlementAttribution(PartyRef, CategoryCode)` — both source-owned strings, required iff `Effect == SettlementOnly`, with a SQL constraint preventing half-population.

### 4.3 No immutable `SalesContext`, no `BuyerSnapshot`, seller organisation discarded *(new)*

`DOMAIN/01` §2 requires an immutable `SalesContext` and a `BuyerSnapshot` **at accepted creation**. `DOMAIN/04` §5: "Buyer, financial Customer, Traveler, payer, agency/seller and actor are separate references … SalesContext … remain historical."

Today:
- `BuyerActorContextType` / `BuyerActorId` hold the **initiating actor** — `AirlineUserId`, `TravelAgencyUserId`, `PartnerApiAccessProfileId` or `ActorId` by surface. Naming an actor a buyer is a naming error with consequences.
- On OtaPanel, `TravelAgencyId` is resolved to a `FinancialCustomerId` and then **dropped**. It survives only inside the `agency:{id}` fragment of the `CallerScope` **string** on `OrderPreparations` and `CommandReceipts` — so the fact is durable, but only as a parsed idempotency key, which is not a snapshot and is not queryable. Historical seller identity otherwise depends on mutable ReferenceData.
- **A defect the reviewer did not name:** `Order.SellingOfficeId` is one untyped `long` holding **two namespaces** — airline office on Backoffice, travel-agency office on OtaPanel. A later join to an airline-office master silently mis-joins agency offices.

AIDM independently separates Carrier, Distributor and Seller as chain roles distinct from organisation type. Needs `OD-CLOSE-05`; do **not** build a distribution-chain engine — no surface supplies a chain.

### 4.4 Commercial lifecycle vocabulary does not match the Pack *(new)*

| Enum | Current | `DOMAIN/02` requires |
|---|---|---|
| `OrderServiceCommercialStatus` | `Pending, Active, Cancelled, Exchanged, Suspended` | §48: Pending, Active, Cancelled, **Replaced**, **Expired** |
| `OrderItemCommercialStatus` | `Active, Replaced, Cancelled` | §52: Active, **PartiallyChanged**, Cancelled, Replaced, **Partitioned**, **Expired**, **Inactive** |

`Exchanged` is an operation outcome, not a canonical state. `Suspended` has no home on the commercial axis — §52 says "Fulfillment/delivery facets remain separate", and `OrderServiceDeliveryStatus` already exists. AIDM corroborates: Service carries `Status Code` and, separately, `Delivery Status Code`.

No owner decision overrides the Pack here, so this is not a decision — it is a correction. Migration risk is low: `Exchanged` and `Suspended` are referenced **nowhere** in `src` or `tests`, and only `Active` is ever written. This is the cheapest moment in the project's life to fix it; after S5 it would mean migrating accepted rows.

## 5. Domain-shape debts — decide now, implement later

| # | Debt | Pack text | Current | Why it cannot simply be deferred |
|---|---|---|---|---|
| 5.1 | `FulfillmentProfileSnapshot` covers 4½ of 8 semantics | `DOMAIN/02` §23 names resource quantity/**unit policy**, document requirement/type/**authority**, **delivery provider/control policy**, **dependency treatment**, **partial-fulfillment support** | `ProfileRef, ProfileVersion, Assurance, ReservationRequirement, DocumentKind, FundingRequirement, CapacityUnits` | revision 1 rated this `BLOCKED_OWNER_CONTRACT` and stopped, conflating blocked **values** with an incomplete **shape**. S2 is the first slice to need unit policy; adding it later rewrites accepted snapshots. `OD-CLOSE-07` |
| 5.2 | `FundingObligation` has item scope only and no disposition | `DOMAIN/06`: "Service/**Item**/PricingLine scope … and **current disposition**" | `OrderItemId?` only | revision 1 called this "fully sufficient for S3" — **wrong**. A fee-only `MonetaryCharge` needs line scope; an S6 added service needs service scope. The **disposition vocabulary is not enumerated anywhere in `DOMAIN/06`**, so it must be asked, not invented. `OD-CLOSE-08` |
| 5.3 | No persisted current component totals | `DOMAIN/01` §2: "Derived-but-persisted: CommercialSummary, CustomerTotal **and complete current component totals**" | only the first two | AIDM `Price` carries Base/Total plus Fee, Markup, **Tax Summary**, Discount, Surcharge at Order and Item level. Not information loss — the lines survive — but a Pack conformance gap revision 1 never raised. `OD-CLOSE-06` |

## 6. False alarms and concepts correctly deferred

**Seat — unchanged and reconfirmed.** `DOMAIN/02`'s typed-details table gives **Seat** its own row ("Exactly one traveler and related air service; seat product/characteristics, requested seat when sold by identifier"), separate from the AirTransport row; `VERTICAL-SLICE-PLAN` puts it in **S6**. A sold seat product, a requested seat, a reserved inventory seat and a delivered/assigned DCS seat are four facts with four lifecycles. `AirTransportDetail` carries **no** seat field; the historical repository's `RequestedSeat` on the air detail is the precedent to avoid. AIDM M2 and Navitaire N3 ("communicate seat fees and assign or change seat assignments") corroborate seat merchandising as distinct from the flight. **`DEFER_IMPLEMENTATION` / `STRUCTURALLY_READY`, no S6 code.**

Also still correct from revision 1: the ETKT-coupon-entity rejection (the Pack calls AirOffer's coupons a priced projection), the route-shape fare-inference rejection, the generic-Order-TTL rejection, and the finding that the S1 path is not over-abstracted.

**Historical repository comparison** is unchanged from revision 1 except for one row: `OrderPricingLine.SettlementPartyRef` / `.SettlementCategory` are confirmed as the one genuinely useful legacy semantic that must return — justified by `DOMAIN/03` §13 and AIDM Commission, with the legacy field only as corroboration, never as authority.

## 7. Pricing and settlement verdict

| Pack rule | Enforced | Verdict |
|---|---|---|
| CustomerBalance alone contributes to `CustomerTotal` | `PricingArithmetic.CustomerTotal` filters on `Effect == CustomerBalance`, so SettlementOnly **and** Informational are both excluded | **PASS** |
| Commission is not a customer charge | `PricingLineMatrix` + SQL `CK_PricingLines_CommissionNotCustomer` + `CandidateValidatorTests` | **PASS** |
| Tax cannot be settlement-only | matrix + SQL + `PackExamples.SettlementTax` negative fixture | **PASS** |
| Direction alone supplies sign | non-negative magnitudes + `PricingLineMatrix.Sign` | **PASS** |
| **SettlementOnly requires party / category / currency** | **nowhere** | **FAIL** |

**SC-S1-013.** The owner split it: the reversal half is deferred; the settlement/commission half "stays proven". Status of that retained half — arithmetic proven, but only on `PricedAmount` arrays in a pure domain test; matrix and SQL rules proven; **representation incomplete**; **end-to-end unproven**, with no `SC_S1_013` test at any layer and an AirOffer mapper that only ever emits `CustomerBalance`.

Scope boundary reaffirmed: Ordering preserves the accepted commercial/settlement fact and nothing more. No Commission aggregate, no commission calculation, no agency master, no BSP engine, no assumption that the settlement party equals the seller, and no percentage/basis numeric fields merely because AIDM supports them — `OD-P-18` stays binding for AirOffer.

## 8. AirOffer information-loss verdict

Unchanged from revision 1 except the OfferId closure target.

| Outcome | Count |
|---|---|
| Wire fields audited | **105** |
| Read and preserved, or provably derivable | **99** |
| Deliberately ignored with a recorded reason | **3** — `CouponId`, `Coupon.Sequence`, `TravellerIndex` |
| Blocked by an owner-contract gap | **2** — `Flight.Stop`, `Leg.Stop` (`OD-P-12`, handoff `OR-002`) |
| Lost | **1** — `Details.OfferId`, lost as a **validation** (now strict on missing **and** mismatched) |
| Silently lost with no reason recorded | **0** |

The rate-of-exchange limit stands: the recorded live response references a period whose own currencies (`71 → 70`) do not correspond to the line's (`155 → 70`). Evidence is preserved verbatim; nothing interprets it; the meaning sits inside `OR-002`.

## 9. Scenario coverage verdict

Expanded from 43 to **61** scenarios; revision 1 never probed the item-composition boundary, the party/role boundary, or seven pricing boundaries.

| Outcome | Rev 1 | Rev 2 |
|---|---|---|
| Representable **and** tested | 24 | **26** |
| Representable but untested | 9 | **24** |
| Correctly deferred | 4 | **4** |
| **Incorrectly unsupported** | 2 | **11** |
| Blocked by owner semantics | 4 | **4** |

The 11 incorrectly unsupported: four party/role (agency identity survives a ReferenceData change; the four roles are not conflated; the partner profile is not a buyer; office namespaces distinguished), five settlement (amount-based commission end to end; reject with no attribution; reject with half attribution; category preserved losslessly; two counterparties stay distinct), one component-total reconciliation, and two contract fail-closed (`offerId` missing, `offerId` mismatched).

The jump from 2 to 11 is not new breakage — it is coverage revision 1 never attempted.

Pack reconciliation unchanged: 19 of 21 `SC-S1-*` have named tests; `SC-S1-014` is absent by ratified owner decision; `SC-S1-013` is absent and only half-ratified.

## 10. Full-domain future readiness

Revision 1's "13 of 14 families clean `DEFER_IMPLEMENTATION`" is **withdrawn**; it collapsed "is the behavior deferred" and "is the shape correct" into one answer. Reclassified:

| Classification | Families |
|---|---|
| `STRUCTURALLY_READY` | **5** — seat/ancillary, exchange fare graph, disruption, delivery, traveler correction |
| `ADDITIVE_FUTURE_EXTENSION` | **2** — refund, group (plus split's lineage half) |
| `DOMAIN_SHAPE_GAP` | **5** — reservation snapshot, funding scope/disposition, document authority, cancellation vocabulary, split status |
| `BLOCKED_OWNER_CONTRACT` | **5** value-side blocks — BD-002/003, BD-006, BD-010, BD-012 |
| `CURRENT_S1_GAP` | **3** — agency settlement, interline settlement, accepted sales provenance |

Three S1 decisions that genuinely improved future readiness still stand: seat stayed out of `AirTransportDetail`; the fare-construction graph became typed; `ScopeAtAssociation` was added to the item–service link.

## 11. Code simplicity and readability findings

Unchanged except finding 1, whose fix is now scoped so the **canonical candidate JSON and every accepted digest stay byte-identical** — only the in-memory model becomes typed. Revision 1's version would have been a breaking change to the acceptance contract, not a simplification.

Deliberately retained and recorded so nobody "simplifies" them later: the single-entry `ServiceDetailSchemaRegistry` (it is the SC-S1-021 gate), the three mirrored model families (the Pack requires the separation), the four per-surface command triplets (a ratified owner decision), and the `20288` throw in `OrderProjectionMapper`.

The inverted result holds and strengthens: the real problems in this path are **missing checks and missing accepted facts**, not excess machinery.

## 12. Open owner decisions

| ID | Title | Status |
|---|---|---|
| `OD-CLOSE-01` | Settlement attribution on a pricing line | **recommendation corrected** — source-owned `CategoryCode`, not a new enum |
| `OD-CLOSE-02` | Fail-closed check on `data.offerId` | **recommendation corrected** — strict on missing **and** mismatched |
| `OD-CLOSE-03` | Reconcile the stage-04 cleanup decisions | **reframed** — reconciliation table, not eight blanket answers; `OD-C-04` carries a genuine contradiction |
| `OD-CLOSE-04` | Public exposure of pricing code and name | unchanged; explicitly **not** a domain blocker |
| `OD-CLOSE-05` | Accepted Buyer / Seller / SalesContext snapshot | **new** — includes the per-surface availability table |
| `OD-CLOSE-06` | Component totals semantics | **new** |
| `OD-CLOSE-07` | FulfillmentProfileSnapshot target shape | **new** |
| `OD-CLOSE-08` | FundingObligation scope and disposition | **new** |

Open elsewhere and not duplicated: `OD-P-12` (AirOffer `Stop`, `direction`, `journeyType` — handoff `OR-002`) and BD-001 … BD-013.

**`OD-C-04` contradiction, recorded explicitly:** `S1-API-READABILITY-OWNER-DECISION` line 60 says `GetOperation` "remains reachable on the internal surface only"; `S1-CLEANUP-OPEN-DECISIONS` `OD-C-04` says the query, read model and routes were deleted entirely; the code has **no** `GetOperation` on any surface. The implementation followed the report, not the decision. Revision 1 missed this.

## 13. Recommended repair scope

Detail in `IMPLEMENTATION-PLAN.md`. Summary:

**A — before S1 approval.** A1 `offerId` fail-closed *(unambiguous, can start now)*; A2 `SettlementAttribution`; A3 `SalesContext`/seller snapshot and the office-namespace fix; A4 lifecycle vocabulary *(unambiguous, can start now)*; A5 component totals; A6 the missing tests *(20 of them need no new field and can start now)*; A7 the `OD-C-04` reconciliation.

**B — shape decided now, implementation later.** B1 the four remaining fulfillment-profile semantics, plus `PartialFulfillmentSupported` now because the Pack defines it without needing a vocabulary; B2 funding-obligation scope shape now, disposition vocabulary asked not invented.

**C — explicitly deferred behavior.** Reservation, payment, documents, cancellation, ancillary add, seat assignment, allocation and reversal, refund, exchange, disruption, delivery, split, group, interline — no code, no tables, no handlers.

**D — post-closure simplifications.** Typed in-memory candidate detail with an unchanged canonical digest; the candidate-member reflection guard; the `Map` method extraction.

One additive migration covers A2, A3, A4's guard, A5 and B1's boolean. Pre-repair rows receive honest `NotSupplied` values and are never back-derived from today's ReferenceData.

## 14. S1 status

`S1_NOT_READY_FOR_APPROVAL`

Four blockers must close (4.1–4.4), four new owner decisions must be answered (`OD-CLOSE-05` … `-08`), two recommendations have been corrected (`OD-CLOSE-01`, `-02`), and the scenario closure plan must be executed. None of this requires a rewrite; the architecture is sound.

## 15. S2 status

`S2_NOT_STARTED`
