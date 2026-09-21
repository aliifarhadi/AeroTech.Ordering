# Scenario closure matrix

Stage: 08-S1-authoritative-final-closure · 2026-09-21 · branch `k8s-stg`

**This file supersedes the dispositions in `reports/06-S1-create-order-conformance/CREATE-ORDER-SCENARIO-MATRIX.md`.**
That file keeps its scenario definitions, its industry reasoning and its historical audit text; the *Primary* column
there is stale from this date on, and this matrix is the current answer. Nothing in stage 06 was deleted.

Populations, unchanged from stage 06 revision 3: **61 business/domain scenarios (1–61)** and **12 reliability
scenarios (R1–R12)**, counted separately and never mixed.

A scenario is `TESTED` only when a named test asserts that scenario's own claim. "A nearby rule is tested" is not
tested. A scenario is `DEFERRED` when the fact it needs cannot exist in S1 and the stage that introduces it is named.
`NOT_APPLICABLE` means the subject no longer exists in the model.

## 1. Result

| Disposition | Stage 06 rev 3 | This stage | Change |
|---|---|---|---|
| `TESTED` | 24 | **56** | +32 |
| `UNTESTED` | 22 | **0** | −22 |
| `UNSUPPORTED` | 12 | **0** | −12 |
| `DEFERRED` | 1 | **3** | +2 |
| `NOT_APPLICABLE` | 2 | **2** | 0 |
| total | 61 | **61** | |

| Reliability | Stage 06 rev 3 | This stage |
|---|---|---|
| `TESTED` | 12 (4 naming tests that no longer exist) | **12** (all names verified live) |

No scenario is left as `UNTESTED` or `UNSUPPORTED`.

## 2. Business and domain scenarios

`D` = `AeroTech.Ordering.Domain.Tests`, `P` = `AeroTech.Ordering.Persistence.Tests`.

### A. Composition and item boundary

| # | Scenario | Now | Test |
|---|---|---|---|
| 1 | Two independently priced `OrderItem`s in one Order | `TESTED` | D `CompositionScenarioTests.SC_01_two_independently_priced_items_keep_disjoint_services_and_totals`, D `SC_01_one_service_cannot_belong_to_two_items`, P `ScenarioClosurePersistenceTests.SC_01_two_independently_priced_items_survive_acceptance_and_sql` |
| 2 | `OfferPackage` item + independent product item | `TESTED` | D `SC_02_an_offer_package_and_a_separate_product_item_coexist_in_one_order`, P `SC_02_an_offer_package_and_a_product_item_keep_their_kinds_in_sql` |
| 3 | Fee-only `MonetaryCharge` item with zero services | `TESTED` | D `SC_03_a_fee_only_monetary_charge_item_owns_no_service`, D `SC_03_a_monetary_charge_item_that_owns_a_service_is_rejected`, D `SC_03_a_non_charge_item_without_a_service_is_rejected`, P `SC_03_a_fee_only_item_is_persisted_with_no_service_and_its_own_total` |
| 4 | `Included` service, no synthetic zero line | `DEFERRED` | `PriceTreatment` is not an S1 fact — ERRATA §1.2. The stage that introduces ancillaries or bundles owns this scenario. Was `UNTESTED`; "untested" was the wrong word for a field that does not exist. |
| 5 | `Complimentary` service, no synthetic zero line | `DEFERRED` | Same as 4. |
| 6 | Same traveller+segment in two independent items | `DEFERRED` | S6 — needs a second service type. Unchanged. |
| 7 | Fare construction spanning more than one item | `TESTED` | D `SC_07_a_source_proven_fare_construction_may_span_two_items`, P `SC_07_a_construction_spanning_two_items_keeps_both_item_rows` |

### B. Sales, parties and roles

| # | Scenario | Now | Test |
|---|---|---|---|
| 8 | OtaPanel Order retains historical agency identity after a ReferenceData change | `TESTED` | P `SalesProvenanceAndOfficeContractTests.An_agency_panel_sale_persists_the_travel_agency_as_seller_and_survives_a_reference_data_change` — repoints the agency in ReferenceData and re-reads the Order |
| 9 | FinancialCustomer, Buyer, Seller and Actor proven not conflated | `TESTED` | D `SalesProvenanceTests.Financial_customer_seller_and_actor_are_three_separate_facts`, D `A_seller_or_an_office_is_never_half_supplied`, P `ScenarioClosurePersistenceTests.SC_10_the_financial_customer_the_actor_and_the_traveller_stay_three_separate_facts_in_sql` |
| 10 | Corporate financial customer distinct from traveller and actor | `TESTED` | P `SC_10_…` (the distinctness assertion stage 06 said was missing), plus the pre-existing customer gate |
| 11 | PartnerAPI caller identity is not automatically buyer or seller | `TESTED` | D `SalesProvenanceTests.A_partner_api_profile_is_the_actor_and_is_not_the_seller` |
| 12 | Backoffice office and OtaPanel office are not the same namespace | `TESTED` | D `An_office_identifier_carries_the_namespace_it_belongs_to`, P `A_backoffice_sale_records_the_owner_airline_as_seller_in_its_own_office_namespace`, P `An_agency_office_is_never_returned_under_an_airline_office_label` |
| 13–16 | Channel sales and surface identity | `TESTED` | unchanged from stage 06 |

Rows 8–12 were `UNSUPPORTED` in stage 06 revision 3. They are not newly supported by this stage — the tests were
written during the R2 and post-R2 passes and the matrix was never updated. This row set is a **reconciliation**, not a
new capability; each test above was read before its row was changed.

### C. Pricing

| # | Scenario | Now | Test |
|---|---|---|---|
| 17 | Amount-based SettlementOnly commission end to end | `TESTED` | D `SettlementAttributionTests.SC_S1_013_amount_based_agency_commission_leaves_the_customer_total_at_405`, P `ComponentTotalPersistenceTests.A_settlement_attribution_survives_sql_the_projection_and_a_rebuild`, P `A_settlement_component_is_totalled_and_projected_but_stays_out_of_the_customer_total` |
| 18 | SettlementOnly with no attribution → reject | `TESTED` | D `A_settlement_only_line_without_attribution_is_rejected`; on SQL Server `CK_PricingLines_SettlementAttribution` |
| 19 | SettlementOnly with only one of party/category → reject | `TESTED` | D `A_settlement_only_line_with_only_one_attribution_member_cannot_be_expressed` |
| 20 | Settlement category preserved, never interpreted | `TESTED` | D `An_arbitrary_category_code_round_trips_without_interpretation` (theory) |
| 21 | Non-commission SettlementOnly (`Fee`/`Markup`) | `TESTED` | D `A_non_commission_settlement_line_proves_the_model_is_generic`, D `Two_settlement_counterparties_stay_distinct` |
| 22 | Commission + `CustomerBalance` → reject | `TESTED` | unchanged |
| 23 | `Informational` line never affects `CustomerTotal` | `TESTED` | D `CompositionScenarioTests.SC_23_an_informational_line_never_moves_the_customer_total` |
| 24 | `Other` component only `Informational` | `TESTED` | D `SC_24_an_other_component_is_only_informational` (2 cases); SQL `CK_PricingLines_OtherInformational` |
| 25 | Fare + several taxes | `TESTED` | unchanged — `MultipleTaxOccurrenceTests.Every_tax_occurrence_on_one_ticket_survives_acceptance_sql_and_rebuild` |
| 26 | Carrier surcharge + fee persisted | `TESTED` | P `SC_26_28_a_debit_component_is_persisted_as_its_own_line_and_component_total` (CarrierSurcharge, Fee cases) |
| 27 | Discount credit persisted | `TESTED` | P `SC_27_a_discount_credit_is_persisted_on_the_credit_side_of_its_component_total` |
| 28 | Markup persisted | `TESTED` | P `SC_26_28_…` (Markup case; Penalty is covered by the same theory) |
| 29 | `Adjustment` debit **and** credit both allowed | `TESTED` | D `SC_29_an_adjustment_may_be_a_debit_or_a_credit` (2 cases), P `SC_29_an_adjustment_debit_and_credit_are_kept_on_opposite_sides_of_one_component_total` |
| 30 | Repeated identical tax codes remain separate occurrences | `TESTED` | `Every_tax_occurrence_on_one_ticket_survives_acceptance_sql_and_rebuild` — four taxes, three codes, `AT` repeated at two occurrences under two references, nothing merged by code. This is exactly scenario 30's claim; stage 06 credited it to 25 only. |
| 31 | Zero-value line accepted | `TESTED` | D `SC_31_a_zero_value_line_is_accepted_and_recorded`, P `SC_31_a_zero_value_line_is_persisted_as_a_line_not_dropped` |
| 32–37 | Currency, conversion, percentage, scale, overflow | `TESTED` | unchanged |
| 38 | Component-total reconciliation, debit and credit sides kept apart | `TESTED` | P `ComponentTotalPersistenceTests.Component_totals_round_trip_through_sql_and_reconcile_with_the_committed_lines`, P `A_component_total_is_identified_by_order_component_and_effect_on_sql_server`, P `SC_29_an_adjustment_debit_and_credit_are_kept_on_opposite_sides_of_one_component_total` (25 debit / 10 credit / net 15 in one component) |
| 39, 40 | Order-level charge; source total mismatch | `TESTED` | unchanged |
| 41 | Group line extended once | `NOT_APPLICABLE` | unchanged — no group pricing in the S1 model |

### D. Travellers, journeys and itineraries

| # | Scenario | Now | Test |
|---|---|---|---|
| 42, 43 | One adult one-way; multi-traveller package | `TESTED` | unchanged |
| 44 | Adult + child | `TESTED` | D `SC_44_an_adult_and_a_child_are_two_travellers_with_their_own_passenger_type_codes` |
| 45 | Two travellers with different fare groups | `TESTED` | D `SC_45_two_travellers_may_carry_different_fare_groups_on_the_same_bound` |
| 46 | Different itineraries for travellers in one Order | `TESTED` | D `SC_46_two_travellers_may_fly_different_itineraries_inside_one_order` |
| 47 | Connection — two passenger segments under one journey | `TESTED` | D `SC_47_a_connection_is_two_passenger_segments_under_one_journey` |
| 48 | Multi-city — three or more bounds | `TESTED` | D `SC_48_a_multi_bound_itinerary_carries_three_journeys`. **Vocabulary note:** `JourneyType` has no `MultiCity` member (`OneWay`, `RoundTrip`, `Circle`, `OpenJaw`); the test uses `Circle`, which is the Pack vocabulary for a multi-bound itinerary. The structural claim — three journeys, three sequences, one Order — is what the scenario asks for. |
| 49 | Technical stop — one segment, many legs | `TESTED` | unchanged |
| 50 | Same flight number on two dated segments does not collapse | `TESTED` | D `SC_50_the_same_flight_number_on_two_dated_segments_does_not_collapse`, D `SC_50_the_same_segment_key_twice_is_rejected` |
| 51, 52 | Round trip units; opaque construction | `TESTED` | unchanged |
| 53 | OpenAir structural scenario | `TESTED` | D `SC_53_an_open_air_segment_carries_no_dated_flight_and_stays_sellable`, D `SC_53_an_air_service_cannot_cover_a_surface_segment`, D `SegmentStructureTests.An_open_segment_cannot_carry_a_dated_flight` |

### E. Source contract failures

| # | Scenario | Now | Test |
|---|---|---|---|
| 54 | Response `offerId` missing or blank | `TESTED` | P `ScenarioClosurePersistenceTests.SC_54_a_blank_responded_offer_identity_is_a_contract_mismatch` |
| 55 | Response `offerId` mismatched | `TESTED` | P `SC_55_a_mismatched_responded_offer_identity_is_a_contract_mismatch_naming_both_identities` — asserts both identities appear in the message, so the failure is diagnosable |
| 56 | Unknown pricing category | `TESTED` | unchanged |
| 57 | Unknown registered detail schema | `NOT_APPLICABLE` | unchanged — the dynamic registry was removed in stage 07 |
| 58–61 | Duplicate air service; malformed totals; currency inconsistency; invalid traveller binding | `TESTED` | unchanged |

New in this stage, beyond the numbered scenarios: an unapproved `stop` payload on a flight or a leg is now a
fail-closed `UnsupportedCapability` (`SourceEvidenceRetentionTests`), and the raw owner payload is proven to retain the
evidence-only `couponId` / `sequence` / `travellerIndex` that the domain model deliberately drops.

## 3. Reliability scenarios

| # | Scenario | Now | Test |
|---|---|---|---|
| R1 | Same idempotency key replay | `TESTED` | `SC_S1_003_replay_after_owner_outage_returns_the_same_order_without_owner_access`, `The_same_idempotency_key_returns_the_same_order` |
| R2 | Replay conflict | `TESTED` | `SC_S1_004_same_key_with_a_different_request_is_a_conflict_and_changes_nothing` |
| R3 | One source consumed once | `TESTED` | **name corrected** — the stage 06 entry named `A_second_consumption_of_the_same_accepted_source_is_a_conflict`, which no longer exists. Now `ReliabilityClosureTests.R3_one_preparation_can_be_consumed_by_at_most_one_order` (unique index `UX_Orders_Owner_SourcePreparation`, proven on SQL Server), `R3_the_accepted_order_is_the_only_link_back_to_its_preparation`, plus `SC_S1_005_concurrent_requests_with_the_same_key_commit_exactly_one_order` |
| R4 | Owner offline after accepted commit | `TESTED` | `SC_S1_003_…` (asserts the owner is not called again) |
| R5 | Projection rebuild deterministic | `TESTED` | `SC_S1_020_…`, `Projection_rebuild_is_byte_identical_for_the_same_accepted_state` |
| R6 | Projection schema transition | `TESTED` | **name corrected** — `A_schema_two_projection_row_stays_readable_…` no longer exists, and the projection schema is back at version 1 after stage 07, so a "2 → 3" transition has no subject. The live claim is that an unknown schema version is refused rather than misread: `ReliabilityClosureTests.R6_a_projection_row_written_by_an_unknown_schema_version_is_refused_not_misread` and `R6_a_projection_row_of_the_current_schema_version_is_read` |
| R7 | Migration from a previous stage | `TESTED` | **name corrected** — `S1_domain_parity_upgrade_…` no longer exists; the live upgrade test is `MigrationUpgradeTests.S1_migration_upgrades_a_b0_database_without_losing_outbox_or_inbox_rows` |
| R8 | Source evidence unchanged | `TESTED` | `AirOfferLiveCandidateBridgeTests`, and now `SourceEvidenceRetentionTests.The_retained_payload_is_the_exact_owner_response_and_matches_its_recorded_hash` |
| R9 | Atomic projection failure | `TESTED` | `SC_S1_006_projection_failure_before_commit_leaves_no_order_source_receipt_or_outbox` |
| R10 | Cross-customer isolation | `TESTED` | `SC_S1_017_another_customer_can_neither_read_the_order_nor_see_the_offer` |
| R11 | PII never leaves the redacted surfaces | `TESTED` | `Service_surface_needs_no_token_and_never_returns_traveller_names` |
| R12 | Independent deadlines | `TESTED` | **name corrected** — `Independent_deadlines_keep_their_owners_…` no longer exists. Now `ReliabilityClosureTests.R12_the_three_source_deadlines_keep_their_own_instants_and_are_never_conflated` and `R12_a_missing_source_deadline_stays_null_and_is_not_filled_from_another_one`, plus `SC_S1_019_expired_owner_validity_blocks_the_sale_at_its_own_instant` |

**Four stale test names found.** R3, R6, R7 and R12 named tests that no longer exist — they were renamed or removed
during the stage 07 simplification and the matrix was never rechecked. Every name in this matrix was verified to exist
in the test sources. R13 and R14 remain explicitly out of the counted 12, as stage 06 recorded them.

## 4. What is still not closed

- Scenarios 4, 5 and 6 are `DEFERRED` with a named stage, not silently passed.
- Scenarios 41 and 57 are `NOT_APPLICABLE` because their subject was deleted; the reason is recorded in stage 07.
- Nothing else in the 61 + 12 is open.
