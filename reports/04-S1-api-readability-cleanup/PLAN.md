# S1 — API and readability cleanup: plan

**Authority:** `reports/00-decisions/S1-API-READABILITY-OWNER-DECISION-2026-09-18.md` (owner), `CLAUDE.md`,
and every S1 decision already closed (OD-S1-01..10). **Baseline:** `3135b2ca84887a3264fd673fa959679e7ba50fda`
(clean tree). **No S2 work.**

## 1. Frozen "before" picture

Public routes at the baseline (from `reports/03-S1-prepare-create-get/API/openapi.json`):

```
POST /{service|backoffice|otapanel|ota}/v1/order-preparations
POST /{service|backoffice|otapanel|ota}/v1/orders/from-offer
GET  /{service|backoffice|otapanel|ota}/v1/orders/{orderId}
GET  /{service|backoffice|otapanel|ota}/v1/operations/{operationId}
POST /internal/v1/orders/{orderId}/projection-rebuilds
GET  /api/v1/Ping, POST /Syncer/v1/* (unchanged, owned elsewhere in the repo)
```

Create request carried `preparationId`, `acceptedSnapshotDigest`, `acceptedAt`; the create response carried
`operationId`, `acceptedSourceDigest`, `orderRevisionAtCommit`; the read response carried `projectionSchemaVersion`,
`projectedAt` and a raw `JsonElement details`. Baseline tests: 49 domain, 100 application/SQL/API, 1 live.

Usage audit (production references, excluding tests):

| Type | Referenced by production S1 code | Action |
|---|---|---|
| `AllocationPolicy`, `AllocationProposal`, `AllocationShare` | none | remove |
| `PricingReversalPolicy`, `ReversibleLine` | none | remove |
| `OrderingApiSurface` | Application, Query, RestApi | move out of `Contracts/AeroTech.Messages` |
| `OrderingCommandKind` | Domain receipt, Application, Query | move to Domain |
| `CommitConflictKind` | Domain, Persistence, Synchronizer | move to Domain |
| `OfferResolutionOutcome` | Domain port, Providers | move to Domain |
| `ServiceDetailSchemaRegistry` | candidate validation + `OrderService` | replace with explicit AirTransport rules |
| `PricingArithmetic`, `PricingLineMatrix`, `DeadlinePolicy`, `TravelerBindingPolicy` | `Order`, validators | keep |

## 2. Work order

1. **Public contract** — one explicit controller per surface (`Backoffice/v1/Orders`, `Api/v1/Bookings`,
   `OtaPanel/v1/Bookings`, `Service/v1/Bookings`) with `POST FlightOffers` and `GET {orderId:long}`; internal
   controllers for projection rebuild and operation lookup. Delete the three parallel controller families, the shared
   base controllers and `ApiSurfaceRoutes`.
2. **Requests/responses** — immutable records with business fields only; typed booking response (identity, sales
   context, travelers, itinerary, items/services, pricing, total); protected traveler/contact data by the existing
   surface rules; no `JsonElement`, no digests, no operation ids.
3. **One use case** — `CreateOrderFromOffer` resolves scope → replays receipt → reads the offer outside SQL →
   normalizes/validates → captures preparation evidence → commits Order + receipt + projection + outbox in one
   transaction. `PrepareOrderFromOffer` disappears as a command; its capture step lives inside the use case.
4. **Future behavior out** — delete allocation/reversal policies and their tests; record the scenario consequence.
5. **Shared contracts** — move the four internal enums into the Ordering projects.
6. **Typed over dynamic** — typed projection document shared by the projector and the read side; typed candidate
   serialization; explicit AirTransport service details.
7. **Large methods** — split `Order.AcceptOriginalSale`, the create handler, `AirOfferCandidateMapper`,
   `AuthorizedScopeResolver` and `OrderProjector` into well-named private steps inside the same class.
8. **Tests** — rewrite the API tests against the new contract, keep every semantic S1 scenario, add contract guards
   that fail if a removed technical field or route reappears.
9. **Evidence** — regenerate OpenAPI and `requests.http`, update the S1 report/status, write the checkpoint report.

## 3. Known trade-off carried into the report

The normalized candidate keeps the Pack's `detailSchema` / `detailSchemaVersion` / `details` fields: it is the owner
source-evidence contract (`SPEC/schemas/normalized-candidate.schema.json`) whose canonical digest is proven against the
Pack example. Only the generic registry class is removed; the domain and the API expose the typed `AirTransportDetail`.
