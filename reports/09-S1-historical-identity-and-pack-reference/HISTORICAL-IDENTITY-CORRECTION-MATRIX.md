# Historical identity correction matrix

Stage: 09-S1-historical-identity-and-pack-reference · 2026-09-21 · branch `k8s-stg` · base HEAD `14758c6`

## 1. The mistake being corrected

Stage 08 applied one rule to every Order-scoped reference: *if both sides carry an `OrderId`, make the foreign key
composite on it.* That rule is correct for current containment and **wrong for immutable history**, and the Pack says
so in three places:

- `DOMAIN/13-PERSISTENCE-DATA-DICTIONARY.md`, `ItemServiceLinks/ServiceLineage/ItemLineage` row — "Append-only,
  explicit many-to-many; **historical refs not constrained to current owner**".
- `DOMAIN/01-AGGREGATES.md` line 55 — "**Current service ownership can move through split without changing
  ServiceId.** History retains OrderIdAtOccurrence/Association."
- `DOMAIN/12-SPLIT-GROUPS-AND-RELATED-ORDERS.md` step 3 — "move current traveler/service ownership, partition current
  items … **Preserve all historical OrderIdAtOccurrence** and issued facts."

Concretely: an `OrderService` keeps its `ServiceId` and moves to a child Order during a split (S14). Stage 08 bound
`OrderServices.CreatedByChangeId` to `(OrderId, CreatedByChangeId) -> OrderChange(OrderId, Id)`. After the move the
service's current `OrderId` is the child's, while its creating change still belongs to the source Order — so the
constraint would have made the Pack-mandated split **impossible at the database level**. The same applies to every
historical reference that Stage 08 scoped to a current owner.

## 2. The rule the schema now follows

| Kind | Test | Foreign key shape |
|---|---|---|
| **Current containment** | Dependent and principal must be in the same Order *right now*, and they move together | composite on the current scoping column |
| **Immutable historical occurrence** | The reference records what was true when it happened; the dependent identity may later move to another Order | stable-identity FK on the principal's primary key, **plus** an explicit occurrence `OrderId` column that is a recorded fact — not an FK scope |

The question applied to every Stage-08 composite:

> Can the dependent identity move current Order while this referenced occurrence must remain unchanged?

Yes → historical, stable-identity FK. No → current containment, keep the composite.

## 3. Relations kept as current containment (14)

| # | Dependent | Columns | Principal | Why it is containment |
|---|---|---|---|---|
| 1 | `Orders` | `OwnerAirlineId`, `SourcePreparationId` | `OrderPreparations` | An Order never changes owner airline; `DOMAIN/12` requires `OwnerAirlineId` identical across source and child |
| 2 | `CommandReceipts` | `OwnerAirlineId`, `OrderId` | `Orders` | Receipt scope is the owner + Order it was issued for; neither moves |
| 3 | `OrderItems` | `OrderId`, `CreatedByChangeId` | `OrderChanges` | Item identity does not move current Order — see §5 |
| 4 | `OrderServices` | `OrderId`, `OrderItemId` | `OrderItems` | `DOMAIN/13`: "unique **current** ownership". Both columns update together when the service moves |
| 5 | `OrderAirTransportService` | `OrderId`, `TravellerId` | `OrderTravellers` | `DOMAIN/13`: "**required same-current-Order validation**" for beneficiaries/coverage |
| 6 | `OrderAirTransportService` | `OrderId`, `SegmentId` | `OrderSegments` | Same |
| 7 | `OrderSegments` | `OrderId`, `JourneyId` | `OrderJourneys` | A segment belongs to the journey of its own Order; `DOMAIN/12` creates **child-local journey snapshots**, so segments never point across Orders |
| 8 | `OrderTravellers` | `OrderId`, `InfantParentTravellerId` | `OrderTravellers` | `DOMAIN/12`: "Adult and linked infant **remain together**" — they move as a pair, never apart |
| 9 | `OrderItemServiceLinks` | `OrderIdAtAssociation`, `LinkedByChangeId` | `OrderChanges` | Both sides are immutable occurrence facts; neither can move. Explicitly permitted by the stage prompt §A2 |
| 10 | `PriceChangeSets` | `OrderId`, `ChangeId` | `OrderChanges` | A price change set is committed inside its change and never moves |
| 11 | `PricingLines` | `OrderId`, `PriceChangeSetId` | `PriceChangeSets` | A line belongs to the set it was committed in; neither moves |
| 12 | `FareConstructions` | `OrderId`, `CreatedByChangeId` | `OrderChanges` | `DOMAIN/13`: "**historical construction immutable**" — the construction is not in `DOMAIN/12`'s move list |
| 13 | `FundingObligations` | `OrderId`, `ChangeId` | `OrderChanges` | Both immutable occurrence facts |
| 14 | `FundingObligations` | `OrderId`, `PriceChangeSetId` | `PriceChangeSets` | Both immutable occurrence facts |

Rows 9–14 are composite **because neither side can move**, not because they are current ownership. A composite key
across an immutable occurrence pair is not a current-owner constraint.

## 4. Relations corrected to stable-identity historical references (7)

| # | Dependent | Was (stage 08) | Now | Occurrence `OrderId` retained |
|---|---|---|---|---|
| 1 | `OrderServices.CreatedByChangeId` | `(OrderId, CreatedByChangeId) -> OrderChange(OrderId, Id)` | `-> OrderChange(Id)` | `OrderServices.OrderId` stays as the **current** owner column (containment row 4) |
| 2 | `OrderItemServiceLinks.OrderItemId` | `(OrderIdAtAssociation, OrderItemId) -> OrderItem(OrderId, Id)` | `-> OrderItem(Id)` | `OrderIdAtAssociation` **kept** |
| 3 | `OrderItemServiceLinks.OrderServiceId` | `(OrderIdAtAssociation, OrderServiceId) -> OrderService(OrderId, Id)` | `-> OrderService(Id)` | `OrderIdAtAssociation` **kept** |
| 4 | `PricingLines.OrderItemId` | `(OrderId, OrderItemId) -> OrderItem(OrderId, Id)` | `-> OrderItem(Id)` | `PricingLines.OrderId` **kept** |
| 5 | `FundingObligations.OrderItemId` | `(OrderId, OrderItemId) -> OrderItem(OrderId, Id)` | `-> OrderItem(Id)` | `FundingObligations.OrderId` **kept** |
| 6 | `FundingObligations.OrderServiceId` | `(OrderId, OrderServiceId) -> OrderService(OrderId, Id)` | `-> OrderService(Id)` | `FundingObligations.OrderId` **kept** |
| 7 | `FundingObligations.PricingLineId` | `(OrderId, PricingLineId) -> PricingLine(OrderId, Id)` | `-> PricingLine(Id)` | `FundingObligations.OrderId` **kept** |

**No occurrence field was deleted.** `OrderIdAtAssociation`, `PricingLine.OrderId` and `FundingObligation.OrderId`
are all still persisted, still non-null, and still foreign keys to `Orders` through the Order aggregate's child
collections. What changed is that they no longer act as a current-owner scope on the *referenced* row.

Alternate keys `(OrderId, Id)` on `OrderItems`, `OrderServices` and `PricingLines` are **preserved** even where no
foreign key now consumes them, per the stage prompt §D — later mappings still need them. EF dropped
`AK_OrderServices_OrderId_Id` on the first migration draft because it had become unreferenced; it is now declared
explicitly in `OrderServiceConfiguration` so it survives.

## 5. `OrderItem.CreatedByChangeId` — verdict: keep the composite

The stage prompt asked for this verdict "based on `DOMAIN/12` wording **and the Master Catalog**". The Master Catalog
was not supplied (`OD-S1-10`), so the verdict rests on four Pack sources instead of two:

1. `DOMAIN/01` line 55 names only **service** ownership as moving through split. Item is absent from the sentence.
2. `DOMAIN/02` line 7 — the `OrderItem` field list ends with "supersession/cancellation/**partition** references" and
   the paragraph closes with "where terms/product boundary changes, **a successor item**".
3. `DOMAIN/02` line 15 — `ItemLineage` is a many-to-many predecessor/successor relation, which is the shape a
   successor-item model needs and a move does not.
4. `DOMAIN/13` — the `OrderItems` row's mandatory constraint is literally "**FK current Order**", with no historical
   exemption, in direct contrast to the `ItemServiceLinks/…` row which states the exemption explicitly.

`DOMAIN/12` step 3's "partition current items" is ambiguous alone; read with (2) and (3) it means creating successor
child items, not relocating an existing `OrderItemId`.

**Kept, pending confirmation** (`OD-S1-11`). This is the reversible direction: relaxing it later is one migration,
whereas wrongly relaxing it now would have silently dropped a real guarantee with no test able to notice.

## 6. `OrderItemServiceLink` uniqueness — recorded, not asserted as permanent

`LinkId` is an **association occurrence identity**, not a pair identity. The Pack calls the row immutable and
append-only, and says "New membership supersedes a current binding; old links remain history" (`DOMAIN/02` line 13) —
which means the same (item, service) pair can legitimately be associated more than once over an Order's life.

The schema currently carries `IX_OrderItemServiceLinks_OrderItemId_OrderServiceId` as **unique**. That is retained as
an **S1-only** constraint: in S1 a service is associated exactly once and never re-associated, so the index is true
today and catches a duplicate-write bug.

**It is not a permanent domain invariant, and this stage does not state it as one.** The slice that introduces
re-association or split must drop or replace it. That gate belongs in the Master Pack materialization matrix
(blocked on `OD-S1-10`); until those documents exist, this paragraph is the record.

## 7. Proof

`tests/AeroTech.Ordering.Persistence.Tests/S1/HistoricalIdentityMobilityTests.cs` — 8 tests. All operate on raw SQL
against SQL Server; none implements any part of S14 behaviour.

| Claim | Test |
|---|---|
| A service moves to another same-owner Order and its creating change, links and obligations are untouched | `A_service_can_move_to_another_order_of_the_same_owner_without_touching_its_history` |
| The item-service link keeps `OrderIdAtAssociation` and its item after the move | `The_item_service_link_keeps_its_occurrence_order_after_the_service_moves` |
| Pricing lines keep their occurrence Order and item attribution after the move | `A_pricing_line_keeps_its_occurrence_order_and_item_after_the_service_moves` |
| A funding obligation scoped to the moved service stays valid and keeps its occurrence Order | `A_funding_obligation_scoped_to_the_moved_service_stays_valid_and_keeps_its_occurrence_order` |
| Current containment is still enforced: a bare `OrderId` change is rejected | `A_moved_service_must_still_take_the_current_item_traveller_and_segment_of_its_new_order` |
| Current containment is still enforced per reference: item, traveller and segment must all belong to the new Order | `A_moved_service_cannot_keep_a_containment_reference_to_its_previous_order` (3 cases, each naming its constraint) |

`CurrentOwnershipConstraintTests` was corrected in the same pass and now carries two matrix-driven tests instead of
one: `Every_current_containment_relation_is_a_live_composite_foreign_key` (14 relations) and
`Every_historical_reference_is_a_stable_identity_foreign_key_and_not_current_owner_scoped` (7 references, each
asserted to have exactly one foreign key of exactly one column). Four of its theory rows asserted historical
references as current ownership and were removed.

## 8. Red-first evidence

The correction was stashed — all four configurations, the new migration and the model snapshot — and the suites were
run against the uncorrected Stage-08 schema:

```
Failed! - Failed: 8, Passed: 14, Total: 22
```

Failing:

| Test | Failure under Stage 08 |
|---|---|
| `A_service_can_move_to_another_order_of_the_same_owner_without_touching_its_history` | 547 on `FK_OrderItemServiceLinks_OrderServices_OrderIdAtAssociation_OrderServiceId` |
| `The_item_service_link_keeps_its_occurrence_order_after_the_service_moves` | 547 on the same constraint |
| `A_pricing_line_keeps_its_occurrence_order_and_item_after_the_service_moves` | 547 — the move is refused |
| `A_funding_obligation_scoped_to_the_moved_service_stays_valid_…` | 547 on `FK_FundingObligations_OrderServices_OrderId_OrderServiceId` |
| `A_moved_service_cannot_keep_a_containment_reference_to_its_previous_order` (3 cases) | the wrong constraint fires first |
| `Every_historical_reference_is_a_stable_identity_foreign_key_and_not_current_owner_scoped` | all 7 references still composite |

The stash was popped immediately and verified empty; `git status` shows the correction restored intact.

This is the concrete proof that Stage 08 had made the Pack-mandated split undoable in SQL: the very first step of
`DOMAIN/12` step 3 — moving current service ownership — was rejected by a foreign key.
