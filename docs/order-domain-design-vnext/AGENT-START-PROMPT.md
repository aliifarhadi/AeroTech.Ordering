# Agent start prompt - AeroTech.Ordering Day 0 / Pack 2.0

You are implementing a fresh airline Ordering service in the EXISTING repository:
https://github.com/aliifarhadi/AeroTech.Ordering

Use the attached ORDERING-DAY0-DESIGN-PACK-v2.0 as the sole target business specification. It supersedes previous FreshBuild/VNext prompts. This is an implementation task, not another architecture proposal. First execute D0 and B0, then vertical slices S1..S16 in order, delivering a runnable/tested checkpoint at the end of EVERY stage.

## First actions - do exactly this

1. Locate the existing target checkout. If absent, clone AeroTech.Ordering branch k8s-stg; do not clone old aliifarhadi/Ordering as the working project. Run git remote -v, git status --short, git branch --show-current, git rev-parse HEAD and git log -5 --oneline in the local developer/agent terminal. Record output. Reviewed bootstrap is 2f2b9033f675387f68ef7ff95c9d0a575d836fb5. Inspect changes since it; do not reset/discard work or create a second solution/repository.
2. Put the supplied pack under docs/day0-v2.0 without overwriting unrelated work. Read README, START-HERE, IMPLEMENTATION-INSTRUCTIONS, GOVERNANCE/01..04, REVIEW/01..03, ARCHITECTURE/01..04, DOMAIN/01..15, CONTRACTS/00..11 and SPEC indexes. In particular read the prior-pack findings BEFORE touching copied framework or business code.
3. Execute SLICES/D0.md. Produce artifacts/stages/D0 inventory, actual restore/build result, dependency graph, design conformance, exact B0 file dispositions and blockers. Do not write Order/domain entities during this initial inventory. A failed baseline build is reported, not hidden by replacing the architecture.
4. Execute SLICES/B0.md in this existing shell. Correct the documented Domain->Messages dependency, caller-context placement, monetary mappings and messaging/transaction hazards. Reuse generic framework; do not bulk-copy old business code or migrations. Any genuinely missing generic file copied during B0 needs source commit/path/hash and destination. After B0 the implementation must be independent of all other business repository source code.
5. Deliver a running authenticated empty-business host, disposable SQL/broker/reference infrastructure, architecture/DI/SQL/inbox/outbox tests and the required Start-Local/Test-Stage scripts. Publish actual B0 evidence, then begin S1 without asking for a generic continue instruction.

## S1 - first business milestone

Implement PrepareOrderFromOffer -> explicit acceptance of stored digest -> CreateOrderFromOffer -> GetOrder. Current AirOffer Details invokes PriceAsync; use it only BEFORE acceptance. Create must consume the exact stored preparation and perform ZERO owner calls, holds, payment or issue operations. Atomically save receipt/preparation consumption, Order/items/typed services/pricing/change/projection/outbox. Replay the same key; reject changed payload and duplicate preparation consumption; test concurrent requests, restart, read-model rebuild and GetOrder with owners offline.

Use the real AirOffer adapter with the LIVE-CANDIDATE-SANDBOX profile to prove a real candidate reaches Create/Get; this does NOT certify absent owner validity/sales-context guarantees. Report LIVE_CANDIDATE_PROVEN separately from BD-001. Use fully capable reference owner profiles for later local stages. Never relabel unverified real source facts as authoritative to obtain a green demo.

## Non-negotiable architecture and domain rules

Domain is pure; no Messages/EF/HTTP/broker/config dependencies or provider calls. Application owns canonical use cases/ports and explicit transaction choreography. Persistence owns mapping/SQL, Providers own ACLs, Query is local read-only, Synchronizer has ONE OrderProjector in the committing transaction, RestApi/Consumers translate/authorize and dispatch only, ServiceHost is composition only. Follow SPEC/layers.json and test type dependencies, not only folders.

No Entitlement v1, no global mutable Order state machine, no duplicate Create/Reserve/Payment/Issue/Cancel rails, no legacy FulfillmentTask/TrafficDocument/PaymentService workflow copied. OrderItem is the sold pricing boundary; typed OrderService identity is stable; pricing/FX/source facts and membership history are immutable and changes append. Reversal sign, allocation, fare construction, shared beneficiaries, legs/segments, document purpose and all version scopes follow DOMAIN exactly.

For every external mutation persist immutable intent/profile/key/exact payload before dispatch; keep network outside SQL; apply authoritative evidence in a separate local TX. Terminal/pending receipt resolution precedes new eligibility. Unknown never means failure/success. Same-operation recovery, per-member outcomes, source read-back and worker fencing are mandatory. Lease expiry is not claim release. Ingest real observations even during claims and re-evaluate affected eligibility.

CanStartIssue and CanCommitDocuments are distinct. Final document issue requires committed required capacity, exact operation-bound funding authority and document/control eligibility. Ledger is async outbox only. Local ETKT/EMD authority uses local atomic stock/document commit; external authority uses certified per-document evidence/recovery. Do not invent JetPay guarantees, FlightFlow read-back, source expiry, issuer rules or ancillary pricing. Required missing owner bindings are BLOCKED_DECISION; reference simulators implement the specified target contract with independent durable state.

## Every stage must end with evidence

Before code for the stage, record a bounded PLAN.md linking exact command/invariant/scenario IDs and changed files/layers. Implement the full slice, not isolated layers. Execute real SQL, API/auth, domain/application, shared adapter contract and crash/restart tests. Re-read actual changed source and test output independently; do not trust an agent report alone.

Create the full artifacts/stages/<stage>/ bundle required by GOVERNANCE/03. Include tested source commit, API/openapi and requests, migrations/constraints, provider bindings, actual TRX/scenario results, E2E transcript/effect counts, fault matrix, dependency graph, blockers and exact local RUNBOOK. Do not invent passed tests, commits, approvals, provider refs or live certification. Zero tests or skipped required tests is not LOCAL_DONE.

At EVERY checkpoint respond with the stage/status, source commit, what now runs, exact local command/input, actual test results, real/simulated capability status, blockers and evidence path. Then continue to the next permitted stage automatically. A failed required local invariant blocks dependent work; missing unrelated live bindings do not stop complete reference slices. Do not ask questions answered by this pack or source in the target. Do not stop after another plan. Do not perform or promise asynchronous/background work.

Semantic changes require the decision/migration/regression record BEFORE code; ordinary conforming private refactors do not need a new design workshop. Production/shared-data changes, owner repository changes and legacy cutover require separate authorization. Preserve the worktree and secrets. Start now with the repository inventory in step 1.
