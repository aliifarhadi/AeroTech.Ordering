# ERRATA — S1 Closure Clarifications

Status: clarification record, not a Pack amendment.
Scope: S1 (Prepare / Create / Get original sale) only.
Authority: this file records how the S1 implementation reads the Pack where the Pack is silent, ambiguous, or wider
than S1. It never replaces a Pack statement. Where the Pack and this file disagree, the Pack wins and the disagreement
is a defect in this file.

Every entry below is one of:

- **DEFERRED** — the Pack requires it for the Ordering domain, but no S1 command or query can produce or consume it.
  S1 does not materialise it. The stage that first needs it materialises it.
- **CLARIFIED** — the Pack states the requirement; this entry fixes the reading S1 implemented.
- **DIVERGENCE** — S1 does not match the Pack statement. Each one names the Pack line and the reason.

---

## 1. Commercial composition and service identity

### 1.1 `ServiceVersion` — DEFERRED

Pack: `DOMAIN/02-COMMERCIAL-COMPOSITION.md` line 19 lists `ServiceVersion` as a field of the service.
Pack: `DOMAIN/10-ELIGIBILITY-VERSIONS-AND-TIME.md` line 28 — "ServiceVersion starts at 1 and changes for current
commercial definition/owner/item/coverage binding changes."

S1 accepts an original sale and never mutates a service afterwards. Every service created by S1 would carry the
constant value 1 for its whole S1 lifetime, and no S1 code path can increment it. Persisting a structurally constant
column would be evidence of a versioning mechanism that does not exist.

S1 therefore does not persist `ServiceVersion`. The first stage that performs a commercial-definition, owner, item or
coverage-binding change on an existing service introduces the column together with the increment rule and the
`OrderChange` that carries it.

### 1.2 `PriceTreatment` — DEFERRED

Pack: `DOMAIN/02-COMMERCIAL-COMPOSITION.md` lines 19 and 21 — `PriceTreatment: SeparatelyPriced, Included,
Complimentary, SupplierOpaque`.

Every service S1 creates comes from an accepted AirOffer air-transport candidate and is separately priced: the
candidate pricing lines reconcile to the item total, and `CandidateValidator` rejects a candidate whose priced content
does not reconcile. `Included`, `Complimentary` and `SupplierOpaque` require ancillary or bundled content that no S1
command can express.

S1 does not persist `PriceTreatment`. The stage that introduces ancillaries or bundles introduces the enum in
`Contracts/AeroTech.Messages/Ordering/Enums/` and the column, with the rule from line 21 that included/complimentary
must not fabricate a synthetic zero fare line.

### 1.3 `ScopeAtAssociation` — DEFERRED (semantics CLOSED, persistence deferred)

Pack: `DOMAIN/02-COMMERCIAL-COMPOSITION.md` line 13 — "Store immutable `OrderItemServiceLink(LinkId,
OrderIdAtAssociation, OrderItemId, OrderServiceId, ScopeAtAssociation, LinkedByChangeId)`."
Pack: `DOMAIN/13-PERSISTENCE-DATA-DICTIONARY.md`, `ItemServiceLinks/ServiceLineage/ItemLineage` row — persisted
fields are "occurrence OrderId, old/new refs/change, **scope snapshot**".

**Semantics — settled by the owner (stage 09, closes `OD-S1-08`).**

`ScopeAtAssociation` is the immutable snapshot of the Service's commercial **beneficiary/coverage** scope at the time
Item-Service membership was established. It is **not** sales scope and **not** funding scope.

| Service type | Scope |
|---|---|
| `AirTransportation` | `{ TravellerId, SegmentId }` |
| future typed shared services | the complete typed beneficiary/coverage set for that service type |

Stage 08 recorded this field as ambiguous because the Pack names it once with no type; the owner answer above removes
the ambiguity and is recorded in `reports/00-decisions/S1-FINAL-SHAPE-OPEN-DECISIONS.md`.

**Persistence — `DEFER_UNTIL_FIRST_REASSOCIATION_OR_SPLIT`.**

S1 does not persist a column. In S1 a service is created once, associated once and never re-associated, so the
snapshot would equal the service's current `{TravellerId, SegmentId}` for every row's whole S1 lifetime — and those
two columns are already persisted on `OrderAirTransportService` under composite foreign keys to the current traveller
and the current segment. The snapshot first differs from the current values at the first re-association or split,
which is the slice that materialises it, as the typed beneficiary/coverage set for the service type.

**No generic JSON scope column in S1.** An untyped payload would make the eventual typed shape harder rather than
easier and would settle a persistence-identity question before the owner needs it.

What S1 does persist and enforce: `Id`, `OrderIdAtAssociation`, `OrderItemId`, `OrderServiceId`, `LinkedByChangeId`.
Stage 09 corrected `OrderItemId` and `OrderServiceId` to stable-identity foreign keys so that a future service move
cannot invalidate the historical link, while `LinkedByChangeId` stays scoped by `OrderIdAtAssociation` because both
are immutable occurrence facts. See §8.

### 1.4 Service transition history — DEFERRED

Pack: `DOMAIN/02-COMMERCIAL-COMPOSITION.md` line 19 — "... and immutable transition history."

An original sale produces exactly one commercial transition per service: its creation, which is already fully recorded
by `CreatedByChangeId` pointing at the single `OrderChange` of the sale. A separate history table in S1 would contain
exactly one derivable row per service.

S1 does not add a service transition table. The stage that introduces a second commercial transition introduces it.

### 1.5 `SupplierPartyRef`, `DeliveryProviderRef`, `ServiceCode`/`ServiceName` — DEFERRED

Pack: `DOMAIN/02-COMMERCIAL-COMPOSITION.md` line 19.

The AirOffer candidate contract (`CONTRACTS/02-AIROFFER.md`) carries no supplier party, no delivery provider and no
service code/name for air transport; the marketing/operating airline identities and the segment binding are the
complete supplier facts S1 receives. Persisting empty columns would misrepresent the source.

S1 persists the air-transport facts it actually receives, on `OrderAirTransportService`. The stage that sells
non-air-transport or third-party-supplied content introduces the supplier and delivery references.

---

## 2. Pricing and fare construction

### 2.1 `SourceDecisionRef` — DEFERRED

Pack: `DOMAIN/03-PRICING-AND-FARE-CONSTRUCTION.md` line 9 — `PriceChangeSet(SetId, OrderId, ChangeId,
FinancialSequence, Reason, SourceDecisionRef, BaseCommercialVersion, CommittedAt)`.

`SourceDecisionRef` identifies the pricing decision that produced the set. In S1 the only pricing decision is the
accepted offer itself, and that reference is already persisted at the Order level, immutably and with a stronger
guarantee: `Order.SourceOfferId`, `Order.SourcePreparationId` and `Order.AcceptedSnapshotDigest` (SHA-256 of the
canonical normalised candidate). S1 has exactly one `PriceChangeSet` per Order, so a per-set reference would duplicate
the Order-level reference with no ability to differ from it.

S1 does not persist `SourceDecisionRef` on `PriceChangeSets`. The stage that commits a second monetary mutation — a
repricing, a revalidation or an exchange — introduces it, because from that stage on different sets have different
sources.

### 2.2 `BaseCommercialVersion` — DEFERRED

Pack: `DOMAIN/03-PRICING-AND-FARE-CONSTRUCTION.md` line 9.

`BaseCommercialVersion` records the commercial version the set was computed against. In S1 the only set is committed
inside the creating change, whose `CommercialVersion` is 1 by construction and is enforced unique per Order by
`IX_OrderChanges_OrderId_CommercialVersion`. The base version is therefore structurally constant and derivable.

S1 does not persist `BaseCommercialVersion`. The stage that prices against an existing Order introduces it, with the
staleness check that makes it meaningful.

---

## 3. Eligibility, versions and disposition

### 3.1 `CurrentDisposition` — DEFERRED

Pack: `DOMAIN/10-ELIGIBILITY-VERSIONS-AND-TIME.md`, and the eligibility vector it describes.

Disposition is a fulfilment/delivery-side fact — document state, control state, delivery state. S1 creates no document,
no reservation control and no delivery effect; `AcceptedScopeAndLiabilityTests` and `AcceptedShapePersistenceTests`
assert that an accepted S1 sale causes no reservation, funding-document or delivery effect at all. A disposition column
in S1 could only ever hold its initial value.

S1 does not persist a disposition. The fulfilment stage introduces it together with the effects it describes.

### 3.2 Eligibility vector — DEFERRED

Pack: `DOMAIN/10-ELIGIBILITY-VERSIONS-AND-TIME.md` line 50 — the quote/eligibility vector freezing `CommercialVersion`,
relevant `ServiceVersion`s, document/control versions, obligation versions, source pricing context and dependency scope
hash.

S1 has no quote-then-dispatch step: `Create` accepts a prepared candidate and commits in one transaction, and the
staleness protection S1 needs is already carried by `OrderPreparation` validity plus `AcceptedSnapshotDigest`. Every
other component of the vector is itself deferred above.

S1 does not persist an eligibility vector. The first stage with a dispatch or pivot step introduces it.

---

## 4. `OrderPreparation` semantics — CLARIFIED

Pack: `DOMAIN/01-AGGREGATES.md` and `DOMAIN/13-PERSISTENCE-DATA-DICTIONARY.md` line 12 — "OrderPreparations | ID,
caller/customer/owner, normalized source/digest, schema/profile, validity/assurance, consumed OrderId, rowversion |
Unique consumption; digest immutable; index expiry/consumed; no open Order created yet".

S1 reads this as follows.

- **`OrderPreparation` is not an Order and never becomes one.** `Create` reads a preparation and writes a new `Order`
  in one transaction. There is no state in which a preparation is a partially built Order.
- **"Unique consumption" is enforced by the database, and the link is stored on the Order side.** The Pack row lists a
  "consumed OrderId" column on `OrderPreparations`; S1 stores the same relation in the opposite direction, as
  `Orders.SourcePreparationId`, and guarantees uniqueness with the unique index
  `UX_Orders_Owner_SourcePreparation (OwnerAirlineId, SourcePreparationId)`. A second Order cannot consume the same
  preparation. The composite foreign key `(OwnerAirlineId, SourcePreparationId)` ->
  `OrderPreparations(OwnerAirlineId, Id)` additionally prevents an Order from consuming a preparation of another owner.
  **This is a DIVERGENCE in direction, not in guarantee:** there is no `ConsumedByOrderId` column on
  `OrderPreparations`, so "which Order consumed this preparation" is answered by a lookup on `Orders`, not by reading
  the preparation row. It is recorded here rather than left implicit. Proven by
  `ReliabilityClosureTests.R3_one_preparation_can_be_consumed_by_at_most_one_order`.
- **"Digest immutable" is a construction invariant.** The normalised candidate digest is computed once by
  `NormalizedCandidateJson` (canonical form `ordering-canonical-json-v1`) at preparation time and copied to
  `Order.AcceptedSnapshotDigest` at acceptance. No S1 command can recompute or overwrite it.
- **"Validity/assurance"** is the captured-at/expiry window of the preparation plus the assurance of the candidate; S1
  rejects a preparation outside its window at `Create`.
- **The authorized sales scope of the preparation binds the Order.** `CandidateValidator.EnsureSalesContext` rejects a
  candidate whose owner airline, financial customer, sales context or buyer differs from the authorized scope of the
  preparation. The preparation is the scope contract, not a hint.

This entry is a clarification of an existing Pack row, not a new decision.

---

## 5. Schema naming — DIVERGENCE

Pack: `DOMAIN/13-PERSISTENCE-DATA-DICTIONARY.md` line 5 — "One initial SQL Server database, schema areas
`Commercial`, `Fulfillment`, `Operations`, `Delivery`, `ReadModel`, `Messaging`, `Groups`, `Reference`."

S1 persists the Order aggregate in schema `Order`, not `Commercial`, and uses `Operations` as the Pack names it.

Reason: the schema name was fixed by the reviewed bootstrap commit and by every migration up to and including
`S1AuthoritativeFinalShape`. Renaming a schema is explicitly outside what this stage may decide (`CLAUDE.md` — "Never
download images/tools, delete files, rename schemas or change conventions without approval"), and renaming it now would
rewrite every applied migration for a name change with no semantic content.

The divergence is cosmetic: it changes no ownership, no invariant and no transaction boundary. It is recorded here so
that the rename, if the owner wants it, is a deliberate migration rather than silent drift. No Pack rule depends on the
literal schema name.

---

## 6. PII boundary — DIVERGENCE (scoped, open decision)

Pack: `DOMAIN/04-TRAVELERS-JOURNEYS-AND-PRIVACY.md` line 25 — "Monetary immutability is not permission to retain
unlimited PII. Store protected payload references for names/passports/guardian/assistance information with access and
retention policy. History stores non-PII correlation IDs and redacted metadata."

S1 stores traveller identity inline: `OrderTravellerIdentities` holds `GivenName`, `Surname` and `DateOfBirth` as
ordinary columns of the Ordering database, and `OrderContacts` holds `Email` and `Phone` inline.

What S1 does satisfy:

- Names, date of birth and contact details are the only personal data S1 holds. No passport, document, guardian
  assistance or special-needs payload is stored, because no S1 command accepts one.
- Nothing in Ordering history stores personal data. `OrderChange`, `PriceChangeSet`, `PricingLine`,
  `FundingObligation` and the outbox carry identifiers and money only. Traveller identity is reachable only through the
  current `OrderTravellers` row.
- The read side projects the same fields and adds none.

What S1 does not satisfy:

- There is no protected payload reference, no access policy and no retention policy. Erasing the personal data of a
  traveller today means deleting columns of a row that monetary records point at, which the same Pack line forbids
  ("Deleting/erasing a payload does not delete original money, operation identity or document-number uniqueness
  records").

Reason this is not closed in S1: a protected payload store is a service-level capability — a key-managed store, an
access-control surface and a retention job — not a column shape. Introducing one is a new architectural decision and an
owner decision about where the store lives and who owns the keys. S1 does not invent it.

Consequence stated plainly: **S1 is not privacy-complete.** The separation the Pack requires between monetary records
and erasable personal payloads is designed for but not implemented. The monetary tables already reference travellers by
identifier only, so the later move of name/DOB/contact behind a payload reference is a localized change to
`OrderTravellerIdentities` and `OrderContacts` and does not touch money.

**Refined by the owner in stage 09 (`OD-S1-09`).** The single gap is now separated into three parts. The canonical
Domain requirement — stable Traveller/Contact identity must not require perpetual PII retention — is **settled**, and
S1 already satisfies its structural half. A **protected payload reference boundary** is an **approved** internal
abstraction, so introducing it later is not a new architectural decision. What remains open is infrastructure only:
the physical store, key ownership and the retention schedule. That is an architecture gate before the privacy
lifecycle slice, **not an S1 blocker**. See `reports/00-decisions/S1-FINAL-SHAPE-OPEN-DECISIONS.md`.

---

## 7. What this file is not

This file does not relax an invariant, retype a Pack field, rename a Pack concept or authorise a "safe default" for a
missing one. Every DEFERRED entry states the stage that must implement it. Every DIVERGENCE entry states the Pack line
it departs from and why. §1.3 is now closed on semantics and deferred on persistence; §6 is refined to one remaining
infrastructure gate. Both are tracked in `reports/00-decisions/S1-FINAL-SHAPE-OPEN-DECISIONS.md`.

---

## 8. Current containment versus historical reference — CLARIFIED (stage 09)

Pack: `DOMAIN/13-PERSISTENCE-DATA-DICTIONARY.md`, `ItemServiceLinks/ServiceLineage/ItemLineage` row — "Append-only,
explicit many-to-many; **historical refs not constrained to current owner**".
Pack: `DOMAIN/01-AGGREGATES.md` line 55 — "**Current service ownership can move through split without changing
ServiceId.** History retains OrderIdAtOccurrence/Association."
Pack: `DOMAIN/12-SPLIT-GROUPS-AND-RELATED-ORDERS.md` step 3 — "move current traveler/service ownership … **Preserve
all historical OrderIdAtOccurrence** and issued facts."

Stage 08 treated every Order-scoped reference as current containment and made all of them composite on the current
`OrderId`. That was wrong for the historical half, and stage 09 corrected it. The rule the schema now follows:

| Kind | Rule | Example |
|---|---|---|
| **Current containment** — the dependent and the principal must be in the same Order *right now*, and they move together | composite foreign key on the current scoping column | `OrderService(OrderId, OrderItemId)`, `OrderAirTransportService(OrderId, TravellerId)` |
| **Immutable historical occurrence** — the reference records what was true when it happened, and the dependent identity may later move to another Order | stable-identity foreign key, plus an explicit occurrence `OrderId` column that is a recorded fact, not a foreign-key scope | `OrderItemServiceLink.OrderServiceId` with `OrderIdAtAssociation`, `PricingLine.OrderItemId` with `OrderId` |

The occurrence `OrderId` columns (`OrderItemServiceLink.OrderIdAtAssociation`, `PricingLine.OrderId`,
`FundingObligation.OrderId`) are **not** deleted and **not** weakened — they are the Pack's `OrderIdAtOccurrence`
facts. What changed is that they no longer act as a current-owner scope on the referenced row.

A composite key that spans an immutable occurrence pair is still correct, because neither side can move:
`OrderItemServiceLink(OrderIdAtAssociation, LinkedByChangeId)`, `PriceChangeSet(OrderId, ChangeId)`,
`FundingObligation(OrderId, ChangeId)`.

Full matrix and proof: `reports/09-S1-historical-identity-and-pack-reference/HISTORICAL-IDENTITY-CORRECTION-MATRIX.md`.
