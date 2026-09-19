# Domain Benchmark Matrix

Stage: 06-S1-create-order-conformance · **revision 3** · 2026-09-19 · HEAD `4e48447`

Revision 1 is superseded. What it got wrong is listed in `INDEPENDENT-REVIEW-CORRECTIONS.md`; the corrections are folded in below. Section **L** carries the concepts revision 1 never audited.

**Revision 3 corrections, all inside section L:** L6 records that the office collision is already visible on the **public** contract; L7 and L8 are gated on `OD-CLOSE-09` because they are shared-contract and public-wire changes; L10's coverage count and its document-authority claim were both wrong; L11 overstated the line-scope requirement; L12's vocabulary question is settled (the Pack does not enumerate it); **L20 is reclassified** from a current S1 gap to `FUTURE_DOMAIN_DEPENDENCY`. Each row carries exactly **one primary** disposition; a `+` clause is a secondary tag and is never counted.

Benchmark evidence rows (`I1`…`N3`) are defined in `BENCHMARK-SOURCES.md`. This matrix compares **domain meaning**, never vendor class names or schemas. A vendor cell reading `—` means the public material does not name the concept; that is not evidence of absence in the product, only of absence in what is publicly proven.

Dispositions: `KEEP`, `FIX_S1_SEMANTICS`, `FIX_DOMAIN_DESIGN`, `DEFER_IMPLEMENTATION`, `BLOCKED_OWNER_CONTRACT`, `EXTERNAL_BENCHMARK_ONLY`, `REJECT_INVENTION`.

---

## A. Offer, acceptance and provenance

| Concept | Airline business meaning | IATA | Amadeus | Sabre | Navitaire | Pack 3.8 | Current AeroTech | Historical AeroTech | Source supplies? | S1 behavior now? | Future stage | Loss/risk | Disposition | Evidence |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| A1 Offer as the priced proposal | What the airline offered, at a price, under conditions, before the customer committed | I2, I5 "a proposal to sell a specific set of products or Services under specific conditions, for a certain price" | A1 "Offer Management" | S2 OOSD | N1 "Offer ancillaries…" | CONTRACTS/02-AIROFFER; `OrderPreparation` | `OrderPreparation` + `NormalizedCandidate` | none | yes | yes | — | none | **KEEP** | `OrderPreparation.cs` |
| A2 Accepted offer identity | Which offer this Order came from | I2 | A1 | S2 | N1 | DOMAIN/15 | `Order.AcceptedSource` (owner, offer id, profile, digests, four instants) | `AcceptedOrderSource` | yes | yes | — | none | **KEEP** | `AcceptedSource.cs` |
| A3 Offer id agreement between request and response | The response must price the offer that was asked for | I2 (Offer→Order pairing) | — | — | — | INV-006 "no silent repricing"; `CONTRACTS/02-AIROFFER` lists root `OfferId` **without** the `?` that marks optional fields | **`details.OfferId` never compared**; missing **and** mismatched both pass | n/a | yes | **yes** | — | **accepted order claims the wrong offer** | **FIX_S1_SEMANTICS** — closure target corrected in revision 2: missing/blank is also `ContractMismatch` | `AIROFFER-WIRE-LOSS-AUDIT.md` §9 |
| A4 Immutable acceptance binding | Owner-guaranteed snapshot that cannot be repriced | I2 | A1 "single source of truth" | S3 | N1 | BD-001 | `AcceptanceAssurance.LocalCandidateOnly` + sandbox gating | none | **no** | represented as not-certified | — | none — correctly blocked | **BLOCKED_OWNER_CONTRACT** (BD-001) | `AirOfferProfile.cs` |
| A5 Raw source evidence | The untouched owner payload behind the accepted facts | — | — | — | — | DOMAIN/15, SLICES/S1 | `PreparationSourceEvidence.Payload` + SHA-256 | none | yes | yes | — | none | **KEEP** | `PreparationSourceEvidence.cs` |
| A6 Offer / price / ticketing validity with distinct owners | Three different clocks owned by three different parties | — | — | — | — | INV-055, DOMAIN/10 | three `ValidityFact`s + typed `ObservedTicketingDeadline` | `OrderTimeLimit` | partly | yes | — | none | **KEEP** | `Order.cs` |
| A7 Ticketing deadline authority | Who ultimately owns the deadline and how it composes | — | — | — | — | BD-004 | observed fact stored; authoritative deadline `NotSupplied` | legacy asserted a deadline | observed only | represented | — | none | **BLOCKED_OWNER_CONTRACT** (BD-004) | `ObservedTimeFact.cs` |

## B. Order, OrderItem and Service

| Concept | Meaning | IATA | Amadeus | Sabre | Navitaire | Pack 3.8 | Current | Historical | Source supplies? | S1 now? | Future | Loss/risk | Disposition | Evidence |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| B1 Order as the single customer record | Replaces PNR + ETKT + EMD | I1 "combines these multiple records into a single retail and customer focused Order" | A1 | S1 | N1 "Single Order concepts from the beginning" | DOMAIN/01 | `Order` aggregate | `Order` | yes | yes | — | none | **KEEP** | `Order.cs` |
| B2 OrderItem = the sold-pricing boundary | What was priced together, not one row per passenger | **M1** "An individually priced item within an Order, made up of one or more Services"; I5 non-homogeneity | — | — | — | INV-009, DOMAIN/02 §5 | one `OfferPackage` item; coupon display rows do not split it | one item per product | AirOffer proves no item split | yes | — | none | **KEEP** | `CONTRACTS/02-AIROFFER` conservative normalization |
| B3 Service = independently serviceable unit | One traveler on one passenger segment for air | **M2** verbatim: "At time of order, the services should be applied to a single passenger on a single segment", and an offered service "can become multiple services within the Order Item as the service is broken down per segment and passenger" | A1 | S3 | — | INV-010, DOMAIN/02 | `OrderService` + `OrderServiceBeneficiary` + `OrderServiceCoverage` | same | yes | yes | — | none | **KEEP** | `OrderService.cs` |
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
| D4 **Settlement attribution — party and category** | *Who* settles and under *what* category — mandatory whenever Effect is SettlementOnly | I3 names Airlines and Sellers as the settling parties; **M3** defines Commission as "paid to an agent" and models the category as a **Code**, not a closed vocabulary; **I7** separates Customer Order Accounting from Partners/Suppliers Order Accounting | DOMAIN/03 §13 "**SettlementOnly requires party/category/currency**" | **absent from `PricingLine`, `CandidatePricingLine`, SQL, projection and validator** | `OrderPricingLine.SettlementPartyRef` / `.SettlementCategory` existed | not from AirOffer; the reference source can supply it | **yes — owner keeps the settlement half of SC-S1-013 in S1** | — | a settlement line can be accepted with no party and no category, violating DOMAIN/03 | **FIX_S1_SEMANTICS** | §8 of `REPORT.md` |
| D5 Commission is not customer debt | Agency remuneration never inflates what the passenger owes | **M3** "remuneration … paid to an agent"; **I7** separates customer-order accounting from partner/supplier accounting; the direct customer-payable relationship is still `NOT PUBLICLY PROVEN` (I4) | INV-013, DOMAIN/03 §21 | enforced in `PricingLineMatrix` and by SQL check `CK_PricingLines_CommissionNotCustomer` | `PricingComponentPolicy` | n/a | yes | — | rule holds; the *line* is untestable end to end without D4 | **KEEP** (rule) / see D4 | `PricingLineMatrix.cs` |
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

Each row is counted under its **one primary** disposition. Where a row shows a `+` clause (L2, L20), that is a secondary tag and is **not** added to any total. `EXTERNAL_BENCHMARK_ONLY` and `REJECT_INVENTION` rows are unchanged from revision 1.

| Disposition | Rev 1 | Rev 2 | **Rev 3** | What moved in revision 3 |
|---|---|---|---|---|
| KEEP | 44 | 47 | **47** | — |
| FIX_S1_SEMANTICS | 3 | 4 | **4** | — |
| **FIX_DOMAIN_DESIGN** | **0** | 7 | **7** | count unchanged; L7 and L8 are now gated on `OD-CLOSE-09` |
| DOMAIN_SHAPE_GAP | — | 4 | **4** | count unchanged; L10, L11, L12 carry corrected analysis |
| ADDITIVE_FUTURE_EXTENSION | — | 2 | **2** | — |
| **FUTURE_DOMAIN_DEPENDENCY** *(new class)* | — | — | **1** | L20 moves here from BLOCKED_OWNER_CONTRACT |
| DEFER_IMPLEMENTATION | 16 | 17 | **17** | — |
| BLOCKED_OWNER_CONTRACT | 8 | 10 | **9** | L20 leaves; L4 stays |
| EXTERNAL_BENCHMARK_ONLY | 5 | 5 | **5** | — |
| REJECT_INVENTION | 7 | 7 | **7** | — |
| **Total rows** | **83** | **103** | **103** | section L adds 20 |

Revision 1's headline — `FIX_DOMAIN_DESIGN = 0` — stays **withdrawn**. Seven rows require a domain-shape correction, four more require the shape to be decided before the slice that first needs it, and one (L20) requires only that an S1 decision be taken generically.

---

## L. Concepts revision 1 did not audit

These rows exist because `DOMAIN/01` §2, `DOMAIN/02` §23 and §48–52, and `DOMAIN/06` name them explicitly and revision 1 never walked those sections field by field.

| Concept | Airline business meaning | IATA | Pack 3.8 | Current AeroTech | Historical | Source supplies? | S1 now? | Future stage | Loss/risk | Disposition | Evidence |
|---|---|---|---|---|---|---|---|---|---|---|---|
| L1 **Immutable `SalesContext`** | The sales circumstances as accepted, frozen | **M4** Carrier/Distributor/Seller are distribution-chain roles, "not an entity's primary business classification" | `DOMAIN/01` §2 lists it as **required at accepted creation**; `DOMAIN/04` §5 "SalesContext … remain historical" | **absent as a concept.** `Channel`, `SellingOfficeId` and two actor fields sit loose on `Order` | none | yes, per surface | **yes** | — | the accepted sales circumstances are not a modelled snapshot | **FIX_DOMAIN_DESIGN** | `Order.cs`, `AuthorizedScopeResolver.cs` |
| L2 **`BuyerSnapshot`** | Who bought, frozen at acceptance | **M4** Seller = "organization that offers a shopping capability to a shopper" | `DOMAIN/01` §2 required; `DOMAIN/04` §5 "Buyer, financial Customer, Traveler, payer, agency/seller and actor are separate references" | **absent.** `BuyerActorContextType`/`BuyerActorId` hold the *initiating actor* — `AirlineUserId`, `TravelAgencyUserId`, `PartnerApiAccessProfileId` or `ActorId` by surface | none | actor yes; buyer identity **not supplied on any surface today** | **yes** | — | an actor is being named a buyer | **FIX_DOMAIN_DESIGN** + **BLOCKED_OWNER_CONTRACT** (`OD-CLOSE-05`) | `AuthorizedScopeResolver.cs` |
| L3 **Seller / agency organisation snapshot** | Which organisation sold it, historically | **M4** Seller role; I3 settlement is with "Agents, OTAs, TMCs" | `DOMAIN/04` §5 "agency/seller … separate references"; historical snapshots remain historical | **not on the Order.** `TravelAgencyId` is resolved to a `FinancialCustomerId` and then dropped; it survives only inside the `agency:{id}` fragment of the persisted `CallerScope` **string** on `OrderPreparations` and `CommandReceipts` | none | yes on OtaPanel | **yes** | S14/interline reuse it | historical seller identity is recoverable only by parsing an idempotency key, and otherwise depends on mutable ReferenceData | **FIX_DOMAIN_DESIGN** | `CallerScopeKey.cs`, `OrderPreparation.cs` |
| L4 **Distributor identity** | The intermediary in the chain, distinct from the seller | **M4** Distributor = "Consolidator, an Aggregator, more generally an intermediary" | `DOMAIN/04` §5 | **absent.** On Ota the `PartnerApiAccessProfileId` is stored as `BuyerActorId` | none | partner profile id only | no | when a real chain is supplied | conflated with actor | **BLOCKED_OWNER_CONTRACT** (`OD-CLOSE-05`) — no chain input exists; do **not** build a distribution-chain engine | `AuthorizedScopeResolver.OtaSaleAsync` |
| L5 **Initiating actor identity** | Which human or system pressed the button | — | `DOMAIN/04` §5 actor is its own reference | present but **misnamed** `BuyerActor*` | none | yes | yes | — | naming error with real consequences | **FIX_DOMAIN_DESIGN** (rename + separate from buyer) | `Order.cs` |
| L6 **Selling office namespace** | Which office sold it, in which office space | — | `DOMAIN/04` §5 office snapshots are historical | `Order.SellingOfficeId` is **one untyped `long` holding two namespaces** — airline office on Backoffice, travel-agency office on OtaPanel | none | yes | yes | — | a later join to an airline-office master silently mis-joins agency offices — **and the collision is already public**: `Order.SellingOfficeId` → `OrderProjectionBuilder.cs:19` → `OrderProjectionDocument.AirlineOfficeId` → `OrderProjectionMapper.cs:22` → `OrderDto.AirlineOfficeId`, so an OtaPanel order returns a travel-agency office under the field name `airlineOfficeId`. The S1 outbox writes `IntegrationEvents.V2.OrderCreated`, which has no office field, so no integration event is affected | **FIX_DOMAIN_DESIGN** (part of `OD-CLOSE-05`, including its public-contract option) | `Order.cs:47`, `OrderProjectionDocument.cs:15`, `OrderDto.cs:14` |
| L7 **Service commercial lifecycle** | The canonical current commercial state of a sold service | **M2** carries `Status Code` and, separately, `Delivery Status Code` | `DOMAIN/02` §48: Pending, Active, Cancelled, **Replaced**, **Expired** | `Pending, Active, Cancelled, Exchanged, Suspended` — `Replaced` and `Expired` missing; `Exchanged` is an operation outcome; `Suspended` belongs to the delivery axis | six status enums, also non-conformant | n/a | vocabulary yes, transitions no | S5/S8/S10 | later servicing would need a semantic migration | **FIX_DOMAIN_DESIGN** (vocabulary now, behaviour deferred) — **gated on `OD-CLOSE-09`**: the enum is a shared contract and serializes by name in the public `OrderDto` | `OrderServiceCommercialStatus.cs` |
| L8 **Item commercial lifecycle** | The derived current state of a sold item | **M1** Order Item has a `Status Code` | `DOMAIN/02` §52: Active, **PartiallyChanged**, Cancelled, Replaced, **Partitioned**, **Expired**, **Inactive** | `Active, Replaced, Cancelled` — four missing | four statuses | n/a | vocabulary yes | S5/S10/S14 | same | **FIX_DOMAIN_DESIGN** — **gated on `OD-CLOSE-09`** for the same reason as L7; existing numeric values are preserved | `OrderItemCommercialStatus.cs` |
| L9 **Persisted current component totals** | Base/tax/fee/surcharge/discount summary beside the grand total | **M5** `Price` has Base Amount, Total Amount and associations to Fee, Markup, **Tax Summary**, Discount, Surcharge, at **Order** and **Order Item** level | `DOMAIN/01` §2 "Derived-but-persisted: CommercialSummary, CustomerTotal **and complete current component totals**" | **absent.** Only `CommercialSummary` and `CustomerTotal` | `OrderAmount` VO | derivable from the lines | **yes** | — | not information loss — every line survives — but a Pack conformance gap | **DOMAIN_SHAPE_GAP** (`OD-CLOSE-06`) | `Order.cs` |
| L10 **FulfillmentProfileSnapshot — full semantics** | Everything a service will need to be fulfilled | I1 "fulfilment, delivery and accounting"; **M2** associates Delivery Provider | `DOMAIN/02` §23: profile ID/version, reservation requirement, **resource quantity/unit policy**, document requirement/type/**authority**, funding requirement, **delivery provider/control policy**, **dependency treatment**, **partial-fulfillment support** | **five of the eight semantics are covered** — profile id/version, reservation requirement, funding requirement, and document **requirement + type** together through `FulfillmentDocumentKind` (`None` / specific kind / `Unresolved`), so no separate `DocumentRequired` flag is needed. **Absent:** document **authority**, resource **unit** policy (only `CapacityUnits`, a quantity), delivery/control policy, dependency treatment, partial-fulfillment support. *Revision 3 correction:* revision 2 said "4½ of 8" and called the authority vocabulary undefined — both wrong. `DOMAIN/07` defines LOCAL-AIRLINE / EXTERNAL and `DocumentAuthority { Local, External }` already exists in Contracts; only the **live value** is unresolved | booleans on the service | live AirOffer certifies none | shape no, values unresolved | S2/S4/S6/S12 | shape must be closed before S2 or later slices will migrate accepted snapshots | **DOMAIN_SHAPE_GAP** (`OD-CLOSE-07`) | `FulfillmentProfileSnapshot.cs`, `DocumentAuthority.cs` |
| L11 **FundingObligation scope** | Which service, item or pricing line the obligation covers | I1 accounting; **M1** Order Item associates Payment Information | `DOMAIN/06`: "**Service/Item/PricingLine scope**" | **item scope only** (`OrderItemId?`) | payment summary | n/a | item only | S3/S6/S9 | an S6 added service needs service scope, and a future `Fee`-purpose obligation whose boundary genuinely is one charge line needs line scope; item-only would harden into an invariant. *Revision 3 correction:* revision 2 claimed a fee-only `MonetaryCharge` **needs** line scope — it does not; a fee-only charge is an `OrderItem` and is legitimately item-scoped. The recommended shape is also corrected: **three nullable real FKs with an exactly-one CHECK**, not a polymorphic `ScopeKind` + `ScopeId`, which SQL Server cannot enforce | **DOMAIN_SHAPE_GAP** (`OD-CLOSE-08`) | `FundingObligation.cs`, `FundingObligationConfiguration.cs` |
| L12 **FundingObligation current disposition** | Open, superseded, released, settled | — | `DOMAIN/06`: "and **current disposition**" | **absent** | none | n/a | no | S3 | cannot express an obligation's own state. *Revision 3:* the conditional is resolved — `DOMAIN/06` names the behaviours (supersede, release, rebind, preserve for audit) and **never enumerates the states**, so the vocabulary is owner-required and must not be invented | **DOMAIN_SHAPE_GAP** (`OD-CLOSE-08`, unconditional) | `FundingObligation.cs` |
| L13 **Amount-based settlement commission, end to end** | The retained half of SC-S1-013 | **M3** Commission Amount 0..1 | `DOMAIN/03` §13; INV-013; owner kept this half in S1 | representable except party/category; **no test at any layer persists one** | `SettlementPartyRef`/`SettlementCategory` existed | reference source can | **yes** | — | see D4 | **FIX_S1_SEMANTICS** | `PricingLineMatrix.cs` |
| L14 **Percentage commission provenance** | Commission expressed as a rate on a base | **M3** Percentage Percent and Percentage Applied To Amount, both 0..1 | not required by Pack for S1 | `PricingCalculationKind` is the seam; no numeric members | none | **AirOffer supplies none** | no | when an owner supplies them | none — the seam exists | **ADDITIVE_FUTURE_EXTENSION**; `OD-P-18` stays binding — AirOffer's `Amount` is never a percentage value | `PricingCalculationKind.cs` |
| L15 **Multi-item Order** | Two independently priced items in one Order | **M1** an Order has many Order Items | `DOMAIN/01` §3 `Order 1 -> many OrderItem` | fully representable; `AddItemsAndServices` loops items; validator enforces one owner per service | one item per product | AirOffer supplies one package | representable, **untested** | S6 supplies the second item | none structural | **KEEP** + test gap | `Order.cs` |
| L16 **Fee-only `MonetaryCharge` item** | A charge with no service behind it | **M1** an item "may or may not be a selected Offer Item" | `DOMAIN/01` §3 "The `OrderItem` fee-only exception is explicit `ItemKind=MonetaryCharge`" | `OrderItemKind.MonetaryCharge` exists; `CandidateValidator` rejects a monetary-charge item that owns services | none | no | representable, **untested** | S9/S10 | none structural | **KEEP** + test gap | `CandidateValidator.EnsureItems` |
| L17 **Included / Complimentary service without a synthetic zero line** | A bundled bag is not a zero-price product | N1 "bundled or unbundled"; N3 "branded fares with included services (fare families)" | `DOMAIN/02` §21 "Included/complimentary does not imply a synthetic zero fare line" | `ServicePriceTreatment` has `Included` and `Complimentary`; AirOffer always yields `SupplierOpaque` | none | no | representable, **untested** | S6 | none structural | **KEEP** + test gap | `ServicePriceTreatment.cs` |
| L18 **External / provider locator identity** | The provider PNR or record locator | **M2** Service associates a Booking Reference | `DOMAIN/01` §2 "A provider locator is stored in ExternalReference with provider namespace and scope" | **no `ExternalReference` concept** | `OrderExternalReference` existed | **AirOffer Details supplies no locator** | no | S2 (FlightFlow) supplies the first real locator | none today — nothing to store | **DEFER_IMPLEMENTATION** | `CONTRACTS/02-AIROFFER` DTO mirror |
| L19 **Item-level time limits** | Payment, price-guarantee, ticketing and naming limits per item | **M1** Order Item carries four separate time-limit attributes | `DOMAIN/10`; INV-055 deadlines have distinct owners | validity facts are held at **Order** level | `OrderTimeLimit` per order | AirOffer supplies one observed ticketing date at root | yes at order level | when a source supplies per-item limits | none at S1 — one item, one source | **ADDITIVE_FUTURE_EXTENSION** | `Order.cs` |
| L20 **Interline settlement information** | What is owed between carriers for a service | **M2** Service associates Interline Settlement Information; **M3** Commission associates it too | `DOMAIN/12`, BD-012 | absent | absent | no | no — not an S1 requirement, and the owner never kept it in S1 | later | none today. The only S1 obligation is that `SettlementAttribution` is decided **generically** rather than commission-shaped, which is why `OD-CLOSE-01` requires a non-commission `SettlementOnly` test | **FUTURE_DOMAIN_DEPENDENCY** + `BLOCKED_OWNER_CONTRACT` (BD-012) — *revision 3 reclassification; revision 2 wrongly counted this as a second current S1 gap* | BD-012 |
