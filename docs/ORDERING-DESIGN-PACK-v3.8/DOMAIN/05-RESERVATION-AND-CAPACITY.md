# Reservation, capacity evidence, coupling and expiry

## What Ordering stores

A FulfillmentReservation is a durable local association with an owner's resource, not a seat counter. It stores reservation ID, OrderId, provider/profile/contract, initial operation ID, external resource references, immutable requested scope and evidence history. Members identify ServiceId, resource key, capacity type, quantity/unit, traveler binding, owner reservation member ref, observed cabin/RBD and source version/validity.

The requested capacity quantity comes from the accepted fulfillment profile and owner resource contract. Baggage/seat product capacity is not always one air seat. An infant without a seat is not silently included/excluded merely by PTC. An allotment/block allocation is not another general-sale hold.

## Operation result versus resource state

A remote member effect outcome is Confirmed, Rejected, Pending or Unknown. Confirmed means the requested effect was authoritatively applied (or already applied under the same identity). Rejected means a certified definitive rejection with no effect for that member. HTTP status alone does not prove either.

Resource states are separately Held, Committed, Released, Expired, Waitlisted or Unknown. A successfully completed Reserve operation may later have an Expired resource. It remains historically successful; replaying its key cannot create another hold. A Confirmed Reserve is not a Committed resource and cannot satisfy final issuance.

Batch summary is derived: AllConfirmed, AllRejected, Mixed, Pending or Unknown, with counts and HasUnknown. Known partial effects are retained. Do not make Rejected mean no effect for an entire batch that contains confirmed members.

## Logical transitions

- No binding -> Reserve intent -> Held, Waitlisted, Rejected, Pending or Unknown evidence.
- Held -> Commit intent -> Committed, or definitive expired/rejected evidence; Unknown is unresolved commitment.
- Held -> Release intent -> Released/AlreadyReleased proof; Unknown keeps a blocked operation.
- Committed -> CancelCommitted/ReleaseCommitted only under the owner's explicit capacity-servicing contract. A hold DELETE is not assumed equivalent.
- Held -> Expired only from owner evidence or a certified owner expiry guarantee. Local time can make it ineligible without asserting capacity was physically released.
- Extend updates owner expiry/version only after confirmed owner outcome. TicketingDeadline and OfferExpiresAt do not change with it.

A rejected no-effect reserve can be retried as a NEW business operation with a new key after the original is terminal. An Unknown reserve can only be resumed/reconciled under its original identity. The same distinction applies to release and commit.

## Coupling

ReservationCouplingGroup explicitly identifies Independent, MarriedSegments or ProviderAtomicSet members and owner group ref. Issue/servicing scope must close over the required coupled group. One confirmed married segment cannot be independently issued when another required member is unknown/waitlisted/rejected.

Reference reserve completion policy is `AllRequiredOrRelease`: confirmed members are kept while unknown/pending members are reconciled; after a definitive rejection prevents completion, compensate only the known held members using distinct durable Release operations. Do not release while another commit/issue could be in flight. A separately specified `RetainPartial` profile may expose partial reservation, but requires explicit caller/policy acceptance and never gives full-scope Issue permission.

## Reserve algorithm

Resolve CommandReceipt first. For a new request, load scope/versions and validate active commercial services, complete capacity requirements, non-conflicting claim and no unresolved prior binding. Create operation/claim, planned member IDs, canonical request/body hashes and Pending local view in TX-A. Commit before calling IReservationPort.

Dispatch outside SQL with stable key/profile. Persist each result and verified refs in TX-B. Unknown/Pending keeps the claim and schedules authoritative read-back. AllConfirmed ends Reserve and releases the operation claim; resource bindings remain. On mixed terminal failure, execute the stated compensation plan, preserving every resource and effect record. The failure outcome cannot erase a held resource.

## Expiry and release are part of S2

S2 must include explicit release, reconciliation and local expiry eligibility tests; do not defer all cleanup to S5. A release command does not cancel the commercial Order. It removes/resolves reservation evidence and changes reserve/issue capabilities. A later reserve starts a new operation only after the prior binding is conclusively released/expired/rejected.

Expiry worker identifies candidate due holds locally and reconciles with the owner as required. It does not delete reservation history, set the whole Order Expired or override an issuance operation protected by a committed resource/claim. Race tests: commit vs expire, release vs commit, response loss vs retry, expired lease vs another worker.

## Read-back contract

ReadOperation(key) returns the outcome of that effect; ReadReservation(resourceRef) returns current resource state. They answer different questions. Authoritative Absent must include owner identity, lookup scope and completeness/retention assurance. A 404 from an eventually consistent replica or a key beyond retention is not no-effect proof. Same-key POST recovery is a mutation replay, not a read query; enable it only when owner idempotency and request identity are certified.
