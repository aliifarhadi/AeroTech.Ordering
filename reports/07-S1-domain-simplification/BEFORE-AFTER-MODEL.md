# Before / After Model

Stage: 07-S1-domain-simplification (revision 2) · 2026-09-20 · branch `k8s-stg` · reviewed baseline `0f103a3`,
R2 review HEAD `3e43d37`

All counts are measured, not estimated. Table and property counts come from `Migrations/OrderingDbContextModelSnapshot.cs`
at `0f103a3` and in the working tree; file counts come from `git diff --name-status 0f103a3 -- src tests Contracts`.

## 1. Totals

| | Before (`0f103a3`) | After (R2 corrections) |
|---|---|---|
| Mapped tables in `OrderingDbContext` | 34 | 25 |
| Mapped properties in the model snapshot | 475 | 322 |
| `.cs` files in `AeroTech.Ordering.Domain` | 107 | 91 |
| Files deleted across `src/`, `tests/`, `Contracts/` | — | 50 |
| Files added | — | 7 (+5 renamed) |
| Files modified | — | 102 |

## 2. Tables removed

| Table | Why it is gone |
|---|---|
| `OrderServiceBeneficiaries`, `OrderServiceCoverage` | Always exactly one row each; now the structural invariant `TravellerId` + `SegmentId` on `OrderAirTransportService`. |
| `OrderItemServiceLinkTravelers`, `OrderItemServiceLinkSegments` | Overbuilt children of the link; the typed air service already owns traveller and segment scope. The link itself is **kept** (§3). |
| `FarePricingGroups`, `FarePricingGroupTravelers` | AirOffer supplies no pricing group; always empty. |
| `FareComponentServices`, `FareComponentSegments` | AirOffer supplies no component↔service/segment binding; always empty. |
| `AirTransportServiceDetails` | The one registered detail schema; its three real facts are typed columns on `OrderServices`. |

`OrderTravelers` / `OrderTravelerIdentities` were renamed to `OrderTravellers` / `OrderTravellerIdentities`; the Query
read models and `OrderDtoReader` were corrected to the same spelling, which the full persistence run proved had been
broken.

## 3. Tables kept or restored after the R2 review

The first revision of this stage deleted five things the Pack requires. They are back, in their smallest honest form:

| Table | Shape now | Why it had to come back |
|---|---|---|
| `OrderComponentTotals` | composite key `(OrderId, Component, Effect)`, `DebitAmount`, `CreditAmount`, `CurrencyId` — no surrogate id, no `ROW_NUMBER` | Pack DOMAIN/01 defines complete component totals as derived **and persisted**. Recomputing them only in the projector was a second arithmetic implementation on the read side. |
| `OrderItemServiceLinks` | `Id`, `OrderIdAtAssociation`, `OrderItemId`, `OrderServiceId`, `LinkedByChangeId` — and nothing else | Pack DOMAIN/02 requires immutable historical item↔service membership. |
| `OrderServices` | `abstract OrderService` + `OrderAirTransportService`, TPH on `ServiceType` | The single sealed entity was an air service in disguise; the Pack and the historical model both split the base from the typed detail. |
| `FundingObligations` | `OrderItemId?` / `OrderServiceId?` / `PricingLineId?` with real FKs and an exactly-one CHECK, plus a typed `PriceChangeSetId` | Item-only scope silently dropped the liability of any customer-balance line without an item. |
| `Orders.Buyer*` | `BuyerContextType`, `BuyerId`, both null = `NotSupplied` | Pack DOMAIN/01 and DOMAIN/04 keep Buyer distinct from FinancialCustomer, Seller, Actor and Traveller. Absence is itself the accepted fact. |

## 4. Tables that shrank

| Table | Before | After | What left |
|---|---|---|---|
| `Orders` | 73 | 37 | `AcceptedSource` (14), three `ValidityFact`s, `ObservedTimeFact`, `CurrencySnapshot`, `SourceJourneyTypeRaw`, `IsSandboxScoped` (`Buyer` and `SaleCurrencyCode` are back) |
| `OrderPreparations` | 46 | 22 | consumption lifecycle, validity facts, duplicated channel/office/actor/client-reference columns |
| `OrderServices` | 36 | 33 | dynamic detail dictionary, schema registry columns, supplier/delivery refs, `Quantity`/`QuantityUnit`, `PriceTreatment` (`ServiceType` and the fulfillment snapshot are back) |
| `OrderItems` | 29 | 10 | `ProductSnapshot` (8), `CommercialTermsSnapshot` (7), `SourceItemRef`, `SourceOfferItemRef` |
| `PricingLines` | 37 | 34 | `CandidateLineRef`, `SourceBasisRef`, `OriginalPricingLineId`; the `Source*` prefixes dropped from `Code`/`Name`/`Reference` (`Role` is back) |
| `FareComponents` | 17 | 13 | `FareOwnerRef`, `TariffRef`, `RuleRef`, `RoutingRef`; `SourceFareRef:string` → `AirFareId:long` |
| `FarePricingUnits` | 10 | 7 | `SourceUnitRef`, `SourceKindRaw`, `CombinationMethod`, `PricingGroupId` (`SourceConstructionType` is new, and typed) |
| `FarePricingUnitCoveredBounds` | 5 | 2 | reduced to `PricingUnitId` + `CoveredBoundOfferId` |
| `FareConstructions` | 8 | 6 | `SourceContextRef`, `SupersededByConstructionId` (`Assurance` is back) |
| `FareConstructionItems` | 5 | 2 | reduced to the two keys — now filled from the source's own item keys, not from every order item |
| `FundingObligations` | 15 | 19 | `SourceDecisionRef` and `SupersededObligationId` left; `PriceChangeSetId` and real scope FKs arrived |
| `CommandReceipts` | 18 | 14 | `Status`, `CompletedAt`, `PreparationId`, `OperationId` |
| `OrderChanges` | 10 | 9 | `SourceDecisionRef` |
| `PriceChangeSets` | 10 | 8 | `SourceDecisionRef`, `BaseCommercialVersion` |
| `OrderJourneys` | 10 | 9 | `SourceDirectionRaw`; `SourceBoundRef` → `BoundId` |
| `OrderSegments` | 22 | 21 | every `*Ref:string` became its native numeric identity |
| `OrderComponentTotals` | 9 | 6 | the fabricated surrogate `Id`, `CurrencyRef:string` and `LastUpdateTime` bookkeeping |

## 5. Shape changes that matter more than the counts

**Money carries a currency identity.** `Money(decimal, int currencyId)`; `CurrencySnapshot` and every
`*CurrencyRef:string` are gone. `AppliedConversion` carries `FromCurrencyId`/`ToCurrencyId` as `int`. The source's own
`currencyCode` survives once, as `Order.SaleCurrencyCode`, and is never repeated per money value.

**Identities are typed.** `FlightId:long`, `FlightCapacityId:long`, `LegId:long`, `AirFareId:long`,
`OriginAirportId:int`, `AircraftId:int`, `CabinClassId:int?`, `RbdId:long?`, `MarketingAirlineId:int?`,
`FlightVersion:int?`. The canonical candidate writes them as JSON numbers. `IDENTITY-NAMESPACE-MATRIX.md` classifies
every one.

**Validity is three nullable instants.** `OfferExpiresAt?`, `PriceValidUntil?` on `OrderPreparation` and
`LastTicketingDate?` on `Order`, instead of three `ValidityFact` value objects and one `ObservedTimeFact`. A missing
source fact is `null`.

**The service boundary is typed, not generic.** `OrderService` carries what every service has — identity, item,
`ServiceType`, fulfillment profile, commercial status, creating change. `OrderAirTransportService` carries what only an
air service has — one traveller, one passenger segment, cabin, RBD, booking class, baggage, sold terms. There is no
dynamic detail dictionary, no schema registry and no supplier execution behaviour.

**Fare construction states what it prices and how complete it is.** `Assurance` plus explicit `ItemKeys`; a
construction naming an unknown item is refused, and a source that supplies no pricing units produces no construction at
all instead of a fabricated one.

**Pricing unit kinds are the owner's own three.** `OneWay`, `RoundTripFromOneWays`, `RoundTripFare` map to
`FarePricingUnitType` plus `AirFareConstructionType`, so the distinction between the two round-trip constructions is
not lost. Anything else fails closed.

**Journey direction and journey type are enums.** `BoundDirection` and `JourneyType`; an unrecognised source value
fails closed instead of being stored as raw text beside a null enum.

**Preparation has no consumption lifecycle.** Capture, acceptance and commit happen inside one `SaveChangesAsync`. The
receipt protects idempotency of the scoped command and key — it is not a generic single-use-offer rule, and the removed
`Consume()` never represented one.

**Canonical candidate schema restarts at `1.0`** and the projection schema at `1`, with no compatibility readers in
Domain or Query. This is the **proposed** rebaseline described in `MIGRATION-DECISION.md`; it is not yet authorized in
writing, and no deployed database carries these schemas.

## 6. What deliberately stayed out

- `FarePricingGroup`, `FareComponent` service/segment coverage, `ProductSnapshot`, item `CommercialTermsSnapshot` —
  no owner supplies them; they return with a source, not with a placeholder.
- `PricingLine.OriginalPricingLineId`, `FundingObligation.SupersededObligationId`, link traveller/segment scope —
  deferred, each behind a migration gate (`PACK-REQUIRED-SHAPE-GAP-MATRIX.md` §7).
- `FundingObligation.CurrentDisposition` — `BLOCKED_OWNER_CONTRACT`; the enum is not invented.
- `SellingOfficeKind.NotRecorded` — **removed** in this revision. With no deployed S1 schema, no writer can produce it,
  and an office id whose namespace cannot be proven now fails closed instead of being recorded as unknown.

## 7. What deliberately stayed in

- `FarePricingUnitCoveredBound.CoveredBoundOfferId` keeps the owner's own name and stays an opaque string — the one
  `BLOCKED_REAL_CONTRACT` in `IDENTITY-NAMESPACE-MATRIX.md`.
- `OrderJourney.BoundId`, `SourceTravellerRef`, `ClientTravellerRef`, `SourceOfferId`, `OwnerBindingRef`,
  `SourceConversionRef`, `EvidenceRef`, `SettlementAttribution.PartyRef` stay strings because the owner really does
  supply opaque strings there.
- `PricingLine.SourceOccurrencePath` stays, because pricing audit needs the exact source occurrence.
