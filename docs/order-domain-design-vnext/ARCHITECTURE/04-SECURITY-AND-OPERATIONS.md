# Security, observability and operations from B0

## Trusted context and authorization

One owning airline per deployment/database. OwnerAirlineId comes from validated deployment configuration; TenantId when transported comes from authenticated platform context, never arbitrary request input. FinancialCustomerId is the responsible customer, not automatically the logged-in user or traveler. Authorization checks both permission and object scope. Agency users cannot read another financial customer's preparations, orders, operations or idempotency results.

Service identities call `/service/` with capability-specific scopes. Office users call `/backoffice/`; agency browser sessions `/otapanel/`; OTA credentials `/ota/`; only system administrators `/internal/`. Enforce these distinctions in authorization tests; path naming alone grants nothing. Do not expose admin reconciliation, simulated evidence injection or issuer stock administration to agencies.

No actor=0 fallback. Historical actor/seller/office snapshots are explicit, including a service actor for scheduled work and the initiating actor on the operation. Access to raw provider evidence and PII is separately authorized and audited. Log correlation IDs, not payment tokens, traveler passports or full offer payloads.

## Configuration

Validate options on startup. Each capability declares `Mode: Disabled|Real|Simulator`, `ProviderProfileId`, `ContractVersion`, `BaseAddress` where applicable, finite request deadline, bounded read retry/backoff, recovery polling bounds and credential reference. Unknown mode, duplicate binding or a required enabled capability with no implementation fails startup. Disabled capabilities return a structured unsupported response, not an unhandled DI exception.

Production environment MUST reject simulator mode, fake issuer stock and sandbox acceptance profiles. A profile change applies to new operations only; outstanding operations retain their profile/contract. Do not rename a profile in place to repoint existing resource references. No real request is retried through a simulator on outage.

Clock and ID generation are injected using the generic framework. Validate unique node configuration for existing long-ID generation. Persist DateTimeOffset/UTC instants; never use server local time for flight identity or owner deadlines. Owner-supplied technical clock skew tolerance is recorded separately from business expiry.

## Observability

Every operation/span: OwnerAirlineId, OrderId, OperationId, ExternalOperationId, stable provider-key hash, ClaimId, Step, Attempt, ContractVersion, ProviderProfileId, outcome, evidence revision, elapsed time and error code. Never use unbounded passenger names/order IDs as metric labels.

Metrics include outstanding Unknown count/age, reconciliation queue depth, worker lease conflicts, outbox lag, duplicate payload conflicts, projection drift, stock remaining, gate denial reason and contract mismatch count. Alerts do not auto-resolve business uncertainty.

Liveness checks process viability. Readiness checks mandatory local dependencies/schema and valid configuration; a temporary optional owner outage should not make local GetOrder unavailable. Capability availability is exposed separately to authorized operators. A missing required DB remains not-ready.

## Recovery administration

Provide read-only operation timeline and evidence views in backoffice when permitted. `/internal/v1/operations/{id}/recovery-attempts` schedules another SAME-operation read-back, not an arbitrary resend. `/internal/v1/reconciliation/{id}/resolutions` records typed authoritative evidence with source, reference, timestamp, scope, actor and rationale. It cannot set `Succeeded=true` without domain validation. Irreversible override needs explicit approved authority/two-person procedure in production; that authorization policy is an operational gate.

Retention never deletes unresolved evidence, claims, idempotency results needed for a possible retry or document/price lineage. Technical logs and protected PII payloads have separate policies. A minimal key/hash/tombstone can survive PII erasure without storing the erased payload. Use backups/restore tests and documented reconciliation after restore before re-enabling mutation dispatch.

## Release quality

S16 covers dependency/security checks, secret scan, non-root/container configuration where supported, backup/restore, migration rehearsal, concurrency/load limits, recovery drain/resume, provider profile changes, broker outage and authorization matrix. No numeric production SLO is invented in this pack: deployment-specific latency/throughput/RPO/RTO targets are BD-013. Benchmark load tests report actual conditions/results rather than declaring production capacity from a laptop run.
