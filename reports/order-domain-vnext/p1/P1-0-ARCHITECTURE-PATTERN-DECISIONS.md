# P1.0 — Architecture Pattern Decisions

Purpose: fix the DDD / Clean Architecture / CQRS shape of the first vNext slice before any business code is
written. Every later slice copies it. Fill every **Answer** line; anything left blank stays blocked.

Inputs:
- Skeleton in this repo (Framework `AggregateRoot<TId>`, `CommandDbContext` pre-commit event dispatch, outbox/inbox).
- Read-only study of `E:\Projects\DotAir\StoredValue` (patterns only).
- Rules carried from legacy Ordering (`CLAUDE.md`).

Limitation: `docs/order-domain-design-vnext/` is still empty. Business names below (`CommandReceipt`,
`FulfillmentReservation`, `OrderView`) are placeholders and will follow the pack once it is present. The pack wins
on business semantics; this document only fixes mechanics.

---

## A. Already ratified (no answer needed)

- Layer responsibilities, placement and naming rules in `CLAUDE.md`.
- Controllers depend on `IMediator` only; every `*Query` lives in the Query project.
- Domain seam for external services is `Domain/Ports/{Area}/`; `Providers` = real ACLs only; deterministic adapters in
  `Providers.Deterministic`.
- `ExceptionFactory` codes 20000–29999; no inline `BusinessException`.
- Enums and integration events only in `Contracts/AeroTech.Messages/Ordering/**`, following existing objects.
- Reference data (Currency, City, Airport, Airline, Customer, …) is referenced by identity, never re-implemented.
- No comments, no hardcoded parametric values, fail-fast infrastructure config.

---

## B. Proposed reference shape

### B1. Aggregate

```csharp
public sealed class Order : AggregateRoot<long>
{
    private readonly List<OrderItem> _items = new();

    private Order()
    {
    }

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public static Order Create(CreateOrderArgs args, IIdGenerator ids, IClock clock)
    {
        var order = new Order(ids.NewId(), args, clock.GetDateTime());
        order.Causes(new OrderCreated(ids.NewId().ToString(), order.Id.ToString(), clock.GetDateTime(), order.Id));
        return order;
    }
}
```

Private constructors and setters, business-named factory, `*Args` in `{X}Aggregate/Arguments/`, private child
collections exposed read-only, invariants guarded inside and thrown through `ExceptionFactory`, other aggregates held
by identity only.

### B2. Repository contract

```csharp
public interface IOrderRepository
{
    Task<Order?> GetAsync(long id, CancellationToken cancellationToken = default);

    Task AddAsync(Order order, CancellationToken cancellationToken = default);
}
```

`Domain/OrderAggregate/Contracts/`. No `IQueryable`, no list/search/sum methods, no `Update` (change tracking).

### B3. Port

```csharp
public interface IFlightInventoryPort
{
    Task<ReservationResult> ReserveAsync(ReserveRequest request, CancellationToken cancellationToken = default);

    Task<ReservationResult> GetReservationAsync(ReservationLookup request, CancellationToken cancellationToken = default);
}
```

`Domain/Ports/FlightInventory/` with its request/result records. Real adapter in `Providers/FlightFlow/`,
deterministic adapter in `Providers.Deterministic/`.

### B4. Command side

```
Application/OrderAggregate/Commands/CreateOrder/
  CreateOrderCommand.cs          sealed record : IRequest<CreateOrderResult>
  CreateOrderCommandHandler.cs   the use case
  CreateOrderCommandValidator.cs request shape only
  CreateOrderResult.cs           identities, never domain objects
```

The handler loads through repositories and ports, calls aggregate behavior, and calls `IUnitOfWork.SaveChangesAsync`
explicitly (see D3 and D4 for the transaction shape).

### B5. Domain event → integration event

`Causes(domainEvent)` → `CommandDbContext.SaveChangesAsync` dispatches pre-commit →
`Application/OrderAggregate/EventHandlers/Publish{Event}IntegrationEvent` → `IOutboxWriter.WriteAsync(contract, domainEvent)`
→ same transaction → `OutboxPublisher` → RabbitMQ.

### B6. Query side

```
Query/OrderAggregate/Queries/ViewOrder/
  ViewOrderQuery.cs, ViewOrderQueryHandler.cs
Query/OrderAggregate/Models/OrderViewReadModel.cs
Query/OrderAggregate/Configurations/OrderViewReadModelConfiguration.cs
Query/OrderAggregate/Dto/OrderViewDto.cs
```

Handler → `OrderQueryDbContext` (`AsNoTracking`) → DTO. Never repositories, never command stores, never ports.

### B7. Controllers

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("ota/v{version:apiVersion}/orders")]
[Tags("ota")]
public sealed class OtaController : ControllerBase
{
    private readonly IMediator _mediator;

    public OtaController(IMediator mediator) => _mediator = mediator;

    [HttpPost("create")]
    public async Task<IActionResult> Create(OtaCreateOrderRequest request, CancellationToken cancellationToken)
    {
        var created = await _mediator.Send(request.ToCommand(IdempotencyKey.Require(Request)), cancellationToken);
        return Ok(await _mediator.Send(new ViewOrderQuery(created.OrderId), cancellationToken));
    }
}
```

`RestApi/V1/OrderAggregate/{Controllers,Requests,Responses}`. Routes and verbs follow the pack's `05-API-CONTRACTS.md`.

### B8. Consumers and jobs

Thin: `Consumers/{Source}/{X}Aggregate/When{Event}/{Action}When{Event}` and `Consumers/Jobs/*Poller` only build a
command and call `ISender.Send`. No business logic.

---

## C. Decisions

### D1. Handler is the use case — no default application-service layer

- **Evidence:** legacy Ordering put a service behind every handler (handler → `I*Service`), which spread use-case
  logic across two classes. StoredValue handlers are the use case, with small shared classes under
  `Commands/_Shared` when two entry points need the same orchestration.
- **Options:** (a) handler is the use case; extract an Application class only when ≥2 entry points share
  orchestration (e.g. reservation dispatch used by CreateOrder and the recovery command); (b) handler always delegates
  to an application service.
- **Recommendation:** (a). Shared orchestration lives in `Application/{X}Aggregate/Services/{Capability}/`. It
  coordinates only; it never holds rules that belong in the aggregate.
- **Risk to watch:** fat handlers (StoredValue's `AuthorizeWalletCommandHandler` is 188 lines). Rule: if a handler
  decides a business outcome, that decision moves to the aggregate or a domain policy.

**Answer:**

### D2. Channels map to one command per use case

- **Evidence:** the P1 prompt wants thin channel-specific controllers mapping to the same use case. Legacy used
  per-channel command classes (`BackofficeReserveOrderCommand`). StoredValue maps channel requests by hand to one
  command.
- **Options:** (a) one `CreateOrderCommand`; each channel controller maps its own request DTO to it, and the channel
  and sales context come from `ICallerContext` server-side, never from the body; (b) a command class per channel,
  each with its own handler delegating to a shared service.
- **Recommendation:** (a). Add a channel-specific command only when the channel's input or validation genuinely
  differs.

**Answer:**

### D3. OrderView projection commits in the same local transaction

- **Evidence:**
  - The P1.1 algorithm step 8 requires Order + receipt + OrderView projection in one local transaction.
  - StoredValue has no projection (Query reads write tables, Synchronizer empty).
  - **AirPrice** (the read-model reference): `I{Agg}QueryDbSynchronizer` interfaces in Domain, implemented in
    `Synchronizer/`, called **explicitly by every command handler** with the aggregate instance; projectors only
    track changes; `AirPriceUnitOfWork` (in Synchronizer) then saves the command context and the query context
    **one after the other with no shared transaction** (the `TransactionScope` is commented out). Projection is
    upsert + child diff or delete-insert, with no version column. Commands return only the id; clients re-query.
    Derived "compact" tables are also rebuilt asynchronously by consumers.
  - Legacy Ordering used a Synchronizer-owned `IUnitOfWork` that saved and projected together.
- **What we keep from AirPrice:** projection code in `Synchronizer/{X}Aggregate/`; the projector reads the
  **aggregate instance** (not event payloads, not repositories); upsert with child diff; Synchronizer owns the unit of
  work; the Query project owns read-model classes, configurations and migrations (`__QueriesMigrationHistory`,
  schema `ReadModel`).
- **What we do not copy:** the non-atomic double save; the "every handler must remember to call the synchronizer"
  trigger; projector interfaces in Domain (a projection is not a domain concept); projectors doing lookups through
  command repositories or business filtering; no projection version.
- **Options:**
  - (a) **Synchronizer owns `IUnitOfWork`, projection triggered by the unit of work:** begin a transaction on
    `OrderingDbContext` → collect the changed `Order` aggregates from its change tracker → `SaveChangesAsync`
    (events → outbox) → `IOrderProjector.ProjectAsync(order)` on `OrderQueryDbContext` enlisted in the same
    connection and transaction → save → commit. Application only calls `IUnitOfWork.SaveChangesAsync`. The projector
    contract lives in Synchronizer, not Domain. Idempotent: skip when the stored `CommercialVersion` is newer. The
    same projector serves rebuild.
  - (b) AirPrice as-is: handler calls `IOrderQueryDbSynchronizer.Sync(order)` explicitly, and the unit of work saves
    both contexts (fixed to share one transaction). Visible in the handler, but easy to forget on a new command.
  - (c) Async projection by a consumer of the integration event. Eventually consistent, which breaks step 8 and
    "return OrderView" right after create. Allowed only for derived secondary tables.
- **Recommendation:** (a). Unlike AirPrice, CreateOrder returns the OrderView (pack step 15) by sending
  `ViewOrderQuery` after the command. That is only correct because (a) makes the projection read-your-writes and
  atomic.

**Answer:**

### D3b. Read-model naming and reference data in projections

- **Evidence:** AirPrice names read models `*QueryModel` with configurations named inconsistently (`*Config`,
  `*QueryModelConfiguration`, …). Our skeleton, legacy Ordering and ReferenceData use `*ReadModel` and
  `*Configuration`. AirPrice denormalises reference codes (airport IATA, currency code) into the read model at
  write time.
- **Options:** (a) keep `*ReadModel` + `*ReadModelConfiguration` (consistent with ReferenceData already in the
  repo); store reference **identities** in the read model and join `ReferenceData` read models at query time; copy a
  reference value into the read model only when the pack requires a point-in-time snapshot (e.g. accepted-offer air
  snapshot); (b) adopt AirPrice's `*QueryModel` naming and denormalise reference names at projection time.
- **Recommendation:** (a).

**Answer:**

### D4. Transaction shape for commands with external side effects

- **Evidence:** CreateOrder needs "commit intent → HTTP → persist outcome", i.e. two local commits in one use case.
  It also saves several roots together (Order, receipt, reservation operation). StoredValue does the same for money
  atomicity; textbook DDD prefers one aggregate per transaction.
- **Options:** (a) allow several aggregates in one local transaction only where the pack requires atomic local
  intent; everything else is one aggregate per transaction plus events; (b) strict one aggregate per transaction with
  event-driven follow-up.
- **Recommendation:** (a). Rule: never hold a transaction open across a port call; the second commit only records
  provider evidence and schedules recovery.

**Answer:**

### D5. Idempotency is a durable receipt keyed by the `Idempotency-Key` header

- **Evidence:** the pack requires same key + same request → replay, and same key + different request → conflict.
  StoredValue dedups on a business ID plus a canonical SHA-256 request hash, and stores the HTTP header without using
  it. Create has no business ID yet, so the header must be the key.
- **Options:** (a) a `CommandReceipt` aggregate (scope = caller scope + operation + key; canonical request hash;
  status; result identity) written in the D3/D4 local transaction, with its unique index as the concurrency guard,
  replay returning the stored `OrderId` then reading OrderView, and a hash mismatch → `ExceptionFactory` 409; (b) a
  MediatR pipeline behavior. It cannot share the handler's transaction cleanly and hides a business rule.
- **Recommendation:** (a). Placement `Domain/CommandReceiptAggregate/` (renamed to the pack's term);
  `CanonicalRequestHasher` in `Application/_Shared/Hashing/`.

**Answer:**

### D6. Concurrency conflicts become a 409, never a 500

- **Evidence:** `AggregateRoot.RowVersion` exists. StoredValue never catches `DbUpdateConcurrencyException` (it
  surfaces as 500) and relies on Redis locks.
- **Options:** (a) the D3 unit of work translates `DbUpdateConcurrencyException` and unique-key violations of the
  receipt into `ExceptionFactory` errors (409); single-worker recovery uses the pack's operation claim; no Redis lock
  on create; (b) a Redis lock per operation plus optimistic concurrency.
- **Recommendation:** (a). Add a distributed lock only where the pack names one.

**Answer:**

### D7. Rules for domain event handlers

- **Evidence:** Framework dispatches once, before the database write. Events raised inside a handler are lost, and a
  throwing handler aborts the save.
- **Recommendation (rule):** in-process domain event handlers may only write outbox messages. They never mutate
  aggregates, call `SaveChangesAsync`, call ports, or send commands. Follow-up work goes through an integration event
  → consumer → command.

**Answer:**

### D8. Money is a value object

- **Evidence:** StoredValue's `Money` exists but aggregates store raw `decimal` + `int CurrencyId`, and `Money` throws
  inline. Currency decimals and rounding live in `ReferenceData.CurrencyReadModel`.
- **Options:** (a) `Money(decimal Amount, int CurrencyId)` value object in `Domain/_Shared/ValueObjects/`, mapped as an
  EF owned type, with arithmetic guarded against currency mismatch; precision supplied by a Domain contract
  implemented in ServiceHost over `ReferenceDbContext`; (b) raw decimal + currency fields.
- **Recommendation:** (a). Accepted source prices are stored as given, never recalculated.

**Answer:**

### D9. Identity and time

- **Evidence:** Framework and StoredValue use `long` snowflake IDs and pass `IIdGenerator`/`IClock` into factories and
  behavior methods. Legacy parked Guid-on-API.
- **Options:** (a) `long` IDs, generator and clock as method parameters, no strongly typed IDs; (b) strongly typed
  IDs (`OrderId`), which need EF converters, JSON converters and wider framework changes.
- **Recommendation:** (a), consistent with Framework and siblings. Trade-off: the compiler won't catch an
  `OrderId`/`ItemId` mix-up, so argument names must stay explicit.

**Answer:**

### D10. Read ports never live in Application

- **Evidence:** StoredValue declares `IWalletReadStore` in Application, implements it on the command DbContext, and
  injects it into a controller, bypassing MediatR.
- **Evidence (AirPrice):** command handlers inject query services backed by the query DbContext for uniqueness and
  validation (`PointOfSaleQueryService.CheckExistDuplicatedTitleAsync`), and a query service runs domain calculation
  (`AirFareQueryService` → `IRefundCalculationService`).
- **Recommendation (rule):** reject both patterns. Every read is a Query-project `*Query` over `OrderQueryDbContext`,
  reached via `IMediator`. The command side never reads the query side: a handler that needs a read for a decision
  uses repositories or a Domain contract over command data, and uniqueness is enforced by a unique index on the
  command table. Query handlers never run domain calculations.

**Answer:**

### D11. Architecture tests project

- **Evidence:** the P1 prompt requires an executable Providers-layer architecture rule. StoredValue has none. Putting
  it in `Persistence.Tests` would fall under the run-approval rule.
- **Options:** (a) new `tests/AeroTech.Ordering.Architecture.Tests` (xUnit, reflection over assembly references and
  namespaces, no database, runs freely) enforcing: Domain references no infrastructure; Providers does not reference
  Persistence/Application/Query and has no DbContext/repository usage; controllers inject only `IMediator` and caller
  context; no `*Query` outside Query; enums only under `AeroTech.Messages.Ordering.Enums`; (b) put these tests in
  `Domain.Tests` by adding references to every project.
- **Recommendation:** (a). It adds a project, so it needs your approval.

**Answer:**

### D12. Outbox publisher scale-out

- **Evidence:** our `OutboxPublisher` (same as StoredValue) reads unprocessed rows without claiming them. Two
  ServiceHost replicas would publish the same event twice. Our inbox protects our consumers, but not other
  services'.
- **Options:** (a) fix in P1.0: claim a batch atomically (`UPDATE TOP(n) … SET ClaimedBy/ClaimedUntil OUTPUT …`)
  before publishing; (b) defer and run a single replica until fixed.
- **Recommendation:** (a) if staging runs more than one replica; otherwise (b) with a tracked item.

**Answer:**

### D13. Endpoint authorization

- **Evidence:** StoredValue has no `[Authorize]` anywhere. Our `ICallerContext` exposes `AuthorizationSurface` and
  `ContextType` from IdentityServer claims.
- **Options:** (a) one authorization policy per channel (`ota`, `otapanel`, `backoffice`) bound to
  `AuthorizationSurface`, applied per controller, with the pack deciding which caller scopes may view which Order;
  (b) defer authorization as StoredValue did.
- **Recommendation:** (a). The exact claim-to-channel mapping waits for the pack's `05-API-CONTRACTS.md`.

**Answer:**
