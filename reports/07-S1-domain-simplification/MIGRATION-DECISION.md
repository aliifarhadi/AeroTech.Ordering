# Migration Decision

Stage: 07-S1-domain-simplification (revision 2) · 2026-09-20 · branch `k8s-stg`

**Status: `PROPOSED_REBASELINE_AWAITING_OWNER_AUTHORIZATION`.**

## Correction to the previous revision of this document

The previous revision said the owner "answered the migration question with *No deployed data — rebaseline*" and
described the collapse as "the one the owner approved". There is **no decision file in `reports/00-decisions/`
recording that**, so this document must not claim it. The rebaseline is a proposal by the agent, awaiting the owner's
explicit authorization. The request is written up in
`reports/00-decisions/S1-MIGRATION-REBASELINE-OPEN-DECISION.md`.

## What exists in the chain today

```
20260917144148_B0InfrastructureShell            inbox + outbox only
20260918201810_S1OrderCreate
20260918224935_S1DomainParityTypedStructures
20260918225050_S1DomainParityBackfill
20260918231551_S1DomainParityDropRetiredColumns
20260919133032_S1ClosureDomainRepair
20260919204507_S1DomainSimplification           ← disposable, from revision 1
20260919222251_S1SimplificationR2Corrections    ← disposable, from this revision
20260920223736_S1ClosureScopeAndChangeSetConstraints  ← additive, from the post-R2 closure pass
```

The last entry was added by the independent closure pass and is **not** part of the disposable scaffolding argument
below: it carries only the two new database constraints that closure required — alternate keys `(OrderId, Id)` on
`OrderTravellers` and `OrderSegments`, the composite foreign keys that confine an air service's scope to its own
Order, and the unique index on `PriceChangeSet.ChangeId`. It contains **no** `CreateTable`, `DropTable`, `AddColumn`,
`DropColumn`, `AlterColumn` or `RenameColumn`, and no raw SQL — 89 lines of keys, indexes and foreign keys only. It
folds into the proposed `S1OrderModel` baseline when that is authorized, and nothing was deleted to add it.

Both of the last two are **disposable scaffolding**. They exist so that the corrected model could be proven against a
real SQL Server database before anything is frozen. Neither should ship.

## Why the additive chain must not become the baseline

EF's rename detection is a heuristic over column shape. On a change this large it matched columns that have nothing to
do with each other. Every one of these is in `20260919204507_S1DomainSimplification`:

| Table | Scaffolded rename | Reality |
|---|---|---|
| `PricingLines` | `Role` → `SaleValueCurrencyId` | a line role enum becoming a currency identity |
| `OrderServices` | `SupplierPartyRef` → `BookingClass` | a supplier reference becoming a booking class |
| `OrderServices` | `DocumentAuthority` → `CheckedBaggageWeightUnit` | unrelated |
| `OrderServices` | `CapacityUnits` → `CheckedBaggagePieces` | unrelated |
| `Orders` | `TicketingValidityState` → `CustomerTotalCurrencyId` | a validity state becoming a currency identity |
| `Orders` | `PriceValidityState` → `CurrencyId` | same defect |
| `OrderPreparations` | `PriceValidityValue` → `OfferExpiresAt` | offer validity is the correct source, not price validity |
| `OrderPreparations` | `TicketingValidityValue` → `PriceValidUntil` | price validity is the correct source |
| `OrderItems` | `TermsUpgradeEligibility` → `AcceptedTotalCurrencyId` | a term state becoming a currency identity |

These are harmless **only** because no database holds S1 rows: the test fixture creates a fresh database per run and
drops it afterwards, so every renamed column is empty when the rename executes and the resulting schema is correct.
They would be data corruption against any populated database. This is the same class of defect as the
`BuyerActorId → SellerId` rename that had to be corrected by hand in the previous stage.

The R2 corrections then added a second layer of churn on top: a TPH discriminator on `OrderServices`, a restored
`OrderComponentTotals` with a different key, restored `OrderItemServiceLinks`, new nullable scope FKs on
`FundingObligations`, `Orders.BuyerContextType`/`BuyerId`/`SaleCurrencyCode`, `PricingLines.Role` and
`FarePricingUnits.SourceConstructionType`. Replaying seven migrations to build each test database is also the reason a
run now takes about 90 seconds instead of about 50.

## Proposed end state

1. Delete the five S1 migrations and their designers (10 files), `20260919204507_S1DomainSimplification` and its
   designer (2 files), and `20260919222251_S1SimplificationR2Corrections` and its designer (2 files).
2. Delete `tests/AeroTech.Ordering.Persistence.Tests/S1/LegacyOrderFixture.cs`, which became unreferenced when the
   parity-upgrade test lost its subject.
3. Reset `OrderingDbContextModelSnapshot.cs` to the `B0InfrastructureShell` state.
4. Generate one migration, `S1OrderModel`, straight from the final model on top of `B0InfrastructureShell`: the chain
   becomes infrastructure shell, then one honest S1 baseline of `CreateTable` only — no renames, no drops.
5. Re-run: full Domain tests, full Persistence/SQL Server tests, API/OpenAPI tests, architecture tests, a fresh
   database migration, `has-pending-model-changes`, and a manual read of the new snapshot.

## What the owner has to authorize

Two things, together:

- that **no deployed S1 data must survive** — there is no database outside this repository's test fixtures carrying
  `Order.*` rows from any of these migrations; and
- that the fifteen files above may be **deleted**.

Until both are on record in `reports/00-decisions/`, this status stays
`PROPOSED_REBASELINE_AWAITING_OWNER_AUTHORIZATION` and nothing is deleted.

A separate, mechanical obstacle also exists: in the previous revision, every attempt to remove a migration file
(`dotnet ef migrations remove`, `git rm`) was refused by this environment's destructive-action guard. That is a tooling
permission, not a decision, and it is recorded here only so the owner knows a second confirmation will be needed at the
moment of deletion.

## Tests that lost their subject

`MigrationUpgradeTests.S1_domain_parity_upgrade_preserves_accepted_rows_without_inventing_facts` asserted the backfill
behaviour of `S1DomainParityTypedStructures` / `S1DomainParityBackfill`. Those are the migrations the rebaseline
retires, and the current model cannot represent the rows the test seeded (a segment with no journey is no longer
expressible). The test was removed in revision 1.
`S1_migration_upgrades_a_b0_database_without_losing_outbox_or_inbox_rows` stays and now asserts generically that a B0
database has pending migrations and upgrades cleanly with its outbox and inbox rows intact, so it survives the
rebaseline unchanged.

## Leftover development databases

Two killed test runs in the previous revision left `OrderingS1_*` databases on `localhost\SQLEXPRESS` that the fixture
would normally have dropped. They belong to this suite only. They have not been dropped without the owner's word.
