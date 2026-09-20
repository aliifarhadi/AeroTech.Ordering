# Pack-Required Shape Gap Matrix

Stage: 07-S1-domain-simplification (revision 2) · 2026-09-20 · branch `k8s-stg` · reviewed HEAD `3e43d37`

`FIELD-INVENTORY.md` inventories fields that exist. It cannot see a Pack-required fact that was deleted outright, which
is how the R2 review found nine regressions the inventory had missed. This matrix compares the Pack requirement against
the code directly, so absence is visible.

## Verdicts

| Verdict | Meaning |
|---|---|
| `PRESENT_CORRECT` | the fact exists, typed and named as the Pack requires |
| `PRESENT_WRONG_TYPE` | the fact exists under the wrong type |
| `PRESENT_WRONG_NAME` | the fact exists under a misleading name |
| `MISSING_RESTORE_NOW` | a Pack-required fact is absent and must come back before the model is frozen |
| `SOURCE_NOT_SUPPLIED_DO_NOT_INVENT` | the Pack asks for it, no owner supplies it, and inventing it would be a lie |
| `SAFE_DEFER_WITH_MIGRATION_GATE` | deferrable, and its return is a schema change that must be gated |
| `BLOCKED_OWNER_CONTRACT` | the fact is required but has no approved vocabulary or owner meaning |

`At R2` is the verdict the review found at `3e43d37`. `Now` is the verdict after these corrections.

## 1. Order root and parties

| Pack section | Aggregate / entity | Required fact | Current implementation | Source supplied? | Current consumer? | At R2 | Now |
|---|---|---|---|---|---|---|---|
| DOMAIN/01 §2, DOMAIN/04 | `Order` | Buyer is its own party, distinct from FinancialCustomer / Seller / Actor / Traveller | `Order.Buyer : BuyerSnapshot`, in the candidate and in the digest; `NotSupplied` is the honest value every current surface writes | no surface supplies a buyer today; absence is itself the accepted fact | `AcceptedScopeAndLiabilityTests` (five scope-match cases), persisted column, digest | `MISSING_RESTORE_NOW` | **`PRESENT_CORRECT`** — and, since the independent closure pass, the candidate buyer must equal the authorized scope's buyer |
| DOMAIN/01 | `Order` | FinancialCustomer identity | `Order.FinancialCustomerId : long` | yes | everywhere | `PRESENT_CORRECT` | `PRESENT_CORRECT` |
| DOMAIN/01 | `Order` | Seller party and selling office, each with its namespace | `SalesContextSnapshot` (`SellerContextType`, `SellerId`, `SellingOfficeKind`, `SellingOfficeId`) | yes | projection + public DTO | `PRESENT_CORRECT` | `PRESENT_CORRECT` |
| DOMAIN/01 | `Order` | Initiating actor | `InitiatingActorSnapshot` | yes | projection | `PRESENT_CORRECT` | `PRESENT_CORRECT` |
| DOMAIN/01 | `Order` | `RootOrderId` — original order is its own root, a split child is not | `RootOrderId = Id` at creation; DB check is now `[RootOrderId] > 0` instead of `[RootOrderId] = [Id]` | n/a (local) | split (S9) | `PRESENT_WRONG_TYPE` (DB pinned equality forever) | **`PRESENT_CORRECT`** |
| DOMAIN/03 | `Order` | sale currency identity plus the source currency code snapshot when supplied | `CurrencyId : int` everywhere, plus one `Order.SaleCurrencyCode : string?` | AirOffer supplies both `currencyId` and `currencyCode` | public DTO, tests | `MISSING_RESTORE_NOW` (code lost) | **`PRESENT_CORRECT`** |

## 2. Items and services

| Pack section | Aggregate / entity | Required fact | Current implementation | Source supplied? | Current consumer? | At R2 | Now |
|---|---|---|---|---|---|---|---|
| DOMAIN/02 | `OrderService` | a service declares its type | `OrderService.ServiceType : OrderServiceType`, the EF discriminator, projected and in the public DTO | one type in S1 (`AirTransportation`) | SQL discriminator, projection, DTO | `MISSING_RESTORE_NOW` | **`PRESENT_CORRECT`** |
| DOMAIN/02 | `OrderService` | typed service boundary, not one entity carrying another type's facts | `abstract OrderService` (Id, OrderId, OrderItemId, ServiceType, FulfillmentProfile, CommercialStatus, CreatedByChangeId) + `OrderAirTransportService` (TravellerId, SegmentId, CabinClassId, RbdId, BookingClass, baggage, sold terms) | yes | repository, projector, tests | `MISSING_RESTORE_NOW` (one sealed air-service-in-disguise) | **`PRESENT_CORRECT`** |
| DOMAIN/02, INV-010 | `OrderAirTransportService` | exactly one traveller on one passenger segment | required positive `TravellerId` + `SegmentId`, enforced in the constructor (20293) and by a unique index | yes | four-level missing-beneficiary evidence | `PRESENT_CORRECT` | `PRESENT_CORRECT` |
| DOMAIN/02 | `OrderService` | sale-time fulfillment context | `FulfillmentProfileSnapshot` with nullable `ProfileRef`/`ProfileVersion`, `Assurance`, `ReservationRequirement`, `DocumentKind`, `DocumentAuthority?`, `FundingRequirement`, `CapacityUnits?`, three policy refs, `PartialFulfillmentSupported?` | AirOffer supplies none of it: `NotCertified` + `Unresolved` + nulls, and no invented profile id | persisted, validated, tested | `MISSING_RESTORE_NOW` (deleted with the fake id) | **`PRESENT_CORRECT`** — identity and version are now paired, and `Certified` requires both |
| DOMAIN/02 | `OrderItemServiceLink` | immutable historical item↔service membership | `OrderItemServiceLink` (LinkId, OrderIdAtAssociation, OrderItemId, OrderServiceId, LinkedByChangeId); no traveller/segment child tables | n/a (local history) | persisted, tested | `MISSING_RESTORE_NOW` | **`PRESENT_CORRECT`** |
| DOMAIN/02 | `OrderItemServiceLink` | traveller/segment scope at association | not restored; the typed AirTransport service owns traveller and segment | n/a | — | — | **`SAFE_DEFER_WITH_MIGRATION_GATE`** (S14 needs it when membership can change) |
| DOMAIN/02 | `OrderItem` | product / brand facts | absent | AirOffer supplies none | — | `SOURCE_NOT_SUPPLIED_DO_NOT_INVENT` | `SOURCE_NOT_SUPPLIED_DO_NOT_INVENT` |
| DOMAIN/02 | `OrderItem` | item commercial terms summary | absent; per-service sold terms are retained | derived only | — | `SOURCE_NOT_SUPPLIED_DO_NOT_INVENT` | `SOURCE_NOT_SUPPLIED_DO_NOT_INVENT` |

## 3. Pricing

| Pack section | Aggregate / entity | Required fact | Current implementation | Source supplied? | Current consumer? | At R2 | Now |
|---|---|---|---|---|---|---|---|
| DOMAIN/03 | `PricingLine` | `LineRole` (Original / Reversal / Adjustment / Transfer) | `PricingLine.Role : PricingLineRole`, in the candidate, persisted and projected; the mapper writes `Original` | the owner supplies only original sales today | SQL, projection, tests | `MISSING_RESTORE_NOW` | **`PRESENT_CORRECT`** |
| DOMAIN/03 | `PricingLine` | `OriginalPricingLineId` for a reversal | absent | no reversal in S1 | — | `SAFE_DEFER_WITH_MIGRATION_GATE` | `SAFE_DEFER_WITH_MIGRATION_GATE` |
| DOMAIN/01 | `OrderComponentTotal` | complete current component totals, derived **and persisted** | `OrderComponentTotal` with composite key `(OrderId, Component, Effect)`, `DebitAmount`, `CreditAmount`, `CurrencyId`, computed at acceptance from `PricingLine.SaleValue`; the projector reads the persisted rows | derived | SQL, projection | `MISSING_RESTORE_NOW` (recomputed only in the projector) | **`PRESENT_CORRECT`** |
| DOMAIN/03 | `PricingLine` | settlement attribution pairing | `SettlementAttribution` + SQL CHECK | yes | tests | `PRESENT_CORRECT` | `PRESENT_CORRECT` |
| DOMAIN/03 | `PricingLine` | exact source occurrence | `SourceOccurrencePath` | yes | pricing audit | `PRESENT_CORRECT` | `PRESENT_CORRECT` |
| DOMAIN/03 | `AppliedConversion` | conversion evidence, never recomputed | `AppliedConversion` with `FromCurrencyId`/`ToCurrencyId` | yes | tests | `PRESENT_CORRECT` | `PRESENT_CORRECT` |

## 4. Funding

| Pack section | Aggregate / entity | Required fact | Current implementation | Source supplied? | Current consumer? | At R2 | Now |
|---|---|---|---|---|---|---|---|
| DOMAIN/06 | `FundingObligation` | scope is Item **or** Service **or** PricingLine | `OrderItemId?`, `OrderServiceId?`, `PricingLineId?`, real FKs, `CK_FundingObligations_ExactlyOneScope`, built through the typed `FundingObligationScope` factory | derived | SQL CHECK test | `MISSING_RESTORE_NOW` (item-only, and a line with no item silently lost its liability) | **`PRESENT_CORRECT`** |
| DOMAIN/06 | `FundingObligation` | no customer-balance liability may disappear | `AddOriginalSaleObligations` scopes every `CustomerBalance` line and then asserts the obligations sum to `CustomerTotal` (20296); the candidate validator also rejects a customer-balance line with neither item nor service basis | derived | domain test | `MISSING_RESTORE_NOW` | **`PRESENT_CORRECT`** |
| DOMAIN/06 | `FundingObligation` | the pricing decision that created it | typed `PriceChangeSetId` relation; no fabricated `SourceDecisionRef` string | local | SQL FK | `MISSING_RESTORE_NOW` | **`PRESENT_CORRECT`** |
| DOMAIN/06 | `FundingObligation` | `CurrentDisposition` | not implemented | — | — | `BLOCKED_OWNER_CONTRACT` | **`BLOCKED_OWNER_CONTRACT`** — no approved vocabulary; the enum is not invented. See §7. |
| DOMAIN/06 | `FundingObligation` | `SupersededObligationId` lineage | absent | no supersession in S1 | — | `SAFE_DEFER_WITH_MIGRATION_GATE` | `SAFE_DEFER_WITH_MIGRATION_GATE` |

## 5. Fare construction

| Pack section | Aggregate / entity | Required fact | Current implementation | Source supplied? | Current consumer? | At R2 | Now |
|---|---|---|---|---|---|---|---|
| DOMAIN/03 §19–20 | `FareConstruction` | explicit item scope | `CandidateFareConstruction.ItemKeys` resolved to `FareConstructionItem` rows; naming an unknown item is refused | yes (the mapper scopes to the package item it built) | domain + SQL tests | `MISSING_RESTORE_NOW` (linked **all** order items) | **`PRESENT_CORRECT`** |
| DOMAIN/03 | `FareConstruction` | completeness / assurance of the retained graph | `FareConstructionAssurance` on the candidate and the entity; the mapper writes `SourceProvided` | yes | projection, tests | `MISSING_RESTORE_NOW` | **`PRESENT_CORRECT`** |
| DOMAIN/03 | `FareConstruction` | absent when the owner supplies none | `NormalizedCandidate.FareConstruction` is nullable; the test builder no longer fabricates a default unit or covered bound | yes | domain + SQL tests | `MISSING_RESTORE_NOW` (builder invented one) | **`PRESENT_CORRECT`** |
| DOMAIN/03 | `FarePricingUnit` | the owner's own construction vocabulary | `Type : FarePricingUnitType` **plus** `SourceConstructionType : AirFareConstructionType`; `OneWay`→OneWay/OneWay, `RoundTripFromOneWays`→RoundTrip/RoundTripFromOneWays, `RoundTripFare`→RoundTrip/RoundTrip; anything else fails closed | yes | three-value theory test | `PRESENT_WRONG_TYPE` (mapper accepted `RoundTrip`/`OpenJaw`/`CircleTrip`, which the contract never sends) | **`PRESENT_CORRECT`** |
| DOMAIN/03 | `FarePricingUnitCoveredBound` | covered bound identities | `CoveredBoundOfferId`, opaque, no join to `Journey.BoundId` | yes, but the namespace is unproven | audit only | `BLOCKED_REAL_CONTRACT` | **`BLOCKED_REAL_CONTRACT`** — see `IDENTITY-NAMESPACE-MATRIX.md` |
| DOMAIN/03 | `FarePricingGroup` | passenger-group pricing | absent | AirOffer supplies none | — | `SOURCE_NOT_SUPPLIED_DO_NOT_INVENT` | `SOURCE_NOT_SUPPLIED_DO_NOT_INVENT` |
| DOMAIN/03 | `FareComponent` service/segment coverage | component↔service binding | absent | AirOffer supplies none | — | `SOURCE_NOT_SUPPLIED_DO_NOT_INVENT` | `SOURCE_NOT_SUPPLIED_DO_NOT_INVENT` |

## 6. Accepted source and receipts

| Pack section | Aggregate / entity | Required fact | Current implementation | Source supplied? | Current consumer? | At R2 | Now |
|---|---|---|---|---|---|---|---|
| DOMAIN/08 | `OrderPreparation` | accepted scope binding in the digest | `CallerScope` participates in `ComputeDigest`; it is **not** indexed | yes | digest | `PRESENT_CORRECT` (with a false rationale in the report) | **`PRESENT_CORRECT`** — rationale corrected in `FIELD-INVENTORY.md` |
| DOMAIN/08 | `OrderPreparation` | validity of the accepted source | `OfferExpiresAt?`, `PriceValidUntil?`, `LastTicketingDate?` | partly | `EnsureAcceptable` | `PRESENT_CORRECT` | `PRESENT_CORRECT` |
| DOMAIN/08 | `OrderPreparation` | single-use consumption | not restored: capture, acceptance and commit are one SQL unit and the receipt protects idempotency of the scoped command/key, not generic single-use offer consumption | n/a | — | `PRESENT_CORRECT` | `PRESENT_CORRECT` |
| ARCHITECTURE/03 | `CommandReceipt` | idempotent replay identity | `(OwnerAirlineId, CallerScope, CommandKind, IdempotencyKey)` + `RequestDigest` | yes | replay tests | `PRESENT_CORRECT` | `PRESENT_CORRECT` |
| API-CONTRACTS | rebuild result | the identity it returns is named for what it is | `RebuildOrderProjectionResult.ReceiptId` (was `OperationId` carrying `CommandReceipt.Id`) | n/a | test asserts the old name is gone | `PRESENT_WRONG_NAME` | **`PRESENT_CORRECT`** |
| API-CONTRACTS | create surfaces | one role name end to end | `FinancialCustomerId` on both create requests, commands, validators, controllers and the published OpenAPI document | n/a | OpenAPI test | `PRESENT_WRONG_NAME` | **`PRESENT_CORRECT`** |
| API-CONTRACTS | `SellingOfficeKind` | an office always declares its namespace | `NotRecorded` removed from the shared enum and from the validation branches that existed only to reject it | n/a | vocabulary + OpenAPI tests | `PRESENT_WRONG_TYPE` (dead compatibility member) | **`PRESENT_CORRECT`** |

## 6b. Defects closed by the independent closure pass

These were `PRESENT_CORRECT` in shape at `96f3690` but carried enforcement gaps. All six are closed; none needed a new
field, a new abstraction or a public-contract change.

| # | Pack section | Fact | Gap at `96f3690` | Correction | Test | Status |
|---|---|---|---|---|---|---|
| 1 | DOMAIN/01 §2, DOMAIN/04 | Buyer is its own party | `EnsureSalesContext` compared owner, customer and sales context but **not** Buyer, so a captured candidate could be sold under a different buyer | `candidate.SalesContext.Buyer == scope.Buyer`, fail closed | `A_candidate_buyer_is_accepted_only_under_the_same_buyer`, `A_candidate_buyer_cannot_be_sold_under_a_different_buyer`, `A_candidate_buyer_cannot_be_sold_under_a_scope_that_supplies_none`, `A_candidate_without_a_buyer_cannot_be_sold_under_a_scope_that_supplies_one`, `A_candidate_with_no_buyer_is_accepted_under_a_scope_with_no_buyer`, `A_buyer_is_never_inferred_from_another_role` | **`PRESENT_CORRECT`** |
| 2 | DOMAIN/06 | an original-sale obligation is a liability, never a refund | an item-less service-scoped `CustomerBalance` could net negative while the order total stayed positive; only SQL rejected it | candidate refuses a negative `CustomerTotal` and any negative service-scope net; `FundingObligation` refuses a negative amount (20295); SQL CHECK remains | `A_service_scope_whose_original_sale_net_is_negative_is_rejected_before_any_order`, `A_negative_original_sale_customer_total_is_rejected_before_any_order`, `A_positive_item_less_service_scoped_liability_remains_valid`, `A_negative_obligation_can_never_be_materialised_by_the_aggregate` | **`PRESENT_CORRECT`** |
| 3 | DOMAIN/02 | a fulfillment profile snapshot is identified by profile ID **and** version | only `Certified ⇒ ProfileRef` was required; ref-without-version, version-without-ref and `Certified` without a version all passed | both supplied or both absent; `Certified` requires both | `A_fulfillment_profile_supplies_its_identity_and_version_together_or_not_at_all`, `A_supplied_fulfillment_profile_pair_round_trips_unchanged`, `The_unresolved_live_profile_stays_unresolved` | **`PRESENT_CORRECT`** |
| 4 | DOMAIN/13 | required same-current-Order validation for beneficiary and coverage | FKs were `TravellerId → OrderTravellers.Id` and `SegmentId → OrderSegments.Id`, so a direct SQL mutation could point a service at another Order's traveller or segment | alternate keys `(OrderId, Id)` on both parents; composite FKs `(OrderId, TravellerId)` and `(OrderId, SegmentId)`, `Restrict` | `An_air_service_cannot_be_pointed_at_another_orders_traveller`, `An_air_service_cannot_be_pointed_at_another_orders_segment`, `A_same_order_traveller_and_segment_reference_remains_valid` | **`PRESENT_CORRECT`** |
| 5 | DOMAIN/13 | one monetary set per commercial change | unique `(OrderId, FinancialSequence)` existed, but nothing stopped two sets referencing one `ChangeId` | unique index on `PriceChangeSet.ChangeId` | `A_commercial_change_cannot_carry_a_second_price_change_set`, `A_different_commercial_change_may_carry_its_own_price_change_set` | **`PRESENT_CORRECT`** |
| 6 | DOMAIN/03 | every source pricing occurrence is preserved | fare + several taxes and repeated identical tax codes had no executable test | one fare + four taxes, three codes, `AT` repeated at two occurrences with two references | `Every_tax_occurrence_on_one_ticket_survives_acceptance_sql_and_rebuild` | **`PRESENT_CORRECT`** |

## 7. Open verdicts

No `MISSING_RESTORE_NOW` remains.

Two entries remain unresolved, and neither is a gap the agent may close alone:

- **`FundingObligation.CurrentDisposition` — `BLOCKED_OWNER_CONTRACT`.** The Pack requires a current disposition, but
  there is no approved vocabulary for it. Nothing in S1 changes an obligation after the original sale, so no value
  could be written honestly today. The enum is not invented. This needs an owner decision before S5/S6.
- **`coveredBoundOfferIds` — `BLOCKED_REAL_CONTRACT`.** The recorded live payload carries a `flightId` under a bound
  name. Stored opaque under the owner's own name, joined to nothing.

`SAFE_DEFER_WITH_MIGRATION_GATE` entries (`OriginalPricingLineId`, `SupersededObligationId`, link traveller/segment
scope) each return as a schema change and are listed in `MIGRATION-DECISION.md` as the reason the baseline must not be
frozen casually.
