# Create-Order Scenario Matrix

Stage: 06-S1-create-order-conformance · **revision 3**, accounting corrected · 2026-09-19 · HEAD `4e48447`

> **SUPERSEDED DISPOSITIONS — 2026-09-21.** The scenario definitions, industry reasoning and audit text in this file
> remain current. The **Primary** column is stale from this date. The authoritative dispositions are in
> `reports/08-S1-authoritative-final-closure/SCENARIO-CLOSURE-MATRIX.md`, which closes every `UNTESTED` and
> `UNSUPPORTED` row (24 -> 56 `TESTED`, 0 `UNTESTED`, 0 `UNSUPPORTED`) and corrects four reliability rows that named
> tests which no longer exist. Nothing here was deleted.

## Counting rules (new in revision 3)

Revision 2's totals were internally inconsistent: the table said 9 incorrectly unsupported, the prose said 11, and the prose enumeration listed 12 rows. It also compared "43 → 61" without noting that revision 1's 43 **included** the reliability cases as numbered rows while revision 2's 61 did not. Both are corrected by three rules:

1. **Two populations, stated separately.** `61 business/domain scenarios (1–61) + 12 reliability scenarios (R1–R12) = 73 total.` Revision 1's 43 was 31 business/domain + 12 reliability, so the like-for-like growth is **31 → 61**, not 43 → 61.
2. **Exactly one primary disposition per scenario.** The five primary dispositions are mutually exclusive and sum to 61. No overlap subtraction is used anywhere.
3. **Secondary tags are reported separately** and are never added into the primary totals.

A scenario counts as `TESTED` only when a named test asserts **that scenario's own claim**. A test that proves an adjacent rule does not make the scenario tested; that is why 25, 27, 44 and 58 moved out of `TESTED` in this revision.

Columns: **Repr.** = the domain model and canonical candidate can hold the facts without loss or invention. **Exec.** = an S1 caller can produce this outcome today through the real path. **Closed** = where unsupported, the system fails closed with a named error. **Primary** = the one disposition counted.

Test layers: `D` domain, `A` application, `S` SQL, `H` HTTP/API, `M` migration.

---

## A. Composition and item boundary

> **Reconciled 2026-09-20 by the post-R2 independent closure pass.** Rows whose subject no longer exists in the S1
> model (pricing groups, the dynamic detail-schema registry) are now `NOT_APPLICABLE` with the reason, and rows whose
> gap has since been closed carry their current test. Historical audit text elsewhere in this file is untouched.

| # | Scenario | Industry reason | Required canonical facts | Repr. | Exec. | Closed | Layer | Existing / new test | Primary | Stage |
|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Two independently priced `OrderItem`s in one Order | AIDM M1: an Order has many individually priced items | 2 items, disjoint service ownership, 2 accepted totals | yes | yes (reference source) | n/a | D,S | **new** | `UNTESTED` | S1 |
| 2 | One `OfferPackage` item + one independent product item | Air plus a separately sold ancillary | mixed `OrderItemKind` in one Order | yes | yes (reference source) | n/a | D,S | **new** | `UNTESTED` | S1 shape, S6 content |
| 3 | Fee-only `MonetaryCharge` item with **zero** services | `DOMAIN/01` §3 explicit fee-only exception | item with no services, line with `PricingBasisType.OrderItem` | yes | yes (reference source) | yes — a `MonetaryCharge` that owns services is rejected | D,S | **new** (`CandidateValidator.EnsureItems` rule is untested) | `UNTESTED` | S1 |
| 4 | `Included` service, no synthetic zero line | `DOMAIN/02` §21 | `PriceTreatment.Included`, no zero-amount line | yes | yes (reference source) | n/a | D,S | **new** | `UNTESTED` | S1 shape, S6 content |
| 5 | `Complimentary` service, no synthetic zero line | same | `PriceTreatment.Complimentary` | yes | yes (reference source) | n/a | D,S | **new** | `UNTESTED` | S1 shape, S6 content |
| 6 | Same traveler+segment in two legitimate independent items | A seat product and the air service cover the same segment | two items, two services, air-uniqueness rule not violated | yes — the uniqueness rule is scoped to **air** services | **no** — needs a second service type, which is S6 | yes for two *air* services | D | **new** | `DEFERRED` | S6 |
| 7 | Source-proven fare construction spanning more than one item | `DOMAIN/03` §47 "may cross items" | `FareConstructionItem` rows for 2 items | yes | yes (reference source) | n/a | D,S | **new** | `UNTESTED` | S1 |

## B. Sales, parties and roles

| # | Scenario | Industry reason | Required canonical facts | Repr. | Exec. | Closed | Layer | Test | Primary | Stage |
|---|---|---|---|---|---|---|---|---|---|---|
| 8 | OtaPanel Order retains historical agency/seller identity after ReferenceData changes | AIDM M4 Seller is a distribution-chain role; `DOMAIN/04` §5 snapshots are historical | accepted seller org id on the Order **and in the accepted candidate digest** | **NO** — `TravelAgencyId` is resolved away; it survives only inside the `agency:{id}` fragment of the `CallerScope` string, which is an idempotency key, not a snapshot | no | no | S,H | **new** | `UNSUPPORTED` | S1 |
| 9 | FinancialCustomer, Buyer, Seller and Actor proven not conflated | `DOMAIN/04` §5 lists six distinct references | four distinguishable accepted facts | **NO** — `BuyerActorId` is the actor; there is no buyer and no seller | no | no | D,S | **new** | `UNSUPPORTED` | S1 |
| 10 | Corporate financial customer distinct from traveler and actor | The payer is not the passenger | customer id ≠ any traveler; actor is a third identity | yes — all three identities exist today | yes | yes (inactive customer rejected) | H | `An_unknown_or_inactive_customer_is_rejected` proves the customer gate only; the **distinctness** assertion has no test | `UNTESTED` | S1 |
| 11 | PartnerAPI caller identity is not automatically buyer or seller | AIDM M4 role ≠ organisation type | partner profile stored as what it is | **NO** — stored in `BuyerActorId` | no | no | D | **new** | `UNSUPPORTED` | S1 |
| 12 | Backoffice office and OtaPanel office are not the same namespace | two office identity spaces; the public DTO currently labels both `airlineOfficeId` | office value carries its namespace, in the domain, the candidate digest and the public contract | **NO** — one untyped `long` | no | no | D,S,H | **new** | `UNSUPPORTED` | S1 |
| 13 | Airline direct / backoffice sale | airline's own staff | channel, office from token | yes | yes | yes | H | `Backoffice_sells_a_flight_offer_…`, `Backoffice_selling_office_must_match_the_token_office` | `TESTED` | S1 |
| 14 | OTA sale for its token customer | online agency | customer from token | yes | yes | yes | H | `Ota_sells_for_its_token_customer_and_cannot_read_another_customer_order` | `TESTED` | S1 |
| 15 | Agency panel sale for the agency's customer | agency staff | agency-scoped customer | yes | yes | yes | H | `OtaPanel_sells_for_the_customer_of_its_travel_agency` | `TESTED` | S1 |
| 16 | Same accepted order across authorized surfaces | one truth, four doors | shared handler, per-surface read scope | yes | yes | yes | H | `Current_controller_surface_is_exactly_the_characterized_inventory`, `The_route_surface_is_bound_to_the_token_surface_…` | `TESTED` | S1 |

## C. Pricing and settlement

| # | Scenario | Industry reason | Required canonical facts | Repr. | Exec. | Closed | Layer | Test | Primary | Stage |
|---|---|---|---|---|---|---|---|---|---|---|
| 17 | **Amount-based SettlementOnly commission, candidate → domain → SQL → projection → rebuild** | owner kept this half of SC-S1-013 in S1 | `Commission` + `SettlementOnly` + **PartyRef** + **CategoryCode**, excluded from `CustomerTotal` | **NO** — no attribution members | no | no | D,A,S,H | `Sale_with_settlement_commission_charges_the_customer_405` (domain arithmetic only) | `UNSUPPORTED` | S1 |
| 18 | SettlementOnly with **no** attribution → reject | `DOMAIN/03` §13 requires party/category | domain + SQL both refuse | **NO** | no | **no — currently accepted** | D,S | **new** | `UNSUPPORTED` | S1 |
| 19 | SettlementOnly with only one of party/category → reject | half-populated attribution is meaningless | SQL constraint prevents it | **NO** | no | no | D,S | **new** | `UNSUPPORTED` | S1 |
| 20 | Settlement category preserved losslessly, never interpreted | the Pack requires a category; it never enumerates one | source string round-trips unchanged | **NO** | no | n/a | D,S | **new** | `UNSUPPORTED` | S1 |
| 21 | **Non-commission SettlementOnly** — a `Fee` or `Markup` line on an explicit source settlement basis | `DOMAIN/03` §19: Fee/Markup/Penalty carry an "explicit source settlement basis"; settlement attribution is **not** commission-specific | same attribution members on a non-`Commission` component | **NO** | no | n/a | D,S | **new** *(revision 3 replaces revision 2's second commission scenario with this one, so the value object cannot silently become commission-shaped)* | `UNSUPPORTED` | S1 |
| 22 | Commission + `CustomerBalance` → reject | INV-013 | domain + SQL check | yes | n/a | **yes** | D,S | `Commission_as_settlement_only_is_allowed_…`, `Commission_cannot_be_a_customer_charge`, `CK_PricingLines_CommissionNotCustomer` | `TESTED` | S1 |
| 23 | `Informational` line never affects `CustomerTotal` | Effect is not a second sign | filter excludes it | yes | yes | n/a | D | **new** (`PricingArithmetic` filters it; no test asserts it) | `UNTESTED` | S1 |
| 24 | `Other` component only `Informational` | `DOMAIN/03` §15 | matrix rejects other effects | yes | n/a | **yes** | D,S | **new** (rule exists in `PricingLineMatrix` and SQL; untested) | `UNTESTED` | S1 |
| 25 | Fare + several taxes | every real ticket | N distinct `Tax` lines | yes | yes | n/a | S | `Every_tax_occurrence_on_one_ticket_survives_acceptance_sql_and_rebuild` — one fare + four taxes, three codes, `AT` repeated at two occurrences under two references; nothing merges by code | `TESTED` | S1 |
| 26 | Carrier surcharge + fee persisted | YQ/YR plus service fee | categories 2 and 3 mapped | yes | yes | n/a | S | **new** | `UNTESTED` | S1 |
| 27 | Discount credit persisted | a concession | `Discount` + `Credit` | yes | yes | n/a | S | `PricingArithmeticTests` theory proves the **arithmetic**; persistence is untested | `UNTESTED` | S1 |
| 28 | Markup persisted | agency or airline uplift | `Markup` + `Debit` | yes | yes | n/a | S | **new** | `UNTESTED` | S1 |
| 29 | `Adjustment` debit **and** credit both allowed | `NormalCustomerDirection` returns null for Adjustment | either direction accepted | yes | yes | n/a | D | **new** | `UNTESTED` | S1 |
| 30 | Repeated identical tax codes remain separate occurrences | two YQ rows are two charges | unique `(PriceChangeSetId, CandidateLineRef)` on the occurrence path | yes | yes | yes | S | **new** | `UNTESTED` | S1 |
| 31 | Zero-value line accepted | a zero fee is not negative | `EnsureMagnitude` rejects only negatives | yes | yes | n/a | D | **new** | `UNTESTED` | S1 |
| 32 | Multi-currency original / sale | 100 USD original, 92 EUR sale | two `Money` values, never summed | yes | yes | yes | D,A,S | `SC_S1_012_…`, `Equivalent_amount_…`, `Two_values_in_one_currency_are_a_contract_mismatch` | `TESTED` | S1 |
| 33 | Applied conversion evidence preserved, money not recomputed | rate provenance | rate, decimal places, rounding token | yes | yes | n/a | S | `Applied_conversion_evidence_is_preserved_without_recomputing_money` | `TESTED` | S1 |
| 34 | Source percentage row | the source priced a percentage | `CalculationKind = Percentage`, value untouched | yes | yes | yes | A,S | `Percentage_rows_record_their_calculation_kind_…` | `TESTED` | S1 |
| 35 | 3-decimal currency keeps its scale | BHD/KWD/TND | exact decimal round-trip | yes | yes | yes | S | `Three_and_zero_decimal_currencies_keep_their_scale` | `TESTED` | S1 |
| 36 | Zero-decimal currency keeps its scale | JPY/KRW | same | yes | yes | yes | S | same | `TESTED` | S1 |
| 37 | Precision / overflow boundary rejected, not rounded | INV-059 | amount beyond scale refused | yes | n/a | **yes** | D,S | `Amount_beyond_storage_scale_is_rejected_not_rounded`, `Amount_rate_and_quantity_round_trip_unchanged_on_sql_server` | `TESTED` | S1 |
| 38 | **Component-total reconciliation, debit and credit sides kept apart** | `DOMAIN/01` §2 "complete current component totals"; AIDM M5 Price has Base/Total plus component associations | persisted `(Component, Effect, DebitAmount, CreditAmount)` equal the sum of their lines; net is derived, never stored as a signed amount | **NO** — no component totals exist | no | n/a | D,S | **new** | `UNSUPPORTED` | S1 |
| 39 | Order-level charge | a charge on the package | line with item basis | yes | yes | n/a | S | `Percentage_order_charge_…` | `TESTED` | S1 |
| 40 | Source total mismatch → reject | never invent a rounding fee | `ContractMismatch`, nothing persisted | n/a | n/a | **yes** | A,S | `SC_S1_015_…` | `TESTED` | S1 |
| 41 | Group line extended once, not multiplied | INV-012, SC-S1-011 | store 200 not 400; group quantity kept | yes | yes | n/a | D,S | — | `NOT_APPLICABLE` | AirOffer supplies no pricing group; `FarePricingGroup` was removed as `SOURCE_NOT_SUPPLIED_DO_NOT_INVENT` in stage 07 |

## D. Composition and itinerary

| # | Scenario | Required canonical facts | Repr. | Exec. | Closed | Layer | Test | Primary | Stage |
|---|---|---|---|---|---|---|---|---|---|
| 42 | One adult, one one-way flight | the irreducible sale | yes | yes | n/a | D,A,S,H | `SC_S1_002_…` | `TESTED` | S1 |
| 43 | Multi-traveler package | 2 travelers, 1 item, 4 services | yes | yes | n/a | D,S | `SC_S1_008_…` | `TESTED` | S1 |
| 44 | Adult + child | distinct PTC | yes | yes | yes (PTC must agree with the binding) | D | `Incomplete_or_inconsistent_traveler_binding_is_rejected` proves the **binding rule**; no CHD-specific scenario test exists | `UNTESTED` | S1 |
| 45 | Two travelers with different fare groups | non-homogeneous Order (AIDM M1/I5) | yes | yes (reference source) | n/a | D,S | **new** | `UNTESTED` | S1 |
| 46 | Different itineraries for travelers in one Order | each traveler's services cover different segments | yes | yes (reference source) | n/a | D,S | **new** | `UNTESTED` | S1 |
| 47 | Connection — two passenger segments under one journey | 2 segments sharing a `JourneyId` | yes | yes | n/a | D,S | **new** | `UNTESTED` | S1 |
| 48 | Multi-city — three or more bounds | ≥3 journeys with sequence | yes | yes | n/a | D,S | **new** | `UNTESTED` | S1 |
| 49 | Technical stop — one segment, many legs | legs never multiply services | yes | yes | n/a | D,S | `SC_S1_007_…`, `Technical_stop_keeps_one_service_and_two_legs` | `TESTED` | S1 |
| 50 | Same flight number on two distinct dated segments does not collapse | identity is `{BoundId}\|{FlightId}`, not the number | yes | yes | yes — a duplicate segment ref is rejected | D,S | **new** | `UNTESTED` | S1 |
| 51 | Round trip as one RT unit vs two OW units | source fare construction retained as supplied | yes | yes (reference source) | n/a | D,S | `Every_owner_pricing_unit_kind_maps_to_its_own_construction_type` — the real contract's `OneWay`, `RoundTripFromOneWays` and `RoundTripFare`, without collapsing the two round trips | `TESTED` | S1 |
| 52 | Opaque fare construction | zero groups, zero coverage rows | yes | yes | yes | D,S | `SC_S1_010_…`, `Opaque_construction_cannot_claim_component_links` | `TESTED` | S1 |
| 53 | OpenAir structural scenario under a certified reference profile | `SegmentKind.OpenAir`, no dated flight | yes — validator has the rule | yes (reference source) | yes — an open segment carrying a dated flight is rejected | D | **new** | `UNTESTED` | S1 |

## E. Contract fail-closed

| # | Scenario | Required outcome | Closed today? | Layer | Test | Primary | Stage |
|---|---|---|---|---|---|---|---|
| 54 | Response `offerId` **missing or blank** | `ContractMismatch`, nothing persisted | **NO** | A | **new** | `UNSUPPORTED` | S1 |
| 55 | Response `offerId` **mismatched** | `ContractMismatch`, nothing persisted | **NO** | A | **new** | `UNSUPPORTED` | S1 |
| 56 | Unknown pricing category | `ContractMismatch` | yes | A | `Observed_wire_violations_fail_mapping_instead_of_guessing` | `TESTED` | S1 |
| 57 | Unknown registered detail schema / version | `UnsupportedCapability` before any Order | yes | D,A | — | `NOT_APPLICABLE` | the dynamic detail-schema registry was removed in stage 07; air transport is a typed table, so there is no unregistered schema to reject |
| 58 | Duplicate traveler+segment air service **occurrence** | rejected, never merged | yes | D,S | `An_air_service_cannot_cover_two_segments` at the candidate boundary, plus the unique index `(OrderId, TravellerId, SegmentId)` on SQL Server | `TESTED` | S1 |
| 59 | Malformed source hierarchy totals | `ContractMismatch` | yes | A,S | `SC_S1_015_…` | `TESTED` | S1 |
| 60 | Source currency / value inconsistency | `ContractMismatch` | yes | D | `Two_values_in_one_currency_are_a_contract_mismatch` | `TESTED` | S1 |
| 61 | Invalid traveler binding | 422 before any Order | yes | D,A,H | `SC_S1_016_…` | `TESTED` | S1 |

## F. Reliability (R1–R12, counted separately)

| # | Scenario | Closed / proven | Test |
|---|---|---|---|
| R1 | Same idempotency key replay | yes | `SC_S1_003_…`, `The_same_idempotency_key_returns_the_same_order` |
| R2 | Replay conflict | yes | `SC_S1_004_…` |
| R3 | Concurrent acceptance of one source | yes | `SC_S1_005_…`, `A_second_consumption_of_the_same_accepted_source_is_a_conflict` |
| R4 | Owner offline after accepted commit | yes | `SC_S1_003_…`, `Every_supplied_air_offer_fact_survives_…` (call count) |
| R5 | Projection rebuild deterministic | yes | `SC_S1_020_…`, `Projection_rebuild_is_byte_identical_…` |
| R6 | Projection schema 2 → 3 transition | yes | `A_schema_two_projection_row_stays_readable_…` |
| R7 | Migration from pre-repair S1 | yes | `S1_domain_parity_upgrade_preserves_accepted_rows_without_inventing_facts` |
| R8 | Source evidence unchanged | yes | `S1/AirOfferLiveCandidateBridgeTests` |
| R9 | Atomic projection failure | yes | `SC_S1_006_…` |
| R10 | Cross-customer isolation | yes | `SC_S1_017_…` |
| R11 | PII never leaves the redacted surfaces | yes | `Service_surface_needs_no_token_and_never_returns_traveller_names` |
| R12 | Independent deadlines | yes | `SC_S1_019_…`, `Independent_deadlines_keep_their_owners_…` |

All twelve are proven. **Revision 3 adds two more that the repair will need**, recorded here as planned, not claimed:

| # | Scenario | State |
|---|---|---|
| R13 | Projection schema **3 → 4** deterministic rebuild after the repair | planned in `IMPLEMENTATION-PLAN.md` §A8; does not exist yet, and is **not** counted in the 12 |
| R14 | Accepted candidate **digest/schema transition** after `SalesContext` enters the canonical form | planned in §A3; not counted |

## G. Pack scenario reconciliation (SC-S1-001 … 021)

| Pack scenario | Named test | Status |
|---|---|---|
| SC-S1-001 … 012, 015 … 021 | yes (19) | covered |
| **SC-S1-013** Sign and settlement | **no** | arithmetic covered at domain level only; the settlement half the owner **kept in S1** has no scenario test and no representable attribution — scenarios 17–21 |
| **SC-S1-014** Allocation not money | **no** | owner-deferred with the allocation removal; INV-014 remains an S1-introduced invariant that is unproven by that decision |

## H. Primary disposition counts

These five are mutually exclusive and sum to **61**. No overlap arithmetic is applied.

| Primary disposition | Count | Scenarios |
|---|---|---|
| `TESTED` | **24** | 13–16, 22, 32–37, 39–43, 49, 51, 52, 56, 57, 59, 60, 61 |
| `UNTESTED` — representable, no test asserts the scenario's own claim | **24** | 1–5, 7, 10, 23–31, 44–48, 50, 53, 58 |
| `UNSUPPORTED` — cannot be represented today and should be | **12** | 8, 9, 11, 12 (parties) · 17–21 (settlement) · 38 (component totals) · 54, 55 (offerId) |
| `DEFERRED` — content belongs to a later slice | **1** | 6 |
| `BLOCKED` — no primary row; see the secondary table | **0** | — |
| **Total business/domain** | **61** | |

| Population | Count | Proven |
|---|---|---|
| Business/domain scenarios | 61 | 24 |
| Reliability scenarios (R1–R12) | 12 | 12 |
| **Total** | **73** | **36** |

Revision 1 counted 43 scenarios (31 business/domain + 12 reliability) and proved 24. Like for like, the business/domain population grew **31 → 61**. The `UNSUPPORTED` count grew 2 → 12; that is coverage revision 1 never attempted, not new breakage. `TESTED` fell 26 → 24 because revision 3 applies the stricter rule above to scenarios 25, 27, 44 and 58 — no test was removed or weakened.

## I. Secondary tags (never added into the primary totals)

Four facts are blocked by an owner contract. None of them is a standalone scenario; each is a blocked **fact inside** a scenario that has its own primary disposition.

| Secondary tag | Blocked fact | Host scenario(s) | Authority |
|---|---|---|---|
| `BLOCKED_OWNER_CONTRACT` | infant without a seat | 44 | BD-002 |
| `BLOCKED_OWNER_CONTRACT` | AirOffer `Flight.Stop` / `Leg.Stop` | 49 | `OD-P-12`, handoff `OR-002` |
| `BLOCKED_OWNER_CONTRACT` | bound `direction` vocabulary | 47, 48 | `OD-P-12`, handoff `OR-002` |
| `BLOCKED_OWNER_CONTRACT` | root `journeyType` vocabulary | 48, 51 | `OD-P-12`, handoff `OR-002` |

Revision 2 counted these four as a sixth primary bucket, which is why its columns did not sum. They are evidence-preserved and uninterpreted today; the wire audit records them as preserved-but-unmapped, not lost.

| Secondary tag | Scenarios | Meaning |
|---|---|---|
| `S6_CONTENT` | 2, 4, 5, 6 | the S1 **shape** holds; the product content arrives with S6 |
| `PARTIALLY_TESTED` | 10, 25, 27, 44, 58 | an adjacent rule is tested; the scenario's own claim is not |
| `PUBLIC_CONTRACT_IMPACT` | 12 | closing it changes `OrderDto`, not only the domain |
