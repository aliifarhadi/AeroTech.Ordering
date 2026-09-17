# Start here - exact day-zero protocol

## 1. Work in the existing repository

The repository is `aliifarhadi/AeroTech.Ordering`, not `aliifarhadi/Ordering`. The latter is a historical framework source only. The target's reviewed bootstrap is `2f2b9033f675387f68ef7ff95c9d0a575d836fb5` on `k8s-stg`.

Use the existing repository/workspace and the team-approved source-control workflow. The commands below are examples for recording repository identity; they are not a mandate about terminal location or environment provisioning:

```powershell
# Only when the target is not already checked out:
git clone --branch k8s-stg https://github.com/aliifarhadi/AeroTech.Ordering.git
Set-Location AeroTech.Ordering
# When already checked out, enter that working directory instead.
git remote -v
git status --short
git branch --show-current
git rev-parse HEAD
git log -5 --oneline
Get-ChildItem -Name
```

Record the actual HEAD. If it has advanced, inspect the diff from the reviewed bootstrap before changing anything. Compare current `CLAUDE.md` against Pack 3.8. The user authorizes only the limited conflict-alignment described in GOVERNANCE/05 C18; record any edit and do not broaden it. Do not reset to the reviewed commit, discard changes, remove another user's files, overwrite existing migrations, or create another solution. Create a feature branch only after confirming the worktree changes will be preserved. Lack of an upstream remote is not a reason to initialize another repository.

## 2. Read the authoritative files in this order

1. README, this file, GOVERNANCE/01-NONNEGOTIABLES, GOVERNANCE/02-DECISIONS, GOVERNANCE/03-STAGE-GATES and GOVERNANCE/05-STANDING-RULE-CONFLICT-RESOLUTION.
2. REVIEW/01-PREVIOUS-PACK-REVIEW, REVIEW/02-TARGET-BASELINE and REVIEW/05-PACK-2.1-SCOPE-CORRECTION (historical predecessor correction retained for audit).
3. ARCHITECTURE/01-LAYERS through 04-SECURITY-AND-OPERATIONS.
4. DOMAIN/01-AGGREGATES through 15-EVIDENCE-TEMPORAL-CONSISTENCY, including the explicit money and document rules.
5. CONTRACTS/00-INTERACTION-CATALOG, CONTRACTS/01-COMMON-PROTOCOL and CONTRACTS/02-AIROFFER.
6. SPEC/invariants.json, SPEC/scenarios.json, SPEC/stages.json, BLOCKED-DECISIONS.

This is review, not permission to implement all future stages. The vocabulary and boundaries are fixed before S1; tables/handlers are introduced only by the active slice.

## 3. First output: D0, before business code

Record D0 evidence using the repository/team-approved reporting convention: actual repository/branch/HEAD inventory, project-reference graph, current build result, disposition of existing files, design reading checklist, invariant-to-stage map and unresolved decisions. Do not claim the copied framework is correct merely because it compiles. Do not write an Order entity yet.

Run restore/build using the repository's pinned SDK/packages; record failure exactly if access/SDK is unavailable. No package upgrades or architecture renaming to hide a failed bootstrap. Test existing generic infrastructure using the user/team-approved non-production test dependencies.

D0 ends when every existing project is assigned a role and every discrepancy has an explicit B0 action. It must not become an open-ended design workshop. The target decisions in this pack settle the design they explicitly cover. Any missing/ambiguous/contradictory material decision goes into `BLOCKED_DECISIONS` and requires explicit user approval; do not silently decide it from taste, benchmark preference or legacy behavior.

## 4. Second output: B0, a running empty business service

Retain/correct the existing layer shell. Apply ARCHITECTURE/02-BOOTSTRAP actions. Copy from old Ordering only a missing, reviewed, generic file; record exact source commit/path/hash and destination. No wildcard copy. The normal path for this target is **zero further old-repository copies**.

Provide a clean build, architecture checks, SQL Server migration/persistence verification in the approved test environment, authenticated-host smoke behavior using the existing security convention, liveness/readiness and outbox/inbox recovery tests where applicable. No business route yet. Then publish B0 evidence. Whether execution pauses or continues is controlled by the user/team, not by this pack.

## 5. S1 is the first business output

Implement `PrepareOrderFromOffer -> explicit acceptance -> CreateOrderFromOffer -> GetOrder`. The preparation is not an Order, not an inventory hold and not a payment. AirOffer's current Details read is permitted only in the pre-acceptance preparation, never in Create after acceptance. Caller binds named travelers to owner traveler references; names and financial customer identity are not invented from offer totals.

Run simulator E2E and the live AirOffer candidate E2E using the included runbook. Differentiate `LIVE_CANDIDATE_PROVEN` from `LIVE_CERTIFIED(AcceptedOffer)` when AirOffer has not supplied required validity/ownership guarantees. Never call S1 live-certified after testing only a fake offer. Missing connectivity is `BLOCKED_ENVIRONMENT`; missing semantics is `BLOCKED_DECISION`.

## 6. Continue without drifting

At each checkpoint produce actual evidence, a source commit and exact reproduction command. A later stage may only be considered semantically unblocked when its prerequisites pass. The user/team controls when the agent actually proceeds. A missing future live adapter does not prevent later simulator slices. A failed invariant or unresolved local semantic prerequisite does prevent the affected dependent slice. Do not skip to later screens or add placeholder handlers to make a progress report longer.
