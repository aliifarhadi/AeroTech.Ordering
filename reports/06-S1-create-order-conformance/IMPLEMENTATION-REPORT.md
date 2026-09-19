# S1 Create-Order Closure — Implementation Report

Stage: 06-S1-create-order-conformance · 2026-09-19 · branch `k8s-stg`

## 1. Repository state

| | |
|---|---|
| **Starting HEAD** | `61d64724e07e38d10808e1895b82afaaf1d16fe0` ("Reports added") — verified before editing; worktree clean |
| **Parent** | `4e48447f51ab2ecc1829f30ec1c14d759f2659f5` |
| **Production/domain baseline** | `e86171103e9fef0b6a57a7895b32ccc87b0e5773` ("S1-Domain Repair") |
| **HEAD did not advance** beyond `61d64724` during the run | confirmed by `git rev-parse HEAD` at start |
| **Ending worktree** | modified, **uncommitted and unpushed** |
| **Commit / push state** | **nothing committed, nothing pushed** — the owner has not asked for a commit |
| **Unrelated work** | none discarded, reset or reverted; no second solution or repository created |
| **Pack** | `docs/ORDERING-DESIGN-PACK-v3.8/` untouched |
| **Packages** | **zero** added, removed or upgraded |

## 2. Binding decisions implemented

| Decision | Code | Migration |
|---|---|---|
| D1 strict `offerId` fail-closed | `AirOfferCandidateMapper.EnsureRespondedOffer` (called first in `Map`) | none |
| D2 generic `SettlementAttribution` | `Domain/_Shared/ValueObjects/SettlementAttribution.cs`; `CandidatePricingLine`; `PricingLine`; `PricingLineMatrix.EnsureSettlementAttribution`; `CandidateValidator`; `PricingLineConfiguration`; candidate JSON v4; projection schema 4 | `CK_PricingLines_SettlementAttribution` + two nullable columns |
| D3 accepted sales provenance | `SalesContextSnapshot`, `BuyerSnapshot`, `InitiatingActorSnapshot`; `AuthorizedSalesScope`; `CandidateSalesContext`; `Order`; `OrderConfiguration` | rename + 5 columns + backfill |
| §6 per-surface mapping | `AuthorizedScopeResolver` (four surfaces) | — |
| §7 public office contract | `OrderDto`, `OrderProjectionDocument`, `OrderProjectionMapper`, `ServiceController`, `CreateOrderFromOfferRequests` | — |
| §8 candidate schema 4 | `NormalizedCandidate`, `NormalizedCandidateJson` (schema-aware writer **and** reader) | — |
| §11 lifecycle vocabulary | `OrderServiceCommercialStatus`, `OrderItemCommercialStatus`, new `SellingOfficeKind` | guard `THROW 52010/52011` |
| §12 component totals | `OrderComponentTotal`, `Order.AddComponentTotals`, `OrderComponentTotalConfiguration` | new table + deterministic backfill |
| §13 fulfillment profile shape | `FulfillmentProfileSnapshot`, `CandidateFulfillmentProfile`, `OrderService`, `OrderServiceConfiguration` | 5 nullable columns |
| §14 funding obligation scope | `FundingObligation`, `FundingObligationScope`, `Order.AddOriginalSaleObligations`, `FundingObligationConfiguration` | 2 FKs + `CK_FundingObligations_ExactlyOneScope` |
| §15 projection schema 4 | `OrderProjectionJson`, `Projection/Compatibility/` (documents + reader), `OrderDtoReader`, `OrderProjectionBuilder` | none |
| §17 `GetOperation` stays deleted | no route added | — |

Applied decisions are recorded in `reports/00-decisions/S1-FINAL-CLOSURE-APPLIED-DECISIONS-2026-09-19.md`.

## 3. Domain before / after

```
Order (before)                          Order (after)
  Channel                                 SalesContext : SalesContextSnapshot
  SellingOfficeId                           Channel, SellerContextType?, SellerId?,
  BuyerActorContextType                      SellingOfficeKind?, SellingOfficeId?
  BuyerActorId                            Buyer : BuyerSnapshot
                                          InitiatingActor : InitiatingActorSnapshot
                                          ComponentTotals : OrderComponentTotal[]

PricingLine                    + SettlementAttribution?
FundingObligation   OrderItemId?          OrderItemId? | OrderServiceId? | PricingLineId? (exactly one)
FulfillmentProfileSnapshot 7 members      12 members
```

New types: `SalesContextSnapshot`, `BuyerSnapshot`, `InitiatingActorSnapshot`, `SettlementAttribution`,
`FundingObligationScope`, `OrderComponentTotal`, `LegacySellingOfficePolicy`, `SellingOfficeKind`.
`Order.Channel`, `Order.SellingOfficeId` and `Order.SellingOfficeKind` survive as derived getters over `SalesContext`
and are `Ignore`d by EF, so there is no duplicate canonical copy.

## 4. Sales-context result

| Surface | Seller | Selling office | Buyer | Initiating actor |
|---|---|---|---|---|
| Backoffice | `(Airline, OwnerAirlineId)` | `(AirlineOffice, token AirlineOfficeId)` | NotSupplied | `(Airline, AirlineUserId)` |
| OtaPanel | `(TravelAgency, TravelAgencyId)` — **persisted**, never recovered from `CallerScope` | `(TravelAgencyOffice, TravelAgencyOfficeId)` | NotSupplied | `(TravelAgency, TravelAgencyUserId)` |
| OTA / PartnerAPI | NotSupplied | `(TravelAgencyOffice, id)` when the token has one, else null | NotSupplied | `(PartnerApi, PartnerApiAccessProfileId)` |
| Service2Service | NotSupplied | request `SellingOfficeId` + `SellingOfficeKind`, both-or-neither, `NotRecorded` rejected | NotSupplied | `(Service, ActorId?)` |

**Historical backfill** (migration `S1ClosureDomainRepair`):

- office kind derived **only** from the immutable `Channel` — BackOffice → AirlineOffice, AgencyPanel → TravelAgencyOffice,
  PartnerAPI → TravelAgencyOffice, anything else → `NotRecorded`; and only where an office id exists.
- seller set **only** for `Channel = BackOffice`, from the row's own immutable `OwnerAirlineId`.
- buyer left NULL everywhere.
- `BuyerActorContextType`/`BuyerActorId` **renamed** to `InitiatingActorContextType`/`InitiatingActorId`; values untouched.
- ReferenceData is never queried; `CallerScope` is never parsed.

EF's generated migration proposed renaming `BuyerActorId` → **`SellerId`**. That would have silently turned every
historical initiating actor into a seller. The rename was corrected by hand to `InitiatingActorId`, and `SellerId` is a
new column.

## 5. Pricing / settlement result

`Fare 400 Debit + Fee 50 Debit − Discount 45 Credit = 405`, with `Commission 20 SettlementOnly`
attributed to `("agency:77", "COMMISSION")` and **excluded** from `CustomerTotal`. Proven end to end — candidate → domain
→ SQL → projection → rebuild — by `SettlementAttributionTests.SC_S1_013_amount_based_agency_commission_leaves_the_customer_total_at_405`
and `ComponentTotalPersistenceTests.A_settlement_attribution_survives_sql_the_projection_and_a_rebuild`.

The model is **generic, not commission-shaped**: `A_non_commission_settlement_line_proves_the_model_is_generic` accepts a
`Fee` line on an explicit source settlement basis with attribution `("partner:consolidator-9", "PARTNER-FEE")`, and
`CustomerTotal` stays 100.

Rules enforced: attribution required iff `SettlementOnly` (domain **and** SQL), no half population (SQL), commission never
`CustomerBalance`, tax never `SettlementOnly`, `Other` informational only. The matrix checks component/effect rules
**before** attribution completeness, so the Pack's own negative example still fails with its own rule.

## 6. Candidate schema 3 → 4

- `NormalizedCandidate.CurrentSchemaVersion = "4.0"`, `LegacySchemaVersion = "3.0"`.
- **`CanonicalJson.Version` remained `ordering-canonical-json-v1`** — the canonicalization algorithm did not change, so
  `OrderPreparation.CanonicalizationVersion` is untouched and no second algorithm version was invented.
- The writer and the reader are both schema-aware. A `3.0` candidate is written with the **exact** v3 key sets and a `4.0`
  candidate with the v4 sets; the v4-only members are omitted from v3 output entirely rather than emitted as null.
- v4 adds `salesContext.seller|sellingOffice|buyer`, `pricingLines[].settlementAttribution` and five
  `fulfillmentProfile` members. No Distributor member.
- A v3 candidate read into the current model gets honest defaults: seller/buyer NotSupplied, settlement attribution null,
  new fulfillment members null, office kind inferred only from the immutable channel.

Evidence: `CandidateSchemaTransitionTests` asserts the v3 canonical **key sets** explicitly (root, `salesContext`,
every pricing line, every fulfillment profile) for all four retained Pack examples, plus v3 round-trip stability; and for
v4, byte-stable round-trip and that seller, office, buyer and settlement attribution each change the accepted digest.

## 7. Projection schema 2 / 3 → 4

`OrderProjectionJson.SchemaVersion = 4`; `LegacyInternalSchemaVersion = 3`. Schema 3 is **not** mutated in place.
`OrderDtoReader` dispatches 4 → current mapper, 3 → `LegacyProjectionReader.ReadInternalSchemaThree`,
2 → `LegacyProjectionReader.ReadPublicSchemaTwo`, anything else → `UnsupportedCapability`.

Legacy office values are mapped through the same `LegacySellingOfficePolicy`; seller and buyer are **never** fabricated
for a legacy row. `ProjectionSchemaTransitionTests` writes genuine legacy payloads (asserting they contain
`airlineOfficeId` and no `componentTotals`), reads them back through the API, rebuilds, and asserts the rebuilt schema-4
JSON is byte-identical to the original — for both 2 → 4 and 3 → 4 — plus a case proving a legacy row with no office
yields a null office **and** a null kind.

Stale-rebuild protection in `OrderProjector.ReplaceAsync` is unchanged.

## 8. Fulfillment / Funding structural result

`FulfillmentProfileSnapshot` gains `DocumentAuthority?` (the **existing** `Local/External` enum — nothing invented),
`ResourceUnitPolicyRef`, `DeliveryControlPolicyRef`, `DependencyTreatmentPolicyRef` (opaque source/profile refs) and
`PartialFulfillmentSupported` as a **nullable** bool. No duplicate `DocumentRequired` flag: `FulfillmentDocumentKind`
already expresses requirement and type. The live uncertified AirOffer profile maps all five to null. **No fulfillment
behaviour, no engine.**

`FundingObligation` carries three nullable real FKs with `DeleteBehavior.Restrict` and a SQL CHECK requiring exactly one;
callers pass a typed `FundingObligationScope`. S1's original-sale obligation stays **item**-scoped; a `CustomerBalance`
line with no item produces a **PricingLine**-scoped obligation instead of a null-scope one. **No disposition column and no
`Order.ObligationRevision`** — both remain S3 design input because `DOMAIN/06` never enumerates the disposition states.

## 9. Public API changes

Exactly two, both required by the closure:

1. `OrderDto`: `AirlineOfficeId` **removed**; `SellingOfficeId` + `SellingOfficeKind` **added**. No deprecated
   compatibility field.
2. Lifecycle enum vocabularies change the OpenAPI enum strings (`Exchanged`→`Replaced`, `Suspended`→`Expired`, plus four
   new item members).

`ServiceCreateOrderFromOfferRequest` replaces `AirlineOfficeId` with `SellingOfficeId` + `SellingOfficeKind`. The
Backoffice create request keeps `AirlineOfficeId` because that surface's office genuinely is airline-scoped.
The create response is unchanged: `{ orderId, orderReference, status, grandTotal, currencyRef }`. No new route.
The S1 outbox writes `IntegrationEvents.V2.OrderCreated`, which has no office field — no integration event changed.

Internal-only and deliberately not exposed: seller, buyer, initiating actor, component totals, settlement attribution,
pricing source code/name/reference.

## 10. Migration result

One guarded migration, `20260919133032_S1ClosureDomainRepair`, structured as named steps: guard → rename → add sales
provenance → backfill provenance → settlement attribution → component totals table → backfill totals → fulfillment
members → funding scope.

**Partition note, stated honestly:** the closure specification preferred several migrations by semantic concern. EF Core
diffs the model snapshot as a whole, so with the full model already in place it emits a single migration; splitting it
by hand would have required per-migration Designer snapshots describing intermediate models that never existed. One
migration with named, independently readable steps was chosen over fabricated snapshots. The five guards make each
concern fail on its own terms.

**Guards (fail loudly, never coerce):**

| Code | Refuses |
|---|---|
| 52010 | an `OrderService` outside Pending/Active/Cancelled |
| 52011 | an `OrderItem` outside Active/Replaced/Cancelled |
| 52012 | an existing `SettlementOnly` line (attribution is a source fact, never invented) |
| 52013 | a `FundingObligation` with no item scope |
| 52014 | a pricing line valued in a currency other than the order sale currency |

Results are in `PERSISTENCE-CERTIFICATION.md`.

## 11. Scenario certification

See `SCENARIO-RESULTS.md` for the row-by-row mapping of the 61 business/domain scenarios and R1–R12.

## 12. Reliability regression

R1–R12 all re-run green: idempotent replay, replay conflict, concurrent acceptance, no owner call on replay, owner
offline after commit, atomic projection/outbox/receipt, deterministic rebuild, migration upgrade, source evidence
unchanged, PII isolation, customer isolation, independent deadlines. Two new reliability cases were added and pass:
schema 2 → 4 and schema 3 → 4 deterministic rebuild, and accepted-candidate digest/schema transition.

## 13. Simplicity result

**Added (10 production types):** `SalesContextSnapshot`, `BuyerSnapshot`, `InitiatingActorSnapshot`,
`SettlementAttribution`, `FundingObligationScope`, `OrderComponentTotal`, `OrderComponentTotalConfiguration`,
`LegacySellingOfficePolicy`, `SellingOfficeKind`, and the `Projection/Compatibility/` pair.

**Deleted:** none. **Renamed:** `Order.BuyerActor*` → `Order.InitiatingActor` (with its DB columns).

**Deliberately not introduced:** commercial rule engine, settlement engine, party graph, DistributionChain aggregate,
workflow, polymorphic FK infrastructure, reflection mapping, custom serialization framework, a second
repository/unit-of-work abstraction, and any S2–S16 table.

**Deferred on purpose:** the typed in-memory candidate detail refactor (§18) was **not** attempted — it cannot be done
without touching the canonical bytes unless it is done carefully, and closure correctness came first. It stays recorded
as a post-closure item.

## 14. Deferred / blocked

| Item | State |
|---|---|
| `Live_owner_details_are_sold_into_a_local_order` | **`BLOCKED_ENVIRONMENT`** — `ORDERING_LIVE_AIROFFER_BASEURL` / `..._OFFERID` are not configured here, so the live endpoint could not be called. Not run, not counted as passed. The recorded live fixture remains regression evidence and now also proves strict `offerId` acceptance against a real payload |
| Funding disposition vocabulary | owner-required; `DOMAIN/06` names behaviours but never enumerates states |
| Resource unit / delivery-control / dependency-treatment vocabularies | owner-required; carried as opaque refs |
| Buyer identity on any surface | no surface supplies one; persisted as explicit NotSupplied |
| Typed in-memory candidate detail | post-closure simplification, not attempted |

Nothing was marked `BLOCKED_BY_SPEC`: no hard contradiction between the specification and the Pack or the source was found.

## 15. S1 verdict

`S1_CREATE_ORDER_READY_FOR_OWNER_APPROVAL`

## 16. S2

`S2_NOT_STARTED`
