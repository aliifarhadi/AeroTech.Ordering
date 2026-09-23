# S1 authoritative final shape closure — report

Stage: 08-S1-authoritative-final-closure · 2026-09-21 · branch `k8s-stg` · base HEAD `e9fe97a`

> **CORRECTED BY STAGE 09 — 2026-09-21.** This stage over-applied current-owner composite foreign keys to seven
> immutable historical references, which would have made the Pack-mandated split (`DOMAIN/12`) impossible in SQL.
> Corrected in `reports/09-S1-historical-identity-and-pack-reference/`. The status line below is superseded by
> `S1_HISTORICAL_IDENTITY_CORRECTED_PACK_REFERENCE_READY_FOR_ARCHITECT_REVIEW`.

**Status: `S1_FINAL_SHAPE_READY_FOR_ARCHITECT_REVIEW`**
**`S2_NOT_STARTED`**

Two owner decisions are open (`OD-S1-08`, `OD-S1-09`). Neither blocks the review: neither can be produced or consumed
by any S1 command, and both are recorded with the question, the evidence and the reason they were not decided here.

---

## 1. What this stage did

The S1 model was semantically correct at `e9fe97a` but its correctness lived almost entirely in C#. This stage moved
the invariants that the Pack states as structural facts into the database, tightened three domain guards that were
accepting facts the owner contract calls required, and closed the scenario matrix.

| Area | Before | After |
|---|---|---|
| Order-scope enforcement | 18 single-column foreign keys — a child could reference a row of another Order | 21 composite foreign keys — **corrected by stage 09 to 14 current-containment composites + 7 stable-identity historical references** |
| Closed-vocabulary enforcement | an enum column accepted any `int` | **39 `CHECK` constraints**, generated from `Enum.GetValues`, one per persisted enum column |
| `CommercialVersion` uniqueness | non-unique index | **unique** per Order |
| Leg required facts | nullable columns the validator already refused to leave null | **non-null** |
| Leg / scheduled-segment invariants | partial | `EnsureLegs` + `EnsureScheduledAir`: identity, sequence, airports, time ordering, 7 owner operational facts |
| AirOffer `stop` | silently dropped | **fails closed** (`UnsupportedCapability`, `BD-006`), payload still retained in evidence |
| Value-object vocabularies | free enums | closed-enum guards on `SalesContextSnapshot`, `BuyerSnapshot`, `InitiatingActorSnapshot`, `BaggageAllowance` |
| Scenario matrix | 24 `TESTED`, 22 `UNTESTED`, 12 `UNSUPPORTED` | **56 `TESTED`, 0 `UNTESTED`, 0 `UNSUPPORTED`**, 3 `DEFERRED`, 2 `NOT_APPLICABLE` |
| Tests | 250 | **371** (+121) |

One additive migration, `20260921000413_S1AuthoritativeFinalShape`: 119 schema operations, **zero** data operations,
no `DropColumn`, no `RenameColumn`, no rebaseline. The chain of ten migrations is intact.

## 2. The point of the change

A Pack invariant that only C# enforces is a claim about the current code, not a property of the data. Three concrete
consequences of the "before" column:

- `OrderItems.CreatedByChangeId` pointed at *some* `OrderChange`. A direct SQL write, a future repricing slice, or a
  replication path could attribute an item to a change of a different Order, and nothing would notice.
- `PricingLines.Component` was an `int`. A row with `Component = 9999` would round-trip through the database, fail on
  read, and the failure would surface far from the write.
- A leg could be stored with a null origin airport, even though `CandidateValidator` refused to produce one — the
  nullable column was a promise the model was not making.

After this stage each of those is refused by SQL Server, with the constraint named in the error, and each refusal has
a test that issues raw SQL and asserts the specific error number and constraint name. Domain correctness did not
change; the storage stopped depending on it.

## 3. Findings this stage produced

Things that were wrong and are now right, or wrong and now recorded.

1. **Two enum columns had no constraint and no one knew.** The hand-written matrix missed
   `CommandReceipts.CommandKind` and `OrderPreparations.AcceptanceAssurance`. A model-walking test found them.
   (`TEST-RESULTS.md` §4.2)
2. **Four reliability scenarios named tests that no longer exist.** R3, R6, R7 and R12 in the stage 06 matrix pointed
   at tests renamed or removed during stage 07. R7 has a live test under a new name; R3, R6 and R12 now have new tests
   written in this stage. (`SCENARIO-CLOSURE-MATRIX.md` §3)
3. **Three stage 07 deletions had prose justification but no test.** Deadline independence, unique preparation
   consumption, and refusal of an unknown projection schema version. All three are now asserted.
   (`NEGATIVE-DELETION-AUDIT.md` §10)
4. **`FareConstruction.Assurance` is listed as deleted in stage 07 but exists today.** Restored in R2, never
   corrected. (`NEGATIVE-DELETION-AUDIT.md` §6)
5. **Preparation consumption is a direction divergence, not an equivalence.** Stage 07 justified removing the
   consumption columns with a statement about the application flow. The database guarantee is the unique index on
   `Orders`, which is a different claim. (ERRATA §4, `NEGATIVE-DELETION-AUDIT.md` §7)
6. **Three ownership tests were initially proving the wrong constraint** — they caught a unique index (2601) rather
   than a foreign key (547). A weaker assertion would have passed while proving nothing. (`TEST-RESULTS.md` §4.4)
7. **`OrderingCommandKind` violates the repository enum convention** — wrong folder, no `[Display]`. Reported, not
   moved, because publishing an internal idempotency discriminator as a shared wire contract is a public-contract
   decision. (`AUTHORITY-MATRIX.md` §4)

## 4. What was deliberately not done

| Item | Reason |
|---|---|
| `ScopeAtAssociation` | Named once in the Pack with no type, no value set and no rule. Three readings of "scope" exist in the Pack. `OD-S1-08`. |
| Protected personal-data payload store | A service-level capability with cross-service key ownership and retention decisions. `OD-S1-09`. **S1 is not privacy-complete and the reports do not claim it is.** |
| Renaming schema `Order` to `Commercial` | `CLAUDE.md` forbids renaming a schema without approval; it would rewrite ten migrations for a cosmetic change. ERRATA §5. |
| `FareConstructionItems` cross-order constraint | The join table has no `OrderId`; adding one is a persistence-identity decision. `CURRENT-OWNERSHIP-FK-MATRIX.md` §3. |
| Migration rebaseline | Would delete the upgrade path `MigrationUpgradeTests` exercises and make earlier stage evidence unreproducible. `MIGRATION-IMPACT.md` §1. |
| Editing any Pack file | Only the authorised `ERRATA/` subfolder was added. |

## 5. The one behavioural restriction

An AirOffer response that supplies a `stop` on a flight or a leg can no longer be sold. Before this stage it was sold
with the stop dropped.

The Pack says: "Preserve source stop metadata; it never determines commercial segment or coupon count. Exact optional
stop shape requires a captured wire fixture before it is consumed." Dropping the field silently satisfied the first
half and violated the second. Failing closed satisfies both — the payload is preserved in evidence, and nothing is
consumed without an approved shape.

This is a real restriction, not a neutral hardening, and it is reversible in one method the moment the owner supplies
a captured fixture. `SOURCE-CONTRACT-MATRIX.md` §5.

## 6. Evidence

| Run | Result |
|---|---|
| `dotnet build AeroTech.Ordering.sln` | 0 errors |
| Domain tests | **135 / 135** |
| Persistence tests (`Category!=Live`) | **236 / 236**, 1 m 46 s |
| API + OpenAPI + architecture + composition + host | 48 / 48 |
| Fresh DB + previous-stage upgrade + atomicity | 4 / 4 |
| `has-pending-model-changes` × 3 contexts | No changes |
| Dev database | migration applied |

Red-first proof for every new guard: with `EnsureLegs`, `EnsureScheduledAir` and `EnsureNoUnsupportedStop` disabled,
17 of 22 `SegmentStructureTests` and exactly the 2 stop tests fail; restored, all pass. `TEST-RESULTS.md` §4.

Two matrix-driven tests read the live EF model and the live `sys.foreign_keys` / `sys.check_constraints`, so the
matrices in this folder cannot silently drift from the database.

## 7. Documents in this folder

| File | Contents |
|---|---|
| `AUTHORITY-MATRIX.md` | What decided each change; what was deliberately not decided |
| `PACK-S1-MATERIALIZATION-MATRIX.md` | Bidirectional Pack↔code audit: 46 materialized, 10 deferred, 3 divergences; every table traced to a Pack line |
| `SOURCE-CONTRACT-MATRIX.md` | Every AirOffer wire field: modelled, evidence, reconciled or fail-closed |
| `NEGATIVE-DELETION-AUDIT.md` | Every stage 07 deletion re-audited against the Pack at the final shape |
| `CURRENT-OWNERSHIP-FK-MATRIX.md` | The 21 scoped relations, their proof, and what is not enforced |
| `ENUM-CONSTRAINT-MATRIX.md` | The 39 guarded columns, their proof, and what is not guarded |
| `SCENARIO-CLOSURE-MATRIX.md` | All 61 + 12 scenarios reconciled; supersedes the stage 06 dispositions |
| `MIGRATION-IMPACT.md` | Every operation in the migration, including the one risky default |
| `TEST-RESULTS.md` | All runs, the 121 new tests, red-first evidence, failures fixed, what is not proven |
| `01-runs/` | Raw output |

Also written outside this folder:

- `docs/ORDERING-DESIGN-PACK-v3.8/ERRATA/S1-CLOSURE-CLARIFICATIONS.md` — 10 deferrals, 1 clarification, 3 divergences
- `reports/00-decisions/S1-FINAL-SHAPE-OPEN-DECISIONS.md` — `OD-S1-08`, `OD-S1-09`
- A supersession note at the top of `reports/06-S1-create-order-conformance/CREATE-ORDER-SCENARIO-MATRIX.md`

## 8. Open for the owner

1. ~~**`OD-S1-08` — `ScopeAtAssociation`.**~~ **CLOSED by the owner in stage 09:** immutable beneficiary/coverage
   snapshot at association; `{TravellerId, SegmentId}` for AirTransport; persistence deferred to the first
   re-association or split.
2. **`OD-S1-09` — protected personal-data payload store.** **Refined in stage 09:** the Domain requirement and the
   payload-reference abstraction are settled; only the physical store, key ownership and retention schedule remain
   open, as an architecture gate before the privacy lifecycle slice.
3. **`OrderingCommandKind` placement.** Move it to `Contracts/AeroTech.Messages/Ordering/Enums/` with `[Display]`
   per the repository convention, or keep it internal to Domain and record the exception?
4. **Schema name.** Keep `Order`, or plan a rename to `Commercial` as a deliberate migration?
5. **`FareConstructionItems` scoping.** Add an `OrderId` column so its item reference can be owner-matched, or accept
   the transitive scope through `FareConstructions`?

Nothing was committed or pushed. The working tree holds 29 modified and 13 new files.
