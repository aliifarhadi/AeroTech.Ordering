# Commercial obligations, funding evidence and issue authority

## Ownership

Ordering owns what is commercially owed for an accepted scope. JetPay owns payment instruments, authorization/capture/refund and applied coverage truth. Ledger owns customer-wide debit/credit balances, AR/AP and journals. A local OrderTotal/coverage view is not a wallet or customer balance master.

## Scoped obligations

`FundingObligation` fields: ObligationId, OrderId, Version, Purpose, sale currency, exact amount, Service/Item/PricingLine scope, accepted ChangeId, superseded obligation ref, source pricing decision and current disposition. Purpose: OriginalSale, AddedService, ExchangeAdditionalCollection, Fee or RefundDisposition. RefundDisposition is not positive issue coverage.

Versions are per obligation. Adding a EUR20 bag to an already covered EUR120 ticket creates a EUR20 added-service obligation; it does not request another EUR140 payment and does not revoke the original issued ticket. The Order can have several obligations. `Order.ObligationRevision` is a discovery watermark only, not a substitute for the exact obligation ID/version in a provider request.

If accepted servicing supersedes a still-open obligation, record the supersession and release/rebind its owner application through an explicit owner operation. Never move a payment reference to another scope locally. Preserve original obligations and applications for audit.

## Coverage evidence

`FundingCoverageEvidence`: provider/profile/contract, CoverageRef, ApplicationRef, MovementId where applicable, ObligationId/Version, ScopeHash, confirmed currency and amount, cash/guarantee classification, source revision, ValidUntil when supplied, ObservedAt, operation ref and raw protected evidence ref/hash. A successful HTTP payment screen is not coverage. Pending, insufficient, unknown, expired, wrong-currency, wrong-scope or stale-version evidence cannot authorize Issue.

Cash applications are deduped by owner MovementId/ApplicationId; reversals/refunds reference the original. An authorization/credit guarantee may satisfy Issue under its profile but is displayed separately from captured/applied cash. Do not add a guarantee and the later settling cash as two payments. Split transfers preserve value and unique applications; never copy them.

## Required port methods (not claimed current JetPay endpoints)

`EstablishCoverage` is a command only when an owner action is requested. `ReadCoverage` and `ReadOperation` are queries. `AcquireIssueAuthority` is a command if it reserves/commits coverage for the exact issuance operation. `FinalizeIssueAuthority`, `RequestRelease`, `ExecuteRefund` and `TransferApplications` are distinct commands. They share the same funding ACL and common operation protocol; they are not a local PaymentService rewrite.

Querying evidence must not silently capture funds or extend authorization. An adapter cannot implement a query using a mutating owner route without declaring that difference and reclassifying the application operation.

## Closing the check/use race

A recent coverage query alone does not guarantee funds will remain valid while capacity is committed and documents are created. The REFERENCE target requires an `IssueAuthority` bound to the exact OperationId, obligations, amount/currency/scope and document plan hash. It must be an owner-backed durable commitment that remains valid for that operation until finalize/release, or an explicitly certified equivalent time-window guarantee with defined clock/dispatch semantics. The baseline simulator uses the durable commitment form.

This is OWNER_REQUIRED, not an assertion that JetPay already has such an endpoint. BD-005 blocks production binding without it or an owner-approved equivalent. Do not add a made-up five-minute token that can expire while a document outcome is Unknown. Persist authority before document work. Recovered Confirmed documents use the ORIGINAL authority; do not require a new charge because a later read now shows an expired authorization.

Finalization after local Issue reports document/effect references to the owner. A failure to acknowledge finalization leaves a cleanup/reconciliation step; it does not undo an already issued ticket or permit a second charge. The backing authority must cover this state, otherwise that provider profile is not eligible for production Issue.

## Coverage and refund arithmetic

Order-local display may derive `CommercialObligationTotal`, `ConfirmedCashAppliedNet`, `GuaranteedCoverage`, `UncoveredObligation` and `PendingRefund`; each has a precise scope/currency and as-of revision. A displayed overapplication requires an explicit refund/disposition decision, not automatic payout.

Refund execution is capped separately by remaining tender/application value and active refund reservations. Commercial reversal limits do not prove cash is refundable. An AirPrice decision establishes customer economics; JetPay confirms which source application/tender can pay it. A goodwill amount above historic payment needs explicitly authorized payout funding, not an over-refund hack.

## Reference lifecycle

Obligation Established -> CoveredEvidence -> IssueAuthorityAcquired -> AppliedToDocuments; expired/released evidence remains history. No status is canonical payment truth. Unknown establish/release/refund keeps its durable operation and claim; retry does not reconstruct amount/scope from a now-changed Order. S3 provides API, durable simulator, read-back, explicit release and restart tests before S4 uses it.
