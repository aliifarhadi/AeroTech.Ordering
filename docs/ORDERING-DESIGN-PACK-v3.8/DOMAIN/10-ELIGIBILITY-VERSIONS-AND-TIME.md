# Eligibility, versions, time and mutation matrix

## Eligibility contract

A pure `EligibilityDecision` has Allowed/Denied/PendingEvidence/Unsupported, structured reasons, evaluated scope, required facts/capabilities and EvaluatedAt. Denied means a known business condition fails; PendingEvidence is not a business rejection. Application loads canonical facts and obtains fresh owner authority when required, then calls the domain policy. Query may show a local evaluation from its denormalized facts with staleness labels.

Required dimensions are commercial activity/terms, subject/beneficiary scope, pricing decision and accepted context, capacity, funding authority, document financial disposition/control/validity, operational consumption, dependency closure, actor authority and active operation claim. No single Order status replaces them.

| Use case | Main preconditions |
|---|---|
| Prepare | Authorized offer/customer context, supported source projection and product schema; no Order mutation |
| Create | Exact unconsumed preparation/digest, complete traveler bindings, accepted source/authority policy valid, no pricing replacement |
| Reserve | Active resource-bearing scope, exact quantity/type, no unresolved existing effect, coupling closure |
| StartIssue | Eligible commercial/doc data and an executable capacity/funding plan; Held can start commit |
| CommitDocuments | Committed required capacity + operation-bound funding authority + document/control/profile eligibility |
| Cancel | Allowed unissued scope/terms, dependency closure, no uncertain issue, known release dispositions before final reference pivot |
| Void | Issuer-authorized deadline and unused eligible coupon/control; explicit effect on commercial scope |
| Refund | Accepted AirPrice decision, reversible value + tender authority, valid coupon/control scope, no competing disposition |
| Exchange | Accepted full old/new pricing scope, new resource/funding authority, old coupon control, dependency reassociation plan |
| Revalidate | Profile supports exact change; no implicit fare change, no replacing immutable issued snapshot |
| Split | Whole-traveler/guardian closure, shared-service rule, priced transfer and owner divide/application evidence |

## Version scopes

- CommercialVersion starts at 1 on accepted Create; increments once per Order's committed commercial change. A duplicate/no-op/rejection increments zero.
- FinancialSequence starts at 0; the original committed monetary PriceChangeSet receives 1. Every subsequent set increments once, including an explicitly accepted zero-delta repricing set. It does not count events.
- OrderRevision starts at 1 and increments once per local transaction that changes the projected Order facts (commercial, documents, capacity/funding evidence, operations or delivery). Duplicate observations/audit-only attempts with no view change need not increment it. It is the local read watermark, not a business version.
- ServiceVersion starts at 1 and changes for current commercial definition/owner/item/coverage binding changes. Price-only item reassociation keeps ServiceId but increments ServiceVersion for the ownership change. A DCS seat reassignment does not change it.
- DocumentVersion changes for accepted document/control/association facts, not a GET/retry.
- Obligation.Version changes only for that obligation; Order.ObligationRevision tracks discovery of obligation changes. Source coverage is checked against the exact obligation version.
- SQL RowVersion is a concurrency token; it is not a timestamp, business version or provider version.
- EventOrdinal is a strictly increasing sequence of published facts per Order stream; EventId identifies a fact across delivery retries. Multiple facts can share one CommercialVersion.

| Committed action | CommercialVersion | FinancialSequence | Other relevant revision |
|---|---:|---:|---|
| Create with sale | initialize 1 | initialize set 1 | OrderRevision 1 |
| Reserve/release/commit/expire evidence | 0 | 0 | OrderRevision; resource evidence version |
| Coverage evidence / cash confirmation | 0 | 0 | OrderRevision; owner movement version |
| Issue ETKT/EMD only | 0 | 0 | OrderRevision; DocumentVersion |
| Add paid product | +1 | +1 | New scoped obligation; ServiceVersion 1 |
| Commercial cancel/refund/exchange pivot | +1 once | +1 if accepted monetary set | Service/document/obligation revisions as changed |
| Later refund payout or exchange cleanup | 0 | 0 | OrderRevision, owner evidence |
| Document-only Void | 0 | 0 | DocumentVersion, OrderRevision |
| Traveler correction accepted | +1 | 0 unless priced charge set | OrderRevision; affected document flow separately |
| Revalidation changing accepted flight binding | +1 | 0 unless priced change | ServiceVersion, DocumentVersion |
| Operational schedule / DCS observation | 0 | 0 | OrderRevision; source/aspect version |
| Split | source +1; child starts1 | paired transfer sets | Current ownership versions; lineage |
| Rebuild projection, read, replay/no-op | 0 | 0 | Projection schema/technical timestamp only |

A quote freezes CommercialVersion plus relevant ServiceVersions, document/control versions, obligation versions, source pricing context and dependency scope hash. A DCS control change can invalidate acceptance even when CommercialVersion did not change. Check the vector before dispatch/pivot; a recovered remote effect is still recorded even if later facts conflict, then reconciled rather than discarded.

## Independent time facts

OfferExpiresAt is AirOffer's fact; PriceValidUntil is AirPrice's fact; HoldExpiresAt is FlightFlow's fact; Coverage/AuthorizationExpiresAt is JetPay's fact. Each carries source/profile/reference/version and scope. TicketingDeadline ownership/composition is BD-004; retain a supplied LastTicketingDate with its actual source label, never relabel it as OfferExpiresAt.

A missing deadline is Unknown/NotSupplied, not infinite validity. A source can explicitly declare NotApplicable with reason. Ordering may compute the earliest of applicable known constraints for a display, but stores each original and does not invent a universal Order TTL. A local preparation AcceptBy/retention policy may be stricter; it never extends an owner's deadline or substitutes for missing owner validity.

For locally evaluated eligibility, expired at `now >= DueAt`. Owner-confirmed timely irreversible effects remain valid evidence even if received later. Timeout/worker-lease expiration does not commercially expire an Order. Before an expiry-triggered mutation, reconcile any protecting uncertain effect; never mark issued services expired from an obsolete hold TTL.
