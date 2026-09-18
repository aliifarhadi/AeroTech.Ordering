# S1 — Prepare, accept, CreateOrderFromOffer and local GetOrder

**Stage:** S1 · **Execution status:** LOCAL_DONE · **Baseline:** B0 `0c027457f3d36059bb7128ceb055612d27e0ce9c` plus an
uncommitted worktree (`SOURCE-COMMIT.txt`) · **Pack:** 3.8 · **Plan:** `PLAN.md` · **Owner decisions:**
`../00-decisions/S1-OWNER-DECISIONS-2026-09-18.md` · **Blockers:** `BLOCKERS.md`

The whole slice now runs end to end over HTTP: `POST /{surface}/v1/order-preparations` → explicit acceptance →
`POST /{surface}/v1/orders/from-offer` → `GET /{surface}/v1/orders/{orderId}` and `GET /{surface}/v1/operations/{operationId}`,
plus `POST /internal/v1/orders/{orderId}/projection-rebuilds`, against real SQL Server, with the reference offer simulator or the
real AirOffer adapter. The real AirOffer host was reachable during this checkpoint, so the live candidate is proven with a genuine
owner response instead of a synthetic fixture.

## 1. Scenario results

Detail per scenario: `TESTS/scenario-results.json`. Runs: `TESTS/runs/`, TRX in `TESTS/trx/`.

| Scenario | Result | Where proven |
|---|---|---|
| SC-S1-001 repriced before acceptance | PASS | reference offer republished at 150 after Prepare; Order keeps 120; zero owner reads during Create |
| SC-S1-002 authoritative simulator Create | PASS | 1 item, 1 service, total 120, CV1/FS1/rev1, one outbox fact, one projection row, no capacity/document/funding effect |
| SC-S1-003 replay after owner outage | PASS | owner unavailable; Create and Prepare replay from the durable receipt; zero owner reads; new Prepare 503 |
| SC-S1-004 key conflict | PASS | same key, other preparation → 409 (20265); nothing changed |
| SC-S1-005 concurrent consumption | PASS | 6 parallel Creates, different keys → 1 Order, 1 receipt, 1 projection row, others 20267 |
| SC-S1-006 atomic projection failure | PASS | injected projection fault → no Order/price set/outbox/receipt/consumption; retry with the same key succeeds |
| SC-S1-007 technical stop | PASS | one air service, two legs (domain + SQL + local view) |
| SC-S1-008 source-priced package | PASS | one item, four services, four membership links, two travelers |
| SC-S1-009 RT vs 2×OW | PASS | both constructions persisted as supplied; JSON differs |
| SC-S1-010 opaque construction | PASS | live AirOffer projection → one conservative package, Opaque context, no invented component links |
| SC-S1-011 double quantity | PASS | extended group line stored once (200), group quantity retained |
| SC-S1-012 currency equivalents | PASS | sale 92 EUR / original 100 USD, plus 1.23456789 round-trip (INV-059) |
| SC-S1-013 sign and settlement | PASS (Domain) | 405 → 450 after an explicit discount reversal; commission excluded; reversal ceiling enforced |
| SC-S1-014 allocation is not money | PASS (Domain) | 60/40 allocation leaves the total at 100; purposes/versions never combined |
| SC-S1-015 source hierarchy totals | PASS | root 119 vs coupons 120 → ContractMismatch; nothing persisted |
| SC-S1-016 invalid traveler binding | PASS | duplicate/missing/foreign/PTC-mismatch/guardian-cycle rejected before any Order |
| SC-S1-017 cross-customer | PASS | command/query **and** HTTP: `/ota` caller cannot read another customer's order or operation (404, identical shape to a missing id), foreign preparation is undisclosed, wrong surface 403, anonymous 401 |
| SC-S1-018 missing validity | PASS | the live AirOffer candidate keeps NotSupplied facts and reports `LIVE_ACCEPTANCE_BLOCKED(BD-001)`; production policy refuses acceptance |
| SC-S1-019 independent deadlines | PASS | per-owner facts; expiry at `now >= due`; price expiry blocks acceptance while the offer fact is still valid |
| SC-S1-020 restart and rebuild | PASS | new provider, owner offline, projection deleted → rebuild reproduces byte-identical JSON; CV unchanged; `/internal` rebuild replay |
| SC-S1-021 unknown registered product | PASS | unregistered schema/version → UnsupportedCapability before any preparation row |

Runs (all on `localhost\SQLEXPRESS`, isolated `OrderingS1_*` databases created and dropped by the run; after the last run zero
`Ordering*` databases remain):

| Run | File | Result |
|---|---|---|
| Domain (S1 set) | `TESTS/runs/01-DOMAIN-TESTS.txt`, `08-DOMAIN-TESTS-FINAL.txt` | 49 / 49 |
| Application + SQL, earlier checkpoint | `TESTS/runs/02-…07-FULL-SUITE-CLEAN.txt` | 83 / 83 (history, including the two corrected failures) |
| API surface and authorization matrix | `TESTS/runs/10-API-SURFACE-MATRIX.txt` | 13 / 13 |
| OpenAPI document | `TESTS/runs/11-OPENAPI-DOCUMENT.txt` | 1 / 1 (operation inventory asserted, `API/openapi.json` written) |
| Live AirOffer candidate | `E2E/LIVE-AIROFFER-RUN.txt` | 1 / 1 (`Category=Live`, run explicitly) |
| **Final clean run** | `TESTS/runs/08-DOMAIN-TESTS-FINAL.txt`, `09-FULL-SUITE-WITH-API.txt`, `TESTS/trx/s1-final.trx` | **49 / 49 domain, 100 / 100 application + SQL + API, 0 build errors, 1 m 47 s** |

Red proofs: `RECOVERY/MUTATION-RUNS.txt` (six S1 rail defects) and `RECOVERY/MUTATION-RUNS-API.txt` (eight surface defects: route
surface unbound from the token surface, backoffice office not checked, inactive customer accepted, `/service` exposing protected
payloads, `/ota` read scope unrestricted, preparation accepted under another sales context, ambiguous OrderReference alphabet and
length, percentage charge silently valued as zero). Every mutation turns its test red and the sources are restored afterwards; two
of them were green on the first attempt and exposed weak tests, which is recorded at the top of that file.

## 2. What the S1 API surface does

| Surface | Authentication (OD-S1-01) | Financial customer / office (OD-S1-02, OD-S1-09, OD-S1-10) | Channel | Protected payloads (OD-S1-04) |
|---|---|---|---|---|
| `/backoffice/v1` | token, `authz_surface=backoffice` | body `financialCustomerId` (any Active synced customer) + body `sellingOfficeId` that must equal the token `airline_office_id` | `BackOffice` | returned |
| `/otapanel/v1` | token, `authz_surface=otapanel` | customer resolved from `travel_agency_id` (Active `TravelAgency` customer); office = token `travel_agency_office_id` | `AgencyPanel` | returned |
| `/ota/v1` | token, `authz_surface=api` | customer = token `customer_id`; office = token `travel_agency_office_id` or null | `PartnerAPI` | returned, restricted to that customer's orders |
| `/service/v1` | none in this platform phase | body `financialCustomerId` required, `sellingOfficeId` optional and recorded as supplied | `System` | always redacted |
| `/internal/v1` | none in this platform phase | — (administration only) | — | never returned |

`CallerScope` — the idempotency namespace of `CommandReceipt` — is `surface|customer:<id>|office:<id|none>|principal:<scope>`
where `<scope>` is `agency:<travelAgencyId>` for OtaPanel, `partner:<profileId>` for OTA/API and `none` elsewhere. No individual
user id is part of it; the actor (airline user, agency user, partner profile) is stored separately on the preparation, the order
and the order change. On an unauthenticated `/service` or `/internal` request the actor is absent (never `0`) and the request
context is recorded as `BusinessContextType.Service`.

`CreateOrderFromOffer` re-resolves the caller scope and rejects (409, code 20287) any attempt to accept a preparation under a
different sales context than the one it was captured with.

## 3. What was built in this checkpoint

**Contracts (`Ordering/**` only)**: `Enums/OrderingApiSurface` added to the S1 enum set.

**ReferenceData**: the Customer projection now matches Core's real `Service/v1/Customers` contract
(`CustomerNumber`, `CustomerType`, `TravelAgencyId`, `SubjectId`, `SubjectName`, `CustomerStatus`, `PreferredCurrencyId`) — the
previous shape (`UniqueIdentifier`, `Name`, `CityId`, contact, `ActivationStatus`) could not have been deserialized from the owner
and carried no agency link. Migration `S1CustomerScope`, plus an index on `TravelAgencyId`.

**Domain**: `_Shared/Contracts/ICustomerDirectory` + `_Shared/ValueObjects/CustomerRelationship` (local reference snapshot, the same
pattern as `IHomeOperatorProvider`); new error code 20287 `SalesContextMismatch`.

**Application**: `AuthorizedScopeResolver` now resolves the full authorized scope per surface from `ICallerContext` +
`ICustomerDirectory` + `IHomeOperatorProvider` and produces the `CallerScope` key (`CallerScopeKey`); commands carry a
`SalesScopeRequest`/`AdministrativeScopeRequest` instead of a caller-built authority, so every entry point authorizes through one
path; `OrderCreationOptions.MaxReferenceAttempts` bounds the Create retry after an `OrderReference` collision.

**Persistence**: `RandomOrderReferenceGenerator` (OD-S1-03) replaces the placeholder that failed closed;
`_Shared/CustomerContext/ReferenceDataCustomerDirectory` reads the customer projection.

**Query**: `GetOrder`/`GetOperation` resolve their own read scope (`AuthorizedReadScope`) — customer restriction and
protected-payload permission — and the protected traveler/contact payloads are read from their own tables
(`Commercial.OrderTravelers`, `OrderTravelerIdentities`, `OrderContacts`, mapped read-only and excluded from query migrations).

**Providers**: the AirOffer mapper values a percentage charge row from its sale-currency `equivalentAmount` (OD-S1-08) and still
rejects a percentage row that supplies no sale-currency valuation.

**RestApi**: `_Shared/ApiSurfaceRoutes` plus one thin controller per surface in
`V1/{OrderPreparationAggregate,OrderAggregate,OperationAggregate}/Controllers` over a shared abstract base per aggregate; requests
and responses in the sibling `Requests`/`Responses` folders. Controllers depend on `IMediator` only.

## 4. Owner and environment status

| Owner | Profile | Mode | Status |
|---|---|---|---|
| AirOffer | `LIVE-CANDIDATE-SANDBOX` | real adapter | **Live candidate proven** against `http://localhost:5095` (`E2E/LIVE-AIROFFER-RUN.txt`): real priced offer → normalized candidate, 264 005 012 IRR, 1 traveler, 2 services, 7 pricing lines, digest `f00010da…`. Acceptance remains `LIVE_ACCEPTANCE_BLOCKED(BD-001)` and the fulfillment profile stays uncertified (OD-S1-07) |
| AirOffer | `REFERENCE-OFFER-2.0` | reference simulator | Owner-bound candidates, independent durable store, read counters, availability switch |
| Core (customers, operator settings) | — | ReferenceData sync | Contract-aligned; the staging customer endpoint currently returns an empty collection, so live customer rows were not observed |
| FlightFlow / JetPay / document issuer / Ledger | — | — | Not touched by S1 (zero calls asserted) |

## 5. Points for the owner

1. **`Idempotency:DigestKey`** and **`Jwt:Authority`/`Jwt:Audience`** are required host configuration with no default.
2. **Production** refuses `Providers:UseDeterministicTestAdapters=true` at startup and refuses every acceptance profile (both are
   sandbox/reference) — Production cannot accept an Order until BD-001 closes.
3. **`/service` and `/internal` are open** by the current platform phase (OD-S1-01). They must stay cluster-internal by deployment
   topology; `/service` reads are redacted precisely because the surface is unauthenticated.
4. **No office reference data exists** (OD-S1-09). A `/service` caller can name any office and it is recorded as supplied.
5. The **Core customer projection change** is a repair of a stale contract, not a new mapping decision; it is described in §3.

## 6. Divergences recorded

- Pack `CONTRACTS/01` places port types in Application; the standing convention C2 (`Domain/Ports/{Area}`) wins, as GOVERNANCE/05
  requires.
- `PrepareRequest`/`CreateRequest` carry `financialCustomerId` (+ `sellingOfficeId`) on `/backoffice` and `/service` — the approved
  S1 change from the Pack's generic body (OD-S1-02).
- Responses keep the existing framework envelope (`{ data, errors }`, C13) rather than the Pack's `Error` schema, and expose
  `operationId` on Prepare/Create so the durable operation identity is reachable.
- Candidate `currencyRef` is the platform `CurrencyId` string for AirOffer-sourced candidates; the sale-currency code stays in the
  retained raw evidence.
- Outbox stream uniqueness is `(StreamKind, StreamId, EventOrdinal)` without `OwnerAirlineId` (DOMAIN/13 names the owner too):
  `StreamId` is a global snowflake Order id and `IOutboxWriter` has no owner in scope.
- S1 persists no `AllocationSet` rows: the accepted sources supply no allocation purpose beyond the line itself; the policy and its
  tests exist for the first source that does.
- The percentage charge row keeps its percentage basis only in the retained raw evidence and `sourceLineRef`; the normalized
  candidate contract was not extended (OD-S1-08).
- `Query` depends on `Application` for the authorization resolver (pre-existing project reference); no query mutates anything.

## 7. Checkpoint

S1 stops here. S2 is not started.
