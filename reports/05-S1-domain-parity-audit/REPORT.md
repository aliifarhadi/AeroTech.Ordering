# S1 Domain Parity Repair — Report

## 1. Repository state

- **Branch:** `k8s-stg`
- **Starting HEAD:** `d33209934389bf93070c9c52b24b8f3ea915e03f` ("Report") — exactly the reviewer-observed commit; the working tree was clean at start, so nothing had to be reconciled and no unrelated work existed to protect.
- **Ending HEAD:** `d33209934389bf93070c9c52b24b8f3ea915e03f` — unchanged.
- **Working-tree status:** dirty with this repair. 54 tracked files modified, 1 deleted (`OrderDtoBuilder.cs`), 49 untracked paths added.
- **Files changed:** 2894 insertions, 498 deletions across the tracked files, plus the new ones. By area:
  - `Contracts/AeroTech.Messages/Ordering/Enums`: 3 new enums (`PricingCalculationKind`, `FulfillmentProfileAssurance`, `FundingRequirement`), 2 edited (`ReservationRequirement`, `FulfillmentDocumentKind` each gained `Unresolved`).
  - `Domain`: 11 new entities, 6 new/moved value objects, `Order` and 7 entities reshaped, candidate value objects and canonical JSON moved to schema `3.0`, validator extended, one new exception code (20288).
  - `Persistence`: 12 new configurations, 9 edited, 3 migrations, new owned-value mappings.
  - `Providers`: `AirOfferCandidateMapper` rewritten around the restored facts.
  - `Query` / `Synchronizer`: new internal typed projection (`OrderProjectionDocument`, `OrderProjectionJson`, `OrderProjectionMapper`, `OrderProjectionBuilder`); `OrderDtoBuilder` deleted; `OrderDtoReader` handles both projection schemas.
  - `tests`: 3 new test files, 1 new fixture, 9 edited.
  - `reports`: decisions register, matrix, plan, and four new stage documents.
  - Outside the repo: handoff `OR-002` written to `E:\Projects\DotAir\handoffs`.
- **Committed:** **nothing.** No commit, no push. The owner asks for commits explicitly.

## 2. Decisions applied

**OD-P-21 — final answer.** `OrderSegment` is the single canonical accepted store for flight-level sold facts: external/source `FlightId`, `FlightNumber`, `FlightVersion`, `MarketingCarrierRef`, `OperatingCarrierRef`, origin/destination airport refs, origin/destination terminal refs, sold departure/arrival, duration, aircraft ref and the source capacity reference. `AirTransportDetail` keeps the traveler/service-level facts: cabin ref, RBD ref, booking class, checked and cabin baggage; the three sold term flags sit on `OrderService` itself. `OrderSegment.FlightRef` is reused as the external FlightId — no second column. The public `OrderAirTransportDto.FlightNumber`, `.MarketingAirlineRef` and `.OperatingAirlineRef` are kept and read from the single covered segment; `FlightVersion` is not exposed. An accepted `AirTransportation` service has exactly one beneficiary and exactly one covered segment, enforced in candidate/domain validation, and a projection that meets anything else fails deterministically with code 20288.

**OD-C-02 — correction.** It was never open. `reports/00-decisions/S1-CLEANUP-OPEN-DECISIONS.md` already records the owner's answer, "`Messages.Shared.Enums.SalesChannel` is allowed". Matrix row A3 is `KEEP` under that explicit exception; `SalesChannel` stays in the Domain and no alternative channel type was invented.

**OD-P-12 — remains the only blocker.** The AirOffer `Stop` node is still unread. No Connection / Stopover / SurfaceBreak / Protected semantics were created, and no canonical `Unknown` connection row was fabricated either. The raw response stays in `PreparationSourceEvidence.Payload`. Handoff `OR-002` asks the AirOffer owner for the `Stop` shape and vocabulary, and — for the same reason — for the undocumented `airTransports[].direction` and root `journeyType` vocabularies, which are preserved raw with their canonical enums left `NULL`.

All of OD-P-01 … OD-P-20 were implemented as answered; the register carries each answer verbatim.

## 3. Plan corrections applied

- **No synthetic legacy Journey.** `OrderSegments.JourneyId` is nullable; a pre-repair segment keeps `NULL` and `OrderJourneys` stays empty after a legacy upgrade. Journey grouping is never rebuilt from raw evidence and never inferred from segment order. Every new accepted segment gets a real journey, which the validator requires.
- **No false `Amount` backfill.** `PricingCalculationKind` gained `NotRecorded`; pre-repair lines are backfilled as `NotRecorded`, and a new accepted line may never be — the validator rejects a candidate line that does not state its kind.
- **Existing `FlightRef` reused.** No `SourceFlightRef` column was added; the AirOffer `FlightId` is persisted once.
- **`FlightNumber` moved to segment ownership** along with the other flight-level facts, per OD-P-21.
- **Detail-to-segment migration validation.** The flight facts already accepted into `AirTransportServiceDetails` are migrated to `OrderSegments` through `OrderServiceCoverage`, and the migration fails before any write if the covering services disagree (`THROW 51001`) or if a stored `FlightVersion` is not an invariant integer (`THROW 51002`). No first-value-wins.
- **ProductSnapshot / terms time correction.** `ProductSnapshot` has no `AcceptedAt`. `CommercialTermsSnapshot.TermsCapturedAt` is the already-persisted `AcceptedSource.CapturedAt`, never `Order.CreatedAt` and never the candidate capture clock renamed.
- **Internal typed projection separated from the public DTO.** The read model previously stored the public `OrderDto`, which made "preserve the repaired facts through the projection" and "do not change the ratified API" mutually exclusive. `OrderProjectionDocument` (schema 3) is now the internal typed document; `OrderProjectionMapper` maps it to the unchanged public `OrderDto`; `OrderDtoReader` reads schema 3 through the mapper and schema 2 through the legacy reader during the transition.

One further correction was forced by the data, not by the owner: see §5.

## 4. Domain changes

| Concept | Before | After | Authority | Invariant |
|---|---|---|---|---|
| Sale currency | `Order.SaleCurrencyRef` (stringified CurrencyId) | `Order.SaleCurrency` — `CurrencySnapshot(CurrencyRef, CurrencyCode?)` | DOMAIN/03 §31, OD-P-01 | the code is stored only when the source supplies it; never fetched from ReferenceData |
| Applied conversion | `PricingLine.SourceConversionRef` string | `PricingLine.AppliedConversion` — typed snapshot of the referenced rate row | OD-P-16 | rounding token preserved verbatim and read by no calculation; accepted money never recomputed |
| Percentage provenance | `IsPercentage` discarded | `PricingLine.CalculationKind` | OD-P-18 | the numeric basis is not interpreted; `NotRecorded` is legacy-only |
| Product snapshot | absent | `OrderItem.Product` — `ProductSnapshot` | DOMAIN/02 §9, OD-P-03 | supplied facts only; no invented product identity, no package-level carrier, no item quantity/UOM |
| Commercial terms | discarded | `OrderService.SoldTerms` (exact per-service flags) + `OrderItem.CommercialTerms` (item summary) | DOMAIN/02 §9, OD-P-04 | all true → `Permitted`; all false → `Prohibited`; mixed → `Conditional`; none → `Unknown`; disagreement is never "not supplied" |
| Service base fields | missing | `ServiceCode`, `Name`, `PriceTreatment`, `SupplierPartyRef`, `DeliveryProviderRef` | DOMAIN/02 §19, OD-P-06 | AirOffer populates only `SupplierOpaque`; no fake code, no inferred supplier, never `SeparatelyPriced` |
| Fulfillment profile | uncertified assumptions presented as fact | `FulfillmentProfileSnapshot(ProfileRef, ProfileVersion, Assurance, ReservationRequirement, DocumentKind, FundingRequirement, CapacityUnits?)` | DOMAIN/02 §23, OD-P-07 | a `NotCertified` profile may not claim any requirement, and a `Certified` one must state them — enforced in `CandidateValidator` |
| Baggage | discarded | `AirTransportDetail.CheckedBaggage` / `.CabinBaggage` — `BaggageAllowance` | OD-P-08 | checked and cabin kept separate; weight requires a unit; nothing inferred; no separate baggage service |
| Journey | absent | `OrderJourney` between Order and Segment; `Order.SourceJourneyTypeRaw` / `JourneyType` | DOMAIN/04 §15, OD-P-09 | raw direction and journey type preserved; canonical enums stay `NULL` until `OR-002` |
| Sold segment | origin/destination refs, times, flight ref | plus terminals, duration, aircraft, capacity ref, flight number, flight version, both carriers | DOMAIN/04 §15, OD-P-10, OD-P-21 | cabin/RBD/booking class are **not** duplicated here; `SourceCapacityRef` is a source reference, not capacity truth |
| Legs | sequence + ref | plus airports, terminals, departure, arrival | OD-P-11 | no stop/connection/protection member exists |
| Pricing line identity | JSON path only | `SourceCode`, `SourceName`, `SourceReference` | DOMAIN/03, OD-P-15 | preserved without reinterpretation; `Code` is not assumed to be a tax code |
| Fare construction | one `PricingUnitsJson` column | `FareConstruction` → `FarePricingGroup(+Traveler)`, `FarePricingUnit(+CoveredBound)`, `FareComponent(+Service, +Segment)`, `FareConstructionItem` | DOMAIN/03 §47–51, DOMAIN/13, OD-P-19 | groups and coverage rows exist only where the source proves them; `CombinationMethod = Unspecified` when unestablished; `SourceKindRaw` preserved |
| Ticketing deadline | prose inside a validity reason | `Order.ObservedTicketingDeadline` — typed `ObservedTimeFact` | OD-P-02 | the authoritative deadline stays `NotSupplied` under BD-004; the two can never be confused |
| Item–service link | no scope | `TravelersAtAssociation` + `SegmentsAtAssociation` typed child rows | DOMAIN/02 §13, OD-P-05 | `ScopeAtAssociation` is typed, append-only, and not JSON; `LinkedAt` deliberately not added |
| Candidate schema | `2.0` | `3.0` | — | `CandidateValidator` still refuses anything that is not the current version |

## 5. Information-loss fixes

The full table is in `INFORMATION-PRESERVATION.md` §1 — every previously discarded AirOffer member with its canonical destination. Thirty source facts that the pre-repair code parsed and threw away are now persisted.

**One plan assumption was wrong and the data corrected it.** The plan assumed a pricing row's `rateOfExchangePeriodId` resolves to a rate converting that row's currency into the sale currency, and validated it. The recorded live AirOffer response disproves that: a fare line in currency `155` with an equivalent in `70` references a period whose row reads `fromCurrencyId 71 → toCurrencyId 70`. No published contract explains the correspondence, so the repair now preserves the rate row exactly as the source states it and correlates nothing. A referenced period missing from `ratesOfExchange` leaves the snapshot `NULL` while the reference itself is preserved. This is recorded as a finding, not a workaround: had the validation shipped, every real AirOffer sale with a converted line would have been refused.

## 6. Single-source-of-truth audit

| Fact | Single canonical owner | Anywhere else? |
|---|---|---|
| `FlightId` | `OrderSegments.FlightRef` | no — no `SourceFlightRef` was added |
| `FlightNumber` | `OrderSegments.FlightNumber` | no — the detail column is dropped; the public DTO field is read from the segment at projection time |
| `FlightVersion` | `OrderSegments.FlightVersion` | no — detail column dropped; not public |
| marketing carrier | `OrderSegments.MarketingCarrierRef` | no — detail column dropped; public field read from the segment |
| operating carrier | `OrderSegments.OperatingCarrierRef` | no — same |
| cabin / RBD / booking class | `AirTransportServiceDetails.CabinRef` / `RbdRef` / `BookingClass` | the fare **component** carries its own source-supplied cabin/RBD/booking class, which is a different fact (what the fare was filed under, not what this passenger was sold) and comes from a different wire node |
| baggage | `AirTransportServiceDetails.CheckedBaggage*` / `CabinBaggage*` | no |
| commercial flags | `OrderServices.SoldTerm*` (exact per service) | `OrderItems.Terms*` is a **derived summary**, computed from those flags at acceptance, not a second copy — and it is the shape DOMAIN/02 §9 requires at item level |
| pricing source identity | `PricingLines.SourceCode` / `SourceName` / `SourceReference` / `SourceLineRef` | no |
| conversion evidence | `PricingLines.Conversion*` | no — `SourceConversionRef` is the reference, the snapshot is the resolved evidence |

The projection document denormalizes some of these for reading; that is presentation, regenerated from the aggregate on every rebuild, and byte-deterministic.

## 7. Persistence and migrations

Full detail in `PERSISTENCE-AUDIT.md`. Summary:

- **Tables added:** 11 (`OrderJourneys`, the eight fare-graph tables, and the two association-scope tables).
- **Columns added:** 6 on `Orders`, 10 on `OrderSegments`, 6 on `OrderSegmentLegs`, 15 on `OrderItems`, 11 on `OrderServices`, 6 on `AirTransportServiceDetails`, 10 on `PricingLines`.
- **Columns removed:** `OrderServices.RequiresFunding` (migration 1, replaced by the tri-state); `FareConstructions.PricingUnitsJson` and the four `AirTransportServiceDetails` flight columns (migration 3, gated).
- **Blob removed:** yes — `PricingUnitsJson`, only after the typed rows were proven equivalent.
- **Backfills:** every one derived from already-persisted canonical columns, or recorded as an honest unknown. The full derivation table is in `PERSISTENCE-AUDIT.md` §5. The one stored value the repair rewrites is the live AirOffer profile's `ReservationRequirement` / `DocumentKind` / `CapacityUnits`, reset to "not certified" per OD-P-07; it replaces an adapter invention with the truth, never the reverse.
- **Unknown/null legacy states:** listed in `PERSISTENCE-AUDIT.md` §6, including the deliberately empty `OrderJourneys` and the absent pricing groups and component coverage.
- **Indexes / checks / FKs:** listed in `PERSISTENCE-AUDIT.md` §7; every new child FK is `Restrict`, matching the aggregate.
- **Verification:** `has-pending-model-changes` clean; fresh-database and upgrade-from-pre-repair paths both green; dev database updated in place, never reset.

## 8. Projection / API compatibility

- **Internal projection schema:** `OrderProjectionJson.SchemaVersion = 3`, a strongly typed `OrderProjectionDocument`. No dictionaries, no untyped JSON, no second read-model framework.
- **Schema-2 transition:** `OrderDtoReader` dispatches on `ProjectionSchemaVersion` — 3 through `OrderProjectionMapper`, 2 through the legacy `OrderDtoJson`, anything else raises an unsupported-capability error. No fact is fabricated when reading a schema-2 row.
- **Schema-3 rebuild:** a rebuild replaces a schema-2 row deterministically and byte-identically for the same canonical state; both properties are asserted.
- **Public DTO compatibility:** `OrderDto` is unchanged. `OrderAirTransportDto.FlightNumber`, `.MarketingAirlineRef` and `.OperatingAirlineRef` return the same values, now sourced from the covered segment. No field added, none removed. Pricing code/name/reference and the journey grouping are **not** exposed — that stays a separate owner decision.
- **OpenAPI:** `Api/OpenApiDocumentTests` and `Api/OrderingApiSurfaceTests` pass with no expected-schema change.

## 9. FareConstruction result

- **Typed graph:** `FareConstruction` → `FarePricingGroups` (+ traveler binding) and `FarePricingUnits` (+ covered bounds) → `FareComponents` (+ service and segment coverage), plus `FareConstructionItems` for the item binding. Units hang off the construction with an **optional** group id, so a construction with no source-proven grouping is a first-class state rather than a hierarchy to be padded.
- **What AirOffer supplied:** the unit's source reference and raw `kind`, its `coveredBoundOfferIds`, and per component the source fare id, fare basis, fare family, fare type, cabin, RBD, booking class and `ticketingRestrictionMinutes`. All are persisted.
- **What remains absent:** pricing groups, per-component service/segment coverage, fare owner, tariff, rule and routing references. AirOffer proves none of them.
- **Proof that nothing was fabricated:** `AcceptOriginalSaleTests.Opaque_context_is_retained_without_invented_links` asserts zero groups and empty component coverage while the typed unit and component carry their source facts; `CandidateValidator` rejects an opaque construction that claims any component coverage; the migration inserts no group or coverage row; and the upgrade test asserts `FarePricingGroups`, `FareComponentServices` and `FareComponentSegments` are all empty after a legacy upgrade. The fabricated `FareCombinationMethod = ProviderDefined` is gone, replaced by `Unspecified` both in the mapper and in the migrated rows.
- **JSON migration / drop status:** migrated in `S1DomainParityBackfill`, verified by `MigrationUpgradeTests`, dropped in `S1DomainParityDropRetiredColumns`. The gate is asserted in the test: it checks the blob is still present after the backfill migration and gone after the final one.

## 10. Reliability regression results

One row per §15 invariant, all from the runs recorded in `TEST-RESULTS.md` §5:

| Invariant | Result |
|---|---|
| durable idempotency | PASS |
| same key + same semantic payload replay | PASS |
| same key + different payload conflict | PASS |
| successful replay performs no owner call | PASS |
| one Order under concurrency | PASS |
| one accepted source consumed once | PASS |
| exact retained source evidence | PASS |
| no network call inside SQL transaction | PASS |
| Order + receipt + projection + outbox atomicity | PASS |
| SQL Server concurrency behavior | PASS |
| one projector | PASS |
| deterministic projection rebuild | PASS |
| PII separation / redaction | PASS |
| authorization and owner boundaries | PASS |
| real/live sandbox vs reference simulator distinction | PASS |
| pricing sign / total invariants | PASS |
| exact accepted customer total reconciliation | PASS |
| no invented infant / seat / capacity semantics | PASS |

No test was weakened, disabled or deleted. Six tests have new expected values because the canonical form and the fare-construction storage changed; each is listed with its reason in `TEST-RESULTS.md` §4, and the fare-construction assertions are strictly stronger than the substring checks they replace.

## 11. Tests executed

| Suite | Command | Discovered | Passed | Failed | Skipped |
|---|---|---|---|---|---|
| Domain | `dotnet test tests/AeroTech.Ordering.Domain.Tests --no-build -o "$TEMP/ordbuild-final"` | 46 | 46 | 0 | 0 |
| Persistence / SQL | `dotnet test tests/AeroTech.Ordering.Persistence.Tests --no-build -o "$TEMP/ordbuild-final" --filter "Category!=Live"` (S1 36, Inbox 16, Composition 9, Outbox 6, Precision 5, Deterministic 3, ReferenceData 1) | 76 | 76 | 0 | 0 |
| API | same run, `Api.*` (surface + OpenAPI) and `Host.*` | 20 | 20 | 0 | 0 |
| Architecture | same run, `Architecture.*` | 18 | 18 | 0 | 0 |
| Migration | same run, `S1.MigrationUpgradeTests` (also run alone as `--filter "FullyQualifiedName~MigrationUpgradeTests"`) | 2 | 2 | 0 | 0 |
| Live AirOffer | `dotnet test tests/AeroTech.Ordering.Persistence.Tests --no-build -o "$TEMP/ordbuild-final" --filter "Category=Live"` | 1 | 0 | 1 | 0 |
| Model vs migrations | `dotnet ef migrations has-pending-model-changes --project src/AeroTech.Ordering.Persistence --startup-project src/AeroTech.Ordering.ServiceHost --context OrderingDbContext` | — | pass | — | — |

Totals: **46 + 114 = 160 passed, 0 failed** in the runnable suites. The persistence project's 114 tests are broken out above as Persistence/SQL 76 + API 20 + Architecture 18; the Migration row names the 2 migration tests that are already counted inside the Persistence/SQL 76, listed separately because they were also run on their own.

The single Live failure is an environment gate, not a defect: `ORDERING_LIVE_AIROFFER_BASEURL must point at the live AirOffer service.` The live AirOffer instance was not running on this machine, so the test refuses rather than pretending. The **recorded** live response is still exercised in the normal suite by `AirOfferLiveOwnerTests.Recorded_live_owner_response_is_normalized_and_sold`, which passes — and that test is what caught the rate-of-exchange semantics in §5.

## 12. Remaining blockers

- **`OD-P-12` — the AirOffer `Stop` vocabulary.** Still blocked, by the owner's own answer. No Stop-domain code was written. Handoff `OR-002` is filed at `E:\Projects\DotAir\handoffs\OR-002-airoffer-stop-direction-and-journeytype-vocabulary.md` and also asks for the `direction` and `journeyType` vocabularies, which are preserved raw for exactly the same reason.

Nothing else is blocked. `OD-C-02` and `OD-P-21` are answered and implemented.

## 13. Deferred behavior

No S2 behavior was implemented. Specifically: no capacity or reservation behavior (`SourceCapacityRef` is a retained reference and nothing reads it for eligibility), no payment behavior, no document issue behavior, no servicing, refund or exchange behavior, no eligibility computed from the display term flags, no percentage-rate interpretation, no `Stop` interpretation, no `PricingLine` quantity/UOM/unit price, no `PriceChangeSet` source split, no `ContactPoint` redesign, no item quantity/UOM, no raw pricing-category column, no fulfillment-profile engine, no FX engine.

## 14. S1 status

`S1_DOMAIN_REPAIR_READY_FOR_OWNER_REVIEW`

## 15. S2 status

`S2_NOT_STARTED`
