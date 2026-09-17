# B0 — Owner decisions (2026-09-17, binding for B0)

1. **Test placement:** architecture / DI / host tests live in `tests/AeroTech.Ordering.Persistence.Tests`. No new
   ProjectReference is added to `Domain.Tests`; it stays pure-domain.
2. **Authentication in B0 tests:** no test token issuer, fake JWT issuer, mock authentication scheme or bypass. B0 host
   tests cover composition / DI / configuration without authenticated HTTP. Authenticated endpoint E2E is outside B0
   until the real Identity/Aegis convention is defined; if B0 truly depends on it, record a BLOCKED_DECISION.
3. **Persistence.Tests on SQL:** approved for every run B0 needs, without per-run approval. Only `localhost\SQLEXPRESS`,
   isolated test databases, no staging/shared/production DB, no drop/reset outside test DBs, command and result of every
   run recorded in evidence. Valid for B0 only; the general CLAUDE.md rule is unchanged.
4. **`PingController` and `Syncer/v1/*`:** not changed in B0 (route, `[Authorize]`, visibility, controller shape). B0 only
   inventories/characterizes the current exposure.
5. **Real vs simulator:** no new configuration. Existing convention `Providers:UseDeterministicTestAdapters`:
   false/absent → real / fail-closed providers; true → deterministic adapters in `Providers.Deterministic` override the
   owner-port registrations. Never an automatic production fallback. No per-provider mode, Real/Simulator/Hybrid enum or
   config matrix. No new ProjectReference. `AddDeterministicProviders()` registers only adapters a slice really needs.

Scope: decimal/string global conventions, inbox correctness, outbox leasing/EventId correctness, architecture/DI/host
tests, deterministic-seam infrastructure. No Order behavior, business API, auth design or servicing behavior.
New semantic/architecture decisions found during B0 are recorded in this folder and stop only the dependent path.
