# D0 — Plan

Authority: `docs/ORDERING-DESIGN-PACK-v3.8/` (`SLICES/D0.md`, `RUNBOOKS/D0.md`, `REVIEW/02-TARGET-BASELINE.md`).
Scope: inspection and evidence only. No business code, no entity, no project/reference change.

| Requirement | Proof |
|---|---|
| INV-001, SC-D0-001 — correct repository, worktree preserved, drift from reviewed bootstrap inspected | `01-runs/01..06` |
| SC-D0-002 — shell findings read; every discrepancy has one named B0 action | `REPORT.md` §4 |
| Actual restore/build result | `01-runs/09-restore.txt`, `01-runs/10-build.txt` |
| Dependency graph and Domain→Messages usage | `02-architecture/DEPENDENCIES.md` |

Files written: only under `reports/01-D0-conformance/`. `CLAUDE.md` was recreated on the owner's instruction before D0.
