# Aggregate map, ownership and lifecycle boundaries

## 1. Vocabulary and module map

`Commercial` owns Order, accepted commercial changes and monetary history. `Fulfillment` owns the local binding to capacity and document roots, not external capacity. `Operations` owns durable orchestration evidence and coordination. `Delivery` owns ingested observations and local derived operational facets. `Groups` owns group organization and block references. These are modules within the one Ordering deployment and database, not compulsory microservices.

| Root / record | Owns | Does not own | Atomic boundary |
|---|---|---|---|
| Order | Current commercial composition, traveler ownership, accepted snapshots, change acceptance, money history, summary/revisions | Provider resource lifecycles, customer wallet, flight operations, accounting journal | One Order commercial commit plus explicitly enlisted local roots |
| OrderPreparation | Immutable candidate captured before acceptance, source/validity/actor binding and one-time consumption | Inventory reservation, source offer validity guarantee, payment | Local preparation capture; consumption atomic with Create |
| FulfillmentReservation | Local resource binding, source revisions and per-service evidence/coupling | Available seats, canonical held/committed capacity | One evidence application under owning Order coordination |
| ElectronicTicket | Issued document facts, coupon financial/control aspects, service association history | Entire sale price model or DCS protocol | Local authority: same transaction as issue/servicing pivot; external: evidence application |
| ElectronicMiscDocument | EMD type/purpose, RFIC/RFISC, coupon links/value references | Wallet or automatic collection/refund authority | Same local document rules, separate root/invariants |
| DocumentStock | Issuer namespace/ranges, atomic allocation history | Random ticket generation or external owner's number pool | Serialized namespace/allocation transaction |
| ServicingOperation | Durable typed plan, affected scope, phase/outcome, relation to receipt/change/effects | A universal business Order status | Each local phase transaction |
| GroupBooking | Unnamed/name-slot organization, confirmed block references, materialization results | FlightFlow inventory or JetPay deposit balance | Group-local commit; bounded Group+new Order materialization pivot |

CommandReceipt, OperationOrderClaim, ExternalOperation, ProviderEvidence, PricingLine, PriceChangeSet, PricingAllocationSet, AirFareConstruction, OrderChange and DeliveryObservation are durable records/owned entities. They are NOT all separate DDD aggregate roots. Do not introduce a generic repository and CRUD controller for every table.

## 2. Order fields

Required at accepted creation: `OrderId`, `OrderReference`, `RootOrderId` (self), `OwnerAirlineId`, `FinancialCustomerId`, immutable `SalesContext`, `BuyerSnapshot`, sale currency reference, `CreatedAt`, accepted-source links, at least one item/service or explicitly supported fee-only commercial scope, current travelers where required, initial commercial change and monetary set when priced.

Optional lineage: ParentOrderId, SplitTransferId, RelatedOrder links. Explicit closure: ClosedAt, CloseReason. Derived-but-persisted: CommercialSummary, CustomerTotal and complete current component totals. Revisions: CommercialVersion, FinancialSequence, OrderRevision, SQL RowVersion; definitions are in DOMAIN/10.

OrderReference is a customer-facing reference, not a document number, provider PNR or database ID. Generate it using a deployment-approved reference namespace and unique DB constraint; retries reuse it. Reference format is a configuration/representation decision, not an airline ticket algorithm. A provider locator is stored in ExternalReference with provider namespace and scope.

## 3. Multiplicities

```text
Order 1 -> many OrderItem
Order 1 -> many current Traveler; historical ownership may move by Split
Order 1 -> many Journey -> many JourneySegment
OrderItem 1 -> one-or-many current OrderService (except explicit fee-only item)
OrderService -> exactly one current Order and current OrderItem
OrderService -> one-or-many beneficiaries when the type requires persons
OrderService -> typed coverage (zero-or-many segments for non-air; exactly one for air)
OrderItem <-> OrderService historical membership through immutable links
OrderChange 1 -> zero-or-one PriceChangeSet; set 1 -> zero-or-many PricingLine
PricingLine 1 -> zero-or-many versioned AllocationSet; set 1 -> rows
AirFareConstruction -> explicit set of item/service/traveler scope; may cross items
FulfillmentReservation -> one-or-many member service/resource bindings
ElectronicTicket -> one traveler; one-or-many coupons by certified document profile
EMD -> one-or-many coupons; purpose determines service/price/value link
ServicingOperation -> one receipt; zero-or-one final commercial change per participating Order
ServicingOperation -> many independently idempotent ExternalOperation steps
```

Fee-only EMD-S is linked to money, not a fabricated service. The `OrderItem` fee-only exception is explicit `ItemKind=MonetaryCharge`; it cannot masquerade as an air obligation. A service cannot be current in two items. Old membership remains queryable after replacement or split.

## 4. Identity and lifetime

Use the existing generic long ID generator consistently, transmitted as decimal strings where JSON clients could lose precision. All IDs are immutable and nonzero. External opaque IDs remain strings with owner namespace; never parse an opaque offer ID into local business truth. Reuse generic framework ID/clock abstractions, not a second project-wide primitive framework.

A change in commercial fulfillment identity (flight, beneficiary or independently sold product scope) normally creates successor services with lineage. Price-only item replacement reuses unchanged service IDs and appends new membership. Current service ownership can move through split without changing ServiceId. History retains OrderIdAtOccurrence/Association.

## 5. Behavior surfaces, not setters

Order exposes behaviors such as AcceptOriginalSale, AcceptAddedItems, ApplyAcceptedChange, CancelScope, CorrectTraveler, ApplySplitTransfer and Close. These validate a typed accepted decision and complete affected scope. There is no public SetStatus/SetTotal/SetPaid. Document roots expose Issue/RecordExternalIssue, Void, ReserveForServicing, Exchange, RecordRefund, TransferControl, Revalidate and RecordAuthoritativeConsumption as allowed by profile.

Mutating a root does not call a provider or SaveChanges. The application obtains evidence and opens an explicit local session; domain behavior accepts validated values and returns domain facts. CommandReceipt and an active claim are checked by the application before invoking a new mutation. Failed domain validation creates no partial business graph.

## 6. Bounded loading

Ordinary orders may start with full CURRENT graph loading. Historical facts use indexed references and pagination. Whole-order totals are derived from all applicable committed line rows, not the subset requested for servicing. Large group capacity remains outside Order. Add bulk/performance optimizations only when these completeness requirements remain testable.
