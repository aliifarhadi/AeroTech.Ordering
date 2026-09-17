# CLAUDE.md — AeroTech.Ordering

AeroTech.Ordering is a fresh airline PSS **Order Service** built on the technical shell in this repository. It is not a
refactor or migration of the legacy `E:\Projects\DotAir\Ordering` service.

**Stack:** .NET 10 (`net10.0`), EF Core 10 (SQL Server), MediatR 12, FluentValidation, MassTransit 8 over RabbitMQ,
IdGen (snowflake IDs), RedLock.net. **DDD + CQRS + Clean Architecture.** `Contracts/AeroTech.Messages` targets `net9.0`
on purpose (shared wire contracts).

## Authority

1. **Design Pack 3.8** — `docs/ORDERING-DESIGN-PACK-v3.8/` — is the authority for Ordering domain semantics, service
   ownership, invariants, layer responsibilities and transaction/recovery semantics. Start at `START-HERE.md`; binding
   conflict resolutions are in `GOVERNANCE/05-STANDING-RULE-CONFLICT-RESOLUTION.md`.
2. **This file** is the authority for repository mechanics: conventions, naming, environment, tests, reporting.
3. Owner contracts of sibling services as mapped in the Pack's `CONTRACTS/`.

The Pack is not authority for developer environment or team process. If an Ordering semantic invariant truly conflicts
with a convention here, report the conflict; never silently replace the convention.

## Decisions and BLOCKED_DECISION

- The agent makes **no** new semantic, architectural, public-contract, persistence-identity/cardinality or
  repository-convention decision without the owner's explicit approval. Pure code-local mechanics inside an already
  approved rule are not a new decision.
- When a required choice is genuinely missing, ambiguous or contradictory, record a `BLOCKED_DECISION` with evidence
  and ask the owner. Affected work waits for explicit approval; independent unaffected work may continue. Never choose
  among valid alternatives and never fill a gap with a "safe default".
- Do not ask questions already answered by the Pack, this file or the target source.
- Execution cadence belongs to the owner: finish the currently authorized stage, report the checkpoint, and start
  another stage only when the owner's current instruction authorizes it (C12).
- Never download images/tools, delete files, rename schemas or change conventions without approval.

**Legacy `E:\Projects\DotAir\Ordering` is not a design source.** It may be read only for Framework, solution/csproj
topology, DI/host style, or a specific technical type needed to compile (record file and reason). Never copy legacy
business code, mappings, migrations, workflows or tests.

## Git

Work on branch `k8s-stg` of `https://github.com/aliifarhadi/AeroTech.Ordering` from its current HEAD. Reviewed bootstrap
is `2f2b9033f675387f68ef7ff95c9d0a575d836fb5`. Never reset or discard existing work, never create a second
solution/repository. Commit or push only when the owner asks.

## Sibling services (read-only; never modify their code)

| Service | Role for Ordering | Path |
|---|---|---|
| AirAvail (AirOffer) | Offers | `E:\Projects\DotAir\AirAvail` |
| FlightFlow | Inventory / capacity | `E:\Projects\DotAir\FlightFlow` |
| AirPrice | Pricing | `E:\Projects\DotAir\AirPrice` |
| IdentityServer | Identity, token claims, caller context | `E:\Projects\DotAir\IdentityServer` (not `...\Identity`) |
| StoredValue | Pattern reference only | `E:\Projects\DotAir\StoredValue` |

Pattern references: AirPrice for the CQRS read side; FlightFlow and StoredValue for domain/command side/consumers.
Copy a pattern in natively; never add a project/path reference to another repo; never copy a sibling pattern that breaks
a rule in this file or the Pack. For an integration owner, inspect only the contract files needed for the mapping.

## Solution layout

```
Framework/         Core ← Infrastructure ← Presentation   (shared mechanics; not Ordering-specific)
Contracts/         AeroTech.Messages                       (wire contracts + enums; net9.0)
src/
  Domain                     aggregates, entities, value semantics, pure policies, repository contracts, Ports/
  Application                canonical commands + handlers, use-case choreography (consumes Domain ports)
  Persistence                EF/SQL mappings, repositories, persistence mechanics, outbox/inbox
  Query                      every *Query + handler, read models, OrderQueryDbContext
  Synchronizer               the single authoritative Order read-model projector
  Consumers                  broker/job entry points, OutboxPublisher, Jobs/
  Providers                  real external ACL adapters implementing Domain ports
  Providers.Deterministic    approved project/seam for reference/test adapters of the same ports
  ReferenceData              local reference snapshots/read models, own schema/context
  RestApi                    thin surface-specific controllers
  ServiceHost                composition root
tests/
  AeroTech.Ordering.Domain.Tests, AeroTech.Ordering.Persistence.Tests
```

Do not add a project or layer, and do not rewire project references, without approval. Change a project reference only
when it violates a rule here or source/test evidence proves it breaks a Pack responsibility boundary; record the reason.
Required test categories go into the approved existing test projects.

## Layer responsibilities (Pack `ARCHITECTURE/01-LAYERS.md`)

- **Domain** — business invariants. No EF/DbContext, HTTP, provider wire DTOs, integration events, base transport
  contracts, MassTransit behavior, configuration lookup or service locator. Behaviors, not public setters; domain
  behavior never pulls ambient runtime context — Application/host passes trusted values/snapshots in. External seam:
  `Domain/Ports/{Area}/` (port interface + its semantic request/result types).
  **Messages allowlist (C1):** Domain may use only `AeroTech.Messages.Ordering.Enums.*` plus exactly
  `AeroTech.Messages.Aegis.Enums.BusinessContextType`, `AeroTech.Messages.Aegis.Enums.PrincipalType` and
  `AeroTech.Messages.Shared.Enums.AuthorizationSurface` (caller-context identity). This is a type allowlist, not a
  namespace exemption; any other Messages type needs explicit approval. The project reference itself is allowed.
- **Application** — every operation is `{Name}Command` + `{Name}CommandHandler` (+ validator/result) under
  `{X}Aggregate/Commands/{Name}/`; explicit transaction boundaries; no provider HTTP DTOs, EF mapping or controller
  concerns; no duplicate business rules.
- **Persistence** — mappings, repositories (one per aggregate root), stores. No business eligibility, no remote calls.
- **Synchronizer** — exactly one authoritative Order projector following the established pattern; no second workflow,
  no remote enrichment.
- **Query** — every `*Query` and its execution types live here; local read access only; may join ReferenceData read
  models read-only (`ExcludeFromMigrations`); no mutations, no provider side effects.
- **Providers** — real ACLs only; never mutate Ordering aggregates/DB, never invent owner semantics.
- **Providers.Deterministic** — approved shell/seam. It currently contains activation/composition plumbing only; each
  slice implements the reference adapters it needs there. Never a production fallback, never writes Ordering DB, never
  claimed as pre-existing behavior.
- **Consumers / RestApi** — thin entry points dispatching one canonical command/query; controllers depend on
  `IMediator` (plus identity/caller context) only.
- **ServiceHost** — composition/options/middleware only.
- **OwnerAirlineId (C15)** comes through `IHomeOperatorProvider` with the `ReferenceDataHomeOperatorProvider` binding.
  `ICallerContext` is caller/actor scope only and never the source of `OwnerAirlineId`.

Pack guarantees to keep: one effective unit of work, one projector, required local atomicity, no network call inside a
SQL transaction, no duplicate DI registration deciding behavior by registration order.

## Placement and naming

- Put every type in its most relevant folder — a folder name is a promise about its contents; folder = namespace. Ask
  "what *is* this type?", not "who uses it?".
- Per-aggregate folders: Domain `{X}Aggregate/{Arguments,Contracts,DomainEvents,Entities,ValueObjects,...}` + `_Shared/`;
  Application `{X}Aggregate/{Commands,EventHandlers,Services}` + `_Shared/{Behaviors,Events}`; Persistence
  `{X}Aggregate/`; Query `{X}Aggregate/{Queries/{Name},Dto,Models,Configurations}` + `_Shared/DbContexts`; RestApi
  `V1/{X}Aggregate/{Controllers,Requests,Responses}`; Providers `{Service}/{Options,Requests,Responses,Services,Wire}`.
  Pack modules (Commercial, Fulfillment, …) are semantic groupings, not a folder rewrite.
- A DTO/payload gets a concept-named folder (e.g. `Pricing/`), never a generic `Dtos/`/`Helpers/` bucket or a sibling
  concept's folder.
- **All Ordering-owned enums → `Contracts/AeroTech.Messages/Ordering/Enums/`** with `[Display(Name = "...")]` per
  member — never next to the class that uses them.
- Shared folder name is `_Shared`. Idiom: `*Command`/`*CommandHandler`, `*Query`/`*QueryHandler`, `*Configuration`,
  explicit domain method names; avoid `Manager`/`Helper`/`Processor`. SPEC command names are semantic IDs, not C# names.

## API

Surfaces, routes, bodies and semantics come from the Pack's `API-CONTRACTS.md` (and `SPEC/openapi-s1.json` for S1):
`/service/v1`, `/backoffice/v1`, `/otapanel/v1`, `/ota/v1` wrap the same canonical command/query; `/internal/v1` is
privileged administration only, never a service-to-service path. The existing Framework response/error envelope stays
(C13); the Pack defines semantic error distinctions only. No replay header/flag is invented.

## Reference data — reference, never re-implement

Currency, City, Country, Airport, AirportTerminal, Airline, Customer and operator settings are owned elsewhere and cached
by `AeroTech.Ordering.ReferenceData` (`ReadModels/*ReadModel`, `Syncing/*Syncer`). Never duplicate them as Ordering
entities, enums, tables or seed data. Domain keeps only identities/codes and the accepted historical snapshot a sale
needs; current reference data is display/enrichment only and never rewrites accepted history. A missing read model is
added to ReferenceData following its pattern, after asking.

## Shared contracts — `Contracts/AeroTech.Messages`

Shared by all AeroTech services. **Only `Contracts/AeroTech.Messages/Ordering/**` may be changed.** Never edit other
folders, the root base types (`BaseIntegrationEvent`, `BaseCommand`, `BaseAcknowledgeCommand`) or the `.csproj`.

- **Enums:** `Ordering/Enums/{Name}.cs`, one per file, explicit values from `1` (`0` only for a genuine Unknown),
  `[Display(Name = "...")]` on every member.
- **Integration events:** `Ordering/IntegrationEvents/V1/{Subject}{PastTenseVerb}.cs`, positional
  `public record ...( ... ) : BaseIntegrationEvent` with immutable primitives/enums/`IReadOnlyList<>` and **no custom
  envelope fields** — the shared envelope is stamped by `OutboxWriter`. One-event nested payloads in the same file
  (`{EventName}{Part}`); shared payloads in a concept folder under `Ordering/`. A breaking change is a new version
  folder, never an edit of a published version.

## Messaging and durability

Domain event → `DomainEventNotification<T>` (MediatR) → publisher handler writes the integration event to the
transactional outbox → `OutboxPublisher` → RabbitMQ → consumers dedup through the inbox. Persist immutable intent and
commit locally **before** any irreversible provider call; a provider exception or lost response is `Unknown` and is
recovered through the same operation identity — never blindly re-dispatched. Raw SQL on the caller's `OrderingDbContext`
enlists in its transaction; never register `AddDbContextFactory<OrderingDbContext>` as a singleton. Detailed semantics:
Pack `DOMAIN/08` and `ARCHITECTURE/03`.

## Error handling

Throw via `ExceptionFactory` (`Domain/_Shared/Resources/`), messages in `ExceptionMessages` — never inline
`new BusinessException(...)`. Codes live in 20000–29999; each new code is the next free number; every factory method
sets a REST-accurate `HttpStatus`.

## Code style

- **No code comments** unless explicitly requested; strip comments when porting.
- **No hardcoded parametric values** — bound options classes; defaults may live inside the options class, never inline
  `?? literal` in DI code; environment-specific or secret values have no default and fail fast.
- Do not guess ambiguous field mappings; do not invent, rename or reshape fields beyond the Pack — ask.

## Environment

- **SQL Server:** `localhost\SQLEXPRESS` only. Tests may create new databases on this server (and nowhere else); never
  touch databases of other services or the legacy Ordering databases.
- **AirOffer:** base address from the `Offer` configuration key; candidate details endpoint
  `http://localhost:5095/Service/v1/FlightOffers/Details`.
- RabbitMQ + Redis on `localhost` (dev `guest/guest`).
- A running `ServiceHost` locks `bin/`; build elsewhere with `dotnet build AeroTech.Ordering.sln -o "$TEMP/ordbuild"`.
  `dotnet ef` cannot redirect output — stop the app for migrations.
- "Update database" = apply pending EF migrations to the local dev DB without asking; drop/reset needs approval.

## Tests

- `dotnet build`, `dotnet test tests/AeroTech.Ordering.Domain.Tests` and
  `dotnet ef migrations has-pending-model-changes` are always fine to run.
- Do not run `tests/AeroTech.Ordering.Persistence.Tests` without explicit per-run approval until the suite is proven to
  finish in under 3–4 minutes; report it as "not run / pending approval" with the exact command.
- Prove crash/durability claims with forced-failure seams that fail against the unfixed code. Write full `dotnet test`
  output to a file and inspect it — never pipe through `tail`. A filtered run with zero tests is a failure; never delete
  or disable an assertion to go green.

## Reports

All plans, evidence, decisions and reviews go under `reports/`, foldered and ordered — never at the repo root:

```
reports/
  00-decisions/                 BLOCKED_DECISION records and owner answers
  01-D0-conformance/            one folder per stage, numbered in execution order
  02-B0-technical-shell/
  03-S1-.../
```

Inside a stage folder: `PLAN.md`, `REPORT.md`, then numbered subfolders for raw evidence as needed (e.g. `01-runs/`,
`02-architecture/`). File names `UPPER-HYPHENATED.md`. Stage evidence content follows Pack `GOVERNANCE/03`; items not
applicable to a stage are stated as not applicable with a reason. Never invent results, commits or IDs.

## Cross-service handoffs

Shared ledger `E:\Projects\DotAir\handoffs\` (protocol in its README). Read/write handoff files there freely; never write
code in another service's repo.
