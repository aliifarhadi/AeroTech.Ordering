# Durable operation protocol, claims, fencing and recovery

## Records and identities

CommandReceipt: owner, caller/customer idempotency scope, canonical command kind, client key, canonicalization version, protected request/hash, stable result/OperationId/OrderId, Accepted/InProgress/Succeeded/Rejected status and timestamps. Unique key is `(OwnerAirlineId, CallerScope, CommandKind, IdempotencyKey)`; caller scope is stable across authorized retries, not a transient access token. Cross-surface duplicates can share it when they represent the same client operation and principal scope.

ServicingOperation: OperationId, typed Kind, OrderId, initiating actor, frozen target IDs and dependency closure, base fact-version vector, immutable accepted decision/plan hash, current phase/outcome, planned ChangeId, ClaimId and external steps. No generic JSON-programmable state machine. Each supported operation has a versioned explicit coordinator.

ExternalOperation: ExternalOperationId, parent OperationId, StepKind/TargetKey, immutable provider profile/contract, stable key, exact normalized semantic request and wire-body hash, optional encrypted wire body, dispatch/attempt history, latest verified evidence, recovery schedule and phase. `UNIQUE(OperationId, StepKind, TargetKey)` prevents accidentally creating the same step twice.

OperationOrderClaim: one active exclusive claim per `(OwnerAirlineId, OrderId)` in baseline. It has semantic disposition, operation owner and fencing generation. WorkerLease: worker ID, lease expiry and incrementing fencing token. Lease expiry permits a different worker to continue the SAME operation; it does not release the business claim or prove no remote effect.

## Receipt first

Authenticate/scope-check, canonicalize input and look up/acquire the receipt before evaluating eligibility for a NEW operation. Same key+same hash returns/resumes the existing result; same key+different hash returns 409. Existing Unknown must not fail as AlreadyReserved/Ticketed merely because its own pending evidence exists. A terminal rejected no-effect operation can be followed by a new user intent/key, not silently repurposed.

Canonicalization version is persisted. Object property ordering is normalized; semantic sequence fields are preserved; unordered sets are sorted by stable ID; amounts are exact decimal values; opaque external IDs are case-sensitive unless owner says otherwise. Transport headers/trace IDs are excluded, but accepted digest, scope, amounts, traveler data binding and expected versions are included. Secrets are not logged. Do not change fingerprint rules for old receipts after a serializer upgrade.

## Phases and outcomes

Coordinator phases: Prepared, Executing, AwaitingExternal, CommercialApplied (for a defined pivot), Completing, Completed, Rejected, Compensating, NeedsReconciliation. Completed/Rejected are terminal only when all required effects/dispositions are known. NeedsReconciliation is NOT evidence of failure and retains restrictions.

Before each remote dispatch commit the exact intent and a dispatch-attempt marker. A crash after that marker but before sending is conservatively uncertain: read back first, then resend the SAME request/key only when permitted. There is no reliable local bit proving a network request reached the remote process.

Single-effect outcomes: Confirmed, RejectedNoEffect (represented as Rejected with guaranteed no-effect evidence), Pending, Unknown. Batch outcomes are per member with derived summary. HTTP 5xx/timeout/cancellation after dispatch normally means Unknown; a known pre-dispatch failure can remain NotDispatched. An HTTP 4xx is not automatically safe if the owner can have partial effects.

## Three boundaries

TX-A creates/updates receipt, operation/claim, planned IDs, exact request and Pending projection; COMMIT. Then HTTP/broker mutation outside SQL. TX-B appends evidence, verifies key/profile/scope/version/currency/counts, applies local resource facts and updates operation/projection; COMMIT. A later commercial TX-C performs the defined pivot with all facts and outbox atomically. Some use cases finish in TX-B; none assumes atomicity across an owner and SQL.

Cancellation of the incoming HTTP request after TX-A affects waiting for the response, not durable operation existence. A worker continues by policy. Return 202 with operation URL when the response budget expires; do not erase the receipt or report false rejection.

## Worker race and late results

Only the current fenced lease can advance coordinator state. Remote idempotency protects duplicate dispatch across workers; local lease alone cannot prevent a paused worker from resuming. Every state write conditions on operation version/fencing token. A stale worker may append a uniquely identified immutable evidence observation for review, but cannot overwrite a newer decision. The current worker validates/applies it. Conflicting owner evidence becomes NeedsReconciliation, not last-write-wins success.

Active business claims survive restarts and lease expiry. They are released only after terminal effect knowledge, completed compensation, or an explicitly validated authoritative resolution. Limit automatic retries with backoff; exhaustion raises NeedsReconciliation and an alert, never ConvertsUnknownToFailed.

## Recovery policy

ReadOperation answers whether THIS effect happened. ReadResource answers current resource state. Prefer side-effect-free authoritative lookup. A negative lookup is actionable only with complete lookup scope and retention/consistency guarantee. Unknown cannot be converted to no-effect from an eventually consistent 404.

Mutation replay uses the exact original key/body/profile and only certified idempotency semantics. Do not rebuild from current Order or change target sets. Compensation is another durable operation with a causation link, never changing the original request into a release. Do not release potentially issued/committed resources until conflicting effects are resolved.

Provider key retention must outlive unresolved operation recovery. If the owner has forgotten the key, forbid blind resend and escalate to authoritative resource reconciliation. Local receipt retention cannot be shorter than replay/recovery obligations. Timers, owner TTL and worker lease are independent.

## Reconciliation surface

Expose durable timeline, original intent hash, attempts, owner evidence, affected scope and permissible next actions. Admin resolution requires typed evidence and reason/authority. It cannot rewrite source amount or set arbitrary final status. Record resolution as append-only, re-evaluate invariants and emit corrective facts when required. Recovery itself does not increment CommercialVersion unless it applies a previously unapplied commercial pivot.

## Required crash points

Before/after TX-A; after dispatch marker before send; after owner commits before response; after response before TX-B; after TX-B before pivot; after SQL commit before HTTP response; after local pivot before cleanup; after broker publish before delivered marker; lease expiry while first worker is paused. Restart BOTH Ordering and simulator stores. For every point assert one intended owner effect, no lost evidence and no duplicate commercial version/document number/event identity.
