# Domain Benchmark Matrix

Stage: 06-S1-create-order-conformance · 2026-09-19 · HEAD `e861711`

Benchmark evidence rows (`I1`…`N3`) are defined in `BENCHMARK-SOURCES.md`. This matrix compares **domain meaning**, never vendor class names or schemas. A vendor cell reading `—` means the public material does not name the concept; that is not evidence of absence in the product, only of absence in what is publicly proven.

Dispositions: `KEEP`, `FIX_S1_SEMANTICS`, `FIX_DOMAIN_DESIGN`, `DEFER_IMPLEMENTATION`, `BLOCKED_OWNER_CONTRACT`, `EXTERNAL_BENCHMARK_ONLY`, `REJECT_INVENTION`.

---

## A. Offer, acceptance and provenance

| Concept | Airline business meaning | IATA | Amadeus | Sabre | Navitaire | Pack 3.8 | Current AeroTech | Historical AeroTech | Source supplies? | S1 behavior now? | Future stage | Loss/risk | Disposition | Evidence |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| A1 Offer as the priced proposal | What the airline offered, at a price, under conditions, before the customer committed | I2, I5 "a proposal to sell a specific set of products or Services under specific conditions, for a certain price" | A1 "Offer Management" | S2 OOSD | N1 "Offer ancillaries…" | CONTRACTS/02-AIROFFER; `OrderPreparation` | `OrderPreparation` + `NormalizedCandidate` | none | yes | yes | — | none | **KEEP** | `OrderPreparation.cs` |
| A2 Accepted offer identity | Which offer this Order came from | I2 | A1 | S2 | N1 | DOMAIN/15 | `Order.AcceptedSource` (owner, offer id, profile, digests, four instants) | `AcceptedOrderSource` | yes | yes | — | none | **KEEP** | `AcceptedSource.cs` |
| A3 Offer id agreement between request and response | The response must price the offer that was asked for | I2 (Offer→Order pairing) | — | — | — | INV-006 "no silent repricing" | **`details.OfferId` never compared** | n/a | yes | **yes** | — | **accepted order claims the wrong offer** | **FIX_S1_SEMANTICS** | `AIROFFER-WIRE-LOSS-AUDIT.md` §9 |
| A4 Immutable acceptance binding | Owner-guaranteed snapshot that cannot be repriced | I2 | A1 "single source of truth" | S3 | N1 | BD-001 | `AcceptanceAssurance.LocalCandidateOnly` + sandbox gating | none | **no** | represented as not-certified | — | none — correctly blocked | **BLOCKED_OWNER_CONTRACT** (BD-001) | `AirOfferProfile.cs` |
| A5 Raw source evidence | The untouched owner payload behind the accepted facts | — | — | — | — | DOMAIN/15, SLICES/S1 | `PreparationSourceEvidence.Payload` + SHA-256 | none | yes | yes | — | none | **KEEP** | `PreparationSourceEvidence.cs` |
| A6 Offer / price / ticketing validity with distinct owners | Three different clocks owned by three different parties | — | — | — | — | INV-055, DOMAIN/10 | three `ValidityFact`s + typed `ObservedTicketingDeadline` | `OrderTimeLimit` | partly | yes | — | none | **KEEP** | `Order.cs` |
| A7 Ticketing deadline authority | Who ultimately owns the deadline and how it composes | — | — | — | — | BD-004 | observed fact stored; authoritative deadline `NotSupplied` | legacy asserted a deadline | observed only | represented | — | none | **BLOCKED_OWNER_CONTRACT** (BD-004) | `ObservedTimeFact.cs` |

## B. Order, OrderItem and Service

| Concept | Meaning | IATA | Amadeus | Sabre | Navitaire | Pack 3.8 | Current | Historical | Source supplies? | S1 now? | Future | Loss/risk | Disposition | Evidence |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| B1 Order as the single customer record | Replaces PNR + ETKT + EMD | I1 "combines these multiple records into a single retail and customer focused Order" | A1 | S1 | N1 "Single Order concepts from the beginning" | DOMAIN/01 | `Order` aggregate | `Order` | yes | yes | — | none | **KEEP** | `Order.cs` |
| B2 OrderItem = the sold-pricing boundary | What was priced together, not one row per passenger | I5 "An Order may support non-homogeneity, i.e. each passenger in an Order may hold different sets of products and services" | — | — | — | INV-009, DOMAIN/02 §5 | one `OfferPackage` item; coupon display rows do not split it | one item per product | AirOffer proves no item split | yes | — | none | **KEEP** | `CONTRACTS/02-AIROFFER` conservative normalization |
| B3 Service = independently serviceable unit | One traveler on one passenger segment for air | I5 "products or Services"; I1 names Service | A1 | S3 | — | INV-010, DOMAIN/02 | `OrderService` + `OrderServiceBeneficiary` + `OrderServiceCoverage` | same | yes | yes | — | none | **KEEP** | `OrderService.cs` |
| B4 ProductSnapshot | What was sold, frozen at acceptance | I5 "products or Services" | — | — | N1 ancillaries | DOMAIN/02 §9 | `OrderItem.Product` — source system/offer/item ref, code, name, brand, version | `OrderItemProductSnapshot` | only system + offer id today | yes | richer at S6 | none | **KEEP** | `ProductSnapshot.cs` |
| B5 CommercialTermsSnapshot | Display refund/change/upgrade summary at sale | — | — | — | — | DOMAIN/02 §9 | `OrderItem.CommercialTerms` (4-state) + exact per-service `SoldTerms` | `OrderItemCommercialTermsSnapshot` | yes (3 coupon flags) | yes | no-show summary at S9 | none | **KEEP** | `CommercialTermsSnapshot.cs` |
| B6 AcceptedPriceSnapshot | The original item value, not the current balance | — | — | — | — | DOMAIN/02 §7 | `OrderItem.AcceptedTotal` + immutable lines under one `PriceChangeSet` | per-item amounts | yes | yes | — | none | **KEEP** | `OrderItem.cs` |
| B7 CommercialSource on the item | Which commercial owner produced this item | — | — | — | — | DOMAIN/02 §7 | `OrderItem.Product.SourceSystem` carries it at item granularity | `ProductSnapshot.SourceSystem` | yes | yes | needed as a distinct field only when one Order mixes sources (S6/S10) | none today | **KEEP** | `ProductSnapshot.cs` |
| B8 Item/service current membership | Which services belong to which item now | I5 non-homogeneity | — | — | — | DOMAIN/02 §13 | `OrderItemServiceLink` + `ScopeAtAssociation` child rows | link without scope | yes | yes | — | none | **KEEP** | `OrderItemServiceLink.cs` |
| B9 ServiceLineage / ItemLineage | Many-to-many predecessor/successor across servicing | — | A1 "servicing" | S3 | — | DOMAIN/02 §15 | absent | absent | n/a | no | S5/S10 | none — no servicing exists yet | **DEFER_IMPLEMENTATION** | DOMAIN/02 §15 |
| B10 Supersession / cancellation / partition refs on the item | Item-level servicing history | — | — | — | — | DOMAIN/02 §7 | absent | absent | n/a | no | S5/S10/S14 | none | **DEFER_IMPLEMENTATION** | DOMAIN/02 §7 |
| B11 PriceTreatment | Separately priced vs included vs complimentary vs supplier-opaque | — | — | — | N1 "bundled or unbundled" | DOMAIN/02 §21 | `OrderService.PriceTreatment`; AirOffer = `SupplierOpaque` | none | inferable from the opaque package | yes | Included/Complimentary at S6 | none | **KEEP** | `OrderService.cs` |

## C. Travelers, journeys, segments, source occurrence

| Concept | Meaning | IATA | Pack 3.8 | Current | Historical | Source? | S1 now? | Future | Loss/risk | Disposition | Evidence |
|---|---|---|---|---|---|---|---|---|---|---|---|
| C1 Traveler / beneficiary | Who travels; who the service is for | I1 Customer; I5 per-passenger non-homogeneity | DOMAIN/04 | `OrderTraveler` + `TravelerIdentity`, beneficiaries per service | `OrderTraveller` + documents | refs + PTC only | yes | documents/gender/nationality at S13 | none | **KEEP** | `OrderTraveler.cs` |
| C2 Journey grouping | Customer travel grouped into bounds | I5 "not necessarily Journey based" implies Journey exists as a concept | DOMAIN/04 §15 | `OrderJourney` with raw direction + journey type | `OrderItinerary` | yes | yes | — | none | **KEEP** | `OrderJourney.cs` |
| C3 Passenger segment vs operational leg | One sold segment may have several physical legs; legs never multiply services | — | INV-010, DOMAIN/04 §19 | `OrderSegment` + `OrderSegmentLeg`, one air service per traveler+segment | same | yes | yes | — | none | **KEEP** | `CandidateValidator` air rule |
| C4 Source coupon occurrence identity | The owner's own identity for a priced coupon row | — | — (Pack calls the AirOffer coupon "a priced projection, NOT issued ETKT/coupons") | **not persisted**; `SourceServiceRef = {TravellerRef}\|{BoundId}\|{FlightId}` and `SourceLineRef = tickets/{i}/coupons/{j}/pricings/{k}` | none | yes (`CouponId`, `Sequence`) | yes | — | see justification below | **KEEP** | `AIROFFER-WIRE-LOSS-AUDIT.md` §11 |
| C5 Connection / stopover / protection | Whether two segments are a protected connection | — | DOMAIN/04 §19 requires them as *explicit source facts* | `Stop` unread; **no** canonical row, not even `Unknown` | `FlightStopType` | yes, shape unknown | no | when `OR-002` answers | none created | **BLOCKED_OWNER_CONTRACT** (`OD-P-12`) | handoff `OR-002` |
| C6 Bound direction / journey type vocabulary | Outbound vs inbound; one-way vs round trip | — | DOMAIN/04 | raw preserved, canonical enum `NULL` | `BoundDirection` asserted | yes, vocabulary unknown | raw only | when `OR-002` answers | none | **BLOCKED_OWNER_CONTRACT** | `OR-002` |

### C4 justification for `KEEP`

The prompt requires this to be argued, not assumed:

1. **Is it an owner occurrence identity needed to trace the accepted source?** It is *an* identity, but not the only one. `SourceServiceRef` and the `SourceLineRef` occurrence path are stable, deterministic and persisted.
2. **Is it already preserved?** The coupon ordinal survives positionally in every pricing line's `SourceLineRef`. `CouponId` itself survives in the retained raw payload.
3. **Would future repricing/servicing need it?** AirPrice, not AirOffer, owns servicing decisions, and BD-007 blocks that contract entirely. No published contract requires quoting an AirOffer coupon id back.
4. **Is the sequence commercial identity or display ordering?** The Pack states plainly that these Ticket/Coupon names "describe a priced projection, NOT issued ETKT/coupons", so the sequence is source ordering.
5. **Can two coupons collide on one traveler+segment?** No — they would produce a duplicate `ServiceRef`, which `CandidateValidator.UniqueIndex` rejects, and the air-coverage rule rejects a second air service for the same traveler+segment. The model **fails closed**; it never silently merges two coupons.

Therefore the identity can neither be lost (raw evidence) nor collide (fails closed), and a stronger stable identity already exists. `KEEP`. Creating an ETKT-coupon entity in S1 to hold an AirOffer source id would be `REJECT_INVENTION`.

## D. Pricing and money

| Concept | Meaning | IATA | Pack 3.8 | Current | Historical | Source? | S1 now? | Future | Loss/risk | Disposition | Evidence |
|---|---|---|---|---|---|---|---|---|---|---|---|
| D1 Non-negative magnitude + direction supplies sign | Never a second sign from Effect | — | INV-013, DOMAIN/03 §11 | `PricingLine` magnitudes, `PricingLineMatrix`, SQL checks | same | yes | yes | — | none | **KEEP** | `PricingLineMatrix.cs` |
| D2 Component taxonomy (fare/tax/fee/surcharge/discount/markup/penalty/commission/adjustment/other) | Money kinds an airline recognises | — | DOMAIN/03 §15 table | `PricingComponentType` (11 members) | same | AirOffer supplies 4 | yes | rest at S6–S10 | none | **KEEP** | `PricingComponentType.cs` |
| D3 Customer-effective vs settlement-only vs informational | Only CustomerBalance is customer payable | I3 settlement is between airline and seller, a different exchange from the customer payment | INV-013, DOMAIN/03 §13 | `PricingEffect` + `PricingArithmetic.CustomerTotal` | same | AirOffer supplies CustomerBalance only | yes | — | none | **KEEP** | `PricingArithmetic.cs` |
| D4 **SettlementPartyRef / SettlementCategory** | *Who* settles and under *what* category — mandatory whenever Effect is SettlementOnly | I3 names Airlines and Sellers as the settling parties | DOMAIN/03 §13 "**SettlementOnly requires party/category/currency**" | **absent from `PricingLine`, `CandidatePricingLine`, SQL, projection and validator** | `OrderPricingLine.SettlementPartyRef` / `.SettlementCategory` existed | not from AirOffer; the reference source can supply it | **yes — owner keeps the settlement half of SC-S1-013 in S1** | — | a settlement line can be accepted with no party and no category, violating DOMAIN/03 | **FIX_S1_SEMANTICS** | §8 of `REPORT.md` |
| D5 Commission is not customer debt | Agency remuneration never inflates what the passenger owes | I3 (settlement is a separate airline↔seller exchange); the customer-payable relationship is `NOT PUBLICLY PROVEN` (I4) | INV-013, DOMAIN/03 §21 | enforced in `PricingLineMatrix` and by SQL check `CK_PricingLines_CommissionNotCustomer` | `PricingComponentPolicy` | n/a | yes | — | rule holds; the *line* is untestable end to end without D4 | **KEEP** (rule) / see D4 | `PricingLineMatrix.cs` |
| D6 Original vs sale valuation, never summed | 100 USD original, 92 EUR sale is one line, not 192 | — | INV-012, SC-S1-012 | two `Money` values per line; validator rejects two values in one currency | same | yes | yes | — | none | **KEEP** | `CandidateValidator.EnsurePricing` |
| D7 Conversion provenance | Which rate was applied, at what precision and rounding | — | DOMAIN/03 §31 | `AppliedConversion` typed snapshot; rounding token stored, never calculated with | `ExchangeRate` VO | yes | yes | — | the from/to pair's relation to the line is unexplained by any contract | **KEEP** + `BLOCKED_OWNER_CONTRACT` for the interpretation | `AIROFFER-WIRE-LOSS-AUDIT.md` §10 |
| D8 Source line identity (code / name / reference) | A tax code is what an airline user reads first | — | DOMAIN/03 §11 "exact source occurrence/reference" | `SourceCode`, `SourceName`, `SourceReference`, `SourceLineRef` | `Code`, `Description` | yes | yes | public exposure is a separate owner choice | not exposed publicly | **KEEP** | `PricingLine.cs` |
| D9 Repeated identical codes stay distinct rows | Two YQ rows are two charges | — | CONTRACTS/02-AIROFFER "identical tax/fare codes do not imply one charge" | unique key `(PriceChangeSetId, CandidateLineRef)` where the ref is the occurrence path | `OccurrenceKey` | yes | yes | — | none | **KEEP** | `PricingLineConfiguration.cs` |
| D10 Percentage provenance | Was the source row a percentage or a flat amount | — | OD-P-18 | `PricingCalculationKind` | none | yes | yes | — | the numeric basis is deliberately uninterpreted | **KEEP** | `PricingCalculationKind.cs` |
| D11 Pricing group quantity extended once | 2 adults at 200 is 200, not 400 | — | INV-012, SC-S1-011 | `FarePricingGroup.Quantity` + traveler links; line values stored as extended | `OrderFarePricingGroup` | only when the source proves a group | yes | — | none | **KEEP** | `FarePricingGroup.cs` |
| D12 Line quantity / unit of measure / unit price | "2 × 15.00" on a line | — | not required by DOMAIN/03's line field list | absent (rejected by owner in OD-P-17) | present in legacy | **no** | no | when a source supplies it | none — no source supplies it | **DEFER_IMPLEMENTATION** | OD-P-17 |
| D13 Allocation sets and allocations | Value attribution inside one line, never extra money | — | INV-014, SC-S1-014 | **removed** in the stage-04 cleanup | full reconciliation existed | AirOffer supplies none | no | the slice that first persists an allocation | INV-014 is an S1-introduced invariant that is now unproven | **DEFER_IMPLEMENTATION** (owner-ratified) | `S1-API-READABILITY-OWNER-DECISION` line 63 |
| D14 Reversal lines | Reversing a discount adds a Debit | — | DOMAIN/03 §21, SC-S1-013 second half | `PricingLineRole.Reversal` exists in vocabulary; no reversal path | `PricingReversalPolicy` | n/a | no | same slice as D13 | same | **DEFER_IMPLEMENTATION** (owner-ratified) | same decision |
| D15 Decimal round-trip without rounding | Accepted decimals never silently rounded | — | INV-059 | canonical decimal strings, (28,8) columns, overflow rejected | same | yes | yes | — | none | **KEEP** | `DecimalRepresentation.cs` |

## E. Fare construction

| Concept | Meaning | IATA | Pack 3.8 | Current | Historical | Source? | S1 now? | Loss/risk | Disposition | Evidence |
|---|---|---|---|---|---|---|---|---|---|---|
| E1 Fare construction as an immutable accepted context | What fare coupling the airline accepted | — | DOMAIN/03 §47 | `FareConstruction` + typed graph | typed graph | partly | yes | none | **KEEP** | `FareConstruction.cs` |
| E2 PricingUnit with source identity and type | OneWay / RoundTrip / OpenJaw / CircleTrip | — | DOMAIN/03 §51 | `FarePricingUnit` with `SourceKindRaw` + canonical `Unspecified` | typed unit | kind only, vocabulary unknown | yes | canonical type unmapped | **KEEP** | `FarePricingUnit.cs` |
| E3 FareComponent with basis/family/type/cabin/RBD/booking class | The filed fare behind a portion of the journey | — | DOMAIN/03 §51 | `FareComponent` typed | typed | yes | yes | none | **KEEP** | `FareComponent.cs` |
| E4 Explicit component coverage | Which services/segments a component covers | — | DOMAIN/03 §51 | `FareComponentService` / `FareComponentSegment`, rows only where proven | typed links | **AirOffer proves none** | structure yes, rows no | none — opacity is the honest state | **KEEP** | `CandidateValidator` opaque rule |
| E5 Traveler pricing groups | Which travelers were priced together | — | DOMAIN/03 §49 | `FarePricingGroup`, rows only where proven | typed group | AirOffer proves none | structure yes, rows no | none | **KEEP** | `FarePricingGroup.cs` |
| E6 Fare owner / tariff / rule / routing refs | Filing provenance for later reprice | — | DOMAIN/03 §51 "when supplied" | columns exist, always `NULL` for AirOffer | present in legacy | **no** | columns only | none | **KEEP** | `FareComponent.cs` |
| E7 No pricing unit inferred from route shape | A round trip is not assumed from two bounds | — | INV-015 | canonical type stays `Unspecified`; combination method `Unspecified` | legacy hard-coded `ProviderDefined` | — | yes | none | **KEEP** | OD-P-19 |

## F. Fulfilment, resources, documents, funding

| Concept | Meaning | IATA | Amadeus | Sabre | Navitaire | Pack 3.8 | Current | Source? | S1 now? | Future | Disposition | Evidence |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| F1 FulfillmentProfileSnapshot | What this service will require to be fulfilled | I1 "fulfilment, delivery and accounting" | A1 "Delivery Management" | S1 "settlement and delivery" | — | DOMAIN/02 §23 | typed snapshot with profile version + `Certified`/`NotCertified` assurance | AirOffer certifies nothing | yes, as *not certified* | real semantics at S2/S4 | **BLOCKED_OWNER_CONTRACT** (BD-002/005/006) | `FulfillmentProfileSnapshot.cs` |
| F2 Explicit seat/resource requirement on the air service | Does this air service need a seat/capacity resource | — | — | — | — | DOMAIN/02 typed-details table requires it | represented as `ReservationRequirement.Unresolved` | no | honest unresolved state | BD-002 | **BLOCKED_OWNER_CONTRACT** | DOMAIN/02 typed details |
| F3 Reservation / capacity evidence | The inventory hold | — | — | — | — | DOMAIN/05, S2 | none; `SourceCapacityRef` retained as a source reference only | n/a | no | S2 | **DEFER_IMPLEMENTATION** | S2 slice |
| F4 Funding obligation | What must be paid for this sale | I1 accounting | A1 "Payment Management" | S3 payments | N1 SkyLedger | DOMAIN/06 | `FundingObligation` per item with version and supersession | n/a | yes (obligation only, no payment) | S3 | **KEEP** | `FundingObligation.cs` |
| F5 Payment / coverage / refund authority | Moving money | I3 | A1 | S3 | N1 | BD-005, S3/S9 | none | n/a | no | S3/S9 | **DEFER_IMPLEMENTATION** | BD-005 |
| F6 ETKT / EMD documents | The accountable documents ONE Order replaces | I1 explicitly phases out ETKT/EMD | A1 ONE Order alignment | S1 | N1 ARM/ONE Order | DOMAIN/07, S4/S7 | none | n/a | no | S4/S7 | **DEFER_IMPLEMENTATION** | S4 slice |
| F7 Delivery / consumption observations | Check-in, boarding, DCS chronology | I1 "delivery" | A1 "Delivery Management" | S2 "deliver" | — | DOMAIN/11, S12 | none | n/a | no | S12 | **DEFER_IMPLEMENTATION** | BD-009 |

## G. Ancillary products

| Concept | Meaning | IATA | Navitaire | Pack 3.8 | Current | Historical | S1 now? | Future | Disposition | Evidence |
|---|---|---|---|---|---|---|---|---|---|---|
| G1 Included baggage allowance as air-product context | What the fare already includes | — | N1 ancillaries bundled/unbundled | OD-P-08 | `AirTransportDetail.CheckedBaggage` / `.CabinBaggage` | `Baggage` VO | yes | — | **KEEP** | `BaggageAllowance.cs` |
| G2 Seat as an independently sold product | A paid seat is its own service with its own lifecycle | I5 "Services that are not necessarily Journey based" | N1 ancillaries | DOMAIN/02 typed-details table: **Seat** = "Exactly one traveler and related air service; seat product/characteristics, requested seat when sold by identifier"; implementation in **S6** | no seat type; **no seat field on `AirTransportDetail`** | legacy put `RequestedSeat` on the air-transport detail | no | S6 | **DEFER_IMPLEMENTATION** | DOMAIN/02, SLICES/S6 |
| G3 Paid baggage / meal / lounge / hotel / ground transport | Independent services with their own suppliers | I5 | N1 | DOMAIN/02, S6 | `OrderServiceType` vocabulary exists; no typed details | seven typed detail entities | no | S6 | **DEFER_IMPLEMENTATION** | S6 slice |
| G4 Service dependencies (seat requires air service) | A dependent service cannot outlive its parent | — | — | DOMAIN/02 §13, DOMAIN/13 "no RequiresAirService cycle" | absent | `OrderServiceCoveredService` | no | S6 | **DEFER_IMPLEMENTATION** | DOMAIN/13 |
| G5 Shared coverage portions (room, bag pool) without multiplying value | Two travelers, one room, one price | — | — | S6 required work §5 | absent | absent | no | S6 | **DEFER_IMPLEMENTATION** | S6 slice |

## H. Sales, distribution and settlement context

| Concept | Meaning | IATA | Pack 3.8 | Current | S1 now? | Disposition | Evidence |
|---|---|---|---|---|---|---|---|
| H1 Owner airline | Whose Order this is | — | C15 | `Order.OwnerAirlineId` via `IHomeOperatorProvider` | yes | **KEEP** | `Order.cs` |
| H2 Financial customer | Who owes the money | I3 sellers | OD-S1-02 | `Order.FinancialCustomerId` resolved per surface | yes | **KEEP** | `AuthorizedScopeResolver.cs` |
| H3 Sales channel | Backoffice / IBE / OTA / agency panel / GDS | — | C1 exception recorded in `S1-CLEANUP-OPEN-DECISIONS` | `Order.Channel` | yes | **KEEP** | owner answer |
| H4 Selling office | Which office sold it | — | OD-S1-09 | `Order.SellingOfficeId` (token-scoped) | yes | **KEEP** | OD-S1-09 |
| H5 Buyer actor | Which principal accepted | — | DOMAIN/01 | `BuyerActorContextType` + `BuyerActorId` | yes | **KEEP** | `Order.cs` |
| H6 Seller / agency settlement counterparty | The party the airline settles with | I3 "Airlines and Sellers (Agents, OTAs, TMCs, etc.)" | DOMAIN/03 §13 party/category | **absent** — see D4 | required for the settlement half of SC-S1-013 | **FIX_S1_SEMANTICS** | D4 |
| H7 Interline / partner settlement | Money owed between carriers | I3 | BD-012, DOMAIN/12 | none | no | **DEFER_IMPLEMENTATION** | BD-012 |
| H8 Revenue accounting integration | Posting the sale to the ledger | I1 "Revenue Accounting"; N1 SkyLedger | BD-011, CONTRACTS/08 | outbox only | no | **DEFER_IMPLEMENTATION** | BD-011 |

## I. Reliability and truth

| Concept | Meaning | Pack 3.8 | Current | S1 now? | Disposition | Evidence |
|---|---|---|---|---|---|---|
| I1 Command idempotency with scoped receipt | Same key, same result | INV-007, DOMAIN/08 | `CommandReceipt` keyed `(Owner, CallerScope, CommandKind, IdempotencyKey)` | yes | **KEEP** | `CommandReceipt.cs` |
| I2 One preparation yields at most one Order | Even under concurrency and restart | INV-008 | unique consumption + SQL uniqueness | yes | **KEEP** | `OrderPreparationConfiguration.cs` |
| I3 One atomic local transaction | Receipt + graph + projection + outbox | INV-003, INV-016 | one SQL transaction, provider read outside | yes | **KEEP** | `AtomicityAndRebuildTests` |
| I4 Rebuildable projection, owner offline | Read the Order with everything down | INV-016 | typed schema-3 projection, deterministic rebuild | yes | **KEEP** | `InformationPreservationTests` |
| I5 No cross-customer leakage | Scope from authenticated authority | INV-017 | per-surface authorized scope; undisclosed 404 | yes | **KEEP** | `OrderingApiSurfaceTests` |
| I6 PII separation | Names and contacts out of the projection | DOMAIN/04, OD-S1-04 | projection redacted; re-hydrated per surface | yes | **KEEP** | `OrderDtoReader.cs` |
| I7 Unknown product/owner semantics fail explicitly | Never accept an unregistered schema | INV-058, SC-S1-021 | `ServiceDetailSchemaRegistry.EnsureRegistered` | yes | **KEEP** | `ServiceDetailSchemaRegistry.cs` |
| I8 CommercialVersion counts commercial mutations, not events | Versions do not inflate | INV-054 | `CommercialVersion` = 1 at create | yes | **KEEP** | `Order.cs` |

## J. Benchmark-only observations (no S1 obligation)

| Concept | Why it is benchmark-only | Disposition |
|---|---|---|
| J1 Dynamic / continuous pricing and dynamic bundling (I2) | Offer-side capability owned by AirOffer/AirPrice, not by Ordering | **EXTERNAL_BENCHMARK_ONLY** |
| J2 IATA Reference Business Architecture and ARM accreditation (I2, N1) | Programme/maturity constructs, not a domain entity | **EXTERNAL_BENCHMARK_ONLY** |
| J3 Vendor capability names — "Offer Management", "Order Management", "Payment Management", "Delivery Management", OOSD (A1, S2) | Product packaging vocabulary; the underlying concepts already map to Pack aggregates and slices | **EXTERNAL_BENCHMARK_ONLY** |
| J4 Hybrid coexistence with legacy PSS (S3) | Deployment strategy; `LEGACY-CUTOVER-OUT-OF-SCOPE.md` excludes it | **EXTERNAL_BENCHMARK_ONLY** |
| J5 Loyalty / traveler experience personalisation (A1) | Not an Ordering concern in Pack 3.8 | **EXTERNAL_BENCHMARK_ONLY** |

## K. Inventions explicitly rejected

| Candidate | Why it would be wrong | Disposition |
|---|---|---|
| K1 `SeatNumber` on `AirTransportDetail` | DOMAIN/02 assigns seat product/characteristics and requested seat to the **Seat** service type, implemented in S6. The legacy repo's `RequestedSeat` on the air-transport detail is the precedent to avoid, not to copy. | **REJECT_INVENTION** |
| K2 An ETKT coupon entity in S1 to hold `AirOfferCouponWire.CouponId` | The Pack states these are a priced projection, not issued coupons; documents arrive at S4 | **REJECT_INVENTION** |
| K3 A canonical `Unknown` connection row derived from the presence of a `Stop` node | Inventing a canonical fact from an unread payload | **REJECT_INVENTION** |
| K4 Mapping `direction`/`journeyType` by enum-name guessing | No published vocabulary; raw is preserved instead | **REJECT_INVENTION** |
| K5 An agency commission engine, or commission derived from agency contracts | Ordering preserves the accepted settlement fact; Ledger/settlement owners compute | **REJECT_INVENTION** |
| K6 A generic order-TTL from the earliest of the validity facts | INV-055 forbids a universal Order TTL | **REJECT_INVENTION** |
| K7 A synthetic rounding fee to reconcile a source total mismatch | SC-S1-015 requires `ContractMismatch` | **REJECT_INVENTION** |

## Disposition counts

| Disposition | Rows |
|---|---|
| KEEP | 44 |
| FIX_S1_SEMANTICS | 3 (A3, D4, H6 — H6 is the same defect as D4 seen from the distribution side) |
| FIX_DOMAIN_DESIGN | 0 |
| DEFER_IMPLEMENTATION | 16 |
| BLOCKED_OWNER_CONTRACT | 8 |
| EXTERNAL_BENCHMARK_ONLY | 5 |
| REJECT_INVENTION | 7 |
| **Total rows** | **83** |

`FIX_DOMAIN_DESIGN` is zero: no audited concept is forced into the wrong home by the current design. The two real defects are a missing validation (A3) and a missing pair of fields on an existing entity (D4/H6), not a structural error.
