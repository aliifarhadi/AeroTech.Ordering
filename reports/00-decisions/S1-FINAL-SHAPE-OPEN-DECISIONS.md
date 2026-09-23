# S1 final shape — decisions

Raised during stage 08 (`reports/08-S1-authoritative-final-closure/`).
`OD-S1-08` closed and `OD-S1-09` refined by the owner in stage 09
(`reports/09-S1-historical-identity-and-pack-reference/`).

---

## OD-S1-08 — `ScopeAtAssociation` semantics — **CLOSED** (owner, stage 09)

### The question that was open

`docs/ORDERING-DESIGN-PACK-v3.8/DOMAIN/02-COMMERCIAL-COMPOSITION.md` line 13 names `ScopeAtAssociation` in the
`OrderItemServiceLink` tuple and nowhere else in the Pack. It had no type, no value set and no rule, and "scope"
carries at least three distinct meanings in the Pack (funding scope, sales scope, coverage scope). Stage 08 refused to
choose and recorded the gap.

### Owner answer

> `ScopeAtAssociation` is the immutable snapshot of the Service's commercial **beneficiary/coverage** scope at the
> time Item-Service membership was established.

It is:

- **NOT** sales scope (owner airline, financial customer, seller, buyer, actor);
- **NOT** funding scope (item / service / pricing line liability attribution).

Per service type:

| Service type | `ScopeAtAssociation` |
|---|---|
| `AirTransportation` | `{ TravellerId, SegmentId }` |
| future typed shared services | the complete typed beneficiary/coverage set for that service type |

This reading is consistent with `DOMAIN/13`, whose `ItemServiceLinks/ServiceLineage/ItemLineage` row lists the
persisted fields as "occurrence OrderId, old/new refs/change, **scope snapshot**" — a snapshot of what the service
covered, captured at the association occurrence.

### Physical persistence

`DEFER_UNTIL_FIRST_REASSOCIATION_OR_SPLIT`.

S1 does **not** persist a column. Reasons, stated so the deferral is auditable rather than convenient:

1. In S1 a service is created once, associated once, and never re-associated. The snapshot would equal the service's
   current `{TravellerId, SegmentId}` for the whole S1 lifetime of every row, and those two columns are already
   persisted on `OrderAirTransportService` under composite foreign keys to the current traveller and segment.
2. The snapshot only starts to differ from the current values at the first re-association or split — which is exactly
   the slice named in the deferral.
3. **No generic JSON scope in S1.** An untyped payload column would make the eventual typed shape harder, not easier,
   and would be a persistence-identity decision taken before the owner needs it.

When the first re-association or split slice lands, the field materialises as the typed beneficiary/coverage set for
the service type — not as a generic document.

### Consequence for the S1 schema

None. `OrderItemServiceLinks` keeps `Id`, `OrderIdAtAssociation`, `OrderItemId`, `OrderServiceId`,
`LinkedByChangeId`. Stage 09 corrected the foreign keys on `OrderItemId` and `OrderServiceId` to stable-identity
references so that a future service move does not invalidate the historical link
(`reports/09-S1-historical-identity-and-pack-reference/HISTORICAL-IDENTITY-CORRECTION-MATRIX.md`).

**Status: CLOSED.** ERRATA §1.3 updated to record the definition and the deferral point.

---

## OD-S1-09 — Protected personal-data payload boundary — **REFINED, still open** (owner, stage 09)

Stage 08 recorded this as one undifferentiated gap. The owner has separated it into three parts, of which only the
third is still open.

### 1. Canonical Domain requirement — settled

**Stable Traveller and Contact identity must not require perpetual PII retention.**

`DOMAIN/04-TRAVELERS-JOURNEYS-AND-PRIVACY.md` line 25: monetary immutability is not permission to retain unlimited
PII; history stores non-PII correlation ids and redacted metadata; erasing a payload must not delete original money,
operation identity or document-number uniqueness records.

S1 already satisfies the structural half of this. No Ordering history table holds personal data: `OrderChange`,
`PriceChangeSet`, `PricingLine`, `FundingObligation`, `OrderComponentTotal`, `OrderItemServiceLink` and the outbox
carry identifiers and money only. `OrderTraveller.Id` is a stable identity that the monetary record references;
personal data hangs off it and is reachable only through the current `OrderTravellers` / `OrderContacts` rows. The
money therefore survives erasure of the personal payload by construction.

Proven by `R11` (`Service_surface_needs_no_token_and_never_returns_traveller_names`) and by the absence of any
personal column on the historical tables.

### 2. Allowed internal abstraction — approved, not yet built

A **protected payload reference boundary** is an approved internal abstraction: traveller identity attributes
(`GivenName`, `Surname`, `DateOfBirth`) and contact attributes (`Email`, `Phone`) may be addressed through a
reference rather than stored as inline columns, without that being a new architectural decision.

What this permits, when the privacy lifecycle slice lands: replacing the inline columns on
`OrderTravellerIdentities` and `OrderContacts` with a reference, leaving `OrderTravellers.Id`, all foreign keys and
every monetary row untouched. The change is local to two tables.

This is **not** authorisation to build the store now.

### 3. Still open — infrastructure choices

| Open item | Why Ordering cannot decide it alone |
|---|---|
| Physical store | Inside Ordering, in IdentityServer, or a dedicated service — the choice binds other AeroTech services that hold the same traveller data. |
| Key ownership | Who holds and rotates the encryption keys, and who can authorise a read. |
| Retention schedule | Per-jurisdiction retention windows and the erasure trigger, which is a commercial and legal decision, not a schema one. |

### Gate

This is an **architecture gate before the privacy lifecycle slice**, not an S1 blocker. S1 ships with inline personal
columns and the honest statement that it is **not privacy-complete** (ERRATA §6). No S1 command accepts a passport,
guardian-assistance or special-needs payload, so the exposure is bounded to name, date of birth and contact details.

**Status: OPEN on item 3 only.** Items 1 and 2 are settled and are no longer counted as gaps.

---

## Open decisions raised in stage 09

## OD-S1-10 — Master Domain Reference not supplied — **BLOCKED_MISSING_SOURCE**

The stage 09 prompt, Phase B, requires creating seven documents under
`docs/ORDERING-DESIGN-PACK-v3.8/REFERENCE/` "using the supplied Master Reference as the source", and Phase B1 requires
adding cross-references from thirteen `DOMAIN/*` files to that catalog.

**No Master Reference file was supplied with the prompt**, and none exists in the repository — verified by
`find . -iname "*MASTER*"` (no hits outside `.git`) and by the absence of
`docs/ORDERING-DESIGN-PACK-v3.8/REFERENCE/`.

The prompt is explicit that the field-level detail must not be paraphrased away. Writing the seven documents from the
existing Pack prose would not be integration of the Master Reference; it would be a new document set invented by the
agent and then labelled as the owner's canonical field index — precisely the failure mode
`00-AUTHORITY-AND-ADMISSION-RULES.md` is supposed to prevent.

**Not done, not faked.** Phase B (all seven documents) and Phase B1 (thirteen cross-references) wait for the files.
Phase B2 and B3 were fully specified in the prompt text itself and are completed above.

**Question for the owner:** please supply the Master Domain Reference documents.

**Status: OPEN.** Blocks Phase B and B1 only. Phases A, C, D, E completed.

## OD-S1-11 — `OrderItem` identity across split — verdict recorded, second source missing

The stage 09 prompt asked for a verdict on `OrderItem.CreatedByChangeId` "based on `DOMAIN/12` wording **and the
Master Catalog**". The Master Catalog was not supplied (`OD-S1-10`), so the verdict below rests on four Pack sources
instead of two.

**Verdict: keep the composite** `OrderItems(OrderId, CreatedByChangeId) -> OrderChanges(OrderId, Id)`.

Proof that an `OrderItemId` does not move current Order:

1. `DOMAIN/01` line 55 — "Current **service** ownership can move through split without changing ServiceId." Service
   is named; item is not. The same sentence lists what history retains, and stops at services.
2. `DOMAIN/02` line 7 — the `OrderItem` field list ends with "supersession/cancellation/**partition** references", and
   the paragraph closes with "where terms/product boundary changes, **a successor item**". Partition of an item is
   expressed as successor identities carrying partition references, not as a move.
3. `DOMAIN/02` line 15 — `ItemLineage` exists as a many-to-many predecessor/successor relation, which is the shape
   required for successor items and unnecessary for a move.
4. `DOMAIN/13` — the `OrderItems` row's mandatory constraint is literally "**FK current Order**", with no
   historical-reference exemption. Contrast the `ItemServiceLinks/ServiceLineage/ItemLineage` row, which states
   "historical refs **not** constrained to current owner".

`DOMAIN/12` step 3 says "partition current items", which on its own is ambiguous; read with (2) and (3) it means
creating successor child items, not relocating an existing `OrderItemId`.

**What would change the verdict:** a Master Catalog entry stating that `OrderItemId` is preserved across a split the
way `TravelerId`/`ServiceId` are. If that entry exists, this composite must be relaxed to a stable-identity foreign
key the same way `OrderService.CreatedByChangeId` was.

**Status: OPEN for confirmation.** The composite is kept meanwhile, which is the reversible direction: relaxing it
later is one migration, while having wrongly relaxed it would have silently dropped a real guarantee.
