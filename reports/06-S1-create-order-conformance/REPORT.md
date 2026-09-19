# S1 Create-Order Domain Benchmark & Conformance Audit

## 1. Repository state

- **Branch:** `k8s-stg`
- **HEAD:** `e86171103e9fef0b6a57a7895b32ccc87b0e5773` ("S1-Domain Repair") — exactly the reviewer-observed commit, so nothing had advanced and no diff had to be inspected.
- **Worktree at start:** clean.
- **Worktree at end:** one untracked directory, `reports/06-S1-create-order-conformance/`.
- **Code changed:** none. No file under `src/`, `Contracts/`, `tests/` or `Migrations/` was touched, no package reference altered, no public API changed. This run was audit-only.
- **Historical evidence repository:** `E:\Projects\DotAir\Ordering`, branch `k8s-stg` (`077a851`), read only.

## 2. Authority and benchmark sources

Authority order applied: owner decisions in `reports/00-decisions/` → Pack 3.8 → actual AeroTech contracts and observed wire payloads → IATA → Amadeus/Sabre/Navitaire → historical repository → engineering preference last.

Internal reading: `START-HERE`, `VERTICAL-SLICE-PLAN`, `SLICES/S1`, `SLICES/S6`, `GOVERNANCE/05`, `DOMAIN/01`–`04`, `06`, `13`–`15`, `CONTRACTS/02-AIROFFER`, `BLOCKED-DECISIONS`, `SPEC/scenarios.json` (all 21 S1 scenarios), `SPEC/invariants.json` (all S1 invariants), and every file in `reports/00-decisions/` and `reports/05-S1-domain-parity-audit/`. Source code and migrations were read directly; no claim below rests on a report alone.

External sources, with confidence and two honest limits, are in `BENCHMARK-SOURCES.md`:

- **IATA** — ONE Order, Airline Retailing, Settlement with Orders retrieved and quoted (HIGH). The normative AIDM/schema detail is behind the developer portal, so entity field lists are `NOT PUBLICLY PROVEN` and are used as concept names only.
- **Amadeus** — Nevio capability names quoted (HIGH). No entity field asserted.
- **Sabre** — `sabre.com` returns **HTTP 403** to this agent and `investors.sabre.com` timed out. Sabre claims rest on search extracts of official pages, capped at **MEDIUM**, and no Sabre entity field is asserted anywhere.
- **Navitaire** — New Skies wording quoted (HIGH).

## 3. Executive result

**`S1_CREATE_ORDER_DOMAIN_GAPS_FOUND`**

The domain can faithfully represent a realistic airline sale. Of 105 AirOffer wire fields, 99 are preserved, 3 are deliberately not consumed with recorded reasons, and 2 are blocked by an owner-contract gap the owner already recorded. Of 83 benchmarked concepts, none requires a domain redesign — `FIX_DOMAIN_DESIGN` is zero.

Two gaps can invalidate S1, and both are small: a **missing validation** and a **missing pair of fields**. Neither is structural.

## 4. Critical S1 gaps

### 4.1 An AirOffer response for a different offer is accepted silently

`AirOfferSourceAdapter.Interpret` calls `AirOfferCandidateMapper.Map(request.OfferId, envelope.Data, …)` and never compares `envelope.Data.OfferId` to `request.OfferId`. Receiver-qualified search confirms `details.OfferId` is read nowhere.

If AirOffer answers 200 for a different offer, the Order is accepted with offer B's money, journeys, services and fare construction while `Orders.AcceptedSourceOfferId`, `OrderItems.ProductSourceOfferId` and the public `offerId` all claim offer A. The raw payload retains B's id, so it is detectable afterwards — but the accepted record is wrong and the customer is told they bought A.

This defeats INV-006 in substance: a silently substituted offer is the strongest form of "silent repricing".

Smallest fix, documented not implemented: one ordinal comparison at the top of `Map`, throwing the existing `AirOfferContractMismatchException`, which the adapter already maps to `ContractMismatch` so nothing is persisted. No alias or equivalence semantics. See `OD-CLOSE-02`.

### 4.2 A settlement line can be accepted with no counterparty and no category

DOMAIN/03 §13: "SettlementOnly requires party/category/currency." `SettlementPartyRef` and `SettlementCategory` exist in **no** layer — not `PricingLine`, not `CandidatePricingLine`, not SQL, not the projection, not `PricingLineMatrix`. The historical repository had both.

This is not deferrable polish. `S1-API-READABILITY-OWNER-DECISION-2026-09-18` line 64 states that "the settlement/commission half of SC-S1-013 stays proven" in S1, and INV-013 is an S1-introduced invariant. Today the *arithmetic* half is proven (`PricingArithmeticTests` shows 405 with the commission excluded) but only on plain `PricedAmount` arrays; **no test at any layer persists a settlement commission line**, and there is no `SC_S1_013` test at all. See `OD-CLOSE-01`.

## 5. Important but non-blocking gaps

| # | Gap | Why it matters | Where |
|---|---|---|---|
| 1 | Nine representable scenarios have no test: multi-city, connecting itinerary, two travelers on different fares, fare + surcharge + fee, markup, repeated identical tax codes, multiple taxes, mixed-coupon `Conditional` terms, corporate payer distinct from traveler | The model handles them; nothing proves it stays that way | `CREATE-ORDER-SCENARIO-MATRIX.md` §G |
| 2 | Eight `Answer:` lines in `S1-CLEANUP-OPEN-DECISIONS.md` are empty (`OD-C-03` … `OD-C-10`) | `BLOCKED-DECISIONS.md` says "Empty Answer lines … remain unanswered". S1 cannot close cleanly while decisions that shaped its public contract are formally unanswered | `OD-CLOSE-03` |
| 3 | INV-014 (allocation) is an S1-introduced invariant that is now unproven, by owner decision | The deferral is ratified, but the invariant register still lists it as introduced at S1 | `OD-C-05`, `OD-CLOSE-03` |
| 4 | `CandidateService.Details` is a string dictionary for three known core concepts | `CLAUDE.md` forbids dynamic JSON for known concepts; the typed home already exists | `CODE-SIMPLICITY-AUDIT.md` §1 |
| 5 | Pricing `code` and `name` are preserved internally but invisible to every caller | An airline user sees "Tax — 20.00" instead of "AT — Airport tax" | `OD-CLOSE-04` |
| 6 | No reflection guard that every candidate member reaches the canonical form | The `Only()` contract catches writer/reader drift, but not a member added to a record and to neither side | `CODE-SIMPLICITY-AUDIT.md` §3 |

## 6. False alarms and concepts correctly deferred

**Seat number — addressed explicitly, as required.**

Seat was *not* pulled into S1, and that is correct, not an omission:

- DOMAIN/02's typed-details table gives **Seat** its own row — "Exactly one traveler and related air service; seat product/characteristics, requested seat when sold by identifier" — separate from the **AirTransport** row.
- `VERTICAL-SLICE-PLAN` places Seat implementation in **S6**.
- Four different facts are involved and they have four lifecycles: a *sold seat product*, a *requested seat preference*, a *committed operational assignment*, and a *DCS reassignment*. DOMAIN/02 says the last is "a separate observation" and a commercial downgrade "is a servicing decision".
- IATA corroborates the separation at MEDIUM confidence: an Order "supports the sale of a flexible range of Airline products and Services that are not necessarily Journey based", and "each passenger in an Order may hold different sets of products and services".
- `AirTransportDetail` carries **no** seat field. The historical repository put `RequestedSeat` on its air-transport detail — that is the precedent to avoid, and the current model avoided it.
- The design has a valid future home: `OrderService` already has `Type`, `DetailSchema`, `DetailSchemaVersion` and one nullable typed-detail navigation; a `SeatDetail` follows the identical pattern.

**Verdict: `DEFER_IMPLEMENTATION`, not `FIX_DOMAIN_DESIGN`.** Adding `SeatNumber` to `AirTransportDetail` is recorded as `REJECT_INVENTION` (matrix §K1).

**Historical repository comparison** (`Ordering/k8s-stg` @ `077a851`) — every concept present there and absent now, classified:

| Historical concept | Classification | Verdict |
|---|---|---|
| `OrderPricingLine.SettlementPartyRef` / `.SettlementCategory` | genuinely useful airline-domain semantic | **the one real gap** — see 4.2 |
| `OrderPricingLine.Code` / `.Description` | genuinely useful | already restored as `SourceCode` / `SourceName` |
| `Commission` value object | implementation detail | the component + effect + (missing) party/category carry the fact; a VO adds nothing |
| `OrderSeatServiceDetail`, `OrderAirTransportServiceDetail.RequestedSeat` | later-slice behavior / legacy debt | Seat belongs to S6; `RequestedSeat` on the air detail was the wrong home |
| Fare construction graph | genuinely useful | restored typed in stage 05 |
| `OrderItemProductSnapshot`, `OrderItemCommercialTermsSnapshot` | genuinely useful | restored in stage 05 |
| `OrderItemPolicySnapshot` | duplicated source of truth | overlaps `FulfillmentProfileSnapshot`; correctly absent |
| `OrderTimeLimit` | later-slice behavior | S1 keeps the observed fact typed; the authoritative deadline is BD-004 |
| `OrderExternalReference` | later-slice behavior | one source at S1; `AcceptedSource` holds it |
| Typed ancillary details (seat/bag/meal/lounge/hotel/ground/generic) | later-slice behavior | S6 |
| `OrderServiceCoveredService` (dependencies) | later-slice behavior | S6, with the no-cycle rule DOMAIN/13 names |
| `OrderPricingAllocationSet` / `OrderPricingAllocation` | later-slice behavior | owner-deferred; DOMAIN/03 warns RefundBasis is not an implicit allocation purpose |
| `OrderPricingLine.Quantity` / `UnitOfMeasure` / `UnitPrice` | unsupported assumption | rejected in `OD-P-17`; no source supplies it |
| `OrderRemark`, `OrderChange` variants, six service status enums | later-slice behavior | S5/S8/S12 |

Nothing was copied because "old had more fields". The single restoration this audit recommends — settlement party and category — is justified by DOMAIN/03 and IATA, with the historical field only as corroboration.

## 7. Pricing and settlement verdict

**Customer economics vs settlement economics — audited end to end.**

| Pack rule | Enforced where | Verdict |
|---|---|---|
| CustomerBalance alone contributes to `CustomerTotal` | `PricingArithmetic.CustomerTotal` filters on `Effect == CustomerBalance` | **PASS** |
| SettlementOnly does not affect customer payable | same filter; tested by `Sale_with_settlement_commission_charges_the_customer_405` | **PASS** (domain level) |
| Commission is not a customer charge | `PricingLineMatrix` throws; SQL `CK_PricingLines_CommissionNotCustomer`; `CandidateValidatorTests.Commission_cannot_be_a_customer_charge` | **PASS** |
| Tax cannot be settlement-only | `PricingLineMatrix`; SQL `CK_PricingLines_TaxNotSettlement`; `PackExamples.SettlementTax` negative fixture | **PASS** |
| Direction alone supplies sign | non-negative magnitudes + `PricingLineMatrix.Sign` | **PASS** |
| **SettlementOnly requires explicit party / category / currency** | **nowhere** | **FAIL** |

**Agency commission and SC-S1-013.** The scenario reads: "Fare400 + bag50 + discount credit45 + settlement commission20 … Customer405 initially, 450 after explicit reversal; commission excluded." The owner split it: the reversal half is deferred to the slice that first persists a reversal; **the settlement/commission half stays proven in S1**.

Status of that retained half:
- Arithmetic: proven, but only on `PricedAmount` arrays in a pure domain test.
- Matrix and SQL rules: proven.
- **Representation: incomplete** — no party, no category.
- **End-to-end: unproven** — no `SC_S1_013` test exists at any layer, and the AirOffer mapper always emits `CustomerBalance`, so only the reference source could construct one and nothing does.

The fix must stay inside Ordering's boundary: preserve the accepted commercial/settlement fact, and nothing more. Explicitly rejected (`matrix §K5`): an agency commission engine, commission derived from agency contracts, settlement accounting moved into Ordering, or commission made part of the customer total.

**Can the reference source construct and persist a valid settlement commission case today?** Structurally yes — `CandidateBuilder.Line(…, effect:)` accepts `SettlementOnly` and the validator permits `Commission` + `SettlementOnly`. But the resulting record would be Pack-violating (no party, no category), which is precisely why the missing fields matter before a test is written.

## 8. AirOffer information-loss verdict

| Outcome | Count |
|---|---|
| Wire fields audited | **105** |
| Read and preserved, or provably derivable | **99** |
| Deliberately ignored with a recorded reason | **3** — `CouponWire.CouponId`, `CouponWire.Sequence`, `TicketWire.TravellerIndex` |
| Blocked by an owner-contract gap | **2** — `FlightWire.Stop`, `LegWire.Stop` (`OD-P-12`, handoff `OR-002`) |
| Lost | **1** — `DetailsWire.OfferId`, lost as a **validation**, not as data |
| Silently lost with no reason recorded | **0** |

The three ignored fields are justified in `DOMAIN-BENCHMARK-MATRIX.md` §C4: the coupon ordinal survives positionally inside every `SourceLineRef`, `TravellerRef` is the identity the caller binds against, the raw payload retains all three, and a duplicate coupon on one traveler+segment **fails closed** through `UniqueIndex` rather than colliding. An ETKT coupon entity in S1 to hold an AirOffer source id is `REJECT_INVENTION`.

One semantic limit is recorded honestly: the recorded live response references a rate period whose own `from`/`to` currencies (`71 → 70`) do not correspond to the line's (`155 → 70`). The evidence is preserved verbatim and nothing interprets it; the meaning is `NOT PUBLICLY PROVEN` and sits inside `OR-002`.

## 9. Scenario coverage verdict

43 practical scenarios plus the 21 Pack scenarios reconciled.

| Outcome | Count |
|---|---|
| Representable **and** tested | **24** |
| Representable but untested | **9** |
| Correctly deferred to a later stage | **4** |
| Incorrectly unsupported | **2** — settlement commission party/category; `offerId` mismatch |
| Blocked by owner semantics | **4** — infant (BD-002), `Stop`, bound `direction`, root `journeyType` |

Pack reconciliation: 19 of 21 `SC-S1-*` have named tests. `SC-S1-014` is absent by ratified owner decision. `SC-S1-013` is absent and only half-ratified — that absence is gap 4.2.

## 10. Full-domain future readiness

Thirteen of fourteen behavior families are clean `DEFER_IMPLEMENTATION`: canonical identities exist, the Pack specifies what is missing, and no S1 decision would force a destructive redesign. Detail in `FUTURE-READINESS.md`.

Three S1 decisions actively improved future readiness:

1. **Seat stayed out of `AirTransportDetail`** — the most common way to corrupt this model was avoided.
2. **The fare-construction graph became typed** — S10 exchange can query fare coupling instead of parsing a JSON column.
3. **`ScopeAtAssociation` was added to the item–service link** — S14 split can reconstruct historical item contents without relying on current ownership, which DOMAIN/02 §13 forbids.

The fourteenth family, agency (and by extension partner) settlement, is not a future question. It is gap 4.2.

## 11. Code simplicity and readability findings

The S1 path is not over-abstracted. Full detail in `CODE-SIMPLICITY-AUDIT.md`.

- **One genuine unnecessary indirection:** `CandidateService.Details` is a string dictionary carrying three known core concepts that are typed everywhere else. Replaceable by a typed candidate detail while keeping `ServiceDetailSchemaRegistry` for the `(type, schema, version)` gate that INV-058 depends on.
- **One cheap guard worth adding:** a reflection test asserting every candidate member reaches the canonical form.
- **One cosmetic extraction:** `AirOfferCandidateMapper.Map` is ~120 lines; two private business-named methods would help. The single-pass traversal must stay, because coupon totals reconcile against ticket totals against the root.
- **Deliberately retained, recorded so nobody "simplifies" them later:** the single-entry `ServiceDetailSchemaRegistry` (it is the SC-S1-021 gate), the three mirrored model families (the Pack requires the separation, and collapsing them is exactly the mistake stage 05 had to undo), the four per-surface command triplets (a ratified owner decision), and the `20288` throw in `OrderProjectionMapper` (the deterministic failure the owner required instead of `FirstOrDefault()`).
- **Inverted result:** the two real problems in this path are **missing** checks, not excess machinery.

## 12. Open owner decisions

| ID | Title | Kind |
|---|---|---|
| `OD-CLOSE-01` | Where the settlement party and category live | closes critical gap 4.2 |
| `OD-CLOSE-02` | Fail-closed check on `data.offerId` | closes critical gap 4.1 |
| `OD-CLOSE-03` | Close the eight unanswered stage-04 cleanup decisions | governance |
| `OD-CLOSE-04` | Public exposure of pricing code and name | public contract |

Already open elsewhere and not duplicated here: `OD-P-12` (AirOffer `Stop` vocabulary, plus `direction` and `journeyType`, carried as handoff `OR-002`) and BD-001 … BD-013.

## 13. Recommended repair scope

No code was written. In dependency order, once the owner answers:

1. **`OD-CLOSE-02`** — one ordinal comparison in `AirOfferCandidateMapper.Map` plus one negative test. Smallest change, highest integrity value.
2. **`OD-CLOSE-01`** — a `SettlementAttribution` value object on `PricingLine` and `CandidatePricingLine`, one conditional rule in `PricingLineMatrix`, one SQL check, one migration, one `SettlementCategory` enum in `Ordering/Enums`, and the missing `SC_S1_013` scenario test through the reference source.
3. **Test-only** — the nine representable-but-untested scenarios, and the candidate-member reflection guard.
4. **`OD-CLOSE-03`** — answer the eight register lines. Writing, not engineering.
5. **Optional, owner-gated** — `OD-CLOSE-04` (expose pricing code and name) and the typed candidate air-transport detail replacing the string dictionary.

Nothing in this list is a redesign. Items 1 and 2 are what stand between the current state and S1 closure.

## 14. S1 status

`S1_NOT_READY_FOR_APPROVAL`

Two gaps must close first: the `offerId` integrity check (4.1) and the settlement party/category representation with its SC-S1-013 proof (4.2). Both are small and neither requires a domain redesign.

## 15. S2 status

`S2_NOT_STARTED`
