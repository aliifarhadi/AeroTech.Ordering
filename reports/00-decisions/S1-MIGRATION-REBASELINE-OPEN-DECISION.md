# BLOCKED_DECISION — S1 migration rebaseline authorization

Raised: 2026-09-20 · branch `k8s-stg` · stage 07-S1-domain-simplification (revision 2)

**Status: OPEN — awaiting the owner's explicit answer. Nothing has been deleted.**

## Why this record exists

The independent R2 review found that the stage-07 reports claimed the rebaseline was owner-approved, while no decision
file in this folder records such an answer. The claim has been withdrawn from
`reports/07-S1-domain-simplification/MIGRATION-DECISION.md`, and the request is restated here so the answer has a home.

## The question

The S1 migration chain is now eight migrations, of which the last two exist only as scaffolding so that the corrected
model could be proven against SQL Server. The intended end state is one `B0InfrastructureShell` plus one honest
`S1OrderModel` baseline of `CreateTable` only.

Reaching it requires deleting fifteen files:

| Files | Why |
|---|---|
| `20260918201810_S1OrderCreate` (+ designer) | superseded by the baseline |
| `20260918224935_S1DomainParityTypedStructures` (+ designer) | superseded |
| `20260918225050_S1DomainParityBackfill` (+ designer) | superseded |
| `20260918231551_S1DomainParityDropRetiredColumns` (+ designer) | superseded |
| `20260919133032_S1ClosureDomainRepair` (+ designer) | superseded |
| `20260919204507_S1DomainSimplification` (+ designer) | contains nine semantically wrong scaffolded column renames |
| `20260919222251_S1SimplificationR2Corrections` (+ designer) | scaffolding for the R2 corrections |
| `tests/AeroTech.Ordering.Persistence.Tests/S1/LegacyOrderFixture.cs` | unreferenced once the parity-upgrade test lost its subject |

## What the agent needs

Two answers, together. Both must be explicit; neither is assumed.

**Q1. Is there any deployed database — outside this repository's per-run test fixtures — holding `Order.*` rows created
by any of the S1 migrations above, whose data must survive?**

Answer:

**Q2. May the fifteen files listed above be deleted, so that a single `S1OrderModel` baseline can be generated from the
final model on top of `B0InfrastructureShell`?**

Answer:

## What happens on each answer

- **Q1 = no deployed data, Q2 = yes.** The agent deletes the fifteen files, resets
  `OrderingDbContextModelSnapshot.cs` to the `B0InfrastructureShell` state, generates `S1OrderModel`, and re-runs the
  full sequence: Domain tests, Persistence/SQL Server tests, API/OpenAPI tests, architecture tests, a fresh database
  migration, `has-pending-model-changes`, and a manual read of the new snapshot.
- **Q1 = there is deployed data.** No rebaseline. The additive chain must instead be hand-corrected: the nine wrong
  renames become explicit drop-and-add pairs, and each restored table gets a deliberate backfill. That is a different
  and larger piece of work, and it needs its own plan.
- **Q2 = no, keep the history.** Same as above: the chain stays, and the nine wrong renames must still be corrected by
  hand before any database with rows is migrated.

## Secondary note, not a decision

Every attempt to remove a migration file in the previous revision (`dotnet ef migrations remove`, `git rm`) was refused
by this environment's destructive-action guard. That is a tooling permission rather than a decision; if Q1 and Q2 are
answered in favour of the rebaseline, the owner will also need to allow the deletion at the moment it runs.

## Related

- `reports/07-S1-domain-simplification/MIGRATION-DECISION.md`
- `reports/07-S1-domain-simplification/PACK-REQUIRED-SHAPE-GAP-MATRIX.md` §7
