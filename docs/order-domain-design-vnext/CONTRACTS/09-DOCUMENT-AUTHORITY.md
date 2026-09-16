# Document-authority strategy and external capability contract

The Local profile uses domain factories plus OrderingDbContext under Application coordination; no external issuance service is required. It still requires approved issuer/stock/coupon/control configuration for production (BD-006).

For External authority use `IDocumentIssuancePort` for ticket issuance, `IEmdIssuancePort` for EMD issuance, and `IDocumentServicingPort` for Void, RedeemForRefund, Exchange, Revalidate, ReassociateEmd and document/effect reads. CONTRACTS/11 and SPEC/ports.json give the exact canonical methods. Separate ETKT/EMD invariants remain. These are required target capabilities, not asserted production owner endpoints.

Issue request freezes traveler protected snapshot reference, service/coupon roles and sequence, immutable issue amounts/price refs, issuer/office/profile, funding authority and committed-capacity evidence references, number authority (LocalAssigned or ProviderAssigned), and exact idempotency identity. A provider cannot replace service scope/price silently; mismatch becomes reconciliation.

External result identifies document namespace/number, actual coupon mapping, issue instant/validity, amounts/currency, owner resource refs/status/revision and control authority. No blind creation from a 202/204. For partial groups persist every confirmed document with original role/key; remaining roles are recovered, not recreated with new numbers.

Void/refund/exchange/revalidation require expected control/source versions and exact allowed scope. A provider that cannot explain partial group outcomes or supply authoritative read-back is not certified for those capabilities. Declare unsupported separately from technical outage. External stock-owned providers return numbers; local stock is not also consumed.

Local and external authority is a semantic document property and stays stable for each document. Simulator/Real is an implementation binding and must not appear as a domain business branch.


RedeemForRefund is documentary disposition, not tender payout. If an actual issuer couples document redemption with payment execution, that profile stays disabled until an explicit composite owner mapping proves value is executed once; do not also execute a duplicate JetPay refund. EMD reassociation is a versioned documentary effect with history and control proof, not a mutable foreign-key shortcut.
