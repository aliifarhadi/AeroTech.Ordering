# Commercial composition, services and immutable lineage

## OrderItem is the sold-pricing boundary

An accepted source defines which services were priced as one package. Preserve that boundary, even when it covers several travelers/segments or contains flight and included benefits. Do not split one through-fare into independent priced items because the provider returns coupon display allocations. Conversely, do not merge independent items merely because they share currency or a traveler.

Fields: OrderItemId, OrderId, ItemKind, SourceOffer/Item/OwnerRef where supplied, ProductSnapshot, CommercialTermsSnapshot, AcceptedPriceSnapshot, CommercialSource, CreatedByChangeId, CurrentStatus, supersession/cancellation/partition references. AcceptedPriceSnapshot is the original item version, not current balance. Price changes append a change set and, where terms/product boundary changes, a successor item.

ProductSnapshot: source product ID/code/type/name, brand/family/version, supplier product reference, immutable validated attributes. CommercialTermsSnapshot: original pricing/sale date, policy references/version, source calculation context, display refund/change/no-show/upgrade summaries, validity and protected raw payload reference where needed. A display `IsRefundable` flag is not a runnable refund algorithm.

## Current and historical membership

Store immutable `OrderItemServiceLink(LinkId, OrderIdAtAssociation, OrderItemId, OrderServiceId, ScopeAtAssociation, LinkedByChangeId)`. Current service owns exactly one OrderId/OrderItemId. New membership supersedes a current binding; old links remain history. Do not rebuild an old item's sold contents by joining only the service's current owner.

Many-to-many lineage uses `ServiceLineage(ChangeId, PredecessorServiceId, SuccessorServiceId, RelationType)` and `ItemLineage`. One old direct service can be replaced by two connection services; two old services can become one direct successor. Single predecessor/successor convenience fields cannot replace this relation.

## OrderService base contract

Fields: ServiceId, current OrderId/ItemId, ServiceType, ServiceCode/Name, CommercialStatus, ServiceVersion, beneficiary IDs, SupplierPartyRef, DeliveryProviderRef, FulfillmentProfileSnapshot, PriceTreatment, current coverage/detail link, CreatedByChangeId and immutable transition history.

PriceTreatment: SeparatelyPriced, Included, Complimentary, SupplierOpaque. Included/complimentary does not imply a synthetic zero fare line. A service has independent identity only when it needs independent servicing, supplier execution or delivery tracking; untracked product characteristics stay attributes.

FulfillmentProfileSnapshot: profile ID/version, reservation requirement, resource quantity/unit policy, document requirement/type/authority, funding requirement, delivery provider/control policy, dependency treatment and whether partial fulfillment is supported. This snapshot cannot change just because the catalog's latest default changed.

## Typed details and coverage

| Type | Cardinality and required detail | Change/delivery rule |
|---|---|---|
| AirTransport | Exactly one traveler, one ScheduledAir/OpenAir passenger segment; accepted cabin/RBD refs; explicit seat/resource requirement | Flight replacement creates successor; OpenAir binding only through explicit revalidation/change; legs do not multiply coupons |
| Seat | Exactly one traveler and related air service; seat product/characteristics, requested seat when sold by identifier | DCS reassignment is a separate observation; commercial seat-product downgrade is a servicing decision |
| Baggage | Kind, allowance basis, positive sold pieces/weight with units as relevant; coverage portions; shared pool policy | Pieces and per-piece weight cap can coexist; delivery consumption does not shrink sold quantity |
| Meal | Meal code, positive integral quantity, beneficiary and flight coverage when required | Partial delivered quantities are operational evidence |
| Lounge | Airport/location, access window when restricted, named beneficiary/guest allowance | Need not invent a flight segment |
| Hotel | Supplier/property/room/rate plan, CheckOut > CheckIn, rooms and full guest set | One service per independently cancellable supplier room-stay; nightly prices are not nightly services |
| GroundTransport | Pickup/dropoff and time/window, vehicle/passenger count and explicit shared beneficiaries | Vehicle price is not multiplied by passenger count |
| Registered extension | SchemaName + SchemaVersion + validated required attributes/profile | Unknown schema rejected; no opaque unvalidated JSON for core air/seat/baggage |

Assistance, UMNR, PETC/AVIH, wheelchair, WiFi, insurance, CIP and SIM can use registered schemas until dedicated invariants/queries justify a typed table. Sensitive guardian/assistance/medical-adjacent details use protected payload references, not unrestricted JSON in OrderDetails.

## Dependencies

`ServiceDependency(ServiceId, RelatedServiceId, Kind, OnChangePolicyRef)` permits RequiresAirService, CoverageMember and BundledWith. RequiresAirService is acyclic. Flight servicing evaluates every affected dependent as Keep, Replace, Cancel or ManualReview according to an explicit accepted/owner decision. A paid seat cannot remain active against a replaced air service without a supported reassociation.

A shared service belongs to all explicit beneficiaries, not just a primary traveler. Split moves it only when all beneficiaries move or an authoritative priced/supplier partition exists. Otherwise reject SharedServiceCannotBePartitioned. Never duplicate a room, car, bag pool or payment application to make both Orders look complete.

## Commercial lifecycle

Service states: Pending (accepted but contractually awaiting commercial activation), Active, Cancelled, Replaced, Expired. Accepted creation defaults Active in the reference profile; Active does not mean reserved, funded or issued. Pending is not used to encode provider timeout.

Allowed commercial transitions: Pending -> Active/Cancelled/Expired; Active -> Cancelled/Replaced/Expired through a named accepted operation. Cancelled/Replaced/Expired are terminal for that identity. A correction appends a new authorized change/successor, not arbitrary reactivation. Expire is allowed only for a source-backed unsatisfied commercial deadline and no protecting uncertain irreversible step.

Item status is derived from current membership plus its own historical role: all active -> Active; mixed active and terminal -> PartiallyChanged; all cancelled -> Cancelled; explicit predecessor -> Replaced; split predecessor -> Partitioned; all expired -> Expired; other fully inactive mix -> Inactive. MonetaryCharge items use their charge disposition, not nonexistent service rows. Order summary is Active when any current accepted active item exists; Cancelled only when all relevant accepted scope is commercially cancelled; otherwise Inactive; Closed overrides only by an explicit closure fact. Fulfillment/delivery facets remain separate.
