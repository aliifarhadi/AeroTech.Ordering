# Deleted Fields and Types

Stage: 07-S1-domain-simplification · 2026-09-19 · branch `k8s-stg`

Every entry states what was removed and why it was unnecessary or wrong. Nothing here was removed because it was
merely unused today; each row failed at least one of the four admission tests in `FIELD-INVENTORY.md`.

## 1. The generic validity machinery

| Removed | Why |
|---|---|
| `ValidityFact(State, Value, Owner, SourceRef, Reason)` | A five-member abstraction over three facts. For the live AirOffer ticketing case it recorded `NotSupplied`, owner `"Unresolved owner"`, a null `SourceRef` and a prose reason — producing the column `TicketingValiditySourceRef`, which never held a value. |
| `ObservedTimeFact(Value, SourceOwner, SourceRef)` | A wrapper whose two extra members were mapper constants; the fact itself is `LastTicketingDate`. |
| `Order.OfferValidity` / `PriceValidity` / `TicketingValidity` | The committed Order never read them; `EnsureAcceptable` reads the preparation's copy. |
| `Order.ObservedTicketingDeadline` | Duplicated `LastTicketingDate` under a wrapper. |
| `OrderPreparation.TicketingValidity`, `ValidityFacts` | Ticketing state was always `NotSupplied`; the derived list had no consumer. |
| `DeadlinePolicy` (+ its test) | Existed only to operate on `ValidityFact`; no production consumer. |

**Replaced by** three explicit nullable instants: `OfferExpiresAt?`, `PriceValidUntil?`, `LastTicketingDate?`. A missing
source fact is `null`. Blocking reasons belong to acceptance policy and reporting, not to commercial columns.

## 2. Invented source identities

| Removed | Was | Why |
|---|---|---|
| `OrderItem.SourceItemRef` | `"OFFER-PACKAGE"` | A mapper constant, not an owner identity. |
| `OrderSegment.SourceSegmentRef` | `"{boundId}\|{flightId}"` | A local correlation key. Survives only as the candidate-only `SegmentKey`. |
| `OrderService.SourceServiceRef` | `"{travellerRef}\|{segmentRef}"` | Same. Survives only as the candidate-only `ServiceKey`. |
| `FarePricingUnit.SourceUnitRef` | `"pricingUnits/{i}"` | An array index. Replaced by the unit's own `Sequence`. |
| `FareConstruction.SourceContextRef` | `"airoffer:details:pricingUnits"` | A constant path string. |
| `OrderChange.SourceDecisionRef`, `PriceChangeSet.SourceDecisionRef`, `FundingObligation.SourceDecisionRef` | `"preparation:{id}:{digest}"` | A local preparation reference dressed as an external decision identity. The link already exists through `Order.SourcePreparationId`. |
| `PricingLine.CandidateLineRef` | duplicate of `SourceLineRef` | Two columns holding the identical occurrence path. |
| `PricingLine.SourceBasisRef` | candidate key | Resolved into `BasisId` at acceptance; the unresolved copy had no reader. |

**Retained:** exactly one honestly named `PricingLine.SourceOccurrencePath` (for example
`tickets/0/coupons/0/pricings/1`), because pricing audit genuinely needs the exact source occurrence.
`OrderJourney.BoundId` (`"B1"`), `SourceTravellerRef`, `SourceConversionRef` and `CoveredBoundOfferId` stay strings
because the owner really does supply opaque strings there.

## 3. Stringified numeric identities

`CurrencyRef`, `OriginRef`, `DestinationRef`, `OriginTerminalRef`, `DestinationTerminalRef`, `FlightRef`,
`FlightVersion`, `MarketingCarrierRef`, `OperatingCarrierRef`, `SourceCapacityRef`, `AircraftRef`, `CabinRef`,
`RbdRef`, `SourceLegRef`, `SourceFareRef`, `AppliedConversion.From/ToCurrencyRef`, `CurrencySnapshot`.

All of these are numeric in the AirOffer wire contract (`int`/`long`) and were converted to strings by the mapper.
They are now `CurrencyId`, `OriginAirportId`, `DestinationAirportId`, `…TerminalId`, `FlightId`, `FlightVersion`,
`MarketingAirlineId`, `OperatingAirlineId`, `FlightCapacityId`, `AircraftId`, `CabinClassId`, `RbdId`, `LegId`,
`AirFareId`, `FromCurrencyId`/`ToCurrencyId`. `CurrencyCode` ("IRR") is ReferenceData enrichment and is not a
commercial column.

## 4. The generic service framework

| Removed | Why |
|---|---|
| `ServiceDetailSchemaRegistry`, `DetailSchema`, `DetailSchemaVersion`, dynamic `Details` dictionary | A registry with one entry and a string dictionary carrying three typed concepts (cabin, RBD, booking class), which are now typed members. |
| `OrderService.ServiceCode`, `Name`, `PriceTreatment`, `SupplierPartyRef`, `DeliveryProviderRef`, `ServiceVersion`, `Quantity`, `QuantityUnit` | Constants or never supplied by AirOffer, and nothing mutates a service in S1. **Revision 2:** `ServiceType` is back — a service declares its type, it is the EF discriminator for `OrderAirTransportService`, and it reaches the projection and the public DTO. |
| the invented fulfillment profile id `AIROFFER-OBSERVED-AIR-UNCERTIFIED` and its version constant | A mapper constant presented as an owner profile. **Revision 2:** `FulfillmentProfileSnapshot` and `CandidateFulfillmentProfile` are back, with `ProfileRef`/`ProfileVersion` nullable and null — deleting the whole sale-time fulfillment context to remove the fake id also removed a Pack-required fact. A profile that claims `Certified` without naming itself is now refused. |
| `OrderServiceBeneficiary`, `OrderServiceCoverage`, `SoleCoveredSegmentId` | Collections that always held exactly one row, plus a derived workaround for that fact. |

### Missing beneficiary — how the scenario is still covered

> Generic beneficiary collection and the corresponding "missing beneficiary after construction" state were removed.
> For AirTransport, exactly one traveller and one passenger segment are now structural invariants. The original Pack
> scenario remains covered at the input boundary and construction invariant level.

Concretely, the scenario stays **TESTED** in the scenario matrix and is proven at four levels instead of one runtime
validation:

| Level | Evidence | Test |
|---|---|---|
| Provider / ACL | a ticket with a blank, missing or repeated `travellerRef` fails closed before any candidate exists | `A_ticket_without_a_usable_traveller_reference_fails_closed_before_any_candidate` |
| Canonical reader | a service object that omits `travellerRef` is rejected by the schema-1 reader | `A_candidate_service_that_names_no_traveller_cannot_be_read` |
| Candidate validation | a service naming a traveller that is not in the candidate is rejected | `Pack_missing_beneficiary_is_rejected_at_the_candidate_boundary` |
| Domain construction | `OrderService` cannot be constructed with a non-positive `TravellerId` or `SegmentId` (error 20293) | `An_accepted_service_is_bound_to_exactly_one_traveller_and_one_segment` |

The previous runtime-validation test was removed because the invalid state it asserted can no longer be constructed.

## 5. Item metadata the owner does not supply

| Removed | Why |
|---|---|
| `ProductSnapshot` (8 members) on `OrderItem` | Only `SourceSystem` and `SourceOfferId` were populated, and both duplicate the accepted source. Real product/brand facts return when an owner supplies them. |
| `OrderItem.SourceOfferItemRef` | AirOffer supplies no offer-item identity; always null. |
| `CommercialTermsSnapshot` (7 members) on `OrderItem` | A derived summary of the per-service sold-term flags, which are retained. |

## 6. Placeholder fare-construction structure

| Removed | Why |
|---|---|
| `FarePricingGroup`, `FarePricingGroupTraveler` | AirOffer supplies no pricing group; always empty. |
| `FareComponentService`, `FareComponentSegment` | AirOffer supplies no component↔service/segment binding; always empty. |
| `FareComponent.FareOwnerRef`, `TariffRef`, `RuleRef`, `RoutingRef` | Never supplied; always null. |
| `FarePricingUnit.SourceKindRaw`, `CombinationMethod` | The raw kind is a known vocabulary (`"OneWay"`) now mapped to `FarePricingUnitType`; combination method is never supplied. |
| `FareConstruction.Assurance`, `SupersededByConstructionId` | `Assurance` only gated component-coverage claims, which no longer exist; supersession belongs to repricing. |

`CoveredBoundOfferIds` is kept under the owner's exact name and stays opaque — see the `BLOCKED_REAL_CONTRACT` note in
`FIELD-INVENTORY.md`: the recorded live value is a `flightId`, not a `boundId`.

## 7. Lifecycle that cannot occur

| Removed | Why |
|---|---|
| `OrderPreparation.ConsumedByOrderId`, `ConsumedAt`, `IsConsumed`, `Consume()`, the consumption index and CHECK, `CommitConflictKind.PreparationConsumption` translation | `CreateOrderFromOfferService` captures, accepts and consumes inside one `SaveChangesAsync`; a preparation can never commit unconsumed. The only thing exercising `Consume()` was a test calling it twice on an in-memory object. Concurrency is protected by the idempotency receipt. |
| `OrderPreparation.Channel`, `SellingOfficeId`, `ActorContextType`, `ActorId`, `ClientReference`, `CreatedAt` | Duplicated from the candidate, the scope or the Order with no index or behaviour needing the copy. `CallerScope` is kept because it is one of the inputs to the snapshot digest, which binds the accepted candidate to the scope that accepted it; it is not indexed. |
| `CommandReceipt.Status`, `CompletedAt`, `PreparationId`, `OperationId` | Only `Completed` is constructible; `CompletedAt` always equalled `CreatedAt`; the preparation is reachable through `Order.SourcePreparationId`; no current behaviour reads an operation id. |
| `CommandReceiptStatus` (shared Contracts) | Internal receipt vocabulary that did not belong in the shared public contract. |

## 8. Order root

| Removed | Why |
|---|---|
| `AcceptedSource` (14-member value object) | A mega-wrapper duplicating the preparation row. Reduced to `SourceOfferId`, `SourcePreparationId` and `AcceptedSnapshotDigest`, which are what anything actually reads. |
| `AcceptedSource.PreparationId` | Duplicate of `Order.SourcePreparationId`. |
| `Order.SourceJourneyTypeRaw` | Dual modelling; `"RoundTrip"` maps to the existing `JourneyType` enum and an unknown value now fails closed. |
| — | **Revision 2:** `Order.Buyer` / `BuyerSnapshot` are back. Every current surface records `NotSupplied`, but Pack DOMAIN/01 and DOMAIN/04 keep Buyer distinct from FinancialCustomer, Seller, Actor and Traveller, and absence is itself the accepted fact. |
| `Order.IsSandboxScoped` | A convenience over the removed `AcceptedSource`. |
| `OrderComponentTotal.Id` (the fabricated surrogate the previous migration generated with `ROW_NUMBER`) and `CurrencyRef:string` | **Revision 2:** the table itself is back — Pack DOMAIN/01 defines complete component totals as derived *and persisted*. Only the fabricated identity went: the key is now `(OrderId, Component, Effect)` and the currency is `CurrencyId:int`. |
| `FundingObligation.SourceDecisionRef`, `SupersededObligationId` | A local preparation reference dressed as an external decision identity, and a lineage field no S1 flow writes. **Revision 2:** `OrderServiceId`, `PricingLineId` and a typed `FundingObligationScope` are back — item-only scope silently dropped the liability of a customer-balance line with no item. The source pricing decision is now the typed `PriceChangeSetId`. |
| `PricingLine.OriginalPricingLineId` | Always null; reversals are an explicitly deferred slice. **Revision 2:** `Role` is back — Pack DOMAIN/03 makes `LineRole` a first-class monetary semantic, and every S1 row being `Original` does not make it redundant. |
| `PriceChangeSet.BaseCommercialVersion` | Constant `0`, no consumer. |
| `OrderItemServiceLinkTravelers`, `OrderItemServiceLinkSegments` | Overbuilt children; the typed air service already owns traveller and segment scope, and `ScopeAtAssociation` is an S14 need. **Revision 2:** the link itself is back — Pack DOMAIN/02 requires immutable historical item↔service membership. |

## 9. Legacy compatibility (removed under the proposed rebaseline, which is not yet authorized)

| Removed | Why |
|---|---|
| `LegacySellingOfficePolicy` | Migration/read compatibility living in Domain vocabulary. |
| `Query/.../Projection/Compatibility/*` (schema 2 and 3 documents and reader) | Compatibility for unapproved S1 development rows only. |
| `NormalizedCandidate` schema `3.0` reader/writer and `LegacySchemaVersion` | Same. Candidate schema restarts at `1.0`. |
| `OrderDtoJson` schema-2 path | Same. Projection schema restarts at `1`. |
| `PackExamples.cs` (~900 lines of schema-3 candidate JSON) | Kept only to feed four fixtures. Each business intent now has an explicit replacement test built with the current builder: one-way reference, incorrect total, settlement tax, and missing beneficiary (see §4). A 900-line schema-3 artifact is exactly what this cleanup exists to remove. |

**Revision 2:** `SellingOfficeKind.NotRecorded` is now **removed** from the shared enum, together with the two
validation branches that existed only to reject it. With no deployed S1 schema there is no writer that can produce it,
and an office identifier whose namespace cannot be proven fails closed instead of being recorded as unknown.

## Counts

| | |
|---|---|
| Files deleted across `src/`, `tests/`, `Contracts/` | 50 |
| Domain `.cs` files | 107 → 91 |
| Mapped tables in `OrderingDbContext` | 34 → 25 |
| Mapped properties in the model snapshot | 475 → 322 |
| Persisted columns removed or collapsed | see `BEFORE-AFTER-MODEL.md` |
