# Migration impact — `S1AuthoritativeFinalShape`

Stage: 08-S1-authoritative-final-closure · 2026-09-21 · branch `k8s-stg`

File: `src/AeroTech.Ordering.Persistence/Migrations/20260921000413_S1AuthoritativeFinalShape.cs`
Context: `OrderingDbContext`.

> **SUPERSEDED IN PART BY STAGE 09.** Seven of the foreign keys this migration added were over-constrained and are
> replaced by `20260921194857_S1HistoricalIdentityCorrection`, an FK/index-only correction with no data operation and
> no column change. The chain is still not rebaselined. See
> `reports/09-S1-historical-identity-and-pack-reference/MIGRATION-IMPACT.md`.

## 1. Decision: one additive migration, no rebaseline

The migration chain is unchanged and complete. No existing migration was edited, removed or squashed:

```
20260917144148_B0InfrastructureShell
20260918201810_S1OrderCreate
20260918224935_S1DomainParityTypedStructures
20260918225050_S1DomainParityBackfill
20260918231551_S1DomainParityDropRetiredColumns
20260919133032_S1ClosureDomainRepair
20260919204507_S1DomainSimplification
20260919222251_S1SimplificationR2Corrections
20260920223736_S1ClosureScopeAndChangeSetConstraints
20260921000413_S1AuthoritativeFinalShape   <- this stage
```

The rebaseline option recorded in `reports/00-decisions/S1-MIGRATION-REBASELINE-OPEN-DECISION.md` was **not** taken.
Rebaselining would delete the upgrade path that `MigrationUpgradeTests` exercises and would make the earlier stage
evidence unreproducible, for a cosmetic gain. An additive migration keeps both.

## 2. Every operation in `Up()`

119 operations, all schema-only. There is no `DropColumn`, no `RenameColumn`, no `DropTable`, no raw `migrationBuilder.Sql(…)`,
no `InsertData`/`UpdateData`/`DeleteData`, and no data reset anywhere in the file — verified by grep over the whole
file, `Up()` and `Down()`.

| Operation | Count | What it is |
|---|---|---|
| `AddUniqueConstraint` | 8 | Principal alternate keys `(OrderId, Id)` / `(OwnerAirlineId, Id)` on `Orders`, `OrderPreparations`, `OrderChanges`, `OrderItems`, `OrderServices`, `OrderJourneys`, `PriceChangeSets`, `PricingLines` |
| `DropForeignKey` | 18 | The single-column form of each relation being replaced |
| `AddForeignKey` | 19 | The composite form (18 replacements + the new `CommandReceipts` -> `Orders`) — see `CURRENT-OWNERSHIP-FK-MATRIX.md` |
| `DropIndex` | 16 | Single-column indexes that backed the dropped foreign keys |
| `CreateIndex` | 17 | Composite indexes backing the new foreign keys, plus `IX_OrderChanges_OrderId_CommercialVersion` recreated as **unique** |
| `AddCheckConstraint` | 39 | The enum vocabulary constraints — see `ENUM-CONSTRAINT-MATRIX.md` |
| `AlterColumn` | 4 | `OrderSegmentLegs.OriginAirportId`, `.DestinationAirportId`, `.DepartureDateTime`, `.ArrivalDateTime` — nullable to non-null |

Each operation was read individually. The four `AlterColumn`s are the only ones that touch a column definition; the
other 115 add or replace constraints and indexes.

## 3. The four `AlterColumn` operations

`OrderSegmentLeg` required facts became non-nullable in the domain model in this stage, because a leg without an
origin, a destination, a departure or an arrival is not a leg: `CandidateValidator.EnsureLegs` rejects it at the
candidate boundary, so no code path could ever have written null. The columns were nullable only because the model
had not been tightened.

EF emits `defaultValue: 0` for the two `int` columns and `defaultValue: DateTimeOffset.MinValue` for the two
`datetimeoffset` columns. That value is used only to back-fill rows that already hold NULL.

**Risk, stated plainly.** If a database existed with NULL leg facts, this migration would silently replace them with
`0` and `0001-01-01T00:00:00+00:00` instead of failing. That is exactly the kind of speculative backfill this stage is
supposed to refuse.

Why it is nonetheless correct here: there is no deployed S1 database (owner decision, `S1-MIGRATION-REBASELINE-OPEN-DECISION.md`),
and no row in any environment can hold a NULL in these columns, because every leg that ever reached SQL Server passed
`EnsureLegs` first. The back-fill is unreachable. It is recorded here rather than left as an unremarked default.

If a deployed database ever does exist before this migration is applied, the correct action is to run
`SELECT COUNT(1) FROM [Order].[OrderSegmentLegs] WHERE [OriginAirportId] IS NULL OR [DestinationAirportId] IS NULL OR
[DepartureDateTime] IS NULL OR [ArrivalDateTime] IS NULL` first and stop if it is not zero.

## 4. The one index that changed semantics

`IX_OrderChanges_OrderId_CommercialVersion` was a non-unique index and is now **unique**. Pack
`DOMAIN/10-ELIGIBILITY-VERSIONS-AND-TIME.md` makes `CommercialVersion` the per-Order commercial version counter; two
changes at the same version in one Order is a contradiction that the database now refuses. In S1 there is exactly one
change per Order, so no existing row can collide.

Evidence: `AcceptedScopeConstraintTests.A_commercial_change_cannot_carry_a_second_price_change_set` and
`A_different_commercial_change_may_carry_its_own_price_change_set` bracket the rule from both sides.

## 5. Reversibility

`Down()` mirrors `Up()` exactly: 39 `DropCheckConstraint`, 19 `DropForeignKey`, 8 `DropUniqueConstraint`, the index
swaps in reverse, and 4 `AlterColumn` back to nullable. It contains no data operation either. The migration is
reversible without loss on an empty or S1-shaped database.

## 6. Verification performed

| Check | Result |
|---|---|
| `dotnet build AeroTech.Ordering.sln` | succeeded, 0 errors |
| `dotnet ef migrations has-pending-model-changes` — `OrderingDbContext` | No changes |
| `dotnet ef migrations has-pending-model-changes` — `OrderQueryDbContext` | No changes |
| `dotnet ef migrations has-pending-model-changes` — `ReferenceDbContext` | No changes |
| Fresh database from the full chain | every persistence test creates one; 230 tests pass |
| Upgrade from a previous-stage database | `MigrationUpgradeTests.S1_migration_upgrades_a_b0_database_without_losing_outbox_or_inbox_rows` passes |
| Constraint inspection against the live database | `CurrentOwnershipConstraintTests`, `EnumConstraintTests` — 51 tests reading `sys.foreign_keys` and `sys.check_constraints` |
| Local dev database (`localhost\SQLEXPRESS`) | migration applied; chain up to date |

The migration was generated twice and discarded twice before this version: once after the `PricingLines` and
`FundingObligations` composite foreign keys were added, and once after `EnumConstraintTests` found the two missing
enum constraints. Each time the draft was removed with `dotnet ef migrations remove` and regenerated from the final
model, so the committed migration is generated from the shape the tests actually prove — not from an intermediate one.
