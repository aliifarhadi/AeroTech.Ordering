# S1 — Plan: PrepareOrderFromOffer → explicit acceptance → CreateOrderFromOffer → GetOrder

Authority: Pack 3.8 `SLICES/S1.md`, `RUNBOOKS/S1.md`, `CONTRACTS/02-AIROFFER.md`, `API-CONTRACTS.md`, `DOMAIN/01–04, 06, 08,
10, 13`, `ARCHITECTURE/01, 03, 04`, `SPEC/openapi-s1.json`, `SPEC/schemas/*`, `EVENT-CATALOG.md` EV-001; `CLAUDE.md`.
Baseline: B0 frozen at `0c027457f3d36059bb7128ceb055612d27e0ce9c`. Decisions: `reports/00-decisions/S1-OWNER-DECISIONS-2026-09-18.md` (OD-S1-01..10, all closed); the questions as raised are kept in `reports/00-decisions/S1-OPEN-DECISIONS.md`.

Out of scope (S2+): reservation/hold, funding coverage or payment execution, ETKT/EMD, cancellation, refund, exchange,
disruption, split, group. S1 records the original-sale `FundingObligation` only (DOMAIN/06, S1 work item 4).

## 1. Commands, events, invariants

| ID | C# | Rail |
|---|---|---|
| CMD-001 PrepareOrderFromOffer | `PrepareOrderFromOfferCommand(Handler)` | receipt lookup → `IOfferSourcePort.ResolveCandidate` (outside SQL) → validate/normalize → one TX: preparation + receipt |
| CMD-002 CreateOrderFromOffer | `CreateOrderFromOfferCommand(Handler)` | receipt lookup → one TX: receipt + preparation consumption + Order graph + change/price set/lines + obligation + OrderDetails + outbox; zero owner calls |
| QRY-001 GetOrder | `GetOrderQuery(Handler)` (Query) | local `ReadModel.OrderDetails`, scope-filtered, `minRevision` |
| QRY-002 GetOperation | `GetOperationQuery(Handler)` (Query) | local receipt-backed operation view |
| ADM-001 RebuildOrderProjection | `RebuildOrderProjectionCommand(Handler)` | canonical snapshot → same `OrderProjector` → conditional replace |
| EV-001 OrderCreated | `Ordering/IntegrationEvents/V2/OrderCreated` | V1 `OrderCreated` has a different published shape → new version folder (CLAUDE.md rule) |

Invariants: INV-006…018, INV-054, INV-055, INV-059 (+ INV-058 for SC-S1-021, INV-003 for SC-S1-006).

## 2. Scenario → proof

| Scenario | Domain (`Domain.Tests`) | Application + SQL (`Persistence.Tests`) | API |
|---|---|---|---|
| 001 repriced before acceptance | — | reference source changes price after Prepare; Create stores prepared amounts; owner call count unchanged | OD-S1-01/05 |
| 002 simulator Create | Order factory: 1 item/1 service, total 120, CV1/FS1 | end-to-end Prepare/Create on SQL, no reservation/document/payment rows | 〃 |
| 003 replay after outage | — | source disabled; replay returns same result from durable receipt; receipt-path marker | 〃 |
| 004 key conflict | — | same key, other preparation → 409, original rows unchanged | 〃 |
| 005 concurrent consumption | — | two keys race on SQL; one Order; `PreparationAlreadyConsumed`; one OrderDetails row | 〃 |
| 006 projection failure | — | fault-injected projector throws before commit → no Order/lines/outbox/receipt/consumption | 〃 |
| 007 technical stop | normalization: 1 segment, 2 legs, 1 service | persisted legs, one service | 〃 |
| 008 package | 2 travelers × 2 segments → 1 item, 4 services | persisted membership links | 〃 |
| 009 RT vs 2×OW | construction retained as supplied | both persisted, structures differ | 〃 |
| 010 opaque | AirOffer mapper → `Opaque`, one package, no PU/FC links | persisted opaque context | 〃 |
| 011 double quantity | group quantity 2, line 200 → total 200 | persisted 200 | 〃 |
| 012 currency equivalents | USD 100 original / EUR 92 sale → total 92 | decimal round-trip both values | 〃 |
| 013 sign/settlement | 405 → reversal 450; commission excluded; reversal ceiling | — (no S1 servicing command) | — |
| 014 allocation | 100 with 60/40 → total 100; purposes/versions separate | — | — |
| 015 hierarchy totals | AirOffer mapper root 120 vs coupons 119 → ContractMismatch | Prepare rejects, nothing persisted | 〃 |
| 016 traveler binding | duplicate/missing/foreign/PTC mismatch → rejected | 422, no Order | 〃 |
| 017 cross-customer | — | other customer: preparation/order/operation/receipt not disclosed | OD-S1-01/02/05 |
| 018 missing validity | sandbox candidate: NotSupplied kept, blocking reason `LIVE_ACCEPTANCE_BLOCKED(BD-001)`; production policy refuses | persisted NotSupplied | 〃 |
| 019 independent deadlines | offer/price/hold facts keep owners; earliest is display only; `now >= due` | — | — |
| 020 restart/rebuild | — | new host scope, source offline, projection row deleted, rebuild → identical JSON, CV unchanged | 〃 |
| 021 unregistered product | schema/version not registered → UnsupportedCapability | Prepare rejects before persistence | 〃 |

Architecture checks (existing `Persistence.Tests/Architecture`) extended: no provider → Persistence/EF writes; every `*Query`
in Query; one handler per S1 command; one `IOrderQuerySynchronizer` binding; C1 allowlist still holds.

Live: real AirOffer adapter wire-mapping tests against the observed DTO shape; live-candidate run needs AirOffer up
(BLOCKED_ENVIRONMENT today) and remains `LIVE_ACCEPTANCE_BLOCKED(BD-001)`.

## 3. Files by layer

- **Contracts/AeroTech.Messages/Ordering**: `Enums/` new — `AcceptanceAssurance`, `ValidityState`, `OrderItemKind`,
  `FareConstructionAssurance`, `ReservationRequirement`, `FulfillmentDocumentKind`, `FundingObligationPurpose`,
  `OfferResolutionOutcome`, `OrderingCommandKind`; reused — `PricingComponentType`, `PricingEffect`,
  `OrderPricingLineDirection`, `PricingLineRole`, `PricingAllocationPurpose/Method/Completeness`, `PricingBasisType`,
  `SegmentKind`, `ServicePriceTreatment`, `OrderServiceType`, `OrderServiceCommercialStatus`, `CommercialSummary`,
  `ContactRole`, `OrderChangeType`, `PriceChangeReason`, `FarePricingUnitType`, `FareCombinationMethod`,
  `CommandReceiptStatus`. `IntegrationEvents/V2/OrderCreated`.
- **Domain**: `_Shared/ValueObjects` (Money, CurrencyRef, ValidityFact, ActorRef), `_Shared/Contracts/IStreamFact`,
  `Ports/Offers` (IOfferSourcePort + request/result types), `OrderPreparationAggregate` (root, candidate value graph,
  `CandidateValidator`, repository contract), `OrderAggregate` (root + entities: traveler, contact, segment/leg, item,
  service + beneficiary/coverage + air detail, item-service link, change, price change set, pricing line, allocation
  set/rows, fare construction, funding obligation; policies: pricing arithmetic/matrix/reversal, allocation, deadlines,
  traveler binding; `OrderCreatedDomainEvent`; contracts: repository, reference generator, query synchronizer),
  `CommandReceiptAggregate`, `ExceptionFactory` codes from 20265.
- **Application**: `OrderPreparationAggregate/Commands/PrepareOrderFromOffer`, `OrderAggregate/Commands/CreateOrderFromOffer`,
  `OrderAggregate/Commands/RebuildOrderProjection`, `OrderAggregate/EventHandlers` (outbox), `_Shared/Idempotency`
  (canonical JSON `ordering-canonical-json-v1`, keyed request digest, receipt coordinator), `_Shared/Acceptance`
  (acceptance-profile policy abstraction).
- **Persistence**: `OrderPreparationAggregate/`, `OrderAggregate/`, `CommandReceiptAggregate/` configurations +
  repositories; schemas `Commercial` and `Operations` (DOMAIN/13); transaction enlistment of the projection;
  outbox `StreamKind/StreamId/EventOrdinal` + unique index; migration `S1CommercialCreate`.
- **Providers**: `AirOffer/{Options,Wire,Services}` — Details client + conservative normalization mapper,
  `ReadBoundCandidate` = UnsupportedCapability; profile `LIVE-CANDIDATE-SANDBOX`.
- **Providers.Deterministic**: `Offers/` — reference offer catalog in the independent owner store, bound candidates,
  `ReadBoundCandidate`, read counters; profile `REFERENCE-OFFER-2.0`; registered only with the existing flag.
- **Synchronizer**: `OrderAggregate/OrderProjector : IOrderQuerySynchronizer`.
- **Query**: `OrderAggregate/{Models,Configurations,Queries/GetOrder}`, `OperationAggregate/Queries/GetOperation`,
  first `OrderQueryDbContext` migration (`ReadModel.OrderDetails`).
- **RestApi**: `_Shared/ApiSurfaceRoutes` plus `V1/{OrderPreparationAggregate,OrderAggregate,OperationAggregate}/{Controllers,Requests,Responses}` — one thin controller per surface over an abstract base per aggregate (delivered under OD-S1-01/02/04/05).
- **ServiceHost**: acceptance-profile policy from host environment (Production refuses sandbox profile and simulator).
- **ReferenceData**: Customer projection aligned with Core's `Service/v1/Customers` contract and extended with `TravelAgencyId` + customer status (delivered under OD-S1-02).

## 4. Transactions

Prepare: remote read, then one local TX. Create/Rebuild: one explicit SQL transaction on the `OrderingDbContext` connection;
`OrderQueryDbContext` for the projection is created on the same connection and enlisted; outbox rows come from the
domain-event handler during `SaveChanges`. Guards: unique receipt `(OwnerAirlineId, CallerScope, CommandKind,
IdempotencyKey)`; unique `OrderPreparations.ConsumedByOrderId`; unique `Orders(OwnerAirlineId, SourcePreparationId)`;
unique `Orders(OwnerAirlineId, OrderReference)`; preparation rowversion.

## 5. Evidence

`reports/03-S1-prepare-create-get/`: `REPORT.md`, `STATUS.json`, `SOURCE-COMMIT.txt`, `API/`, `DB/`, `CONTRACTS/`,
`TESTS/`, `E2E/`, `RECOVERY/`, `ARCHITECTURE/`, `BLOCKERS.md`, `RUNBOOK.md` (GOVERNANCE/03), items not applicable stated.
