# Pack → S1 materialization matrix

Stage: 08-S1-authoritative-final-closure · 2026-09-21 · branch `k8s-stg`

Bidirectional audit. **Forward:** for each Pack concept that S1 touches, where it lives in the code and what enforces
it. **Reverse:** for each persisted structure, which Pack line asks for it — a structure with no Pack line is a local
invention and has to justify itself.

Every Pack concept below is in exactly one state: `MATERIALIZED`, `DEFERRED` (with the owning stage named in the
ERRATA), or `DIVERGENCE` (with the gap recorded).

---

## 1. Forward — `DOMAIN/01-AGGREGATES.md`

| Pack concept | State | Where | Enforced by |
|---|---|---|---|
| Order is the consistency boundary | `MATERIALIZED` | `Domain/OrderAggregate/Order.cs`; one repository, one unit of work | 21 composite foreign keys (`CURRENT-OWNERSHIP-FK-MATRIX.md`); `AtomicityAndRebuildTests.SC_S1_006` |
| `OrderPreparation` is a separate aggregate, not a partial Order | `MATERIALIZED` | `Domain/OrderPreparationAggregate/` | `UX_Orders_Owner_SourcePreparation`; ERRATA §4; `ReliabilityClosureTests.R3_*` |
| An Order has many individually priced items | `MATERIALIZED` | `OrderItem` | scenario 1 (D+S tests) |
| A fee-only item may own no service | `MATERIALIZED` | `CandidateValidator.EnsureItems` | scenario 3 (3 D tests + 1 S test) |
| Component totals are derived **and persisted** | `MATERIALIZED` | `OrderComponentTotal`, key `(OrderId, Component, Effect)` | `ComponentTotalPersistenceTests`; scenario 38 |
| Order tree with a shared root | `MATERIALIZED` | `Order.RootOrderId`, `CK_Orders_Root` | — |

## 2. Forward — `DOMAIN/02-COMMERCIAL-COMPOSITION.md`

| Pack concept | State | Where | Enforced by |
|---|---|---|---|
| Immutable `OrderItemServiceLink` membership history | `MATERIALIZED` | `OrderItemServiceLink` | FK matrix rows 10–12 |
| `OrderIdAtAssociation` | `MATERIALIZED` | column + all three outbound FKs composite on it | `An_item_service_link_cannot_bind_a_row_of_another_order` (3 cases) |
| `ScopeAtAssociation` | `DEFERRED` | — | ERRATA §1.3, `OD-S1-08` — the Pack names it once with no type |
| Current service owns exactly one OrderId/OrderItemId | `MATERIALIZED` | `OrderService.OrderId`, `.OrderItemId` | FK matrix row 4 |
| `ServiceType`, `CommercialStatus`, `FulfillmentProfileSnapshot`, `CreatedByChangeId` | `MATERIALIZED` | `OrderService` / `OrderAirTransportService` | 7 enum CHECKs; FK matrix row 5 |
| `ServiceVersion` | `DEFERRED` | — | ERRATA §1.1 |
| `PriceTreatment` | `DEFERRED` | — | ERRATA §1.2; scenarios 4, 5 |
| `ServiceCode`/`Name`, `SupplierPartyRef`, `DeliveryProviderRef` | `DEFERRED` | — | ERRATA §1.5 — AirOffer supplies none |
| Immutable transition history | `DEFERRED` | — | ERRATA §1.4 — one transition per service in S1, already recorded by `CreatedByChangeId` |
| Included/complimentary does not imply a synthetic zero line | `DEFERRED` | — | follows `PriceTreatment` |
| Untracked product characteristics stay attributes | `MATERIALIZED` | cabin, RBD, booking class, baggage and sold terms are typed columns of the air service, not separate services | `A_service_carries_only_its_own_facts_and_never_repeats_the_flight` |

## 3. Forward — `DOMAIN/03-PRICING-AND-FARE-CONSTRUCTION.md`

| Pack concept | State | Where | Enforced by |
|---|---|---|---|
| One immutable `PriceChangeSet` per accepted monetary mutation, with all its lines in the same transaction | `MATERIALIZED` | `PriceChangeSet` + `PricingLine` | `IX_PriceChangeSets_ChangeId` unique; FK matrix rows 13–14 |
| `FinancialSequence` | `MATERIALIZED` | `PriceChangeSet.FinancialSequence` | `CK_PriceChangeSets_FinancialSequence`, unique `(OrderId, FinancialSequence)` |
| `Reason` | `MATERIALIZED` | typed | `CK_PriceChangeSets_Reason_Enum` |
| `SourceDecisionRef` | `DEFERRED` | — | ERRATA §2.1 |
| `BaseCommercialVersion` | `DEFERRED` | — | ERRATA §2.2 |
| A quote is not a committed line | `MATERIALIZED` | S1 has no quote step; only accepted lines are written | `SC_S1_006` (nothing commits on failure) |
| Line role is a first-class monetary semantic | `MATERIALIZED` | `PricingLine.Role` | `CK_PricingLines_Role_Enum` (all four members) |
| Component/effect matrix (tax not settlement, commission not customer, other only informational) | `MATERIALIZED` | `PricingLineMatrix` | 3 semantic CHECKs + scenarios 22, 24 |
| Settlement attribution complete or absent | `MATERIALIZED` | `SettlementAttribution` | `CK_PricingLines_SettlementAttribution`; scenarios 18–21 |
| Settlement category preserved, never interpreted | `MATERIALIZED` | opaque string | scenario 20 (theory over arbitrary codes) |
| Identical codes do not imply one charge | `MATERIALIZED` | `SourceOccurrencePath` unique per set | scenarios 25, 30 |
| Money/rate capacities | `MATERIALIZED` | `decimal(28,8)` amounts, `decimal(28,12)` rates, `decimal(18,6)` quantities | `DecimalPrecisionTests`, `ModelPrecisionTests`; scenarios 35–37 |
| Fare construction may cross items | `MATERIALIZED` | `FareConstructionItem` | scenario 7 (D+S) |
| Opaque pricing context when construction is incomplete | `MATERIALIZED` | `FareConstructionAssurance` | `CK_FareConstructions_Assurance_Enum`; scenario 52 |

## 4. Forward — `DOMAIN/04-TRAVELERS-JOURNEYS-AND-PRIVACY.md`

| Pack concept | State | Where | Enforced by |
|---|---|---|---|
| Traveller identity and passenger type | `MATERIALIZED` | `OrderTraveller`, `OrderTravellerIdentity` | `CK_OrderTravellers_PassengerTypeCode_Enum`; scenario 44 |
| Guardian/infant binding | `MATERIALIZED` | `InfantParentTravellerId` | `CK_OrderTravellers_Guardian`, FK matrix row 9 |
| Journeys, bounds, segments, legs | `MATERIALIZED` | `OrderJourney`, `OrderSegment`, `OrderSegmentLeg` | FK matrix row 8; `SegmentStructureTests` (22 tests); scenarios 46–50, 53 |
| Technical stop is legs, not segments | `MATERIALIZED` | legs nested under one segment | scenario 49 |
| History stores non-PII correlation ids and redacted metadata | `MATERIALIZED` | no history table holds personal data | `R11`, `Service_surface_needs_no_token_and_never_returns_traveller_names` |
| Protected payload references, access and retention policy | `DIVERGENCE` | names, DOB and contacts are inline columns | ERRATA §6, `OD-S1-09`. **S1 is not privacy-complete and does not claim to be.** |

## 5. Forward — `DOMAIN/05` sales context, `DOMAIN/06` funding, `DOMAIN/10` versions, `DOMAIN/15` evidence

| Pack concept | State | Where | Enforced by |
|---|---|---|---|
| FinancialCustomer, Buyer, Seller, Actor and Traveller are five distinct facts | `MATERIALIZED` | `Order.FinancialCustomerId`, `Buyer`, `SalesContext`, `InitiatingActor`, `OrderTraveller` | scenarios 9–12; 4 enum CHECKs on `Orders` |
| Office identity carries its namespace | `MATERIALIZED` | `SellingOfficeKind` + `SellingOfficeId` | `CK_Orders_SellingOfficeKind_Enum`; scenario 12 |
| Accepted history is never rewritten by current reference data | `MATERIALIZED` | snapshots, not lookups | scenario 8 (repoints the agency and re-reads) |
| `OwnerAirlineId` comes from `IHomeOperatorProvider`, never from `ICallerContext` | `MATERIALIZED` | `ReferenceDataHomeOperatorProvider` | `HomeOperatorFromCoreTests`, `SingleRegistrationGuardTests` |
| Funding obligation scoped to exactly one of item / service / pricing line | `MATERIALIZED` | `FundingObligation` | `CK_FundingObligations_ExactlyOneScope`; FK matrix rows 19–21 |
| Obligation amount non-negative and versioned | `MATERIALIZED` | | `CK_FundingObligations_Amount`, `CK_FundingObligations_Version`, plus the domain guard added post-R2 |
| `CommercialVersion` is the per-Order counter | `MATERIALIZED` | `OrderChange.CommercialVersion` | `IX_OrderChanges_OrderId_CommercialVersion` **unique** (new in this stage) |
| `CurrentDisposition`, eligibility vector | `DEFERRED` | — | ERRATA §3.1, §3.2 — S1 creates no document, control or delivery effect |
| Three independent validity instants | `MATERIALIZED` | `OfferExpiresAt`, `PriceValidUntil`, `LastTicketingDate` | `R12` (2 tests, new in this stage) |
| Accepted state traceable to its source | `MATERIALIZED` | `SourceOfferId`, `SourcePreparationId`, `AcceptedSnapshotDigest`, `SourceOccurrencePath`, raw evidence + hash | `SourceEvidenceRetentionTests` (6 tests, new in this stage) |
| Historical payloads carry an explicit `SchemaVersion`; unknown versions are not misread | `MATERIALIZED` | `ProjectionSchemaVersion` | `R6` (2 tests, new in this stage) |

## 6. Forward — `DOMAIN/13-PERSISTENCE-DATA-DICTIONARY.md`

| Pack line | State | Note |
|---|---|---|
| Local IDs bigint, opaque as strings on HTTP | `MATERIALIZED` | snowflake `long`, string on the wire |
| External refs nvarchar with explicit per-contract limits | `MATERIALIZED` | `PersistenceSchemas.*Length`, every string column has an explicit `HasMaxLength` |
| Timestamps datetimeoffset | `MATERIALIZED` | |
| JSON payload columns explicitly nvarchar(max) with schema version | `MATERIALIZED` | `CandidateJson`, `DetailsJson` + `ProjectionSchemaVersion`, `ISJSON` CHECK |
| Never rely on blanket string(256) | `MATERIALIZED` | |
| `OrderPreparations` row: unique consumption, digest immutable | `DIVERGENCE` (direction) | ERRATA §4 — the link is on `Orders`, the guarantee is equivalent, there is no `ConsumedByOrderId` column |
| Schema areas `Commercial`, `Fulfillment`, … | `DIVERGENCE` | ERRATA §5 — the schema is named `Order`, cosmetic, renaming needs approval |
| Constraints/index inspection is stage evidence | `MATERIALIZED` | `ENUM-CONSTRAINT-MATRIX.md`, `CURRENT-OWNERSHIP-FK-MATRIX.md`, 51 inspection tests |
| No implicit precision loss, no data reset | `MATERIALIZED` | `MIGRATION-IMPACT.md` §2 — zero data operations |

---

## 7. Reverse — every persisted table, and the Pack line that asks for it

25 mapped tables in `OrderingDbContext`.

| Table | Pack basis |
|---|---|
| `Orders` | `DOMAIN/01`, `DOMAIN/13` |
| `OrderTravellers`, `OrderTravellerIdentities` | `DOMAIN/04` |
| `OrderContacts` | `DOMAIN/04` |
| `OrderJourneys`, `OrderSegments`, `OrderSegmentLegs` | `DOMAIN/04` |
| `OrderItems` | `DOMAIN/01` §3, `DOMAIN/02` |
| `OrderServices` (TPH: `OrderAirTransportService`) | `DOMAIN/02` |
| `OrderItemServiceLinks` | `DOMAIN/02` line 13 |
| `OrderChanges` | `DOMAIN/10` |
| `PriceChangeSets`, `PricingLines` | `DOMAIN/03` line 9 |
| `OrderComponentTotals` | `DOMAIN/01` (derived and persisted) |
| `FundingObligations` | `DOMAIN/06` |
| `FareConstructions`, `FareConstructionItems`, `FarePricingUnits`, `FarePricingUnitCoveredBounds`, `FareComponents` | `DOMAIN/03` |
| `OrderPreparations`, `PreparationSourceEvidence` | `DOMAIN/13` line 12, `DOMAIN/15`, `CONTRACTS/02-AIROFFER.md` |
| `CommandReceipts` | `ARCHITECTURE/03` (idempotency) |
| outbox / inbox tables | `ARCHITECTURE/03` (transactional outbox, inbox dedup) |

**No table exists without a Pack line.** The nearest thing to an exception is `PreparationSourceEvidence`, which the
Pack describes as part of the preparation record ("source payload") rather than as its own table; splitting it out is
a storage decision, not a semantic one, and it is what makes the byte-for-byte evidence retention in
`SOURCE-CONTRACT-MATRIX.md` §5 possible.

## 8. Summary

| State | Count |
|---|---|
| `MATERIALIZED` | 46 |
| `DEFERRED` (each with an owning stage named in the ERRATA) | 10 |
| `DIVERGENCE` (each recorded, two of them open owner decisions) | 3 |

The three divergences are: the PII payload store (`OD-S1-09`, S1 is not privacy-complete), the preparation-consumption
direction (guarantee equivalent, shape differs), and the schema name (`Order` vs `Commercial`, cosmetic).
