# D0 — Repository and design conformance

**Stage:** D0 · **Status:** COMPLETED · **Gate:** D0_CONFORMANT · **Tested source:**
`957757471d9db3fde91922142d537a559b8084e4` (`k8s-stg`) · **Pack:** 3.8 (`SHA256SUMS` verified, all entries OK)

## 1. Repository

- `origin = https://github.com/aliifarhadi/AeroTech.Ordering.git`, branch `k8s-stg`, HEAD `9577574 Pack Redesign`.
- Drift from reviewed bootstrap `2f2b903`: only `CLAUDE.md` in history (added, later deleted in the worktree). No source,
  project, package or configuration file differs from the bootstrap.
- Worktree: `CLAUDE.md` recreated on the owner's instruction (2026-09-17); `docs/` (Pack 3.8) and `reports/` untracked.
  Nothing reset, deleted or recopied. Generated `bin/`/`obj/` folders were removed before the build.

## 2. Build

SDK 10.0.401 (no `global.json`). `dotnet restore` OK; `dotnet build --no-restore`: **0 errors, 12 warnings** — all in
other owners' folders of `Contracts/AeroTech.Messages` and generic Framework; none in Ordering projects.
Evidence: `01-runs/07..10`.

## 3. Project roles

| Project | Role (Pack `ARCHITECTURE/01`) | State |
|---|---|---|
| Framework.Core / Infrastructure / Presentation | generic mechanics | retained, certify in B0 |
| Contracts/AeroTech.Messages | shared wire contracts + Ordering enums | retained; legacy `Ordering/**` content stays until a slice supersedes it |
| Domain | invariants, ports | only `_Shared` (caller context, home operator, exception factory) |
| Application | use cases | MediatR behaviors + domain-event dispatcher only |
| Persistence | EF/SQL | `OrderingDbContext` with Outbox/Inbox only; no migrations |
| Query | read side | `OrderQueryDbContext` with ReferenceData joins only; no migrations |
| Synchronizer | single projector | empty |
| Consumers | entry points/workers | inbox filter, outbox publisher, retention poller |
| Providers / Providers.Deterministic | ACLs / reference adapters seam | empty shells |
| ReferenceData | reference cache | complete module with 2 migrations |
| RestApi | controllers | `PingController`, `IdempotencyKey` helper |
| ServiceHost | composition | `Program.cs`, caller context, home-operator provider |
| Domain.Tests / Persistence.Tests | tests | helpers only, **zero test methods** |

## 4. Discrepancies → B0 actions

Conforming already (no action): Domain→Messages allowlist (C1), owner identity source (C15), single `IUnitOfWork`
registration, no real/simulator ambiguity. Details: `02-architecture/DEPENDENCIES.md`.

| # | Finding (file) | B0 action | Proof |
|---|---|---|---|
| 1 | No automated boundary check exists | Add architecture tests in an existing test project: C1 type allowlist, Domain/Application forbidden types, single UoW/projector registration | SC-B0-001, SC-B0-003 |
| 2 | Global `decimal(18,2)` and `string(256)` conventions (`OrderingDbContext`, `OrderQueryDbContext`) | Remove the blanket monetary/string defaults; explicit precision per mapped field (amount 28,8 / rate 28,12 / quantity 18,6) | SC-B0-002, INV-059 |
| 3 | `InboxConsumeFilter` skips dedup when `MessageId` is missing and treats any `DbUpdateException` as a possible duplicate | Reject/quarantine missing identity; classify only the inbox unique-key violation as duplicate | SC-B0-004, SC-B0-006 |
| 4 | `InboxStore.MarkProcessedAsync/PersistProcessedAsync` call `SaveChanges` on their own | Marker and accepted work commit in one transaction | SC-B0-005, INV-004 |
| 5 | `OutboxPublisher`: no multi-instance lease, rows without usable `EventId` silently skipped | Stable EventId on republish proven; unusable rows surface as failures; publisher lease | SC-B0-007, INV-005 |
| 6 | Provider activation is one global boolean + registration order | Explicit per-capability binding validated at startup; production rejects simulator | SC-B0-003 |
| 7 | No durable simulator state exists | Minimal independent simulator persistence in `Providers.Deterministic` | SC-B0-008 |
| 8 | Host: JWT validation silently absent when `JwtSecrets:Key` is empty; `PingController` and `Syncer/v1/*` are anonymous | Verify authenticated-host behavior with the existing security convention; liveness/readiness truthful | SC-B0-010 |
| 9 | Target-only independence not yet proven by a run | Clean-checkout build/start with legacy repo inaccessible | SC-B0-009 |

## 5. BLOCKED_DECISIONs for B0 (need the owner before the affected work)

1. **Test placement:** architecture/DI tests → `Domain.Tests` or `Persistence.Tests`? (Persistence.Tests already
   references ServiceHost; Domain.Tests references only Domain.) Host/auth tests need a project that references
   ServiceHost.
2. **Auth in host tests (SC-B0-010):** no test-issuer convention exists in the repository.
3. **Persistence.Tests runs:** B0 requires real SQL tests; `CLAUDE.md` requires per-run approval.
4. **Finding 8:** whether `PingController` and the anonymous `Syncer/v1/*` routes are changed in B0, and how.
5. **Finding 6:** the configuration shape of per-capability provider binding.

Pack `BLOCKED-DECISIONS.md` BD-001…BD-013 are unchanged; none affects D0/B0.

## 6. Not applicable in D0

API/OpenAPI, migrations run, provider bindings, TRX, E2E transcript, fault matrix: D0 starts no host, runs no test and
touches no database.
