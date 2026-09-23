# Stage 09 — historical identity correction + Pack reference integration

Stage: 09-S1-historical-identity-and-pack-reference · 2026-09-21 · branch `k8s-stg` · base HEAD `14758c6`

**Status: `S1_HISTORICAL_IDENTITY_CORRECTED_PACK_REFERENCE_READY_FOR_ARCHITECT_REVIEW`**
**`S2_NOT_STARTED`**

S1 is **not** declared frozen. The chain is **not** rebaselined. The only planned stage after architect approval
remains `S1_MIGRATION_REBASELINE_AND_FREEZE`, subject to the owner authorisation recorded in
`reports/00-decisions/S1-MIGRATION-REBASELINE-OPEN-DECISION.md`.

Baseline check: HEAD was verified as `14758c6d4850e0bac74ca2246b9f59a62b318201` with a clean tree before any change.

---

## 1. Phase completion

| Phase | State |
|---|---|
| **A** — fix the stage 08 historical/current ownership mistake | **Done** |
| **B** — integrate the Master Domain Reference into Pack 3.8 | **BLOCKED** — files not supplied (`OD-S1-10`) |
| **B1** — Pack cross-references to the REFERENCE catalog | **BLOCKED** — depends on B |
| **B2** — resolve `OD-S1-08` | **Done** — closed |
| **B3** — refine `OD-S1-09` | **Done** — refined to one open infrastructure gate |
| **C** — correct the stage 08 reports | **Done** — five documents |
| **D** — one minimal correction migration, no rebaseline | **Done** |
| **E** — full test runs + historical-mobility SQL test | **Done** |
| **F** — status | **Done** |

## 2. Phase B is blocked, and nothing was invented in its place

The prompt requires seven documents under `docs/ORDERING-DESIGN-PACK-v3.8/REFERENCE/` created "**using the supplied
Master Reference as the source**", and says "Do not paraphrase away field-level detail."

**No Master Reference file was supplied**, and none exists in the repository — verified by `find . -iname "*MASTER*"`
(no hits outside `.git`) and by the absence of the `REFERENCE/` folder.

Writing those seven documents from the existing Pack prose would not be integration of the Master Reference. It would
be a document set invented by the agent and then labelled as the owner's canonical field-level index — which is
exactly what `00-AUTHORITY-AND-ADMISSION-RULES.md` is meant to prevent. So it was not done, and it is not reported as
done.

Recorded as `OD-S1-10`. **Please supply the Master Domain Reference documents.**

This also cost the prompt's second source for one verdict — see §5.

## 3. What Phase A corrected, and why it mattered

Stage 08 applied one rule to every Order-scoped reference: *if both sides carry an `OrderId`, make the foreign key
composite on it.* That is right for current containment and wrong for immutable history.

The Pack says so three times:

- `DOMAIN/13`, `ItemServiceLinks/ServiceLineage/ItemLineage` row — "**historical refs not constrained to current
  owner**".
- `DOMAIN/01` line 55 — "**Current service ownership can move through split without changing ServiceId.** History
  retains OrderIdAtOccurrence/Association."
- `DOMAIN/12` step 3 — "move current traveler/service ownership … **Preserve all historical OrderIdAtOccurrence**".

**The consequence was not theoretical.** Under the stage-08 schema, the very first action of `DOMAIN/12` step 3 —
moving a service to the child Order — is rejected by SQL Server with error 547 on
`FK_OrderItemServiceLinks_OrderServices_OrderIdAtAssociation_OrderServiceId`. Stage 08 had made the Pack's own split
undoable in the database. That is now proven, not argued: the correction was stashed and the new tests were run
against the old schema, producing 8 failures. `TEST-RESULTS.md` §5.

### The corrected rule

| Kind | Shape |
|---|---|
| **Current containment** — dependent and principal must be in the same Order now, and move together | composite foreign key on the current scoping column |
| **Immutable historical occurrence** — records what was true then; the dependent identity may later move | stable-identity foreign key **plus** an explicit occurrence `OrderId` column, which is a recorded fact and not an FK scope |

14 relations kept as composite; 7 corrected to stable identity. **No occurrence field was deleted or weakened** —
`OrderIdAtAssociation`, `PricingLine.OrderId` and `FundingObligation.OrderId` are all still persisted, non-null and
indexed. Full matrix, with the Pack line behind each verdict:
`HISTORICAL-IDENTITY-CORRECTION-MATRIX.md`.

A composite key across an **immutable occurrence pair** is still correct and was kept — `PriceChangeSet(OrderId,
ChangeId)`, `FundingObligation(OrderId, ChangeId)`, `OrderItemServiceLink(OrderIdAtAssociation, LinkedByChangeId)` —
because neither side of those can move.

## 4. `OD-S1-08` closed, `OD-S1-09` refined

**`ScopeAtAssociation` — CLOSED.** The owner's definition: the immutable snapshot of the Service's commercial
**beneficiary/coverage** scope at the moment Item-Service membership was established. Not sales scope, not funding
scope. For AirTransport, `{TravellerId, SegmentId}`. Persistence is
`DEFER_UNTIL_FIRST_REASSOCIATION_OR_SPLIT`, and explicitly **no generic JSON scope column in S1**. This is consistent
with `DOMAIN/13`, which lists the link row's fields as "occurrence OrderId, old/new refs/change, **scope snapshot**".

**`OD-S1-09` — refined, one part still open.** Separated into three: the canonical Domain requirement (stable
Traveller/Contact identity must not require perpetual PII retention) is **settled** and S1 already satisfies its
structural half; a **protected payload reference boundary** is an **approved** internal abstraction; only the
physical store, key ownership and retention schedule remain open. That is an architecture gate before the privacy
lifecycle slice, **not an S1 blocker**.

Both recorded in `reports/00-decisions/S1-FINAL-SHAPE-OPEN-DECISIONS.md` and reflected in ERRATA §1.3 and §6.

## 5. `OrderItem.CreatedByChangeId` — kept composite, with the proof

The prompt required this verdict from `DOMAIN/12` **and the Master Catalog**; the Catalog was not supplied. The
verdict rests on four Pack sources instead:

1. `DOMAIN/01` line 55 names only **service** ownership as moving through split.
2. `DOMAIN/02` line 7 — the item field list ends with "supersession/cancellation/**partition** references" and the
   paragraph closes with "**a successor item**".
3. `DOMAIN/02` line 15 — `ItemLineage` is a many-to-many predecessor/successor relation, the shape a successor model
   needs and a move does not.
4. `DOMAIN/13` — the `OrderItems` row's mandatory constraint is literally "**FK current Order**", in direct contrast
   to the link row's explicit historical exemption.

`DOMAIN/12`'s "partition current items" is ambiguous alone; with (2) and (3) it means successor child items.

**Kept, pending confirmation** (`OD-S1-11`). This is the reversible direction: relaxing later is one migration,
whereas wrongly relaxing now would silently drop a real guarantee with no test able to notice.

## 6. `OrderItemServiceLink` uniqueness — recorded as S1-only

`LinkId` is an **association occurrence identity**, not a pair identity. `DOMAIN/02` line 13 — "New membership
supersedes a current binding; old links remain history" — means the same (item, service) pair can legitimately recur.

`IX_OrderItemServiceLinks_OrderItemId_OrderServiceId` stays unique as an **S1-only** constraint that is true today
and catches a duplicate-write bug. **This stage does not state it as a permanent domain invariant.** The slice
introducing re-association or split must drop or replace it; that gate belongs in the Master Pack materialization
matrix, which is blocked on `OD-S1-10`. Until those documents exist,
`HISTORICAL-IDENTITY-CORRECTION-MATRIX.md` §6 is the record.

## 7. Migration

One minimal correction migration, `20260921194857_S1HistoricalIdentityCorrection`: **26 operations**, all foreign-key
and index swaps. Zero `DropColumn`, `RenameColumn`, `AlterColumn`, check-constraint change, `Sql(…)` or data
operation, in `Up()` and `Down()`. No rebaseline; the chain is now eleven migrations.

One near-miss caught by manual inspection: the first draft contained
`DropUniqueConstraint("AK_OrderServices_OrderId_Id")`, because relaxing its two consumers left the implicitly created
alternate key unreferenced. Phase D requires preserving current-containment alternate keys, so it was declared
explicitly and the migration regenerated. `MIGRATION-IMPACT.md` §3.

## 8. Evidence

| Run | Result |
|---|---|
| `dotnet build AeroTech.Ordering.sln` | 0 errors |
| Domain tests | **135 / 135** |
| Persistence tests (`Category!=Live`) | **241 / 241**, 2 m 28 s |
| API + OpenAPI + architecture + composition + host | 48 / 48 |
| Fresh DB + upgrade + atomicity + historical identity | 26 / 26 |
| `has-pending-model-changes` × 3 contexts | No changes |
| Red-first against the stage-08 schema | **8 failed / 22**, naming the exact wrongly-added constraints |

Tests: 371 → **376**. 8 added, 4 wrong theory cases removed, 1 new matrix test. No test weakened to go green.

## 9. Documents

In this folder:

| File | Contents |
|---|---|
| `HISTORICAL-IDENTITY-CORRECTION-MATRIX.md` | The 14 kept + 7 corrected relations, each with its Pack line; the `OrderItem` verdict; link uniqueness; red-first evidence |
| `MIGRATION-IMPACT.md` | Every one of the 26 operations; alternate keys preserved; the near-miss |
| `TEST-RESULTS.md` | All runs, tests added and corrected, red-first detail, failures fixed, what is not proven |
| `REPORT.md` | This file |
| `01-runs/` | Raw output, including `07-RED-FIRST-AGAINST-STAGE08-SCHEMA.txt` |

Changed outside this folder:

- `reports/08-S1-authoritative-final-closure/` — five documents carry a correction note (Phase C); none was deleted
- `reports/00-decisions/S1-FINAL-SHAPE-OPEN-DECISIONS.md` — `OD-S1-08` closed, `OD-S1-09` refined, `OD-S1-10` and
  `OD-S1-11` opened
- `docs/ORDERING-DESIGN-PACK-v3.8/ERRATA/S1-CLOSURE-CLARIFICATIONS.md` — §1.3 rewritten, §6 refined, new §8 on
  current containment versus historical reference

No Pack file outside the authorised `ERRATA/` folder was edited.

## 10. Open for the owner

1. **`OD-S1-10` — supply the Master Domain Reference.** Blocks Phase B (seven `REFERENCE/` documents) and B1
   (thirteen Pack cross-references). Nothing else.
2. **`OD-S1-11` — confirm `OrderItemId` does not move current Order during split.** If the Master Catalog says it is
   preserved the way `TravelerId`/`ServiceId` are, `OrderItems(OrderId, CreatedByChangeId)` must be relaxed the same
   way `OrderService.CreatedByChangeId` was.
3. **`OD-S1-09` item 3** — physical store, key ownership and retention schedule for protected personal data.
4. Carried from stage 08, unchanged: `OrderingCommandKind` placement, schema name `Order` vs `Commercial`,
   `FareConstructionItems` scoping.
5. Carried from the previous turn and still unanswered: `OrderServices` has no CHECK enforcing that an
   `AirTransportation` row has a non-null `TravellerId`/`SegmentId`. TPH forces those columns nullable, so the
   invariant lives only in C#. Adding `CK_OrderServices_AirTransportBinding` is a one-line schema change awaiting
   your approval.

Nothing was committed or pushed.
