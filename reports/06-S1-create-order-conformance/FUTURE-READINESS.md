# Full-Domain Future Readiness

Stage: 06-S1-create-order-conformance · **revision 3** · 2026-09-19 · HEAD `4e48447`

Revision 1 said "13 of 14 families are clean `DEFER_IMPLEMENTATION`". That verdict was **withdrawn** in revision 2: it collapsed two different questions — *is the behavior deferred* and *is the domain shape correct* — into one answer, and answered both with the first.

Revision 3 corrects three classifications and adds a sixth class:

- **`STRUCTURALLY_READY`** — canonical identities exist, shape is Pack-conformant, only behavior is missing.
- **`ADDITIVE_FUTURE_EXTENSION`** — a later slice adds columns or tables; nothing existing must change.
- **`DOMAIN_SHAPE_GAP`** — the shape must be decided now or a later slice will have to migrate accepted data.
- **`FUTURE_DOMAIN_DEPENDENCY`** *(new)* — not an S1 requirement, but it depends on a shape S1 is deciding, so the S1 decision must not foreclose it.
- **`BLOCKED_OWNER_CONTRACT`** — the **values** depend on an owner contract that does not exist. Used as a **secondary** tag; it never competes with the primary classification.
- **`CURRENT_S1_GAP`** — not a future question at all; S1 is already wrong.

Each family has exactly **one primary** classification. `BLOCKED_OWNER_CONTRACT` appears only as a secondary tag.

| # | Family | Slice | Canonical identities | Would an S1 decision force a destructive redesign? | Primary | Secondary |
|---|---|---|---|---|---|---|
| 1 | Reservation / capacity | S2 | `OrderSegment.SourceCapacityRef` retains the AirOffer `FlightCapacityId` as a *source reference*; `FulfillmentProfileSnapshot.ReservationRequirement` exists and reads `Unresolved` | **Partly.** The identities are right, but the snapshot lacks resource **unit policy** and partial-fulfillment support, which S2 is the first slice to need. Adding them later means rewriting accepted snapshots | `DOMAIN_SHAPE_GAP` | `BLOCKED_OWNER_CONTRACT` (BD-002/003) |
| 2 | Payment / funding | S3 | `FundingObligation` has version, purpose, money, change and supersession | **Yes, for scope.** `DOMAIN/06` requires **Service / Item / PricingLine** scope and a **current disposition**; only nullable `OrderItemId` exists. Freezing item-only scope is the destructive choice. *Revision 3 corrects the recommended fix*: three nullable **real FKs** with an exactly-one CHECK, not a polymorphic `ScopeKind` + `ScopeId`, which SQL Server cannot enforce | `DOMAIN_SHAPE_GAP` | — |
| 3 | ETKT / EMD issuance | S4/S7 | `OrderService` is the unit a coupon attaches to; `FulfillmentDocumentKind` exists | **Partly.** The snapshot is missing document **authority**. *Revision 3 correction:* the vocabulary is **not** undefined — `DOMAIN/07` defines LOCAL-AIRLINE / EXTERNAL and `DocumentAuthority { Local, External }` already exists in Contracts. Only the field and the live value are missing. `FulfillmentDocumentKind` already covers requirement **and** type, so no second flag is needed. Document tables themselves are a clean additive group | `DOMAIN_SHAPE_GAP` | `BLOCKED_OWNER_CONTRACT` (BD-006, for the live value) |
| 4 | Cancellation | S5 | commercial status vocabularies exist | **Yes.** `OrderServiceCommercialStatus` lacks `Replaced` and `Expired` and carries `Exchanged`/`Suspended`, which the Pack does not define on this axis; `OrderItemCommercialStatus` lacks four states. Cancelling with the wrong vocabulary means migrating accepted rows later. *Revision 3:* correcting it is a **public-contract decision** — `OD-CLOSE-09` | `DOMAIN_SHAPE_GAP` | — |
| 5 | Seat / bag / meal / lounge | S6 | `OrderService` carries `Type`, `DetailSchema`, `DetailSchemaVersion` and one nullable typed-detail navigation; `ServiceDetailSchemaRegistry` is the gate; **no seat field on `AirTransportDetail`** | **No.** Adding `SeatDetail` follows the identical pattern. Corroborated by AIDM M2 and by Navitaire N3. The one caution is the candidate-side string dictionary, which is maintainability, not shape | `STRUCTURALLY_READY` | — |
| 6 | Refund | S9 | `PricingLineRole.Reversal` and `PricingComponentType.Penalty` exist; allocations were removed by owner decision | **No.** `DOMAIN/03` warns "RefundBasis is NOT an implicit allocation purpose or entitlement", so the removal pre-commits nothing. Allocations return as new tables | `ADDITIVE_FUTURE_EXTENSION` | — |
| 7 | Exchange / reprice | S10 | typed fare-construction graph; `FareConstruction.SupersededByConstructionId` | **No for fare coupling.** The graph is exactly what an exchange needs. The status half belongs to family 4 | `STRUCTURALLY_READY` | — |
| 8 | Disruption | S11 | journeys and segments with external flight id and version; no operational state on the sold snapshot | **No.** `DOMAIN/04` §21 says a schedule change never edits the sold snapshot, and it cannot here | `STRUCTURALLY_READY` | `BLOCKED_OWNER_CONTRACT` (BD-010) |
| 9 | Delivery / consumption | S12 | `OrderService` is the correlation unit; observations are a separate table group | **No — but only if `Suspended` leaves the commercial axis.** AIDM M2 carries `Status Code` and `Delivery Status Code` as separate attributes, and `OrderServiceDeliveryStatus` already exists as the right home | `STRUCTURALLY_READY` once family 4 is corrected | — |
| 10 | Traveler correction | S13 | `SourceTravellerRef` / `ClientTravelerRef` / `TravelerIdentity` are three separate things, and the PII already lives in its own table behind its own authorized read | **No.** A name correction touches one of the three | `STRUCTURALLY_READY` | — |
| 11 | Split | S14 | `OrderItemServiceLink.ScopeAtAssociation` reconstructs an old item's contents without joining current ownership, exactly as `DOMAIN/02` §13 demands; `RootOrderId` exists | **Partly.** `OrderItemCommercialStatus` has no `Partitioned`, which is the state split produces, and there is no parent/lineage. Lineage is additive; the missing status is not | `DOMAIN_SHAPE_GAP` | lineage half is `ADDITIVE_FUTURE_EXTENSION` |
| 12 | Group / charter | S15 | group blocks are a separate table group materialising ordinary Orders | **No.** | `ADDITIVE_FUTURE_EXTENSION` | `BLOCKED_OWNER_CONTRACT` (BD-012) |
| 13 | Interline / partner settlement | later | `MarketingCarrierRef` and `OperatingCarrierRef` are on the segment — the identity interline settlement needs | **No, provided `SettlementAttribution` is generic.** *Revision 3 correction:* revision 2 classified this as a second `CURRENT_S1_GAP`. That was wrong — interline settlement behaviour is not an S1 requirement and the owner never kept it in S1. What it needs is that the S1 settlement decision produces a **generic** counterparty concept rather than a commission-shaped one, which is why `OD-CLOSE-01` now requires a non-commission `SettlementOnly` test. AIDM M2 associates Interline Settlement Information to Service and M3 associates Commission to it; both land on the same generic attribution later | `FUTURE_DOMAIN_DEPENDENCY` | `BLOCKED_OWNER_CONTRACT` (BD-012) |
| 14 | Agency settlement | S1 and later | `SettlementOnly` and `Commission` exist; the arithmetic excludes them from `CustomerTotal` | **Already wrong today.** `DOMAIN/03` §13 requires party/category whenever Effect is SettlementOnly; neither exists. The owner explicitly kept this half of SC-S1-013 in S1 | `CURRENT_S1_GAP` | — |
| 15 | Accepted sales provenance | S1 | `FinancialCustomerId`, `Channel`, `SellingOfficeId`, two actor fields | **Already wrong today.** `DOMAIN/01` §2 requires immutable `SalesContext` **and** `BuyerSnapshot` at accepted creation. Neither exists; `BuyerActor*` holds the initiating actor; `TravelAgencyId` survives only inside an idempotency scope string; `SellingOfficeId` mixes two office namespaces **and is published publicly as `airlineOfficeId`** | `CURRENT_S1_GAP` | — |

## Summary

Primary classifications are mutually exclusive and sum to 15.

| Primary classification | Families |
|---|---|
| `STRUCTURALLY_READY` | **5** — seat/ancillary (5), exchange graph (7), disruption (8), delivery (9), traveler correction (10) |
| `DOMAIN_SHAPE_GAP` | **5** — reservation snapshot (1), funding scope/disposition (2), document authority (3), cancellation vocabulary (4), split status (11) |
| `ADDITIVE_FUTURE_EXTENSION` | **2** — refund (6), group (12); plus the lineage half of split |
| `FUTURE_DOMAIN_DEPENDENCY` | **1** — interline/partner settlement (13) |
| `CURRENT_S1_GAP` | **2** — agency settlement (14), accepted sales provenance (15) |
| **Total** | **15** |

Secondary tag, counted separately and never added into the above: `BLOCKED_OWNER_CONTRACT` on families 1, 3, 8, 12, 13 — **5 value-side blocks** (BD-002/003, BD-006, BD-010, BD-012).

## What still holds

Three S1 decisions genuinely improved future readiness and are unchanged across all three revisions:

1. **Seat stayed out of `AirTransportDetail`** — corroborated by AIDM M2 and by the Pack's typed-details table, which gives Seat its own row.
2. **The fare-construction graph became typed** — S10 can query fare coupling instead of parsing a JSON column.
3. **`ScopeAtAssociation` was added to the item–service link** — S14 can reconstruct historical item contents without relying on current ownership.

## What earlier revisions got wrong here

**Revision 1** declared `FundingObligation` "fully sufficient for S3" without comparing it to the `DOMAIN/06` field list, and rated `FulfillmentProfileSnapshot` `BLOCKED_OWNER_CONTRACT` and stopped — conflating blocked *values* with an incomplete *shape*.

**Revision 2** corrected both but introduced three errors of its own, corrected above: it called interline settlement a second current S1 gap (family 13); it said the document-authority vocabulary was undefined when `DocumentAuthority` already exists in Contracts (family 3); and it recommended a polymorphic scope column that SQL Server cannot enforce (family 2).
