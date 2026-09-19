# Open Decisions — S1 Create-Order Conformance

Stage: 06-S1-create-order-conformance · **revision 3** · 2026-09-19 · HEAD `4e48447`

**Nine** decisions. Everything below a **Recommendation** heading is a recommendation only. Nothing here is an accepted decision until the owner writes on its `Answer:` line. `BLOCKED-DECISIONS.md` governs: "Empty Answer lines … remain unanswered."

Revision 3 changes: `OD-CLOSE-06` and `OD-CLOSE-08` carry corrected shapes; `OD-CLOSE-07`'s document-authority analysis was wrong and is corrected; `OD-CLOSE-01`'s benchmark wording was too strong and is narrowed; `OD-CLOSE-05` gains the public office-contract choice; **`OD-CLOSE-09` is new**.

---

## OD-CLOSE-01 — Settlement attribution on a pricing line *(benchmark wording corrected)*

**Problem:** `DOMAIN/03` §13 — "SettlementOnly requires party/category/currency and is excluded from customer payable totals." No party and no category exist in any layer: not `PricingLine`, not `CandidatePricingLine`, not SQL, not the projection, not `PricingLineMatrix`. A settlement line can be accepted today with neither.

**Evidence:** `PricingLine.cs`, `CandidatePricingLine.cs`, `PricingLineMatrix.cs`, `PricingLineConfiguration.cs`. Historical `Ordering/k8s-stg` `OrderPricingLine` had `SettlementPartyRef` and `SettlementCategory`. No test at any layer persists a settlement line, and there is no `SC_S1_013` test.

**Settlement attribution is not commission-specific.** `DOMAIN/03` §19 gives **Fee / Markup / Penalty** an "explicit source settlement basis" and §20 gives Discount an "explicit settlement discount only". Commission is one component among several that can be `SettlementOnly`.

**Benchmark wording, corrected.** Revision 2 wrote that "AIDM models *the category* as a Code". That claims more than the source supports. What AIDM M3 actually proves is that **Commission** may be an amount or a percentage, that it carries `Code` and `Commission Code` (both 0..1), and that it is remuneration "paid to an agent". It does **not** establish that IATA's Commission Code is the same field as Pack 3.8's generic settlement category. Corrected statement: `CategoryCode` is an **opaque source/contract-owned settlement-category code required by Pack 3.8**; AIDM corroborates that settlement-adjacent remuneration carries codes rather than a closed enum, and is not the semantic authority for AeroTech's generic settlement category.

**Revision 1's recommendation was wrong** and stays withdrawn: it proposed a new `SettlementCategory` enum in `Ordering/Enums`, inventing a closed vocabulary the owner has never defined, which `GOVERNANCE/05` and `CLAUDE.md` both forbid.

**Options:**
- **(a)** `SettlementAttribution(PartyRef, CategoryCode)` — a nullable owned value on `PricingLine`, both source-owned strings, **required iff `Effect == SettlementOnly`**, with a SQL constraint that prevents half-population.
- **(b)** Two loose nullable columns on the line with the same conditional rule.
- **(c)** Defer the whole settlement half and record SC-S1-013 as fully deferred, not half-proven.
- **(d)** Leave it.

**Recommendation: (a).** It matches how every other multi-field accepted fact is modelled here (`AppliedConversion`, `BaggageAllowance`, `ProductSnapshot`), makes presence a single decision rather than two columns that can disagree, and extends to partner/interline settlement, which needs the same counterparty concept. Both members are opaque source/contract strings — **no new AeroTech enum, no interpretation, and no assumption that `PartyRef` is the seller**. The monetary currency of the line is the committed **`SaleValue`** currency; `OriginalValue` remains provenance and is never the settlement currency. No Commission aggregate, no calculation, no BSP engine. At least one test must exercise a **non-commission** `SettlementOnly` line (scenario 21) so the value object cannot quietly become commission-shaped.

**Consequences:** (a) one value object, one migration, one rule, one SQL check, two scenario families. (b) two columns that can drift apart, and a third settlement attribute later means a third loose column. (c) contradicts a decision the owner already made in writing and leaves INV-013 half-proven at closure. (d) leaves the codebase able to persist a Pack-violating record.

**Answer:**

---

## OD-CLOSE-02 — Fail-closed check on `data.offerId`

**Problem:** `AirOfferSourceAdapter` never compares the response `offerId` to the requested one. A 200 for a different offer is accepted, and the Order records the **requested** id while carrying the other offer's money, journeys, services and fare construction.

**Evidence:** `AirOfferSourceAdapter.Interpret` → `AirOfferCandidateMapper.Map(request.OfferId, envelope.Data, …)`; receiver-qualified search for `details.OfferId` returns nothing; the mapper writes `requestedOfferId` into `CandidateSource.OfferId` and `ProductSnapshot.SourceOfferId`.

**Pack / owner rule:** INV-006 — "no external calls or **silent repricing**". `CONTRACTS/02-AIROFFER.md` line 13 lists the root fields as `OfferId, PricedAt, LastTicketingDate?, CurrencyId, CurrencyCode, JourneyType, …` — **`LastTicketingDate?` is the only root field marked optional; `OfferId` is not.**

**Revision 1's recommendation was wrong** and stays withdrawn: it tolerated a missing `data.offerId` using our own C# mirror's nullability as contract authority.

**Options:**
- **(a)** Strict on both: null, empty or whitespace → `ContractMismatch`; non-equal (ordinal) → `ContractMismatch`; exact match → proceed.
- **(b)** Strict only on mismatch, tolerate missing *(revision 1's position)*.
- **(c)** Warn only, record in evidence, accept.
- **(d)** Leave it.

**Recommendation: (a).** One comparison at the top of `Map`, inside a method that already throws `ContractMismatch` six times, so the adapter's failure path handles it and nothing is persisted. Ordinal comparison only — no trim, no case-fold, no alias, no normalisation. Negative tests for **both** cases.

**Consequences:** (a) closes the defect completely and matches the Pack's own field list. (b) accepts an Order whose provenance rests on nothing the owner returned. (c) leaves a wrong accepted record with a note beside it. (d) unacceptable.

**Answer:**

---

## OD-CLOSE-03 — Reconcile the stage-04 cleanup decisions

**Problem:** eight `Answer:` lines in `S1-CLEANUP-OPEN-DECISIONS.md` are empty. Revision 1 recommended answering all eight with a blanket answer. That is wrong in method: several are already substantively ratified by later owner decisions, and one contains a real contradiction that a blanket answer would bury.

**Reconciliation table:**

| Decision | Already ratified elsewhere? | Exact authority | Current code | True remaining decision? | Recommended answer |
|---|---|---|---|---|---|
| `OD-C-03` | no | — | (as the entry describes) | yes — minor | confirm as built |
| **`OD-C-04`** public `/operations/{id}` and `GetOperation` deleted | **contradicted** | `S1-API-READABILITY-OWNER-DECISION` line 60: "Pack QRY-002 `GetOperation` … **remains reachable on the internal surface only**" | search for `GetOperation` / `operations/` across `src`: **no match** — nothing exists on any surface | **YES — a genuine contradiction** | approve full deletion for S1 **and amend the earlier internal-surface statement**, or restore QRY-002 on `Internal/v1` as that decision states |
| `OD-C-05` allocation and reversal removed | **yes, substantively** | `S1-API-READABILITY-OWNER-DECISION` line 63 defers SC-S1-014 and the reversal half of SC-S1-013 | as described | no | ratified-by-reference; note INV-014 is an S1-introduced invariant now unproven |
| `OD-C-06` Prepare merged into Create | **yes, substantively** | the same decision's one-call Create | as described | no | ratified-by-reference |
| `OD-C-07` small Create response | **yes, substantively** | the same decision's response shape | as described | no | ratified-by-reference |
| `OD-C-08` invariant-string amount rendering | no | — | as described | yes — minor | confirm as built |
| **`OD-C-09`** candidate detail dictionary | partly | — | `details` dictionary still carries three typed concepts | **YES, but split it** | the *serialized canonical* `details` shape and digest stay unchanged; the *in-memory* model may become typed **post-closure**. Answer those two halves separately. |
| `OD-C-10` dev-only configuration | no | — | as described | yes — minor | confirm as built |

**Recommendation:** answer `OD-C-04` and `OD-C-09` deliberately; mark `OD-C-05`, `OD-C-06` and `OD-C-07` as ratified-by-reference with the exact citation; confirm `OD-C-03`, `OD-C-08` and `OD-C-10` as built. Do **not** mark anything closed that no owner decision actually covers.

**Answer:**

---

## OD-CLOSE-04 — Public exposure of pricing code and name *(explicitly separated from S1 approval)*

**Problem:** `PricingLine.SourceCode` / `.SourceName` / `.SourceReference` are preserved and are in the internal projection; the public `OrderPriceLineDto` exposes only the component enum and the amounts.

**This is not a domain blocker and must not gate S1 domain approval.**

**Recommendation:** keep all three preserved internally; no public API change is required for domain closure. Add `code` and `name` to `OrderPriceLineDto` only if the owner wants richer `GET` usability now — it is additive and backward compatible. **Do not** expose `SourceReference` by default: it is an opaque owner handle, not a display field.

**Answer:**

---

## OD-CLOSE-05 — Accepted SalesContext / BuyerSnapshot / InitiatingActor, and the public office contract *(expanded)*

**Problem:** `DOMAIN/01` §2 requires an immutable `SalesContext` **and** a `BuyerSnapshot` at accepted creation. Neither exists. `DOMAIN/04` §5 says "Buyer, financial Customer, Traveler, payer, agency/seller and actor are separate references" and "SalesContext … remain historical". Today:

- `BuyerActorContextType` / `BuyerActorId` hold the **initiating actor**, not a buyer.
- `TravelAgencyId` is resolved to a `FinancialCustomerId` and then dropped; it survives only inside the `agency:{id}` fragment of the `CallerScope` **string** on `OrderPreparations` and `CommandReceipts` — recoverable by parsing an idempotency key, which is not a snapshot and must never be treated as one.
- `SellingOfficeId` is one untyped `long` holding **two namespaces**: airline office on Backoffice, travel-agency office on OtaPanel/OTA.

### 5a. The office mismatch is a **current public semantic bug**, not only a provenance debt

Traced end to end in this revision:

| Step | File | What happens |
|---|---|---|
| 1 | `Order.cs:47` | `public long? SellingOfficeId` — one column, two namespaces |
| 2 | `OrderProjectionBuilder.cs:19` | `order.SellingOfficeId` is passed positionally |
| 3 | `OrderProjectionDocument.cs:15` | that position is named **`long? AirlineOfficeId`** |
| 4 | `OrderProjectionMapper.cs:22` | `document.AirlineOfficeId` |
| 5 | `OrderDto.cs:14` | `long? AirlineOfficeId` — **public** |

So a `GET` on an OtaPanel order returns a **travel-agency** office id under the field name `airlineOfficeId`. The rename happens silently at step 3 because both are positional records.

Scope check: the S1 outbox writes `IntegrationEvents.V2.OrderCreated`, which has no office field, so the leak is confined to the query/read path. `IntegrationEvents.V1.OrderCreated` does carry `AirlineOfficeId` but is not published on the S1 path.

**Public-contract options:**
- **(a) Clean, pre-freeze:** replace `AirlineOfficeId` with `SellingOfficeId` + `SellingOfficeKind` in `OrderProjectionDocument` and `OrderDto`. Correct, and a breaking rename.
- **(b) Compatibility:** add `sellingOfficeId` + `sellingOfficeKind`, keep `airlineOfficeId` as a **deprecated** field populated **only** when the kind is `AirlineOffice`, null otherwise. Nothing keeps receiving a mislabelled value.
- **(c)** Leave it.

**Recommendation: (a) if S1 is not yet consumed externally, otherwise (b).** (c) is not acceptable: it continues returning agency-office identifiers under an airline-office label. Either way the fix starts in the **domain** — the office value carries its kind — and the public shape follows.

### 5b. `BuyerSnapshot` is required separately and must not be inferred

The Pack requires both. `SalesContext` does **not** satisfy `BuyerSnapshot`. No surface today carries an independently identified Buyer fact:

| Surface | Financial customer | Buyer fact available? | Seller org fact available? | Seller office | Distributor fact | Actor | Can snapshot without inference? |
|---|---|---|---|---|---|---|---|
| Backoffice | request parameter, checked active | **no** | the owner airline itself (implicit) | `_caller.AirlineOfficeId` — airline namespace | no | `_caller.AirlineUserId` | yes, if "seller = owner airline" is ratified |
| OtaPanel | derived from `TravelAgencyId` via `ICustomerDirectory` | **no** | **yes — `_caller.TravelAgencyId`, currently discarded** | `_caller.TravelAgencyOfficeId` — agency namespace | no | `_caller.TravelAgencyUserId` | yes, once the agency id is persisted |
| Ota (PartnerAPI) | `_caller.CustomerId` | **no** | not distinguishable from the customer | `_caller.TravelAgencyOfficeId` (nullable) | `_caller.PartnerApiAccessProfileId` — currently stored as `BuyerActorId` | same profile id | partly |
| Service | request parameter | **no** | **no** | request parameter (nullable) | no | `_caller.ActorId` when authenticated | no |

**Recommendation:** persist **three separate things** — `SalesContextSnapshot`, `BuyerSnapshot`, `InitiatingActor` — and never call the actor the buyer. Buyer is **not** FinancialCustomer, **not** Actor, **not** Traveler and **not** Seller by default. If the owner accepts an explicit `NotSupplied` buyer state, persist a small `BuyerSnapshot` carrying presence and provenance rather than omitting the concept; if the owner does not accept it, a surface must start supplying a buyer identity before S1 closes. **Do not invent a buyer identity to make a column non-null.** Do not build a distribution-chain engine; no surface supplies a chain.

**Decision required:** (1) role definitions mapped to Carrier / Distributor / Seller for this deployment; (2) the authoritative source of each role per surface; (3) the minimum immutable snapshot at acceptance; (4) whether explicit `NotSupplied` is an acceptable accepted buyer state; (5) office kind vs split columns; (6) public-contract option (a) or (b).

**Consequences of not deciding:** historical seller identity stays dependent on mutable ReferenceData or on parsing an authorization key; a later office join silently mis-joins two namespaces; the public API keeps mislabelling agency offices; and S14 split and interline settlement inherit the same hole.

**Answer:**

---

## OD-CLOSE-06 — Component totals semantics and shape *(shape corrected)*

**Problem:** `DOMAIN/01` §2 — "Derived-but-persisted: CommercialSummary, CustomerTotal **and complete current component totals**." Only the first two exist.

**Revision 2's proposed shape was insufficient.** It recommended `OrderComponentTotals(OrderId, Component, Effect, Amount, CurrencyRef)` with a single `Amount`. `DOMAIN/03` §11 is explicit: "Amounts are NONNEGATIVE magnitudes. **Direction alone supplies sign**: Debit = +1, Credit = -1." A single unsigned `Amount` cannot represent a component that has both debit and credit lines, and storing a signed net would contradict the line model it summarises.

**Corrected recommended shape:**

```
OrderComponentTotal
  OrderId
  Component      (PricingComponentType)
  Effect         (CustomerBalance | SettlementOnly | Informational)
  DebitAmount    >= 0
  CreditAmount   >= 0
  CurrencyRef
unique (OrderId, Component, Effect)
```

**Rules:**
- both magnitudes nonnegative; **net is derived** as `DebitAmount - CreditAmount`, never stored;
- currency is the Order **sale** currency only — original and sale valuations are never summed together;
- deterministically derived from the committed `SaleValue` lines, and rebuilt with the projection so it can never disagree with them;
- **all effects included**, so the summary is complete;
- `CustomerTotal` remains the net of `CustomerBalance` only; `SettlementOnly` and `Informational` stay outside it;
- **Order level only** for S1; item level when a slice needs it (AIDM M5 has both);
- a deterministic persisted **summary**, not a second monetary source of truth — no behaviour reads it to make a decision.

**Decision required:** (1) confirm the debit/credit pair rather than one amount; (2) confirm all-effects coverage; (3) confirm sale currency only; (4) confirm summary-not-canonical; (5) confirm order-level-only for S1; (6) whether `Informational` totals are exposed publicly (recommendation: internal projection only for now).

Explicitly **not** recommended: a JSON dictionary, or one column per component family — that would freeze an eleven-member taxonomy into eleven columns.

**Answer:**

---

## OD-CLOSE-07 — FulfillmentProfileSnapshot target shape *(analysis corrected)*

**Problem:** `DOMAIN/02` §23 lists the snapshot as "profile ID/version, reservation requirement, resource quantity/unit policy, document requirement/type/authority, funding requirement, delivery provider/control policy, dependency treatment and whether partial fulfillment is supported."

**Two corrections to revision 2.**

**(i) Document authority is *not* an undefined vocabulary.** `DOMAIN/07` defines `LOCAL-AIRLINE` ("Ordering is the domain authority for ETKT/EMD and uses locally configured DocumentStock") and `EXTERNAL` ("an identified owner issues/transitions the document"), and the repository **already has** `AeroTech.Messages.Ordering.Enums.DocumentAuthority { Local = 1, External = 2 }`, used today by `ElectronicTicketIssued` and `ElectronicMiscDocumentIssued`. What is unresolved is the **value** for a live uncertified AirOffer profile, not the vocabulary. Represent the unknown as null/unresolved; do not invent an authority and do not add a third member.

**(ii) `FulfillmentDocumentKind` may already cover requirement *and* type.** It reads `None = 1, Etkt = 2, EmdA = 3, EmdS = 4, Unresolved = 5`. `None` expresses "no document required", a specific member expresses requirement **and** type together, and `Unresolved` expresses "not yet known". A separate `DocumentRequired` flag would duplicate it and allow the two to disagree. **Recommendation: do not add one**; add only the missing **authority**.

**Corrected coverage of the eight Pack semantics:**

| Pack semantic | State | Vocabulary defined? | Needed before |
|---|---|---|---|
| profile ID / version | **present** — `ProfileRef`, `ProfileVersion` | n/a | — |
| reservation requirement | **present** — `ReservationRequirement` | yes | S2 |
| funding requirement | **present** — `FundingRequirement` | yes | S3 |
| document requirement / type | **present** — `FulfillmentDocumentKind` covers both | yes | — |
| document **authority** | **absent** | **yes — `DocumentAuthority`, already in Contracts** | S4 |
| resource quantity / **unit policy** | partial — `CapacityUnits` is a quantity; the policy is absent | **no** | S2 |
| **delivery provider / control policy** | absent | **no** | S12 |
| **dependency treatment** | absent | **no** | S6 |
| **partial-fulfillment support** | absent | not a vocabulary — Pack states it as a boolean | S6 |

So: five semantics covered, one (authority) has a vocabulary that already exists, and three have no Pack vocabulary at all.

**Recommendation:**
- Add **`DocumentAuthority?`** now — nullable, left null for the uncertified live profile. The enum exists; nothing is invented.
- Add **`PartialFulfillmentSupported` as a *nullable* bool** — true / false / unknown. A non-nullable bool backfilled `false` would be a **business assertion** that partial fulfillment is not supported, which no source has stated. Revision 2's "add it as a boolean now" is corrected to explicitly nullable.
- For **unit policy, delivery/control policy and dependency treatment**, where the Pack defines no vocabulary, carry a **source/profile-owned opaque policy ref or code**, exactly as `SettlementAttribution.CategoryCode` does. Do not invent three enums.
- Decide the **shape** now so S2/S4/S6/S12 never migrate accepted snapshots. **No fulfillment engine, no rule framework.**

**Answer:**

---

## OD-CLOSE-08 — FundingObligation scope and disposition *(scope strategy corrected)*

**Problem:** `DOMAIN/06` — "`FundingObligation` fields: ObligationId, OrderId, Version, Purpose, sale currency, exact amount, **Service/Item/PricingLine scope**, accepted ChangeId, superseded obligation ref, source pricing decision and **current disposition**."

Current `FundingObligation` has `OrderId`, `Version`, `Purpose`, `Amount`, `OrderItemId?`, `ChangeId`, `SourceDecisionRef`, `SupersededObligationId?` — so **scope is item-only** and **disposition is absent**. `Order.ObligationRevision` does not exist. Revision 1 wrongly called this "fully sufficient for S3".

**Revision 2's proposed scope shape was wrong.** It recommended a `ScopeKind` discriminator plus one reference because it "reads better". That is a polymorphic foreign key: SQL Server cannot enforce one column against three target tables, so the only integrity left would be application-side. `FundingObligationConfiguration` already uses real FKs with `DeleteBehavior.Restrict` for `OrderChange`, `OrderItem` and the superseded obligation; a polymorphic column would be the one unenforced reference in the aggregate.

**Corrected recommended shape:**

```
OrderItemId?     FK -> Order.OrderItems      (exists today)
OrderServiceId?  FK -> Order.OrderServices   (new)
PricingLineId?   FK -> Order.PricingLines    (new)
CHECK: exactly one of the three is non-null
```

with a small typed `FundingObligationScope` in the Domain so callers pass a scope, not three nullable longs. **No separate `ScopeKind` column** — the populated FK identifies the kind, and a discriminator that can disagree with the FKs is a second source of truth.

**Also corrected:** revision 2 claimed "a fee-only `MonetaryCharge` **needs** line scope". That overstates it. A fee-only charge is an `OrderItem` and is legitimately item-scoped. Line scope becomes necessary when a future `Fee`-purpose obligation's boundary genuinely is one specific charge line. The real point stands: **item-only must not harden into an invariant**.

**Decision required:**
1. Confirm three nullable real FKs with an exactly-one CHECK, over the polymorphic alternative.
2. Confirm the S1 original-sale obligation stays **item**-scoped. (It should — that is what it is.)
3. The **disposition vocabulary**. `DOMAIN/06` names behaviours — supersede, release, rebind, preserve for audit — but **never enumerates the states**. The owner must supply them; inventing `Open/Settled/Released` would be exactly the guessing this process forbids.
4. `Order.ObligationRevision` — the Pack calls it "a discovery watermark only, not a substitute for the exact obligation ID/version in a provider request", so it belongs with **S3**, when something discovers.

**Recommendation:** decide the scope shape now and implement the two additional nullable FKs plus the CHECK with the A-batch migration; keep the S1 obligation item-scoped; defer `ObligationRevision` to S3; **raise the disposition vocabulary to the owner** and add no disposition column until it is answered. No payment behaviour now.

**Answer:**

---

## OD-CLOSE-09 — Commercial lifecycle vocabulary: shared-contract and public-wire correction *(new)*

**Problem:** the commercial lifecycle enums do not match Pack 3.8.

| Enum | Current | `DOMAIN/02` requires |
|---|---|---|
| `OrderServiceCommercialStatus` | `Pending=1, Active=2, Cancelled=3, Exchanged=4, Suspended=5` | §48: Pending, Active, Cancelled, **Replaced**, **Expired** |
| `OrderItemCommercialStatus` | `Active=1, Replaced=2, Cancelled=3` | §52: Active, **PartiallyChanged**, Cancelled, Replaced, **Partitioned**, **Expired**, **Inactive** |

`Exchanged` is an operation outcome, not a canonical state. `Suspended` has no home on the commercial axis — §52 keeps fulfillment/delivery facets separate and `OrderServiceDeliveryStatus` already exists. AIDM M2 corroborates the separation: Service carries `Status Code` **and** `Delivery Status Code` as distinct attributes.

**Why this is a decision and not a free correction.** Revision 2 recorded this as "not a decision — the Pack enumerates the states exactly". **That was wrong on process.** Both enums live in shared `Contracts/AeroTech.Messages`, both appear in the public `OrderDto`, and both serialize **by name**. Correcting the vocabulary therefore changes a shared contract, the OpenAPI document and public wire strings — a public-contract change, which under `GOVERNANCE/05` and `CLAUDE.md` needs explicit owner approval regardless of how clear the Pack text is.

**Blast radius, measured:** `Exchanged` and `Suspended` are referenced **nowhere** in `src` or `tests`; only `Active` is ever written. No accepted order has ever held either value. Item `Replaced` and `Cancelled` are equally unwritten today but are Pack-correct names and must keep their numbers.

**Recommended proposal — Pack vocabulary, current numbers preserved:**

```
OrderServiceCommercialStatus          OrderItemCommercialStatus
  Pending   = 1   (unchanged)           Active           = 1   (unchanged)
  Active    = 2   (unchanged)           Replaced         = 2   (unchanged)
  Cancelled = 3   (unchanged)           Cancelled        = 3   (unchanged)
  Replaced  = 4   (replaces Exchanged)  PartiallyChanged = 4   (new)
  Expired   = 5   (replaces Suspended)  Partitioned      = 5   (new)
                                        Expired          = 6   (new)
                                        Inactive         = 7   (new)
```

Item `Replaced` and `Cancelled` are **not** renumbered to match the Pack's prose ordering; numeric stability outranks prose order. `Suspended` does not move to the item enum — it leaves the commercial axis entirely and belongs to `OrderServiceDeliveryStatus`.

**Options:**
- **(a)** Approve the correction now, before S1 contract freeze, with the numbering above.
- **(b)** Approve additions only (`Expired`, `PartiallyChanged`, `Partitioned`, `Inactive`) and keep `Exchanged`/`Suspended` as deprecated members.
- **(c)** Defer to S5 and migrate accepted rows then.

**Recommendation: (a).** This is the cheapest moment in the project's life: nothing has been persisted with either value, so the change costs a migration guard and an enum-conformance test. Under (b) the shared contract keeps two members the Pack does not define, and every later reader must know which are real. Under (c) the same change costs a data migration of accepted commercial state. A migration guard must fail loudly if any row holds an unexpected value, and `OpenApiDocumentTests` must show the enum-schema change deliberately rather than silently.

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
| Item-level time limits (AIDM M1 has four) | at S1 there is one item and one source; INV-055 already forbids collapsing order-level validity facts into one TTL. Nothing to decide until a source supplies per-item limits |
| Percentage commission numeric fields | AIDM proves they are legitimate future source facts; AirOffer supplies none, and `PricingCalculationKind` is already the seam. `OD-P-18` stays binding |
| A `DocumentRequired` flag beside `FulfillmentDocumentKind` | the existing enum's `None` / specific-kind / `Unresolved` members already express requirement and type — see `OD-CLOSE-07` |
