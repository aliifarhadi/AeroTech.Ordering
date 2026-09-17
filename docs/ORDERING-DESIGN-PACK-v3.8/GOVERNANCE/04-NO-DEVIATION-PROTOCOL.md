# Agent execution protocol and review gates

Before editing a slice, write a small `slice-plan.json` listing its stage ID, requirement/scenario IDs, intended files by layer, migrations and external capabilities. This is a concrete work manifest, not a new design proposal. Check the declared dependency graph before building.

Implement in this sequence WITHIN a slice: contract examples and failing acceptance test; domain rules; use-case orchestration; real SQL persistence/constraints; projector/read path; ACL/simulator; API or consumer wrapper; dependency injection; runbook; crash/concurrency tests; final evidence. Run smoke tests throughout. This sequence is not permission to finish all domains before any executable API.

Independent review after an Agent report: inspect the actual diff, public call path, DI resolution in all modes, SQL transaction boundaries, migrations and mandatory tests. Re-run at least the stage acceptance suite. Check worker execution as well as HTTP handlers. A report's assertion is never evidence of an unseen test.

Forbidden shortcuts include: in-memory persistence replacing SQL, hardcoded owner success, swallowed ContractMismatch, reading new prices in Create, bypassing coverage for zero/unknown amount, false 204 success inference, catching all DbUpdateException as duplicate, emitting events directly before commit, reconstructing an old external request from current Order state, and moving business logic into Controllers/Consumers/Providers for convenience.

No long-lived operations are implemented as request-scoped fire-and-forget Tasks. Durable operations are resumed by a hosted worker calling the canonical application coordinator. Request cancellation after persisted acceptance does not delete the operation.

When blocked, complete independent safe work and deliver the current checkpoint with exact scope of the blocker. Do not choose a cross-service business meaning from intuition. Do not ask the user to locate a file already present in the target or this pack. Do not delete the workspace as an independence test; use a clean independent checkout.
