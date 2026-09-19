# Independent Review — Corrections Applied

Stage: 06-S1-create-order-conformance · **revision 3** · 2026-09-19 · HEAD `4e48447`

Two independent reviews have now been answered. Neither previous report is defended: every claim was re-checked against Pack 3.8, the actual source, the actual migrations and public IATA AIDM, and where a prior recommendation was wrong it is withdrawn in place rather than argued.

---

# Round 2 — review of revision 2

The reviewer's repository-state correction is accepted: revision 2 **was** committed as `4e48447` ("Reports Added") after the run that produced it ended. That commit contains exactly the ten Stage-06 report files — eight modified, two added — and **zero** changes under `src/`, `Contracts/`, `tests/` or migrations. The claim "nothing committed" was true when it was written and is no longer true; revision 3 states both, which is what `REPORT.md` §1 now does.

## Round-2 verdict table

| # | Reviewer claim | Verdict | What changed in revision 3 |
|---|---|---|---|
| 1 | Scenario counts are internally inconsistent (table 9, prose 11, enumeration 12) and "43 → 61" is not apples-to-apples | **Confirmed — all three defects reproduce** | the matrix now states `61 business/domain + 12 reliability = 73`, gives every scenario exactly **one primary disposition**, and the five primary counts sum to 61. Like-for-like growth is restated as **31 → 61**. The true `UNSUPPORTED` count is **12**, which matches the reviewer's own enumeration. Overlap subtraction is gone |
| 2 | The lifecycle correction is **not self-authorizing** — it is a shared-contract and public-wire change | **Confirmed — revision 2 was wrong on process** | revision 2 listed this under "explicitly not a decision". It is now **`OD-CLOSE-09`**, and `IMPLEMENTATION-PLAN` A4 waits for it. The recommended numbering preserves every existing value, exactly as proposed |
| 3 | `SellingOfficeId` is already a **public** semantic bug, not only a provenance debt | **Confirmed, and traced end to end** | `Order.cs:47` → `OrderProjectionBuilder.cs:19` → `OrderProjectionDocument.cs:15` (`AirlineOfficeId`) → `OrderProjectionMapper.cs:22` → `OrderDto.cs:14`. The rename happens silently because both are positional records. `OD-CLOSE-05` now carries the public-contract choice (clean rename vs deprecated compatibility field). **Scope refinement:** the S1 outbox writes `IntegrationEvents.V2.OrderCreated`, which has no office field, so no integration event is affected; `V1.OrderCreated` does carry `AirlineOfficeId` but is not on the S1 path |
| 4 | A3 does not bind the repaired sales context into the **accepted candidate digest** | **Confirmed — a real hole in revision 2's plan** | `CandidateSalesContext(OwnerAirlineId, FinancialCustomerId, Channel, SellingOfficeId)` already participates in `NormalizedCandidateJson` under the `salesContext` key. A3 now changes the candidate record, the writer **and** reader, the canonicalization version, preparation read compatibility, and carries digest tests. The `agency:{id}` fragment is restated as an idempotency key that must never be parsed into a snapshot |
| 5 | `SalesContext` does not satisfy `BuyerSnapshot`; the Pack requires both | **Confirmed** | `OD-CLOSE-05` now decides **three** separate things — `SalesContextSnapshot`, `BuyerSnapshot`, `InitiatingActor` — and records that no surface supplies an independent buyer fact today. Explicit `NotSupplied` is offered as an owner choice, never as a default, and Buyer is never inferred from FinancialCustomer, Actor, Traveler or Seller |
| 6 | Projection schema transition is missing from A2/A3/A5 | **Confirmed** | new **A8**: schema 2 and 3 stay readable, schema 4 becomes current, no facts are fabricated when reading 2 or 3, the deterministic rebuild is the upgrade path, stale-rebuild concurrency is unchanged, and R13 covers 2→4 and 3→4. Schema 3 is not mutated in place |
| 7 | Settlement benchmark wording is too strong | **Confirmed** | revision 2 wrote that "AIDM models the category as a Code". That overstates M3. The corrected statement: `CategoryCode` is an **opaque source/contract-owned** settlement-category code required by Pack 3.8; AIDM corroborates that commission carries codes and is not the authority for AeroTech's generic category. Settlement currency is restated as the committed **`SaleValue`** currency, and a **non-commission `SettlementOnly`** test is now required (scenario 21, replacing revision 2's second commission scenario) |
| 8 | Component totals lose direction semantics under one unsigned `Amount` | **Confirmed** | `DOMAIN/03` §11 — amounts are nonnegative magnitudes and Direction alone supplies sign. `OD-CLOSE-06` now recommends `(OrderId, Component, Effect, DebitAmount, CreditAmount, CurrencyRef)` with net derived, all effects included, sale currency only, order level only, summary not canonical |
| 9a | Document authority is **not** an undefined vocabulary | **Confirmed — revision 2 was factually wrong** | `DOMAIN/07` defines LOCAL-AIRLINE / EXTERNAL and `DocumentAuthority { Local = 1, External = 2 }` already exists in `Contracts/AeroTech.Messages/Ordering/Enums`, used by `ElectronicTicketIssued` and `ElectronicMiscDocumentIssued`. Only the **live value** is unresolved. **Additional finding of this round:** `FulfillmentDocumentKind` (`None` / specific kind / `Unresolved`) already expresses document **requirement and type** together, so no separate `DocumentRequired` flag should be added — which raises the snapshot's true coverage from "4½ of 8" to **five of eight** |
| 9b | `PartialFulfillmentSupported` must allow unknown | **Confirmed** | a non-nullable bool backfilled `false` asserts that partial fulfillment is unsupported, which no source has stated. Corrected to a **nullable** bool. The three genuinely undefined policy semantics stay as source/profile-owned opaque refs |
| 10 | `ScopeKind + ScopeId` is a polymorphic FK that SQL Server cannot enforce | **Confirmed — revision 2's recommendation is withdrawn** | corrected to `OrderItemId?` + `OrderServiceId?` + `PricingLineId?`, real FKs, CHECK exactly one non-null, typed `FundingObligationScope` in the Domain, no discriminator. Revision 2's "every fee-only `MonetaryCharge` needs line scope" is also withdrawn as overstated. S1 stays item-scoped; disposition stays owner-required; `ObligationRevision` stays S3 |
| 11 | Interline settlement is not a separate current S1 blocker | **Confirmed** | reclassified to **`FUTURE_DOMAIN_DEPENDENCY` + `BLOCKED_OWNER_CONTRACT` (BD-012)** in both `FUTURE-READINESS` (family 13) and the benchmark matrix (L20). `CURRENT_S1_GAP` falls from 3 to **2**. The only S1 obligation it imposes is that `SettlementAttribution` be decided generically |
| 12 | AIDM maturity should be recorded separately from retrieval confidence | **Confirmed** | `BENCHMARK-SOURCES` now records page status: Order Item, Service, Commission and Price are **Approved**; Distribution Chain Role Code and Distribution Chain Link are **Proposed** and are used as corroboration only. No AeroTech shape rests on a `Proposed` page |
| 13 | Recommendations must not read as accepted decisions | **Confirmed** | `OPEN-DECISIONS` now states at the top that nothing is accepted until the owner writes on an `Answer:` line, and every `Answer:` line remains empty |

**Net effect on the headline numbers:** blockers stay at 4 but one of them (A4) becomes decision-gated rather than free; `CURRENT_S1_GAP` falls 3 → 2; `UNSUPPORTED` scenarios rise 9/11 → **12** with the arithmetic now closing; open decisions rise 8 → **9**; two plan items (candidate digest binding, projection schema 4) that revision 2 omitted entirely are added.

---

# Round 1 — review of revision 1

Everything below is unchanged from revision 2 except where round 2 supersedes it, which the rows above state explicitly.

## Verdict table

| # | Reviewer claim | Verdict | Evidence |
|---|---|---|---|
| 1 | OfferId not validated — S1 blocker | **Confirmed** (already in the prior report) | `AirOfferSourceAdapter.Interpret`; receiver-qualified search for `details.OfferId` returns nothing |
| 2 | OfferId closure must reject **missing** as well as mismatched | **Confirmed — prior recommendation was wrong** | see §1 below |
| 3 | SettlementOnly lacks party/category — S1 blocker | **Confirmed** (already in the prior report) | DOMAIN/03 §13; no such member anywhere |
| 4 | `SettlementCategory` must **not** become a new AeroTech enum | **Confirmed — prior recommendation was wrong** | see §2 below |
| 5 | Seat correctly deferred | **Confirmed**, unchanged | DOMAIN/02 typed-details table; SLICES/S6 |
| 6 | Gap A — immutable `SalesContext` / `BuyerSnapshot` / seller history missing | **Confirmed, and worse than the reviewer stated in one way, better in another** | see §3 |
| 7 | Gap B — commercial lifecycle vocabulary mismatch | **Confirmed exactly** | see §4 |
| 8 | Gap C — `FulfillmentProfileSnapshot` structurally incomplete | **Confirmed** | see §5 |
| 9 | Gap D — `FundingObligation` partially shaped; prior FUTURE-READINESS wrong | **Confirmed** | see §6 |
| 10 | Gap E — persisted current component totals missing | **Confirmed** | see §7 |
| 11 | `CandidateService` dictionary — simplification, canonical JSON must not change | **Confirmed; prior finding was right but under-specified** | see §8 |
| 12 | `OD-C-04` has a recorded contradiction | **Confirmed — the prior audit missed it entirely** | see §9 |
| 13 | Public AIDM is more detailed than the prior report assumed | **Confirmed** | see §10 |
| 14 | `FIX_DOMAIN_DESIGN = 0` and "only two small gaps" not strong enough | **Confirmed — the prior executive conclusion was wrong** | see §11 |
| 15 | Public pricing exposure is not a domain blocker | **Confirmed**, and it is now explicitly separated from S1 approval | `OD-CLOSE-04` |

---

## 1. OfferId — the prior recommendation was wrong

The prior `OD-CLOSE-02` recommended tolerating a missing `data.offerId` "because the observed contract types it `string?`". That reasoning used the **C# mirror class** as contract authority. It is not.

`CONTRACTS/02-AIROFFER.md` line 13 lists the root fields as:

> `OfferId, PricedAt, LastTicketingDate?, CurrencyId, CurrencyCode, JourneyType, BaseAmount, ChargeAmount, TotalAmount, AirTransports[], PricingUnits[], Tickets[], OrderCharges[], RatesOfExchange[]`

`LastTicketingDate?` is the **only** root field the Pack marks optional. `OfferId` carries no `?`. The Pack is authority; `AirOfferDetailsWire.OfferId` being declared `string?` is an implementation detail of our own mirror.

**Corrected closure target:** null or empty → `ContractMismatch`; non-equal (ordinal) → `ContractMismatch`; exact ordinal match → proceed. No trim, no case-fold, no alias. Negative tests for **both** cases.

## 2. Settlement attribution — the prior recommendation was wrong

The prior `OD-CLOSE-01` recommended making `SettlementCategory` "a new `Ordering/Enums` enum, not a free string, so an unknown category fails closed". That is inventing a closed vocabulary the owner has never defined, which is exactly what `GOVERNANCE/05` and `CLAUDE.md` forbid.

Public IATA AIDM Commission settles it independently:

> "A remuneration either an amount of money, or a set percentage of the value involved, paid to an agent in relations to a commercial transaction."

with attributes **Amount**, **Code**, **Commission Code**, **Percentage Applied To Amount**, **Percentage Percent**, **Remark Text**, **Taxable Indicator** — each cardinality 0..1. IATA itself models the category as a **code**, not a closed enum.

**Corrected shape:** `SettlementAttribution(PartyRef, CategoryCode)`, both source-owned strings, nullable on `PricingLine`, **required iff `Effect == SettlementOnly`**, enforced in the domain and by a SQL constraint that prevents half-population. Currency stays on the line's `Money`. No Commission aggregate, no calculation, no BSP engine, and no assumption that the settlement party equals the seller.

AIDM also shows that percentage and percentage-applied-to-amount are legitimate **future** source facts. `PricingCalculationKind` is the right extension seam, but `OD-P-18` remains binding: AirOffer's `Amount` is never interpreted as a percentage value.

## 3. Gap A — SalesContext and BuyerSnapshot

`DOMAIN/01` §2 verbatim:

> Required at accepted creation: `OrderId`, `OrderReference`, `RootOrderId` (self), `OwnerAirlineId`, `FinancialCustomerId`, **immutable `SalesContext`**, **`BuyerSnapshot`**, sale currency reference, `CreatedAt`, accepted-source links …

`DOMAIN/04` §5 verbatim:

> Buyer, financial Customer, Traveler, payer, agency/seller and actor are separate references. … **SalesContext and accepted traveler identity snapshots remain historical.**

Current `Order` has `FinancialCustomerId`, `Channel`, `SellingOfficeId`, `BuyerActorContextType`, `BuyerActorId`. There is no named `SalesContext` and no `BuyerSnapshot`.

**Where the reviewer is right.** `BuyerActorId` is the *initiating actor*, not a buyer. `AuthorizedScopeResolver` proves it per surface: `AirlineUserId` on Backoffice, `TravelAgencyUserId` on OtaPanel, `PartnerApiAccessProfileId` on Ota, `ActorId` on Service. Calling that field a Buyer is a naming error with real consequences.

**Where the reviewer is slightly too strong.** `TravelAgencyId` is not completely absent from durable storage: `CallerScopeKey.ForSale(..., CallerScopeKey.Agency(travelAgencyId))` puts `agency:{id}` inside the `CallerScope` string, and that string is persisted on both `OrderPreparations.CallerScope` and `CommandReceipts.CallerScope`. So the fact survives.

**Why that does not rescue it.** It survives as a *parsed idempotency-scope string*, not as a modelled accepted party. Reconstructing historical seller identity would mean string-parsing an authorization key — which is not a snapshot, is not queryable, is not typed and is not what DOMAIN/01 requires. The reviewer's conclusion stands.

**One further defect the reviewer did not name.** `Order.SellingOfficeId` is a **single column with two different namespaces**: on Backoffice it holds `_caller.AirlineOfficeId`, on OtaPanel it holds `_caller.TravelAgencyOfficeId`. Two office identity spaces in one untyped `long`. Any later query that joins office to an airline office master will silently mis-join agency offices.

**Independent corroboration.** IATA AIDM Distribution Chain Role Code defines exactly three roles — Carrier ("carries the passenger…"), Distributor ("provides a distribution capability such as a certain type of Consolidator, an Aggregator, more generally an intermediary"), Seller ("offers a shopping capability to a shopper") — and states these are "roles within the distribution chain mechanism itself, not an entity's primary business classification". Our model conflates seller, distributor and actor into one `BuyerActor*` pair.

**Disposition: `FIX_DOMAIN_DESIGN`, S1 accepted-sale provenance blocker.** A new owner decision (`OD-CLOSE-05`) is required for the exact Buyer vs Seller mapping per surface, because the caller inputs differ per surface and no owner decision defines the mapping.

## 4. Gap B — commercial lifecycle vocabulary

`DOMAIN/02` §48 verbatim:

> Service states: Pending (accepted but contractually awaiting commercial activation), Active, Cancelled, **Replaced**, **Expired**.

§52 verbatim:

> Item status is derived … all active -> Active; mixed active and terminal -> **PartiallyChanged**; all cancelled -> Cancelled; explicit predecessor -> Replaced; split predecessor -> **Partitioned**; all expired -> **Expired**; other fully inactive mix -> **Inactive**. … **Fulfillment/delivery facets remain separate.**

Actual enums:

| Enum | Current members | Pack requires | Verdict |
|---|---|---|---|
| `OrderServiceCommercialStatus` | `Pending=1, Active=2, Cancelled=3, Exchanged=4, Suspended=5` | Pending, Active, Cancelled, **Replaced**, **Expired** | `Exchanged` is an operation outcome, not a canonical state; `Suspended` has no commercial home — it belongs to the delivery axis, which already exists as `OrderServiceDeliveryStatus`; `Replaced` and `Expired` are missing |
| `OrderItemCommercialStatus` | `Active=1, Replaced=2, Cancelled=3` | Active, PartiallyChanged, Cancelled, Replaced, Partitioned, Expired, Inactive | four members missing |

Independent corroboration: IATA AIDM `Service` carries **`Status Code`** and **`Delivery Status Code`** as separate attributes — commercial state and delivery state are two axes in the public model too.

**Migration safety is favourable.** Search of `src` and `tests` shows `OrderServiceCommercialStatus.Exchanged` and `.Suspended` are referenced **nowhere**; the only value ever written is `Active` (2). So the correction can rename value 4 and add the missing members while a migration guard fails loudly if any row holds an unexpected value.

**Disposition: `FIX_DOMAIN_DESIGN` (vocabulary now, behavior deferred).** No owner decision was found overriding the Pack here.

## 5. Gap C — FulfillmentProfileSnapshot

`DOMAIN/02` §23 verbatim requires: "profile ID/version, reservation requirement, resource quantity/unit policy, document requirement/type/authority, funding requirement, delivery provider/control policy, dependency treatment and whether partial fulfillment is supported."

Current record: `ProfileRef, ProfileVersion, Assurance, ReservationRequirement, DocumentKind, FundingRequirement, CapacityUnits`.

| Pack semantic | Present? |
|---|---|
| profile ID / version | yes |
| reservation requirement | yes |
| resource quantity / unit **policy** | partial — `CapacityUnits` is a quantity, not a unit policy |
| document requirement / type / **authority** | partial — `DocumentKind` only; no authority |
| funding requirement | yes |
| delivery provider / control policy | **no** |
| dependency treatment | **no** |
| partial-fulfillment support | **no** |

The prior audit rated this `BLOCKED_OWNER_CONTRACT` and stopped. That confused two things: the **values** are owner-blocked (BD-002/005/006), but the **shape** is a Pack requirement that nothing blocks. Stage 05 correctly stopped the live profile from claiming FlightFlow/ETKT/funding authority; that does not make the snapshot structurally complete.

**Disposition: `DOMAIN_SHAPE_GAP`** — shape must be closed before S2/S4/S6; values stay `Unresolved`/`NotCertified`; no fulfillment engine now. Where Pack wording does not define an exact vocabulary, `OD-CLOSE-07` asks rather than inventing an enum.

## 6. Gap D — FundingObligation

`DOMAIN/06` verbatim:

> `FundingObligation` fields: ObligationId, OrderId, Version, Purpose, sale currency, exact amount, **Service/Item/PricingLine scope**, accepted ChangeId, superseded obligation ref, source pricing decision and **current disposition**.

Current entity: `Id, OrderId, Version, Purpose, Amount, OrderItemId?, ChangeId, SourceDecisionRef, SupersededObligationId`.

Missing: **Service scope**, **PricingLine scope**, **current disposition**. (`FundingObligationPurpose` is complete: OriginalSale, AddedService, ExchangeAdditionalCollection, Fee, RefundDisposition.) The Pack also names `Order.ObligationRevision` as a discovery watermark; it does not exist.

The prior FUTURE-READINESS said the obligation was "fully sufficient for S3". **That was wrong.** A fee-only `MonetaryCharge` item needs PricingLine scope and an S6 added service needs Service scope; today item-only scope would harden into an invariant.

**Disposition: `DOMAIN_SHAPE_GAP`.** Design the scope shape now so item-only does not become permanent; no payment behavior; `OD-CLOSE-08` only if the disposition vocabulary is genuinely undefined.

## 7. Gap E — persisted current component totals

`DOMAIN/01` §2 verbatim: "Derived-but-persisted: CommercialSummary, CustomerTotal **and complete current component totals**."

Current `Order` persists `CommercialSummary` and `CustomerTotal`. There is no component-total concept anywhere.

Independent corroboration: IATA AIDM `Price` carries **Base Amount**, **Total Amount** and **Equivalent Amount**, and associates to **Fee, Markup, Tax Summary, Discount, Surcharge, Currency Conversion**, with an association to **Order (Total Price role)** and to **Order Item**. A component summary beside a grand total is the industry shape, not an AeroTech invention.

This is not an information-loss emergency — every canonical line survives and the totals are derivable — but it is a Pack conformance gap, and the prior audit did not raise it at all.

**Disposition: `DOMAIN_SHAPE_GAP`**, with `OD-CLOSE-06` for the semantics (customer-effective only vs all effects; keying; settlement handling; currency; canonical state vs deterministic persisted summary). No JSON dictionary, no twenty speculative columns.

## 8. Candidate detail dictionary — refined

The prior finding was right but proposed changing `CandidateService` in a way that would have altered the canonical candidate JSON and therefore every accepted digest.

**Corrected direction:** the serialized canonical contract keeps `detailSchema`, `detailSchemaVersion` and the `details` object exactly as it is today — same property names, same ordinal key order, same digest. Only the **in-memory** model becomes a typed `CandidateAirTransportDetail`, with the serializer writing the identical `details` shape from typed members and the reader populating them. Digest compatibility is asserted by a test that re-reads the existing pack example and reproduces the recorded digest unchanged.

If that proves materially awkward, it stays a post-closure simplification. It is not a domain gap either way.

## 9. OD-C-04 — a contradiction the prior audit missed

| Source | States |
|---|---|
| `S1-API-READABILITY-OWNER-DECISION-2026-09-18.md` line 60 | "Pack QRY-002 `GetOperation` is no longer a sales-surface route; **it remains reachable on the internal surface only**." |
| `S1-CLEANUP-OPEN-DECISIONS.md` `OD-C-04` | "I deleted the query, its read model and the routes **entirely**." |
| Actual code | search for `GetOperation` / `operations/` across `src`: **no match**. Nothing exists on any surface. |

The implementation followed the cleanup report, not the owner decision. The owner decision says internal-only; the code has nothing. This must be reconciled before S1 closes — either the owner confirms full deletion, or QRY-002 is restored on `Internal/v1`.

## 10. Benchmark sources — the prior report understated IATA

The prior report marked IATA entity definitions `NOT PUBLICLY PROVEN` because the Implementation Guides sit behind the developer portal. That was true of the guides and **false of AIDM**, which publishes entity pages openly. Five AIDM pages were retrieved in this run and are now cited at HIGH confidence: Order Item, Service, Commission, Distribution Chain Role Code, Price. Public benchmark source count rises from **11** to **17**.

The most consequential retrievals:

- **Order Item** — "An individually priced item within an Order, made up of one or more Services." Associations include **Commission** ("may be present"), Price, Change/Cancel Restrictions, Penalty. Attributes include Grand Total Amount and four separate time limits.
- **Service** — "At time of order, the services should be applied to a single passenger on a single segment", and "At time of Order Creation an Offered Service can become multiple services within the Order Item as the service is broken down per segment and passenger." This is verbatim corroboration of INV-009 + INV-010 and of exactly what the AirOffer mapper does.
- **Commission**, **Distribution Chain Role Code**, **Price** — as quoted in §2, §3 and §7 above.
- **IATA Reference Business Architecture** — names "Customer Order Accounting" and "Partners/Suppliers Order Accounting" as *separate* financial-management capabilities, corroborating the customer-payable vs settlement split at HIGH confidence.

## 11. The prior executive conclusion was wrong

"Only two small gaps" and `FIX_DOMAIN_DESIGN = 0` cannot stand. The corrected position:

| Category | Count | Items |
|---|---|---|
| S1 blockers | **4** | OfferId fail-closed; settlement attribution; accepted SalesContext/Buyer/Seller snapshot; commercial lifecycle vocabulary |
| Domain-shape debts to decide now, implement later | **3** | FulfillmentProfileSnapshot completeness; FundingObligation scope/disposition; component totals |
| Governance | **1** | eight unanswered cleanup decisions, including the OD-C-04 contradiction |
| Maintainability | **2** | candidate detail dictionary; candidate-member drift guard |

`FIX_DOMAIN_DESIGN` is **2** (SalesContext/Buyer/Seller snapshot; lifecycle vocabulary), not 0.

## What the prior audit got right and keeps

The two defects it did find are real and remain blockers. Its wire-field enumeration (105 fields, receiver-qualified) stands unchanged except for the OfferId closure target. Its Seat analysis stands. Its rejection of an ETKT coupon entity, of route-shape fare inference and of a generic Order TTL stands. Its finding that the S1 path is not over-abstracted stands.

Its method failure was narrow and specific: it audited the concepts it went looking for, and did not walk `DOMAIN/01` §2 and `DOMAIN/02` §48–52 field by field against the code. Four of the five new gaps are single sentences in those two sections.
