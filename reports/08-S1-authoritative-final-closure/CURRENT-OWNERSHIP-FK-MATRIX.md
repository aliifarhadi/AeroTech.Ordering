# Current-ownership foreign key matrix

Stage: 08-S1-authoritative-final-closure · 2026-09-21 · branch `k8s-stg` · base HEAD `e9fe97a`

Pack basis: `DOMAIN/01-AGGREGATES.md` (the Order is the consistency boundary), `DOMAIN/02-COMMERCIAL-COMPOSITION.md`
line 13 ("Current service owns exactly one OrderId/OrderItemId"), `DOMAIN/13-PERSISTENCE-DATA-DICTIONARY.md`
(constraint inspection is stage evidence).

**The rule this matrix enforces.** Inside the Order aggregate, a child row must not be able to reference a row that
belongs to a different Order, and an Order must not be able to reference a preparation or receipt that belongs to a
different owner airline. Before this stage most of these relations were single-column foreign keys — `CreatedByChangeId`
pointed at *some* `OrderChange`, not necessarily at a change of the same Order — so the aggregate boundary was an
application convention, not a database fact.

Each relation below is now a composite foreign key whose first column is the scoping column (`OrderId`,
`OrderIdAtAssociation`, or `OwnerAirlineId`), referencing a composite alternate key on the principal.

## 1. Enforced relations

| # | Dependent table | Foreign key columns | Principal table | Principal key | Constraint |
|---|---|---|---|---|---|
| 1 | `Orders` | `OwnerAirlineId`, `SourcePreparationId` | `OrderPreparations` | `OwnerAirlineId`, `Id` | `FK_Orders_OrderPreparations_OwnerAirlineId_SourcePreparationId` |
| 2 | `CommandReceipts` | `OwnerAirlineId`, `OrderId` | `Orders` | `OwnerAirlineId`, `Id` | `FK_CommandReceipts_Orders_OwnerAirlineId_OrderId` |
| 3 | `OrderItems` | `OrderId`, `CreatedByChangeId` | `OrderChanges` | `OrderId`, `Id` | `FK_OrderItems_OrderChanges_OrderId_CreatedByChangeId` |
| 4 | `OrderServices` | `OrderId`, `OrderItemId` | `OrderItems` | `OrderId`, `Id` | `FK_OrderServices_OrderItems_OrderId_OrderItemId` |
| 5 | `OrderServices` | `OrderId`, `CreatedByChangeId` | `OrderChanges` | `OrderId`, `Id` | `FK_OrderServices_OrderChanges_OrderId_CreatedByChangeId` |
| 6 | `OrderServices` (air) | `OrderId`, `TravellerId` | `OrderTravellers` | `OrderId`, `Id` | pre-existing |
| 7 | `OrderServices` (air) | `OrderId`, `SegmentId` | `OrderSegments` | `OrderId`, `Id` | pre-existing |
| 8 | `OrderSegments` | `OrderId`, `JourneyId` | `OrderJourneys` | `OrderId`, `Id` | `FK_OrderSegments_OrderJourneys_OrderId_JourneyId` |
| 9 | `OrderTravellers` | `OrderId`, `InfantParentTravellerId` | `OrderTravellers` | `OrderId`, `Id` | `FK_OrderTravellers_OrderTravellers_OrderId_InfantParentTravellerId` |
| 10 | `OrderItemServiceLinks` | `OrderIdAtAssociation`, `OrderItemId` | `OrderItems` | `OrderId`, `Id` | `FK_OrderItemServiceLinks_OrderItems_OrderIdAtAssociation_OrderItemId` |
| 11 | `OrderItemServiceLinks` | `OrderIdAtAssociation`, `OrderServiceId` | `OrderServices` | `OrderId`, `Id` | `FK_OrderItemServiceLinks_OrderServices_OrderIdAtAssociation_OrderServiceId` |
| 12 | `OrderItemServiceLinks` | `OrderIdAtAssociation`, `LinkedByChangeId` | `OrderChanges` | `OrderId`, `Id` | `FK_OrderItemServiceLinks_OrderChanges_OrderIdAtAssociation_LinkedByChangeId` |
| 13 | `PriceChangeSets` | `OrderId`, `ChangeId` | `OrderChanges` | `OrderId`, `Id` | `FK_PriceChangeSets_OrderChanges_OrderId_ChangeId` |
| 14 | `PricingLines` | `OrderId`, `PriceChangeSetId` | `PriceChangeSets` | `OrderId`, `Id` | `FK_PricingLines_PriceChangeSets_OrderId_PriceChangeSetId` |
| 15 | `PricingLines` | `OrderId`, `OrderItemId` | `OrderItems` | `OrderId`, `Id` | `FK_PricingLines_OrderItems_OrderId_OrderItemId` |
| 16 | `FareConstructions` | `OrderId`, `CreatedByChangeId` | `OrderChanges` | `OrderId`, `Id` | `FK_FareConstructions_OrderChanges_OrderId_CreatedByChangeId` |
| 17 | `FundingObligations` | `OrderId`, `ChangeId` | `OrderChanges` | `OrderId`, `Id` | `FK_FundingObligations_OrderChanges_OrderId_ChangeId` |
| 18 | `FundingObligations` | `OrderId`, `PriceChangeSetId` | `PriceChangeSets` | `OrderId`, `Id` | `FK_FundingObligations_PriceChangeSets_OrderId_PriceChangeSetId` |
| 19 | `FundingObligations` | `OrderId`, `OrderItemId` | `OrderItems` | `OrderId`, `Id` | `FK_FundingObligations_OrderItems_OrderId_OrderItemId` |
| 20 | `FundingObligations` | `OrderId`, `OrderServiceId` | `OrderServices` | `OrderId`, `Id` | `FK_FundingObligations_OrderServices_OrderId_OrderServiceId` |
| 21 | `FundingObligations` | `OrderId`, `PricingLineId` | `PricingLines` | `OrderId`, `Id` | `FK_FundingObligations_PricingLines_OrderId_PricingLineId` |

Principal alternate keys added by `S1AuthoritativeFinalShape`: `Orders(OwnerAirlineId, Id)`,
`OrderPreparations(OwnerAirlineId, Id)`, `OrderChanges(OrderId, Id)`, `OrderItems(OrderId, Id)`,
`OrderServices(OrderId, Id)`, `OrderJourneys(OrderId, Id)`, `PriceChangeSets(OrderId, Id)`,
`PricingLines(OrderId, Id)`. `OrderTravellers(OrderId, Id)` and `OrderSegments(OrderId, Id)` already existed.

**Nullable dependent columns.** Rows 9, 15 and 19–21 have a nullable second column (`InfantParentTravellerId`,
`OrderItemId`, `PricingLineId`, …). SQL Server does not check a composite foreign key when any of its columns is NULL,
which is the intended semantics: *no* association is always allowed, and *any* association must be in scope. The first
column (`OrderId`) is never null, so whenever the association exists the constraint is checked.

## 2. Proof

`tests/AeroTech.Ordering.Persistence.Tests/S1/CurrentOwnershipConstraintTests.cs` — 17 tests.

| Claim | Test |
|---|---|
| Every row of §1 is a live composite FK in the database | `Every_scoped_relation_in_the_matrix_is_a_live_composite_foreign_key` |
| No order-scoped table keeps a single-column FK to another order-scoped table | `No_order_scoped_table_keeps_a_single_column_foreign_key_to_another_order_scoped_table` |
| An Order cannot be moved to an owner that did not prepare it | `An_order_cannot_be_moved_to_an_owner_that_did_not_prepare_it` |
| A command receipt cannot be moved to another owner | `A_command_receipt_cannot_be_moved_to_another_owner_while_it_points_at_an_order` |
| Rows 3, 4, 5, 15 — child cannot point at another order's parent | `A_child_row_cannot_be_pointed_at_a_parent_of_another_order` (4 cases) |
| Rows 10, 11, 12 — a link cannot bind another order's row | `An_item_service_link_cannot_bind_a_row_of_another_order` (3 cases) |
| Row 13 | `A_price_change_set_cannot_be_attached_to_a_commercial_change_of_another_order` |
| Row 14 | `A_pricing_line_cannot_be_moved_into_a_price_change_set_of_another_order` |
| Row 16 | `A_fare_construction_cannot_be_attributed_to_a_commercial_change_of_another_order` |
| Row 8 | `A_segment_cannot_be_moved_into_a_journey_of_another_order` |
| Row 9 | `A_traveller_cannot_be_given_a_guardian_from_another_order` |
| A legitimate same-order move is still allowed | `A_same_order_reassociation_of_a_service_to_its_own_item_remains_valid` |

Each behavioural test issues raw SQL against SQL Server, bypassing the domain and EF entirely, and asserts error 547
naming the exact constraint. Rows 6 and 7 are additionally covered by the pre-existing
`AcceptedScopeConstraintTests`.

**Red-first evidence.** `Every_scoped_relation_in_the_matrix_is_a_live_composite_foreign_key` failed on its first run
with `["CommandReceipts(OwnerAirlineId, OrderId) -> Orders"]`, because the inspection query filtered on schema `Order`
only and `CommandReceipts` lives in `Operations`. The matrix test found a real hole in its own evidence before it went
green. Several behavioural cases also failed first — `PriceChangeSets`, `OrderSegments` and `PricingLines` raised 2601
(unique index) instead of 547, because the naive cross-order update collided with a unique index before reaching the
foreign key. Those tests were rewritten to point at a spare parent row in the other Order so that the foreign key, and
nothing else, is what rejects them.

## 3. Not enforced, and why

| Relation | Why it is not a composite foreign key | Disposition |
|---|---|---|
| `FareConstructionItems(FareConstructionId, OrderItemId)` -> `OrderItems` | The join table carries no `OrderId`. Enforcing the scope needs a new scoping column on the table, which is a persistence-identity decision this stage may not make on its own. | OPEN. Recorded here, not silently skipped. The `FareConstructions` row itself is order-scoped (row 16), so an out-of-scope item can only be reached by a direct SQL write that also bypasses the construction. |
| `FarePricingUnits` -> `FareConstructions`, `FareComponents` -> `FarePricingUnits`, `FarePricingUnitCoveredBounds` -> `FarePricingUnits`, `OrderSegmentLegs` -> `OrderSegments`, `PreparationSourceEvidence` -> `OrderPreparations`, `OrderTravellerIdentities` -> `OrderTravellers` | Single-parent child rows. They carry no `OrderId` because their scope *is* their parent: there is exactly one path from the row to its Order, through a parent that is itself scoped. A composite key would restate the parent's own key. | Correct as single-column. |
| `Orders` -> `Orders` (`RootOrderId`) | Self-reference across Orders by design (`DOMAIN/01`: an Order tree shares a root). Scoping it to one Order would contradict its purpose. | Correct as single-column. |

## 4. What this does not prove

- It does not prove application code never *attempts* a cross-order write; it proves the database refuses one.
- It does not cover cross-owner isolation for tables that carry no `OwnerAirlineId` (every Order-scoped child).
  Those inherit owner scope transitively through `Orders`, which is owner-keyed (row 1 and row 2 are the two places
  where an owner boundary is actually crossable).
- `FareConstructionItems` is stated above as a real remaining hole, not as covered.
