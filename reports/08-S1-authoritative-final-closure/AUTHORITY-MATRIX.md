# Authority matrix

Stage: 08-S1-authoritative-final-closure · 2026-09-21 · branch `k8s-stg` · base HEAD `e9fe97a`

What decided each thing this stage changed, so that a reviewer can check the decision rather than the outcome.

## 1. The authority order applied

Per `CLAUDE.md`:

1. **Design Pack 3.8** — Ordering domain semantics, service ownership, invariants, layer responsibilities,
   transaction/recovery semantics. Conflicts resolved by `GOVERNANCE/05-STANDING-RULE-CONFLICT-RESOLUTION.md`.
2. **`CLAUDE.md`** — repository mechanics: conventions, naming, environment, tests, reporting.
3. **Owner contracts of sibling services** as mapped in the Pack's `CONTRACTS/`.

The stage prompt is the owner's instruction for *what work to do*; it is not a fourth authority over semantics. Where
the prompt asked for something the Pack does not define — `ScopeAtAssociation` — the Pack's silence won and the item
became an open decision rather than an invention.

## 2. What changed, and what decided it

| Change | Authority | Citation |
|---|---|---|
| 21 relations made composite, owner/order-scoped | Pack | `DOMAIN/01-AGGREGATES.md` (the Order is the consistency boundary), `DOMAIN/02` line 13 ("Current service owns exactly one OrderId/OrderItemId") |
| 39 enum `CHECK` constraints | Pack | `DOMAIN/13-PERSISTENCE-DATA-DICTIONARY.md` (constraint inspection is stage evidence); a closed vocabulary the database does not enforce is a claim, not a guarantee |
| `IX_OrderChanges_OrderId_CommercialVersion` made unique | Pack | `DOMAIN/10-ELIGIBILITY-VERSIONS-AND-TIME.md` — `CommercialVersion` is the per-Order counter |
| `OrderSegmentLeg` required facts made non-null | Pack + code evidence | `DOMAIN/04-TRAVELERS-JOURNEYS-AND-PRIVACY.md` (legs are itinerary detail of a sold segment); `CandidateValidator.EnsureLegs` already refused null, so the column was nullable without a writer |
| `EnsureLegs` / `EnsureScheduledAir` invariants | Pack | `CONTRACTS/02-AIROFFER.md` Flight/Leg rows — the owner supplies these as required facts |
| `Stop` fail-closed | Pack | `CONTRACTS/02-AIROFFER.md` Stop row: "Exact optional stop shape requires a captured wire fixture before it is consumed" |
| Closed-enum guards in 4 value objects | Pack | `DOMAIN/05` / `DOMAIN/04` — a snapshot records a vocabulary member, not an arbitrary integer |
| `ERRATA/S1-CLOSURE-CLARIFICATIONS.md` | Stage prompt, explicitly authorised | The prompt authorised an `ERRATA` subfolder. No Pack file was edited. |
| Scenario matrix reconciliation | `CLAUDE.md` reporting rules | Stage 06 keeps its text; the superseding dispositions live in stage 08 |
| One additive migration, no rebaseline | Owner decision + evidence | `reports/00-decisions/S1-MIGRATION-REBASELINE-OPEN-DECISION.md`; keeping the chain preserves the upgrade test |

## 3. What was deliberately not decided

| Item | Why it was not decided here | Recorded as |
|---|---|---|
| `ScopeAtAssociation` type and semantics | Named once in the Pack with no type, no value set and no rule. Three readings of "scope" exist in the Pack and each produces a different column. Choosing one would be a persistence-identity decision the owner has not made. | `OD-S1-08`, ERRATA §1.3 |
| Where the protected personal-data payload store lives | A service-level capability with cross-service consequences (key ownership, retention policy, access surface). Ordering cannot choose it for other services. | `OD-S1-09`, ERRATA §6 |
| Renaming schema `Order` to `Commercial` | `CLAUDE.md`: "Never … rename schemas … without approval". It would also rewrite every applied migration for a cosmetic change. | ERRATA §5 |
| Moving `OrderingCommandKind` into `Contracts/AeroTech.Messages/Ordering/Enums/` | `CLAUDE.md` says all Ordering-owned enums live there with `[Display]` per member; this one lives in `Domain/CommandReceiptAggregate/` with no `[Display]`. Moving it would publish an internal idempotency discriminator as a shared wire contract, which is a public-contract decision. | §4 below |
| A composite key for `FareConstructionItems` | The join table has no `OrderId`; adding one is a persistence-identity change. | `CURRENT-OWNERSHIP-FK-MATRIX.md` §3 |

## 4. Convention divergence found and not fixed

`src/AeroTech.Ordering.Domain/CommandReceiptAggregate/OrderingCommandKind.cs` is an Ordering-owned enum that

- does not live in `Contracts/AeroTech.Messages/Ordering/Enums/`, and
- has no `[Display(Name = "…")]` on its members,

both of which `CLAUDE.md` requires of Ordering-owned enums. It is now guarded by
`CK_CommandReceipts_CommandKind_Enum`, so its storage is correct; only its placement is not. Moving it is a
public-contract decision (it would become visible to every AeroTech service), so it is reported rather than done.

## 5. Evidence discipline

- Every claim in this stage's reports names a test that exists. Four stale test names inherited from stage 06 were
  found and corrected (`SCENARIO-CLOSURE-MATRIX.md` §3).
- Every constraint claim is proven by raw SQL against SQL Server, not by an EF assertion.
- Every new production guard was proven red-first by disabling it and observing the failures
  (`TEST-RESULTS.md` §4).
- Two matrix-driven tests (`Every_scoped_relation_…`, `Every_persisted_enum_column_…`) read the live model and the
  live database, so the matrices in this folder cannot silently go stale.
