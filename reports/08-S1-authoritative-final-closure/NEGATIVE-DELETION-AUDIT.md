# Negative deletion audit

Stage: 08-S1-authoritative-final-closure · 2026-09-21 · branch `k8s-stg`

**Purpose.** Stage 07 deleted 50 files, 16 Domain classes, 9 mapped tables and 153 mapped properties. The R2 pass then
restored 12 of those removals as Pack-required. This audit asks the opposite question from the one stage 07 asked:
instead of *"was this field earning its place?"*, it asks **"if this field were Pack-required, would anything have
caught its removal?"** — and then answers, for each deletion, what the guarantee is today.

Sources audited: `reports/07-S1-domain-simplification/DELETED-FIELDS.md` (9 sections), the Pack `DOMAIN/*` files it
cites, and the current source tree.

**Method.** For each deleted group: (1) locate the Pack statement, if any, that could require it; (2) state which of
four dispositions applies; (3) name the live evidence. A deletion with no disposition would be a defect; there are
none, but two are recorded as `DIVERGENCE` rather than as safe.

| Disposition | Meaning |
|---|---|
| `NOT-PACK` | The Pack never required it. It was a local invention, a mapper constant or a duplicate. |
| `EQUIVALENT` | The Pack requires the *fact*; S1 keeps it in a different, named place with the same or a stronger guarantee. |
| `DEFERRED` | The Pack requires it for Ordering; no S1 command can produce it; the stage that owns it is named in the ERRATA. |
| `DIVERGENCE` | S1 does not hold something the Pack asks for, and that is recorded as a gap, not as an improvement. |

**Residue check.** Every deleted type below was confirmed absent from live source. `grep` over `src/`, `tests/` and
`Contracts/` finds them only in (a) frozen migration `*.Designer.cs` snapshots, which are history and must not change,
and (b) `tests/…/S1/LegacyOrderFixture.cs`, which deliberately builds a pre-migration database for the upgrade test.
No live production or test code references a deleted type.

---

## 1. Generic validity machinery — `NOT-PACK` + `EQUIVALENT`

`ValidityFact`, `ObservedTimeFact`, `Order.OfferValidity`/`PriceValidity`/`TicketingValidity`,
`Order.ObservedTicketingDeadline`, `OrderPreparation.TicketingValidity`/`ValidityFacts`, `DeadlinePolicy`.

Pack: `DOMAIN/10-ELIGIBILITY-VERSIONS-AND-TIME.md` requires that offer validity, price validity and ticketing
deadline are **independent** facts with their own instants and are not conflated.

The Pack requires three instants, not a five-member state machine around each. What was removed is the wrapper; the
facts are `OrderPreparation.OfferExpiresAt`, `OrderPreparation.PriceValidUntil` and `Order.LastTicketingDate`, all
nullable, all carrying the source instant unchanged.

Evidence today: `ReliabilityClosureTests.R12_the_three_source_deadlines_keep_their_own_instants_and_are_never_conflated`
asserts three distinct instants survive acceptance and that none is copied into another;
`R12_a_missing_source_deadline_stays_null_and_is_not_filled_from_another_one` asserts a missing fact stays null rather
than being back-filled; `SC_S1_019_expired_owner_validity_blocks_the_sale_at_its_own_instant` asserts expiry blocks at
its own instant. These three tests did not exist when the deletion was made — the R12 pair was written in this stage
precisely because the deletion had no direct proof.

`DeadlinePolicy` had no production consumer and is absent from the tree: `NOT-PACK`.

## 2. Invented source identities — `NOT-PACK`

`OrderItem.SourceItemRef` (`"OFFER-PACKAGE"`), `OrderSegment.SourceSegmentRef` (`"{boundId}|{flightId}"`),
`OrderService.SourceServiceRef`, `FarePricingUnit.SourceUnitRef` (`"pricingUnits/{i}"`),
`FareConstruction.SourceContextRef`, the three `SourceDecisionRef` columns, `PricingLine.CandidateLineRef`,
`PricingLine.SourceBasisRef`.

Pack: `DOMAIN/15-EVIDENCE-TEMPORAL-CONSISTENCY.md` requires the accepted state to be traceable to its source. It does
not require a per-row string, and a string the mapper itself fabricated is not owner evidence.

What actually provides traceability today: `Order.SourceOfferId`, `Order.SourcePreparationId`,
`Order.AcceptedSnapshotDigest` (SHA-256 of the canonical candidate), `PricingLine.SourceOccurrencePath` (the exact
source occurrence, e.g. `tickets/0/coupons/0/pricings/1`), and the untouched raw owner payload in
`PreparationSourceEvidence.Payload` with its own hash.

Evidence today, new in this stage:
`SourceEvidenceRetentionTests.The_retained_payload_is_the_exact_owner_response_and_matches_its_recorded_hash` and
`Coupon_identity_and_sequence_dropped_from_the_domain_model_stay_readable_in_raw_evidence` prove that the owner facts
the domain model deliberately does not carry (`couponId`, coupon `sequence`, `travellerIndex`) are still recoverable
byte-for-byte from evidence. `Evidence_only_facts_are_not_promoted_into_the_normalized_candidate` proves the boundary
holds in the other direction.

`PriceChangeSet.SourceDecisionRef` is separately `DEFERRED` in ERRATA §2.1, because the Pack does list it on the
`PriceChangeSet` tuple — its removal was correct for S1 but it is a real Pack field, not an invention, and it is
recorded as deferred rather than as junk.

## 3. Stringified numeric identities — `NOT-PACK` (a correction, not a deletion)

17 `*Ref:string` columns became `*Id:int`/`long`.

Pack: `DOMAIN/13-PERSISTENCE-DATA-DICTIONARY.md` line 5 — "Local IDs bigint … external refs nvarchar with explicit
per-contract limits". The AirOffer contract supplies these as numbers, so they are not external string refs; storing
them as strings was the divergence, and typing them is the fix.

No fact was lost: each column holds the same value in its own type. The strings that genuinely are opaque —
`BoundId`, `SourceTravellerRef`, `SourceConversionRef`, `CoveredBoundOfferId`, `SettlementCategoryCode` — were kept as
strings.

Evidence today: `Every_supplied_air_offer_fact_survives_wire_candidate_order_sql_and_projection`.

## 4. Generic service framework — mixed

| Removed | Disposition | Guarantee today |
|---|---|---|
| `ServiceDetailSchemaRegistry`, `DetailSchema`, dynamic `Details` dictionary | `NOT-PACK` | A registry with one entry. Cabin, RBD and booking class are typed columns on `OrderAirTransportService`. Scenario 57 is `NOT_APPLICABLE` for this reason. |
| `OrderService.ServiceCode`, `Name`, `SupplierPartyRef`, `DeliveryProviderRef`, `Quantity`, `QuantityUnit` | `DEFERRED` | Pack `DOMAIN/02` line 19 lists them; AirOffer supplies none for air transport. ERRATA §1.5 names the owning stage. |
| `OrderService.PriceTreatment` | `DEFERRED` | ERRATA §1.2. Scenarios 4 and 5 are `DEFERRED` for this reason, not `UNTESTED`. |
| `OrderService.ServiceVersion` | `DEFERRED` | ERRATA §1.1. |
| `OrderService.ServiceType` | **restored in R2** | It is the EF TPH discriminator and reaches the public DTO. Guarded by `CK_OrderServices_ServiceType_Enum`. |
| the invented profile id `AIROFFER-OBSERVED-AIR-UNCERTIFIED` | `NOT-PACK` | A mapper constant presented as an owner profile. `FulfillmentProfileSnapshot` was **restored in R2** with `ProfileRef`/`ProfileVersion` nullable, and a profile claiming `Certified` without naming itself is refused (`CandidateValidator.EnsureFulfillmentProfiles`). Post-R2 added the paired check that ref and version are supplied together or not at all. |
| `OrderServiceBeneficiary`, `OrderServiceCoverage`, `SoleCoveredSegmentId` | `EQUIVALENT` | Collections that always held exactly one row. Exactly one traveller and one segment are now structural invariants of `OrderAirTransportService`, enforced at four levels (ACL, canonical reader, candidate validation, domain construction) and by the unique index `(OrderId, TravellerId, SegmentId)` and composite FKs 6 and 7 of `CURRENT-OWNERSHIP-FK-MATRIX.md`. The Pack's "missing beneficiary" scenario is unreachable by construction rather than rejected at runtime. |

## 5. Item metadata — `NOT-PACK`

`ProductSnapshot` (8 members, of which 2 were populated and both duplicated the accepted source),
`OrderItem.SourceOfferItemRef` (always null — AirOffer supplies no offer-item identity),
`CommercialTermsSnapshot` (7 members derived from the per-service sold-term flags, which are retained as
`SoldTermRefundable`/`Changeable`/`Upgradable`).

Nothing the Pack names was lost: `DOMAIN/02` requires the sold terms, and they are still there per service, which is
where the owner supplies them.

## 6. Placeholder fare-construction structure — `NOT-PACK` + partly restored

| Removed | Disposition | Note |
|---|---|---|
| `FarePricingGroup`, `FarePricingGroupTraveler` | `NOT-PACK` | AirOffer supplies no pricing group; the tables were always empty. Scenario 41 is `NOT_APPLICABLE` for this reason. |
| `FareComponentService`, `FareComponentSegment` | `NOT-PACK` | No component↔service/segment binding is supplied; claiming one would be invention. `Opaque_construction_cannot_claim_component_links` asserts the absence is deliberate. |
| `FareComponent.FareOwnerRef`, `TariffRef`, `RuleRef`, `RoutingRef` | `NOT-PACK` | Never supplied; always null. |
| `FarePricingUnit.SourceKindRaw`, `CombinationMethod` | `NOT-PACK` | The raw kind is a known vocabulary now typed as `FarePricingUnitType` + `AirFareConstructionType`, both guarded by enum CHECKs and proven by `Every_owner_pricing_unit_kind_maps_to_its_own_construction_type` across all three real contract values. |
| `FareConstruction.Assurance` | **restored** | It is present today (`FareConstructionAssurance`, `CK_FareConstructions_Assurance_Enum`) and asserted by `SC_S1_010` / `AcceptedShapePersistenceTests`. The stage 07 entry is stale; this audit corrects it. |
| `FareConstruction.SupersededByConstructionId` | `DEFERRED` | Supersession is a repricing concept; no S1 command can write it. |

## 7. Lifecycle that cannot occur — `EQUIVALENT` + one `DIVERGENCE`

| Removed | Disposition | Guarantee today |
|---|---|---|
| `OrderPreparation.ConsumedByOrderId`, `ConsumedAt`, `IsConsumed`, `Consume()`, the consumption index and CHECK | **`DIVERGENCE` (direction only)** | Pack `DOMAIN/13` line 12 lists a "consumed OrderId" column on `OrderPreparations` and requires "unique consumption". S1 stores the relation in the opposite direction, `Orders.SourcePreparationId`, and enforces uniqueness with `UX_Orders_Owner_SourcePreparation (OwnerAirlineId, SourcePreparationId)` plus the composite FK to `OrderPreparations(OwnerAirlineId, Id)`. The **guarantee** is equivalent or stronger; the **shape** differs, and "which Order consumed this preparation" needs a lookup on `Orders`. Recorded in ERRATA §4. Proven by `ReliabilityClosureTests.R3_one_preparation_can_be_consumed_by_at_most_one_order` — raw SQL, error 2601/2627 naming the index. That test is new in this stage; the deletion previously had no direct proof. |
| `OrderPreparation.Channel`, `SellingOfficeId`, `ActorContextType`, `ActorId`, `ClientReference`, `CreatedAt` | `EQUIVALENT` | Duplicates of the candidate, the scope or the Order. `CallerScope` is kept because it is an input to the snapshot digest, which binds the accepted candidate to the scope that accepted it. `CandidateValidator.EnsureSalesContext` rejects a candidate whose scope differs. |
| `CommandReceipt.Status`, `CompletedAt`, `PreparationId`, `OperationId` | `NOT-PACK` | Only `Completed` was constructible; `CompletedAt` always equalled `CreatedAt`. Idempotency is `(OwnerAirlineId, CallerScope, CommandKind, IdempotencyKey)` and is proven by R1, R2 and R4. `CommandKind` is now guarded by `CK_CommandReceipts_CommandKind_Enum`. |
| `CommandReceiptStatus` (shared `Contracts`) | `NOT-PACK` | Internal receipt vocabulary that did not belong in the shared public contract. Absent from the tree. |

## 8. Order root — mixed, mostly restored

| Removed | Disposition | Note |
|---|---|---|
| `AcceptedSource` (14-member value object) | `EQUIVALENT` | Reduced to `SourceOfferId`, `SourcePreparationId`, `AcceptedSnapshotDigest`. The integration event still publishes `AcceptedSourceDigest`, so downstream traceability is unchanged. (The `AcceptedSource*` name still appears in `OrderCreatedDomainEvent` and `IntegrationEvents/V2/OrderCreated`; that is the digest field, not the removed value object.) |
| `Order.SourceJourneyTypeRaw` | `NOT-PACK` | Dual modelling. The enum is now the single representation and an unknown value fails closed; `CK_Orders_JourneyType_Enum` makes that a storage guarantee too. |
| `Order.IsSandboxScoped` | `NOT-PACK` | A convenience over the removed value object. Absent from the tree. |
| `Order.Buyer` / `BuyerSnapshot` | **restored in R2** | Pack `DOMAIN/01` and `DOMAIN/04` keep Buyer distinct from FinancialCustomer, Seller, Actor and Traveller; absence is itself the accepted fact. Now also guarded by `CK_Orders_BuyerContextType_Enum` and asserted by `AcceptedScopeAndLiabilityTests` (6 buyer tests) and `SC_10_…`. |
| `OrderComponentTotal` table | **restored in R2** | Only the fabricated `ROW_NUMBER` surrogate went; the key is `(OrderId, Component, Effect)`. Scenario 38 is now `TESTED`. |
| `FundingObligation.SourceDecisionRef`, `SupersededObligationId` | `NOT-PACK` / `DEFERRED` | `OrderServiceId`, `PricingLineId` and the typed scope were **restored in R2** because item-only scope silently dropped the liability of a customer-balance line with no item. Post-R2 added the negative-amount guard. The scope is now enforced by `CK_FundingObligations_ExactlyOneScope` and by composite FKs 19–21. |
| `PricingLine.OriginalPricingLineId` | `DEFERRED` | Reversals are an explicitly deferred slice. `Role` was **restored in R2** and is guarded by `CK_PricingLines_Role_Enum` across all four members, so the later slice does not have to widen the constraint. |
| `PriceChangeSet.BaseCommercialVersion` | `DEFERRED` | ERRATA §2.2. |
| `OrderItemServiceLinkTravelers`, `OrderItemServiceLinkSegments` | `NOT-PACK` | Overbuilt children; the typed air service owns traveller and segment scope. The link itself was **restored in R2**, and its three outbound foreign keys are now composite on `OrderIdAtAssociation` (FK matrix rows 10–12). `ScopeAtAssociation` remains `DEFERRED` as an open decision — ERRATA §1.3, `OD-S1-08`. |

## 9. Legacy compatibility — `NOT-PACK`

`LegacySellingOfficePolicy`, the schema-2/3 projection compatibility readers, the `NormalizedCandidate` 3.0 reader,
the `OrderDtoJson` schema-2 path, `PackExamples.cs` (~900 lines of schema-3 candidate JSON), and
`SellingOfficeKind.NotRecorded`.

Pack: `DOMAIN/13` requires that historical event payloads carry an explicit `SchemaVersion` and that new code read
retained supported versions. With no deployed S1 schema there is no retained version to support, so compatibility for
unapproved development rows was compatibility with nothing.

What the Pack rule needs today: the projection carries `ProjectionSchemaVersion`, and an unknown version is refused
rather than misread. That is new evidence in this stage —
`ReliabilityClosureTests.R6_a_projection_row_written_by_an_unknown_schema_version_is_refused_not_misread` writes
version 98 directly into `ReadModel.OrderDetails` and asserts `UnsupportedCapability` (20273). Before this stage the
removal of the compatibility readers left that behaviour unproven.

`SellingOfficeKind.NotRecorded` was removed together with the two validation branches that existed only to reject it;
an office identifier whose namespace cannot be proven now fails closed. `CK_Orders_SellingOfficeKind_Enum` admits only
`1, 2` or NULL, so the value cannot reappear through a direct write.

The four business intents `PackExamples.cs` fed have named replacement tests: one-way reference
(`Pack_one_way_reference_candidate_is_valid`), incorrect total
(`Pack_incorrect_total_is_rejected_against_its_pricing_lines`), settlement tax
(`Pack_settlement_tax_is_rejected_by_the_component_matrix`), missing beneficiary
(`Pack_missing_beneficiary_is_rejected_at_the_candidate_boundary`).

---

## 10. Findings

Three things this audit changed rather than confirmed:

1. **`FareConstruction.Assurance` is listed as deleted in stage 07 but exists today.** It was restored in R2 and the
   stage 07 entry was never corrected. Corrected in §6 above.
2. **Preparation consumption is a direction divergence, not an equivalence.** The stage 07 entry justified the removal
   by "a preparation can never commit unconsumed", which is a statement about the application flow, not about the
   database. The database guarantee is the unique index on `Orders`, which is a different claim and now has its own
   test. Recorded as `DIVERGENCE` in §7 and in ERRATA §4.
3. **Three deletions had no direct test until this stage.** The independence of the three deadlines (§1), the
   unique consumption of a preparation (§7) and the refusal of an unknown projection schema version (§9) were argued
   in prose in stage 07 and are now asserted by `ReliabilityClosureTests`. Those three tests are the reason this audit
   is not purely a re-reading of the previous report.

No deletion was found that removed a Pack-required fact without a named equivalent, a named deferral or a recorded
divergence.
