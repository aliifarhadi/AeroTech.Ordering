# Bootstrap inspection, reuse and corrections

## Inspected target baseline

The existing solution at `2f2b9033f675387f68ef7ff95c9d0a575d836fb5` already includes Framework, Domain, Application, Persistence, Providers, Query, Synchronizer, Consumers, RestApi, ServiceHost, ReferenceData, Messages, two test projects and Providers.Deterministic. No target business implementation was executed during preparation of this pack. Repository existence and source inspection do not establish that the shell builds or is transactionally correct.

## B0 action manifest

| Source in TARGET repo | Disposition | Required B0 work / verification |
|---|---|---|
| AeroTech.Ordering.sln | Retain | Inventory actual projects and keep names; add missing test projects only. |
| Framework/** generic files | Retain-and-certify | Preserve source/license/provenance; inspect base SaveChanges/event-dispatch behavior. No external publication before commit. No business dependency in generic framework. |
| src/AeroTech.Ordering.Domain/AeroTech.Ordering.Domain.csproj | Modify | Remove reference to Contracts/AeroTech.Messages; keep Framework.Core/net10.0/nullable. Rehome wire enums to ACL/contract mapping; do not copy integration enums into a parallel shared domain library. |
| Domain/_Shared/Contracts/ICallerContext.cs and IHomeOperatorProvider.cs | Rehome interfaces as needed | Resolve runtime user/operator outside domain; domain receives explicit trusted values. |
| Persistence/OrderingDbContext.cs | Modify | Remove blanket decimal(18,2). Use explicit monetary/FX/quantity precision in the new mappings; default string(256) cannot truncate payloads/idempotency/opaque offer IDs. |
| Persistence/Inbox/InboxStore.cs | Retain-and-correct | Processed marker and durable local command acceptance/effects must share a transaction. Never a generic independent SaveChanges after an arbitrary multi-commit handler. |
| Consumers/Inbox/InboxConsumeFilter.cs | Modify | Missing MessageId cannot bypass dedup for required external facts. Catch only the identified unique-key duplicate, not arbitrary DbUpdateException; use fresh context/read-back on uncertain commit. |
| Consumers/Outbox and Persistence/Outbox | Retain-and-certify | Insert with business TX; publish outside TX; stable event identity; publisher confirms; replay after ack loss; multi-worker leases. |
| Application/_Shared/Events and Behaviors | Retain-and-certify | Logging redacts payloads; validation has no side effects; domain-event dispatcher never publishes a broker event synchronously in SaveChanges. |
| Synchronizer/DependencyInjection | Inspect/correct | One projector and NO second IUnitOfWork registration or extra projection commit. |
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
