# Bootstrap inspection, reuse and corrections

## Inspected target baseline

The existing solution at `2f2b9033f675387f68ef7ff95c9d0a575d836fb5` already includes Framework, Domain, Application, Persistence, Providers, Query, Synchronizer, Consumers, RestApi, ServiceHost, ReferenceData, Messages, two test projects and the `Providers.Deterministic` project. A targeted drift check at `k8s-stg@957757471d9db3fde91922142d537a559b8084e4` found only deterministic-project activation/options/composition shell; `AddDeterministicProviders` registers no business adapters. Therefore slices must implement their required reference adapters in the existing project/seam rather than assume working deterministic providers already exist. Repository existence and source inspection do not establish that the shell builds or is transactionally correct.

## B0 action manifest

| Source in TARGET repo | Disposition | Required B0 work / verification |
|---|---|---|
| AeroTech.Ordering.sln | Retain | Inventory actual projects and keep names. Do not add/rearrange projects merely because the pack used a logical layer/example; add a project only under the user/team repository convention when a required responsibility cannot be placed correctly otherwise. |
| Framework/** generic files | Retain-and-certify | Preserve source/license/provenance; inspect base SaveChanges/event-dispatch behavior. No external publication before commit. No business dependency in generic framework. |
| src/AeroTech.Ordering.Domain/AeroTech.Ordering.Domain.csproj | Inspect/retain standing convention | The `AeroTech.Messages` project reference may remain. B0 must certify actual type usage rather than remove the reference by style: allow `AeroTech.Messages.Ordering.Enums.*` plus the existing caller-context types `BusinessContextType`, `PrincipalType`, and `AuthorizationSurface`; reject integration events, provider/owner wire DTOs, base transport messages and every other Messages type unless explicitly approved. Do not remove the project reference merely because Pack 2.0 preferred another layering style. |
| Domain/_Shared/Contracts/ICallerContext.cs and IHomeOperatorProvider.cs | Retain and certify exact role | `ICallerContext` may keep exactly the current platform identity enums (`BusinessContextType`, `PrincipalType`, `AuthorizationSurface`) under the explicit Messages allowlist; it is caller/actor context, not home-operator authority. `OwnerAirlineId` remains sourced through `IHomeOperatorProvider`, with the current `ReferenceDataHomeOperatorProvider` implementation, unless the user explicitly approves a platform change. Aggregates/domain behavior must not pull either ambient runtime context directly; application/host obtains trusted values and passes snapshots/arguments into behavior. |
| Persistence/OrderingDbContext.cs | Modify | Remove blanket decimal(18,2). Use explicit monetary/FX/quantity precision in the new mappings; default string(256) cannot truncate payloads/idempotency/opaque offer IDs. |
| Persistence/Inbox/InboxStore.cs | Retain-and-correct | Processed marker and durable local command acceptance/effects must share a transaction. Never a generic independent SaveChanges after an arbitrary multi-commit handler. |
| Consumers/Inbox/InboxConsumeFilter.cs | Modify | Missing MessageId cannot bypass dedup for required external facts. Catch only the identified unique-key duplicate, not arbitrary DbUpdateException; use fresh context/read-back on uncertain commit. |
| Consumers/Outbox and Persistence/Outbox | Retain-and-certify | Insert with business TX; publish outside TX; stable event identity; publisher confirms; replay after ack loss; multi-worker leases. |
| Application/_Shared/Events and Behaviors | Retain-and-certify | Logging redacts payloads; validation has no side effects; domain-event dispatcher never publishes a broker event synchronously in SaveChanges. |
| Synchronizer/DependencyInjection | Inspect/correct only if proven | Preserve the existing project ownership pattern if it can provide exactly one effective UnitOfWork and one projector. Remove duplicate/ambiguous registrations, not the architectural convention itself. |
| Query | Retain shell | Read-only connection/context; no business entities copied from old Ordering. |
| Providers and Providers.Deterministic | Retain shell | Explicit Disabled/Real/Simulator profiles with fail-closed startup; no production simulator fallback. |
| ServiceHost/Program.cs | Retain composition pattern | Validate registration modes and resolved implementation types, not order-dependent override chains. |
| Existing migrations/tests/config/deploy files | Inspect individually | Keep generic baseline only; do not delete unknown work. No old Order/business mappings/rows/credentials. Local disposable DB only for tests. |

The inbox filter currently enlists a marker, invokes the next consumer and persists it; whether a particular handler's writes are atomic depends on its transaction. This pack does NOT assert all existing consumers lose messages. B0 must prove the stronger target contract with crash tests rather than assume it.

## Copy allowlist when genuinely missing

A copy entry MUST identify old repository `aliifarhadi/Ordering`, commit `077a851217ab0fc3bce922973bb3c7f76e6f2f7f`, exact source path, blob/content hash, destination, retained generic purpose and edits. Eligible categories: generic framework bases, serializer/clock/ID implementations, test harness structure, build/analyzer/package settings and sanitized host/deployment skeleton. Never copy directories wholesale.

Deny: Order/OrderItem/OrderService business classes, old application handlers, FulfillmentTask/ProviderInteraction workflows, Payment aggregate/MockPayment, TrafficDocument, old business migrations, old read-model writers, old contracts treated as owner authority, business tests that cement obsolete behavior. A future behavior test must be written from this pack, not copied as a false oracle.

## Independence gate

In a fresh checkout of ONLY the target repository, restore/build/test/start with local SQL/broker/simulator dependencies. No relative references outside repository, old business NuGet assembly, git submodule of old Ordering, startup SQL reading another service DB, old repo path in build scripts or undocumented private template dependency. External deployed owner APIs are runtime integrations, not source dependencies. Normal NuGet dependencies remain allowed and pinned.

Record actual SDK/package/tool versions and license/feed constraints. Do not upgrade everything to latest. Required package access failures are environment blockers, not permission to replace the framework. SQL tests run only against a disposable, explicitly identified database; historical audit requests to avoid a production persistence suite are not permission to skip new isolated persistence tests.

## No speculative dependency rewiring

B0 must not apply a prewritten project-reference removal/addition table merely because it appears cleaner. Change a project reference only when (a) it violates an explicit standing repository rule or (b) actual source/test evidence proves it breaks one of the responsibility boundaries above. Record the exact reason.
