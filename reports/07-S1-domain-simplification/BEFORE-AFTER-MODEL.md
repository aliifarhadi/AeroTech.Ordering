# Before / After Model

Stage: 07-S1-domain-simplification · 2026-09-20 · branch `k8s-stg` · reviewed baseline `0f103a3`

All counts are measured, not estimated. Table and property counts come from
`Migrations/OrderingDbContextModelSnapshot.cs` before (at `HEAD`) and after (working tree); file counts come from
`git diff --name-status 0f103a3 -- src tests Contracts`.

## 1. Totals

| | Before | After |
|---|---|---|
| Mapped tables in `OrderingDbContext` | 32 | 22 |
| Mapped properties in the model snapshot | 338 | 235 |
| `.cs` files in `AeroTech.Ordering.Domain` | 107 | 84 |
| Files deleted across `src/`, `tests/`, `Contracts/` | — | 50 |
| Files added | — | 5 |
| Files modified | — | 89 |

## 2. Tables removed

| Table | Why it is gone |
|---|---|
| `OrderComponentTotals` | Derived from the committed pricing lines; now computed in the projector. |
| `OrderItemServiceLinks`, `OrderItemServiceLinkTravelers`, `OrderItemServiceLinkSegments` | Nothing in S1 changes item membership; the association snapshot is an S14 need. |
| `OrderServiceBeneficiaries`, `OrderServiceCoverage` | Always exactly one row each; now the structural invariant `TravellerId` + `SegmentId` on `OrderService`. |
| `FarePricingGroups`, `FarePricingGroupTravelers` | AirOffer supplies no pricing group; always empty. |
| `FareComponentServices`, `FareComponentSegments` | AirOffer supplies no component↔service/segment binding; always empty. |
| `AirTransportServiceDetails` | The one registered detail schema; its three real facts are now typed columns on `OrderServices`. |

`OrderTravelers` / `OrderTravelerIdentities` were renamed to `OrderTravellers` / `OrderTravellerIdentities`; the Query
read models and `OrderDtoReader` were corrected to the same spelling in this stage (they had been left pointing at the
old table names, which is a bug that only the full persistence run exposed).

## 3. Tables that shrank

| Table | Before | After | What left |
|---|---|---|---|
| `Orders` | 73 | 33 | `AcceptedSource` (14), `Buyer`, three `ValidityFact`s, `ObservedTimeFact`, `CurrencySnapshot`, `SourceJourneyTypeRaw`, `IsSandboxScoped` |
| `OrderPreparations` | 46 | 22 | consumption lifecycle, validity facts, duplicated channel/office/actor/client-reference columns |
| `OrderServices` | 36 | 24 | generic service framework, fulfillment snapshot, supplier/delivery refs, detail-schema dictionary |
| `OrderItems` | 29 | 10 | `ProductSnapshot` (8), `CommercialTermsSnapshot` (7), `SourceItemRef`, `SourceOfferItemRef` |
| `PricingLines` | 37 | 33 | `CandidateLineRef`, `SourceBasisRef`, `Role`, `OriginalPricingLineId`; the `Source*` prefixes dropped from `Code`/`Name`/`Reference` |
| `FareComponents` | 17 | 13 | `FareOwnerRef`, `TariffRef`, `RuleRef`, `RoutingRef`; `SourceFareRef:string` → `AirFareId:long` |
| `FarePricingUnits` | 10 | 6 | `SourceUnitRef`, `SourceKindRaw`, `CombinationMethod`, `PricingGroupId` |
| `FarePricingUnitCoveredBounds` | 5 | 2 | reduced to `PricingUnitId` + `CoveredBoundOfferId` |
| `FareConstructions` | 8 | 5 | `SourceContextRef`, `Assurance`, `SupersededByConstructionId` |
| `FareConstructionItems` | 5 | 2 | reduced to the two keys |
| `FundingObligations` | 15 | 11 | `FundingObligationScope`, `OrderServiceId`, `PricingLineId`, `SupersededObligationId`, `SourceDecisionRef` |
| `CommandReceipts` | 18 | 14 | `Status`, `CompletedAt`, `PreparationId`, `OperationId` |
| `OrderChanges` | 10 | 9 | `SourceDecisionRef` |
| `PriceChangeSets` | 10 | 8 | `SourceDecisionRef`, `BaseCommercialVersion` |
| `OrderJourneys` | 10 | 9 | `SourceDirectionRaw`; `SourceBoundRef` → `BoundId` |
| `OrderSegments` | 22 | 21 | every `*Ref:string` became its native numeric identity |

## 4. Shape changes that matter more than the counts

**Money carries a currency identity.** `Money(decimal, int currencyId)`; `CurrencySnapshot` and every
`*CurrencyRef:string` are gone. `AppliedConversion` carries `FromCurrencyId`/`ToCurrencyId` as `int`.

**Identities are typed.** `FlightId:long`, `FlightCapacityId:long`, `LegId:long`, `AirFareId:long`,
`OriginAirportId:int`, `AircraftId:int`, `CabinClassId:int?`, `RbdId:long?`, `MarketingAirlineId:int?`,
`FlightVersion:int?`. The canonical candidate writes them as JSON numbers. `IDENTITY-NAMESPACE-MATRIX.md` classifies
every one.

**Validity is three nullable instants.** `OfferExpiresAt?`, `PriceValidUntil?` on `OrderPreparation` and
`LastTicketingDate?` on `Order`, instead of three `ValidityFact` value objects and one `ObservedTimeFact`. A missing
source fact is `null`.

**The air service is one typed entity.** `OrderService` names exactly one traveller and one passenger segment, both
required and positive, and carries `CabinClassId`, `RbdId`, `BookingClass`, `CheckedBaggage`, `CabinBaggage`,
`SoldTerms`. The schema registry, the dynamic `Details` dictionary and the generic service metadata are gone.

**Journey direction and journey type are enums.** `BoundDirection` and `JourneyType`; an unrecognised source value now
fails closed instead of being stored as raw text beside a null enum.

**Preparation has no consumption lifecycle.** Capture, acceptance and consumption happen inside one
`SaveChangesAsync`, so a preparation can never commit unconsumed; concurrency is protected by the idempotency receipt.

**Canonical candidate schema restarts at `1.0`** and the projection schema at `1`, with no compatibility readers in
Domain or Query. This is the approved rebaseline, not a silent break.

## 5. What deliberately stayed

- `FarePricingUnitCoveredBound.CoveredBoundOfferId` keeps the owner's own name and stays an opaque string — the one
  `BLOCKED_REAL_CONTRACT` in `IDENTITY-NAMESPACE-MATRIX.md`.
- `OrderJourney.BoundId`, `SourceTravellerRef`, `ClientTravellerRef`, `SourceOfferId`, `OwnerBindingRef`,
  `SourceConversionRef`, `EvidenceRef`, `SettlementAttribution.PartyRef` stay strings because the owner really does
  supply opaque strings there.
- `PricingLine.SourceOccurrencePath` stays, because pricing audit needs the exact source occurrence.
- `SellingOfficeKind.NotRecorded` stays as the honest value for a channel whose office namespace cannot be proven.
- `FundingObligation` stays, scoped to the item only, because original-sale liability is a stable identity.
