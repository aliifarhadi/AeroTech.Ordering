# S1 — API and readability cleanup: checkpoint

**Authority:** `reports/00-decisions/S1-API-READABILITY-OWNER-DECISION-2026-09-18.md` · **Baseline:** `3135b2ca`
**Presentation precedent:** `E:\Projects\DotAir\Ordering` (branch `main`) — routes, controller topology, command/DTO style.
**Domain semantics:** Pack 3.8 (unchanged). **S2 is not started.**

## 1. Before and after — public routes

| Before (S1 baseline) | After |
|---|---|
| `POST /{service\|backoffice\|otapanel\|ota}/v1/order-preparations` | removed |
| `POST /{surface}/v1/orders/from-offer` | `POST Backoffice/v1/Orders/FlightOffers`, `POST Api/v1/Bookings/FlightOffers`, `POST OtaPanel/v1/Bookings/FlightOffers`, `POST Service/v1/Bookings/FlightOffers` |
| `GET /{surface}/v1/orders/{orderId}` | `GET Backoffice/v1/Orders/{id}`, `GET Api/v1/Bookings/{id}`, `GET OtaPanel/v1/Bookings/{id}`, `GET Service/v1/Bookings/{id}` |
| `GET /{surface}/v1/operations/{operationId}` | removed (the operation view was a reliability concept, not a booking concept) |
| `POST /internal/v1/orders/{orderId}/projection-rebuilds` | `POST Internal/v1/Orders/{id}/ProjectionRebuilds` |

## 2. Before and after — create request

Before (technical: the caller had to run a preparation first and echo its digest):

```json
{ "preparationId": "639252830769193205",
  "acceptedSnapshotDigest": "f00010dadcf1ea25d8fae853c40ae22650d3554769231c239b2f907196979611",
  "acceptedAt": "2026-09-18T09:00:00Z",
  "financialCustomerId": "100", "sellingOfficeId": "10",
  "travelerBindings": [ { "sourceTravellerRef": "ADT-1", "clientTravelerRef": "CLIENT-ADT-1", … } ],
  "contacts": [ … ] }
```

After (business only, one call):

```json
{ "customerId": "100", "airlineOfficeId": "10", "offerId": "<priced offer id>",
  "travellers": [ { "offerTravellerRef": "ADT-1", "travellerRef": "PAX-1",
                    "firstName": "Sample", "surName": "Traveler",
                    "passengerType": "ADT", "dateOfBirth": "1990-01-01",
                    "guardianTravellerRef": null } ],
  "contacts": [ { "role": "Primary", "email": "traveler@example.invalid", "phone": null } ],
  "clientReference": null }
```

`Idempotency-Key` stays the mutation header. `/Api` and `/OtaPanel` bodies carry no customer or office at all (both come from
the token). Full examples: `requests.http`.

## 3. Before and after — order response

Before: `{ orderId, operationId, orderReference, commercialVersionAtCommit, orderRevisionAtCommit, acceptedSourceDigest, orderUrl }`
for create and `{ orderId, orderRevision, projectionSchemaVersion, projectedAt, details: <raw JsonElement>, protectedTravelers[], protectedContacts[] }`
for read.

After — create returns a small typed result, read returns a typed order:

```json
{ "orderId": "639…", "orderReference": "K7M2PQD4", "status": "Active", "grandTotal": "120", "currencyRef": "EUR" }
```

```json
{ "orderId": "639…", "orderReference": "K7M2PQD4", "status": "Active", "commercialVersion": 1,
  "offerId": "REF-OFFER-…", "channel": "BackOffice", "customerId": "100", "airlineOfficeId": "10",
  "grandTotal": { "amount": "120", "currencyRef": "EUR" },
  "travellers": [ { "travellerId": "…", "travellerRef": "CLIENT-PAX-A", "offerTravellerRef": "PAX-A",
                    "passengerType": "ADT", "guardianTravellerId": null,
                    "firstName": "Sample", "surName": "Traveler", "dateOfBirth": "1990-01-01" } ],
  "contacts": [ { "role": "Primary", "email": "traveler@example.invalid", "phone": null } ],
  "itinerary": [ { "segmentId": "…", "sequence": 1, "originRef": "AIRPORT-A", "destinationRef": "AIRPORT-B",
                   "departure": "…", "arrival": "…", "flightRef": "FLIGHT-SEG-A",
                   "legs": [ { "sequence": 1, "legRef": "LEG-SEG-A" } ] } ],
  "items": [ { "itemId": "…", "kind": "OfferPackage", "status": "Active",
               "acceptedTotal": { "amount": "120", "currencyRef": "EUR" },
               "services": [ { "serviceId": "…", "serviceType": "AirTransportation", "status": "Active",
                               "quantity": 1, "quantityUnit": "PassengerSegment",
                               "travellerIds": [ … ], "segmentIds": [ … ],
                               "airTransport": { "cabinRef": "ECONOMY", "bookingClass": "Y", … } } ] } ],
  "pricing": [ { "lineId": "…", "itemId": "…", "component": "Fare", "effect": "CustomerBalance",
                 "direction": "Debit", "saleValue": { … }, "originalValue": { … } } ],
  "creationDate": "…" }
```

On `Service/v1/Bookings/{id}` the traveller name fields are `null` and `contacts` is empty (OD-S1-04 unchanged).

## 4. Deleted abstractions and types

| Deleted | Why |
|---|---|
| `OrderingApiSurface` enum and `SalesScopeRequest` / `AdministrativeScopeRequest` | the surface is the controller and the command, not a value passed around |
| `ApiSurfaceRoutes`, `OrdersController`, `OrderPreparationsController`, `OperationsController` base classes | three parallel controller families and their inheritance replaced by one explicit controller per surface |
| `PrepareOrderFromOffer` command, handler, validator, `PreparationResult` | Prepare is now the internal accepted-source capture inside `CreateOrderFromOfferService` |
| `GetOperationQuery`, `OperationDetailsView`, `OperationReadModel` and its configuration | the public operation lookup was reliability mechanics |
| `AllocationPolicy`, `AllocationProposal`, `AllocationShare`, `PricingReversalPolicy`, `ReversibleLine` | S2+ behavior that S1 never executes (consequence recorded in the decision) |
| `OrderDetailsDocument` (dictionary builder) and the `JsonElement` API model | replaced by the typed `OrderDto` graph used by both the projector and the read side |
| `OrderPreparation.BlockingReasons` and its reason constants, `AcceptedDigestMismatch` (20268) | the caller no longer accepts a digest, so the mismatch path and the reason strings had no consumer |
| `TestOrderReferenceGenerator` | the production generator is used in tests too |
| `MinRevision` / `ProjectionRevisionLagging` on the read path | projection lag was an implementation concept on a synchronous create |

Internal-mechanics enums left the shared wire contracts: `OrderingCommandKind` → `Domain.CommandReceiptAggregate`,
`CommitConflictKind` → `Domain._Shared.Exceptions`, `OfferResolutionOutcome` → `Domain.Ports.Offers`.

## 5. Invented types replaced by the existing contracts

The previous S1 modelled several concepts as strings although the platform already had the enum. They now use the shared
enums everywhere (domain, persistence as `int` columns, DTOs and requests):

| Was | Now |
|---|---|
| `string PassengerTypeCode` on the traveller binding, the order traveller and the candidate | `AeroTech.Messages.Ordering.Enums.PassengerTypeCode` |
| `string Channel` on the sales scope, the preparation, the order and the candidate | `AeroTech.Messages.Shared.Enums.SalesChannel` |
| `string QuantityUnit` on the service and the candidate | `AeroTech.Messages.Ordering.Enums.OrderItemUnitOfMeasure` (`PassengerSegment` added — the pack's value the enum lacked) |

`CandidateVocabulary` keeps the pack's wire words (`Backoffice`, `PassengerSegment`, `AirTransport`), so the candidate
contract and its canonical digest are unchanged. The domain allowlist now includes `SalesChannel`; that is the only
allowlist change and it is recorded in the owner decision.

## 6. One business operation

`CreateOrderFromOfferService.ExecuteAsync` is the single use case behind all four surfaces:

1. the surface handler resolves its own authorized sales scope (`AuthorizedScopeResolver.BackofficeSaleAsync`, `OtaSaleAsync`,
   `OtaPanelSaleAsync`, `ServiceSaleAsync`);
2. the durable receipt is checked first — a replay returns the original order without any owner call;
3. the offer is read through `IOfferSourcePort` **outside** the SQL transaction;
4. the candidate is normalized and validated;
5. the accepted-source evidence (`OrderPreparation` + retained owner payload) is captured;
6. order, receipt, projection and outbox commit in one local transaction;
7. an `OrderReference` collision regenerates and retries the local commit.

## 7. Tests

`Order.AcceptOriginalSale` now delegates to `AddTravelers`, `AddContacts`, `AddSegments`, `AddItemsAndServices`,
`AddPricing`, `AddFareConstruction`, `AddOriginalSaleObligations` inside the same aggregate.

Contract guards added: the public create/read payloads must not contain `preparationId`, `acceptedSnapshotDigest`,
`acceptedSourceDigest`, `operationId`, `projectionSchemaVersion`, `sourcePayloadHash`, `canonicalizationVersion` or
`callerScope`; the retired `/order-preparations` and `/orders/from-offer` routes must not answer; the controller inventory
and the generated OpenAPI operation list are characterized exactly.

## 8. Remaining complexity, and why it stays

- **`OrderPreparation` and the snapshot digest** remain internal: they are the accepted source evidence, the consumption
  guard and the audit trail. They are invisible on HTTP.
- **`NormalizedCandidate` + `CanonicalJson` + `NormalizedCandidateJson`**: the canonical writer/reader is the pack's
  `ordering-canonical-json-v1` contract and reproduces the published example digest. Making it plain `System.Text.Json`
  would change every digest and break source-evidence conformance.
- **`detailSchema` / `detailSchemaVersion` / `details` on the candidate** stay because `SPEC/schemas/normalized-candidate.schema.json`
  defines them; the domain and the API expose the typed `AirTransportDetail` instead. Only the generic registry behaviour is
  limited to the single registered AirTransport schema.
- **`CommandReceipt`** stays: it is idempotency and replay.

## 9. Tests executed

```
dotnet build AeroTech.Ordering.sln
dotnet test tests/AeroTech.Ordering.Domain.Tests                           -> 46 / 46
dotnet test tests/AeroTech.Ordering.Persistence.Tests --filter "Category!=Live" -> 107 / 107 (35 s)
ORDERING_OPENAPI_OUT=... --filter "FullyQualifiedName~OpenApiDocumentTests" -> openapi.json regenerated and reviewed
```

Every S1 business scenario still passes (SC-S1-001…021 minus the two deferred by §4 of the owner decision), the
authorization matrix passes unchanged, and the new contract guards pass. Baseline before the cleanup: 49 domain +
100 SQL/API.

`reports/04-S1-api-readability-cleanup/openapi.json` now declares typed requests **and** responses: `OrderDto`,
`CreateOrderFromOfferResult`, `RebuildOrderProjectionResult`, plus the platform enums (`PassengerTypeCode`, `SalesChannel`,
`OrderServiceType`, `CommercialSummary`, …). 16 operations total, of which 10 belong to Ordering.

## 10. Size

`git diff --stat` against `3135b2ca`: **100 files changed, 747 insertions, 5702 deletions** (plus the new controller,
command, DTO and test files listed above).

## 11. The service actually runs (smoke test on the real host)

The Development profile was broken before this checkpoint: `Providers:UseDeterministicTestAdapters` was `true` with no
`ConnectionStrings:DeterministicOwnerDbContext` and no `Idempotency:DigestKey`, so `dotnet run` died at startup with
`ArgumentNullException: ConnectionStrings:DeterministicOwnerDbContext`. `appsettings.Development.json` now runs the **real**
AirOffer adapter (`Providers:UseDeterministicTestAdapters: false`, matching `Offer:BaseUrl = http://localhost:5095/service/`),
carries a local `DeterministicOwnerDbContext` connection string for when the simulator is switched on, and a local
`Idempotency:DigestKey`.

Executed against the running host (`dotnet run`, Development, `http://localhost:5555`) with the three dev databases migrated:

| Call | Result |
|---|---|
| `GET /api/v1/Ping` | `200 {"data":{"status":"ok"}}` |
| `GET /swagger/v1/swagger.json` | the 16 operations of §1 |
| `POST /Syncer/v1/OperatorSettings` (real Core staging) | `200`, home operator row present |
| `POST /Service/v1/Bookings/FlightOffers` with an unknown customer | `403` code 20285 — authorization works |
| `POST /Service/v1/Bookings/FlightOffers` with the real AirOffer offer | **`201`** `{"orderId":"1550595833131433984","orderReference":"X4DFS78H","status":"Active","grandTotal":"264005012","currencyRef":"70"}` |
| `GET /Service/v1/Bookings/1550595833131433984` | `200` typed order: 2 segments, 1 item, 2 air services, 7 pricing lines, `quantityUnit: PassengerSegment`, traveller names `null` and `contacts` empty (service surface redaction) |
| the same `Idempotency-Key` again | the same order returned, no second order |

`POST /Syncer/v1/Customers` returns `200` with zero rows because Core staging currently exposes no customers; the smoke test
used one locally inserted dev customer row.

## 12. Database schema (owner decision OD-C-01)

`Commercial` came from pack `DOMAIN/13-PERSISTENCE-DATA-DICTIONARY.md` ("schema areas `Commercial`, `Fulfillment`,
`Operations`, `Delivery`, `ReadModel`, `Messaging`, `Groups`, `Reference`"), which conflicts with the legacy service
(`OrderingDbContext.HasDefaultSchema("Order")`). The owner resolved it in favour of the legacy convention.

| Area | Before | After |
|---|---|---|
| Orders, items, services, segments, travellers, contacts, pricing, fare constructions, funding obligations, preparations, source evidence | `Commercial` | **`Order`** |
| Command receipts | `Operations` | `Operations` (untouched; OD-C-01b still open) |
| Order projection | `ReadModel` | `ReadModel` (already legacy-shaped) |
| Reference snapshots | `ReferenceData` | `ReferenceData` |
| Outbox / inbox | `dbo` | `dbo` |

The S1 migration was rebuilt from the B0 baseline as `20260918201810_S1OrderCreate` (a single create, no rename chain) and
applied to the local dev database; `has-pending-model-changes` is clean for all three contexts.
