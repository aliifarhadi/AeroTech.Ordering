# B0 — Certify and repair the technical shell

**Stage:** B0 · **Status:** LOCAL_DONE, with one scenario part recorded as open (SC-B0-010 authenticated part) ·
**Baseline:** `957757471d9db3fde91922142d537a559b8084e4` (`k8s-stg`) + uncommitted worktree · **Pack:** 3.8 ·
**Owner decisions:** `reports/00-decisions/B0-OWNER-DECISIONS-2026-09-17.md`

No Order behavior, business API, auth design, route/schema rename, new project or new ProjectReference was added.
`PingController` and `Syncer/v1/*` are untouched.

## 1. Scenario results

| Scenario | Result | Proof |
|---|---|---|
| SC-B0-001 Domain→Messages allowlist (INV-002) | pass | `Architecture/DomainMessagesAllowlistTests` (metadata TypeReferences of `Domain.dll`, incl. method bodies), `Architecture/LayerReferenceTests`, C15 member check |
| SC-B0-002 decimal round-trip (INV-059) | pass | `Precision/DecimalPrecisionTests` — 1.23456789 / 0.123456789123 / 12.345678 unchanged on SQL Server; `Precision/ModelPrecisionTests` |
| SC-B0-003 duplicate DI (INV-003) | pass | `Composition/SingleRegistrationGuardTests`, `Composition/HostCompositionTests` (real host chain, `ValidateOnBuild`, `ValidateScopes`) |
| SC-B0-004 inbox without source identity (INV-004) | pass | fault matrix F1 |
| SC-B0-005 inbox business crash (INV-004) | pass | F2, F3 |
| SC-B0-006 unrelated SQL failure (INV-004) | pass | F4, F5, F6 |
| SC-B0-007 outbox broker response loss (INV-005) | pass | F7–F12 |
| SC-B0-008 independent owner persistence (INV-056) | pass | F13, F14 |
| SC-B0-009 target-only startup | pass | `05-independence/` — clean copy outside `E:\Projects\DotAir`: no outside reference, build 0 errors, 37/37 tests |
| SC-B0-010 host | **partial** | `04-host/01-HOST-RUN.txt` — start, `/health/live`, `/health/ready` (redis, sql command, sql query, masstransit all Healthy), restart, same result. Authenticated diagnostics / invalid-credentials: **OD-B0-02**, not built (owner decision 2) |

Test runs (all `localhost\SQLEXPRESS`, databases `OrderingB0_<runid>[_Owner]` created and dropped by the run; after the
last run 0 `OrderingB0_*` databases remain):

| Run | File | Result |
|---|---|---|
| 1 | `01-runs/01-TEST-RUN-1.txt` | 33 tests, 31 pass, 2 fail — both were test-design errors (see §4) |
| 2 | `01-runs/02-TEST-RUN-2.txt` | 37 / 37 pass |
| mutation | `03-fault-matrix/MUTATION-RUNS.txt` | 4 reverted repairs → 5 tests red, source restored |
| final | `01-runs/03-TEST-RUN-FINAL.txt`, `01-runs/trx/B0-FINAL.trx` | 37 / 37 pass |
| clean copy | `05-independence/02-CLEAN-COPY-TESTS.txt` | 37 / 37 pass |

`Domain.Tests` contains no test: no domain behavior exists before S1 and owner decision 1 keeps B0 tests out of it.
`dotnet test` prints "No test is available" for that assembly.

Build: 0 errors; no warning in any Ordering project (remaining warnings are in other owners' `Contracts` folders and
Framework, unchanged from D0).

## 2. What changed

**Persistence**
- `OrderingDbContext`, `OrderQueryDbContext`: blanket `decimal(18,2)` / `string(256)` conventions removed.
- `_Shared/Mapping/DecimalPrecisionExtensions`: `HasAmountPrecision` (28,8), `HasRatePrecision` (28,12),
  `HasQuantityPrecision` (18,6). A model test fails any future decimal without explicit precision.
- Inbox: key named `PK_InboxMessages`; `Inbox/InboxDuplicateClassifier` (SQL 2601/2627 on that key only).
- Outbox: `OutboxMessage` + `EventId` (unique), `AttemptCount`, `LastError`, `LeaseOwner`, `LeaseExpiresOn`,
  `LeaseVersion`; `OutboxWriter` stores the domain `EventId`; `Outbox/OutboxLeaseStore` (claim with
  `UPDLOCK, READPAST`, fenced complete / failure).
- First migration `Migrations/20260917144148_B0InfrastructureShell` (`dbo.InboxMessages`, `dbo.OutboxMessages`; schema
  and table names unchanged). Script: `02-migrations/01-ORDERING-COMMAND-MIGRATIONS.sql`.

**Consumers**
- `InboxConsumeFilter`: no `MessageId` → `InboxMessageIdentityMissingException` (MassTransit error path); only the inbox
  key violation is treated as a duplicate; everything else is rethrown.
- `Outbox/OutboxDispatcher` + `IOutboxTransport` / `MassTransitOutboxTransport`; `OutboxPublisher` is only the polling
  shell with a per-process lease owner. Publication stays outside any SQL transaction. `MessageId` derivation from
  `EventId` is unchanged. `OutboxPublisherOptions.LeaseSeconds` (default 60, section `Outbox`).

**ServiceHost**
- `Composition/OrderingHostComposition.AddOrderingHost` = the former `Program.cs` chain, unchanged in order, so tests
  compose the real host. `Composition/SingleRegistrationGuard`: one registration for `IUnitOfWork`, `ICallerContext`,
  `IHomeOperatorProvider` and every `Domain.Ports.*` interface; the failure names the service and the implementations.
  A deterministic adapter therefore has to *replace* the real binding, never sit beside it.

**Providers.Deterministic** (existing seam, existing flag `Providers:UseDeterministicTestAdapters`)
- `_Shared/Persistence`: `DeterministicOwnerDbContext` (schema `DeterministicOwner`, own database),
  `DeterministicEffectStore` (owner + effect key + request hash, replay / conflict), registered only when the flag is
  true. No adapter registered — slices add only what they need.

**Tests**: `OrderingDatabaseFixture` now creates a per-run database and drops only `OrderingB0_*` names.

## 3. Points the owner should look at

1. **Local dev host:** `appsettings.Development.json` has `Providers:UseDeterministicTestAdapters = true`. The host now
   fails closed at startup until `ConnectionStrings:DeterministicOwnerDbContext` exists (or the flag is false). I did
   not touch that file. The key name follows the existing `ConnectionStrings:{Context}` convention; it is the only new
   configuration key besides `Outbox:LeaseSeconds`.
2. `Providers.Deterministic.csproj` gained `Microsoft.EntityFrameworkCore.SqlServer 10.0.9` (same package/version as
   Framework.Infrastructure and ReferenceData); `AddDeterministicProviders` now takes `IConfiguration`.
3. `EventId` unique (DOMAIN/13) means one integration event per domain event; writing two messages from one domain
   event fails at `SaveChanges` (F8).
4. Host characterization: `JwtSecrets` is absent in the dev configuration, so no JWT validation is configured and
   `GET /api/v1/Ping` answers 200 even with an invalid bearer. Inventory test
   `Host/AnonymousSurfaceInventoryTests` pins the current anonymous surface (Ping + six `Syncer/v1` posts); it will fail
   when that surface changes, which is the intent.

## 4. Divergences and corrections during the stage

- Run 1 failures: (a) MassTransit re-assigns a `MessageId` when a publisher sets it to null, so a harness cannot
  produce an identity-less message — the scenario is proven directly on the filter with a stub context; (b) MassTransit
  publishes `Fault<T>` at consumer level before an outer filter can swallow the exception, so "no fault" is not a valid
  signal for the concurrent-duplicate case — outcome is asserted on database state and on the filter directly.
- D0 finding 4 re-read: the marker was already committed by the consumer's own `SaveChanges` on the shared scoped
  context; B0 proves it (F2, F3) instead of rewriting `InboxStore`. `MarkProcessedAsync` remains on the Framework
  interface and is unused.
- D0 finding 6 ("per-capability binding config") is superseded by owner decision 5; the guard covers the ambiguity.

## 5. Open (recorded in `reports/00-decisions/B0-OPEN-DECISIONS.md`)

- **OD-B0-01** inbox dedup identity `(OwnerAirlineId, SourceSystem, EventId, ConsumerName)` + payload hash — blocks only
  the key reshape; needed before the first owner fact is consumed (not S1's create path, which consumes none).
- **OD-B0-02** SC-B0-010 authenticated part.
- **OD-B0-03** `global.json` / script names.

Not proven: per-stream ordered publication (no stream before S1); real RabbitMQ confirm loss (transport faked, SQL real).

## 6. Runbook

```
dotnet restore
dotnet build --no-restore
dotnet test --no-build --logger "console;verbosity=normal"          # needs localhost\SQLEXPRESS; creates/drops OrderingB0_* only
dotnet ef migrations script --idempotent --context OrderingDbContext --project src/AeroTech.Ordering.Persistence --startup-project src/AeroTech.Ordering.ServiceHost
dotnet ef database update --context OrderingDbContext  --project src/AeroTech.Ordering.Persistence   --startup-project src/AeroTech.Ordering.ServiceHost
dotnet ef database update --context ReferenceDbContext --project src/AeroTech.Ordering.ReferenceData --startup-project src/AeroTech.Ordering.ServiceHost
```
`dotnet ef` and the host read `ASPNETCORE_ENVIRONMENT=Development`; with the deterministic flag true they need
`ConnectionStrings__DeterministicOwnerDbContext`, otherwise set `Providers__UseDeterministicTestAdapters=false`.
Host probe used in `04-host`: `/health/live`, `/health/ready`, `/health` on RabbitMQ `localhost:5672` and Redis
`localhost:6379`.

## 7. Checkpoint

B0 stops here. S1 is not started.

## 8. After the checkpoint — owner answers applied (2026-09-17)

Evidence: `06-owner-answers/`, `03-fault-matrix/MUTATION-RUNS-INBOX-IDENTITY.txt`. Suite: **47 / 47** in ~23 s.

| Owner instruction | Result |
|---|---|
| OwnerAirlineId from Core OperatorSettings | ReferenceData already had the client (`AeroCore:BaseUrl` + `v1/OperatorSettings`), syncer, read model and `Syncer/v1/OperatorSettings`; live Core staging answers `HOME_OPERATOR` / `homeAirlineId = 1`. Proven by `ReferenceData/HomeOperatorFromCoreTests` (recorded Core payload → sync → provider = 1; not provisioned → `HomeOperatorNotProvisioned`). Sync is manual (`POST Syncer/v1/OperatorSettings`) — no scheduled job exists |
| `ReferenceDataHomeOperatorProvider` placement | moved ServiceHost → `Persistence/_Shared/OperatorContext/`, registered in `AddPersistence`; new ProjectReference Persistence → ReferenceData (approved). `CLAUDE.md`/Pack name the binding, not its project — unchanged |
| SourceSystem like StoredValue | already identical pattern (`IntegrationEventOptions`, default `"Ordering"`); payload assertion added |
| `JwtSecrets` removed, Identity settings like Core | `Framework.Presentation`: `Jwt:Authority` / `Jwt:Audience` / `Jwt:RequireHttpsMetadata` (`Options/JwtOptions`), issuer + audience + lifetime + signing key validated, RS256/ES256, fail-fast when missing. Dev config: authority `http://localhost:5050/`, audience `pss-api` (Identity `Protocol:Issuer`, `IdentityTokenClaims.Audience`). No Identity-issued token E2E was run |
| All integration events derive from `BaseIntegrationEvent` | OD-B0-01 closed: inbox key `(OwnerAirlineId, SourceSystem, EventId, Consumer)` + `PayloadHash`; messages without envelope identity or `MessageId` rejected. The uncommitted B0 migration was regenerated (never applied outside dropped test databases). Framework `IInboxStore` is no longer implemented or registered (its `Guid messageId` shape cannot carry the key); the interface file is untouched |
| Missing FlightFlow facts | handoff `OR-001` (hold expiry, flight change). No Ordering consumer until the owner delivers |

New inbox proofs (`Inbox/InboxConsumeFilterOutcomeTests`): republish with a new transport id is not reprocessed; same
`EventId` from another `SourceSystem` is a different fact; same identity + different payload → conflict, consumer not
run (also in the concurrent-marker path); blank/absent envelope rejected. Red proofs M5–M7.

`Contracts/AeroTech.Messages` changed on disk during this work outside my edits (≈90 files, incl. removal of
`FlightReservationExpiredEvent` / `FlightVersionChangedEvent`); the solution still builds with 0 errors.
