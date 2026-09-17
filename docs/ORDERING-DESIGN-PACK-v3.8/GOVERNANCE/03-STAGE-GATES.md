# Stage gates, checkpoints and evidence

## Completion vocabulary

- `NOT_STARTED`, `IN_PROGRESS`, `FAILED`, `BLOCKED_ENVIRONMENT`, `BLOCKED_DECISION` describe execution status.
- `LOCAL_DONE` means all mandatory domain/application/SQL/API/simulator/recovery and architecture tests for this stage passed in an actual run.
- `LIVE_CANDIDATE_PROVEN` is specific to S1: an actual AirOffer candidate passed the pre-acceptance bridge and local sandbox Create/Get. It is not an owner validity guarantee.
- `LIVE_CERTIFIED` belongs to a named capability+profile+contract+environment, not vaguely to a service. It requires real mapping, actual owner effects/read-back, failure evidence and semantic approval.
- `RELEASE_CERTIFIED` belongs to S16 and a deployment scope. It requires all live capabilities used by that scope and operational/security gates. No simulator records may be sold as live inventory/documents.

An unresolved live provider must be shown next to LOCAL_DONE, e.g. `LOCAL_DONE; JetPay/coverage LIVE_BLOCKED(BD-005)`. A skipped required local test is not LOCAL_DONE. After LOCAL_DONE, later simulator-dependent stages may be semantically unblocked; this is not execution permission. Actual progression to the next stage still follows GOVERNANCE/05 C12 and the user's current instruction. LOCAL_DONE cannot claim live completion or production enablement.

## Mandatory stage bundle

Produce stage evidence containing the following information. The storage path/format follows the repository/team convention; this pack does not mandate an `artifacts/stages` directory:

```text
STATUS.json
REPORT.md
SOURCE-COMMIT.txt
WORKTREE-DIFF.patch       # only if unavoidable; not acceptable for release certification
API/openapi.json
API/requests.http
DB/migrations.txt
DB/constraints.md
CONTRACTS/bindings.json
TESTS/results.trx
TESTS/scenario-results.json
E2E/transcript.md
E2E/provider-effect-counts.json
RECOVERY/fault-matrix.json
ARCHITECTURE/dependencies.json
BLOCKERS.md
RUNBOOK.md
```

For D0/B0 fields without business applicability, provide explicit `NotApplicable` reason instead of fabricated evidence. Never invent test IDs, transcripts, counts, source commits or live reference numbers. Run logs are redacted, not falsified.

`SOURCE-COMMIT` identifies the implementation that was tested. Evidence may be committed in a following evidence-only commit; record both. Do not attempt to write a commit's own hash inside itself. Every test result includes profile, data fixture revision, migration version, test name and result. No aggregate count replaces required scenario coverage.

## Checkpoint response

At the end of EVERY stage, before starting the next, provide: stage/status; implementation commit; what can now be executed; exact local terminal command and input file; actual test/E2E results; live vs simulated owners; remaining blockers; downloadable/committed evidence path. Then continue when allowed. Do not stop after a layer implementation and call it a finished stage.

## Universal gates

Clean checkout and migrations from zero; upgrade from previous stage; negative authorization; idempotent replay/conflict/race; response-loss recovery; no network in SQL TX; single handler per semantic command; single projector writer; invariants and examples remain stable; read model works with owners offline; all active operations survive restart. Exact requirements per slice extend these, never weaken them.


Limit any worktree patch to the tested implementation paths. Preserve unrelated changes without including their contents in the evidence bundle. Redact secrets and protected personal data; do not store access tokens, production connection strings or unrelated private source in logs/patches. A redacted patch cannot replace a reproducible tested commit for release certification.
