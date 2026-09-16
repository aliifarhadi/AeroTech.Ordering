# ETKT, EMD, document authority and numbering

## Two authority profiles, one canonical issuance use case

`LOCAL-AIRLINE` is the reference implementation profile: Ordering is the domain authority for ETKT/EMD and uses locally configured DocumentStock. This is a target implementation decision, not proof of production airline issuing authority. Real carrier stock/profile authorization is BD-006.

`EXTERNAL` means an identified owner issues/transitions the document; Ordering stores synchronized authoritative evidence. It requires certified issue/read-back/void/control/servicing contracts. A simulator for that profile is a separate owner process/store. Do not force the local profile through a pretend remote issuer, and do not make an external document local merely because the response timed out.

Both are coordinated by Application.IssueOrder with explicit authority in the document plan. Local domain factories are pure and persist through the enclosing session. External ACL ports only dispatch/recover effects; they never create local entities directly. There is no second Issue handler per API surface.

## ElectronicTicket

Root fields: TicketId, OwnerAirlineId, OriginalOrderId, CurrentServicingOrderId, TravelerId, Authority, provider/profile when external, DocumentNumber/namespace, IssuerCarrier, IssuingOffice, IssuedAt, validity and void-deadline evidence, protected PassengerSnapshotRef, immutable IssuanceAmounts/price links, ExchangeChainRef, DocumentVersion, RowVersion.

Coupon fields: CouponId, TicketId, ordinal, immutable issued service/segment snapshot, current service binding history, FareBasisSnapshot when provided, immutable issuance value, financial disposition, control aspect, external status/version and source provenance.

Financial disposition: Open, Used, Void, Exchanged, Refunded, Suspended. Control: Local, External, ReleasePending, Unknown with holder/token/version. Pending servicing restrictions belong to the active operation/claim; they are not evidence of refund/void. Boarded, CheckedIn, Offloaded and NoShow are delivery aspects, not these financial states.

Local transitions: new -> Open on issued commit; Open -> Void/Exchanged/Refunded under the matching authorized operation; Open -> Used only from certified consumption authority; Open <-> Suspended only by explicit supported control/issuer decision. Terminal Void/Exchanged/Refunded/Used is not arbitrarily reopened. Partially used documents operate on explicit eligible coupon scope. External transitions apply confirmed owner facts and retain source codes even when unsupported/conflicting.

## EMD

ElectronicMiscDocument is a separate root: EmdId, EmdType (A/S), issuer/office/authority/profile, RFIC, namespace/number, issue validity, immutable amounts, protected passenger association where required, coupons and document version.

Each EmdCoupon has exactly one purpose:
- Service: stable ServiceId and sold coverage/quantity/value association.
- Fee: PricingLineId/accepted charge reference, no fake air service.
- Deposit or ResidualValue: canonical external value/application reference; EMD is documentary evidence, NOT a wallet balance or new payment.

EMD-A associates to a specific eligible ticket coupon, not only the ticket header. Association changes are immutable entries with old/new coupon binding and source decision. EMD-S fee/deposit/residual does not require an invented flight. Validate RFIC/RFISC, permitted EMD type, association, currency and coupon limits through the certified issuer profile. Multiple coverage portions cannot double-document the same sold quantity.

## DocumentStock

Stock namespace includes owner/issuer/document type and the certified format namespace, not an assumed globally unique raw number. Fields: StockId, NamespaceId, office scope, prefix, serial width, check-digit profile, range start/end, next serial, Active/Suspended/Exhausted/Closed, version. No universal ticket check-digit algorithm or production range is invented here.

Create/update stock serializes on the namespace and rejects overlapping ranges atomically. Allocation serializes on the active stock row and has a unique `(NamespaceId, Number)` constraint plus `(OperationId, DocumentRole)` replay key. Number states Reserved, Issued, Retired. An externally dispatched number is never recycled after Unknown. Unused local-only rolled-back allocation did not commit; committed abandoned allocations are retired, not silently reused.

External owner-owned stock: accept its returned number and persist its namespace/reference; do not allocate a competing local number. External client-assigned stock: persist the allocation in TX-A before dispatch and reuse it on read-back/replay.

## Local Issue transaction

After required capacity is committed and funding IssueAuthority is durable, load/fence the same operation/claim and revalidate document eligibility and local fact versions. In ONE SQL transaction allocate all numbers for the requested local document set, create ETKT/EMD/coupons and associations, mark allocations Issued, append document facts, update OrderRevision and OrderDetails and insert outbox events. CommercialVersion does not increment unless a separate accepted commercial change occurs.

A crash before commit leaves no partially issued local set. A lost SQL commit response is resolved by operation/commit-token read-back before allocating again. If stock is exhausted after remote prerequisites, do not pretend Issue succeeded; keep resources tracked and either resolve stock or run explicit compensation before any document commit.

## External Issue

Persist planned document roles, number policy, exact request and issue authority. Dispatch outside SQL. Record each confirmed document as evidence even if other members are Unknown; local views must not hide actually issued documents. An all-or-nothing UX does not create a distributed transaction. Reconcile unresolved roles under their original keys. Never compensate or allocate replacement documents until the original outcome is conclusive. Late issue evidence after a client timeout is handled by the same operation.

## Entry gate versus final gate

`CanStartIssue`: active eligible scope, complete required facts, no conflicting operation, recoverable reservation/funding prerequisites. A valid Held resource is sufficient to start the commit workflow.

`CanCommitDocuments`: every REQUIRED coupled capacity resource is Committed (or explicitly exempt by profile), exact durable funding authority confirmed (or explicit free/zero obligation exemption), document data/control/stock/issuer profile eligible, no conflicting late evidence. Hold-only/Unknown/funding-query-only does not satisfy it. Same-key replay bypasses NEW-operation gate and resumes existing evidence.
