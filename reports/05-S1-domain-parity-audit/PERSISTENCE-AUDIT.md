# S1 Domain Parity — Persistence Audit

Stage: 05-S1-domain-parity-audit · 2026-09-19
Schema: `Order` (per the owner's answer to OD-C-01). Migrations history: `dbo.__EFMigrationsHistory` as configured on `OrderingDbContext`.

## 1. Migrations added

| # | Migration | Kind | Gate |
|---|---|---|---|
| 1 | `20260918224935_S1DomainParityTypedStructures` | additive DDL + evidence-derived backfill of the twelve required columns | none |
| 2 | `20260918225050_S1DomainParityBackfill` | data only: flight facts detail → segment, `PricingUnitsJson` → typed graph, association scope | validation `THROW`s before any write |
| 3 | `20260918231551_S1DomainParityDropRetiredColumns` | drops `FareConstructions.PricingUnitsJson` and the four retired `AirTransportServiceDetails` flight columns | shipped only after the equivalence assertions in `MigrationUpgradeTests` were green |

Order is strict: create typed structure → migrate verifiable data → verify → drop. Migration 2 never runs before 1, and 3 never runs before 2.

## 2. Tables added

All in schema `Order`:

| Table | Purpose |
|---|---|
| `OrderJourneys` | the restored Journey grouping (bound id, sequence, raw direction, canonical direction, origin, destination) |
| `FarePricingGroups` | source-proven traveler pricing groups |
| `FarePricingGroupTravelers` | group ↔ traveler binding |
| `FarePricingUnits` | typed pricing units (source unit ref, raw source kind, type, combination method, optional group) |
| `FarePricingUnitCoveredBounds` | `CoveredBoundOfferIds` at the level the source supplies them |
| `FareComponents` | typed fare components |
| `FareComponentServices` | component → service coverage, only where the source proves it |
| `FareComponentSegments` | component → segment coverage, only where the source proves it |
| `FareConstructionItems` | construction → item binding |
| `OrderItemServiceLinkTravelers` | `ScopeAtAssociation`: beneficiary travelers as they stood at acceptance |
| `OrderItemServiceLinkSegments` | `ScopeAtAssociation`: covered segments as they stood at acceptance |

## 3. Columns added

| Table | Columns |
|---|---|
| `Orders` | `SaleCurrencyCode`, `SourceJourneyTypeRaw`, `JourneyType`, `ObservedTicketingDeadlineValue`, `ObservedTicketingDeadlineSourceOwner`, `ObservedTicketingDeadlineSourceRef` |
| `OrderJourneys` | new table |
| `OrderSegments` | `JourneyId` (nullable FK), `OriginTerminalRef`, `DestinationTerminalRef`, `FlightNumber`, `FlightVersion`, `MarketingCarrierRef`, `OperatingCarrierRef`, `SourceCapacityRef`, `Duration`, `AircraftRef` |
| `OrderSegmentLegs` | `OriginRef`, `OriginTerminalRef`, `DestinationRef`, `DestinationTerminalRef`, `Departure`, `Arrival` |
| `OrderItems` | `ProductSourceSystem`, `ProductSourceOfferId`, `ProductSourceOfferItemRef`, `ProductCode`, `ProductName`, `ProductBrandCode`, `ProductBrandName`, `ProductVersion`, `TermsRefundability`, `TermsChangeability`, `TermsUpgradeEligibility`, `TermsSourceSystem`, `TermsSourcePolicyRef`, `TermsSourcePolicyVersion`, `TermsCapturedAt` |
| `OrderServices` | `ServiceCode`, `Name`, `PriceTreatment`, `SupplierPartyRef`, `DeliveryProviderRef`, `SoldTermRefundable`, `SoldTermChangeable`, `SoldTermUpgradable`, `FulfillmentProfileVersion`, `FulfillmentProfileAssurance`, `FundingRequirement` |
| `AirTransportServiceDetails` | `CheckedBaggagePieces`, `CheckedBaggageWeight`, `CheckedBaggageWeightUnit`, `CabinBaggagePieces`, `CabinBaggageWeight`, `CabinBaggageWeightUnit` |
| `PricingLines` | `SourceCode`, `SourceName`, `SourceReference`, `CalculationKind`, `ConversionSourceRef`, `ConversionFromCurrencyRef`, `ConversionToCurrencyRef`, `ConversionRate`, `ConversionDecimalPlaces`, `ConversionRoundingToken` |

`Orders.SaleCurrencyRef` keeps its name and meaning — the owned `CurrencySnapshot` maps onto the existing column plus the new code column, so no rename and no data movement.

## 4. Columns removed

| Table | Column | When | Why |
|---|---|---|---|
| `OrderServices` | `RequiresFunding` (bit) | migration 1 | replaced by the tri-state `FundingRequirement`; the old boolean asserted funding authority the live source never certified, so nothing is read from it |
| `FareConstructions` | `PricingUnitsJson` | migration 3 | replaced by the typed graph, after equivalence was proven |
| `AirTransportServiceDetails` | `FlightNumber`, `FlightVersion`, `MarketingCarrierRef`, `OperatingCarrierRef` | migration 3 | `OD-P-21`: flight-level facts have one canonical owner, `OrderSegments` |

## 5. Backfills, and exactly what each one is derived from

| Column | Value for pre-repair rows | Derived from |
|---|---|---|
| `PricingLines.CalculationKind` | `NotRecorded` (1) | nothing — the fact was discarded before this repair, so the honest state is recorded. **Not** `Amount`. |
| `OrderServices.PriceTreatment` | `SupplierOpaque` (4) | scoped to rows whose `FulfillmentProfileRef` is `AIROFFER-OBSERVED-AIR-UNCERTIFIED` or `REFERENCE-AIR-ETKT`; any other profile raises `THROW 51000` and the migration stops |
| `OrderServices.FulfillmentProfileVersion` | `'1'` | the only profile version that has ever existed |
| `OrderServices.FulfillmentProfileAssurance` | `Certified` for the reference profile, `NotCertified` for the live AirOffer profile | the profile ref already on the row |
| `OrderServices.FundingRequirement` | `Required` for the reference profile, `Unresolved` for the live AirOffer profile | same |
| `OrderServices.ReservationRequirement` / `DocumentKind` / `CapacityUnits` | left as stored for the reference profile; **reset to `Unresolved` / `Unresolved` / `NULL` for the live AirOffer profile** | `OD-P-07`: those values were adapter assumptions, never certified by the source. Leaving them would keep exactly the silent FlightFlow/ETKT claim the owner rejected. This is the one place the repair rewrites a stored value, and it replaces an invented claim with "not certified", never the reverse. |
| `OrderItems.ProductSourceSystem` / `ProductSourceOfferId` | `Orders.AcceptedSourceOwner` / `Orders.AcceptedSourceOfferId` | already-persisted canonical columns on the same order |
| `OrderItems.TermsSourceSystem` | `Orders.AcceptedSourceOwner` | same |
| `OrderItems.TermsCapturedAt` | `Orders.SourceCapturedAt` | same — **not** `Orders.CreatedAt`, which is the acceptance time, not the source capture time |
| `OrderItems.TermsRefundability` / `TermsChangeability` / `TermsUpgradeEligibility` | `Unknown` (1) | the flags were discarded before this repair; `Unknown` means exactly "no supplied fact" |
| `OrderSegments.FlightNumber` / `FlightVersion` / `MarketingCarrierRef` / `OperatingCarrierRef` | the value carried by the air services covering that segment | `AirTransportServiceDetails` joined through `OrderServiceCoverage`; a segment whose covering services disagree on any of the four raises `THROW 51001`; a `FlightVersion` that is not an invariant integer raises `THROW 51002` |
| `FarePricingUnits`, `FarePricingUnitCoveredBounds`, `FareComponents` | only the members present in the stored `PricingUnitsJson` | the blob itself; `CombinationMethod` is rewritten from the fabricated `ProviderDefined` to `Unspecified` per `OD-P-19` |
| `FareConstructionItems` | one binding per construction | only where the order has exactly one item, so the binding is unambiguous; otherwise no row is written |
| `OrderItemServiceLinkTravelers` / `…Segments` | the service's beneficiaries and coverage | the accepted canonical tables on the same order |

## 6. Values deliberately left unknown for pre-repair rows

Nothing below is reconstructable from accepted canonical tables, so nothing is invented:

- `OrderSegments.JourneyId` stays `NULL` and **no synthetic `UNKNOWN` journey is created**. `OrderJourneys` has zero rows after a legacy upgrade.
- `OrderSegments.OriginTerminalRef`, `DestinationTerminalRef`, `Duration`, `AircraftRef`, `SourceCapacityRef` stay `NULL`.
- `OrderSegmentLegs` airports, terminals and times stay `NULL`.
- `Orders.SaleCurrencyCode`, `SourceJourneyTypeRaw`, `JourneyType` and the observed ticketing deadline stay `NULL`.
- `OrderItems` product code, name, brand and version stay `NULL`.
- `OrderServices.ServiceCode`, `Name`, `SupplierPartyRef`, `DeliveryProviderRef` stay `NULL`.
- `AirTransportServiceDetails` baggage columns stay `NULL`.
- `PricingLines.SourceCode`, `SourceName`, `SourceReference` and the whole applied-conversion snapshot stay `NULL`.
- `FareComponents.TicketingRestrictionMinutes`, `FareOwnerRef`, `TariffRef`, `RuleRef`, `RoutingRef` stay `NULL`; `FarePricingUnits.SourceKindRaw` stays `NULL`.
- **No** `FarePricingGroups`, `FareComponentServices` or `FareComponentSegments` row is produced, because the blob proves none.

`Order.PreparationSourceEvidence.Payload` is never read, re-parsed or modified by any migration. Re-deriving accepted facts from the raw owner payload after acceptance would be a second acceptance, which S1 forbids.

## 7. Constraints, indexes and keys

| Object | Definition |
|---|---|
| `CK_OrderJourneys_Sequence` | `[Sequence] >= 1` |
| `UX` on `OrderJourneys` | unique `(OrderId, SourceBoundRef)` and unique `(OrderId, Sequence)` |
| `CK_OrderSegments_Duration` | `[Duration] IS NULL OR [Duration] >= 0` |
| index on `OrderSegments` | `(JourneyId)`, plus the pre-existing unique `(OrderId, SourceSegmentRef)` and `(OrderId, Sequence)` |
| `CK_OrderServices_CapacityUnits` | `[CapacityUnits] IS NULL OR [CapacityUnits] >= 0` |
| `CK_FarePricingGroups_Quantity` | `[Quantity] >= 1` |
| `CK_FarePricingUnits_Sequence` / `CK_FareComponents_Sequence` | `[Sequence] >= 1` |
| unique keys | `(FareConstructionId, SourceUnitRef)`, `(PricingUnitId, Sequence)`, `(PricingUnitId, SourceBoundRef)`, `(FareComponentId, OrderServiceId)`, `(FareComponentId, OrderSegmentId)`, `(FareConstructionId, OrderItemId)`, `(PricingGroupId, TravelerId)`, `(LinkId, TravelerId)`, `(LinkId, SegmentId)` |
| FKs | every new child FK is `DeleteBehavior.Restrict`, matching the rest of the aggregate |
| retained | `UX_FareConstructions_CurrentPerOrder` (filtered unique current construction per order) is unchanged |

## 8. Precision

`ConversionRate` uses `HasRatePrecision()` (28,12); baggage `Weight` uses `HasQuantityPrecision()` (18,6); all monetary columns keep `HasAmountPrecision()` (28,8). `Precision/ModelPrecisionTests` and `Precision/DecimalPrecisionTests` pass unchanged, so every new decimal column is covered by the existing precision guard.

## 9. Verification results

| Check | Command | Result |
|---|---|---|
| model matches migrations | `dotnet ef migrations has-pending-model-changes` | `No changes have been made to the model since the last migration.` |
| fresh database | `MigrationUpgradeTests` (B0 → latest) | PASS |
| upgrade from pre-repair S1 with an accepted order | `MigrationUpgradeTests.S1_domain_parity_upgrade_preserves_accepted_rows_without_inventing_facts` | PASS |
| dev database | `dotnet ef database update` against `localhost\SQLEXPRESS` / `DotAirOrderingVNext` | applied; the database held no accepted orders, so every backfill was a no-op there |

The dev database was **not** reset, dropped or recreated at any point.
