# Open Decisions — S1 Create-Order Conformance

Stage: 06-S1-create-order-conformance · **revision 2** · 2026-09-19 · HEAD `506ccee`

Revision 1 raised four decisions and got two of the recommendations wrong. Both are corrected here. Four more are added for the gaps revision 1 never audited. Decisions already settled by the Pack or by an existing owner answer are listed at the end and need no owner time.

**Eight** decisions. Two recommendations changed; four are new.

---

## OD-CLOSE-01 — Settlement attribution on a pricing line *(recommendation corrected)*

**Problem:** `DOMAIN/03` §13 — "SettlementOnly requires party/category/currency and is excluded from customer payable totals." No party and no category exist in any layer: not `PricingLine`, not `CandidatePricingLine`, not SQL, not the projection, not `PricingLineMatrix`. A settlement line can be accepted today with neither.

**Evidence:** `PricingLine.cs`, `CandidatePricingLine.cs`, `PricingLineMatrix.cs`, `PricingLineConfiguration.cs`. Historical `Ordering/k8s-stg` `OrderPricingLine` had `SettlementPartyRef` and `SettlementCategory`. No test at any layer persists a settlement commission line, and there is no `SC_S1_013` test.

**Industry benchmark:** IATA AIDM Commission — "A remuneration either an amount of money, or a set percentage of the value involved, **paid to an agent** in relations to a commercial transaction", with attributes Amount, **Code**, **Commission Code**, Percentage Applied To Amount, Percentage Percent, Remark Text, Taxable Indicator, all 0..1. IATA models the category as a **code**, not a closed vocabulary. IATA Settlement with Orders names the counterparties as "Airlines and Sellers (Agents, OTAs, TMCs, etc.)". The Reference Business Architecture separates "Customer Order Accounting" from "Partners/Suppliers Order Accounting".

**Pack / owner rule:** `DOMAIN/03` §13; INV-013; SC-S1-013; `S1-API-READABILITY-OWNER-DECISION-2026-09-18` line 64 — "the settlement/commission half of SC-S1-013 stays proven."

**Revision 1's recommendation was wrong.** It proposed making `SettlementCategory` "a new `Ordering/Enums` enum … so an unknown category fails closed". That invents a closed vocabulary the owner has never defined, which `GOVERNANCE/05` and `CLAUDE.md` both forbid, and AIDM shows the industry treats it as a code.

**Options:**
- **(a)** `SettlementAttribution(PartyRef, CategoryCode)` — a nullable owned value on `PricingLine`, both source-owned strings, **required iff `Effect == SettlementOnly`**, with a SQL constraint that prevents half-population.
- **(b)** Two loose nullable columns on the line with the same conditional rule.
- **(c)** Defer the whole settlement half and record SC-S1-013 as fully deferred, not half-proven.
- **(d)** Leave it.

**Recommendation: (a).** It matches how every other multi-field accepted fact is modelled here (`AppliedConversion`, `BaggageAllowance`, `ProductSnapshot`), makes presence a single decision rather than two columns that can disagree, and extends to partner/interline settlement, which needs the same counterparty concept. Currency stays on the line's `Money`. Both members are source/contract facts — **no new AeroTech enum**. No Commission aggregate, no calculation, no BSP engine, and no assumption that the settlement party equals the seller. S1 certifies an **amount-based** commission end to end; AIDM's percentage and percentage-applied-to-amount are legitimate future source facts for which `PricingCalculationKind` is already the seam, and `OD-P-18` stays binding — AirOffer's `Amount` is never read as a percentage value.

**Consequences:** (a) one value object, one migration, one rule, one SQL check, one scenario test. (b) two columns that can drift apart, and a third settlement attribute later means a third loose column. (c) contradicts a decision the owner already made in writing and leaves INV-013 half-proven at closure. (d) leaves the codebase able to persist a Pack-violating record.

**Answer:**

---

## OD-CLOSE-02 — Fail-closed check on `data.offerId` *(recommendation corrected)*

**Problem:** `AirOfferSourceAdapter` never compares the response `offerId` to the requested one. A 200 for a different offer is accepted, and the Order records the **requested** id while carrying the other offer's money, journeys, services and fare construction.

**Evidence:** `AirOfferSourceAdapter.Interpret` → `AirOfferCandidateMapper.Map(request.OfferId, envelope.Data, …)`; receiver-qualified search for `details.OfferId` returns nothing; the mapper writes `requestedOfferId` into `CandidateSource.OfferId` and `ProductSnapshot.SourceOfferId`.

**Pack / owner rule:** INV-006 — "no external calls or **silent repricing**". `CONTRACTS/02-AIROFFER.md` line 13 lists the root fields as `OfferId, PricedAt, LastTicketingDate?, CurrencyId, CurrencyCode, JourneyType, …` — **`LastTicketingDate?` is the only root field marked optional; `OfferId` is not.**

**Revision 1's recommendation was wrong.** It tolerated a missing `data.offerId` "because the observed contract types it `string?`", using our own C# mirror class as contract authority. The Pack is authority; the mirror's nullability is an implementation detail.

**Options:**
- **(a)** Strict on both: null or empty → `ContractMismatch`; non-equal (ordinal) → `ContractMismatch`; exact match → proceed.
- **(b)** Strict only on mismatch, tolerate missing *(revision 1's position)*.
- **(c)** Warn only, record in evidence, accept.
- **(d)** Leave it.

**Recommendation: (a).** One comparison at the top of `Map`, inside a method that already throws `ContractMismatch` six times, so the adapter's failure path handles it and nothing is persisted. No trim, no case-fold, no alias, no normalisation. Negative tests for **both** cases.

**Consequences:** (a) closes the defect completely and matches the Pack's own field list. (b) accepts an Order whose provenance rests on nothing the owner returned. (c) leaves a wrong accepted record with a note beside it. (d) unacceptable.

**Answer:**

---

## OD-CLOSE-03 — Reconcile the stage-04 cleanup decisions *(reframed — do not blanket-answer)*

**Problem:** eight `Answer:` lines in `S1-CLEANUP-OPEN-DECISIONS.md` are empty. Revision 1 recommended answering all eight. That is wrong in method: several are already substantively ratified by later owner decisions, and one contains a real contradiction that a blanket answer would bury.

**Reconciliation table:**

| Decision | Already ratified elsewhere? | Exact authority | Current code | True remaining decision? | Recommended answer |
|---|---|---|---|---|---|
| `OD-C-03` | no | — | (as the entry describes) | yes — minor | confirm as built |
| **`OD-C-04`** public `/operations/{id}` and `GetOperation` deleted | **contradicted** | `S1-API-READABILITY-OWNER-DECISION` line 60: "Pack QRY-002 `GetOperation` … **remains reachable on the internal surface only**" | search for `GetOperation` / `operations/` across `src`: **no match** — nothing exists on any surface | **YES — a genuine contradiction** | either confirm full deletion (and amend the earlier decision), or restore QRY-002 on `Internal/v1` as that decision states |
| `OD-C-05` allocation and reversal removed | **yes, substantively** | `S1-API-READABILITY-OWNER-DECISION` line 63 defers SC-S1-014 and the reversal half of SC-S1-013 | as described | no | record as ratified by that decision; note INV-014 is an S1-introduced invariant now unproven |
| `OD-C-06` Prepare merged into Create | **yes, substantively** | the same decision's one-call Create | as described | no | record as ratified |
| `OD-C-07` small Create response | **yes, substantively** | the same decision's response shape | as described | no | record as ratified |
| `OD-C-08` | no | — | as described | yes — minor | confirm as built |
| **`OD-C-09`** candidate detail dictionary | partly | — | `details` dictionary still carries three typed concepts | **YES, but split it** | the *serialized canonical* `details` shape and digest stay unchanged; the *in-memory* model may become typed. Answer those two halves separately. |
| `OD-C-10` | no | — | as described | yes — minor | confirm as built |

**Recommendation:** answer `OD-C-04` and `OD-C-09` deliberately; mark `OD-C-05`, `OD-C-06` and `OD-C-07` as ratified-by-reference with the exact citation; confirm the remaining three as built. Do **not** mark anything closed that no owner decision actually covers — `BLOCKED-DECISIONS.md` says "Empty Answer lines … remain unanswered."

**Answer:**

---

## OD-CLOSE-04 — Public exposure of pricing code and name *(explicitly separated from S1 approval)*

**Problem:** `PricingLine.SourceCode` / `.SourceName` / `.SourceReference` are preserved and are in the internal projection; the public `OrderPriceLineDto` exposes only the component enum and the amounts.

**This is not a domain blocker and must not gate S1 domain approval.**

**Recommendation:** keep all three preserved internally. Add `code` and `name` to `OrderPriceLineDto` only if the owner wants richer `GET` usability now — it is additive and backward compatible. **Do not** expose `SourceReference` by default: it is an opaque owner handle, not a display field.

**Answer:**

---

## OD-CLOSE-05 — Accepted Buyer / Seller / SalesContext snapshot *(new)*

**Problem:** `DOMAIN/01` §2 requires an immutable `SalesContext` and a `BuyerSnapshot` **at accepted creation**. Neither exists. `DOMAIN/04` §5 says "Buyer, financial Customer, Traveler, payer, agency/seller and actor are separate references" and "SalesContext … remain historical". Today:

- `BuyerActorContextType` / `BuyerActorId` hold the **initiating actor**, not a buyer.
- `TravelAgencyId` is resolved to a `FinancialCustomerId` and then dropped; it survives only inside the `agency:{id}` fragment of the `CallerScope` **string** on `OrderPreparations` and `CommandReceipts` — recoverable by parsing an idempotency key, which is not a snapshot.
- `SellingOfficeId` is one untyped `long` holding **two namespaces**: airline office on Backoffice, travel-agency office on OtaPanel.

**Evidence:** `AuthorizedScopeResolver.cs`, `CallerScopeKey.cs`, `Order.cs`.

**Industry benchmark:** AIDM Distribution Chain Role Code defines exactly three roles — **Carrier** ("carries the passenger, baggage, or goods"), **Distributor** ("a certain type of Consolidator, an Aggregator, more generally an intermediary"), **Seller** ("offers a shopping capability to a shopper") — and states these are roles "within the distribution chain mechanism itself, not an entity's primary business classification".

**What each surface actually has today:**

| Surface | Financial customer | Buyer fact available? | Seller org fact available? | Seller office | Distributor fact available? | Actor | Authority / source | Can snapshot without inference? |
|---|---|---|---|---|---|---|---|---|
| Backoffice | request parameter, checked active | **no** | the owner airline itself (implicit) | `_caller.AirlineOfficeId` — airline namespace | no | `_caller.AirlineUserId` | token + request | yes, if "seller = owner airline" is ratified |
| OtaPanel | derived from `TravelAgencyId` via `ICustomerDirectory` | **no** | **yes — `_caller.TravelAgencyId`, currently discarded** | `_caller.TravelAgencyOfficeId` — agency namespace | no | `_caller.TravelAgencyUserId` | token | yes, once the agency id is persisted |
| Ota (PartnerAPI) | `_caller.CustomerId` | **no** | not distinguishable from the customer | `_caller.TravelAgencyOfficeId` (nullable) | `_caller.PartnerApiAccessProfileId` — currently stored as `BuyerActorId` | same profile id | token | partly |
| Service | request parameter | **no** | **no** | request parameter (nullable) | no | `_caller.ActorId` when authenticated | request | no |

**Decision required:**
1. Exact role definitions for this deployment, mapped to Carrier / Distributor / Seller.
2. The authoritative source of each role **per surface**.
3. The minimum immutable snapshot to persist at acceptance.
4. `NotSupplied` semantics — an explicit not-supplied state is preferable to a guessed identity, but only after the snapshot semantics are defined.
5. Whether `SellingOfficeId` is split by namespace or carries an explicit office-kind.

**Recommendation:** define a small immutable `SalesContext` owned value on `Order` carrying channel, seller organisation reference **with its kind**, seller office reference **with its kind**, distributor reference where a surface supplies one, and the initiating actor renamed away from "Buyer". Persist `TravelAgencyId` as the seller organisation on OtaPanel. Leave `Buyer` explicitly `NotSupplied` until a surface actually supplies a buyer identity — no surface does today. **Do not** build a distribution-chain engine; there is no chain input.

**Consequences of not deciding:** historical seller identity stays dependent on mutable ReferenceData or on parsing an authorization key; a later office join silently mis-joins two namespaces; and S14 split and interline settlement inherit the same hole.

**Answer:**

---

## OD-CLOSE-06 — Component totals semantics *(new)*

**Problem:** `DOMAIN/01` §2 — "Derived-but-persisted: CommercialSummary, CustomerTotal **and complete current component totals**." Only the first two exist.

**Industry benchmark:** AIDM `Price` carries **Base Amount**, **Total Amount** and **Equivalent Amount**, and associates to **Fee, Markup, Tax Summary, Discount, Surcharge, Currency Conversion**, at both **Order (Total Price role)** and **Order Item** level. A component summary beside a grand total is the industry shape.

**This is not an information-loss emergency** — every canonical line survives and the totals are derivable. It is a Pack conformance gap.

**Decision required:**
1. Are the totals **customer-effective only**, or do they cover all effects?
2. Is a settlement commission a component total while staying outside `CustomerTotal`?
3. Are totals keyed by **component**, or by component **and effect**?
4. Which currency — sale currency only, or also original?
5. Are they **canonical domain state** or a **deterministic persisted read summary** rebuilt with the projection?
6. Order level only, or Order **and** OrderItem (AIDM has both)?

**Options for shape:**
- **(a)** A child table `OrderComponentTotals(OrderId, Component, Effect, Amount, CurrencyRef)` — relational, queryable, no speculative columns, extends to new components for free.
- **(b)** A fixed owned value with one column per component family — fewer joins, but freezes the taxonomy and adds eleven columns for a vocabulary that already has eleven members.
- **(c)** Compute on read only, and treat the Pack line as satisfied by the lines themselves.

**Recommendation: (a), keyed by component + effect, sale currency, and classified as a deterministic persisted summary rebuilt with the projection** — so it can never disagree with the lines. Explicitly **no** JSON dictionary and **no** twenty speculative columns. Order level first; item level when a slice needs it. But questions 1–6 are the owner's to settle, and the Pack wording alone does not settle 1, 2 or 5.

**Answer:**

---

## OD-CLOSE-07 — FulfillmentProfileSnapshot target shape *(new)*

**Problem:** `DOMAIN/02` §23 lists eight semantics; the current record covers four and a half.

| Pack semantic | Present? | Vocabulary defined in Pack? | Current source supplies? | Reference profile can supply? | Needed before |
|---|---|---|---|---|---|
| profile ID / version | yes | n/a | yes | yes | — |
| reservation requirement | yes | yes (`ReservationRequirement`) | no (`Unresolved`) | yes | S2 |
| resource quantity / **unit policy** | partial — `CapacityUnits` is a quantity, not a policy | **no** | no | yes | **S2** |
| document requirement / type / **authority** | partial — kind only | type yes; **authority no** | no | yes | **S4** |
| funding requirement | yes (`FundingRequirement`) | yes | no (`Unresolved`) | yes | S3 |
| **delivery provider / control policy** | **no** | **no** | no | partly | S12 |
| **dependency treatment** | **no** | **no** | no | yes | **S6** |
| **partial-fulfillment support** | **no** | not as a vocabulary — it is a boolean in Pack wording | no | yes | **S6** |

**Decision required:** for the four semantics whose vocabulary the Pack does **not** define — unit policy, document authority, delivery/control policy, dependency treatment — either the owner supplies the vocabulary, or the snapshot carries a source-owned code the way `SettlementAttribution.CategoryCode` will.

**Recommendation:** decide the **shape** now (fields exist, values stay `Unresolved`/`NotCertified` for the live profile) so S2/S4/S6 never migrate accepted snapshots. Add `PartialFulfillmentSupported` as a nullable boolean now — Pack wording defines it without needing a vocabulary. For the other three, prefer a source-owned code over an invented enum. **No fulfillment engine, no rule framework.**

**Answer:**

---

## OD-CLOSE-08 — FundingObligation scope and disposition *(new)*

**Problem:** `DOMAIN/06` — "`FundingObligation` fields: ObligationId, OrderId, Version, Purpose, sale currency, exact amount, **Service/Item/PricingLine scope**, accepted ChangeId, superseded obligation ref, source pricing decision and **current disposition**."

Current: item scope only (`OrderItemId?`), no disposition. `Order.ObligationRevision` — the Pack's discovery watermark — does not exist either. Revision 1 wrongly called this "fully sufficient for S3".

**Decision required:**
1. Scope shape — one nullable column per scope kind, or a scope-kind discriminator plus one reference?
2. Does the S1 original-sale obligation stay **item**-scoped? (It should — that is what it is.)
3. Is the **disposition** vocabulary defined by the Pack? `DOMAIN/06` names behaviours (supersede, release, rebind) but never enumerates the states. If it does not, the owner must supply them — do **not** invent a payment status.
4. Is `Order.ObligationRevision` needed now, or with S3?

**Recommendation:** design the scope shape now so item-only does not harden into an invariant — a scope-kind discriminator plus one reference is smaller than three nullable FKs and reads better. Keep the S1 obligation item-scoped. Defer `ObligationRevision` to S3. **Raise the disposition vocabulary to the owner**; nothing in the Pack enumerates it, and inventing `Open/Settled/Released` would be exactly the guessing this process forbids. No payment behavior now.

**Answer:**

---

## Explicitly **not** raised as decisions

| Considered | Why no decision is needed |
|---|---|
| Seat number on `AirTransportDetail` | `DOMAIN/02`'s typed-details table gives Seat its own row and S6 its slice; corroborated by AIDM M2 and Navitaire N3 |
| An ETKT coupon entity for `AirOfferCouponWire.CouponId` | the Pack calls these a priced projection; identity cannot be lost (raw evidence) or collide (fails closed) |
| `Stop`, bound `direction`, root `journeyType` vocabularies | already open as `OD-P-12` and handoff `OR-002` |
| `PricingLine` quantity / UoM / unit price | rejected by the owner in `OD-P-17`; no source supplies it |
| `CommercialSource` on `OrderItem` | already carried at item granularity by `ProductSnapshot.SourceSystem` |
| Rate-of-exchange from/to pair vs the line's currencies | evidence preserved, nothing interprets it; the question is inside `OR-002` |
| Commercial lifecycle vocabulary | **not a decision** — `DOMAIN/02` §48 and §52 enumerate the states exactly, and no owner decision overrides them. It is an implementation correction, planned in `IMPLEMENTATION-PLAN.md` §A4. |
| Percentage commission numeric fields | AIDM proves they are legitimate future source facts; AirOffer supplies none, so there is nothing to decide until an owner does |
