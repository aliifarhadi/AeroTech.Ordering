# Open Decisions — S1 Create-Order Conformance

Stage: 06-S1-create-order-conformance · 2026-09-19 · HEAD `e861711`

A decision is raised here only where owner-contract semantics are genuinely unknown, an external benchmark materially disagrees with a recorded choice, two correct canonical homes exist with different future cost, or public-contract compatibility needs an explicit choice. Everything else was decided in the matrix and needs no owner time.

**Four** decisions are raised. Two are consequences of defects found in this audit; two are pre-existing reports that need an explicit closing answer.

---

## OD-CLOSE-01 — Where the settlement party and category live

**Problem:**
DOMAIN/03 §13 states "SettlementOnly requires party/category/currency and is excluded from customer payable totals." `PricingLine` carries `Effect` and `Component`, so a `Commission` + `SettlementOnly` line is *structurally* accepted, but there is no `SettlementPartyRef` and no `SettlementCategory` anywhere in the Domain, the candidate, SQL, the projection or the validator. A settlement line can therefore be accepted today with no counterparty and no category, which the Pack forbids.

**Evidence:**
- `src/AeroTech.Ordering.Domain/OrderAggregate/Entities/PricingLine.cs` — no settlement members.
- `src/AeroTech.Ordering.Domain/OrderPreparationAggregate/ValueObjects/CandidatePricingLine.cs` — none.
- `src/AeroTech.Ordering.Domain/OrderAggregate/Policies/PricingLineMatrix.cs` — enforces `Commission ≠ CustomerBalance` and `Tax ≠ SettlementOnly`, but nothing about party or category.
- `PricingLineConfiguration.cs` — SQL checks mirror only those two rules.
- Historical `Ordering/k8s-stg` `OrderPricingLine` had `SettlementPartyRef` and `SettlementCategory`.
- No test at any layer persists a settlement commission line; `PricingArithmeticTests` exercises the arithmetic on plain `PricedAmount` arrays only.

**Industry benchmark:**
IATA Settlement with Orders describes settlement as running "between Airlines and Sellers (Agents, OTAs, TMCs, etc.), using the current BSP agency program framework" — an exchange with a **named counterparty**, distinct from what the passenger pays. How commission relates to the customer-payable amount is `NOT PUBLICLY PROVEN` on the open pages, so the benchmark supports "a settlement line needs a party", not a specific field shape.

**Pack / owner rule:**
DOMAIN/03 §13 (party/category/currency required); INV-013 (settlement commission is not customer debt); SC-S1-013; and `S1-API-READABILITY-OWNER-DECISION-2026-09-18` line 64: "the settlement/commission half of SC-S1-013 stays proven."

**Current code:** the "not customer debt" half is enforced in the matrix and in SQL. The "requires party/category" half is neither representable nor enforced.

**Options:**

- **(a) Two nullable members on `PricingLine` plus a conditional rule.** Add `SettlementPartyRef` and `SettlementCategory` to `CandidatePricingLine` and `PricingLine`; require both whenever `Effect == SettlementOnly` in `PricingLineMatrix` and mirror it as a SQL check; add the missing `SC_S1_013` scenario test through the reference source.
- **(b) A small owned `SettlementAttribution` value object** (`PartyRef`, `Category`) nullable on the line, required when SettlementOnly.
- **(c) Defer the whole settlement half to the slice that first settles**, and record that SC-S1-013 is fully deferred, not half-proven.
- **(d) Leave it.** A settlement line stays acceptable without a party.

**Recommendation: (b).**
It matches how every other multi-field accepted fact in this codebase is modelled (`AppliedConversion`, `BaggageAllowance`, `ProductSnapshot`), it makes "present or absent" a single decision rather than two independently-nullable columns that can disagree, and the conditional rule then reads as one line in `PricingLineMatrix`. `SettlementCategory` should be a new `Ordering/Enums` enum, not a free string, so an unknown category fails closed like every other vocabulary. Currency is already on the line's `Money`, so nothing extra is needed for it.

**Consequence of each option:**

- (a) works and is slightly less code, but two loose nullable columns can drift apart, and a later third settlement attribute means a third loose column.
- (b) one more value object; one migration; the rule is enforceable in one place; extends cleanly to partner/interline settlement, which needs the same counterparty concept.
- (c) honest and cheap, but it contradicts a decision the owner already made in writing, and it leaves INV-013 only half-proven while S1 is closed.
- (d) leaves the codebase able to persist a Pack-violating record. Not acceptable as a closure state.

**Answer:**

---

## OD-CLOSE-02 — Fail-closed check on `data.offerId`

**Problem:**
`AirOfferSourceAdapter` never compares the `offerId` in the Details response to the one it requested. A 200 response for a *different* offer is accepted, and the accepted Order records the **requested** offer id while carrying the other offer's money, journeys, services and fare construction.

**Evidence:**
- `AirOfferSourceAdapter.Interpret` → `AirOfferCandidateMapper.Map(request.OfferId, envelope.Data, …)`.
- Receiver-qualified search for `details.OfferId` / `Data.OfferId` in the mapper and adapter: **no match** (`AIROFFER-WIRE-LOSS-AUDIT.md` §9).
- `AirOfferCandidateMapper` writes `requestedOfferId` into `CandidateSource.OfferId` and `ProductSnapshot.SourceOfferId`.
- The raw payload is retained, so the mismatch is detectable after the fact — but the accepted record and the public `offerId` are already wrong.

**Industry benchmark:**
Not a vendor question. IATA frames the Offer→Order transition as the Order being the fulfilment of a specific Offer (I2); an Order that names an Offer it did not come from breaks that pairing at its root.

**Pack / owner rule:**
INV-006 — "Create consumes exactly one previously captured, explicitly accepted preparation; **no external calls or silent repricing**." A silently substituted offer is the strongest form of silent repricing. CONTRACTS/02-AIROFFER also records `OfferId` as a root response field, so the owner does return it.

**Current code:** no check.

**Options:**

- **(a) Strict.** Non-empty `data.offerId` must equal `requestedOfferId` ordinally, else `AirOfferContractMismatchException`. A null or empty value is tolerated, because the observed contract types it `string?`.
- **(b) Strict, including null.** A missing `data.offerId` is also a mismatch.
- **(c) Warn only.** Record the discrepancy in evidence and accept.
- **(d) Leave it.**

**Recommendation: (a).**
One ordinal comparison at the top of `Map`, inside a method that already throws `ContractMismatch` six times, so the adapter's existing failure path handles it and nothing is persisted. It invents no alias, equivalence, trimming or case-folding semantics. Tolerating a *missing* id is the conservative reading of a contract that declares the field optional — tightening that is an owner question, not an agent's, which is why (b) is offered separately.

**Consequence of each option:**

- (a) closes the defect with the smallest possible change; a real AirOffer response that omits the id still works.
- (b) also closes it, but a response that legitimately omits an optional field would start failing, and no evidence says AirOffer always populates it.
- (c) leaves a wrong accepted record in the database with a note beside it. Provenance must be right, not annotated.
- (d) unacceptable — the Order claims an offer that never priced it.

**Answer:**

---

## OD-CLOSE-03 — Close the eight unanswered stage-04 cleanup decisions

**Problem:**
`reports/00-decisions/S1-CLEANUP-OPEN-DECISIONS.md` has ten items. Two are answered (`OD-C-01` schema `Order`, `OD-C-01b` schema `Operations`, and `OD-C-02` `SalesChannel` is allowed). **Eight `Answer:` lines are still empty** — `OD-C-03` through `OD-C-10`. Two of them are not cosmetic:

- **`OD-C-05`** records that removing the allocation and reversal policies means "pack scenario SC-S1-014 and the reversal half of SC-S1-013 are no longer proven in S1". `S1-API-READABILITY-OWNER-DECISION-2026-09-18` line 63 does ratify that deferral, so the substance is decided — but the register still reads as open, and INV-014 is an **S1-introduced** invariant that is now unproven.
- **`OD-C-06`** records that `PrepareOrderFromOffer` no longer exists as a command and the preparation happens inside Create, so Pack `CMD-001` survives only as an internal step, and the `/order-preparations` route named in SLICES/S1 is gone.

**Evidence:** `grep -n "^Answer:" reports/00-decisions/S1-CLEANUP-OPEN-DECISIONS.md` returns eight empty lines; `SLICES/S1.md` still lists `/{surface}/v1/order-preparations` and `CMD-001` as required API rail.

**Industry benchmark:** none — this is internal governance.

**Pack / owner rule:** `BLOCKED-DECISIONS.md` states plainly: "Empty Answer lines/recommendations in historical reports remain unanswered."

**Current code:** already built as those entries describe. The code and the register disagree about whether that was approved.

**Options:**
- **(a)** Answer all eight in the register, confirming or reversing each.
- **(b)** Answer only `OD-C-05` and `OD-C-06` now and leave the cosmetic six.
- **(c)** Leave them; treat the stage-04 brief as the approval.

**Recommendation: (a).**
S1 cannot be declared closed while eight decisions that shaped its public contract and its scenario coverage are formally unanswered, and the Pack says so explicitly. It is a writing task, not an engineering one.

**Consequence of each option:**
- (a) the register matches the code; S1 closure has a clean paper trail.
- (b) the two material ones are settled, six small contradictions remain in the record.
- (c) the Pack's own rule is violated, and a later reviewer cannot tell approved design from drift.

**Answer:**

---

## OD-CLOSE-04 — Public exposure of pricing code and name

**Problem:**
`PricingLine.SourceCode` and `.SourceName` are now preserved and are in the internal projection, but the public `OrderPriceLineDto` exposes only the component enum and the amounts. An airline user reading an order sees "Tax — 20.00" where the source said "AT — Airport tax". The previous stage deliberately did not change the ratified contract, and left the choice here.

**Evidence:** `src/AeroTech.Ordering.Query/OrderAggregate/Dto/OrderDto.cs` `OrderPriceLineDto`; `IMPLEMENTATION-PLAN.md` §16 lists this as one of three offered-but-not-applied additive exposures.

**Industry benchmark:** none needed — this is a usability and contract-compatibility question, not a semantic one.

**Pack / owner rule:** `API-CONTRACTS.md` governs the surface; `OD-P-15` states explicitly that "changing the public HTTP response is a separate API-contract choice".

**Current code:** preserved internally, not exposed.

**Options:**
- **(a)** Add `code` and `name` to `OrderPriceLineDto` (additive, backward compatible).
- **(b)** Expose them only on `/backoffice` and `/internal`, not on `/ota` or `/service`.
- **(c)** Leave the contract as it is.

**Recommendation: (a).**
It is additive, every surface already returns the same `OrderDto`, and a tax code is the first thing an airline user looks for. Option (b) would make one canonical DTO behave differently per surface, which is the pattern stage 04 removed.

**Consequence of each option:**
- (a) OpenAPI gains two optional string fields; existing consumers are unaffected; `OpenApiDocumentTests` expectations change once, deliberately.
- (b) reintroduces per-surface DTO variance.
- (c) the data stays invisible to every caller until some later slice needs it.

**Answer:**

---

## Explicitly **not** raised as decisions

These were considered and settled by evidence, so they do not need owner time:

| Considered | Why no decision is needed |
|---|---|
| Seat number on `AirTransportDetail` | DOMAIN/02's typed-details table already assigns seat product and requested seat to the **Seat** type in S6. Settled by the Pack. |
| An ETKT coupon entity for `AirOfferCouponWire.CouponId` | The Pack states these are a priced projection, not issued coupons; identity cannot be lost (raw evidence) or collide (fails closed). Settled in `DOMAIN-BENCHMARK-MATRIX.md` §C4. |
| `Stop`, bound `direction`, root `journeyType` vocabularies | Already an open owner item — `OD-P-12` and handoff `OR-002`. Not duplicated here. |
| `PricingLine` quantity / UoM / unit price | Already rejected by the owner in `OD-P-17`; no source supplies it. |
| `CommercialSource` on `OrderItem` | Already carried at item granularity by `ProductSnapshot.SourceSystem`. |
| The rate-of-exchange from/to pair not matching the line's currencies | Evidence is preserved and nothing interprets it; the interpretation question is already inside `OR-002`. |
