# Start here - exact day-zero protocol

## 1. Work in the existing repository

The repository is `aliifarhadi/AeroTech.Ordering`, not `aliifarhadi/Ordering`. The latter is a historical framework source only. The target's reviewed bootstrap is `2f2b9033f675387f68ef7ff95c9d0a575d836fb5` on `k8s-stg`.

Run these commands in a terminal on the developer machine / agent workspace, NOT inside Rancher and NOT against a production database:

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

Record the actual HEAD. If it has advanced, inspect the diff from the reviewed bootstrap before changing anything. Do not reset to the reviewed commit, discard changes, remove another user's files, overwrite existing migrations, or create another solution. Create a feature branch only after confirming the worktree changes will be preserved. Lack of an upstream remote is not a reason to initialize another repository.

## 2. Read the authoritative files in this order

1. README, this file, GOVERNANCE/01-NONNEGOTIABLES, GOVERNANCE/02-DECISIONS, GOVERNANCE/03-STAGE-GATES.
2. REVIEW/01-PREVIOUS-PACK-REVIEW and REVIEW/02-TARGET-BASELINE.
3. ARCHITECTURE/01-LAYERS through 04-SECURITY-AND-OPERATIONS.
4. DOMAIN/01-AGGREGATES through 15-EVIDENCE-TEMPORAL-CONSISTENCY, including the explicit money and document rules.
5. CONTRACTS/00-INTERACTION-CATALOG, CONTRACTS/01-COMMON-PROTOCOL and CONTRACTS/02-AIROFFER.
6. SPEC/invariants.json, SPEC/scenarios.json, SPEC/stages.json, BLOCKED-DECISIONS.

This is review, not permission to implement all future stages. The vocabulary and boundaries are fixed before S1; tables/handlers are introduced only by the active slice.

## 3. First output: D0, before business code

Create `artifacts/stages/D0/` with the actual repository/branch/HEAD inventory, project-reference graph, current build result, disposition of existing files, design reading checklist, an invariant-to-stage map and unresolved decisions. Do not claim the copied framework is correct merely because it compiles. Do not write an Order entity yet.

Run restore/build using the repository's pinned SDK/packages; record failure exactly if access/SDK is unavailable. No package upgrades or architecture renaming to hide a failed bootstrap. Test existing generic infrastructure using disposable local dependencies only.

D0 ends when every existing project is assigned a role and every discrepancy has an explicit B0 action. It must not become an open-ended design workshop. The target decisions in this pack already settle local architecture; only genuine business/owner ambiguities go into BLOCKED_DECISIONS.

## 4. Second output: B0, a running empty business service

Retain/correct the existing layer shell. Apply ARCHITECTURE/02-BOOTSTRAP actions. Copy from old Ordering only a missing, reviewed, generic file; record exact source commit/path/hash and destination. No wildcard copy. The normal path for this target is **zero further old-repository copies**.

Provide a clean build, architecture tests, disposable SQL Server, fresh infrastructure migration, authenticated host smoke tests, liveness/readiness, outbox/inbox crash tests and a reproducible start command. No business route yet. Then publish B0 evidence and continue to S1.

## 5. S1 is the first business output

Implement `PrepareOrderFromOffer -> explicit acceptance -> CreateOrderFromOffer -> GetOrder`. The preparation is not an Order, not an inventory hold and not a payment. AirOffer's current Details read is permitted only in the pre-acceptance preparation, never in Create after acceptance. Caller binds named travelers to owner traveler references; names and financial customer identity are not invented from offer totals.

Run simulator E2E and the live AirOffer candidate E2E using the included runbook. Differentiate `LIVE_CANDIDATE_PROVEN` from `LIVE_CERTIFIED(AcceptedOffer)` when AirOffer has not supplied required validity/ownership guarantees. Never call S1 live-certified after testing only a fake offer. Missing connectivity is `BLOCKED_ENVIRONMENT`; missing semantics is `BLOCKED_DECISION`.

## 6. Continue without drifting

At each checkpoint produce actual evidence, a source commit and exact reproduction command. Then move to the next stage when its LOCAL_DONE prerequisites pass. A missing future live adapter does not prevent later simulator slices. A failed invariant or unresolved local semantic prerequisite does prevent the affected dependent slice. Do not skip to later screens or add placeholder handlers to make a progress report longer.
