# JetPay - funding and payout capability requirements

No final authoritative JetPay coverage wire contract is supplied. The old audit references a local JetPay worktree and divergent proposals; neither establishes an owner-approved production API. This pack does not create fictitious JetPay endpoint URLs.

Application-owned `IFundingCoveragePort` has typed commands EstablishCoverage, AcquireIssueAuthority, FinalizeIssueAuthority, RequestRelease, AcquireRefundAuthority, ExecuteRefund, AcquireExchangeFundingAuthority, AcquireApplicationTransfer/FinalizeApplicationTransfer, and queries ReadCoverage/ReadOperation. See DOMAIN/06 and DOMAIN/09 for exact scope/phase semantics. One capability family, no local Payment aggregate and no second coverage service rail.

Required issue authority binds operation, obligation IDs/versions, amount/currency, scope/document-plan hash and durable owner commitment/equivalent guarantee. Required refund authority binds original application/tender movement, refundable capacity and accepted refund plan. Required transfer binds balanced old/new scopes and unique resulting applications. Read operations cannot silently authorize/capture/refund.

Each command has its own stable external effect ID/key and exact request. Owner callbacks/events carry original operation/resource refs and source revision, pass inbox dedup and join the same evidence path. A success redirect or transport acknowledgment is not money evidence. Pending/Unknown supports recovery, not a 409 business denial masquerading as insufficient funds.

BD-005 blocks real binding until the owner approves equivalent semantics, wire mappings, key retention, conditional scope application and read-back. The simulator is a target-contract implementation with persistent funds/application/authority/refund/transfer state; it is not a fake assertion that JetPay already offers those operations. Mixed funding, residual value and zero-delta exchange tests distinguish cash from guarantee and prevent duplicate use.


AcquireExchangeFundingAuthority is an owner-side mutation that binds the accepted exchange operation to usable old application value, exact additional collection, transfer authority, residual disposition and a valid issue authority for the new obligation scope. Return each authority/reference explicitly; do not infer a composite guarantee from unrelated numeric coverage queries. Reference JetPay state performs the reserved-value conservation atomically in its own store. An actual owner without this capability or a certified semantically equivalent protocol remains BD-005. Finalization and release use the original authority references and immutable scope.
