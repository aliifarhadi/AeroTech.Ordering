# D0 — Dependency graph and boundary facts

Source: `.csproj` files and source at `957757471d9db3fde91922142d537a559b8084e4` (no source change since bootstrap).

## Direct project references

| Project | References |
|---|---|
| AeroTech.Messages | — |
| Framework.Core | — |
| Framework.Infrastructure | Framework.Core |
| Framework.Presentation | Framework.Infrastructure |
| Ordering.Domain | Framework.Core, Messages |
| Ordering.Application | Domain, Framework.Core |
| Ordering.Persistence | Framework.Infrastructure, Domain |
| Ordering.Query | Framework.Infrastructure, Application, Providers, ReferenceData |
| Ordering.Synchronizer | Persistence, Providers, Query |
| Ordering.Consumers | Framework.Presentation, Persistence, Synchronizer, Messages |
| Ordering.Providers | Domain, Messages |
| Ordering.Providers.Deterministic | Domain, Messages |
| Ordering.ReferenceData | — |
| Ordering.RestApi | Framework.Presentation, Application, Synchronizer |
| Ordering.ServiceHost | Consumers, Persistence, Providers, Providers.Deterministic, Query, ReferenceData, RestApi |
| Domain.Tests | Domain |
| Persistence.Tests | Synchronizer, ServiceHost, Domain.Tests |

Pack 3.8 (`SPEC/layers.json`, C8) is a logical responsibility policy, not a project-reference contract: no reference is
changed unless source/test evidence proves a boundary breach. None is proven today (the projects contain no business
code yet).

## Domain → Messages type usage (C1 allowlist)

Only `Domain/_Shared/Contracts/ICallerContext.cs` uses Messages types: `BusinessContextType`, `PrincipalType`
(`AeroTech.Messages.Aegis.Enums`) and `AuthorizationSurface` (`AeroTech.Messages.Shared.Enums`) — exactly the three
allowed caller-context types. No `Ordering.Enums` usage yet, no integration event, wire DTO or base transport type.
**Conforms.** B0 turns this into an automated test.

## Owner identity (C15)

`IHomeOperatorProvider` (Domain) → `ReferenceDataHomeOperatorProvider` (ServiceHost), registered in `Program.cs`.
`ICallerContext` has no `OwnerAirlineId` member. **Conforms.**

## Unit of work / projector

- One `IUnitOfWork` registration: `Persistence/DependencyInjection.cs` → `OrderingDbContext`. `AddSynchronizer()`
  registers nothing. No projector exists yet. No duplicate registration.
- `CommandDbContext.SaveChangesAsync` dispatches domain events through MediatR before the database write (in-process,
  no broker call found in that path).

## Provider selection

`Providers.AddProviders` and `Providers.Deterministic.AddDeterministicProviders` register nothing.
`AddDeterministicProvidersWhenEnabled` is driven by one boolean `Providers:UseDeterministicTestAdapters`; with zero
adapters there is no real/simulator ambiguity today.
