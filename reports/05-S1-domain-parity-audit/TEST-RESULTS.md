# S1 Domain Parity — Test Results

Stage: 05-S1-domain-parity-audit · 2026-09-19
Build: `dotnet build AeroTech.Ordering.sln -o "$TEMP/ordbuild-final"` → **Build succeeded**, 0 errors.

## 1. Commands and totals

| Suite | Command | Discovered | Passed | Failed | Skipped |
|---|---|---|---|---|---|
| Domain | `dotnet test tests/AeroTech.Ordering.Domain.Tests --no-build -o "$TEMP/ordbuild-final"` | 46 | **46** | 0 | 0 |
| Persistence / SQL / API / architecture / migration | `dotnet test tests/AeroTech.Ordering.Persistence.Tests --no-build -o "$TEMP/ordbuild-final" --filter "Category!=Live"` | 114 | **114** | 0 | 0 |
| Live AirOffer | `dotnet test tests/AeroTech.Ordering.Persistence.Tests --no-build -o "$TEMP/ordbuild-final" --filter "Category=Live"` | 1 | 0 | **1** | 0 |
| Model vs migrations | `dotnet ef migrations has-pending-model-changes --project src/AeroTech.Ordering.Persistence --startup-project src/AeroTech.Ordering.ServiceHost --context OrderingDbContext` | — | pass | — | — |

The single Live failure is an environment gate, not a defect: `ORDERING_LIVE_AIROFFER_BASEURL must point at the live AirOffer service.` The live AirOffer service was not running on this machine, so the test refuses to run rather than pretending. The **recorded** live AirOffer response is still exercised in the normal suite by `AirOfferLiveOwnerTests.Recorded_live_owner_response_is_normalized_and_sold`, which passes, and that is the test that caught the real rate-of-exchange semantics described in `INFORMATION-PRESERVATION.md` §3.

## 2. Persistence suite by area

| Area | Discovered | Result |
|---|---|---|
| `S1.*` (create, atomicity/rebuild, AirOffer bridge, recorded live owner, migration upgrade, information preservation, projection schema transition) | 37 (1 of them `Category=Live`) | 36 pass, 1 environment-gated |
| `Api.*` (surface contracts, OpenAPI document) | 19 | pass |
| `Architecture.*` (layer references, Messages allowlist, S1 rail) | 18 | pass |
| `Inbox.*` | 16 | pass |
| `Composition.*` (host composition, single-registration guard) | 9 | pass |
| `Outbox.*` | 6 | pass |
| `Precision.*` (model precision, decimal precision) | 5 | pass |
| `Deterministic.*` (effect store) | 3 | pass |
| `ReferenceData.*` | 1 | pass |
| `Host.*` (anonymous surface inventory) | 1 | pass |

## 3. Tests added in this stage

| Test | What it proves |
|---|---|
| `S1/InformationPreservationTests.Every_supplied_air_offer_fact_survives_wire_candidate_order_sql_and_projection` | the full vector list of `INFORMATION-PRESERVATION.md` §1, plus no owner call after acceptance |
| `S1/InformationPreservationTests.Schema_three_service_detail_does_not_duplicate_flight_level_facts` | single ownership of flight-level facts, and public DTO compatibility through the segment |
| `S1/InformationPreservationTests.Applied_conversion_evidence_is_preserved_without_recomputing_money` | rate, decimal places and rounding token preserved; accepted money untouched |
| `S1/InformationPreservationTests.Percentage_rows_record_their_calculation_kind_without_interpreting_the_rate` | `IsPercentage` provenance without a local formula |
| `S1/InformationPreservationTests.Projection_rebuild_is_byte_identical_for_the_same_accepted_state` | deterministic schema-3 rebuild |
| `S1/ProjectionSchemaTransitionTests.A_schema_two_projection_row_stays_readable_and_a_rebuild_moves_it_to_schema_three` | schema-2 rows readable during the transition; rebuild replaces them deterministically |
| `S1/MigrationUpgradeTests.S1_domain_parity_upgrade_preserves_accepted_rows_without_inventing_facts` | the whole §14 upgrade checklist against a pre-repair accepted order fixture |

Supporting fixture added: `S1/LegacyOrderFixture.cs` — a pre-repair accepted order written directly against the `S1OrderCreate` schema.

## 4. Tests whose expected values changed, and why

| Test | Change | Reason |
|---|---|---|
| `Domain.Tests/OrderPreparationAggregate/CandidateCanonicalFormTests` | `PackExampleDigest` updated to `2732898652d05598776a1811a5546c55aa45f8af2e782f9d1ee674c9ff9d1adb`; the pack example literals regenerated to schema `3.0` | the canonical candidate form gained the new preserved facts, so its digest necessarily changed. The assertion itself is unchanged and still pins an exact digest. |
| `Domain.Tests/OrderPreparationAggregate/CandidateValidatorTests.Unregistered_detail_schema_version_is_unsupported_before_acceptance` | probes detail schema version `3` instead of `2` | version `2` is now the registered air-transport detail schema |
| `Domain.Tests/OrderAggregate/AcceptOriginalSaleTests` (3 fare-construction facts) | assert the typed graph instead of substrings of `PricingUnitsJson` | the blob no longer exists; these assertions are strictly stronger now (they check unit type, combination method, covered bounds, group quantity, component coverage and the item binding) |
| `Persistence.Tests/S1/CreateOrderFromOfferTests` (2 fare-construction facts) | same | same |
| `Persistence.Tests/S1/AirOfferLiveCandidateBridgeTests.SC_S1_018_…` | asserts the typed `ObservedTicketingDeadline` and that `LastTicketingDate` is **not** in the prose reason | `OD-P-02` forbids hiding the supplied instant inside a reason string |
| `Persistence.Tests/S1/AirOfferLiveCandidateBridgeTests` (leg count) | `Segments.Legs.Count` instead of `OperationalLegRefs.Count` | legs are typed now |

No assertion was deleted, weakened or disabled. No test was skipped to go green.

## 5. Reliability invariants — PASS/FAIL

| Invariant | Covering test | Result |
|---|---|---|
| durable idempotency | `S1/CreateOrderFromOfferTests` | PASS |
| same key + same semantic payload replays | `S1/CreateOrderFromOfferTests` | PASS |
| same key + different payload conflicts | `S1/CreateOrderFromOfferTests` | PASS |
| successful replay performs no owner call | `S1/AirOfferLiveCandidateBridgeTests`, `S1/InformationPreservationTests` | PASS |
| one Order under concurrency | `S1/CreateOrderFromOfferTests` | PASS |
| one accepted source consumed once | `S1/CreateOrderFromOfferTests` | PASS |
| exact retained source evidence | `S1/AirOfferLiveCandidateBridgeTests` | PASS |
| no network call inside the SQL transaction | `S1/AtomicityAndRebuildTests`, `Architecture/S1RailTests` | PASS |
| Order + receipt + projection + outbox atomicity | `S1/AtomicityAndRebuildTests` with `InterruptibleUnitOfWork` | PASS |
| SQL Server concurrency behavior | `S1/CreateOrderFromOfferTests` | PASS |
| one projector | `Composition/SingleRegistrationGuardTests` | PASS |
| deterministic projection rebuild | `S1/AtomicityAndRebuildTests`, `S1/InformationPreservationTests` | PASS |
| PII separation / redaction | `Api/OrderingApiSurfaceTests` | PASS |
| authorization and owner boundaries | `Api/OrderingApiSurfaceTests`, `Host/AnonymousSurfaceInventoryTests` | PASS |
| live sandbox vs reference simulator distinction | `S1/AirOfferLiveCandidateBridgeTests`, `Deterministic/DeterministicEffectStoreTests` | PASS |
| pricing sign / total invariants | `Domain.Tests/PricingArithmeticTests`, `AcceptOriginalSaleTests` | PASS |
| exact accepted customer total reconciliation | `Domain.Tests/AcceptOriginalSaleTests`, `S1/CreateOrderFromOfferTests` | PASS |
| no invented infant / seat / capacity semantics | `S1/AirOfferLiveCandidateBridgeTests` (INF still refused), `S1/InformationPreservationTests` (capacity ref is a source reference only) | PASS |
| layer and Messages-allowlist boundaries | `Architecture/LayerReferenceTests`, `Architecture/DomainMessagesAllowlistTests` | PASS |
| decimal precision of every new column | `Precision/ModelPrecisionTests`, `Precision/DecimalPrecisionTests` | PASS |
| public OpenAPI contract unchanged | `Api/OpenApiDocumentTests` | PASS |
