# Servicing choreography - explicit pivots and failure dispositions

Every servicing plan freezes target services/documents, dependency closure, pricing decision, resource/funding profiles and base fact-version vector. A quote is displayed before acceptance. Expired/stale/wider-than-accepted scope is rejected before a new irreversible dispatch. Later recovery reuses the accepted plan; it does not recalculate today's fare.

## 1. Pre-ticket cancellation (S5)

Reference scope contains only unissued eligible services. An issued/unknown-issue document cannot pass this gate; use Void/Refund or reconcile issuance. Obtain an authoritative cancellation decision when money/terms change. Full free cancellation is a named reference pricing fixture, not an assumed universal airline policy.

Prepare receipt/claim/plan. Release known held capacity (or cancel committed capacity under a supported capacity contract) and release unused funding authorities/applications as the plan specifies, with separate keys/evidence. Unknown in either remains pending; no unconditional ApplyReleased or commercial Cancel. Once required owner effects are conclusive, one local pivot cancels scope, appends approved price reversals/penalties, increments CommercialVersion once and projects/events the result. A service never disappears from history.

Release-only is not cancellation. Failure after capacity release but before funding release leaves active commercial scope with an explicit pending cancellation restriction and no usable reservation. It is not falsely reported as Ready/Cancelled. Continue the same plan or resolve it authoritatively; do not silently re-reserve to roll back.

## 2. Ancillary acceptance (S6) and EMD (S7)

Resolve catalog definition/eligibility from Ancillary, price from AirPrice and candidate composition from AirOffer when bundled. Accept a versioned product/pricing/coverage/profile snapshot. Local AddAncillary commits the new commercial item/service and delta obligation; it does not pretend capacity or supplier fulfillment is guaranteed. A capacity-free lounge/insurance product never consumes a FlightFlow seat. A capacity-bound product uses S2 before S4/S7 issuance.

A scarce product whose owner requires a guarantee before commercial acceptance needs an explicit `ReserveBeforeAccept` product profile. Its provisional owner reservation is its own durable preparation operation; it cannot be disguised as ordinary Create's inline hold. Until that profile's binding/compensation is certified, reject that product rather than weakening the standard Create rule.

S7 extends the SAME IssueOrder/document module with EMD-A associations and EMD-S monetary/value purposes. It does not build a second EMD-only payment/issue rail. Included services, paid quantity, delivery quantity, price amount and document requirement are separate fields.

## 3. Void (S8)

`VoidDocument` means invalidate eligible document/coupons under issuer policy; it does not inherently cancel the commercial service or refund money. Validate exact scope, control authority, deadline, no usage/exchange/refund and dependent EMD associations. Local authority commits document Void plus stock/history/projection/event in one transaction, CommercialVersion unchanged. External authority persists intent, dispatches, recovers and applies only confirmed outcomes.

`VoidAndCancel` is a separately named orchestrated intent composing the same canonical document transition and accepted cancellation/funding/resource plan. No duplicate implementations. CommercialVersion changes only for its accepted commercial cancellation pivot; financial credits require the approved pricing decision. A void-window cutoff is source/profile evidence, not midnight in the server's timezone. DocumentOnly may retain a reservation for reissue; that intention is explicit in the plan/read model.

## 4. Refund (S9)

Reference sequence: read canonical sold/used context -> obtain AirPrice refund decision -> display/accept exact decision -> claim scope -> acquire required document control -> reserve JetPay refund authority for original applications -> perform required capacity disposition -> apply local commercial/document refund pivot -> execute refund payment -> confirm or reconcile payment.

`RefundPlan` freezes original money/application refs, coupon/service scope, refundable components, penalties/waivers, NetPayable and currency, quote expiry/version, historical context and authority. AirPrice computes used fare, taxes and penalty; historic allocations are not a refund entitlement. JetPay reserves/guarantees tender payout capacity under OWNER_REQUIRED `AcquireRefundAuthority`; neither a positive Order credit nor Ledger balance alone permits payout.

Local pivot: append the approved commercial credits/new penalties once, mark local eligible coupons Refunded, record RefundPayable/disposition and operation phase CommercialApplied, update Order/services according to accepted cancellation scope, projection/outbox and CommercialVersion once. For external documents, confirmed redemption evidence is required before applying that part of the local pivot; partial issuer outcomes remain explicit and restrict scope.

After pivot, ExecuteRefund uses one stable payment operation/application reference. Unknown payout remains PendingRefund; coupons and commercial credits are NOT reversed to pretend failure. Emit RefundConfirmed only after authoritative payout confirmation (or a validated zero-payable disposition), not just quote acceptance. Emit RefundAuthorized at the earlier commercial pivot. Payment confirmation does not append another credit or increment CommercialVersion again.

Before the pivot, a definitive no-effect failure may release reserved refund authority and control under the defined compensations. After the pivot, failures require completion/reconciliation; no automatic reactivation. Additional goodwill is a separately authorized adjustment/funding source, not an over-reversal of a historic line. Preserve per-application refund reservations to prevent concurrent over-refund.

## 5. Exchange/reissue (S10) - fixed reference graph

1. AirOffer supplies the replacement candidate; AirPrice supplies an accepted exchange decision over all affected old/new items, pricing units and dependencies. Freeze old unused value, new commercial components, additional collection/residual disposition and historical context.
2. Claim Order/scope; obtain required old coupon/control restriction before replacement effects. Block old coupon use and competing refund/void locally and through any required external control owner.
3. Reserve replacement capacity under stable new resource keys without releasing old usable capacity yet.
4. Acquire operation-bound exchange funding authority: transfer/reuse old valid application value, collect only approved delta, reserve any residual payout. Reusing historic coverage is an owner-confirmed reassignment, not copying an amount locally.
5. Commit replacement capacity and verify all required coupled members. Keep old resources tracked; no premature release.
6. LOCAL document authority: in one local pivot redeem old eligible coupons as Exchanged, issue replacement documents/numbers, append commercial old/new or delta treatment ONCE, create successor items/services and dependency reassociations, update scoped obligations/history/projection/outbox and CommercialVersion once.
7. EXTERNAL authority: execute the declared issuer exchange group and retain per-old/new document evidence. The contract must explain partial redemption/issue and read-back. Do not assume cross-document atomicity; do not mark the full exchange commercially complete while unresolved issuer roles exist. Apply a local pivot only for an explicitly complete accepted group.
8. After the pivot, release old capacity, finalize application transfer/use and execute approved residual payout. Each is a distinct durable cleanup step. Unknown cleanup does not undo replacement tickets or start a second exchange.
9. Mark the operation Completed only when required cleanup/dispositions are conclusive. Views distinguish CommercialApplied from Completing and list unresolved effects.

Before replacement document/pivot commitment, compensate known new holds/unused funding if the plan definitively cannot complete; never compensate unknown document effects. After the pivot, use forward recovery, not database rollback across services. A `Reissue` changes documents according to an accepted plan; a `Revalidation` retains document identity and updates supported associations/details. A zero additional collection does not prove no funding-transfer/control work is needed.

## 6. Economic events versus workflow events

Events identify `EconomicFactId`, ChangeId/PriceChangeSetId, OperationId, document refs and `EconomicRole`. A document fact, a commercial pivot fact and a final workflow-completed notification may describe the SAME economic change. Consumers must not add their totals three times. Ledger chooses posting policy; Ordering supplies identity and distinct facts. Full payload contracts are in CONTRACTS/08-LEDGER and SPEC/events.json.

## 7. Finalized servicing view

Persist an immutable finalized record joining the actual accepted decision, scope, old/new documents/resources, confirmed payment movements, pending dispositions, actor/office and timestamps. Redisplaying a refund/exchange receipt never calls pricing again. Rendering/email delivery is not a new document engine in Ordering; downstream consumers use these semantic facts.
