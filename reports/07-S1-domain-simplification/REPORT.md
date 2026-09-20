# Stage 07 — S1 Domain Simplification (revision 2)

Branch `k8s-stg` · reviewed baseline `0f103a3` · R2 review HEAD `3e43d37` · 2026-09-20

**Status: `S1_DOMAIN_SIMPLIFICATION_NOT_READY`**
**`S2_NOT_STARTED`**

The status stays `NOT_READY` until independent review closes the matrix. All nine `MISSING_RESTORE_NOW` verdicts the
R2 review raised are now closed in code and in tests, but the stage is not the agent's to declare ready, and two
things remain outstanding:

- `PROPOSED_REBASELINE_AWAITING_OWNER_AUTHORIZATION` — the migration chain is not the end state, and the rebaseline has
  **not** been authorized. The previous revision of these reports claimed it had been; that claim is withdrawn. See
  `MIGRATION-DECISION.md` and the open decision at
  `reports/00-decisions/S1-MIGRATION-REBASELINE-OPEN-DECISION.md`.
- Two verdicts that are not the agent's to resolve: `FundingObligation.CurrentDisposition` is
  `BLOCKED_OWNER_CONTRACT` (no approved vocabulary, so no enum was invented) and `coveredBoundOfferIds` is
  `BLOCKED_REAL_CONTRACT` (the recorded owner payload carries a flight identity under a bound name).

## Documents in this stage

| File | What it holds |
|---|---|
| `PACK-REQUIRED-SHAPE-GAP-MATRIX.md` | **New.** Every Pack-required fact against the code, with the R2 verdict and the verdict now. This is the document that can see a fact that was deleted outright. |
| `FIELD-INVENTORY.md` | Phase 1 inventory of fields that exist, with the `CallerScope` rationale corrected. |
| `BEFORE-AFTER-MODEL.md` | Measured before/after, including the five tables restored in this revision. |
| `IDENTITY-NAMESPACE-MATRIX.md` | Every identifier classified. No silent `UNKNOWN`. |
| `DELETED-FIELDS.md` | Every removal with its reason, including the missing-beneficiary replacement wording. |
| `MIGRATION-DECISION.md` | What exists in the chain, why it must not ship, and the authorization the rebaseline needs. |
| `TEST-RESULTS.md` | Commands, outcomes, the tests added for each correction, forced-failure evidence. |
| `01-runs/` | Raw build, test and EF output. |

## What revision 2 corrected

The R2 review confirmed the simplification direction and the fixes in revision 1, and found nine places where the
cleanup had cut into Pack-required shape. All nine are restored, each in its smallest honest form.

**Typed service boundary.** `OrderService` was one sealed entity carrying traveller, segment, cabin, RBD, booking
class and baggage — an air service in disguise. It is now `abstract OrderService` (identity, item, `ServiceType`,
fulfillment profile, commercial status, creating change) plus `OrderAirTransportService` (one traveller, one passenger
segment, cabin, RBD, booking class, baggage, sold terms), persisted as TPH with `ServiceType` as the discriminator and
exposed through the projection and the public DTO. No dynamic details, no schema registry, no supplier execution
behaviour came back with it.

**Buyer.** `BuyerSnapshot(ContextType?, BuyerId?)` is back on the order, in the candidate and in the snapshot digest.
Both null means `NotSupplied`, which is what every current surface writes — and absence is itself the accepted fact.
The buyer is never inferred from the financial customer, the seller, the actor or a traveller.

**Persisted component totals.** `OrderComponentTotal` is back with the composite key `(OrderId, Component, Effect)` —
no surrogate id, no `ROW_NUMBER`. Totals are computed once at acceptance from the committed `PricingLine.SaleValue`
and the projector reads them, so there is no second arithmetic implementation on the read side.

**Honest fulfillment snapshot.** `FulfillmentProfileSnapshot` is back without the invented
`AIROFFER-OBSERVED-AIR-UNCERTIFIED` profile id: `ProfileRef` and `ProfileVersion` are nullable and null,
assurance is `NotCertified`, reservation/document/funding requirements are `Unresolved`, and claiming `Certified`
without naming the profile is refused. No fulfillment engine was built.

**`PricingLine.Role`.** Back on the candidate and the entity, persisted and projected. The mapper writes `Original`;
`OriginalPricingLineId` stays deferred until reversal behaviour exists.

**Funding liability can no longer disappear.** `FundingObligation` carries `OrderItemId?` / `OrderServiceId?` /
`PricingLineId?` with real FKs and `CK_FundingObligations_ExactlyOneScope`, built through a typed scope factory, and
linked to its `PriceChangeSetId` instead of a fabricated `SourceDecisionRef` string. Acceptance scopes every
customer-balance line and then asserts the obligations sum to `CustomerTotal` (20296); the candidate validator refuses
a customer-balance line that has neither an item nor a service basis.

**Fare construction is honest about what it prices.** `CandidateFareConstruction` now carries `Assurance` and explicit
`ItemKeys`; `FareConstruction` links exactly those items instead of every order item, and a construction naming an
unknown item is refused. A source that supplies no pricing units produces **no** construction — and the test builder no
longer fabricates a default unit whose covered bounds were journey bound ids.

**Pricing-unit vocabulary matches the real contract.** `AeroTech.Messages.AirOffer.Enums.PricingUnitKind` contains
`OneWay`, `RoundTripFromOneWays` and `RoundTripFare`. The mapper previously accepted `OneWay`, `RoundTrip`, `OpenJaw`
and `CircleTrip`, three of which the contract never sends, and the live fixture only ever exercised `OneWay`. All
three real values are now mapped and tested, to `FarePricingUnitType` **plus** `FarePricingUnit.SourceConstructionType`
(`AirFareConstructionType`), so the distinction between the two round-trip constructions is not lost. Anything else
fails closed.

**`RootOrderId`.** The original order still sets `RootOrderId = Id`, but the database check is now
`[RootOrderId] > 0` instead of `[RootOrderId] = [Id]`, which a future split child could never satisfy. No split
behaviour was implemented.

**Currency code snapshot.** `CurrencyId` stays the identity everywhere; the source's own `currencyCode` survives once,
as `Order.SaleCurrencyCode`, and is not repeated inside every `Money`.

**Minimal item–service link.** `OrderItemServiceLink` (`Id`, `OrderIdAtAssociation`, `OrderItemId`, `OrderServiceId`,
`LinkedByChangeId`) is created at original sale. The traveller and segment child tables did not come back — the typed
air service already owns that scope.

**Naming corrections.** `SellingOfficeKind.NotRecorded` is deleted from the shared enum together with the validation
branches that existed only to reject it; an office id whose namespace cannot be proven now fails closed. Both create
surfaces, their commands, validators, controllers and the published OpenAPI document say `FinancialCustomerId`.
`RebuildOrderProjectionResult.OperationId` — which was carrying `CommandReceipt.Id` — is now `ReceiptId`, and
`GetOperation` was not restored.

**Report correction.** `OrderPreparation.CallerScope` is **not** indexed. It is kept because it is one of the inputs
to `ComputeDigest`, binding the accepted snapshot to the scope that accepted it. `FIELD-INVENTORY.md`,
`IDENTITY-NAMESPACE-MATRIX.md` and `DELETED-FIELDS.md` now say that instead of the earlier false claim.

## What revision 2 deliberately did not undo

Every simplification the R2 review confirmed stays: typed numeric identities, the validity simplification,
`SourceJourneyTypeRaw` and `SourceDirectionRaw` deletion, the fake source-identity cleanup, `PackExamples.cs` deletion
with its four replacement intents, the preparation consume-lifecycle deletion, the `Traveller` spelling correction, the
Query table-name fix, strict `offerId`, the four-level missing-beneficiary evidence, `SettlementAttribution`, the
removal of pricing groups and component service/segment coverage that no owner supplies, the dynamic service
schema-registry removal, the fake fare `SourceContextRef`/`SourceUnitRef` removal, and the undeployed-schema
compatibility cleanup.

The PII projection design was not touched: names and contacts stay out of the projection JSON, and `OrderDtoReader`
loads traveller identities and contacts only when the authorized read scope permits protected payloads.

On `OrderPreparation.Consume()`, the wording is now precise: the receipt protects idempotency of the scoped command and
key. It is not a generic "an offer can only be sold once" rule, and the removed `Consume()` never represented one —
the same offer with a new intent and key is intentionally allowed to create another order.

## Evidence summary

| | |
|---|---|
| `dotnet build AeroTech.Ordering.sln` | succeeded, 0 errors |
| Domain tests | 78 / 78 |
| Persistence tests (`Category!=Live`), including API/OpenAPI, architecture and fresh-migration tests | 153 / 153, 1 m 41 s |
| `has-pending-model-changes` | none, all three contexts |
| Packages added | none |
| Commits or pushes | none — the owner has not asked |
| `MISSING_RESTORE_NOW` verdicts open | 0 |
| Blocked verdicts open | 2 (`BLOCKED_OWNER_CONTRACT`, `BLOCKED_REAL_CONTRACT`) |
