# Test Results

Stage: 07-S1-domain-simplification (revision 2) · 2026-09-20 · branch `k8s-stg`

Raw output is in `01-runs/`. Nothing here is reconstructed from memory; every figure below is in a file in that folder.

## Commands and outcomes

| Command | Result | Evidence |
|---|---|---|
| `dotnet build AeroTech.Ordering.sln` | Build succeeded, 0 errors, 2 warnings (both pre-existing `NU1510`) | `01-runs/SOLUTION-BUILD.txt` |
| `dotnet test tests/AeroTech.Ordering.Domain.Tests` | **Passed — 91 / 91**, 0 failed, 0 skipped, 315 ms | `01-runs/DOMAIN-TESTS.txt` |
| `dotnet test tests/AeroTech.Ordering.Persistence.Tests --filter "Category!=Live"` | **Passed — 159 / 159**, 0 failed, 0 skipped, 54 s | `01-runs/PERSISTENCE-TESTS.txt` |
| `dotnet ef migrations has-pending-model-changes` (`OrderingDbContext`) | No changes since the last migration | `01-runs/HAS-PENDING-MODEL-CHANGES.txt` |
| same, `OrderQueryDbContext` and `ReferenceDbContext` | No changes since the last migration | `01-runs/HAS-PENDING-ALL-CONTEXTS.txt` |

The persistence suite covers the API/OpenAPI tests (`Api/`), the architecture tests (`Architecture/`) and the fresh
SQL Server migration test (`MigrationUpgradeTests`); they are inside the 159. `Category=Live` tests were not run — they
need `ORDERING_LIVE_AIROFFER_BASEURL` and a currently priced offer id. The recorded live payload is still covered by
`AirOfferLiveOwnerTests.Recorded_live_owner_response_is_normalized_and_sold`.

The run takes about 54 s; each test database replays the nine-migration chain. That cost disappears with the
rebaseline, which is still awaiting owner authorization (`MIGRATION-DECISION.md`).

## Tests added for the R2 corrections

### Typed service boundary

| Test | Proves |
|---|---|
| `AcceptedShapeTests.An_accepted_air_service_declares_its_service_type_and_owns_its_air_facts` | the service carries `ServiceType = AirTransportation`, and traveller, segment, cabin, RBD, booking class and sold terms live on `OrderAirTransportService` |
| `AcceptedShapePersistenceTests.The_service_type_survives_sql_the_projection_and_the_public_order` | `ServiceType` is the SQL discriminator column, and it reaches the projection document and the public `OrderDto` |
| `AcceptOriginalSaleTests.An_accepted_service_is_bound_to_exactly_one_traveller_and_one_segment` | unchanged: the construction invariant still rejects a non-positive traveller or segment (20293) |

### Buyer

| Test | Proves |
|---|---|
| `AcceptedShapeTests.An_accepted_buyer_is_not_supplied_and_is_never_inferred_from_another_role` | `NotSupplied` is distinct from the financial customer, the seller and the initiating actor |
| `AcceptedShapeTests.A_supplied_buyer_survives_acceptance_as_its_own_party` | a supplied buyer is carried through the candidate, the digest and the aggregate as its own party |
| `AcceptedShapeTests.A_buyer_is_never_half_supplied` | context type and identifier are supplied together or not at all |

### Component totals

| Test | Proves |
|---|---|
| `AcceptedShapeTests.Component_totals_are_committed_once_per_component_and_effect` | totals are computed at acceptance from the committed lines, one row per `(Component, Effect)`, and their customer-balance net equals `CustomerTotal` |
| `ComponentTotalPersistenceTests.A_component_total_is_identified_by_order_component_and_effect_on_sql_server` | the SQL primary key is exactly `OrderId, Component, Effect`, and a duplicate insert is refused by the database |
| `ComponentTotalPersistenceTests.A_component_total_cannot_hold_a_negative_magnitude_on_sql_server` | `CK_OrderComponentTotals_Magnitudes` rejects a negative debit |
| `ComponentTotalPersistenceTests.Component_totals_round_trip_through_sql_and_reconcile_with_the_committed_lines` | the projector reads the persisted rows rather than recomputing them |

### Pricing line role

| Test | Proves |
|---|---|
| `AcceptedShapeTests.Every_accepted_pricing_line_records_its_role` | every accepted line records `PricingLineRole.Original` |
| `AcceptedShapePersistenceTests.Every_persisted_pricing_line_records_the_original_role` | the role survives SQL and the projection |

### Funding

| Test | Proves |
|---|---|
| `AcceptedShapeTests.Every_customer_balance_line_is_covered_by_a_funding_obligation` | obligations sum to `CustomerTotal`, each names exactly one scope, and each carries its price-change-set |
| `AcceptedShapeTests.A_customer_balance_line_that_cannot_be_scoped_is_refused_at_the_candidate_boundary` | a customer-balance line with neither item nor service basis is rejected before acceptance, instead of silently losing its liability |
| `AcceptedShapeTests.A_funding_obligation_names_exactly_one_scope` | the typed scope factory refuses a non-positive or absent identifier (20295) |
| `ClosureConstraintTests.A_funding_obligation_needs_exactly_one_scope_on_sql_server` | `CK_FundingObligations_ExactlyOneScope` rejects both two scopes and no scope |

### Fare construction

| Test | Proves |
|---|---|
| `AcceptedShapeTests.A_candidate_without_a_supplied_fare_construction_commits_none` | no construction is fabricated |
| `AcceptedShapeTests.The_candidate_builder_invents_no_covered_bound_offer_ids` | the test builder no longer manufactures a default unit whose covered bounds are journey bound ids |
| `AcceptedShapeTests.A_fare_construction_links_only_the_items_the_source_named` | construction↔item links come from the source's own item keys, not from every order item |
| `AcceptedShapeTests.A_fare_construction_naming_an_unknown_item_is_refused` | an unknown item key fails closed |
| `AcceptedShapeTests.A_pricing_unit_keeps_the_owner_construction_type_it_was_sold_under` | the generic unit type and the owner construction type are both retained |
| `AcceptedShapePersistenceTests.Every_owner_pricing_unit_kind_maps_to_its_own_construction_type` | all three real contract values — `OneWay`, `RoundTripFromOneWays`, `RoundTripFare` — map without collapsing the two round-trip constructions |
| `AcceptedShapePersistenceTests.An_unknown_owner_pricing_unit_kind_fails_closed` | `RoundTrip`, which the mapper previously accepted and the contract never sends, is now a contract mismatch |
| `AcceptedShapePersistenceTests.An_owner_that_supplies_no_pricing_units_commits_no_fare_construction` | absence at the ACL produces absence in the candidate and in the aggregate |

### Fulfillment, currency, links, root, naming

| Test | Proves |
|---|---|
| `AcceptedShapeTests.An_accepted_service_keeps_an_honest_fulfillment_profile_without_inventing_one` | `ProfileRef` and `ProfileVersion` are null, assurance is `NotCertified`, and reservation, document and funding requirements are `Unresolved` |
| `AcceptedShapeTests.A_certified_fulfillment_profile_must_name_itself` | claiming `Certified` without a profile reference is refused |
| `AcceptedShapePersistenceTests.The_fulfillment_snapshot_persists_honest_unresolved_values_with_no_invented_profile` | no row in SQL carries an invented profile id |
| `AcceptedShapeTests.The_sale_currency_keeps_its_identity_and_the_source_code_snapshot` and `A_source_that_supplies_no_currency_code_records_none` | `CurrencyId` stays the identity; `SaleCurrencyCode` is one optional snapshot |
| `AcceptedShapePersistenceTests.The_sale_currency_code_snapshot_survives_sql_and_the_public_order` | the live AirOffer `currencyCode` reaches SQL and the public DTO |
| `AcceptedShapeTests.The_original_sale_records_one_item_service_link_per_service` and `AcceptedShapePersistenceTests.The_original_sale_persists_one_item_service_link_per_service` | the minimal link exists at original sale, with no traveller or segment children |
| `AcceptedShapeTests.An_original_order_is_its_own_root` | the original order still sets `RootOrderId = Id` |
| `ClosureConstraintTests.A_root_order_identifier_is_positive_but_is_not_pinned_to_the_order_identifier` | the database accepts a root that differs from the id, and still rejects zero |
| `AcceptedShapePersistenceTests.A_projection_rebuild_returns_the_receipt_identifier_under_its_own_name` | the rebuild result exposes `ReceiptId`, the value is a real `CommandReceipt.Id`, and no `OperationId` member remains |
| `OpenApiDocumentTests.The_create_surfaces_name_the_financial_customer_by_its_role` | the published document carries `financialCustomerId` and no `customerId`, and `SellingOfficeKind` has no `NotRecorded` |
| `CommercialLifecycleVocabularyTests.A_selling_office_always_declares_the_namespace_it_belongs_to` | the shared enum has exactly `AirlineOffice` and `TravelAgencyOffice`, all positive |

## Evidence preserved from revision 1

### Missing beneficiary — four levels

The Pack scenario stays **TESTED**. The old runtime-validation test was removed because the invalid state it asserted
can no longer be constructed.

| Level | Test |
|---|---|
| Provider / ACL | `A_ticket_without_a_usable_traveller_reference_fails_closed_before_any_candidate(blank \| missing \| duplicate)` |
| Canonical reader | `A_candidate_service_that_names_no_traveller_cannot_be_read` |
| Candidate validation | `Pack_missing_beneficiary_is_rejected_at_the_candidate_boundary` |
| Domain construction | `An_accepted_service_is_bound_to_exactly_one_traveller_and_one_segment` |

**Forced-failure proof for the ACL level.** With the single line `EnsureTravellerReferences(details.Tickets);` removed
from `AirOfferCandidateMapper.Map`, the `duplicate` case goes red while `blank` and `missing` stay green, because an
empty `travellerRef` is independently rejected downstream. Raw output of the unfixed run:
`01-runs/ACL-GUARD-REMOVED-RED.txt`. The line was restored and the full suite re-run green afterwards.

In this revision the three ACL cases were also given their own unique offer id per run, because the shared test
database made the "nothing was persisted" assertion count preparations from other classes.

### The three other Pack intents carried off `PackExamples.cs`

| Intent | Replacement test |
|---|---|
| one-way reference candidate | `Pack_one_way_reference_candidate_is_valid` |
| incorrect customer total | `Pack_incorrect_total_is_rejected_against_its_pricing_lines` |
| settlement-only tax | `Pack_settlement_tax_is_rejected_by_the_component_matrix` |

### Information preservation, PII boundary and rebuild

`Every_supplied_air_offer_fact_survives_wire_candidate_order_sql_and_projection` still asserts the typed identities end
to end, with the owner called exactly once.
`Projection_rebuild_is_byte_identical_for_the_same_accepted_state` and
`A_settlement_attribution_survives_sql_the_projection_and_a_rebuild` still pass against the single-schema projection.
`OrderDtoReader` still loads traveller identities and contacts only when the read scope permits protected payloads, and
the projection JSON still carries no names or contacts — that design was not touched.

## Tests added by the post-R2 closure pass

Six enforcement defects were closed; each has a test that turns red if the rule is removed.

### Buyer bound to the authorized sales scope

| Test | Proves |
|---|---|
| `AcceptedScopeAndLiabilityTests.A_candidate_with_no_buyer_is_accepted_under_a_scope_with_no_buyer` | `NotSupplied` on both sides is valid |
| `…A_candidate_buyer_is_accepted_only_under_the_same_buyer` | the same supplied buyer is valid |
| `…A_candidate_buyer_cannot_be_sold_under_a_different_buyer` | buyer A captured, scope B is refused (20272) |
| `…A_candidate_buyer_cannot_be_sold_under_a_scope_that_supplies_none` | supplied candidate, `NotSupplied` scope is refused |
| `…A_candidate_without_a_buyer_cannot_be_sold_under_a_scope_that_supplies_one` | the reverse is refused |
| `…A_buyer_is_never_inferred_from_another_role` | the accepted buyer equals none of financial customer, seller, actor or traveller |

### Original-sale liability is never negative

| Test | Proves |
|---|---|
| `…A_service_scope_whose_original_sale_net_is_negative_is_rejected_before_any_order` | the exact defect: item fare +100 with an item-less service discount 10, total 90, refused before any Order |
| `…A_negative_original_sale_customer_total_is_rejected_before_any_order` | a negative overall total is refused at the candidate boundary |
| `…A_positive_item_less_service_scoped_liability_remains_valid` | a positive service-scoped liability still works and the obligations still sum to `CustomerTotal` |
| `…A_negative_obligation_can_never_be_materialised_by_the_aggregate` | the constructor guard (20295) as defence in depth |
| `ClosureConstraintTests.The_closure_check_constraints_exist_on_sql_server("CK_FundingObligations_Amount", …)` | the SQL nonnegative CHECK is still there — unchanged |

### Fulfillment profile identity and version

| Test | Proves |
|---|---|
| `…A_fulfillment_profile_supplies_its_identity_and_version_together_or_not_at_all` | certified + ref + version valid; ref-only, version-only, half-supplied non-certified and certified-with-neither all refused |
| `…A_supplied_fulfillment_profile_pair_round_trips_unchanged` | a supplied pair survives acceptance unchanged |
| `…The_unresolved_live_profile_stays_unresolved` | the live AirOffer profile stays null/null + `NotCertified` + `Unresolved` |

### Same-current-Order scope and change-set cardinality, on SQL Server

| Test | Proves |
|---|---|
| `AcceptedScopeConstraintTests.An_air_service_cannot_be_pointed_at_another_orders_traveller` | the composite FK rejects it with error 547 naming `OrderTravellers` |
| `…An_air_service_cannot_be_pointed_at_another_orders_segment` | the same for `OrderSegments` |
| `…A_same_order_traveller_and_segment_reference_remains_valid` | a same-order move is still allowed, so the constraint is not over-tight |
| `…A_commercial_change_cannot_carry_a_second_price_change_set` | the unique index on `ChangeId` fires |
| `…A_different_commercial_change_may_carry_its_own_price_change_set` | a second change can still have its own set |

### N distinct taxes on one ticket

| Test | Proves |
|---|---|
| `MultipleTaxOccurrenceTests.Every_tax_occurrence_on_one_ticket_survives_acceptance_sql_and_rebuild` | four taxes at distinct occurrence paths, three codes with `AT` repeated under two references, survive normalisation, acceptance and SQL; nothing merges by code; `CustomerTotal` 123.5; persisted `Tax` total 23.5 equals the row sum; a rebuild preserves them |

## Tests removed, and why

| Test | Reason |
|---|---|
| `SC_S1_009_round_trip_and_two_one_way_constructions_are_persisted_as_supplied` | asserted component↔service coverage rows that no owner supplies; the round-trip construction vocabulary is now covered by `Every_owner_pricing_unit_kind_maps_to_its_own_construction_type` |
| `SC_S1_011_group_extended_line_is_persisted_once` | asserted `FarePricingGroup`, which AirOffer never supplies |
| `SC_S1_021_unregistered_product_schema_is_unsupported_before_any_order` | asserted the detail-schema registry, which is gone |
| `ProjectionSchemaTransitionTests` (whole file) | asserted schema-2/3 projection compatibility readers for undeployed development schemas, retired by the proposed rebaseline |
| `S1_domain_parity_upgrade_preserves_accepted_rows_without_inventing_facts` | asserted the backfill behaviour of the migrations the rebaseline retires; see `MIGRATION-DECISION.md` |

The component-total and funding-scope SQL tests that revision 1 removed are **back**, because the tables they assert
are back.

## Production defects found by the suite, not by review

- The Query side still mapped `OrderTravelerIdentityReadModel` to `Order.OrderTravelerIdentities` after the command
  side had been renamed, so every authenticated read of an order with traveller names failed. Corrected in revision 1
  and still covered.
- `OrderingApiSurfaceTests` read `offerId` and `grandTotal` from the order response; the public DTO exposes
  `sourceOfferId` and `customerTotal`. Corrected in the test, not in the contract.
