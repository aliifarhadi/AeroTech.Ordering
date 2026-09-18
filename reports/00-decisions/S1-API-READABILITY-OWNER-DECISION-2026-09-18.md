# S1 — API and readability cleanup (owner decision, 2026-09-18)

**Status:** approved by the owner and binding for S1. **Scope:** presentation contract and internal over-engineering.
**Not affected:** every correctness guarantee proven in B0/S1.

This decision is an explicit owner correction to the S1 presentation/API design. Where it conflicts with Pack 3.8
presentation wording (`API-CONTRACTS.md` S1 section and `SPEC/openapi-s1.json`), **this file wins for S1**, per
GOVERNANCE/05 (owner decisions override pack presentation clauses when recorded).

## 1. Binding principles

- Straightforward, readable, explicit, boring code.
- No abstraction whose only justification is removing a few duplicated lines.
- Explicit code over inheritance, generic frameworks, registries, dynamic dictionaries, clever indirection.
- Abstractions survive only where they protect a real architectural or business boundary.
- No behavior for future slices. S2 remains not started.

## 2. Public S1 contract

The public consumer contract is airline business language (Amadeus/NDC Offer → Order), not implementation mechanics.

Routes (historical AeroTech route precedent):

| Surface | Root | Create | Retrieve |
|---|---|---|---|
| Backoffice | `Backoffice/v{version:apiVersion}/Orders` | `POST FlightOffers` | `GET {orderId:long}` |
| OTA | `Api/v{version:apiVersion}/Bookings` | `POST FlightOffers` | `GET {orderId:long}` |
| OTA Panel | `OtaPanel/v{version:apiVersion}/Bookings` | `POST FlightOffers` | `GET {orderId:long}` |
| Service | `Service/v{version:apiVersion}/Bookings` | `POST FlightOffers` | `GET {orderId:long}` |
| Internal | `Internal/v{version:apiVersion}/...` | operational routes only | — |

Removed from the public contract: `/order-preparations`, `/orders/from-offer`, public `/operations/{operationId}`,
`preparationId`, `acceptedSnapshotDigest`, `acceptedAt`, `operationId`, `projectionSchemaVersion`, raw `JsonElement`
details, canonicalization version, source payload hash and internal evidence references.

Kept: `Idempotency-Key` on every mutation, and the surface-scoped `financialCustomerId` / `sellingOfficeId` that
OD-S1-02, OD-S1-09 and OD-S1-10 require on `/backoffice` and `/service`.

## 3. Preparation and recovery mechanics

`OrderPreparation`, the snapshot digest, retained owner evidence and `CommandReceipt` remain **internal**: they are
required for source evidence, idempotency and audit. The caller sees one business operation:

1. resolve the authorized sales scope;
2. check/replay the durable receipt before any owner work;
3. read the offer outside the SQL transaction;
4. normalize and validate it;
5. capture the internal accepted-source evidence;
6. create the Order in one local atomic commit with receipt, projection and outbox;
7. on same-key replay, return the existing Order without a second business mutation.

No network call inside the SQL commit. Production acceptance rules are unchanged: while the real AirOffer contract
proves no authoritative acceptance state, production acceptance stays blocked (BD-001). The internal digest is never
exposed to the consumer as a workaround.

## 4. Consequences accepted with this decision

- The two-step `PrepareOrderFromOffer` + `CreateOrderFromOffer` command pair collapses into one `CreateOrderFromOffer`
  use case. Pack CMD-001 survives as the internal preparation-capture step of that use case, not as a public route.
- Pack QRY-002 `GetOperation` is no longer a sales-surface route; it remains reachable on the internal surface only.
- Future-slice behavior is removed: allocation and reversal policies (`AllocationPolicy`, `AllocationProposal`,
  `AllocationShare`, `PricingReversalPolicy`, `ReversibleLine`). S1 scenario SC-S1-014 (allocation is not money) and the
  reversal half of SC-S1-013 are therefore **deferred to the slice that first persists an allocation or a reversal**;
  the settlement/commission half of SC-S1-013 stays proven.
- Internal-mechanics enums leave the shared wire contracts: `OrderingApiSurface`, `OrderingCommandKind`,
  `CommitConflictKind`, `OfferResolutionOutcome`. Order-vocabulary enums stay with their existing siblings in
  `Contracts/AeroTech.Messages/Ordering/Enums` because they describe order content, not internal mechanics.

## 5. Preserved without exception

One Order under concurrency; same key + same request replays; same key + different request conflicts; one accepted
source consumed once; no network call during the SQL commit; atomic command + projection + outbox; protected PII rules;
customer/surface authorization; owner airline from `IHomeOperatorProvider`; live-vs-reference AirOffer separation; exact
pricing arithmetic; no invented seat/INF semantics; B0 inbox/outbox behavior.
