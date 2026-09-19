# S1 Domain Field Inventory

Stage: 07-S1-domain-simplification · **Phase 1, before coding** · 2026-09-19
Branch `k8s-stg` · reviewed baseline HEAD `0f103a3442cf500f71990eb61647017cc99290d7` ("Fixes") · worktree clean

## How the verdicts were reached

Every field below was checked against four things: the **AirOffer wire contract**
(`src/AeroTech.Ordering.Providers/AirOffer/Wire/*.cs`), the **recorded live payload**
(`tests/.../S1/LiveFixtures/airoffer-live-details.json`), the **current consumers** (receiver-qualified search across
`src/` and `tests/`, excluding migrations), and the Pack. A field is `KEEP` only when it has a proven origin, a business
meaning, a current consumer or a stable identity that cannot be safely reconstructed, and a reason to live in Domain.
"It may be needed later" is not a reason.

## Source facts established in Phase 1

These are measured, not assumed.

| Wire field | C# wire type | Recorded live value | Current Domain type | Note |
|---|---|---|---|---|
| `currencyId` | `int` | `70` | `string CurrencyRef` | stringified |
| `currencyCode` | `string?` | `"IRR"` | `string? CurrencyCode` | ReferenceData display value |
| `journeyType` | `string?` | `"RoundTrip"` | `string? SourceJourneyTypeRaw` **+** `JourneyType?` | dual-modelled; enum exists |
| `airTransports[].boundId` | `string` | `"B1"` | `string SourceBoundRef` | **genuinely opaque** — correctly a Ref |
| `airTransports[].direction` | `JsonElement` | `1`, `2` | `string? SourceDirectionRaw` **+** `BoundDirection?` | dual-modelled; enum exists |
| `airTransports[].sequence` | `int` | `1`, `2` | `int Sequence` | preserved for journeys |
| `originAirportId` / `destinationAirportId` | `int` | numeric | `string OriginRef` / `DestinationRef` | stringified |
| `originAirportTerminalId` / `destination…` | `int?` | numeric | `string? …TerminalRef` | stringified |
| `flights[].sequence` | `int` | `1` | **dropped** | `Order.AddSegments` writes `index + 1` |
| `flightId` | `long` | `"1544009211417985722"` | `string? FlightRef` | stringified |
| `flightVersion` | `int` | `1` | `string? FlightVersion` | stringified |
| `flightCapacityId` | `long` | `"1544009211417985747"` | `string? SourceCapacityRef` | stringified |
| `operatingAirlineId` / `marketingAirlineId` | `int` | numeric | `string? …CarrierRef` | stringified |
| `aircraftId` | `int` | `1` | `string? AircraftRef` | stringified |
| `cabinClassId` | `int?` | `1` | `string? CabinRef` | stringified |
| `rbdId` | `long?` | `"25"` | `string? RbdRef` | stringified |
| `legs[].legId` | `long` | `"1544009211417985721"` | `string SourceLegRef` | stringified |
| `legs[].sequence` | `int` | `1` | `int Sequence` | preserved |
| `pricingUnits[].kind` | `string?` | `"OneWay"` | `FarePricingUnitType.Unspecified` **+** `SourceKindRaw` | **vocabulary exists and is discarded** |
| `pricingUnits[].coveredBoundOfferIds` | `List<string>` | `["1544009211417985722"]` | `string SourceBoundRef` on `FarePricingUnitCoveredBound` | **see BLOCKED_REAL_CONTRACT note below** |
| `fareComponents[].airFareId` | `long` | `"1469435125056929792"` | `string SourceFareRef` | stringified |
| `fareComponents[].ticketingRestrictionMinutes` | `int?` | `null` | `int?` | correct |
| `tickets[].travellerRef` | `string` | opaque | `string SourceTravellerRef` | correctly a Ref |
| `coupons[].couponId` | `string` | opaque | **not persisted** | ratified `KEEP` decision |
| `ratesOfExchange[].fromCurrencyId` / `toCurrencyId` | `int` | numeric | `string From/ToCurrencyRef` | stringified |

**Numeric transport note.** Large ids arrive as JSON *strings* (`"1544009211417985722"`) for precision safety, and the
wire DTOs already deserialize them into `long`/`int`. The stringification defect is therefore introduced by the
**mapper**, not by the owner: the typed value exists and is converted back into a string.

### `BLOCKED_REAL_CONTRACT` — `coveredBoundOfferIds`

In the recorded live payload, `pricingUnits[0].coveredBoundOfferIds = ["1544009211417985722"]`, which is **exactly the
`flightId` of the first flight** — not the `boundId` (`"B1"`) of any bound. The field name says bound, the value is a
flight identity. Ordering cannot safely decide which namespace it is, and joining it to either would be an invented
semantic.

**Disposition:** keep the owner's exact name and treat the values as opaque strings (`CoveredBoundOfferId`). Do **not**
rename to a flight id, do **not** create an FK, do **not** join it to `boundId`. This is raised to the owner as an
open contract question; it does not block the rest of the simplification. Existing handoff `OR-002` is the right home.

---

## 1. `Order` (aggregate root)

| Field | Origin | Business meaning | Current consumer / invariant | Persistence reason | Verdict |
|---|---|---|---|---|---|
| `Id` | local snowflake | order identity | everything | identity | **KEEP** |
| `OrderReference` | generated | customer-facing reference | unique index, public DTO | identity | **KEEP** |
| `RootOrderId` | self at create | split lineage root | unique/root CHECK; S14 anchor | stable identity that cannot be reconstructed after a split | **KEEP** |
| `OwnerAirlineId` | `IHomeOperatorProvider` | owning airline | every authorized read, all indexes | identity | **KEEP** |
| `FinancialCustomerId` | scope | who is billed | authorization, indexes | identity | **KEEP** |
| `SalesContext` | scope | channel + seller + selling office, frozen | closure tests; public office contract | immutable history | **KEEP** |
| `Buyer` | scope | who bought | **no surface supplies one**; every current row is both-null | none today | **DEFER_TO_LATER_SLICE** — remove from S1 Domain; Pack requirement re-opens when a surface supplies a buyer |
| `InitiatingActor` | scope | who pressed the button | `OrderChange` actor, audit | immutable history | **KEEP** |
| `Channel` | derived from `SalesContext` | convenience | projection builder | EF-ignored | **KEEP** (derived) |
| `SellingOfficeKind` / `SellingOfficeId` | derived | convenience | projection builder | EF-ignored | **KEEP** (derived) |
| `SaleCurrency` (`CurrencySnapshot`) | candidate | sale currency + code | `Money` currency comparisons | code duplicates ReferenceData | **RETYPE** → `CurrencyId : int`; `CurrencyCode` moves to evidence/enrichment |
| `AcceptedSource` (14-member VO) | preparation | accepted-source provenance | digest assertions, sandbox check | mega-wrapper duplicating the preparation row | **MOVE_TO_EVIDENCE** — reduce to the source-snapshot link + the few facts with consumers |
| `AcceptedSource.PreparationId` | preparation | duplicate of `SourcePreparationId` | — | duplicate | **DELETE** |
| `SourcePreparationId` | preparation | accepted source link | unique index, FK | identity | **KEEP** |
| `OfferValidity` (`ValidityFact`) | candidate | offer expiry state | `EnsureAcceptable` reads the **preparation** copy, not this one | none on the committed Order | **RETYPE** → `OfferExpiresAt?` |
| `PriceValidity` | candidate | price expiry | same | none | **RETYPE** → `PriceValidUntil?` |
| `TicketingValidity` | candidate | ticketing deadline state | **none** — always `NotSupplied`, owner `"Unresolved owner"`, reason text | produces `TicketingValiditySourceRef` with no value | **DELETE** |
| `ObservedTicketingDeadline` (`ObservedTimeFact`) | `lastTicketingDate` | observed ticketing deadline | live test | wrapper adds `SourceOwner`/`SourceRef` constants | **RETYPE** → `LastTicketingDate?` |
| `SourceJourneyTypeRaw` | `journeyType` | raw string | round-trip assertions only | duplicates the enum | **DELETE** |
| `JourneyType` | `journeyType` | whole-trip classification | projection, public DTO | not always derivable from bounds | **RETYPE** → non-nullable, fail closed on unknown |
| `CommercialSummary` | derived at accept | Active/Inactive | index, public DTO | derived-but-persisted per Pack | **KEEP** |
| `CustomerTotal` | `PricingArithmetic` | payable total | INV-012/013, public DTO | derived-but-persisted per Pack | **KEEP** |
| `CommercialVersion` | 1 at create | commercial revision | `OrderChange`, events, projection | concurrency/lineage | **KEEP** |
| `FinancialSequence` | 1 at create | price-set sequence | `PriceChangeSet`, events | lineage | **KEEP** |
| `OrderRevision` | 1 at create | projection staleness guard | `OrderProjector.ReplaceAsync` | concurrency | **KEEP** |
| `LastEventOrdinal` | 1 at create | outbox ordering | domain event, outbox | ordering | **KEEP** |
| `ClientReference` | request | caller's own reference | public/read | caller fact | **KEEP** |
| `CreatedAt` | clock | acceptance time | index, DTO | history | **KEEP** |
| `ComponentTotals` | derived from lines | component summary | projection only | fabricated `Entity<long>` id via `ROW_NUMBER` | **MOVE_TO_QUERY** — derive in the projector; if persistence is later proven, natural key `(OrderId, Component, Effect)` outside the entity graph |
| `IsSandboxScoped` | `AcceptedSource` | sandbox acceptance profile | **one test assertion** | EF-ignored derived | **KEEP** (derived, no column) |
| child collections | — | — | see below | — | per child |

## 2. `OrderJourney`

| Field | Origin | Meaning | Consumer | Verdict |
|---|---|---|---|---|
| `Id` | local | row identity | segments FK | **KEEP** |
| `OrderId` | parent | owner | FK | **KEEP** |
| `SourceBoundRef` | `boundId` `"B1"` | owner's bound identity | segment grouping | **RENAME** → `BoundId` (string; genuinely opaque) |
| `Sequence` | `airTransports[].sequence` | bound order | ordering | **KEEP** |
| `SourceDirectionRaw` | `direction` | raw | none beyond echo | **DELETE** |
| `Direction` | `direction` `1/2` | outbound/inbound | projection | **RETYPE** → non-nullable `BoundDirection`, fail closed |
| `OriginRef` / `DestinationRef` | `originAirportId` / `destinationAirportId` `int` | airports | projection | **RETYPE** → `OriginAirportId` / `DestinationAirportId` : `int` |

## 3. `OrderSegment`

| Field | Origin | Meaning | Consumer | Verdict |
|---|---|---|---|---|
| `Id` | local | identity | services, legs, fare components | **KEEP** |
| `OrderId` | parent | owner | FK | **KEEP** |
| `JourneyId` | journey map | bound membership | projection | **RETYPE** → required (`long`), not nullable |
| `Sequence` | **fabricated** `index + 1` | flight order | ordering | **RETYPE** → take `flights[].sequence`; uniqueness `(JourneyId, Sequence)` |
| `SourceSegmentRef` | **fabricated** `"{boundId}\|{flightId}"` | none | candidate correlation, unique index | **DELETE** from Domain; keep as a candidate-only `SegmentKey` |
| `Kind` | mapper constant | scheduled air | validator | **KEEP** |
| `OriginRef` / `OriginTerminalRef` / `DestinationRef` / `DestinationTerminalRef` | `int` / `int?` | airports/terminals | projection | **RETYPE** → `…AirportId : int`, `…AirportTerminalId : int?` |
| `SoldDeparture` / `SoldArrival` | `departureDateTime` / `arrivalDateTime` | sold schedule | projection | **KEEP** |
| `FlightRef` | `flightId : long` | flight identity | projection | **RETYPE** → `FlightId : long?` |
| `FlightNumber` | `flightNumber` | marketing number | public DTO | **KEEP** |
| `FlightVersion` | `flightVersion : int` | schedule version | S11 | **RETYPE** → `int?` |
| `MarketingCarrierRef` / `OperatingCarrierRef` | `int` | airlines | interline identity | **RETYPE** → `MarketingAirlineId` / `OperatingAirlineId` : `int?` |
| `SourceCapacityRef` | `flightCapacityId : long` | capacity identity | S2 | **RETYPE** → `FlightCapacityId : long?` |
| `Duration` | `duration : int` | minutes | display | **KEEP** |
| `AircraftRef` | `aircraftId : int` | aircraft | display | **RETYPE** → `AircraftId : int?` |
| `Legs` | wire | physical legs | technical-stop invariant | **KEEP** |

## 4. `OrderSegmentLeg`

| Field | Origin | Meaning | Consumer | Verdict |
|---|---|---|---|---|
| `Id` | local | row identity | projection | **KEEP** |
| `SegmentId` | parent | owner | FK | **KEEP** |
| `Sequence` | `legs[].sequence` | leg order | ordering | **KEEP** |
| `SourceLegRef` | `legId : long` | owner leg identity | projection | **RETYPE** → `LegId : long` |
| `OriginRef` / terminals / `DestinationRef` | `int` / `int?` | airports | projection | **RETYPE** → typed ids |
| `Departure` / `Arrival` | wire | leg schedule | projection | **RENAME** → `DepartureDateTime` / `ArrivalDateTime` |
| *(`Stop`)* | `JsonElement? Stop` | unknown vocabulary | **not mapped** | **BLOCKED_REAL_CONTRACT** — stays unmapped (`OD-P-12`, `OR-002`) |

## 5. `OrderTraveler` → `OrderTraveller`

| Field | Origin | Meaning | Consumer | Verdict |
|---|---|---|---|---|
| `Id` | local | identity | services, fare groups | **KEEP** |
| `OrderId` | parent | owner | FK | **KEEP** |
| `SourceTravellerRef` | `travellerRef` | owner's traveller ref | candidate binding | **KEEP** (genuinely opaque) |
| `ClientTravelerRef` | request | caller's ref | binding, public DTO | **RENAME** → `ClientTravellerRef` |
| `PassengerTypeCode` | `passengerTypeCode` | PTC | binding invariant | **KEEP** |
| `InfantParentTravelerId` | guardian binding | infant→adult link | binding policy | **RENAME** → `InfantParentTravellerId` |
| `Identity` (`TravelerIdentity`) | request | protected PII | protected read | **RENAME** type → `TravellerIdentity` |
| *class + all `Traveler*` types* | — | — | — | **RENAME** → `Traveller` spelling throughout |

## 6. `OrderContact`

All five fields (`Id`, `OrderId`, `Sequence`, `Role`, protected `Email`/`Phone`) — **KEEP**. Already minimal.

## 7. `OrderItem`

| Field | Origin | Meaning | Consumer | Verdict |
|---|---|---|---|---|
| `Id` | local | identity | services, lines, obligations | **KEEP** |
| `OrderId` | parent | owner | FK | **KEEP** |
| `SourceItemRef` | **fabricated** `"OFFER-PACKAGE"` | none | candidate correlation, unique index | **DELETE** from Domain; candidate-only `ItemKey` |
| `Kind` | mapper constant | package / product / charge | fee-only invariant | **KEEP** |
| `SourceOfferItemRef` | **never supplied** by AirOffer | owner's offer-item id | always null | **DEFER_TO_LATER_SLICE** |
| `AcceptedTotal` | lines | accepted item value | Pack `DOMAIN/02` §7 | **KEEP** |
| `Product` (`ProductSnapshot`, 8 members) | mapper | product identity | only `SourceSystem` + `SourceOfferId` populated, both duplicating `AcceptedSource` | **DELETE** — re-add exact facts when an owner supplies product/brand |
| `CommercialTerms` (`CommercialTermsSnapshot`, 7 members) | derived from service `SoldTermFlags` | display terms | `Summarize()` only | **DERIVE** — remove persisted copy; keep the per-service flags |
| `CreatedByChangeId` | change | provenance | audit | **KEEP** |
| `CommercialStatus` | constant `Active` | lifecycle | S5 | **KEEP** |

## 8. `OrderService` → typed AirTransport service

| Field | Origin | Meaning | Consumer | Verdict |
|---|---|---|---|---|
| `Id`, `OrderId`, `OrderItemId`, `CommercialStatus`, `CreatedByChangeId` | — | base service | invariants, projection | **KEEP** |
| `SourceServiceRef` | **fabricated** `"{travellerRef}\|{segmentRef}"` | none | unique index | **DELETE**; candidate-only `ServiceKey` |
| `Type` | constant `AirTransportation` | service type | registry gate | **DEFER_TO_LATER_SLICE** — one type in S1 |
| `ServiceCode` / `Name` | **never supplied** | — | always null | **DEFER_TO_LATER_SLICE** |
| `PriceTreatment` | constant `SupplierOpaque` | pricing treatment | none | **DEFER_TO_LATER_SLICE** |
| `SupplierPartyRef` / `DeliveryProviderRef` | **never supplied** | — | always null | **DEFER_TO_LATER_SLICE** |
| `ServiceVersion` | constant `1` | mutation counter | nothing mutates services in S1 | **DEFER_TO_LATER_SLICE** |
| `Quantity` / `QuantityUnit` | constant `1` / `Each` | quantity | none | **DEFER_TO_LATER_SLICE** |
| `DetailSchema` / `DetailSchemaVersion` | constants | schema gate | `ServiceDetailSchemaRegistry` | **DELETE** with the registry |
| `SoldTerms` (`SoldTermFlags`) | coupon `isRefundable/Changeable/Upgradable` | sold terms | item summary, projection | **KEEP** — move onto the typed AirTransport detail |
| `FulfillmentProfile` (12 members) | mapper constants + `Unresolved` | fulfillment policy | `IsCertified` has **no consumer** | **DEFER_TO_LATER_SLICE** — invented profile ref `AIROFFER-OBSERVED-AIR-UNCERTIFIED`, no certified owner contract |
| `AirTransport` (`AirTransportDetail`) | coupon/flight | cabin/RBD/class/baggage | projection, public DTO | **KEEP**, retyped: `CabinClassId : int?`, `RbdId : long?`, `BookingClass`, baggage |
| `Beneficiaries` | traveller map | who the service is for | exactly one in S1 | **RETYPE** → `TravellerId` on the typed service |
| `Coverage` | segment map | which segment | exactly one in S1 | **RETYPE** → `SegmentId` on the typed service |
| `SoleCoveredSegmentId` | derived | workaround for the above | **no consumer** | **DELETE** |

## 9. Pure relation rows

| Type | Independent lifecycle? | Verdict |
|---|---|---|
| `OrderServiceBeneficiary` | no | **DELETE** — becomes `TravellerId` on the typed service |
| `OrderServiceCoverage` | no | **DELETE** — becomes `SegmentId` on the typed service |
| `FareConstructionItem` | no | **RETYPE** → composite key `(FareConstructionId, OrderItemId)` |
| `FarePricingGroupTraveler` | no | **DELETE** with `FarePricingGroup` |
| `FareComponentService` / `FareComponentSegment` | no | **DELETE** — source supplies no component↔service/segment binding |
| `FarePricingUnitCoveredBound` | no | **RETYPE** → composite key `(PricingUnitId, CoveredBoundOfferId)` |
| `OrderItemServiceLinkTraveler` / `…Segment` | no | **DELETE** with the link, or composite key if the link is kept |
| `OrderItemServiceLink` | historical membership has identity | **DEFER_TO_LATER_SLICE** — nothing in S1 changes item membership; `ScopeAtAssociation` is an S14 need. If the owner wants it pre-seeded, keep `LinkId` and give the two child tables composite keys |

## 10. `OrderChange`, `PriceChangeSet`

| Field | Origin | Consumer | Verdict |
|---|---|---|---|
| `OrderChange.Id`, `OrderId`, `Type`, `CommercialVersion`, `ActorContextType`, `ActorId`, `CommittedAt` | — | audit, FK | **KEEP** |
| `OrderChange.SourceDecisionRef` | **fabricated** `"preparation:{id}:{digest}"` | none | **DELETE** — not an external decision identity |
| `PriceChangeSet.Id`, `OrderId`, `ChangeId`, `FinancialSequence`, `Reason`, `CommittedAt` | — | lines, audit | **KEEP** |
| `PriceChangeSet.SourceDecisionRef` | same fabricated string | none | **DELETE** |
| `PriceChangeSet.BaseCommercialVersion` | constant `0` | **no consumer** (only the legacy SQL fixture) | **DEFER_TO_LATER_SLICE** |

## 11. `PricingLine`

| Field | Origin | Consumer | Verdict |
|---|---|---|---|
| `Id`, `OrderId`, `PriceChangeSetId`, `OrderItemId` | — | arithmetic, obligations | **KEEP** |
| `SourceLineRef` | `"tickets/0/coupons/0/pricings/1"` | audit path | **RENAME** → `SourceOccurrencePath` (the one retained occurrence) |
| `CandidateLineRef` | same value | unique index `(PriceChangeSetId, CandidateLineRef)` | **DELETE** — duplicate of the above; move the unique index onto `SourceOccurrencePath` |
| `Component`, `Effect`, `Direction` | wire category + rules | INV-013, matrix, SQL checks | **KEEP** |
| `SourceCode`, `SourceName`, `SourceReference` | `code`, `name`, `reference` | audit | **RENAME** → `Code`, `Name`, `Reference` (they are the source's own values) |
| `CalculationKind` | `isPercentage` | percentage provenance | **KEEP** |
| `OriginalValue`, `SaleValue` | `amount` / `equivalentAmount` | INV-012, arithmetic | **KEEP**, retyped to `CurrencyId` |
| `BasisType`, `BasisId` | mapper | what the line prices | projection | **KEEP** |
| `SourceBasisRef` | candidate key | resolved into `BasisId` at accept | **DELETE** |
| `SourceConversionRef` | `rateOfExchangePeriodId` | FX provenance | **KEEP** (genuinely opaque owner ref) |
| `AppliedConversion` | rate row | FX evidence | **KEEP**, retyped to `FromCurrencyId`/`ToCurrencyId` |
| `SettlementAttribution` | source settlement fact | INV-013, SQL check | **KEEP** |
| `Role` (`PricingLineRole`) | constant `Original` | reversal CHECK only; reversals deferred | **DEFER_TO_LATER_SLICE** |
| `OriginalPricingLineId` | always null | **no consumer** beyond its own CHECK/FK | **DEFER_TO_LATER_SLICE** |

## 12. `OrderComponentTotal`

| Field | Verdict |
|---|---|
| `Id` (`ROW_NUMBER` in migration, snowflake at runtime) | **DELETE** — fabricated identity |
| `OrderId`, `Component`, `Effect`, `DebitAmount`, `CreditAmount`, `CurrencyRef` | **MOVE_TO_QUERY** — derive in the projector from committed lines; `CurrencyRef` retypes to `CurrencyId` if it is ever persisted |

## 13. `FundingObligation`

| Field | Origin | Consumer | Verdict |
|---|---|---|---|
| `Id`, `OrderId`, `Version`, `Purpose`, `Amount`, `OrderItemId`, `ChangeId` | accepted sale | stable original-sale liability | **KEEP** |
| `OrderServiceId`, `PricingLineId` | S1 never produces them | exactly-one CHECK | **DEFER_TO_LATER_SLICE** — revert to item scope; the CHECK becomes `OrderItemId IS NOT NULL` |
| `SourceDecisionRef` | fabricated preparation string | none | **DELETE** |
| `SupersededObligationId` | always null | none | **DEFER_TO_LATER_SLICE** |

## 14. Fare construction

| Field | Origin | Consumer | Verdict |
|---|---|---|---|
| `FareConstruction.Id`, `OrderIdAtCreation`, `CreatedByChangeId` | — | identity | **KEEP** |
| `FareConstruction.Assurance` | constant `Opaque` for AirOffer | gates "opaque construction cannot claim links" | **KEEP** only if it still gates behaviour after the graph is trimmed; otherwise **DELETE** |
| `FareConstruction.SourceContextRef` | **fabricated** `"airoffer:details:pricingUnits"` | none | **DELETE** |
| `FareConstruction.SupersededByConstructionId` | always null | filtered index only | **DEFER_TO_LATER_SLICE** |
| `FareConstructionItem` | link | item coupling | **RETYPE** → composite key |
| `FarePricingGroup` (+`Traveler`) | **never supplied** by AirOffer | always empty | **DEFER_TO_LATER_SLICE** |
| `FarePricingUnit.Sequence` | index | order | **KEEP** |
| `FarePricingUnit.SourceUnitRef` | **fabricated** `"pricingUnits/{i}"` | none | **DELETE** |
| `FarePricingUnit.SourceKindRaw` | `kind` `"OneWay"` | echo | **DELETE** |
| `FarePricingUnit.Type` | discarded `kind` | projection | **RETYPE** → map `"OneWay"` → `FarePricingUnitType.OneWay`, fail closed |
| `FarePricingUnit.CombinationMethod` | **never supplied**, constant `Unspecified` | none | **DEFER_TO_LATER_SLICE** |
| `FarePricingUnitCoveredBound.SourceBoundRef` | `coveredBoundOfferIds` | RT-vs-2OW evidence | **RENAME** → `CoveredBoundOfferId`, composite key, **namespace left opaque** (see blocked note) |
| `FareComponent.SourceFareRef` | `airFareId : long` | audit | **RETYPE** → `AirFareId : long` |
| `FareComponent.FareBasis`, `FareFamily`, `FareType`, `BookingClass`, `TicketingRestrictionMinutes` | supplied | audit/display | **KEEP** |
| `FareComponent.CabinRef` / `RbdRef` | `cabinClassId : int?` / `rbdId : long?` | display | **RETYPE** → `CabinClassId : int?`, `RbdId : long?` |
| `FareComponent.FareOwnerRef`, `TariffRef`, `RuleRef`, `RoutingRef` | **never supplied** | always null | **DEFER_TO_LATER_SLICE** |
| `FareComponent.CoveredServices` / `CoveredSegments` | **never supplied** | always empty | **DEFER_TO_LATER_SLICE** |

## 15. `OrderPreparation` (accepted-source snapshot)

Proven in `CreateOrderFromOfferService`: `CaptureAcceptedSource` → `Order.AcceptOriginalSale` → `preparation.Consume(order.Id, now)`
→ **one** `SaveChangesAsync`. A preparation can therefore **never** commit unconsumed in the current one-command S1 flow.

| Field | Consumer | Verdict |
|---|---|---|
| `Id`, `CandidateJson`, `CanonicalizationVersion`, `SnapshotDigest`, `SourcePayloadHash`, `CapturedAt`, `Evidence` | acceptance proof, digest tests | **KEEP** |
| `ProviderProfileId`, `ContractVersion`, `AcceptanceProfile`, `AcceptanceAssurance` | acceptance policy, sandbox refusal | **KEEP** |
| `SourceOwner`, `SourceOfferId` | provenance, index | **KEEP** |
| `OwnerBindingRef` | owner-bound assurance invariant | **KEEP** |
| `PricedAt` | source pricing time | **KEEP** |
| `OwnerAirlineId`, `FinancialCustomerId` | scope check at accept | **KEEP** |
| `Channel`, `SellingOfficeId`, `ActorContextType`, `ActorId`, `CallerScope` | duplicated from the candidate/scope; only `CallerScope` has an index | **MOVE_TO_EVIDENCE** — keep `CallerScope` (idempotency), drop the rest |
| `OfferValidity`, `PriceValidity`, `TicketingValidity` | `EnsureAcceptable` reads offer+price | **RETYPE** → `OfferExpiresAt?`, `PriceValidUntil?`; ticketing **DELETE** |
| `ValidityFacts` (derived list) | none | **DELETE** |
| `ClientReference` | duplicate of `Order.ClientReference` | **DELETE** |
| `ConsumedByOrderId`, `ConsumedAt`, `IsConsumed`, `Consume()` | a state that cannot occur | **DELETE** |
| `CreatedAt` | equals `CapturedAt` in the current flow | **DELETE** unless a divergence is proven |

## 16. `CommandReceipt`

| Field | Consumer | Verdict |
|---|---|---|
| `Id`, `OwnerAirlineId`, `FinancialCustomerId`, `CallerScope`, `CommandKind`, `IdempotencyKey`, `CanonicalizationVersion`, `RequestDigest`, `ResultJson`, `CreatedAt` | idempotency and replay | **KEEP** |
| `OrderId` | replay result lookup | **KEEP** |
| `Status` | **only `Completed` is constructible** | **DELETE** (and move `CommandReceiptStatus` out of shared public Contracts) |
| `CompletedAt` | always equals `CreatedAt` | **DELETE** |
| `OperationId` | no current behaviour reads it | **DEFER_TO_LATER_SLICE** |
| `PreparationId` | reachable through `Order.SourcePreparationId` | **DELETE** |

## 17. Shared value objects

| Type | Verdict |
|---|---|
| `Money(Amount, CurrencyRef)` | **RETYPE** → `Money(Amount, CurrencyId : int)` |
| `AppliedConversion` | **RETYPE** → `FromCurrencyId`/`ToCurrencyId : int`; keep `Rate`, `DecimalPlaces`, `RoundingToken`, `SourceConversionRef` |
| `CurrencySnapshot` | **DELETE** — `CurrencyId` on Order; `CurrencyCode` is ReferenceData enrichment |
| `ValidityFact` | **DELETE** — replaced by explicit nullable instants |
| `ObservedTimeFact` | **DELETE** — replaced by `LastTicketingDate?` |
| `BuyerSnapshot` | **DEFER_TO_LATER_SLICE** |
| `ProductSnapshot` | **DELETE** for the current AirOffer item |
| `CommercialTermsSnapshot` | **DERIVE** |
| `FulfillmentProfileSnapshot` | **DEFER_TO_LATER_SLICE** |
| `FundingObligationScope` | **DEFER_TO_LATER_SLICE** with service/line scope |
| `SalesContextSnapshot`, `InitiatingActorSnapshot`, `SettlementAttribution`, `BaggageAllowance`, `SoldTermFlags`, `TravelerIdentity`, `TravelerBinding`, `ContactDetails`, `CustomerRelationship`, `AuthorizedSalesScope` | **KEEP** (`Traveler*` renamed to `Traveller*`) |
| `AuthorizedSalesScope.Channel/SellingOfficeId/ActorContextType/ActorId` | **KEEP** as derived getters (compatibility shims over the snapshots) |

## 18. Legacy compatibility to remove from Domain

| Item | Verdict |
|---|---|
| `Domain/_Shared/Policies/LegacySellingOfficePolicy` | **MOVE_TO_QUERY** or delete with the rebaseline — it is migration/read compatibility, not Domain vocabulary |
| `SellingOfficeKind.NotRecorded` | **DELETE** if the migration rebaseline removes the only rows that needed it |
| `Query/.../Projection/Compatibility/*` (schema 2 and 3) | **DELETE** if no deployed S1 data must be preserved |
| `NormalizedCandidate` schema `3.0` reader/writer | **DELETE** on the same condition |
| Projection schema 4 → schema 1 | rebaseline candidate |

## Verdict counts

| Verdict | Count |
|---|---|
| KEEP | 78 |
| RETYPE | 34 |
| RENAME | 12 |
| DELETE | 33 |
| DEFER_TO_LATER_SLICE | 27 |
| DERIVE | 2 |
| MOVE_TO_QUERY | 3 |
| MOVE_TO_EVIDENCE | 2 |
| BLOCKED_REAL_CONTRACT | 2 |

## Blocking questions for the owner

1. **`coveredBoundOfferIds` namespace** — the recorded value is a `flightId`, not a `boundId`. Kept opaque; needs an
   owner answer before it can ever be joined. *(Does not block the rest of the work.)*
2. **`Stop`** on flight and leg — unchanged, still `OD-P-12` / handoff `OR-002`.
3. **Is there any deployed S1 database that must preserve the current unapproved schema?** This single answer decides
   whether §18 is a **migration rebaseline** (delete all compatibility code and squash S1 migrations) or an
   **upcaster** (keep compatibility at the migration/query boundary only). Phase 2 cannot start on §18 without it.

Everything except §18 can proceed on the evidence above.

Status: `S1_DOMAIN_SIMPLIFICATION_NOT_READY` (Phase 1 inventory complete, no code changed)
`S2_NOT_STARTED`
