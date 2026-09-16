# Persistent target-contract simulators

## Deployment shape

Implement semantic behavior in `Providers.Deterministic` with an isolated provider-owned persistence context. Add `tools/AeroTech.Ordering.SimulatorHost` to expose it over HTTP for E2E/fault tests. Each simulated owner has separate durable state/namespace; no shared OrderingDbContext/transaction and no direct write to Order tables. A single test host may contain several owner modules, but their operation/resource stores and identities remain distinct.

Ordering Application consumes the same port regardless of provider selection. The adapter translates simulator transport into the same normalized result type. Shared contract suites exercise port behavior; additional real-wire tests exercise actual owner DTOs/routes. A successful simulator does not certify a real owner.

## Required state

Offer: immutable bound candidates and validity/ownership evidence. Pricing: immutable decisions and acceptance/expiry/scope rules. Capacity: per-resource counts, held/committed/released/expired/waitlisted members, operation keys, atomic groups and block allocations. Funding: application movements, guarantee vs cash, issue/refund authority reservations, releases/refunds/transfers. Document external-profile module: numbers/documents/coupons/control and group effects. Ancillary: versioned product/catalog and supplier bookings. Delivery/Disruption: versioned facts/control/authorized plans. Ledger: received envelope IDs and economic-reference associations only.

Simulated state survives host restart, not merely Ordering restart. Same key+same payload returns the same effect; changed payload conflicts. Current resource read may change with expiry, but historical effect identity does not. Implement concurrent duplicate requests and resource capacity contention under persistent constraints.

## Fault scenarios

Before dispatch; reject with certified no effect; Pending with later completion; commit then drop response; delay beyond client deadline; partial member completion; missing/unexpected member; wrong amount/currency/scope/source version; stale or incomplete read-back; key retention elapsed; resource expires during operation; conflict between two workers; broker down/duplicate event; external issuer partial exchange; funding authority acquired but acknowledgment lost.

Fault injection control is available ONLY on the simulator host's local/test administration surface. Production API requests cannot select a fake outcome. Save fixtures and fault scenario IDs in test evidence. Any clock advancement is scoped to a test owner profile and explicit; never rewrites a real owner's timestamps.

## Contract assertions

One effect per key/payload/target; no over-reservation; no overspending/refund; no fabricated success; recovery side-effect-free unless explicitly invoking certified mutation replay; partial/unknown retained; immutable monetary/issue history; correct target counts; source metadata preserved; profile swap cannot adopt old resources; double restart preserves identity and outcome. These are acceptance specifications to implement and run, not tests executed by this design-pack generator.
