# Layer contracts - actual target projects

This is a physical dependency and responsibility contract, not an illustrative clean-architecture diagram. Keep the existing project names. A logical business module is a namespace/folder inside these layers, not another deployable by default.

## Allowed direct project dependencies

| Project | Allowed project dependencies | Responsibility | Forbidden responsibility |
|---|---|---|---|
| Framework.Core | none | Generic IDs/clock/results/domain base abstractions; no airline vocabulary | HTTP, EF, broker DTOs, current caller resolution, business policy |
| Framework.Infrastructure | Framework.Core | Generic technical implementations | Order-specific schema, fare/payment decisions |
| Framework.Presentation | Framework.Core; generic infrastructure only where already required | Generic API/auth/error plumbing | Order use-case orchestration |
| Contracts/AeroTech.Messages | Framework.Core only if generic marker required | Versioned integration envelopes and wire DTOs | Aggregate classes, repository interfaces, price calculation |
| Ordering.Domain | Framework.Core | Aggregates, entities, immutable value types, pure policies, domain facts | Messages, EF, HTTP, JSON wire attributes, MediatR handlers, IOptions/IConfiguration, service locator |
| Ordering.Application | Domain, Framework.Core | Commands, normalized query/use-case contracts, ports, phase coordination, transaction/projection abstractions | EF/DbContext, provider HTTP DTOs, MassTransit, SQL, controller types |
| Ordering.Persistence | Application, Domain, Framework.Core, Framework.Infrastructure | EF mappings, SQL repositories, transaction/session, command/inbox/outbox/evidence stores | Business eligibility or remote calls |
| Ordering.Synchronizer | Application, Domain, Persistence | Sole OrderProjector; deterministic projection construction and enlisted projection writes | Broker-driven second write model; another unit of work; remote enrichment |
| Ordering.Query | Application, Framework.Core | Read-only query handlers and read-model SQL/EF read context; local eligibility presentation | Command repositories, writes, Providers, remote enrichments |
| Ordering.Providers | Application, Domain value types, Framework.Core/Infrastructure, Messages | Real anti-corruption adapters, HTTP/wire mapping, fail-closed unavailable bindings | Mutating Order or writing Ordering DB, inventing owner policy |
| Ordering.Providers.Deterministic | Application, Domain value types, Framework.Core | Persistent isolated simulators for the same semantic ports | Production registration; direct Order DB writes; shared unit-of-work with Ordering |
| Ordering.Consumers | Application, Messages, Framework.Core/Infrastructure | Broker ingress, dispatcher/recovery/outbox worker hosting, ack/retry plumbing | Commercial policy, direct aggregate edits, calling another service instead of the application port |
| Ordering.RestApi | Application, Framework.Core/Presentation | Surface-specific HTTP contracts, input shape validation, claims to actor context, response mapping | Pricing, SQL transactions, provider calls, duplicate workflows |
| Ordering.ReferenceData | Application, Framework.Core/Infrastructure, Messages when consuming reference updates | Local reference snapshots/cache and ACL for currency/operator/customer references | Historical sale rewrites, canonical Customer/Price master |
| Ordering.ServiceHost | all required composition modules | Composition root, hosting, options validation, middleware, registration | Any Order business decision |

`Ordering.Query` may use a dedicated read-only EF context or parameterized SQL in its own project. It does not need a reference to command Persistence. Projection DTOs and query request/response types live in `Application/ReadModels` so both Query and Synchronizer can use them without referencing each other.

`IOrderSession`, `ICommandReceiptStore`, `IExternalOperationStore`, `IOutboxStore`, `IInboxStore`, `IOrderProjectionWriter` and `IOrderProjectionSource` are application-owned abstractions. Their SQL implementations enlist in the same scoped OrderingDbContext/DbTransaction. Synchronizer implements only the projection interface and never registers a replacement IUnitOfWork. Consumers need these interfaces, not a reference to Ordering.Persistence.

## Exact placement examples

| Concern | Destination |
|---|---|
| PricingLine reversal ceiling / item-service membership invariant | Domain/Commercial/Pricing and Domain/Commercial/Orders |
| IssueGate using already normalized evidence and a supplied instant | Domain/Fulfillment/Eligibility |
| Load, claim, persist intent, dispatch, apply evidence, finalize | Application/Issuance/IssueOrderHandler + typed operation coordinator |
| FlightFlow request DTO and 204 ambiguity | Providers/FlightFlow/Wire and FlightFlowReservationAdapter |
| SQL rowversion and filtered unique active-claim index | Persistence/Operations/Configurations |
| OrderDetails construction | Synchronizer/OrderProjector |
| GetOrder endpoint / tenant/customer disclosure guard | RestApi/<surface>; authoritative access policy in Application |
| GetOrder local SELECT and paging | Query/Orders |
| Recovery worker scheduling | Consumers/Recovery; it invokes Application.ResumeOperation |
| JWT principal parsing | ServiceHost/CallerContext or generic Presentation; return an Application actor record |
| Frozen ActorSnapshot on a commercial change | Domain/Commercial/History |
| Product catalog lookup / data changes | Providers/Ancillary / ReferenceData as appropriate, never Domain |

Existing `Domain/_Shared/Contracts/ICallerContext` is not a reason to let an aggregate resolve the current user. Move request-context interfaces to Application in B0; pass immutable actor/owner snapshots into domain behavior. Pure domain validation of those values remains in Domain.

## Testable architectural enforcement

Create `tests/AeroTech.Ordering.Architecture.Tests`. Test direct AND transitive assembly/package dependencies, project reference graph, concrete DI registrations per profile, and forbidden type usage. Domain must not reference EntityFrameworkCore, AspNetCore, HttpClient, MassTransit, System.Text.Json.Serialization, MediatR or integration Messages. Application may use the existing mediator abstraction/package for dispatch, but no HTTP/EF/broker client types.

Use a call-path smoke test per HTTP/consumer entry point to prove all surfaces reach the same canonical handler. Reflection that finds a second handler or duplicate port registration fails B0/S-stage gates. Architecture assertions must cover namespaces and referenced types, not just folder names.

No blanket `SaveChanges` pipeline wraps every command. It would put remote calls inside a transaction or commit half a phase. Each use case explicitly declares its local transaction boundaries.
