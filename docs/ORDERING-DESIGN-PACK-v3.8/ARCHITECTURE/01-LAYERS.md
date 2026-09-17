# Layer responsibilities and standing repository conventions

This document fixes **responsibility boundaries**, not a new project topology. The existing AeroTech.Ordering project/layer structure and explicit user-ratified conventions are preserved unless a concrete business invariant cannot be satisfied without a change. A design pack must not reorganize the repository merely to match a textbook architecture.

## Standing conventions that remain binding

- Ordering-owned enums are defined under `Contracts/AeroTech.Messages/Ordering/Enums/` according to the existing user rule. Domain may consume `AeroTech.Messages.Ordering.Enums.*`. The current `ICallerContext` also has a narrow, source-proven platform-identity exception for exactly `AeroTech.Messages.Aegis.Enums.BusinessContextType`, `AeroTech.Messages.Aegis.Enums.PrincipalType`, and `AeroTech.Messages.Shared.Enums.AuthorizationSurface`. This is an explicit type allowlist, not permission to consume the `Aegis`/`Shared` namespaces generally. Any additional non-Ordering Messages type in Domain requires explicit user approval. Domain must not depend on integration-event DTOs, provider/owner wire DTOs, base transport messages or transport behavior.
- Semantic external-service ports follow the existing `Domain/Ports/{Area}/` convention. Their contracts use domain/application-neutral semantic request/result types and contain no HTTP/EF/provider implementation.
- Project/folder organization follows the AeroTech `{X}Aggregate/{Arguments,Contracts,DomainEvents,Entities,...}` convention. Logical modules in the design documents are semantic groupings, not permission to replace that convention.
- Commands use `{Name}Command` + `{Name}CommandHandler` unless an already-established repository convention for a specific feature says otherwise.
- Every `*Query` and its query execution types live in the Query project.
- Reference data is referenced by identity and may be joined on the read side using the existing ReferenceData/query pattern. Historical accepted facts required to understand an Order are still snapshotted at acceptance time and must never be reconstructed from today's reference data.
- Do not add a project without the user's/project's existing approval rule. The pack may require a category of test/verification, not a new project name.

## Exact responsibility boundaries

| Project | Responsibility | Forbidden responsibility |
|---|---|---|
| Framework.Core | Generic IDs/clock/results/domain base abstractions | airline/order business vocabulary beyond existing generic contracts, HTTP/EF/broker behavior |
| Framework.Infrastructure | Generic technical infrastructure | Order-specific business policy |
| Framework.Presentation | Generic API/auth/error plumbing | Order orchestration or pricing rules |
| Contracts/AeroTech.Messages | Shared/versioned Ordering enums and integration/wire contracts according to platform convention | aggregates, repositories, business calculation |
| Ordering.Domain | Aggregates, entities, value semantics, pure policies, repository contracts and semantic external-service ports | EF/DbContext, HTTP clients, provider wire DTOs, MassTransit behavior, configuration lookup, service locator |
| Ordering.Application | Canonical commands/handlers and multi-step use-case choreography; obtains external effects through Domain ports | provider HTTP DTOs, EF mapping, controller concerns, duplicate business rule implementations |
| Ordering.Persistence | EF/SQL mappings, repositories and persistence mechanics | business eligibility, owner policy, remote service calls |
| Ordering.Synchronizer | The single Order read-model projection implementation using the established repository pattern | second independent business workflow, remote enrichment, competing projection writer |
| Ordering.Query | `*Query` types/handlers and local read model access; may use approved ReferenceData read-side enrichment | business mutations, provider side effects, command repositories as mutation path |
| Ordering.Providers | Real ACL/adapters implementing Domain ports; wire mapping, timeout/transport normalization | mutating Ordering aggregates/DB directly, inventing owner semantics |
| Ordering.Providers.Deterministic | Approved project/seam for test/reference implementations of the same Domain ports; current checked branch contains shell/plumbing only | production fallback, direct Ordering DB mutation, assuming adapters already exist |
| Ordering.Consumers | Broker/job entry points and technical dispatch/retry according to existing framework pattern | independent commercial policy or alternate mutation rail |
| Ordering.RestApi | Surface-specific HTTP mapping/auth and dispatch to canonical command/query | pricing/domain decisions, SQL transaction ownership, provider calls |
| Ordering.ReferenceData | Local reference snapshots/read models according to existing project convention | rewriting historical accepted Order truth |
| Ordering.ServiceHost | Composition root/options/middleware/registrations | Order business decisions |

## Unit of work and projection

The pack requires these semantics and **does not choose a new owning project merely for style**:

1. exactly one effective unit-of-work/transaction mechanism for an Ordering local commit;
2. exactly one authoritative Order projector/writer;
3. when a slice requires Order + receipt + projection + Outbox to be atomic, the implementation must provide one local SQL transaction boundary that guarantees it;
4. no network/provider call is inside that SQL transaction;
5. no duplicate DI registration may cause a different UnitOfWork/projector to win by registration order.

The concrete implementation location should follow the existing AeroTech pattern unless source evidence shows it cannot meet those guarantees. If changing that established pattern is necessary, record it as a user-visible architecture decision rather than silently moving ownership between Application/Synchronizer/Persistence.

## ReferenceData and historical truth

The existing read-side ReferenceData join convention may remain. The distinction is semantic:

- **accepted historical fact** needed to explain what was sold -> store/snapshot with the Order/read projection at acceptance;
- **current display/reference value** -> may come from current ReferenceData read models;
- current reference-data changes must never rewrite or reinterpret an accepted historical commercial fact.

## Architecture verification

Use the existing test-project convention to verify the boundaries above. The pack does not mandate a project named `Architecture.Tests`. At minimum verify:

- no Domain use of EF/HTTP/provider implementation types;
- no provider adapter directly writes Ordering persistence;
- one mutation rail per business effect;
- one effective UnitOfWork and one Order projector;
- controllers/consumers dispatch to canonical use cases rather than implement business logic;
- `*Query` placement follows the standing Query-project rule;
- Domain use of `AeroTech.Messages` is type-allowlisted: `AeroTech.Messages.Ordering.Enums.*` plus exactly `BusinessContextType`, `PrincipalType`, and `AuthorizationSurface` for the existing caller-context contract; all integration events, provider/owner messages, base transport contracts and any other Messages types are forbidden unless the user explicitly approves an architecture change.
