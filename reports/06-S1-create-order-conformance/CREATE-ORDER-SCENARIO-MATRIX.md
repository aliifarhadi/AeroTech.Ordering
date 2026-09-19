# Create-Order Scenario Matrix

Stage: 06-S1-create-order-conformance · 2026-09-19 · HEAD `e861711`

"Representable" = the **domain model and canonical candidate** can hold the facts without loss or invention.
"Executable" = an S1 caller can actually produce this outcome today through the real path.
"Correctly rejected" = where the scenario is *not* supported, the system fails closed with a named error rather than accepting a distorted record.

Stage names follow `VERTICAL-SLICE-PLAN.md`.

---

## A. Core air sale

| # | Scenario | Industry reason | Required canonical facts | Representable? | S1 executable? | Correctly rejected if unsupported? | Existing test | Gap | Required stage |
|---|---|---|---|---|---|---|---|---|---|
| 1 | One adult, one one-way flight | The irreducible sale | traveler, journey, segment, air service, item, 2 lines, total | yes | yes | n/a | `SC_S1_002_…`, `One_traveler_one_segment_package_creates_one_item_one_service_total_120_versions_1` | none | S1 |
| 2 | Two adults, same itinerary | Most common family/business booking | 2 travelers, 1 item, 2 services per segment | yes | yes | n/a | `SC_S1_008_source_priced_package_persists_one_item_with_four_services` | none | S1 |
| 3 | Adult + child | Different PTC in one Order | `PassengerTypeCode` per traveler, per-PTC pricing group when supplied | yes | yes | n/a | `Incomplete_or_inconsistent_traveler_binding_is_rejected` (PTC agreement), `Infant_guardian_is_linked_to_the_same_order_traveler` | **no CHD-specific test**; INF is refused outright (BD-002) | S1 for CHD; INF blocked |
| 4 | Round trip as one RT pricing unit | The fare is coupled across both bounds | 2 journeys, 1 unit typed `RoundTrip`, component covering both | yes | yes (reference source) | n/a | `SC_S1_009_…` | AirOffer never supplies a canonical unit type — stays `Unspecified` | S1 |
| 5 | Same itinerary as two one-way units | Different fare construction, same journey | 2 units, each covering one bound | yes | yes (reference source) | n/a | `SC_S1_009_…` | none | S1 |
| 6 | Multi-city | Three or more bounds | ≥3 journeys with sequence and origin/destination | yes | yes | n/a | **none** | representable and untested | S1 |
| 7 | Connecting itinerary | Two segments in one bound | 2 segments sharing a `JourneyId` | yes | yes | n/a | **none directly** (`RoundTrip` helper builds 2 journeys, not 2 segments in 1) | representable and untested | S1 |
| 8 | Technical stop — one segment, many legs | Legs must not multiply services or coupons | 1 segment, N legs, 1 air service | yes | yes | n/a | `SC_S1_007_…`, `Technical_stop_keeps_one_service_and_two_legs` | none | S1 |
| 9 | Two travelers on different fares | Non-homogeneous Order (IATA I5) | 2 pricing groups or 2 units, distinct components | yes | yes (reference source) | n/a | **none** | representable and untested | S1 |
| 10 | Opaque fare construction | The source proves no coupling | construction with units and components, **zero** groups and coverage | yes | yes | n/a | `SC_S1_010_…`, `Opaque_context_is_retained_without_invented_links`, `Opaque_construction_cannot_claim_component_links` | none | S1 |

## B. Commercial / pricing

| # | Scenario | Industry reason | Required canonical facts | Representable? | S1 executable? | Correctly rejected? | Existing test | Gap | Stage |
|---|---|---|---|---|---|---|---|---|---|
| 11 | Fare + several taxes | Every real ticket | N `Tax` lines, each a distinct occurrence | yes | yes | n/a | `SC_S1_002_…` (1 tax); live fixture has more | representable and thinly tested | S1 |
| 12 | Fare + carrier surcharge + fee | YQ/YR plus service fee | `CarrierSurcharge` and `Fee` components | yes | yes | n/a | **none** (mapper maps categories 2 and 3; no test asserts them) | representable and untested | S1 |
| 13 | Discount credit | A concession reduces what the customer owes | `Discount` with `Credit` direction | yes | yes | n/a | `PricingArithmeticTests` theory rows | domain-level only, never persisted | S1 |
| 14 | Markup | Agency or airline uplift | `Markup` `Debit` | yes | yes | n/a | **none** | representable and untested | S1 |
| 15 | **Settlement-only agency commission** | Airline↔seller settlement is a different exchange from the customer payment (IATA I3) | `Commission` + `SettlementOnly` + **party** + **category** + currency, excluded from `CustomerTotal` | **partly — party and category are not representable** | **no** | no — a settlement line with no party or category is accepted | `Sale_with_settlement_commission_charges_the_customer_405`, `Commission_as_settlement_only_is_allowed_and_as_customer_charge_is_not` (both pure domain arithmetic) | **critical: DOMAIN/03 §13 requires party/category; no field, no constraint, no end-to-end test; no `SC_S1_013` test exists** | S1 (owner kept this half) |
| 16 | Multi-currency original / sale | Original 100 USD, sale 92 EUR | two `Money` values, never summed, conversion evidence | yes | yes | yes — two values in one currency is rejected | `SC_S1_012_…`, `Equivalent_amount_is_used_as_sale_value_and_the_original_is_retained`, `Applied_conversion_evidence_is_preserved_without_recomputing_money`, `Two_values_in_one_currency_are_a_contract_mismatch` | none | S1 |
| 17 | Source percentage row | The source priced a percentage | `CalculationKind = Percentage`, value not recomputed | yes | yes | yes — a percentage row without a sale-currency valuation is rejected | `Percentage_order_charge_is_valued_from_its_sale_currency_equivalent`, `Percentage_rows_record_their_calculation_kind_without_interpreting_the_rate` | the numeric basis is deliberately uninterpreted (OD-P-18) | S1 |
| 18 | Repeated tax codes staying distinct | Two YQ rows are two charges | unique `(PriceChangeSetId, CandidateLineRef)` on the occurrence path | yes | yes | yes | **no dedicated test**, but the unique index and the occurrence-path ref enforce it | representable and untested | S1 |
| 19 | Order-level charge | A charge on the package, not a coupon | line with `PricingBasisType.OrderItem` on the package | yes | yes | n/a | `Percentage_order_charge_…` | none | S1 |
| 20 | Source total mismatch → reject | Never invent a rounding fee | `ContractMismatch`, nothing persisted | n/a | n/a | **yes** | `SC_S1_015_hierarchy_total_mismatch_is_contract_mismatch_and_persists_nothing` | none | S1 |
| 21 | Unknown pricing category → reject | Never silently default money | `ContractMismatch` | n/a | n/a | **yes** | `Observed_wire_violations_fail_mapping_instead_of_guessing` | none | S1 |

## C. Product and service context

| # | Scenario | Industry reason | Required canonical facts | Representable? | S1 executable? | Correctly rejected? | Existing test | Gap | Stage |
|---|---|---|---|---|---|---|---|---|---|
| 22 | Baggage allowance included in the air product | What the fare includes | `CheckedBaggage` / `CabinBaggage` on the air detail | yes | yes | yes — weight without a unit is rejected | `Every_supplied_air_offer_fact_survives_…` | none | S1 |
| 23 | Refundable / changeable / upgradable flags | Display terms at sale | exact per-service flags + item summary | yes | yes | n/a | `Every_supplied_air_offer_fact_survives_…` | mixed-coupon `Conditional` case untested end to end | S1 |
| 24 | Fare basis / family / RBD / cabin / booking class | The filed fare and what was sold | component fields + air detail fields | yes | yes | n/a | `Every_supplied_air_offer_fact_survives_…` | none | S1 |
| 25 | Source product / brand facts when supplied | Brand-fare retailing | `ProductSnapshot` code/name/brand/version | yes | structurally yes; AirOffer supplies none | n/a | `Every_supplied_air_offer_fact_survives_…` asserts they are `NULL` | none — absence is honest | S1 structure, S6 content |
| 26 | **Separate paid seat product** | A paid seat is an independently sold, independently serviced product | own `OrderService` of type Seat with its own typed detail and beneficiary | **not yet — no Seat detail type**, and correctly **not** on `AirTransportDetail` | no | yes — an unregistered schema is `UnsupportedCapability` | `SC_S1_021_…`, `Unregistered_service_type_is_unsupported_before_acceptance` | none — this is the correct S1 boundary | **S6** |
| 27 | Paid baggage / meal / lounge as independent services | Ancillary revenue | typed details per service type | not yet | no | yes (same gate) | same | none | **S6** |
| 28 | Service dependency semantics | A seat cannot outlive its air service | dependency edge with no `RequiresAirService` cycle | not yet | no | n/a | **none** | design has the home (`OrderServiceCoverage` pattern); no cycle rule yet | **S6** |

## D. Sales and distribution

| # | Scenario | Industry reason | Required canonical facts | Representable? | S1 executable? | Correctly rejected? | Existing test | Gap | Stage |
|---|---|---|---|---|---|---|---|---|---|
| 29 | Airline direct / backoffice | Airline's own staff sells | `Channel = BackOffice`, office from token | yes | yes | yes — office must match the token | `Backoffice_sells_a_flight_offer_…`, `Backoffice_selling_office_must_match_the_token_office` | none | S1 |
| 30 | OTA | Online agency sells for its own customer | `Channel`, customer from token | yes | yes | yes | `Ota_sells_for_its_token_customer_and_cannot_read_another_customer_order` | none | S1 |
| 31 | Travel agency / office panel | Agency staff sells for the agency's customer | agency-scoped customer | yes | yes | yes | `OtaPanel_sells_for_the_customer_of_its_travel_agency` | none | S1 |
| 32 | Corporate financial customer | The payer is not the traveler | `FinancialCustomerId` distinct from travelers | yes | yes | yes | `An_unknown_or_inactive_customer_is_rejected` | no explicit corporate-payer test | S1 |
| 33 | Same accepted order across authorized surfaces | One commercial truth, four doors | shared handler, shared receipt scope, per-surface read scope | yes | yes | yes | `Current_controller_surface_is_exactly_the_characterized_inventory`, `The_route_surface_is_bound_to_the_token_surface_…` | none | S1 |

## E. Reliability and truth

| # | Scenario | Industry reason | Required canonical facts | Representable? | S1 executable? | Correctly rejected? | Existing test | Gap | Stage |
|---|---|---|---|---|---|---|---|---|---|
| 34 | Same idempotency key replay | Networks retry | receipt returns the original result | yes | yes | n/a | `SC_S1_003_…`, `The_same_idempotency_key_returns_the_same_order`, `Same_key_and_hash_replays_and_a_different_hash_conflicts` | none | S1 |
| 35 | Conflicting replay | Same key, different request | 409, nothing changed | yes | yes | **yes** | `SC_S1_004_…` | none | S1 |
| 36 | Concurrency on the same source | Two clients accept one snapshot | exactly one Order | yes | yes | **yes** | `SC_S1_005_…`, `A_second_consumption_of_the_same_accepted_source_is_a_conflict` | none | S1 |
| 37 | Owner offline after acceptance | The sale must survive the supplier | no owner call on read or replay | yes | yes | n/a | `SC_S1_003_…`, `SC_S1_020_…`, `Every_supplied_air_offer_fact_survives_…` (call count) | none | S1 |
| 38 | Projection rebuild | The read model is derived, never authoritative | byte-identical rebuild, no new commercial version | yes | yes | yes — a stale rebuild conflicts | `SC_S1_020_…`, `Projection_rebuild_is_byte_identical_…`, `Rebuild_never_overwrites_a_newer_projection_revision` | none | S1 |
| 39 | Migration upgrade | Accepted history survives schema change | no row loss, honest unknowns | yes | yes | yes — disagreeing flight facts abort the migration | `S1_domain_parity_upgrade_preserves_accepted_rows_without_inventing_facts`, `S1_migration_upgrades_a_b0_database_…` | none | S1 |
| 40 | **Response `offerId` mismatch** | The response must price the offer that was asked for | reject before anything is persisted | n/a | n/a | **NO — silently accepted** | **none** | **critical defect**, see `AIROFFER-WIRE-LOSS-AUDIT.md` §9 | S1 |
| 41 | Missing optional source field | Real payloads omit optionals | `NULL`, never a placeholder | yes | yes | n/a | `Every_supplied_air_offer_fact_survives_…` asserts `NULL`s; `Sandbox_candidate_keeps_not_supplied_validity_…` | none | S1 |
| 42 | Unknown owner vocabulary | A new enum value must not be guessed | explicit failure or raw-only preservation | yes | yes | **yes** | `SC_S1_021_…`, `Unregistered_extension_type_is_an_unsupported_capability`, `Observed_wire_violations_fail_mapping_instead_of_guessing` | `Stop`, `direction`, `journeyType` are preserved raw and unmapped by decision | S1 |
| 43 | Source identity and occurrence preservation | Trace any accepted fact back to the payload | occurrence path, source refs, raw evidence | yes | yes | n/a | `Every_supplied_air_offer_fact_survives_…`, `Digest_binds_the_authorized_scope_as_well_as_the_candidate` | coupon id and traveller index live only in raw evidence (justified) | S1 |

## F. Pack scenario reconciliation (SC-S1-001 … 021)

| Pack scenario | Named test present? | Status |
|---|---|---|
| SC-S1-001 … SC-S1-012 | yes | covered |
| **SC-S1-013** Sign and settlement | **no** | arithmetic covered by `PricingArithmeticTests` at domain level only. The **reversal** half is owner-deferred; the **settlement/commission** half the owner kept in S1 has **no scenario test and no representable party/category** |
| **SC-S1-014** Allocation not money | **no** | owner-deferred with the allocation removal; INV-014 is unproven in S1 by that decision |
| SC-S1-015 … SC-S1-021 | yes | covered |

19 of 21 named. One absence is owner-ratified (014); the other (013) is only half-ratified and is a real gap.

## G. Counts

| Outcome | Count |
|---|---|
| Representable **and** tested | 24 |
| Representable but untested | 9 (6, 7, 9, 11, 12, 14, 18, 23-mixed, 32) |
| Correctly deferred to a later stage | 4 (26, 27, 28, and the S6/S9 content half of 25) |
| Incorrectly unsupported | **2** — 15 (settlement commission party/category) and 40 (offerId mismatch) |
| Blocked by owner semantics | 4 (3-INF under BD-002, plus `Stop`, `direction`, `journeyType` under `OD-P-12`/`OR-002`) |

No scenario in this suite requires a domain redesign. The two incorrect rows are a missing validation and a missing pair of fields.
