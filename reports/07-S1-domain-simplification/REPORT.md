# Stage 07 — S1 Domain Simplification

Branch `k8s-stg` · reviewed baseline `0f103a3` · 2026-09-20

**Status: `S1_DOMAIN_SIMPLIFICATION_READY_FOR_OWNER_REVIEW`**
**`S2_NOT_STARTED`**

One item is blocked and one is unresolved; neither blocks review of the model itself:

- `BLOCKED_PERMISSION` — the migration rebaseline the owner approved could not be completed, because every attempt to
  remove the superseded migration files was refused by the environment's destructive-action guard. Details and the
  exact request in `MIGRATION-DECISION.md`.
- `BLOCKED_REAL_CONTRACT` — `pricingUnits[].coveredBoundOfferIds` carries a flight identity under a bound name in the
  recorded live payload. Stored opaque under the owner's own name, joined to nothing. Question in
  `IDENTITY-NAMESPACE-MATRIX.md`.

## Documents in this stage

| File | What it holds |
|---|---|
| `FIELD-INVENTORY.md` | Phase 1. Every field of the S1 model against the four admission tests, with a disposition. |
| `BEFORE-AFTER-MODEL.md` | Measured before/after: tables, columns, files, and the shape changes that matter. |
| `IDENTITY-NAMESPACE-MATRIX.md` | Every identifier classified. No silent `UNKNOWN`. |
| `DELETED-FIELDS.md` | Every removal with its reason, including the missing-beneficiary replacement wording. |
| `MIGRATION-DECISION.md` | What was generated, why it is not the end state, and the blocked request. |
| `TEST-RESULTS.md` | Commands, outcomes, forced-failure evidence, tests removed and why. |
| `01-runs/` | Raw build, test and EF output. |

## What the stage did

**Removed the abstractions that had no second case.** `ValidityFact` (five members over one instant),
`ObservedTimeFact`, `CurrencySnapshot`, `ProductSnapshot`, `CommercialTermsSnapshot`, `BuyerSnapshot`,
`AcceptedSource` (fourteen members duplicating the preparation row), `FulfillmentProfileSnapshot`, the service detail
schema registry with its single entry, and the dynamic `Details` dictionary carrying three typed concepts.

**Removed the invented source identities.** `"OFFER-PACKAGE"`, `"{boundId}|{flightId}"`, `"pricingUnits/{i}"`,
`"airoffer:details:pricingUnits"`, `"preparation:{id}:{digest}"` — mapper constants and local correlation keys that
were being stored as if the owner had supplied them. Exactly one honest occurrence path survives:
`PricingLine.SourceOccurrencePath`.

**Typed every identity the owner already sends as a number.** Seventeen `*Ref:string` columns became `int`/`long`.
The canonical candidate writes them as JSON numbers. `Money` carries `CurrencyId:int`.

**Made the air service structurally correct.** One traveller, one passenger segment, both required and positive, with
cabin, RBD, booking class, baggage and sold terms as typed members — instead of a beneficiary collection and a
coverage collection that always held exactly one row each, plus a derived `SoleCoveredSegmentId` workaround.

**Removed the lifecycle that cannot occur.** Capture, acceptance and consumption commit in one `SaveChangesAsync`, so
`OrderPreparation` needs no consumption state, no consumption index, no CHECK and no conflict translation.

**Removed compatibility leakage from Domain and Query.** `LegacySellingOfficePolicy`, the schema-2/3 projection
compatibility readers, the candidate schema-3 reader/writer. Under the owner's rebaseline answer the candidate schema
restarts at `1.0` and the projection schema at `1`.

**Removed the placeholder fare-construction structure** — pricing groups, component↔service and component↔segment
links, fare owner/tariff/rule/routing refs — all of which AirOffer never supplies and which were always empty.

## Scenario matrix delta

The stage-06 matrix is that stage's record and has not been rewritten. This is the delta a reader needs:

| Stage-06 row | Was | Now | Note |
|---|---|---|---|
| Missing beneficiary (Pack scenario) | `TESTED` via one runtime validation | **`TESTED`** via four levels | ACL fail-closed, canonical reader, candidate validation, construction invariant — see `TEST-RESULTS.md` |
| 41 — group line extended once | `TESTED` (`SC_S1_011_…`) | **`NOT_APPLICABLE`** | AirOffer supplies no pricing group; the concept is removed, not untested |
| 51 — round trip as one RT unit vs two OW units | `TESTED` (`SC_S1_009_…`) | **`NOT_APPLICABLE`** | the structure asserted (coverage rows, construction assurance) is not supplied by any owner today |
| 52 — opaque fare construction | `TESTED` | **`TESTED`** | `SC_S1_010_…` retained, minus the assurance and coverage assertions |
| 57 — unknown registered detail schema / version | `TESTED` (`SC_S1_021_…`) | **`NOT_APPLICABLE`** | the registry is removed; the service is one typed shape |
| 54, 55 — response `offerId` missing / mismatched | `UNSUPPORTED` | **`TESTED`** | strict fail-closed guard plus `A_response_without_a_usable_offer_id_…` and `A_response_that_prices_another_offer_…` |
| — new | — | **`TESTED`** | repeated `travellerRef` across tickets is a contract mismatch before any candidate exists |

The three `NOT_APPLICABLE` rows are the only scenarios whose tests were removed without replacement. Each is a Pack
scenario whose *structure* the current owner contract does not produce; none is a rule that silently stopped being
enforced. If a future owner supplies pricing groups or component coverage, the structure and its tests come back.

## Divergences and things still unproven

- **The migration chain is not the one the owner approved.** See `MIGRATION-DECISION.md`. The generated additive
  migration contains nine semantically wrong column renames that are harmless only because no database holds S1 rows.
- **`coveredBoundOfferIds` is unresolved.** Fare-construction coverage cannot be joined to journeys or segments until
  the owner answers. Nothing else in S1 depends on it.
- **`LegacyOrderFixture.cs` is now unreferenced** and is listed for deletion with the migration files.
- **`Category=Live` tests were not run** — they need a live AirOffer base URL and a currently priced offer.
- **One full persistence run hung** for over ten minutes with no SQL activity and was stopped; two later runs on the
  same build finished in under a minute. Not reproducible, recorded rather than hidden.
- **Four `OrderingS1_*` development databases** were left behind by the stopped runs on `localhost\SQLEXPRESS`. They
  belong to this suite; they have not been dropped without the owner's word.

## Evidence summary

| | |
|---|---|
| `dotnet build AeroTech.Ordering.sln` | succeeded, 0 errors |
| Domain tests | 58 / 58 |
| Persistence tests (`Category!=Live`) | 134 / 134, 49 s |
| `has-pending-model-changes` | none, all three contexts |
| Packages added | none |
| Commits or pushes | none — the owner has not asked |
