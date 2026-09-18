# S1 Domain Parity — Implementation Plan

Stage: 05-S1-domain-parity-audit
Planned against HEAD `deaf6b0` (branch `k8s-stg`, clean tree).
Authority: `reports/00-decisions/S1-DOMAIN-PARITY-OPEN-DECISIONS.md` (OD-P-01 … OD-P-20 answered) and Pack 3.8.
Status: plan only. **No production, domain or persistence code was changed in this run.**

---

## 0. Cross-cutting mechanics that every area depends on

### 0.1 Candidate schema version

Every approved repair adds a fact to `NormalizedCandidate`, whose canonical form is the digest the client accepts. Therefore:

- `NormalizedCandidate.CurrentSchemaVersion` moves `"2.0"` → `"3.0"`.
- `NormalizedCandidateJson.ToNode` / `Write` / `Digest` emit 3.0 only. `CandidateVocabulary` gains every new key.
- `CandidateValidator` keeps rejecting anything that is not the current version, so an unconsumed 2.0 preparation cannot be accepted. Preparations are pre-acceptance and re-creatable; none is migrated.
- `NormalizedCandidateJson.Read` is made tolerant of a 2.0 document (new members read as absent) **for evidence and forensics only**, so a consumed historical preparation stays machine-readable. Acceptance still requires 3.0. If the owner prefers a hard cut, `Read` rejects 2.0 and the historical rows stay readable only as raw text — say so in review.

### 0.2 Order of work

1. Contracts (`AeroTech.Messages/Ordering/Enums`) — the few enum additions listed in §3 and §7.
2. Domain value objects and entities.
3. Candidate value objects + canonical JSON + validator.
4. Persistence configurations + additive migration.
5. AirOffer mapper + reference adapter.
6. Projection (`OrderDtoBuilder`, `OrderDto`) and read path.
7. Tests.
8. Data migration of `PricingUnitsJson`, verification, then the drop migration.

Steps 1–7 are one reviewable change; step 8's drop is a separate commit gated on the verification in §17.3.

### 0.3 Shared-contract changes required

Only `Contracts/AeroTech.Messages/Ordering/**` is touched, one enum per file, explicit values, `[Display(Name = …)]` on every member:

| Enum | Change | Driven by |
|---|---|---|
| `PricingCalculationKind` | **new**: `Amount = 1`, `Percentage = 2` | OD-P-18 |
| `FulfillmentProfileAssurance` | **new**: `Certified = 1`, `NotCertified = 2` | OD-P-07 |
| `FundingRequirement` | **new**: `Unresolved = 1`, `NotRequired = 2`, `Required = 3` | OD-P-07 |
| `ReservationRequirement` | add `Unresolved = 4` | OD-P-07 |
| `FulfillmentDocumentKind` | add `Unresolved = 5` | OD-P-07 |

No other Messages folder is touched. `CommercialTermState`, `ServicePriceTreatment`, `FarePricingUnitType`, `FareCombinationMethod`, `JourneyType`, `BoundDirection`, `BaggageWeightUnit` and `PricingBasisType` already carry every member these repairs need.

---

## 1. Source currency code snapshot — OD-P-01

| Aspect | Detail |
|---|---|
| **Exact current files** | `src/AeroTech.Ordering.Domain/OrderAggregate/Order.cs` (`SaleCurrencyRef`), `src/AeroTech.Ordering.Domain/_Shared/ValueObjects/Money.cs`, `src/AeroTech.Ordering.Providers/AirOffer/Services/AirOfferCandidateMapper.cs` (`Currency(int)`), `src/AeroTech.Ordering.Providers/AirOffer/Wire/AirOfferDetailsWire.cs` (`CurrencyCode` already parsed, unused), `src/AeroTech.Ordering.Persistence/OrderAggregate/OrderConfiguration.cs` |
| **Add / change / delete** | add `Domain/_Shared/ValueObjects/CurrencySnapshot.cs`; change `Order`, `NormalizedCandidate`, `NormalizedCandidateJson`, `CandidateVocabulary`, `CandidateValidator`, `AirOfferCandidateMapper`, `OrderConfiguration` |
| **Domain shape** | `public sealed record CurrencySnapshot(string CurrencyRef, string? CurrencyCode)`. `Order.SaleCurrencyRef` becomes `Order.SaleCurrency` of that type. **`Money` is not changed**: AirOffer supplies no per-value currency code, and OD-P-17 forbids speculative empty columns. When a future source supplies per-value codes, `Money` gains the same optional snapshot then. |
| **DB change** | `Order.Orders` gains `SaleCurrencyCode NVARCHAR(3) NULL`; existing `SaleCurrencyRef` keeps its name and meaning |
| **Mapper change** | `AirOfferCandidateMapper.Map` passes `details.CurrencyCode` (already deserialised) into the candidate's sale currency; a missing/blank code is `null`, never looked up in ReferenceData |
| **Projection change** | `MoneyDto` gains `CurrencyCode` **only for the order-level `grandTotal`** if the owner wants it exposed; default plan is **no public change** (see §16) |
| **Tests** | `Domain.Tests/OrderPreparationAggregate/CandidateCanonicalFormTests` (code appears in the canonical form and changes the digest), `Persistence.Tests/S1/AirOfferLiveCandidateBridgeTests` (live response code preserved), new `Persistence.Tests/S1/InformationPreservationTests` vector `CurrencyId + CurrencyCode`, plus a test proving a details payload **without** a code yields `null` and touches no ReferenceData |

---

## 2. Applied conversion / rate-of-exchange evidence — OD-P-16

| Aspect | Detail |
|---|---|
| **Exact current files** | `src/AeroTech.Ordering.Domain/OrderAggregate/Entities/PricingLine.cs` (`SourceConversionRef`), `src/AeroTech.Ordering.Domain/OrderPreparationAggregate/ValueObjects/CandidatePricingLine.cs`, `src/AeroTech.Ordering.Providers/AirOffer/Wire/AirOfferRateOfExchangeWire.cs` (parsed, unused), `AirOfferCandidateMapper.Line`, `src/AeroTech.Ordering.Persistence/OrderAggregate/PricingLineConfiguration.cs` |
| **Add / change / delete** | add `Domain/OrderAggregate/ValueObjects/AppliedConversion.cs`; change `PricingLine`, `CandidatePricingLine`, canonical JSON, validator, mapper, `PricingLineConfiguration` |
| **Domain shape** | `public sealed record AppliedConversion(string SourceConversionRef, string FromCurrencyRef, string ToCurrencyRef, decimal Rate, int DecimalPlaces, string? RoundingToken)`, held as `PricingLine.AppliedConversion` (nullable — present only when the accepted sale value came from the equivalent valuation). `RoundingToken` is stored verbatim and **never read by any calculation**. No FX engine, no rate table, no recomputation of accepted money. |
| **DB change** | `Order.PricingLines` gains `Conversion_SourceRef`, `Conversion_FromCurrencyRef`, `Conversion_ToCurrencyRef`, `Conversion_Rate DECIMAL(19,8)`, `Conversion_DecimalPlaces INT`, `Conversion_RoundingToken NVARCHAR` — all nullable, owned type. Precision registered in `ModelPrecisionTests`. |
| **Mapper change** | build a lookup of `details.RatesOfExchange` by `RateOfExchangePeriodId`; when the line's sale value comes from `EquivalentAmount`, resolve the row and attach the snapshot. A referenced period id with no matching rate row is `AirOfferContractMismatchException`, not a silent null. |
| **Projection change** | none by default (see §16) |
| **Tests** | canonical-form + digest test; mapper test for resolved rate, for a converted line, and for the mismatch case; preservation vector `ROE reference and rate evidence`; an explicit test that `RoundingToken` participates in no arithmetic (accepted totals unchanged when the token differs) |

---

## 3. Percentage provenance — OD-P-18

| Aspect | Detail |
|---|---|
| **Exact current files** | `AirOfferCandidateMapper.Line` (the `row.IsPercentage` branch), `CandidatePricingLine`, `PricingLine` |
| **Add / change / delete** | add `Contracts/AeroTech.Messages/Ordering/Enums/PricingCalculationKind.cs`; change `CandidatePricingLine`, `PricingLine`, canonical JSON, validator, mapper, `PricingLineConfiguration` |
| **Domain shape** | `PricingLine.SourceCalculationKind` of type `PricingCalculationKind`. Nothing else: the numeric percentage basis is **not** modelled, because OD-P-18 leaves its meaning uninterpreted. |
| **DB change** | `Order.PricingLines` gains `SourceCalculationKind INT NOT NULL` (default `Amount` for backfill, see §17.2) |
| **Mapper change** | `Line(...)` sets `Percentage` when `row.IsPercentage`, otherwise `Amount`. The existing monetary behavior of the percentage branch (sale = original = `EquivalentAmount`, per OD-S1-08) is **unchanged**; `row.Amount` is still not read as a rate. |
| **Projection change** | none |
| **Tests** | mapper test for both kinds; preservation vector `IsPercentage`; a regression test asserting the accepted monetary value of a percentage row is byte-identical to today's |

---

## 4. `OrderItem.ProductSnapshot` — OD-P-03

| Aspect | Detail |
|---|---|
| **Exact current files** | `src/AeroTech.Ordering.Domain/OrderAggregate/Entities/OrderItem.cs`, `OrderPreparationAggregate/ValueObjects/CandidateItem.cs`, `src/AeroTech.Ordering.Persistence/OrderAggregate/OrderItemConfiguration.cs`, `AirOfferCandidateMapper.Map` (the single `CandidateItem`) |
| **Add / change / delete** | add `Domain/OrderAggregate/ValueObjects/ProductSnapshot.cs` and `OrderPreparationAggregate/ValueObjects/CandidateProductSnapshot.cs`; change `OrderItem`, `CandidateItem`, canonical JSON, validator, mapper, `OrderItemConfiguration` |
| **Domain shape** | `public sealed record ProductSnapshot(string SourceSystem, string SourceOfferId, string? SourceOfferItemRef, string? ProductCode, string? ProductName, string? BrandCode, string? BrandName, string? ProductVersion, DateTimeOffset AcceptedAt)`, owned by `OrderItem`. **No marketing/operating carrier on the package item** (OD-P-03 forbids collapsing per-segment carriers). Item quantity and unit of measure are **not** added. |
| **DB change** | `Order.OrderItems` gains owned columns `Product_SourceSystem`, `Product_SourceOfferId`, `Product_SourceOfferItemRef`, `Product_Code`, `Product_Name`, `Product_BrandCode`, `Product_BrandName`, `Product_Version`, `Product_AcceptedAt` — one table, matching DOMAIN/13's `OrderItems … source/product/terms` row |
| **Mapper change** | populate `SourceSystem = AirOfferProfile.Owner`, `SourceOfferId = requestedOfferId`, `AcceptedAt` from the capture clock; everything else `null` — AirOffer publishes no product code, name, brand or version for the package |
| **Projection change** | none by default; `OrderItemDto` could gain the supplied subset later (see §16) |
| **Tests** | canonical-form/digest; a mapper test asserting every unsupplied member is `null` and that no ReferenceData call is made; preservation vector `source offer id at item level` |

---

## 5. `CommercialTermsSnapshot` + exact per-service source flags — OD-P-04

| Aspect | Detail |
|---|---|
| **Exact current files** | `OrderItem.cs`, `Domain/OrderAggregate/ValueObjects/AirTransportDetail.cs`, `OrderPreparationAggregate/Policies/ServiceDetailSchemaRegistry.cs`, `CandidateItem.cs`, `CandidateService.cs`, `AirOfferCandidateMapper.AirService`, `src/AeroTech.Ordering.Providers/AirOffer/Wire/AirOfferCouponWire.cs` (flags parsed, unused), `OrderItemConfiguration.cs`, `OrderServiceConfiguration.cs` |
| **Add / change / delete** | add `Domain/OrderAggregate/ValueObjects/CommercialTermsSnapshot.cs` and `Domain/OrderAggregate/ValueObjects/SoldTermFlags.cs`; change `OrderItem`, `AirTransportDetail`, `ServiceDetailSchemaRegistry`, candidate VOs, canonical JSON, validator, mapper, both configurations |
| **Domain shape** | per service: `SoldTermFlags(bool? Refundable, bool? Changeable, bool? Upgradable)` on `AirTransportDetail` — the exact source facts at their original granularity. Per item: `CommercialTermsSnapshot(CommercialTermState Refundability, CommercialTermState Changeability, CommercialTermState UpgradeEligibility, string SourceSystem, string? SourcePolicyRef, string? SourcePolicyVersion, DateTimeOffset TermsCapturedAt)`. Summary rule, computed in the Domain when the item is built: all supplied `true` → `Permitted`; all supplied `false` → `Prohibited`; mixed → `Conditional`; nothing supplied → `Unknown`. **`NotSupplied` is never used for disagreement** — and `CommercialTermState` has no such member, so the rule is enforced by the type. No eligibility behavior is derived from either. |
| **DB change** | `Order.AirTransportServiceDetails` gains `Terms_Refundable BIT NULL`, `Terms_Changeable BIT NULL`, `Terms_Upgradable BIT NULL`. `Order.OrderItems` gains `Terms_Refundability INT`, `Terms_Changeability INT`, `Terms_UpgradeEligibility INT`, `Terms_SourceSystem`, `Terms_SourcePolicyRef NULL`, `Terms_SourcePolicyVersion NULL`, `Terms_CapturedAt` |
| **Mapper change** | `AirService(...)` carries the three coupon flags into the candidate service detail (as typed members of `CandidateService`, **not** as new string keys in the `Details` dictionary — these are booleans, and the registry forbids non-registered keys); the item-level summary is computed by the Domain from the accepted services, never by the adapter |
| **Projection change** | `OrderServiceDto.AirTransport` and `OrderItemDto` may expose them; default plan is **no public change** (see §16) |
| **Tests** | unit tests for all four summary outcomes (`Permitted`, `Prohibited`, `Conditional`, `Unknown`) including the mixed-coupon case; a test asserting the per-service flags survive independently of the summary; preservation vectors `IsRefundable / IsChangeable / IsUpgradable` |

---

## 6. `OrderService` base fields — OD-P-06

| Aspect | Detail |
|---|---|
| **Exact current files** | `Domain/OrderAggregate/Entities/OrderService.cs`, `CandidateService.cs`, `AirOfferCandidateMapper.AirService`, `src/AeroTech.Ordering.Persistence/OrderAggregate/OrderServiceConfiguration.cs` |
| **Add / change / delete** | change `OrderService`, `CandidateService`, canonical JSON, validator, mapper, `OrderServiceConfiguration` |
| **Domain shape** | `string? ServiceCode`, `string? Name`, `ServicePriceTreatment PriceTreatment`, `string? SupplierPartyRef`, `string? DeliveryProviderRef` |
| **DB change** | `Order.OrderServices` gains `ServiceCode NULL`, `Name NULL`, `PriceTreatment INT NOT NULL`, `SupplierPartyRef NULL`, `DeliveryProviderRef NULL` |
| **Mapper change** | `ServiceCode = null`, `Name = null`, `SupplierPartyRef = null`, `DeliveryProviderRef = null`, `PriceTreatment = ServicePriceTreatment.SupplierOpaque`. `FlightNumber` is **not** used as a code; carrier identity is **not** used as a supplier ref; `SeparatelyPriced` is **not** used. |
| **Projection change** | none by default |
| **Tests** | a mapper test asserting the four null members and `SupplierOpaque`; an architecture-style assertion that no code path writes `SeparatelyPriced` for the AirOffer profile |

---

## 7. Fulfillment profile — reference vs live — OD-P-07

| Aspect | Detail |
|---|---|
| **Exact current files** | `Domain/OrderAggregate/ValueObjects/FulfillmentProfileSnapshot.cs`, `OrderPreparationAggregate/ValueObjects/CandidateFulfillmentProfile.cs`, `AirOfferCandidateMapper` (`FulfillmentProfileRef` constant, `AirService`), `src/AeroTech.Ordering.Providers/AirOffer/AirOfferProfile.cs`, `src/AeroTech.Ordering.Providers.Deterministic/Offers/ReferenceOfferProfile.cs`, `…/Offers/ReferenceOfferSourceAdapter.cs`, `…/Offers/ReferenceOfferCatalog.cs`, `OrderServiceConfiguration.cs` |
| **Add / change / delete** | add the three enums of §0.3; change `FulfillmentProfileSnapshot`, `CandidateFulfillmentProfile`, canonical JSON, validator, the AirOffer mapper, `OrderServiceConfiguration` |
| **Domain shape** | `FulfillmentProfileSnapshot(string ProfileRef, string ProfileVersion, FulfillmentProfileAssurance Assurance, ReservationRequirement ReservationRequirement, FulfillmentDocumentKind DocumentKind, FundingRequirement FundingRequirement, int? CapacityUnits)`. Live AirOffer emits `Assurance = NotCertified`, `ReservationRequirement = Unresolved`, `DocumentKind = Unresolved`, `FundingRequirement = Unresolved`, `CapacityUnits = null`. The reference simulator emits `Assurance = Certified` with exact target semantics, because it owns its own test contract. |
| **DB change** | `Order.OrderServices` fulfillment columns become `Fulfillment_ProfileRef`, `Fulfillment_ProfileVersion`, `Fulfillment_Assurance INT`, `Fulfillment_ReservationRequirement INT`, `Fulfillment_DocumentKind INT`, `Fulfillment_FundingRequirement INT`, `Fulfillment_CapacityUnits INT NULL` |
| **Mapper change** | the AirOffer literal `AIROFFER-OBSERVED-AIR-UNCERTIFIED` stays as the profile **identity** but carries a version and the `NotCertified` assurance; no requirement value is asserted. Configuration is **not** used to launder the assumption. No generic profile engine is built. |
| **Projection change** | none |
| **Tests** | a test proving a live AirOffer order carries `NotCertified` and `Unresolved` requirement semantics; a test proving no code path treats such a service as Reserve/Issue eligible; the existing reference-adapter tests keep asserting `Certified` semantics; `S1RailTests` gains an assertion that the live and reference profiles never share a profile ref |
| **Residual block** | the live profile's real requirement semantics stay owner-blocked under BD-002 / BD-005 / BD-006; this plan records them as unresolved, it does not resolve them |

---

## 8. Baggage allowance — OD-P-08

| Aspect | Detail |
|---|---|
| **Exact current files** | `Domain/OrderAggregate/ValueObjects/AirTransportDetail.cs`, `CandidateService.cs`, `AirOfferCandidateMapper.AirService`, `src/AeroTech.Ordering.Providers/AirOffer/Wire/AirOfferCouponWire.cs`, `OrderServiceConfiguration.cs` |
| **Add / change / delete** | add `Domain/OrderAggregate/ValueObjects/BaggageAllowance.cs`; change `AirTransportDetail`, `CandidateService`, canonical JSON, validator, mapper, `OrderServiceConfiguration` |
| **Domain shape** | `public sealed record BaggageAllowance(int? Pieces, decimal? Weight, BaggageWeightUnit? Unit)`; `AirTransportDetail` gains `CheckedBaggage` and `CabinBaggage`, both nullable and kept separate. Sold/included product context only — not a separately sold service, not consumption state. Missing pieces or weight stay `null`; nothing is inferred. |
| **DB change** | `Order.AirTransportServiceDetails` gains `CheckedBaggage_Pieces`, `CheckedBaggage_Weight DECIMAL`, `CheckedBaggage_Unit INT`, `CabinBaggage_Pieces`, `CabinBaggage_Weight`, `CabinBaggage_Unit` — all nullable |
| **Mapper change** | map the coupon's six baggage members; a weight with no unit is a contract mismatch (mirroring the historical rule), a zero-piece/zero-weight allowance with no unit is `null`, not a fabricated zero |
| **Projection change** | none by default |
| **Tests** | mapper tests for pieces-only, weight+unit, both, and neither; a test proving no `OrderService` of type baggage is created; preservation vectors `baggage pieces/weight/unit` and `cabin baggage pieces/weight/unit` |

---

## 9. Journey — OD-P-09

| Aspect | Detail |
|---|---|
| **Exact current files** | `Domain/OrderAggregate/Order.cs` (`AddSegments`), `Entities/OrderSegment.cs`, `CandidateSegment.cs`, `NormalizedCandidate.cs`, `AirOfferCandidateMapper.Map` (the `AirTransports` loop that flattens bounds), `src/AeroTech.Ordering.Providers/AirOffer/Wire/AirOfferAirTransportWire.cs` (`BoundId`, `Direction`, `Sequence`, origin, destination — parsed, unused), `OrderSegmentConfiguration.cs`, `OrderConfiguration.cs` |
| **Add / change / delete** | add `Domain/OrderAggregate/Entities/OrderJourney.cs`, `OrderPreparationAggregate/ValueObjects/CandidateJourney.cs`, `src/AeroTech.Ordering.Persistence/OrderAggregate/OrderJourneyConfiguration.cs`; change `Order`, `OrderSegment`, `CandidateSegment`, `NormalizedCandidate`, canonical JSON, vocabulary, validator, mapper, `OrderSegmentConfiguration` |
| **Domain shape** | `OrderJourney : Entity<long>` with `OrderId`, `SourceBoundRef`, `Sequence`, `SourceDirectionRaw` (string, verbatim), `Direction` (`BoundDirection?`, **null until the vocabulary is contractually known**), `OriginRef`, `DestinationRef`. `OrderSegment` gains `JourneyId`. `Order` gains `SourceJourneyTypeRaw` (string?, verbatim) and `JourneyType` (`JourneyType?`, null for the same reason). Outbound/inbound is never inferred from segment order. |
| **DB change** | new table `Order.OrderJourneys` (PK `Id`, FK `OrderId`, unique `(OrderId, SourceBoundRef)`, index `(OrderId, Sequence)`); `Order.OrderSegments` gains `JourneyId BIGINT NOT NULL` with an FK; `Order.Orders` gains `SourceJourneyTypeRaw NVARCHAR NULL`, `JourneyType INT NULL` |
| **Mapper change** | emit one `CandidateJourney` per `airTransports[]` entry with its raw `Direction` rendered losslessly (the wire member is a `JsonElement`: a string is taken verbatim, a number is rendered invariantly); each `CandidateSegment` names its journey ref; `details.JourneyType` is carried verbatim |
| **Projection change** | `OrderDto.Itinerary` is currently a flat segment list. Making it journey-grouped **changes a ratified public contract** — default plan is **no public change**: the journey is persisted and the projection keeps its current flat shape (see §16). |
| **Tests** | a round-trip test for a two-bound offer proving two journeys with their raw direction values; a test proving `Direction` and `JourneyType` stay `null` while the vocabulary is unknown; preservation vectors `BoundId`, `direction raw/source fact`, `JourneyType` |
| **Open handoff** | the direction and journey-type vocabularies join the `Stop` handoff of §16 |

---

## 10. Sold segment snapshot — OD-P-10

| Aspect | Detail |
|---|---|
| **Exact current files** | `Entities/OrderSegment.cs`, `CandidateSegment.cs`, `AirOfferCandidateMapper.Map`, `src/AeroTech.Ordering.Providers/AirOffer/Wire/AirOfferFlightWire.cs`, `OrderSegmentConfiguration.cs` |
| **Add / change / delete** | change `OrderSegment`, `CandidateSegment`, canonical JSON, vocabulary, validator, mapper, `OrderSegmentConfiguration` |
| **Domain shape** | `OrderSegment` gains `OriginTerminalRef`, `DestinationTerminalRef`, `Duration` (minutes, `int?`), `AircraftRef`, `SourceFlightRef` (external `FlightId`), `SourceFlightVersion`, `MarketingCarrierRef`, `OperatingCarrierRef`, `SourceCapacityRef` (the AirOffer `FlightCapacityId`). The name `SourceCapacityRef` is deliberate: it is a retained source reference, **not** locally owned capacity truth, and BD-002/BD-003 still govern any FlightFlow use. **Cabin, RBD and booking class are not copied here.** |
| **DB change** | `Order.OrderSegments` gains the nine columns above, all nullable except those AirOffer always supplies (`SourceCapacityRef`, `SourceFlightVersion`, carriers) |
| **Mapper change** | the existing `segments.Add(...)` loop reads the members it already has in `AirOfferFlightWire` and stops discarding them |
| **Projection change** | `OrderSegmentDto` could carry carriers, terminals, duration and aircraft; default plan is **no public change** (see §16) |
| **Tests** | preservation vectors `FlightId`, `FlightVersion`, `FlightCapacityId as source ref`, `terminals`, `duration`, `aircraft`, `marketing/operating carrier`; an assertion that no segment column holds cabin/RBD/booking class |
| **Depends on** | `OD-P-21` for where the carriers and flight version finally live — recorded in `reports/00-decisions/S1-DOMAIN-PARITY-OPEN-DECISIONS.md`; both paths are carried until it is answered |

---

## 11. Segment legs — OD-P-11

| Aspect | Detail |
|---|---|
| **Exact current files** | `Entities/OrderSegmentLeg.cs`, `CandidateSegment.OperationalLegRefs` (a bare `IReadOnlyList<string>`), `AirOfferCandidateMapper.Map`, `src/AeroTech.Ordering.Providers/AirOffer/Wire/AirOfferLegWire.cs`, `OrderSegmentLegConfiguration.cs` |
| **Add / change / delete** | add `OrderPreparationAggregate/ValueObjects/CandidateSegmentLeg.cs`; change `OrderSegmentLeg`, `CandidateSegment` (`OperationalLegRefs` → `IReadOnlyList<CandidateSegmentLeg>`), canonical JSON, vocabulary, validator, mapper, `OrderSegmentLegConfiguration` |
| **Domain shape** | `OrderSegmentLeg` gains `OriginRef`, `OriginTerminalRef`, `DestinationRef`, `DestinationTerminalRef`, `Departure`, `Arrival`. **No stop, connection or protection member** — OD-P-12 forbids it. |
| **DB change** | `Order.OrderSegmentLegs` gains the six columns; terminals nullable |
| **Mapper change** | project each `AirOfferLegWire` into the new candidate leg; `Stop` remains unread |
| **Projection change** | `OrderSegmentLegDto` currently carries `Sequence` + `LegRef` only; default plan is **no public change** |
| **Tests** | a multi-leg segment round trip; preservation vector `leg airports/terminals/times`; an assertion that no leg column or DTO member mentions stop or connection |

---

## 12. Pricing-line code / name / reference — OD-P-15

| Aspect | Detail |
|---|---|
| **Exact current files** | `Entities/PricingLine.cs`, `CandidatePricingLine.cs`, `AirOfferCandidateMapper.Line`, `src/AeroTech.Ordering.Providers/AirOffer/Wire/AirOfferPricingLineWire.cs` (`Name`, `Code`, `Reference` parsed, unused), `PricingLineConfiguration.cs` |
| **Add / change / delete** | change `PricingLine`, `CandidatePricingLine`, canonical JSON, vocabulary, validator, mapper, `PricingLineConfiguration` |
| **Domain shape** | `PricingLine` gains `SourceCode`, `SourceName`, `SourceReference`, all `string?`. The existing `SourceLineRef` (the exact occurrence path) is unchanged. No reinterpretation: `SourceCode` is not treated as a tax code, and its meaning follows `Component`. |
| **DB change** | `Order.PricingLines` gains `SourceCode NULL`, `SourceName NULL`, `SourceReference NULL` |
| **Mapper change** | `Line(...)` carries the three members through on both the percentage and the amount branch |
| **Projection change** | `OrderPriceLineDto` gaining `code` and `name` is the one place where public exposure is genuinely useful to an airline user. It is **additive** to a ratified contract, so it is listed as a decision point in §16 and is **not** applied by default. |
| **Tests** | preservation vectors `pricing Code`, `pricing Name`, `pricing Reference`; a test that two rows with the same code and different occurrence paths stay distinct rows |

---

## 13. Typed fare-construction graph — OD-P-19

| Aspect | Detail |
|---|---|
| **Exact current files** | `Domain/OrderAggregate/Entities/FareConstruction.cs` (`PricingUnitsJson`), `Domain/OrderAggregate/Order.cs` (`AddFareConstruction`, which serialises a node of the candidate), `OrderPreparationAggregate/ValueObjects/CandidateFareConstruction.cs`, `CandidatePricingUnit.cs`, `CandidatePricingGroup.cs`, `CandidateFareComponent.cs`, `AirOfferCandidateMapper.Map` (the `details.PricingUnits` projection), `src/AeroTech.Ordering.Providers/AirOffer/Wire/AirOfferPricingUnitWire.cs`, `AirOfferFareComponentWire.cs`, `src/AeroTech.Ordering.Persistence/OrderAggregate/FareConstructionConfiguration.cs` |
| **Add / change / delete** | **add** `Entities/FareConstructionItem.cs`, `Entities/FareConstructionCoveredBound.cs`, `Entities/FarePricingGroup.cs`, `Entities/FarePricingGroupTraveler.cs`, `Entities/FarePricingUnit.cs`, `Entities/FareComponent.cs`, `Entities/FareComponentService.cs`, `Entities/FareComponentSegment.cs`, and one persistence configuration per entity; **change** `FareConstruction`, `Order.AddFareConstruction`, the candidate VOs, canonical JSON, vocabulary, validator, mapper; **delete** `FareConstruction.PricingUnitsJson` only in the final migration of §17.3 |
| **Domain shape** | `FareConstruction` keeps `OrderIdAtCreation`, `CreatedByChangeId`, `SupersededByConstructionId`, `Assurance`, `SourceContextRef` and gains `Items`, `PricingGroups`, `CoveredBounds`. `FarePricingUnit` carries `SourceUnitRef`, `SourceKindRaw` (verbatim), `Type` (`FarePricingUnitType`, `Unspecified` unless the vocabulary is contractually known), `CombinationMethod` (`FareCombinationMethod`, **`Unspecified` when the source did not establish it** — never `ProviderDefined` merely because a provider answered), `Sequence`, `CoveredBoundRefs`. `FareComponent` carries `SourceFareRef`, `Sequence`, `FareBasis`, `FareFamily`, `FareType`, `CabinRef`, `RbdRef`, `BookingClass`, `TicketingRestrictionMinutes`, and the optional `FareOwnerRef`, `TariffRef`, `RuleRef`, `RoutingRef`. Coverage (`FareComponentService`, `FareComponentSegment`) and `FarePricingGroup` rows exist **only when the source proves them**; for AirOffer today the graph is a construction with units, components and covered bounds, **no groups and no component coverage rows**. Partial/opaque construction is a first-class state, not a defect to be padded. |
| **DB change** | new tables in schema `Order`: `FareConstructionItems`, `FareConstructionCoveredBounds`, `FarePricingGroups`, `FarePricingGroupTravelers`, `FarePricingUnits`, `FareComponents`, `FareComponentServices`, `FareComponentSegments`. `Order.FareConstructions` keeps `PricingUnitsJson` until §17.3 drops it. Uniqueness: `(FareConstructionId, SourceUnitRef)`, `(PricingUnitId, Sequence)`, `(FareComponentId, OrderServiceId)`, `(FareComponentId, OrderSegmentId)`, `(FareConstructionId, OrderItemId)`. |
| **Mapper change** | keep emitting units and components from `details.PricingUnits`, **plus** `SourceKindRaw = unit.Kind`, `CombinationMethod = Unspecified`, `CoveredBoundRefs = unit.CoveredBoundOfferIds`, `TicketingRestrictionMinutes` on the component. `CoveredServiceRefs` stays empty because AirOffer proves no per-component coverage — and the empty list now means "not supplied", which the typed model can hold. |
| **Projection change** | the fare construction is not in `OrderDto` today and is not added |
| **Tests** | a test proving a construction with zero groups and zero component-coverage rows is valid; a test proving no group is created from equal PTC or ticket order; preservation vectors `FareBasis`, `FareFamily`, `FareType`, `TicketingRestrictionMinutes`, `source PricingUnit Kind`, `CoveredBoundOfferIds`; a rebuild test proving the typed graph reproduces the accepted facts that `PricingUnitsJson` held |

---

## 14. Typed observed `LastTicketingDate` — OD-P-02

| Aspect | Detail |
|---|---|
| **Exact current files** | `Domain/_Shared/ValueObjects/ValidityFact.cs`, `Domain/OrderAggregate/Order.cs` (`TicketingValidity`), `OrderPreparationAggregate/ValueObjects/CandidateValidity.cs`, `AirOfferCandidateMapper.TicketingReason` (the prose), `OrderConfiguration.cs` |
| **Add / change / delete** | add `Domain/_Shared/ValueObjects/ObservedTimeFact.cs`; change `CandidateValidity`, `Order`, canonical JSON, vocabulary, validator, mapper, `OrderConfiguration` |
| **Domain shape** | `public sealed record ObservedTimeFact(DateTimeOffset Value, string SourceOwner, string SourceRef)`. `Order` gains `ObservedTicketingDeadline` (nullable). `TicketingValidity` stays `NotSupplied` with its unresolved-ownership reason: the observed fact and the authoritative deadline are separate by construction, so no code can mistake one for the other while BD-004 is open. |
| **DB change** | `Order.Orders` gains `ObservedTicketing_Value DATETIMEOFFSET NULL`, `ObservedTicketing_SourceOwner NULL`, `ObservedTicketing_SourceRef NULL` |
| **Mapper change** | when `details.LastTicketingDate` is supplied, emit the observed fact with `SourceOwner = "AirOffer"` and `SourceRef = "details.lastTicketingDate"`; the ticketing `ValidityFact` reason loses the embedded timestamp and states only the unresolved ownership (BD-004) |
| **Projection change** | none |
| **Tests** | a test proving the instant is a typed value and no longer appears inside any reason string; a test proving `TicketingValidity.State` is still `NotSupplied`; preservation vector `LastTicketingDate typed observed fact` |

---

## 15. `OrderItemServiceLink.ScopeAtAssociation` — OD-P-05

| Aspect | Detail |
|---|---|
| **Exact current files** | `Domain/OrderAggregate/Entities/OrderItemServiceLink.cs`, `Domain/OrderAggregate/Order.cs` (`AddItemsAndServices`), `src/AeroTech.Ordering.Persistence/OrderAggregate/OrderItemServiceLinkConfiguration.cs` |
| **Add / change / delete** | add `Entities/OrderItemServiceLinkTraveler.cs`, `Entities/OrderItemServiceLinkSegment.cs` and their configurations; change `OrderItemServiceLink`, `Order.AddItemsAndServices`, `OrderItemServiceLinkConfiguration` |
| **Domain shape** | `OrderItemServiceLink` gains the immutable association scope as **typed child rows** — the beneficiary traveler ids and the covered segment ids as they stood at acceptance — so a later reparenting or split can reconstruct the original association without reading the service's current state. No JSON. `LinkedAt` is **not** added: `LinkedByChangeId` already carries the business change and its committed instant. |
| **DB change** | new tables `Order.OrderItemServiceLinkTravelers` (`LinkId`, `TravelerId`, unique together) and `Order.OrderItemServiceLinkSegments` (`LinkId`, `SegmentId`, unique together); both append-only |
| **Mapper change** | none — the scope comes from the accepted service, not from the wire |
| **Projection change** | none |
| **Tests** | a test proving the scope rows match the service's beneficiaries and coverage at acceptance; a test proving they are not rewritten when the service changes later (asserted with a direct service mutation in the domain test); preservation vector `ItemServiceLink historical scope` |

---

## 16. Public contract: what is deliberately not exposed

The ratified stage-04 API contract is not changed by this plan. Every repair above lands in the domain, SQL and the accepted record; the public `OrderDto` keeps its current shape. Three additive exposures are worth the owner's explicit yes or no, and none is implemented without it:

| Candidate exposure | Why it is worth asking | Default |
|---|---|---|
| `pricing[].code` and `pricing[].name` | a tax code is what an airline user reads first; today the response shows a component enum and an amount only | not applied |
| `itinerary[]` grouped by journey, or `journeyRef` on each segment | outbound/inbound is currently invisible to every caller | not applied |
| `itinerary[].marketingAirlineRef` and terminals/duration/aircraft | today carriers appear only under `airTransport` per service | not applied |

`OpenApiDocumentTests` and `OrderingApiSurfaceTests` stay green unchanged, which is the proof that nothing leaked into the contract.

---

## 17. Migration design

### 17.1 Principles

No destructive reset. Existing accepted S1 rows are preserved. Backfill happens only where existing canonical data proves the value; anything else is recorded as unknown/not-supplied, never invented. Raw source evidence (`Order.PreparationSourceEvidence.Payload`) is never touched, trimmed or re-parsed — re-deriving accepted facts from the raw wire would be a second acceptance, which S1 forbids.

### 17.2 Migration 1 — `S1DomainParityTypedStructures` (additive)

- Adds every column and table listed in §§1–15.
- Nullable wherever the fact may be absent.
- Three non-nullable additions need a deterministic backfill value for existing rows:

| Column | Backfill for existing rows | Justification |
|---|---|---|
| `OrderSegments.JourneyId` | one synthetic journey per existing order, `SourceBoundRef = 'UNKNOWN'`, `Sequence = 1`, `SourceDirectionRaw = NULL`, `Direction = NULL` | the bound structure was never persisted and cannot be recovered from the Order tables; a single unknown journey preserves the FK without asserting a direction. If the owner prefers, `JourneyId` is nullable for legacy rows instead — say which in review. |
| `PricingLines.SourceCalculationKind` | `Amount` | every existing accepted row came from a non-percentage wire row **or** from a percentage row whose accepted monetary value equals the amount-based case; the distinction was not recorded, so `Amount` is the only non-inventing choice available. Rows created after the migration record the true kind. |
| `OrderServices.PriceTreatment` | `SupplierOpaque` | every existing row is an AirOffer air-transport service, for which OD-P-06 fixes exactly this value |

- Everything else stays `NULL` for existing rows: currency code, conversion evidence, product snapshot members, term flags and summaries, baggage, segment terminals/duration/aircraft/capacity ref/carriers, leg airports and times, pricing code/name/reference, observed ticketing deadline. None of them is recoverable from the Order tables, and none is invented.
- The item-level `CommercialTermsSnapshot` of existing rows is written as `Unknown / Unknown / Unknown` with the source system and the order's `CreatedAt` — `Unknown` is exactly "no supplied fact", which is true of those rows.

### 17.3 `PricingUnitsJson` → typed graph

Strict order, three steps, never reversed:

1. **Create** the typed tables (part of migration 1). `PricingUnitsJson` is untouched and still written by the current code path until the domain change ships.
2. **Migrate** in migration 2, `S1DomainParityFareGraphBackfill`: for every `FareConstructions` row, parse the stored canonical JSON and insert one `FarePricingUnits` row per element and one `FareComponents` row per component, using **only** the members the stored JSON actually contains — `sourceUnitRef`, `type`, `combinationMethod`, `coveredSourceBoundRefs`, and per component `sourceFareRef`, `fareBasis`, `fareFamily`, `fareType`, `cabinRef`, `rbdRef`, `bookingClass`. `SourceKindRaw`, `TicketingRestrictionMinutes`, `FareOwnerRef`, `TariffRef`, `RuleRef` and `RoutingRef` are `NULL` — the blob never held them. `CombinationMethod` is rewritten from the stored `ProviderDefined` to `Unspecified`, because OD-P-19 rules that the value was never established by the source; the original blob remains as evidence until step 4. **No `FarePricingGroups` row and no `FareComponentService`/`FareComponentSegment` row is created** — the blob proves neither.
3. **Validate** with the test of §18.4: for every migrated construction, the typed rows reproduce the unit and component facts of its blob, and the row counts match. A single mismatch fails the suite and the drop does not ship.
4. **Drop** `PricingUnitsJson` in a separate migration `S1DomainParityDropPricingUnitsJson`, in its own commit, only after step 3 is green on both a fresh database and an upgraded one.

### 17.4 Both paths tested

`tests/AeroTech.Ordering.Persistence.Tests/S1/MigrationUpgradeTests.cs` already exists and is extended with:

- **fresh**: create an empty database, apply all migrations, assert the model has no pending changes (`dotnet ef migrations has-pending-model-changes`) and that a full create/read round trip works;
- **upgrade**: create a database at the current S1 migration, insert an accepted order through the pre-change code path fixture, apply the new migrations, and assert every existing row survives with the backfill values of §17.2 and the typed graph of §17.3.

---

## 18. Information-preservation test plan

New file `tests/AeroTech.Ordering.Persistence.Tests/S1/InformationPreservationTests.cs`, driven by the existing `AirOfferWireFixtures` plus one richer fixture (two bounds, a multi-leg segment, two travelers, a converted line, a percentage line, baggage on every coupon).

The trace asserted for every vector: **AirOffer wire → `NormalizedCandidate` → `Order` (in memory) → SQL (read back through a fresh `DbContext`) → projection → projection rebuild**.

| # | Vector | Asserted at |
|---|---|---|
| 1 | `CurrencyId` + `CurrencyCode` | candidate, order, SQL |
| 2 | pricing `Code` | candidate, order, SQL |
| 3 | pricing `Name` | candidate, order, SQL |
| 4 | pricing `Reference` | candidate, order, SQL |
| 5 | original and sale values | candidate, order, SQL, projection, rebuild |
| 6 | ROE reference and rate evidence | candidate, order, SQL |
| 7 | `IsPercentage` | candidate, order, SQL |
| 8 | `FareBasis` | candidate, order, SQL |
| 9 | `FareFamily` | candidate, order, SQL |
| 10 | `FareType` | candidate, order, SQL |
| 11 | `TicketingRestrictionMinutes` | candidate, order, SQL |
| 12 | source `PricingUnit.Kind` | candidate, order, SQL |
| 13 | `CoveredBoundOfferIds` | candidate, order, SQL |
| 14 | baggage pieces / weight / unit | candidate, order, SQL |
| 15 | cabin baggage pieces / weight / unit | candidate, order, SQL |
| 16 | `IsRefundable` | candidate, order (service **and** item summary), SQL |
| 17 | `IsChangeable` | as above |
| 18 | `IsUpgradable` | as above |
| 19 | `BoundId` | candidate, order, SQL |
| 20 | raw direction source fact | candidate, order, SQL |
| 21 | `JourneyType` | candidate, order, SQL |
| 22 | `FlightId` | candidate, order, SQL |
| 23 | `FlightVersion` | candidate, order, SQL |
| 24 | `FlightCapacityId` as a source ref | candidate, order, SQL |
| 25 | terminals | candidate, order, SQL |
| 26 | duration | candidate, order, SQL |
| 27 | aircraft | candidate, order, SQL |
| 28 | marketing / operating carrier | candidate, order, SQL |
| 29 | leg airports / terminals / times | candidate, order, SQL |
| 30 | `LastTicketingDate` typed observed fact | candidate, order, SQL |
| 31 | `ItemServiceLink` historical scope | order, SQL, and after a later service mutation |

Two further assertions carry the point of the exercise:

- **18.1 No owner call after acceptance** — the test resolves a candidate, accepts it, then disposes the AirOffer HTTP handler and asserts that reading every vector back from SQL needs no outbound call. The existing fake handler already counts requests; the assertion is "request count unchanged after acceptance".
- **18.2 Nothing silently dropped** — a fixture-driven assertion that every member of `AirOfferDetailsWire` and its children is either mapped into the candidate or listed in an explicit, named "intentionally not consumed" set (`Stop`, and the reconciliation-only ticket/coupon subtotals). A new wire member added later fails this test until it is classified.
- **18.3 Projection determinism** — rebuild the projection twice and assert byte equality, extending the existing `AtomicityAndRebuildTests`.
- **18.4 Fare-graph migration equivalence** — see §17.3 step 3.

---

## 19. Reliability regression plan

Every existing guarantee keeps its existing test, unchanged. Nothing is weakened, deleted or rewritten to fit the new model.

| Guarantee | Existing coverage that must stay green |
|---|---|
| durable idempotency; same key + same payload replays | `S1/CreateOrderFromOfferTests` |
| same key + different payload conflicts | `S1/CreateOrderFromOfferTests` |
| one accepted source consumed once | `S1/CreateOrderFromOfferTests` |
| no remote call inside the SQL transaction | `S1/AtomicityAndRebuildTests`, `Architecture/S1RailTests` |
| one atomic local transaction for order + receipt + projection + outbox | `S1/AtomicityAndRebuildTests` with `_Shared/InterruptibleUnitOfWork` |
| one projector | `Composition/SingleRegistrationGuardTests` |
| deterministic projection rebuild | `S1/AtomicityAndRebuildTests` |
| concurrency / reference uniqueness | `S1/CreateOrderFromOfferTests` |
| source-evidence retention | `S1/AirOfferLiveCandidateBridgeTests` |
| pricing total and sign invariants | `Domain.Tests/OrderAggregate/PricingArithmeticTests`, `AcceptOriginalSaleTests` |
| PII redaction | `Api/OrderingApiSurfaceTests` (`Service_surface_needs_no_token_and_never_returns_traveller_names`) |
| authorization and owner boundaries | `Api/OrderingApiSurfaceTests`, `Host/AnonymousSurfaceInventoryTests` |
| live candidate vs reference simulator separation | `S1/AirOfferLiveOwnerTests`, `Deterministic/DeterministicEffectStoreTests`, plus the new §7 assertion |
| layer and Messages-allowlist boundaries | `Architecture/LayerReferenceTests`, `Architecture/DomainMessagesAllowlistTests` |
| decimal precision of every new monetary/quantity column | `Precision/ModelPrecisionTests`, `Precision/DecimalPrecisionTests` |

Two tests will legitimately need new expected values because the canonical candidate form changes: `Domain.Tests/OrderPreparationAggregate/CandidateCanonicalFormTests` and any fixture asserting a literal digest. Those are expected-value updates, not weakened assertions, and the plan keeps the old vectors alongside the new ones so the schema move itself is visible.

---

## 20. Explicitly excluded from this work

| Excluded | Reason |
|---|---|
| any interpretation of the AirOffer `Stop` node | OD-P-12 — vocabulary unknown; raw evidence retained; **no code is written** |
| reading `AirOfferPricingLineWire.Amount` as a percentage rate | OD-P-18 — meaning not confirmed by any owner contract |
| `PricingLine` quantity, unit of measure and unit price | OD-P-17 — not required by Pack 3.8 and not supplied by the source |
| splitting `PriceChangeSet.SourceDecisionRef` | OD-P-14 — Pack 3.8 defines the field set deliberately |
| contact-point cardinality and country code | OD-P-13 — closed for S1 |
| `OrderItem` quantity and unit of measure | OD-P-03 — rejected |
| a raw pricing-category column | OD-P-20 — closed; the Pack records the CLR values |
| any generic fulfillment-profile engine | OD-P-07 — explicitly forbidden |
| any capacity, reservation, payment, document-issue, refund, exchange or servicing behavior | S2; `S2_NOT_STARTED` |
| any change to the ratified public API contract | §16 — three additive exposures are offered, none applied |

---

## 21. Estimated shape of the change

| Layer | Files added | Files changed |
|---|---|---|
| Contracts (Ordering enums) | 3 new, 2 edited | — |
| Domain | ~14 | ~12 |
| Persistence (configurations + 3 migrations) | ~11 | ~8 |
| Providers (AirOffer) | 0 | 2 |
| Providers.Deterministic | 0 | 2 |
| Query / Synchronizer | 0 | 0 by default (see §16) |
| Tests | 2 | ~6 |

`FareConstruction` is the largest single item: eight new entities, eight configurations and a data migration. Everything else is additive columns on existing tables.

Status: `S1_DOMAIN_REPAIR_PLAN_READY_FOR_OWNER_REVIEW`. `S2_NOT_STARTED`.
