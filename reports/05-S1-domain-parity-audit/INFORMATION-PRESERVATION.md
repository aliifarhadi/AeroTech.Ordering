# S1 Domain Parity — Information Preservation

Stage: 05-S1-domain-parity-audit · 2026-09-19

The chain proven end to end:

`AirOffer wire → NormalizedCandidate (schema 3.0) → Order → SQL → internal typed projection → projection rebuild`

Tests: `tests/AeroTech.Ordering.Persistence.Tests/S1/InformationPreservationTests.cs` (5 facts, all green) and
`tests/AeroTech.Ordering.Persistence.Tests/S1/ProjectionSchemaTransitionTests.cs` (1 fact, green).

## 1. Every previously discarded AirOffer fact and its canonical destination

| AirOffer wire member | Before this repair | Canonical destination now |
|---|---|---|
| `currencyCode` | discarded | `Orders.SaleCurrencyCode` (owned `CurrencySnapshot`) |
| `journeyType` | discarded | `Orders.SourceJourneyTypeRaw` verbatim; canonical `Orders.JourneyType` stays `NULL` until `OR-002` answers |
| `lastTicketingDate` | embedded in a prose validity reason | `Orders.ObservedTicketingDeadline*` (typed `ObservedTimeFact`); the authoritative deadline stays `NotSupplied` under BD-004 |
| `airTransports[].boundId` | discarded | `OrderJourneys.SourceBoundRef` |
| `airTransports[].sequence` | discarded | `OrderJourneys.Sequence` |
| `airTransports[].direction` | discarded | `OrderJourneys.SourceDirectionRaw` verbatim; canonical `Direction` stays `NULL` until `OR-002` answers |
| `airTransports[].originAirportId` / `destinationAirportId` | discarded | `OrderJourneys.OriginRef` / `DestinationRef` |
| `flights[].flightId` | kept | `OrderSegments.FlightRef` — the existing column, reused, **not** duplicated |
| `flights[].flightNumber` | service detail only | `OrderSegments.FlightNumber` (single owner) |
| `flights[].flightVersion` | service detail only | `OrderSegments.FlightVersion` (single owner) |
| `flights[].marketingAirlineId` / `operatingAirlineId` | service detail only | `OrderSegments.MarketingCarrierRef` / `OperatingCarrierRef` (single owner) |
| `flights[].flightCapacityId` | discarded | `OrderSegments.SourceCapacityRef` — a retained source reference, not certified capacity truth (BD-002/BD-003 still govern) |
| `flights[].originAirportTerminalId` / `destinationAirportTerminalId` | discarded | `OrderSegments.OriginTerminalRef` / `DestinationTerminalRef` |
| `flights[].duration` | discarded | `OrderSegments.Duration` |
| `flights[].aircraftId` | discarded | `OrderSegments.AircraftRef` |
| `flights[].cabinClassId` / `rbdId` / `bookingClass` | service detail | unchanged: `AirTransportServiceDetails.CabinRef` / `RbdRef` / `BookingClass` — passenger-level facts stay there |
| `legs[].legId` | kept as a bare ref | `OrderSegmentLegs.SourceLegRef` (unchanged) |
| `legs[].originAirportId` / `destinationAirportId` | discarded | `OrderSegmentLegs.OriginRef` / `DestinationRef` |
| `legs[].originAirportTerminalId` / `destinationAirportTerminalId` | discarded | `OrderSegmentLegs.OriginTerminalRef` / `DestinationTerminalRef` |
| `legs[].departureDateTime` / `arrivalDateTime` | discarded | `OrderSegmentLegs.Departure` / `Arrival` |
| `flights[].stop`, `legs[].stop` | unread | still unread, by `OD-P-12`. Raw response retained in `PreparationSourceEvidence.Payload`; **no** canonical connection row, not even `Unknown`. Handoff `OR-002`. |
| `coupons[].baggagePieces` / `baggageWeight` / `baggageUnit` | discarded | `AirTransportServiceDetails.CheckedBaggage*` (typed `BaggageAllowance`) |
| `coupons[].cabinBaggagePieces` / `cabinBaggageWeight` / `cabinBaggageUnit` | discarded | `AirTransportServiceDetails.CabinBaggage*` |
| `coupons[].isRefundable` / `isChangeable` / `isUpgradable` | discarded | exact per-service flags in `OrderServices.SoldTerm*`, **and** the item summary in `OrderItems.Terms*` |
| `pricings[].name` | discarded | `PricingLines.SourceName` |
| `pricings[].code` | discarded | `PricingLines.SourceCode` |
| `pricings[].reference` | discarded | `PricingLines.SourceReference` |
| `pricings[].isPercentage` | discarded | `PricingLines.CalculationKind` (`Amount` / `Percentage`); the numeric basis stays uninterpreted |
| `ratesOfExchange[]` | only the period id survived | `PricingLines.Conversion*` — the referenced rate row's own from/to identity, rate, decimal places and rounding token |
| `pricingUnits[].kind` | discarded | `FarePricingUnits.SourceKindRaw` verbatim; canonical `Type` stays `Unspecified` |
| `pricingUnits[].coveredBoundOfferIds` | discarded | `FarePricingUnitCoveredBounds.SourceBoundRef` |
| `fareComponents[].ticketingRestrictionMinutes` | discarded | `FareComponents.TicketingRestrictionMinutes` |
| `fareComponents[]` fare basis / family / type / cabin / RBD / booking class | JSON blob | typed `FareComponents` columns |
| `tickets[].baseAmount` / `chargeAmount` / `totalAmount` | reconciliation only | unchanged — derivable by summing that traveler's lines |

## 2. Facts the source does not supply, and which are therefore absent by design

`OrderServices.ServiceCode`, `Name`, `SupplierPartyRef`, `DeliveryProviderRef`; `OrderItems` product code/name/brand/version; `FareComponents.FareOwnerRef` / `TariffRef` / `RuleRef` / `RoutingRef`; `FarePricingGroups` and `FareComponentService`/`FareComponentSegment` rows. Each is a column or table that exists and stays empty, which is the honest state — no placeholder, no inference.

## 3. The applied-conversion correction

The plan assumed a referenced rate row converts the line's own currency into the sale currency. The recorded live AirOffer response disproves it: a fare line in currency `155`, equivalent in `70`, references period `1533121255006273536` whose row reads `fromCurrencyId 71 → toCurrencyId 70`. The wire fact and the line's currencies do not correspond, and no published contract explains why.

So the repair preserves the rate row exactly as the source states it — its own from/to identity, rate, decimal places, rounding token — and correlates nothing. A referenced period that is absent from `ratesOfExchange` leaves the snapshot `NULL` while `SourceConversionRef` is still preserved. `RoundingToken` is stored verbatim and is read by no calculation. The accepted monetary values are never recomputed.

## 4. Assertions that carry the point

| Assertion | Where |
|---|---|
| no owner call is needed after acceptance to read any preserved fact back | `Every_supplied_air_offer_fact_survives_…` compares the stub handler's call count before and after reading the whole graph from SQL |
| the schema-3 service detail does not duplicate flight-level facts | `Schema_three_service_detail_does_not_duplicate_flight_level_facts` asserts the candidate detail keys are exactly `bookingClass`, `cabinRef`, `rbdRef` |
| the public AirTransport DTO still returns the same flight number and carriers, now derived from the segment | same test, through `OrderProjectionMapper.ToPublicOrder` |
| an air service without exactly one covered segment fails deterministically | `OrderProjectionMapper.CoveredSegment` throws `20288`, and `CandidateValidator` rejects the candidate before acceptance |
| projection rebuild is byte-identical for the same canonical state | `Projection_rebuild_is_byte_identical_for_the_same_accepted_state` |
| a schema-2 projection row stays readable and a rebuild moves it to schema 3 deterministically | `A_schema_two_projection_row_stays_readable_and_a_rebuild_moves_it_to_schema_three` |
| conversion evidence is preserved without recomputing money | `Applied_conversion_evidence_is_preserved_without_recomputing_money` |
| percentage rows record their kind without interpreting the rate | `Percentage_rows_record_their_calculation_kind_without_interpreting_the_rate` |

## 5. Internal projection vs public contract

`OrderProjectionDocument` (schema 3) is a strongly typed internal document — no dictionaries, no untyped JSON — holding the sale currency code, pricing code/name/reference, calculation kind, applied conversion, product snapshot, commercial-term summaries, per-service term flags, baggage, journeys, restored segment and leg facts, the observed ticketing deadline, the typed opaque fare-construction graph and the association scope.

`OrderProjectionMapper` maps it to the unchanged public `OrderDto`. `OpenApiDocumentTests` and `OrderingApiSurfaceTests` pass without a single expected-schema change, which is the proof that nothing leaked into the ratified contract.
