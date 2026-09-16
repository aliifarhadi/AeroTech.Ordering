# CLAUDE.md — AeroTech.Ordering (vNext)

AeroTech.Ordering vNext is the **airline PSS Order Service**, rebuilt from a clean skeleton. It is a new
repository, not a refactor of the legacy `E:\Projects\DotAir\Ordering` service.

**Stack:** .NET 10 (`net10.0`), EF Core 10 (SQL Server), MediatR 12, FluentValidation, MassTransit 8 over
RabbitMQ, IdGen (snowflake IDs), RedLock.net. **DDD + CQRS + Clean Architecture.** `Contracts/AeroTech.Messages`
targets `net9.0` on purpose (shared wire contracts).

## Design authority

1. The vNext design pack `docs/order-domain-design-vnext/` (read the slice set listed in its `MANIFEST.md` before coding).
2. Explicit owner contracts of sibling services (AirOffer, FlightFlow, ...).
3. AeroTech Framework / project conventions (this file) for mechanics.

If code or this file conflicts with the pack on business semantics, the pack wins. If neither settles a material
business or cross-service semantic, write a `BLOCKED_DECISION` and continue independent work — never guess.

**Legacy repo `E:\Projects\DotAir\Ordering` is not a design source.** It may be read only for Framework,
solution/csproj topology, DI/host style, or a specific technical type needed to compile (record the file and
reason). Never lift legacy business implementations; when any logic is ported, re-examine every check.

## Sibling references — replicate, never cross-reference

Sibling services (read-only; never modify their code):

| Service | Role for Ordering | Path |
|---|---|---|
| AirAvail (AirOffer) | Offers — accepted offer owner | `E:\Projects\DotAir\AirAvail` |
| FlightFlow | Inventory — flight reservation/hold owner | `E:\Projects\DotAir\FlightFlow` |
| AirPrice | Pricing | `E:\Projects\DotAir\AirPrice` |
| IdentityServer | Identity, token claims, caller context | `E:\Projects\DotAir\IdentityServer` (not `...\Identity`) |
| StoredValue | Pattern reference (domain, command side, messaging) | `E:\Projects\DotAir\StoredValue` |

Pattern references: **AirPrice** for the CQRS read side / building read models; **FlightFlow** and **StoredValue** for
domain / command side / consumers / providers. When a sibling is an integration owner (AirAvail, FlightFlow,
AirPrice, IdentityServer), inspect only the contract files needed for the mapping. Copy a pattern in natively; never
add a project/path reference to another repo. Before hand-rolling infrastructure (outbox, inbox, idempotency,
locking, serialization, EF plumbing, pagination), grep the siblings' `Framework/` first and replicate the proven
shape — but do not copy a sibling pattern that breaks the rules in this file.

## Solution layout & dependency direction

```
Framework/         Core ← Infrastructure ← Presentation   (shared mechanics; not Ordering-specific)
Contracts/         AeroTech.Messages                       (wire contracts + enums; leaf; net9.0)
src/
  Domain                     aggregates, entities, VOs, domain events, invariants, Ports/  (→ Framework.Core, Contracts)
  Application                commands + handlers, validators, app services, event handlers (→ Domain)
  Persistence                EF command side, repositories/stores, outbox writer, inbox    (→ Domain, Framework.Infrastructure)
  Query                      every *Query + handler, read models, OrderQueryDbContext      (→ Application, Providers, ReferenceData)
  Synchronizer               projects command state → read models                          (→ Persistence, Providers, Query)
  Consumers                  MassTransit consumers, OutboxPublisher, Jobs/ pollers         (→ Contracts, Persistence, Synchronizer)
  Providers                  real external ACLs only                                        (→ Contracts, Domain)
  Providers.Deterministic    deterministic adapters implementing the same ports            (→ Contracts, Domain)
  ReferenceData              isolated pull-sync module, own schema/context, zero domain coupling
  RestApi                    thin controllers                                              (→ Application, Synchronizer)
  ServiceHost                composition root
tests/
  AeroTech.Ordering.Domain.Tests, AeroTech.Ordering.Persistence.Tests
```

Do not add a new architecture/framework layer or project without asking. Never change an established
architectural pattern silently — if a task seems to require it, say so first.

## Layer responsibilities (enforced)

- **Domain** — pure model: aggregates reference each other **by identity only**; explicit domain methods, no
  setters for business state; value objects for money/refs; invariants inside the aggregate; important
  transitions raise domain events (mutators that raise events take `IIdGenerator` for the snowflake EventId).
  No infrastructure, no `IOptions`/DI — callers pass configured values as parameters. The external seam is
  `Domain/Ports/{Area}/` (port interface + its typed request/result records together).
- **Application** — use cases. Every operation is a `{Verb}{Noun}Command` in `{X}Aggregate/Commands/{Name}/`
  (command + handler + validator + result). Channel-specific commands live under `Commands/{Name}/{Channel}/`
  and map to the same use case. Cross-aggregate orchestration and transaction boundaries live here.
  Integration-event publishers are `{X}Aggregate/EventHandlers/Publish*IntegrationEvent`.
- **Query** — owns its data access: `{X}Query → handler → Query-owned reader → OrderQueryDbContext → Query-owned
  read model`. Every `*Query` lives in the Query project, never in Application. No query repository or read DTO in
  Domain; Query never injects command-side stores/repositories and never references Persistence. Command tables
  are mapped read-only with `ToTable(table, CommandSchema, b => b.ExcludeFromMigrations())`. A rule shared by
  Application and Query goes in Domain taking primitives or an aggregate-produced value, never a query DTO.
  Query handlers never call external services.
- **Persistence** — EF configurations, repositories (one per aggregate root), stores, outbox/inbox.
  `OrderingDbContext` is the `IUnitOfWork`.
- **Providers** — only real HTTP/client adapters: provider DTOs, config/auth/timeout, ACL mapping, technical +
  authoritative outcome mapping. It must NOT load aggregates, use repositories/DbContext/UoW, decide business
  eligibility, write Order/read-model data, coordinate multiple providers, compensate, or call application
  services. Fail-closed stubs for uncertified capability are allowed.
- **Providers.Deterministic** — same port semantics, deterministic/stateful/idempotent; activated by ServiceHost via
  `AddDeterministicProvidersWhenEnabled(configuration)` (`Providers:UseDeterministicTestAdapters`) **after**
  `AddProviders` so its registrations win. Production config must fail closed for uncertified capability.
- **RestApi** — controllers depend on `IMediator` (plus identity/caller context) only; never inject application
  services. Request DTOs one-per-file in `V1/{X}Aggregate/Requests/`, responses in `Responses/`.
- **Consumers** — grouped by source service → aggregate → `When{Event}/`; background pollers in `Jobs/`.

## Placement & naming (enforced)

- Put every type in its **most relevant, most related folder** — a folder name is a promise about its contents.
  Ask "what *is* this type?", not "who uses it?". Folder = namespace.
- Per-aggregate folders: Domain `{X}Aggregate/{Arguments,Contracts,DomainEvents,Entities,ValueObjects,...}` +
  `_Shared/`; Application `{X}Aggregate/{Commands,EventHandlers,Services}` + `_Shared/{Behaviors,Events}`;
  Persistence `{X}Aggregate/`; Query `{X}Aggregate/{Queries/{Name},Dto,Models,Configurations}` + `_Shared/DbContexts`;
  RestApi `V1/{X}Aggregate/{Controllers,Requests,Responses}`; Providers `{Service}/{Options,Requests,Responses,Services,Wire}`.
- A DTO/payload gets a **concept-named** folder (e.g. `Pricing/`), not a generic `Dtos/`/`Helpers/` bucket and never
  a sibling concept's folder (a payload carried by events is not an integration event).
- **All enums → `Contracts/AeroTech.Messages/Ordering/Enums/`** (ns `AeroTech.Messages.Ordering.Enums`) with
  `[Display(Name = "...")]` per member — never next to the class that uses them. See "Shared contracts" below.
- Shared folder name is `_Shared` (AirPrice), not `_Common`.
- Match idiom: `*Command`/`*CommandHandler`, `*Query`/`*QueryHandler`, `*Configuration`, explicit domain method
  names; avoid `Manager`/`Helper`/`Processor`.

## API surfaces

Thin, channel-specific controllers (one per channel) mapping to the same application use cases. Channel audiences:
`ota` (public travel-agency API), `otapanel` (agency panel), `backoffice` (airline admin/servicing). Endpoint names,
casing and verbs come from the pack's `05-API-CONTRACTS.md` (e.g. `POST /ota/v1/orders/create`). Do not use
`/internal/` as a service-to-service path.

## Reference data — reference, never re-implement (enforced)

Base definitions (Currency, City, Country, Airport, AirportTerminal, Airline, Customer, operator settings) are
owned by other services and pulled into `AeroTech.Ordering.ReferenceData` as read models
(`ReferenceData/ReadModels/*ReadModel`, schema `ReferenceData`, synced by `Syncing/*Syncer`). Never create
Ordering entities, value objects, enums, tables or seed data that duplicate them.

- **Domain** holds only the identity (`int CurrencyId`, `int CityId`, `long AirlineId`, ...) or the stable code a
  snapshot needs; it never references the `ReferenceData` project (which stays free of domain coupling).
- **Query** joins the read models read-only in `OrderQueryDbContext` (`MapReferenceReadModel<T>`, `ExcludeFromMigrations`).
- **Command side** needing a reference fact (existence, currency decimals/rounding, home operator) declares a small
  Domain contract and implements it in `ServiceHost` over `ReferenceDbContext` (precedent:
  `ServiceHost/OperatorContext/ReferenceDataHomeOperatorProvider`).
- If a needed definition has no read model yet (e.g. there is no `CountryReadModel` today, only `CityReadModel.CountryId`),
  extend `ReferenceData` with a read model + syncer following its existing pattern, after asking.

## Shared contracts — `Contracts/AeroTech.Messages` (enforced)

`AeroTech.Messages` is shared by all AeroTech services. **Only `Contracts/AeroTech.Messages/Ordering/**` may be
changed** (add, modify, rename or delete there when needed). Never touch any other folder (`Shared/`, `Aegis/`,
`FlightFlow/`, `JetPay/`, ...), the root base types (`BaseIntegrationEvent`, `BaseCommand`,
`BaseAcknowledgeCommand`) or the `.csproj`; they may be *used*, not edited. If something outside `Ordering/` needs
to change, raise it through the handoff ledger.

Every Ordering enum and integration event is written there, following the existing objects:

- **Enums:** `Ordering/Enums/{Name}.cs`, ns `AeroTech.Messages.Ordering.Enums`, one enum per file, explicit
  values starting at `1` (`0` only for a genuine `Unknown`/N/A member), `[Display(Name = "...")]` on every member.
- **Integration events:** `Ordering/IntegrationEvents/V1/{AggregateOrSubject}{PastTenseVerb}.cs`, ns
  `AeroTech.Messages.Ordering.IntegrationEvents.V1`, a positional `public record ...( ... ) : BaseIntegrationEvent`
  with immutable primitives/enums/`IReadOnlyList<>` (no envelope fields — `OutboxWriter` stamps them).
  Nested payloads used by one event are records in the same file named `{EventName}{Part}`
  (e.g. `OrderSplitPricingLine`); a payload shared by several events gets a concept folder under `Ordering/`
  (e.g. `Ordering/Pricing/`). A breaking shape change is a new version folder (`V2`), not an edit of `V1`.

## Messaging & contracts

Domain event → `DomainEventNotification<T>` (MediatR) → publisher handler writes an integration event to the
**transactional outbox** → `OutboxPublisher` → RabbitMQ → consumers dedup through the **inbox** on
`(MessageId, Consumer)`. `MessageId` derives from the snowflake `EventId` (never the outbox row id). Integration
events live in `Contracts/AeroTech.Messages/Ordering/IntegrationEvents/V1/`, extend `BaseIntegrationEvent`; the
envelope is stamped centrally in `OutboxWriter`. Renaming/re-versioning a published event is a wire-contract change —
coordinate via the handoff ledger. Never make a publisher aware of its subscribers.

## Durability rules for external side effects

Persist the immutable intent and commit the local transaction **before** any irreversible provider call. A provider
exception or lost response is `Unknown` (keep the claim, schedule recovery through the same operation identity);
never blindly re-dispatch. Anything that must survive the caller's rollback needs its own connection
(`TransactionScope(Suppress)`) — raw SQL on the caller's `OrderingDbContext` enlists in its transaction. Never
register `AddDbContextFactory<OrderingDbContext>` as a singleton.

## Error handling

Throw via **`ExceptionFactory`** (`Domain/_Shared/Resources/`), messages in `ExceptionMessages` — never
`new BusinessException(...)` inline. Codes live in **20000–29999**; each new code is the next free number after the
last one in the file. Every factory method sets a REST-accurate `HttpStatus` (400 bad request, 404 not found,
409 state conflict, 422 rule violation, 500 system, 502 upstream provider).

## Code style

- **No code comments** unless explicitly requested. Strip comments when porting code. Explanatory notes go in docs
  or memory, not source.
- **No hardcoded parametric values** — retries, timeouts, backoff, batch sizes, thresholds, periods, endpoints go in a
  bound options class. Defaults may live inside the options class; never inline `?? literal` in DI code.
  Environment-specific or secret values (hosts, credentials, instance identity) have no default and fail fast.
- Do not guess field mappings whose source is ambiguous, derived or differently named — propose and confirm first.
- Do not invent, rename or reshape an entity/aggregate field beyond the design pack without asking.

## Tests

- `dotnet build`, `dotnet test tests/AeroTech.Ordering.Domain.Tests` and
  `dotnet ef migrations has-pending-model-changes` are always fine to run.
- **Do not run `tests/AeroTech.Ordering.Persistence.Tests` without explicit per-run approval** until that suite is
  proven to finish in under 3–4 minutes; report it as "not run / pending approval" with the exact command.
  Prefer pure policy/domain coverage in Domain.Tests.
- Persistence tests use only the isolated local test DB. Prove crash/durability claims with forced-failure seams
  that fail against the unfixed code. Write full `dotnet test` output to a file and grep it — never pipe through `tail`.

## Build & dev workflow

- A running `ServiceHost` locks `bin/`; build elsewhere with `dotnet build AeroTech.Ordering.sln -o "$TEMP/ordbuild"`.
  `dotnet ef` cannot redirect output — stop the app for migrations.
- **Database:** `DotAirOrderingVNext` on `localhost\SQLEXPRESS` (tests: `DotAirOrderingVNextTests`); schemas `Order` /
  `ReadModel` / `ReferenceData`; history tables `dbo.__CommandsMigrationHistory`, `dbo.__QueriesMigrationHistory`,
  `dbo.__ReferenceDataMigrationHistory`. Never touch shared/staging/production data.
- "**Update database**" = apply pending EF migrations to the local dev DB without asking. Generating migrations
  and any drop/reset still need approval.
- **Infra:** RabbitMQ + Redis on `localhost` (dev `guest/guest`).
- Reports, catalogs and decision documents go under `reports/order-domain-vnext/<phase>/`
  (`UPPER-HYPHENATED-NAME.md`), never the repo root.

## Cross-service handoffs

Shared ledger `E:\Projects\DotAir\handoffs\` (protocol in its README). Read/write handoff files there freely; never
write code in another service's repo. As owner: scan open items whose Owner is Ordering. As requester: copy
`TEMPLATE.md` to `OR-NNN-short-slug.md` with an implementation-ready spec + verification checklist, add an index row,
Status: Requested. To verify: read the owner repo read-only against the checklist.
