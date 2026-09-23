# Migration impact — `S1HistoricalIdentityCorrection`

Stage: 09-S1-historical-identity-and-pack-reference · 2026-09-21 · branch `k8s-stg`

File: `src/AeroTech.Ordering.Persistence/Migrations/20260921194857_S1HistoricalIdentityCorrection.cs`
Context: `OrderingDbContext`.

## 1. Decision: one minimal correction migration, no rebaseline

`S1AuthoritativeFinalShape` is already committed at `14758c6`, so its wrong foreign keys cannot be edited out of
history. This migration corrects them forward. The chain is now eleven migrations and is still **not** rebaselined:

```
20260917144148_B0InfrastructureShell
…
20260920223736_S1ClosureScopeAndChangeSetConstraints
20260921000413_S1AuthoritativeFinalShape
20260921194857_S1HistoricalIdentityCorrection   <- this stage
```

Rebaseline remains the explicitly deferred `S1_MIGRATION_REBASELINE_AND_FREEZE` stage, subject to the owner
authorisation recorded in `reports/00-decisions/S1-MIGRATION-REBASELINE-OPEN-DECISION.md`.

## 2. Every operation in `Up()`

26 operations. All are foreign-key and index swaps.

| Operation | Count |
|---|---|
| `DropForeignKey` | 7 |
| `AddForeignKey` | 7 |
| `DropIndex` | 6 |
| `CreateIndex` | 6 |

**Zero** of each of: `DropColumn`, `RenameColumn`, `DropTable`, `AlterColumn`, `AddCheckConstraint`,
`DropCheckConstraint`, `AddUniqueConstraint`, `DropUniqueConstraint`, `migrationBuilder.Sql(…)`, `InsertData`,
`UpdateData`, `DeleteData` — verified by grep over the whole file, `Up()` and `Down()`. No data reset, no semantic
rename, no unrelated schema edit.

### 2.1 The seven foreign keys replaced

| Dropped | Added |
|---|---|
| `FK_OrderServices_OrderChanges_OrderId_CreatedByChangeId` | `FK_OrderServices_OrderChanges_CreatedByChangeId` |
| `FK_OrderItemServiceLinks_OrderItems_OrderIdAtAssociation_OrderItemId` | `FK_OrderItemServiceLinks_OrderItems_OrderItemId` |
| `FK_OrderItemServiceLinks_OrderServices_OrderIdAtAssociation_OrderServiceId` | `FK_OrderItemServiceLinks_OrderServices_OrderServiceId` |
| `FK_PricingLines_OrderItems_OrderId_OrderItemId` | `FK_PricingLines_OrderItems_OrderItemId` |
| `FK_FundingObligations_OrderItems_OrderId_OrderItemId` | `FK_FundingObligations_OrderItems_OrderItemId` |
| `FK_FundingObligations_OrderServices_OrderId_OrderServiceId` | `FK_FundingObligations_OrderServices_OrderServiceId` |
| `FK_FundingObligations_PricingLines_OrderId_PricingLineId` | `FK_FundingObligations_PricingLines_PricingLineId` |

### 2.2 The six index swaps

Each composite index existed only to back the composite foreign key it accompanied; each new single-column index
backs its replacement.

| Dropped | Created |
|---|---|
| `IX_OrderServices_OrderId_CreatedByChangeId` | `IX_OrderServices_CreatedByChangeId` |
| `IX_OrderItemServiceLinks_OrderIdAtAssociation_OrderItemId` | *(covered by the existing unique pair index)* |
| `IX_OrderItemServiceLinks_OrderIdAtAssociation_OrderServiceId` | `IX_OrderItemServiceLinks_OrderServiceId` |
| `IX_FundingObligations_OrderId_OrderItemId` | `IX_FundingObligations_OrderItemId` |
| `IX_FundingObligations_OrderId_OrderServiceId` | `IX_FundingObligations_OrderServiceId` |
| `IX_FundingObligations_OrderId_PricingLineId` | `IX_FundingObligations_PricingLineId` |
| — | `IX_PricingLines_OrderItemId` |

`IX_PricingLines_OrderId_OrderItemId` is **not** dropped: it predates stage 08 and serves ordinary queries. No
occurrence `OrderId` column lost its index.

## 3. Alternate keys preserved

Per the stage prompt §D, the current-containment alternate keys are kept even where no foreign key now consumes
them, because later slices still map against them:

`AK_Orders_OwnerAirlineId_Id`, `AK_OrderPreparations_OwnerAirlineId_Id`, `AK_OrderChanges_OrderId_Id`,
`AK_OrderItems_OrderId_Id`, `AK_OrderServices_OrderId_Id`, `AK_OrderJourneys_OrderId_Id`,
`AK_PriceChangeSets_OrderId_Id`, `AK_PricingLines_OrderId_Id`, plus the pre-existing
`AK_OrderTravellers_OrderId_Id` and `AK_OrderSegments_OrderId_Id`.

**One was nearly lost.** The first draft of this migration contained
`DropUniqueConstraint("AK_OrderServices_OrderId_Id")`: EF removed it because, once the two foreign keys that used it
as a principal key were relaxed, nothing referenced it. It had never been declared explicitly — stage 08 created it
implicitly through `HasPrincipalKey`. The draft was discarded, `HasAlternateKey(service => new { service.OrderId,
service.Id })` was added to `OrderServiceConfiguration`, and the migration regenerated. The committed version
contains no `DropUniqueConstraint`.

## 4. No column, constraint or data change

- No occurrence column was dropped. `OrderItemServiceLinks.OrderIdAtAssociation`, `PricingLines.OrderId` and
  `FundingObligations.OrderId` are unchanged — still non-null, still indexed, still foreign keys to `Orders` through
  the Order aggregate's child collections.
- All 39 enum `CHECK` constraints from stage 08 are untouched.
- All semantic `CHECK` constraints are untouched.
- `IX_OrderChanges_OrderId_CommercialVersion` stays unique.
- The four `OrderSegmentLegs` columns stay non-null.

## 5. Reversibility

`Down()` mirrors `Up()` exactly: 7 `DropForeignKey`, 7 `AddForeignKey` restoring the composite form, and the index
swaps in reverse. It contains no data operation. The migration is reversible without loss.

Note the asymmetry that makes this correction necessary in the first place: `Down()` would restore constraints that
make a Pack-mandated split impossible. It exists for mechanical completeness, not as a recommended action.

## 6. Verification performed

| Check | Result |
|---|---|
| `dotnet build AeroTech.Ordering.sln` | succeeded, 0 errors |
| `has-pending-model-changes` — `OrderingDbContext` | No changes |
| `has-pending-model-changes` — `OrderQueryDbContext` | No changes |
| `has-pending-model-changes` — `ReferenceDbContext` | No changes |
| Fresh database from the full eleven-migration chain | every persistence test creates one |
| Upgrade from a previous-stage database | `MigrationUpgradeTests` passes |
| Constraint inspection against the live database | `CurrentOwnershipConstraintTests` (14 + 7 relations), `EnumConstraintTests` |
| Behavioural proof of the corrected shape | `HistoricalIdentityMobilityTests` (8 tests), red against the pre-correction schema |

The migration was generated twice: once before the `AK_OrderServices_OrderId_Id` fix (§3) and once after. The
committed version is generated from the final model, and every one of its 26 operations was read individually.
