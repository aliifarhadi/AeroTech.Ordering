# Test results

Stage: 09-S1-historical-identity-and-pack-reference · 2026-09-21 · branch `k8s-stg` · base HEAD `14758c6`

Raw output: `01-runs/`.

## 1. Final state

| Run | Command | Result |
|---|---|---|
| Build | `dotnet build AeroTech.Ordering.sln` | **0 errors**, 12 warnings (`01-runs/01-BUILD.txt`) |
| Domain tests | `dotnet test tests/AeroTech.Ordering.Domain.Tests --no-build` | **135 passed, 0 failed**, 0 skipped, 0.3 s (`01-runs/02-DOMAIN-TESTS.txt`) |
| Persistence tests | `--filter "Category!=Live"` | **241 passed, 0 failed**, 0 skipped, 2 m 28 s (`01-runs/03-PERSISTENCE-TESTS.txt`) |
| API + OpenAPI + architecture + composition + host | `--filter "…Api\|…Architecture\|…Composition\|…Host"` | **48 passed, 0 failed**, 41 s (`01-runs/05-API-AND-ARCHITECTURE.txt`) |
| Fresh DB + upgrade + atomicity + historical identity | `--filter "…MigrationUpgradeTests\|…AtomicityAndRebuildTests\|…HistoricalIdentityMobilityTests\|…CurrentOwnershipConstraintTests"` | **26 passed, 0 failed**, 42 s (`01-runs/06-MIGRATION-AND-HISTORICAL-IDENTITY.txt`) |
| Pending model changes, 3 contexts | `dotnet ef migrations has-pending-model-changes` | **No changes** for `OrderingDbContext`, `OrderQueryDbContext`, `ReferenceDbContext` (`01-runs/04-PENDING-MODEL-CHANGES.txt`) |

The `Live` category is excluded as in every previous stage; it needs a running AirOffer at `http://localhost:5095`.
It was not run and is not claimed.

Every persistence test builds its database from the full eleven-migration chain, so **241 fresh-database creations**
through `S1HistoricalIdentityCorrection` are part of this run.

## 2. Change from the base commit

| Suite | At `14758c6` | Now | Change |
|---|---|---|---|
| Domain | 135 | **135** | 0 |
| Persistence (`Category!=Live`) | 236 | **241** | +5 |
| total | 371 | **376** | **+5** |

The persistence delta is +8 new tests in `HistoricalIdentityMobilityTests` minus 4 removed theory cases plus 1 new
matrix test in `CurrentOwnershipConstraintTests` (17 → 18 methods, 21 → 22 cases, then 22 → 18 cases after the four
removals, then +8).

## 3. Tests added

`tests/AeroTech.Ordering.Persistence.Tests/S1/HistoricalIdentityMobilityTests.cs` — **8 tests**, all raw SQL against
SQL Server, none implementing any part of S14 behaviour.

| Test | Claim |
|---|---|
| `A_service_can_move_to_another_order_of_the_same_owner_without_touching_its_history` | A service moves Order keeping `ServiceId`; its `CreatedByChangeId` still points at the source Order's change; the link and obligation rows are byte-identical before and after |
| `The_item_service_link_keeps_its_occurrence_order_after_the_service_moves` | `OrderIdAtAssociation` and the linked item still resolve to the source Order |
| `A_pricing_line_keeps_its_occurrence_order_and_item_after_the_service_moves` | No pricing line left the source Order; no line points at an item of another Order |
| `A_funding_obligation_scoped_to_the_moved_service_stays_valid_and_keeps_its_occurrence_order` | The obligation keeps its occurrence `OrderId` and still joins to the service, now in the child Order |
| `A_moved_service_must_still_take_the_current_item_traveller_and_segment_of_its_new_order` | Changing only `OrderId` is rejected (547) and the row is unchanged |
| `A_moved_service_cannot_keep_a_containment_reference_to_its_previous_order` (3 cases) | Item, traveller and segment must each belong to the new Order; each case names its own constraint |

## 4. Tests corrected

`CurrentOwnershipConstraintTests` — the stage-08 file that encoded the mistake.

| Change | Why |
|---|---|
| `OwnerScopedRelations` (21 rows) split into `CurrentContainmentRelations` (14) and `HistoricalReferences` (7) | The two kinds have different correct shapes |
| `Every_scoped_relation_in_the_matrix_is_a_live_composite_foreign_key` → `Every_current_containment_relation_is_a_live_composite_foreign_key` | Scope narrowed to what the name now claims |
| **new** `Every_historical_reference_is_a_stable_identity_foreign_key_and_not_current_owner_scoped` | Asserts each of the 7 has exactly one foreign key of exactly one column — it fails if anyone re-adds a composite |
| `No_order_scoped_table_keeps_a_single_column_foreign_key_to_another_order_scoped_table` → `No_current_containment_reference_is_left_as_a_single_column_foreign_key`, with the 7 historical references exempted by name | The old blanket rule *was* the stage-08 mistake, expressed as a test |
| 4 theory cases removed: `OrderServices.CreatedByChangeId`, `PricingLines.OrderItemId`, `OrderItemServiceLinks.OrderItemId`, `OrderItemServiceLinks.OrderServiceId` | Each asserted a historical reference as current ownership |
| `An_item_service_link_cannot_bind_a_row_of_another_order` → `An_item_service_link_change_reference_stays_inside_its_occurrence_order` | Only the change reference remains composite, and for a different reason |

No test was weakened to go green. The four removed cases were removed because the behaviour they asserted is
**wrong**, and the new historical-reference test fails if that behaviour comes back.

## 5. Red-first evidence

The whole correction — all four entity configurations, the new migration, its designer file and the model snapshot —
was stashed with `git stash push -u`, and both suites were run against the uncorrected stage-08 schema.

`01-runs/07-RED-FIRST-AGAINST-STAGE08-SCHEMA.txt`:

```
Failed! - Failed: 8, Passed: 14, Skipped: 0, Total: 22
```

| Failing test | Failure |
|---|---|
| `A_service_can_move_to_another_order_of_the_same_owner_without_touching_its_history` | 547 on `FK_OrderItemServiceLinks_OrderServices_OrderIdAtAssociation_OrderServiceId` |
| `The_item_service_link_keeps_its_occurrence_order_after_the_service_moves` | 547 on the same constraint |
| `A_pricing_line_keeps_its_occurrence_order_and_item_after_the_service_moves` | 547 — the service move is refused outright |
| `A_funding_obligation_scoped_to_the_moved_service_stays_valid_…` | 547 on `FK_FundingObligations_OrderServices_OrderId_OrderServiceId` |
| `A_moved_service_cannot_keep_a_containment_reference_to_its_previous_order` (3 cases) | `Assert.Contains` — a historical constraint fires before the containment one being tested |
| `Every_historical_reference_is_a_stable_identity_foreign_key_and_not_current_owner_scoped` | `Assert.Empty` — all 7 references were still composite |

**This is the concrete proof that stage 08 made the Pack-mandated split impossible in SQL.** `DOMAIN/12` step 3 begins
"move current traveler/service ownership"; under the stage-08 schema that first move is rejected by a foreign key on
a *historical* row that the Pack requires to stay unchanged.

The stash was popped immediately, `git stash list` verified empty, and `git status` confirmed all eleven changed and
new files restored intact.

## 6. Failures encountered and fixed during the stage

| Failure | Cause | Fix |
|---|---|---|
| First migration draft contained `DropUniqueConstraint("AK_OrderServices_OrderId_Id")` | The alternate key had only ever been created implicitly through `HasPrincipalKey`; relaxing its two consumers left it unreferenced and EF removed it | Declared `HasAlternateKey` explicitly in `OrderServiceConfiguration`, discarded the draft, regenerated. Stage prompt §D requires preserving it |
| `FundingObligationPurpose.Collection` does not exist | The enum is `OriginalSale`, `AddedService`, `ExchangeAdditionalCollection`, `Fee`, `RefundDisposition` | Synthetic obligation uses `OriginalSale` |
| `Conversion failed when converting the varchar value '\|' to data type int` | The history-snapshot helper concatenated `COUNT(1)` (int) with a string separator | Explicit `CAST(… AS nvarchar(32))` |

## 7. What is not proven

- The `Live` category was not run.
- **No S14 behaviour was implemented or tested.** `HistoricalIdentityMobilityTests` performs a persistence-only
  service move to prove the *schema* permits what `DOMAIN/12` requires. It does not create split transfer lines,
  does not touch `CommercialVersion`, does not partition value, and must not be read as a split implementation.
- `OrderItem.CreatedByChangeId` stays composite on a four-source Pack reading, not on the Master Catalog the prompt
  named — see `OD-S1-11`.
- Phase B of the stage prompt (the seven `REFERENCE/` documents and the thirteen Pack cross-references) was not done:
  the Master Reference files were not supplied — see `OD-S1-10`.
