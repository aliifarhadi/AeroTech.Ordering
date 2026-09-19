# Full-Domain Future Readiness

Stage: 06-S1-create-order-conformance · 2026-09-19 · HEAD `e861711`

The question is **not** whether later behavior exists — it must not. It is whether an S1 decision already taken would force a destructive redesign when that behavior arrives.

For each family: do the canonical identities already exist; would S1 force a redesign; is the missing concept already specified in the Pack; does it need a new owner decision.

| # | Family | Slice | Canonical identities sufficient? | S1 forces destructive redesign? | Already specified in Pack? | Needs a new owner decision? | Verdict |
|---|---|---|---|---|---|---|---|
| 1 | **Reservation / capacity** | S2 | Yes. `OrderSegment.SourceCapacityRef` retains the AirOffer `FlightCapacityId` as a **source reference**, deliberately named so it is not mistaken for owned capacity truth. `FulfillmentProfileSnapshot.ReservationRequirement` exists and reads `Unresolved` for the live profile. | No | DOMAIN/05, S2 | No — BD-002/BD-003 already scope the gap | `DEFER_IMPLEMENTATION` |
| 2 | **Payment / funding** | S3 | Yes. `FundingObligation` already exists per item with `Version`, `Purpose`, `SourceDecisionRef` and `SupersededObligationId`. Applications and evidence are separate tables the Pack names. | No | DOMAIN/06, DOMAIN/13 | No — BD-005 scopes it | `DEFER_IMPLEMENTATION` |
| 3 | **ETKT / EMD issuance** | S4/S7 | Yes. `OrderService` is the unit a document coupon attaches to, and `FulfillmentDocumentKind` exists on the profile (currently `Unresolved`). Document tables are a separate DOMAIN/13 group. | No | DOMAIN/07 | No — BD-006 scopes it | `DEFER_IMPLEMENTATION` |
| 4 | **Cancellation** | S5 | Mostly. `OrderItemCommercialStatus` / `OrderServiceCommercialStatus` vocabularies exist. `OrderItem` lacks supersession/cancellation references, which DOMAIN/02 §7 names. | No — they are additive columns on an existing entity | DOMAIN/09, DOMAIN/02 §7 | No | `DEFER_IMPLEMENTATION` |
| 5 | **Seat / bag / meal / lounge** | S6 | Yes, and this is the important one. DOMAIN/02's typed-details table gives **Seat** its own row ("Exactly one traveler and related air service; seat product/characteristics, requested seat when sold by identifier"). `OrderService` already carries `Type`, `DetailSchema`, `DetailSchemaVersion` and one nullable typed detail navigation; adding `SeatDetail` follows the identical pattern. Crucially, S1 did **not** put a seat field on `AirTransportDetail`. | **No.** Had S1 added `SeatNumber` to `AirTransportDetail`, S6 would have had to migrate seat data out of the air product and reconcile two homes for the same fact. It did not. | DOMAIN/02, SLICES/S6 | No — BD-008 scopes supplier products | `DEFER_IMPLEMENTATION` |
| 6 | **Refund** | S9 | Partly. `PricingLineRole.Reversal` and `PricingComponentType.Penalty` exist in the vocabulary. `AllocationSet`/`Allocation` were removed by owner decision, and DOMAIN/03 warns "RefundBasis is NOT an implicit allocation purpose or entitlement", so the removal does not pre-commit refund semantics. | No — allocations return as new tables when a slice needs them | DOMAIN/09, DOMAIN/03 §39 | No | `DEFER_IMPLEMENTATION` |
| 7 | **Exchange / reprice** | S10 | Yes, and better than before this stage. The typed fare-construction graph is the thing an exchange needs to reason about fare coupling; under the old JSON blob it would have had to parse a string. `FareConstruction.SupersededByConstructionId` already exists. | No | DOMAIN/09, DOMAIN/03 §47 | No — BD-007 scopes AirPrice | `DEFER_IMPLEMENTATION` |
| 8 | **Disruption** | S11 | Yes. Journeys and segments are now first-class with external flight id and version, which is what a disruption case must match against. `OrderJourney`/`OrderSegment` carry no operational state, which is correct — DOMAIN/04 §21 says a schedule change never edits the sold snapshot. | No | DOMAIN/11 | No — BD-010 scopes it | `DEFER_IMPLEMENTATION` |
| 9 | **Delivery / consumption** | S12 | Yes. `OrderService` is the unit an observation correlates to; observation tables are a separate DOMAIN/13 group and sold truth stays untouched. | No | DOMAIN/11, DOMAIN/13 | No — BD-009 scopes it | `DEFER_IMPLEMENTATION` |
| 10 | **Traveler correction** | S13 | Yes. `OrderTraveler` separates `SourceTravellerRef` (owner identity) from `ClientTravelerRef` (caller identity) from `TravelerIdentity` (the PII), and the PII already lives in its own table read through a separate authorized path. A name correction touches one of the three without disturbing the others. | No | DOMAIN/04 | No | `DEFER_IMPLEMENTATION` |
| 11 | **Split** | S14 | Partly. `OrderItemServiceLink.ScopeAtAssociation` — added in the previous stage — is exactly the mechanism split needs to reconstruct an old item's contents without joining the service's *current* owner. `Order.RootOrderId` exists; parent/lineage does not. | No — `OrderLineage` is additive | DOMAIN/12, DOMAIN/02 §13 | No — BD-012 scopes it | `DEFER_IMPLEMENTATION` |
| 12 | **Group / charter** | S15 | Yes. Group blocks are a separate DOMAIN/13 table group that materialises ordinary Orders; nothing in S1 obstructs it. | No | DOMAIN/12 | No — BD-012 | `DEFER_IMPLEMENTATION` |
| 13 | **Interline / partner** | later | Partly. `OrderSegment.MarketingCarrierRef` and `.OperatingCarrierRef` are now on the segment, which is the identity an interline settlement needs. No partner settlement concept exists. | No | DOMAIN/12, BD-012 | Yes, eventually — the partner settlement counterparty is the same gap as agency settlement (see 14) | `BLOCKED_OWNER_CONTRACT` |
| 14 | **Agency settlement** | S1 (partly) and later | **No — this is the one gap.** `PricingEffect.SettlementOnly` and `PricingComponentType.Commission` exist and the arithmetic excludes them from `CustomerTotal`, but `SettlementPartyRef` and `SettlementCategory` do not exist anywhere. DOMAIN/03 §13 requires them whenever Effect is SettlementOnly, and IATA's Settlement with Orders frames settlement as an airline↔seller exchange that needs a named counterparty. | **Not destructive, but it is already due.** The owner kept the settlement half of SC-S1-013 in S1, so this is a current gap rather than a future one. | DOMAIN/03 §13 | No — the Pack already states the requirement; only implementation is missing | **`FIX_S1_SEMANTICS`** |

## Summary

Thirteen of fourteen families are clean `DEFER_IMPLEMENTATION`: the canonical identities exist, the Pack already specifies the missing pieces, and no S1 decision would force a destructive redesign.

Three S1 decisions actively **improved** future readiness and are worth naming:

1. **Seat stayed out of `AirTransportDetail`.** The single most common way to corrupt this model was avoided.
2. **The fare-construction graph became typed.** S10 exchange work can now query fare coupling instead of parsing a JSON column.
3. **`ScopeAtAssociation` was added to the item–service link.** S14 split can reconstruct historical item contents without reading current ownership — which DOMAIN/02 §13 explicitly forbids relying on.

One family — agency (and by extension partner) settlement — is not a future-readiness question at all. It is a present S1 gap.
