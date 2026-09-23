# Test results

Stage: 08-S1-authoritative-final-closure · 2026-09-21 · branch `k8s-stg` · base HEAD `e9fe97a`

Raw output: `01-runs/`.

> **CORRECTED BY STAGE 09.** Four theory rows of `CurrentOwnershipConstraintTests` asserted historical references as
> current ownership and were removed; the matrix test was split into a current-containment test and a
> historical-reference test. Counts below are the Stage 08 state. See
> `reports/09-S1-historical-identity-and-pack-reference/TEST-RESULTS.md`.

## 1. Final state

| Run | Command | Result |
|---|---|---|
| Build | `dotnet build AeroTech.Ordering.sln -o "$TEMP/ordbuild4"` | **0 errors**, 3 warnings (`01-runs/01-BUILD.txt`) |
| Domain tests | `dotnet test tests/AeroTech.Ordering.Domain.Tests --no-build` | **135 passed, 0 failed**, 0 skipped, 0.4 s (`01-runs/02-DOMAIN-TESTS.txt`) |
| Persistence tests | `dotnet test tests/AeroTech.Ordering.Persistence.Tests --no-build --filter "Category!=Live"` | **236 passed, 0 failed**, 0 skipped, 1 m 46 s (`01-runs/03-PERSISTENCE-TESTS.txt`) |
| API + OpenAPI + architecture + composition + host | `--filter "…Api|…Architecture|…Composition|…Host"` | **48 passed, 0 failed**, 37 s (`01-runs/05-API-AND-ARCHITECTURE.txt`) |
| Fresh DB + upgrade + atomicity | `--filter "…MigrationUpgradeTests|…AtomicityAndRebuildTests"` | **4 passed, 0 failed**, 22 s (`01-runs/06-MIGRATION-FRESH-AND-UPGRADE.txt`) |
| Pending model changes, 3 contexts | `dotnet ef migrations has-pending-model-changes` | **No changes** for `OrderingDbContext`, `OrderQueryDbContext`, `ReferenceDbContext` (`01-runs/04-PENDING-MODEL-CHANGES.txt`) |
| Dev database | `dotnet ef database update` (`localhost\SQLEXPRESS`) | chain applied through `20260921000413_S1AuthoritativeFinalShape` |

The `Live` category is excluded as in every previous stage; it requires a running AirOffer at
`http://localhost:5095`. It was not run and is not claimed.

Every persistence test creates its database from the full migration chain, so **236 fresh-database creations** from
`B0InfrastructureShell` through `S1AuthoritativeFinalShape` are part of this run.

## 2. Change from the base commit

| Suite | At `e9fe97a` | Now | New |
|---|---|---|---|
| Domain | 91 | **135** | +44 |
| Persistence (`Category!=Live`) | 159 | **236** | +77 |
| total | 250 | **371** | **+121** |

No test was deleted, disabled, skipped or weakened in this stage.

## 3. The 121 new tests

### Domain — 44

| File | Count | What it closes |
|---|---|---|
| `OrderPreparationAggregate/SegmentStructureTests.cs` | 22 | The `EnsureLegs` / `EnsureScheduledAir` invariants added in this stage: 9 missing-operational-fact cases, leg identity and sequence uniqueness and positivity, leg origin/destination, leg and segment time ordering, open-air rejection of a dated flight, plus 4 positive cases (terminals may be absent, zero-length leg, zero duration, complete scheduled segment) |
| `OrderPreparationAggregate/CompositionScenarioTests.cs` | 22 | Scenario matrix rows 1, 2, 3, 7, 23, 24, 29, 31, 44, 45, 46, 47, 48, 50, 53 — including the negative half of each (a service in two items, a monetary charge owning a service, a non-charge item with no service, `Other` outside `Informational`, a duplicate segment key, an air service on a surface segment) |

### Persistence — 77

| File | Count | What it closes |
|---|---|---|
| `S1/EnumConstraintTests.cs` | 34 | §5 — every persisted enum column guarded; each constraint admits exactly its enum; 29 undefined-value rejections proven by raw SQL; nullable columns accept NULL but still reject undefined |
| `S1/CurrentOwnershipConstraintTests.cs` | 17 | §4 — the 21-relation matrix is live; no order-scoped table keeps a single-column FK; 14 cross-order and cross-owner mutations rejected by SQL Server with the constraint named; one legitimate same-order move still allowed |
| `S1/ScenarioClosurePersistenceTests.cs` | 14 | Scenario rows 1, 2, 3, 7, 10, 26, 27, 28, 29, 31, 54, 55 at the SQL layer |
| `S1/SourceEvidenceRetentionTests.cs` | 6 | §3.1 and §3.2 — evidence-only `couponId`/`sequence`/`travellerIndex` recoverable byte-for-byte; payload matches its hash; evidence-only facts not promoted into the candidate; flight and leg `stop` fail closed; explicit JSON null is not a supplied stop |
| `S1/ReliabilityClosureTests.cs` | 6 | R3, R6 and R12 — three stage-07 deletions that had prose justification but no test (`NEGATIVE-DELETION-AUDIT.md` §10) |

## 4. Red-first evidence

Four claims in this stage were proven by making the test fail first. Nothing here is a green test written after the
fact.

### 4.1 The new domain guards

The guard call sites were disabled (`EnsureLegs`, `EnsureScheduledAir`, `EnsureNoUnsupportedStop`), the suites re-run,
and the guards restored.

| Suite | Guards disabled | Guards restored |
|---|---|---|
| `SegmentStructureTests` (22) | **17 failed**, 5 passed | 22 passed |
| `SourceEvidenceRetentionTests` (6) | **2 failed** — exactly the two stop tests | 6 passed |

The 5 that still passed with the guards off are the positive cases, which is correct — they assert acceptance, not
rejection. After restoring, `git diff` over both files shows only this stage's intended changes and zero
negative-control markers remain.

### 4.2 The enum constraint completeness test found two real gaps

`Every_persisted_enum_column_is_guarded_by_a_live_check_constraint` failed on its first run:

```
["Operations.CommandReceipts.CommandKind (OrderingCommandKind)",
 "Order.OrderPreparations.AcceptanceAssurance (AcceptanceAssurance)"]
```

Both columns were missing from the hand-written list. They were added, the migration regenerated, and the test passed.
Constraints 38 and 39 of `ENUM-CONSTRAINT-MATRIX.md` exist because that test was red.

### 4.3 The ownership matrix test found a hole in its own evidence

`Every_scoped_relation_in_the_matrix_is_a_live_composite_foreign_key` failed with
`["CommandReceipts(OwnerAirlineId, OrderId) -> Orders"]` because the inspection query filtered on schema `Order` only
and `CommandReceipts` lives in `Operations`. The query was widened; the constraint itself was present.

### 4.4 Three behavioural tests were initially proving the wrong constraint

`PriceChangeSets(ChangeId)`, `OrderSegments(JourneyId)` and `PricingLines(PriceChangeSetId)` first failed with SQL
error **2601** (unique index) instead of **547** (foreign key): the naive cross-order update collided with a unique
index before it ever reached the foreign key. A test that asserted only "an exception was thrown" would have passed
while proving nothing about ownership. They were rewritten to point at a spare parent row in the other Order, so the
foreign key — and nothing else — is what rejects them. A fourth, `FareConstructions(CreatedByChangeId)`, threw nothing
at all because the default fixture produces no fare construction; it now uses a candidate with pricing units.

## 5. Failures encountered and fixed during the stage

| Failure | Cause | Fix |
|---|---|---|
| `OrderContact.OrderId1` shadow property warning | A second `HasOne<Order>()` on `OrderContact` duplicated the existing `Order.Contacts` relationship | Removed the duplicate; the child FK already existed |
| Two stop tests counted orders by `SourceOfferId` | The fixture offer id is shared inside the test collection, so other tests in the same class had already created orders | Count before and after instead |
| `EnumCheckConstraints` did not compile | `TableBuilder.HasCheckConstraint` returns `CheckConstraintBuilder`, not `TableBuilder` | Explicit `return table;` |
| `JourneyType.MultiCity` does not exist | The enum is `OneWay`, `RoundTrip`, `Circle`, `OpenJaw` | Scenario 48 uses `Circle`; the vocabulary note is recorded in `SCENARIO-CLOSURE-MATRIX.md` |
| `R3` test asserted `OrderPreparation.IsConsumed` | Stage 07 removed the consumption columns; the guarantee is the unique index on `Orders` | The test now proves the unique index on SQL Server, and ERRATA §4 was corrected to describe the real mechanism |

The last one also corrected a factual error in the ERRATA draft: it had said the preparation "is marked consumed in
the same transaction", which is not what the code does.

## 6. What is not proven

- The `Live` category (real AirOffer at `localhost:5095`) was not run.
- `FareConstructionItems` cross-order attachment is not blocked by a constraint; stated in
  `CURRENT-OWNERSHIP-FK-MATRIX.md` §3, not claimed as covered.
- The PII boundary of `DOMAIN/04` line 25 is not satisfied; `OD-S1-09` is open and ERRATA §6 says so plainly.
- Scenarios 4, 5 and 6 are `DEFERRED` with a named owning stage, not tested.
- No transient hang occurred in this stage's persistence runs. The intermittent hang recorded in stage 07
  (three occurrences, >10 min with no SQL activity, resolved by re-running) did not reproduce; run times here were
  1 m 09 s to 1 m 46 s.
