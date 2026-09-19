# S1 Final Closure — Applied Decisions

Recorded 2026-09-19 · branch `k8s-stg` · baseline HEAD `61d64724e07e38d10808e1895b82afaaf1d16fe0`

## Authority for this record

The implementation applied the **binding closure specification** supplied by the owner for this stage
(`AeroTech.Ordering-S1-FINAL-Closure-Implementation-Prompt.md`). That specification is the decision; this file records
what was applied so the code can be traced back to it.

**This file does not answer the earlier `Answer:` lines.** `OD-CLOSE-01` … `OD-CLOSE-09` in
`reports/06-S1-create-order-conformance/OPEN-DECISIONS.md` remain as written — recommendations with empty `Answer:`
lines. Where the binding specification decided the same question, the decision recorded here is the operative one and
supersedes the recommendation; where the specification corrected a recommendation, the correction is noted.

## Applied decisions

| # | Decision as applied | Where it landed | Relation to the earlier recommendation |
|---|---|---|---|
| 1 | **Strict `offerId` fail-closed** at the AirOffer ACL: null, empty, whitespace or non-ordinal-equal → `ContractMismatch`; no trim, case-fold, alias or fallback | `AirOfferCandidateMapper.EnsureRespondedOffer` | matches `OD-CLOSE-02` option (a) |
| 2 | **Generic `SettlementAttribution(PartyRef, CategoryCode)`**, both opaque source strings, required iff `Effect = SettlementOnly` and forbidden otherwise; currency is the committed `SaleValue` currency; no enum, no Commission aggregate, no assumption that `PartyRef` is the seller | `Domain/_Shared/ValueObjects/SettlementAttribution.cs`, `PricingLineMatrix`, `PricingLineConfiguration` | matches `OD-CLOSE-01` option (a) |
| 3 | **Clean public selling-office contract before freeze**: `OrderDto.AirlineOfficeId` removed; `SellingOfficeId` + `SellingOfficeKind` added; no deprecated compatibility field | `OrderDto`, `OrderProjectionDocument`, `OrderProjectionMapper` | `OD-CLOSE-05` option (a) chosen over (b) |
| 4 | **Backoffice seller = the owner airline** — `(BusinessContextType.Airline, OwnerAirlineId)` | `AuthorizedScopeResolver.BackofficeSaleAsync` | ratifies the open question in `OD-CLOSE-05` |
| 5 | **OtaPanel seller = `TravelAgencyId`**, persisted as an accepted fact and never recovered from `CallerScope` | `AuthorizedScopeResolver.OtaPanelSaleAsync` | closes the `OD-CLOSE-05` seller gap |
| 6 | **OTA / Service seller = NotSupplied**; `PartnerApiAccessProfileId` is the initiating actor and is neither buyer nor seller nor distributor | `AuthorizedScopeResolver.OtaSaleAsync` / `ServiceSaleAsync` | closes the `L4` distributor question without building a chain |
| 7 | **Buyer = NotSupplied on every current S1 surface**, as an explicit both-null `BuyerSnapshot`; never inferred from FinancialCustomer, Actor, Traveler or Seller | `Domain/_Shared/ValueObjects/BuyerSnapshot.cs` | answers the `OD-CLOSE-05` `NotSupplied` question |
| 8 | **No Distributor field is added now** — no current S1 source supplies an authoritative distributor | — | corrects the earlier assumption that a distributor slot was needed |
| 9 | **Lifecycle vocabulary corrected before freeze**, preserving every existing numeric value: service `Replaced = 4`, `Expired = 5`; item appends `PartiallyChanged = 4`, `Partitioned = 5`, `Expired = 6`, `Inactive = 7` | `OrderServiceCommercialStatus`, `OrderItemCommercialStatus` | matches `OD-CLOSE-09` option (a) |
| 10 | **Component totals as `(OrderId, Component, Effect, DebitAmount, CreditAmount, CurrencyRef)`** — net derived, never stored; sale currency only; all effects; order level only; deterministic persisted summary | `OrderComponentTotal` + configuration | matches the corrected `OD-CLOSE-06` shape |
| 11 | **FulfillmentProfileSnapshot completed structurally**: `DocumentAuthority?` (existing enum), `ResourceUnitPolicyRef`, `DeliveryControlPolicyRef`, `DependencyTreatmentPolicyRef` as opaque refs, `PartialFulfillmentSupported` as a **nullable** bool. No duplicate `DocumentRequired` flag; no engine | `FulfillmentProfileSnapshot`, `CandidateFulfillmentProfile` | matches the corrected `OD-CLOSE-07` |
| 12 | **FundingObligation scope as three nullable real FKs** (`OrderItemId`, `OrderServiceId`, `PricingLineId`) with a SQL CHECK requiring exactly one, and a typed `FundingObligationScope` factory. No `ScopeKind`, no polymorphic FK. S1 original sale stays item-scoped; a `CustomerBalance` line with no item is line-scoped rather than null-scoped | `FundingObligation`, `FundingObligationScope`, `FundingObligationConfiguration` | matches the corrected `OD-CLOSE-08`; disposition **not** added |
| 13 | **Funding disposition and `Order.ObligationRevision` remain S3 design input** — no vocabulary invented | — | keeps `OD-CLOSE-08` question 3 open |
| 14 | **Candidate schema 3.0 → 4.0; `CanonicalJson.Version` unchanged** at `ordering-canonical-json-v1`, because the canonicalization algorithm did not change | `NormalizedCandidate`, `NormalizedCandidateJson` | new; not previously recorded |
| 15 | **Projection schema 3 → 4** with explicit schema-2 and schema-3 compatibility readers; deterministic rebuild is the upgrade path; schema 3 is never mutated in place | `OrderProjectionJson`, `Projection/Compatibility/` | matches the `A8` plan item |
| 16 | **`GetOperation` remains deleted for S1**; the earlier "remains reachable on the internal surface only" statement is amended by this decision | — | answers the `OD-C-04` contradiction raised in `OD-CLOSE-03` |
| 17 | **Public pricing code/name exposure remains deferred**; `SourceCode`/`SourceName`/`SourceReference` stay internal | — | leaves `OD-CLOSE-04` deferred, as recommended |
| 18 | **Seat remains S6**; no seat member was added to `AirTransportDetail` | — | unchanged across all revisions |
| 19 | **Interline settlement remains future**; it reuses the same generic `SettlementAttribution` when it arrives | — | matches the revision-3 reclassification |
| 20 | **`Service2Service` request carries `SellingOfficeId` + `SellingOfficeKind`**; Backoffice keeps `AirlineOfficeId` because that surface's office is genuinely airline-scoped; `NotRecorded` is rejected on a new request | `ServiceCreateOrderFromOfferRequest`, command, validator, resolver | new; corrects the misleading generic office input |

## Explicitly not decided here

- The **funding disposition vocabulary** (`DOMAIN/06` names behaviours but never enumerates states).
- Resource **unit policy**, **delivery/control policy** and **dependency treatment** vocabularies — carried as opaque
  source/profile-owned refs until an owner contract defines them.
- Any **buyer identity** on any surface.
- `OD-CLOSE-03`'s remaining stage-04 reconciliation rows other than `OD-C-04`.

Nothing in `docs/ORDERING-DESIGN-PACK-v3.8/` was modified.
