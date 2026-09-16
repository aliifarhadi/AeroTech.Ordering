# Implementation instructions - binding workflow

## Project identity and source rule

Work only in `aliifarhadi/AeroTech.Ordering`, the existing .NET10 target. The reviewed bootstrap is k8s-stg@2f2b9033f675387f68ef7ff95c9d0a575d836fb5; record current drift without resetting. Keep the solution/layer names. Do not copy old Domain/Application, business migrations, integrations or P2/P3 services. The target already contains framework/shell; additional generic copying is exceptional, file-allowlisted and limited to B0.

After B0, the new agent must not need to clone/read another business repository to implement Ordering. This pack contains the required semantics and observed owner contract maps. Missing owner guarantees are explicit blockers for the real binding, not permission to consult legacy behavior as authority or to invent an API. A future owner-approved contract/fixture update can change a real adapter after a compatibility review; it cannot silently change the domain.

## Exact execution order

Execute START-HERE, D0, B0, then S1 through S16. Read the full domain specification before business code; introduce tables/handlers by slice. D0 is source/design conformance, not an invitation to redesign the pack. Before each slice record a bounded file/layer/test plan, then implement and run it. Do not stop with a plan instead of a working slice.

At the end of EACH stage create and deliver the full stage evidence bundle and visible checkpoint before continuing. Continue automatically when LOCAL_DONE prerequisites pass. A genuinely unresolved local invariant blocks dependent execution; an unrelated owner binding only blocks that live capability. Never bypass a failed gate by relabeling it as deferred cleanup. Use a written resume checkpoint if the execution session must end; do not claim unperformed background work.

## Code principles

Domain behavior owns invariants and immutable facts; public setters/generic CRUD endpoints cannot mutate business truth. Application coordinates explicit use cases and transaction boundaries using application-owned ports. Persistence implements SQL constraints/stores, not business policy. Provider adapters normalize owner evidence without changing the Order. Query reads local denormalized facts. Synchronizer is the sole enlisted OrderProjector. Controllers/consumers authorize and translate, then call one canonical handler. ServiceHost wires explicit capabilities and validates configuration.

Use the existing generic ID/clock mechanisms and established error conventions where compatible. No second framework, generic workflow language, reflection-based business-rule engine, universal Order status, omnibus ProviderInteraction aggregate or hidden automatic pricing behavior. Add a narrow value type only when its actual invariant and scope justify it; do not invent another cross-service currency/FX master.

Every money field has explicit scale/currency/provenance. Equality and conservation use decimal, not binary floating point. Domain tests include sign/effect and original/equivalent distinction. File names/folders do not establish correct layer placement: dependency and type-use tests do.

## Required runtime behavior

Receipt lookup precedes new eligibility. A terminal replay returns the original result; a pending replay resumes the saved operation with its original key, exact request, profile and authority. No remote call runs inside a SQL transaction. Persist intent before external mutation, then retain authoritative outcome and project locally. Worker lease expiry never releases a commercial claim. Response loss cannot be converted into permanent rejection or an invented confirmation.

Create calls no owner. Prepare may resolve/reprice before explicit acceptance. Issue requires committed necessary capacity and operation-bound funding authority, but Ledger is not its gate. Local document issuance is a local atomic root/stock commit, not a fake external API. External document authority has per-document evidence and exact recovery. Commercial pivot and later financial/provider cleanup have separate version and economic-event treatment.

## Tests and reporting

Run real SQL tests, host/API/authorization tests, shared provider contract tests and actual process-restart/fault scenarios. No required test may be skipped while claiming LOCAL_DONE. Record zero-test filters as failure. Use only disposable environments. Source review, fixture schema validation and compilation are not live E2E proof. Re-read changed source and inspect real test output before accepting another implementation agent's report.

Never delete an assertion to obtain a green report or substitute a broad TODO for a missing capability. A failed architecture assertion must be repaired at its assigned layer, not disabled. No new business endpoint is wired as production-ready with throwing/unimplemented handlers. Explicit unavailable real bindings can remain disabled while complete reference behavior is exercised.

## Change control without bureaucracy

Ordinary private refactors within the current slice are allowed if invariants/contracts remain unchanged and tests pass. A semantic/ownership/identity/price/version/transaction change requires a concise decision record, exact impacted contracts/scenarios, migration and regression tests before code. Missing owner semantics use BLOCKED_DECISION with evidence. Do not ask the user to choose representation details already settled here, but do not approve actual airline funding/issuer/control policy for yourself.

Production deployment, shared environment data mutation, owner-service changes and legacy data cutover are not implied by implementation in the target repository. Keep secrets out of source/evidence, preserve existing work and follow repository branch/commit authorization.
