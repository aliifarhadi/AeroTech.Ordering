# Test Results

Stage: 07-S1-domain-simplification · 2026-09-20 · branch `k8s-stg`

Raw output is in `01-runs/`. Nothing here is reconstructed from memory; every figure below is in a file in that folder.

## Commands and outcomes

| Command | Result | Evidence |
|---|---|---|
| `dotnet build AeroTech.Ordering.sln -o "$TEMP/ordbuild6"` | Build succeeded, 0 errors, 2 warnings (both pre-existing `NU1510`) | `01-runs/SOLUTION-BUILD.txt` |
| `dotnet test tests/AeroTech.Ordering.Domain.Tests` | **Passed — 58 / 58**, 0 failed, 0 skipped, 233 ms | `01-runs/DOMAIN-TESTS.txt` |
| `dotnet test tests/AeroTech.Ordering.Persistence.Tests --filter "Category!=Live"` | **Passed — 134 / 134**, 0 failed, 0 skipped, 49 s | `01-runs/PERSISTENCE-TESTS.txt` |
| `dotnet ef migrations has-pending-model-changes` (`OrderingDbContext`) | No changes since the last migration | `01-runs/HAS-PENDING-MODEL-CHANGES.txt` |
| same, `OrderQueryDbContext` and `ReferenceDbContext` | No changes since the last migration | run inline, same output |

`Category=Live` tests were not run: they need `ORDERING_LIVE_AIROFFER_BASEURL` and a currently priced offer id. The
recorded live payload is still covered by `AirOfferLiveOwnerTests.Recorded_live_owner_response_is_normalized_and_sold`,
which is in the 134.

The persistence suite is inside the 3–4 minute bar (49 s). One earlier full run hung for over ten minutes with no
SQL activity and was stopped; the next two full runs on the same build finished in 56 s and 53 s, so the hang was
transient and is not reproducible. It is recorded here rather than left out.

## What the new and rewritten tests prove

### Missing beneficiary — four levels of evidence

The Pack scenario is **TESTED**, not dropped. The old runtime-validation test was removed because the invalid state it
asserted can no longer be constructed; each level below fails against code that does not enforce it.

| Level | Test | What it proves |
|---|---|---|
| Provider / ACL | `A_ticket_without_a_usable_traveller_reference_fails_closed_before_any_candidate(blank \| missing \| duplicate)` | a ticket with a blank, missing or repeated `travellerRef` is a `ContractMismatch` (20272) with no receipt and no preparation |
| Canonical reader | `A_candidate_service_that_names_no_traveller_cannot_be_read` | a service object that omits `travellerRef` is a `ContractMismatch` (20272) in the schema-1 reader |
| Candidate validation | `Pack_missing_beneficiary_is_rejected_at_the_candidate_boundary` | a service naming a traveller that is not in the candidate is rejected |
| Domain construction | `An_accepted_service_is_bound_to_exactly_one_traveller_and_one_segment` | `OrderService` cannot be constructed with a non-positive `TravellerId` or `SegmentId` (20293) |

### The other three Pack intents carried off `PackExamples.cs`

| Intent | Replacement test |
|---|---|
| one-way reference candidate | `Pack_one_way_reference_candidate_is_valid` |
| incorrect customer total | `Pack_incorrect_total_is_rejected_against_its_pricing_lines` |
| settlement-only tax | `Pack_settlement_tax_is_rejected_by_the_component_matrix` (asserts 20279 and the component rule's own message, not the attribution rule) |

`PackExamples.cs` (933 lines of schema-3 candidate JSON) was deleted only after all four intents had an explicit
replacement built with the current builder.

**Forced-failure proof for the ACL level.** With the single line `EnsureTravellerReferences(details.Tickets);` removed
from `AirOfferCandidateMapper.Map`, the `duplicate` case goes red — it reaches a generic candidate-contract message
instead of naming the traveller reference — while `blank` and `missing` stay green, because an empty `travellerRef`
is independently rejected downstream. So the ACL guard is the only thing that closes the repeated-reference case, and
the two other cases are defended twice. Raw output of the unfixed run: `01-runs/ACL-GUARD-REMOVED-RED.txt`. The line
was restored and the full suite re-run green afterwards.

### Information preservation

`InformationPreservationTests.Every_supplied_air_offer_fact_survives_wire_candidate_order_sql_and_projection` now
asserts the typed identities end to end: `FlightId 7001`, `FlightCapacityId 5001`, `FlightVersion 4`,
`AircraftId 9`, terminals `61`/`62`, legs `81`/`82`, `AirFareId 9001`, `CabinClassId 3`, `RbdId 44`, `CurrencyId 978`,
`BoundDirection.Outbound`, `JourneyType.OneWay` and `LastTicketingDate`, at candidate, aggregate and projection level,
with the owner called exactly once.

`A_service_carries_only_its_own_facts_and_never_repeats_the_flight` replaces the schema-3 detail-dictionary test: it
proves the service carries cabin, RBD and booking class only, and that flight number, marketing airline, operating
airline and flight version live on the segment in the projection and in the public DTO.

### Projection and rebuild

`Projection_rebuild_is_byte_identical_for_the_same_accepted_state` and
`A_settlement_attribution_survives_sql_the_projection_and_a_rebuild` still pass against the single-schema projection
(`SchemaVersion = 1`), with component totals now derived in the projector rather than stored.

## Tests removed, and why

| Test | Reason |
|---|---|
| `SC_S1_009_round_trip_and_two_one_way_constructions_are_persisted_as_supplied` | asserted `FareConstructionAssurance` and component↔service coverage rows that AirOffer never supplies and the model no longer has |
| `SC_S1_011_group_extended_line_is_persisted_once` | asserted `FarePricingGroup`, which AirOffer never supplies |
| `SC_S1_021_unregistered_product_schema_is_unsupported_before_any_order` | asserted the detail-schema registry, which is gone |
| `ProjectionSchemaTransitionTests` (whole file) | asserted schema-2/3 projection compatibility readers, retired by the approved rebaseline |
| `A_funding_obligation_needs_exactly_one_scope_on_sql_server`, `A_component_total_is_unique_per_component_and_effect_on_sql_server`, `A_component_total_cannot_hold_a_negative_magnitude_on_sql_server` | the multi-scope obligation and the `OrderComponentTotals` table no longer exist |
| `S1_domain_parity_upgrade_preserves_accepted_rows_without_inventing_facts` | asserted the backfill behaviour of the migrations the rebaseline retires; see `MIGRATION-DECISION.md` |

`ClosureConstraintTests` did not simply lose rows: the theory now also asserts `CK_PricingLines_OriginalMagnitude`,
`CK_PricingLines_SaleMagnitude`, `CK_PricingLines_Direction`, `CK_FundingObligations_Version` and
`CK_FundingObligations_Amount`, which the previous version did not cover.

## Production defect found by the suite, not by review

The Query side still mapped `OrderTravelerIdentityReadModel` to `Order.OrderTravelerIdentities` and
`OrderTravelerReadModel` to `Order.OrderTravelers` after the command side had been renamed to the `Traveller`
spelling. Every authenticated read of an order with traveller names failed with
`Invalid object name 'Order.OrderTravelerIdentities'`. Both read models, both configurations and `OrderDtoReader` were
corrected. This was invisible while the persistence project did not compile, which is the argument for getting the
suite green before writing anything else.

`OrderingApiSurfaceTests` also still read `offerId` and `grandTotal` from the order response; the public DTO exposes
`sourceOfferId` and `customerTotal`. Corrected in the test, not in the contract.
