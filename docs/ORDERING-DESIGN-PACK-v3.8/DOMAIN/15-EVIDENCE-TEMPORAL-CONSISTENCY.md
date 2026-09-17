# Temporal evidence and limits of distributed guarantees

## No instantaneous global truth assumption

An Ordering SQL transaction cannot freeze FlightFlow, JetPay and DCS simultaneously. The design requires operation-bound authorizations or certified conditional owner effects at irreversible boundaries, not a claim that a prior query makes remote state immutable forever. Every observation has source identity, revision, observed/occurred time and assurance. A local claim fences competing Ordering commands, not real-world flight operations or external cash/control changes.

Before Issue/Refund/Exchange pivots, validate the relevant locally recorded version vector and the durable owner authorities. Record the exact inputs used. If a certified authority has a validity window rather than durable reservation, the owner contract must define the bounded commit rule and recovery for expiry during dispatch. No Agent chooses a convenient clock margin and calls it a guarantee.

## Facts after a pivot

A flight cancellation after valid issuance is a new operational/disruption fact, not proof the original issuance was invalid. A chargeback or coverage revocation after issue becomes external funding/dispute evidence and a reconciliation obligation; Ordering does not silently erase the ticket, rewrite the sale or generate a cash refund. The owner-authorized remedy uses an explicit new servicing operation. A provider correction must identify the earlier fact/authority being superseded; a newer received timestamp alone cannot reverse history.

An in-flight operation that has an actual irreversible effect must keep that effect even when a later local fact makes its intended plan invalid. Record `NeedsReconciliation`, preserve committed documents/value and block unsafe dependent actions. Do not discard the result as stale merely to make optimistic concurrency pass. Recovery can require a new authorized corrective operation; that operation has new identity and explicit lineage to the original, not a retry with a changed request.

## Current state versus effect history

ReadOperation proves what happened under an effect key. ReadResource proves current resource state. Neither substitutes for the other. A HoldCreated confirmation does not prove the hold still exists; an Expired resource does not prove a Commit never happened. Evidence from different resource identities or profile versions cannot be combined into a synthetic confirmation.

For source sequence gaps, epoch resets and competing corrections, use the certified source contract. If ordering cannot be established, retain all observations and suspend the affected current-state interpretation. Do not sort every source by ReceivedAt and select the latest arrival. Reconciliation status is a separate dimension from commercial validity.

## Extending to unimplemented financial/operational remedies

Chargeback dispute execution, involuntary compensation calculations, interline settlement and legal retention are owned outside Ordering. The reference model can retain their normalized source evidence without inventing their process. Enabling an executable remedy requires a typed command/authority, source contract, monetary treatment and regression tests. Unsupported execution returns a named capability/decision error; ingesting a source fact is not permission to execute a remedy.
