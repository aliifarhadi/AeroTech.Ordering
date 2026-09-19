# Create-Order Scenario Matrix

Stage: 06-S1-create-order-conformance · **revision 2**, expanded after independent review · 2026-09-19 · HEAD `506ccee`

Revision 1 had 43 scenarios and proved 24. It did not probe the item-composition boundary, the party/role boundary, or several pricing boundaries at all. Revision 2 carries **61** scenarios. The count is not the goal — boundary coverage is.

Columns: **Repr.** = the domain model and canonical candidate can hold the facts without loss or invention. **Exec.** = an S1 caller can produce this outcome today through the real path. **Closed** = where unsupported, the system fails closed with a named error.

Test layers: `D` domain, `A` application, `S` SQL, `H` HTTP/API, `M` migration.

---

## A. Composition and item boundary *(new in revision 2)*

| # | Scenario | Industry reason | Required canonical facts | Repr. | Exec. | Closed | Layer | Existing / new test | Gap | Stage |
|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Two independently priced `OrderItem`s in one Order | AIDM M1: an Order has many individually priced items | 2 items, disjoint service ownership, 2 accepted totals | yes | yes (reference source) | n/a | D,S | **new** | representable, untested | S1 |
| 2 | One `OfferPackage` item + one independent product item | Air plus a separately sold ancillary | mixed `OrderItemKind` in one Order | yes | yes (reference source) | n/a | D,S | **new** | representable, untested | S1 shape, S6 content |
| 3 | Fee-only `MonetaryCharge` item with **zero** services | `DOMAIN/01` §3 explicit fee-only exception | item with no services, line with `PricingBasisType.OrderItem` | yes | yes (reference source) | yes — a `MonetaryCharge` that owns services is rejected | D,S | **new** (`CandidateValidator.EnsureItems` rule is untested) | representable, untested | S1 |
| 4 | `Included` service, no synthetic zero line | `DOMAIN/02` §21 | `PriceTreatment.Included`, no zero-amount line | yes | yes (reference source) | n/a | D,S | **new** | representable, untested | S1 shape, S6 content |
| 5 | `Complimentary` service, no synthetic zero line | same | `PriceTreatment.Complimentary` | yes | yes (reference source) | n/a | D,S | **new** | representable, untested | S1 shape, S6 content |
| 6 | Same traveler+segment in two legitimate independent items | A seat product and the air service cover the same segment | two items, two services, air-uniqueness rule not violated | yes — the uniqueness rule is scoped to **air** services | no (needs a second service type) | yes for two *air* services | D | **new** | representable, untested | S1 rule, S6 content |
| 7 | Source-proven fare construction spanning more than one item | `DOMAIN/03` §47 "may cross items" | `FareConstructionItem` rows for 2 items | yes | yes (reference source) | n/a | D,S | **new** | representable, untested | S1 |

## B. Sales, parties and roles *(new in revision 2)*

| # | Scenario | Industry reason | Required canonical facts | Repr. | Exec. | Closed | Layer | Test | Gap | Stage |
|---|---|---|---|---|---|---|---|---|---|---|
| 8 | OtaPanel Order retains historical agency/seller identity after ReferenceData changes | AIDM M4 Seller is a distribution-chain role; `DOMAIN/04` §5 snapshots are historical | accepted seller org id on the Order | **NO** — `TravelAgencyId` is resolved away; it survives only inside the `agency:{id}` fragment of the `CallerScope` string | no | no | S,H | **new** | **blocker** — see `OD-CLOSE-05` | S1 |
| 9 | FinancialCustomer, Buyer, Seller and Actor proven not conflated | `DOMAIN/04` §5 lists six distinct references | four distinguishable accepted facts | **NO** — `BuyerActorId` is the actor, and there is no buyer or seller | no | no | D,S | **new** | **blocker** | S1 |
| 10 | Corporate financial customer distinct from traveler and actor | The payer is not the passenger | customer id ≠ any traveler; actor is a third identity | partly — customer and actor exist, buyer does not | yes for customer/actor | yes (inactive customer rejected) | H | `An_unknown_or_inactive_customer_is_rejected` + **new** | partial | S1 |
| 11 | PartnerAPI caller identity is not automatically buyer or seller | AIDM M4 role ≠ organisation type | partner profile stored as what it is | **NO** — stored in `BuyerActorId` | no | no | D | **new** | **blocker** | S1 |
| 12 | Backoffice office and OtaPanel office are not the same namespace | two office identity spaces | office value carries its namespace | **NO** — one untyped `long` | no | no | D,S | **new** | **blocker** (part of `OD-CLOSE-05`) | S1 |
| 13 | Airline direct / backoffice sale | airline's own staff | channel, office from token | yes | yes | yes | H | `Backoffice_sells_a_flight_offer_…`, `Backoffice_selling_office_must_match_the_token_office` | none | S1 |
| 14 | OTA sale for its token customer | online agency | customer from token | yes | yes | yes | H | `Ota_sells_for_its_token_customer_and_cannot_read_another_customer_order` | none | S1 |
| 15 | Agency panel sale for the agency's customer | agency staff | agency-scoped customer | yes | yes | yes | H | `OtaPanel_sells_for_the_customer_of_its_travel_agency` | none | S1 |
| 16 | Same accepted order across authorized surfaces | one truth, four doors | shared handler, per-surface read scope | yes | yes | yes | H | `Current_controller_surface_is_exactly_the_characterized_inventory`, `The_route_surface_is_bound_to_the_token_surface_…` | none | S1 |

## C. Pricing and settlement

| # | Scenario | Industry reason | Required canonical facts | Repr. | Exec. | Closed | Layer | Test | Gap | Stage |
|---|---|---|---|---|---|---|---|---|---|---|
| 17 | **Amount-based SettlementOnly commission, candidate → domain → SQL → projection → rebuild** | AIDM M3 "remuneration … paid to an agent"; owner kept this half of SC-S1-013 in S1 | `Commission` + `SettlementOnly` + **PartyRef** + **CategoryCode**, excluded from `CustomerTotal` | **NO** — no attribution members | no | no | D,A,S,H | `Sale_with_settlement_commission_charges_the_customer_405` (domain arithmetic only) | **blocker**; no `SC_S1_013` test exists | S1 |
| 18 | SettlementOnly with **no** attribution → reject | `DOMAIN/03` §13 requires party/category | domain + SQL both refuse | **NO** | no | **no — currently accepted** | D,S | **new** | **blocker** | S1 |
| 19 | SettlementOnly with only one of party/category → reject | half-populated attribution is meaningless | SQL constraint prevents it | **NO** | no | no | D,S | **new** | **blocker** | S1 |
| 20 | Settlement category preserved losslessly, never interpreted | AIDM models it as a **Code**, not an enum | source string round-trips | **NO** | no | n/a | D,S | **new** | **blocker** | S1 |
| 21 | Multiple settlement counterparties stay distinct | two agents on one sale | two lines, two `PartyRef`s | **NO** | no | n/a | D,S | **new** (reference fixture) | **blocker** | S1 |
| 22 | Commission + `CustomerBalance` → reject | INV-013 | domain + SQL check | yes | n/a | **yes** | D,S | `Commission_as_settlement_only_is_allowed_…`, `Commission_cannot_be_a_customer_charge`, `CK_PricingLines_CommissionNotCustomer` | none | S1 |
| 23 | `Informational` line never affects `CustomerTotal` | Effect is not a second sign | filter excludes it | yes | yes | n/a | D | **new** (`PricingArithmetic` filters it; no test asserts it) | representable, untested | S1 |
| 24 | `Other` component only `Informational` | `DOMAIN/03` §15 | matrix rejects other effects | yes | n/a | **yes** | D,S | **new** (rule exists in `PricingLineMatrix` and SQL; untested) | representable, untested | S1 |
| 25 | Fare + several taxes | every real ticket | N distinct `Tax` lines | yes | yes | n/a | S | `SC_S1_002_…` (one tax) | thinly tested | S1 |
| 26 | Carrier surcharge + fee persisted | YQ/YR plus service fee | categories 2 and 3 mapped | yes | yes | n/a | S | **new** | representable, untested | S1 |
| 27 | Discount credit persisted | a concession | `Discount` + `Credit` | yes | yes | n/a | S | `PricingArithmeticTests` theory only | domain-only | S1 |
| 28 | Markup persisted | agency or airline uplift | `Markup` + `Debit` | yes | yes | n/a | S | **new** | representable, untested | S1 |
| 29 | `Adjustment` debit **and** credit both allowed | `NormalCustomerDirection` returns null for Adjustment | either direction accepted | yes | yes | n/a | D | **new** | representable, untested | S1 |
| 30 | Repeated identical tax codes remain separate occurrences | two YQ rows are two charges | unique `(PriceChangeSetId, CandidateLineRef)` on the occurrence path | yes | yes | yes | S | **new** | representable, untested | S1 |
| 31 | Zero-value line accepted | a zero fee is not negative | `EnsureMagnitude` rejects only negatives | yes | yes | n/a | D | **new** | representable, untested | S1 |
| 32 | Multi-currency original / sale | 100 USD original, 92 EUR sale | two `Money` values, never summed | yes | yes | yes | D,A,S | `SC_S1_012_…`, `Equivalent_amount_…`, `Two_values_in_one_currency_are_a_contract_mismatch` | none | S1 |
| 33 | Applied conversion evidence preserved, money not recomputed | rate provenance | rate, decimal places, rounding token | yes | yes | n/a | S | `Applied_conversion_evidence_is_preserved_without_recomputing_money` | none | S1 |
| 34 | Source percentage row | the source priced a percentage | `CalculationKind = Percentage`, value untouched | yes | yes | yes | A,S | `Percentage_rows_record_their_calculation_kind_…` | numeric basis deliberately uninterpreted | S1 |
| 35 | 3-decimal currency keeps its scale | BHD/KWD/TND | exact decimal round-trip | yes | yes | yes | S | `Three_and_zero_decimal_currencies_keep_their_scale` | none | S1 |
| 36 | Zero-decimal currency keeps its scale | JPY/KRW | same | yes | yes | yes | S | same | none | S1 |
| 37 | Precision / overflow boundary rejected, not rounded | INV-059 | amount beyond scale refused | yes | n/a | **yes** | D,S | `Amount_beyond_storage_scale_is_rejected_not_rounded`, `Amount_rate_and_quantity_round_trip_unchanged_on_sql_server` | none | S1 |
| 38 | **Component-total reconciliation** | `DOMAIN/01` §2 "complete current component totals"; AIDM M5 Price has Base/Total plus component associations | persisted totals equal the sum of their lines | **NO** — no component totals exist | no | n/a | D,S | **new** | **domain-shape gap** (`OD-CLOSE-06`) | S1 |
| 39 | Order-level charge | a charge on the package | line with item basis | yes | yes | n/a | S | `Percentage_order_charge_…` | none | S1 |
| 40 | Source total mismatch → reject | never invent a rounding fee | `ContractMismatch`, nothing persisted | n/a | n/a | **yes** | A,S | `SC_S1_015_…` | none | S1 |
| 41 | Group line extended once, not multiplied | INV-012, SC-S1-011 | store 200 not 400; group quantity kept | yes | yes | n/a | D,S | `SC_S1_011_…`, `Extended_group_line_is_stored_once_with_quantity_metadata` | none | S1 |

## D. Composition and itinerary

| # | Scenario | Required canonical facts | Repr. | Exec. | Closed | Layer | Test | Gap | Stage |
|---|---|---|---|---|---|---|---|---|---|
| 42 | One adult, one one-way flight | the irreducible sale | yes | yes | n/a | D,A,S,H | `SC_S1_002_…` | none | S1 |
| 43 | Multi-traveler package | 2 travelers, 1 item, 4 services | yes | yes | n/a | D,S | `SC_S1_008_…` | none | S1 |
| 44 | Adult + child | distinct PTC | yes | yes | yes (PTC must agree with the binding) | D | `Incomplete_or_inconsistent_traveler_binding_is_rejected` | no CHD-specific test | S1 |
| 45 | Two travelers with different fare groups | non-homogeneous Order (AIDM M1/I5) | yes | yes (reference source) | n/a | D,S | **new** | representable, untested | S1 |
| 46 | Different itineraries for travelers in one Order | each traveler's services cover different segments | yes | yes (reference source) | n/a | D,S | **new** | representable, untested | S1 |
| 47 | Connection — two passenger segments under one journey | 2 segments sharing a `JourneyId` | yes | yes | n/a | D,S | **new** | representable, untested | S1 |
| 48 | Multi-city — three or more bounds | ≥3 journeys with sequence | yes | yes | n/a | D,S | **new** | representable, untested | S1 |
| 49 | Technical stop — one segment, many legs | legs never multiply services | yes | yes | n/a | D,S | `SC_S1_007_…`, `Technical_stop_keeps_one_service_and_two_legs` | none | S1 |
| 50 | Same flight number on two distinct dated segments does not collapse | identity is `{BoundId}\|{FlightId}`, not the number | yes | yes | yes — a duplicate segment ref is rejected | D,S | **new** | representable, untested | S1 |
| 51 | Round trip as one RT unit vs two OW units | source fare construction retained as supplied | yes | yes (reference source) | n/a | D,S | `SC_S1_009_…` | none | S1 |
| 52 | Opaque fare construction | zero groups, zero coverage rows | yes | yes | yes | D,S | `SC_S1_010_…`, `Opaque_construction_cannot_claim_component_links` | none | S1 |
| 53 | OpenAir structural scenario under a certified reference profile | `SegmentKind.OpenAir`, no dated flight | yes — validator has the rule | yes (reference source) | yes — an open segment carrying a dated flight is rejected | D | **new** | representable, untested | S1 |

## E. Contract fail-closed

| # | Scenario | Required outcome | Closed today? | Layer | Test | Gap | Stage |
|---|---|---|---|---|---|---|---|
| 54 | Response `offerId` **missing** | `ContractMismatch`, nothing persisted | **NO** | A | **new** | **blocker** | S1 |
| 55 | Response `offerId` **mismatched** | `ContractMismatch`, nothing persisted | **NO** | A | **new** | **blocker** | S1 |
| 56 | Unknown pricing category | `ContractMismatch` | yes | A | `Observed_wire_violations_fail_mapping_instead_of_guessing` | none | S1 |
| 57 | Unknown registered detail schema / version | `UnsupportedCapability` before any Order | yes | D,A | `SC_S1_021_…`, `Unregistered_detail_schema_version_…`, `Unregistered_service_type_…` | none | S1 |
| 58 | Duplicate traveler+segment air service occurrence | rejected, never merged | yes | D | `Air_service_with_two_segments_is_rejected` + the air-coverage rule | the *duplicate-occurrence* case is untested | S1 |
| 59 | Malformed source hierarchy totals | `ContractMismatch` | yes | A,S | `SC_S1_015_…` | none | S1 |
| 60 | Source currency / value inconsistency | `ContractMismatch` | yes | D | `Two_values_in_one_currency_are_a_contract_mismatch` | none | S1 |
| 61 | Invalid traveler binding | 422 before any Order | yes | D,A,H | `SC_S1_016_…` | none | S1 |

## F. Reliability *(unchanged from revision 1, all green)*

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

## G. Pack scenario reconciliation (SC-S1-001 … 021)

| Pack scenario | Named test | Status |
|---|---|---|
| SC-S1-001 … 012, 015 … 021 | yes (19) | covered |
| **SC-S1-013** Sign and settlement | **no** | arithmetic covered at domain level only; the settlement half the owner **kept in S1** has no scenario test and no representable attribution — scenario 17 above |
| **SC-S1-014** Allocation not money | **no** | owner-deferred with the allocation removal; INV-014 remains an S1-introduced invariant that is unproven by that decision |

## H. Counts

| Outcome | Rev 1 | Rev 2 |
|---|---|---|
| Representable **and** tested | 24 | **26** |
| Representable but untested | 9 | **24** |
| Correctly deferred to a later stage | 4 | **4** |
| **Incorrectly unsupported** | 2 | **9** — 8, 9, 11, 12, 17, 18, 19, 20, 21 minus overlap ⇒ items 8/9/11/12 (parties), 17–21 (settlement), 38 (component totals), 54/55 (offerId) |
| Blocked by owner semantics | 4 | **4** — infant (BD-002), `Stop`, bound `direction`, root `journeyType` |

Precisely: **11 scenarios are incorrectly unsupported** — 4 party/role (8, 9, 11, 12), 5 settlement (17–21), 1 component totals (38), and 2 contract fail-closed (54, 55) — against 2 in revision 1. The jump is not new breakage; it is coverage revision 1 never attempted.
