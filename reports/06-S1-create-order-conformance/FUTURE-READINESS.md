# Full-Domain Future Readiness

Stage: 06-S1-create-order-conformance · **revision 2**, reclassified after independent review · 2026-09-19 · HEAD `506ccee`

Revision 1 said "13 of 14 families are clean `DEFER_IMPLEMENTATION`". That verdict is **withdrawn**. It collapsed two different questions — *is the behavior deferred* and *is the domain shape correct* — into one answer, and answered both with the first.

Revision 2 uses five classes:

- **`STRUCTURALLY_READY`** — canonical identities exist, shape is Pack-conformant, only behavior is missing.
- **`ADDITIVE_FUTURE_EXTENSION`** — a later slice adds columns or tables; nothing existing must change.
- **`DOMAIN_SHAPE_GAP`** — the shape must be decided now or a later slice will have to migrate accepted data.
- **`BLOCKED_OWNER_CONTRACT`** — the values depend on an owner contract that does not exist.
- **`CURRENT_S1_GAP`** — not a future question at all; S1 is already wrong.

| # | Family | Slice | Canonical identities | Would an S1 decision force a destructive redesign? | Classification |
|---|---|---|---|---|---|
| 1 | Reservation / capacity | S2 | `OrderSegment.SourceCapacityRef` retains the AirOffer `FlightCapacityId` as a *source reference*; `FulfillmentProfileSnapshot.ReservationRequirement` exists and reads `Unresolved` | **Partly.** The identities are right, but `FulfillmentProfileSnapshot` is missing resource quantity/unit **policy** and partial-fulfillment support, which S2 is the first slice to need. Adding them later means rewriting accepted snapshots. | **`DOMAIN_SHAPE_GAP`** (shape) + `BLOCKED_OWNER_CONTRACT` (values, BD-002/003) |
| 2 | Payment / funding | S3 | `FundingObligation` has version, purpose, money, change and supersession | **Yes, for scope.** `DOMAIN/06` requires **Service / Item / PricingLine** scope and a **current disposition**; today only nullable `OrderItemId` exists. A fee-only `MonetaryCharge` needs line scope and an S6 added service needs service scope. Freezing item-only scope now is the destructive choice. | **`DOMAIN_SHAPE_GAP`** (revision 1 wrongly called this sufficient) |
| 3 | ETKT / EMD issuance | S4/S7 | `OrderService` is the unit a coupon attaches to; `FulfillmentDocumentKind` exists | **Partly.** The Pack's snapshot requires document **authority**, not just kind. Document tables themselves are a clean additive group. | **`DOMAIN_SHAPE_GAP`** (authority field) + `BLOCKED_OWNER_CONTRACT` (BD-006) |
| 4 | Cancellation | S5 | commercial status vocabularies exist | **Yes.** `OrderServiceCommercialStatus` lacks `Replaced` and `Expired` and carries `Exchanged`/`Suspended`, which the Pack does not define on this axis; `OrderItemCommercialStatus` lacks four derived states. Cancelling with the wrong vocabulary means migrating accepted rows later. | **`DOMAIN_SHAPE_GAP`** |
| 5 | Seat / bag / meal / lounge | S6 | `OrderService` carries `Type`, `DetailSchema`, `DetailSchemaVersion` and one nullable typed-detail navigation; `ServiceDetailSchemaRegistry` is the gate; **no seat field on `AirTransportDetail`** | **No.** Adding `SeatDetail` follows the identical pattern. Corroborated by AIDM M2 (Service associates a Delivery Provider) and N3 (Navitaire sells seat fees as a retail capability distinct from the flight). The one caution is the candidate-side string dictionary, which is maintainability, not shape. | **`STRUCTURALLY_READY`** |
| 6 | Refund | S9 | `PricingLineRole.Reversal` and `PricingComponentType.Penalty` exist; allocations were removed by owner decision | **No.** `DOMAIN/03` warns "RefundBasis is NOT an implicit allocation purpose or entitlement", so the removal pre-commits nothing. Allocations return as new tables. | **`ADDITIVE_FUTURE_EXTENSION`** |
| 7 | Exchange / reprice | S10 | typed fare-construction graph; `FareConstruction.SupersededByConstructionId` | **No for fare coupling — yes for status.** The graph is exactly what an exchange needs. But `Exchanged` currently sits on the commercial axis where the Pack says `Replaced` belongs. | **`STRUCTURALLY_READY`** for the graph; the status half is inside family 4 |
| 8 | Disruption | S11 | journeys and segments with external flight id and version; no operational state on the sold snapshot | **No.** `DOMAIN/04` §21 says a schedule change never edits the sold snapshot, and it cannot here. | **`STRUCTURALLY_READY`** + `BLOCKED_OWNER_CONTRACT` (BD-010) |
| 9 | Delivery / consumption | S12 | `OrderService` is the correlation unit; observations are a separate table group | **No — but only if `Suspended` leaves the commercial axis.** AIDM M2 carries `Status Code` and `Delivery Status Code` as separate attributes, and `OrderServiceDeliveryStatus` already exists as the right home. | **`STRUCTURALLY_READY`** once family 4 is corrected |
| 10 | Traveler correction | S13 | `SourceTravellerRef` / `ClientTravelerRef` / `TravelerIdentity` are three separate things, and the PII already lives in its own table behind its own authorized read | **No.** A name correction touches one of the three. | **`STRUCTURALLY_READY`** |
| 11 | Split | S14 | `OrderItemServiceLink.ScopeAtAssociation` reconstructs an old item's contents without joining current ownership, exactly as `DOMAIN/02` §13 demands; `RootOrderId` exists | **Partly.** `OrderItemCommercialStatus` has no `Partitioned`, which is the state split produces, and there is no parent/lineage. Lineage is additive; the missing status is not. | **`DOMAIN_SHAPE_GAP`** (status) + `ADDITIVE_FUTURE_EXTENSION` (lineage) |
| 12 | Group / charter | S15 | group blocks are a separate table group materialising ordinary Orders | **No.** | **`ADDITIVE_FUTURE_EXTENSION`** + `BLOCKED_OWNER_CONTRACT` (BD-012) |
| 13 | Interline / partner | later | `MarketingCarrierRef` and `OperatingCarrierRef` are on the segment — the identity interline settlement needs | **Yes, through the same hole as family 14.** AIDM M2 associates **Interline Settlement Information** to Service and M3 associates Commission to it; without a settlement counterparty concept there is nowhere to put either. | **`CURRENT_S1_GAP`** (shares the settlement attribution gap) + `BLOCKED_OWNER_CONTRACT` (BD-012) |
| 14 | Agency settlement | S1 and later | `SettlementOnly` and `Commission` exist; the arithmetic excludes them from `CustomerTotal` | **Already wrong today.** `DOMAIN/03` §13 requires party/category whenever Effect is SettlementOnly; neither exists. The owner kept this half of SC-S1-013 in S1. | **`CURRENT_S1_GAP`** |
| 15 | Accepted sales provenance | S1 | `FinancialCustomerId`, `Channel`, `SellingOfficeId`, two actor fields | **Already wrong today.** `DOMAIN/01` §2 requires immutable `SalesContext` and `BuyerSnapshot` at accepted creation. Neither exists; `BuyerActor*` holds the initiating actor; `TravelAgencyId` is resolved away and survives only inside an idempotency scope string; `SellingOfficeId` mixes two office namespaces. | **`CURRENT_S1_GAP`** + **`FIX_DOMAIN_DESIGN`** |

## Summary

| Classification | Families |
|---|---|
| `STRUCTURALLY_READY` | 5 — seat/ancillary, exchange graph, disruption, delivery, traveler correction |
| `ADDITIVE_FUTURE_EXTENSION` | 2 — refund, group; plus the lineage half of split |
| `DOMAIN_SHAPE_GAP` | 5 — reservation snapshot, funding scope/disposition, document authority, cancellation vocabulary, split status |
| `BLOCKED_OWNER_CONTRACT` | 5 value-side blocks (BD-002/003, BD-006, BD-010, BD-012) |
| `CURRENT_S1_GAP` | 3 — agency settlement, interline settlement, accepted sales provenance |

## What still holds from revision 1

Three S1 decisions genuinely improved future readiness and are unchanged:

1. **Seat stayed out of `AirTransportDetail`** — corroborated by AIDM M2 and by the Pack's typed-details table, which gives Seat its own row.
2. **The fare-construction graph became typed** — S10 can query fare coupling instead of parsing a JSON column.
3. **`ScopeAtAssociation` was added to the item–service link** — S14 can reconstruct historical item contents without relying on current ownership.

## What revision 1 got wrong here

It declared `FundingObligation` "fully sufficient for S3" without comparing it to the `DOMAIN/06` field list, and it rated `FulfillmentProfileSnapshot` `BLOCKED_OWNER_CONTRACT` and stopped — conflating blocked *values* with an incomplete *shape*. Both are corrected above.
