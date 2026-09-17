# B0 — Plan: certify and repair the technical shell

Authority: `docs/ORDERING-DESIGN-PACK-v3.8/` (`SLICES/B0.md`, `RUNBOOKS/B0.md`, `ARCHITECTURE/02`, `ARCHITECTURE/03`,
`SPEC/scenarios.json` SC-B0-001…010), `CLAUDE.md`, `reports/00-decisions/B0-OWNER-DECISIONS-2026-09-17.md`.
Source baseline: `957757471d9db3fde91922142d537a559b8084e4` (`k8s-stg`).

Out of scope: Order behavior, business API, auth design, servicing, schema/route renames, new projects, new
ProjectReferences, `PingController` / `Syncer/v1/*` changes, test token issuer.

## 1. Requirement → change → proof

| Scenario / invariant | Change (layer · file) | Proof (all in `tests/AeroTech.Ordering.Persistence.Tests`) |
|---|---|---|
| SC-B0-001 · INV-002 — Domain→Messages allowlist | none in `src` (already conforms) | `Architecture/DomainMessagesAllowlistTests` — reads every `AeroTech.Messages` TypeReference of `Domain.dll` (metadata, includes method bodies); `Architecture/LayerReferenceTests` — forbidden assembly references of Domain/Application; `ICallerContext` has no owner-airline member (C15) |
| SC-B0-002 · INV-059 — decimal round-trip | Persistence · remove `ConfigureConventions` from `OrderingDbContext`; Query · same in `OrderQueryDbContext`; Persistence · `_Shared/Mapping/DecimalPrecisionExtensions` (amount 28,8 · rate 28,12 · quantity 18,6) | `Precision/DecimalPrecisionTests` — 1.23456789 and 0.123456789123 written/read on SQL Server unchanged; `Precision/ModelPrecisionTests` — every decimal property of both models has explicit precision |
| SC-B0-003 · INV-003 — duplicate DI | ServiceHost · `Composition/OrderingHostComposition` (the existing `Program.cs` chain moved as-is) + `Composition/SingleRegistrationGuard` (one `IUnitOfWork`, one binding per `Domain.Ports` interface; failure names the duplicate) | `Composition/SingleRegistrationGuardTests`, `Composition/HostCompositionTests` (flag false and true, `ValidateOnBuild`) |
| SC-B0-004 · INV-004 — inbox without source identity | Consumers · `InboxConsumeFilter` rejects (throws → MassTransit error/quarantine path), no pass-through | `Inbox/InboxConsumeFilterTests` — in-memory MassTransit harness + real SQL: consumer not invoked, no marker, fault published |
| SC-B0-005 · INV-004 — business crash | Persistence · `InboxStore` marker only enlisted, committed by the work's `SaveChanges`; Consumers · filter | same class — crash before commit: no marker, no work; redelivery commits both once |
| SC-B0-006 · INV-004 — unrelated SQL failure | Persistence · `Inbox/InboxDuplicateClassifier` (SQL 2601/2627 on the inbox key only); Consumers · filter uses it | same class — unrelated unique violation is rethrown, not swallowed; concurrent duplicate marker is discarded |
| SC-B0-007 · INV-005 — broker response loss | Persistence · `OutboxMessage` + `EventId` (unique), `AttemptCount`, `LastError`, `LeaseOwner`, `LeaseExpiresOn`; `OutboxWriter` stores `EventId`. Consumers · `Outbox/OutboxDispatcher` (claim with lease, fenced completion, failure recorded on the row), `IOutboxTransport` + `MassTransitOutboxTransport`; `OutboxPublisher` becomes the polling shell | `Outbox/OutboxDispatcherTests` — publish accepted + acknowledgement lost → republish with same MessageId/EventId/payload, receiver inbox commits one fact; two dispatchers never hold the same row; expired lease cannot complete (fencing); unusable row is recorded, not skipped |
| SC-B0-008 · INV-056 — independent owner persistence | Providers.Deterministic · `_Shared/Persistence/DeterministicOwnerDbContext` + `DeterministicEffectStore` (own connection string, own database, key + request hash), registered only when `Providers:UseDeterministicTestAdapters=true` | `Deterministic/DeterministicEffectStoreTests` — Ordering transaction rolled back, owner effect still readable after a new context; same key + different hash conflicts |
| SC-B0-009 · INV-001/002 — target-only startup | none | clean copy of the tracked tree outside `E:\Projects\DotAir`, restore/build/test there → `05-independence/` |
| SC-B0-010 · INV-003 — host | none | unauthenticated part only: host start, `/health/live`, `/health/ready`, restart on an isolated database → `04-host/`; `Host/AnonymousSurfaceInventoryTests` characterizes `Ping` and `Syncer/v1/*` as they are. Authenticated diagnostics / invalid credentials: owner decision 2 → recorded, not built |

Migrations: first `OrderingDbContext` migration (`Order` schema unchanged, `dbo.InboxMessages`, `dbo.OutboxMessages`)
generated with `dotnet ef`. Schema and table names are not renamed.

## 2. Items recorded instead of decided

Written to `reports/00-decisions/B0-OPEN-DECISIONS.md`; only the dependent path stops:

1. Inbox dedup identity `(OwnerAirlineId, SourceSystem, EventId, ConsumerName)` + payload hash (ARCHITECTURE/03) —
   existing owner events carry no such identity; B0 keeps `(MessageId, Consumer)` and repairs correctness.
2. SC-B0-010 authenticated part.
3. SDK pin (`global.json`) and boot/test script names.

## 3. Test data and SQL

`localhost\SQLEXPRESS` only. Each test run creates databases `OrderingB0_<runid>` / `OrderingB0_<runid>_Owner` and drops
exactly those at the end. Every run: command + full output in `01-runs/`.

## 4. Evidence layout

`01-runs/` test and build output · `02-migrations/` migration script and constraints · `03-fault-matrix/` ·
`04-host/` · `05-independence/` · `REPORT.md` (results, blockers, runbook).
