# S1 — E2E transcript

Two end-to-end paths were executed in this checkpoint: the HTTP surfaces against a real Kestrel host, and a live AirOffer
candidate against the real owner service.

## 1. HTTP end to end (real host, real JwtBearer, real SQL Server)

Host: `WebApplication` composed by `AddOrderingHost` on `http://127.0.0.1:<random>` with `Providers:UseDeterministicTestAdapters=true`,
the real `JwtBearer` handler and a test-only ephemeral RSA key (OD-S1-05). Broker/outbox background workers are removed so no test
publishes onto the shared dev broker. Source: `tests/AeroTech.Ordering.Persistence.Tests/Api/OrderingApiSurfaceTests.cs`.

1. Publish a reference offer bound to the surface's own sales context (customer, channel, office).
2. `POST /{surface}/v1/order-preparations` with `Idempotency-Key` → **201**: `preparationId`, `operationId`,
   `acceptedSnapshotDigest`, validity facts, `acceptanceAssurance`, `permittedAcceptanceProfile`, the normalized candidate and the
   production-acceptance blocking reasons.
3. `POST /{surface}/v1/orders/from-offer` echoing that digest with the complete traveler binding → **201**: `orderId`,
   `orderReference` (8 characters of `23456789ABCDEFGHJKLMNPQRSTUVWXYZ`), `commercialVersionAtCommit=1`, `orderRevisionAtCommit=1`,
   `acceptedSourceDigest`, `orderUrl`, `operationId`.
4. `GET /{surface}/v1/orders/{orderId}` → **200** local projection; protected traveler/contact payloads on the authenticated
   surfaces, **none** on `/service` (the response body contains neither the traveler name nor the contact address).
5. `GET /{surface}/v1/operations/{operationId}` → **200** `Completed/Succeeded`.
6. `POST /internal/v1/orders/{orderId}/projection-rebuilds` without a token → **201**, same revision, no commercial change.

Authorization matrix actually executed (OD-S1-01):

| Case | Result |
|---|---|
| backoffice / otapanel / api token on its own surface | allowed |
| no token on `/backoffice`, `/otapanel`, `/ota` | **401** |
| token of another surface (including one carrying every other claim) | **403** code 20285 |
| backoffice `sellingOfficeId` ≠ token `airline_office_id` | **403** code 20285 |
| unknown or suspended `financialCustomerId` | **403** code 20285 |
| `/ota` caller reading another customer's order | **404** (identical shape to a missing id) |
| `/service` and `/internal` without a token | allowed; only business rules decide |
| mutation without `Idempotency-Key` | **400** code 20264 |
| accepting a preparation under a different sales context | **409** code 20287 |

## 2. Live AirOffer candidate (real owner, real adapter)

`ORDERING_LIVE_AIROFFER_BASEURL=http://localhost:5095/Service/` with a real priced `offerId`
(`AirOfferLiveOwnerTests.Live_owner_details_resolve_into_an_acceptable_candidate`, transcript `LIVE-AIROFFER-RUN.txt`):

- `POST v1/FlightOffers/Details` answered with the payload captured in `airoffer-live-details.json`
  (round trip, 2 bounds, 1 ticket, 2 coupons, 1 order charge, sale currency 70/IRR, `lastTicketingDate: null`).
- Normalized candidate: 1 traveler, 2 air services, 2 segments, 7 pricing lines, customer total **264 005 012** IRR;
  the coupon fare row is valued from its `equivalentAmount` with the rate-of-exchange reference retained, and the order-level
  percentage charge is valued from its sale-currency equivalent (OD-S1-08).
- Preparation digest `f00010dadcf1ea25d8fae853c40ae22650d3554769231c239b2f907196979611`, assurance `LocalCandidateOnly`,
  blocking reasons `LIVE_ACCEPTANCE_BLOCKED(BD-001)`, `OFFER_VALIDITY_NOT_SUPPLIED`, `PRICE_VALIDITY_NOT_SUPPLIED`.
- The same payload is replayed deterministically in the standard suite
  (`AirOfferLiveOwnerTests.Recorded_live_owner_response_is_normalized_and_prepared`), so the mapping stays proven when AirOffer is
  down.

## 3. In-process rail (unchanged from the previous checkpoint)

Real SQL Server, real MediatR pipeline, real EF persistence, real projector, reference offer simulator with its own database:
Prepare → Create → GetOrder → GetOperation, replay under owner outage with zero owner reads, and restart + projection rebuild
producing identical JSON. Source: `tests/AeroTech.Ordering.Persistence.Tests/S1/*`; output in `TESTS/runs/`.
Owner read counts: `provider-effect-counts.json`.
