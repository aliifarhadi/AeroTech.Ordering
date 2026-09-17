# Transactions, persistence and single local projection

## Local atomic units

1. Create: consumed preparation + Order graph + commercial change/price set + receipt result + OrderDetails + outbox in ONE SQL transaction.
2. External-operation prepare: receipt + operation + active claim + exact normalized request + stable IDs/keys + pending projection, then commit. No remote effect before this commit.
3. Evidence application: append verified evidence + resource projection + operation phase + OrderRevision + local OrderDetails + any new fact/outbox, ONE transaction.
4. Commercial pivot: change + immutable money deltas + item/service lineage + required local document transitions + obligation changes + projection + receipt/operation progress + outbox, ONE transaction.
5. Inbound fact: dedup marker + durable normalized observation OR accepted operation request + projection/outbox, ONE transaction. Acknowledgment follows durable acceptance, not remote completion.
6. Split: source and child local commercial changes and transfer facts share ONE database transaction. All remote steps are outside; multi-order business graph remains bounded to this explicit exception.

`SaveChanges` is not the use-case transaction boundary by itself. Several saves can occur inside one explicitly shared transaction. Multiple DbContexts require the same connection/transaction; baseline implementation uses one command session. Retryable EF execution strategies may retry a DB-only phase, never a delegate containing remote mutation. Disable MARS for tests relying on savepoints. These technical constraints follow Microsoft EF documentation; see REVIEW/04-EXTERNAL-REFERENCES.

## Commit uncertainty

Allocate stable planned object/event/commit identifiers before a phase retry. Persist a unique `CommitToken`/receipt association with the phase. If connection loss makes commit outcome uncertain, open a fresh session and read that token/receipt. Do not rerun a stock allocation or emit new event IDs just because the response was lost. If read-back is unavailable, remain unresolved and retry read-back. This is local SQL uncertainty, distinct from remote provider uncertainty.

## Command loading and concurrency

An Order is the boundary for accepting commercial mutations, not an invitation to eagerly load all history. `LoadForMutation` loads complete current commercial scope, dependent services, active construction references and required evidence. A partial-scope loader must explicitly report what it loaded. It cannot derive whole-order summary from partial children.

Every command that changes a child relevant to eligibility also conditionally updates the owning Order revision row. SQL rowversion protects the root update; child changes alone do not magically modify root rowversion. One short transaction validates the current claim, fact-version vector and clock instant before writing. Lock acquisition order: owner/order IDs ascending, then document/stock namespace IDs. Unique constraints remain the ultimate guards.

## Read model

`OrderDetails` contains an immutable-versioned JSON projection plus searchable columns/index tables. It includes current commercial facts, historical links, amounts/obligations, per-service capacity/funding/document/delivery facets, operations/reconciliation, source timestamps and authority labels. It never embeds secrets or unrestricted raw owner payloads.

Only `OrderProjector` in Synchronizer writes it. It builds from canonical local snapshots and enlists in the command transaction. Domain owns deterministic business summaries; projector maps them to a view, never re-prices. Query reads the local projection through a read-only connection. UI capability evaluation may apply the LOCAL clock to persisted expiry facts and mark stale/unknown evidence; it makes zero owner calls. Return `projectedAt`, `orderRevision`, `capabilitiesEvaluatedAt` and blocking reasons.

A GET after a successful local command returns at least its committed revision in the primary local store. Any optional replica that cannot satisfy `minRevision` must return retryable lag, not silently stale success. Read replicas are not required in the baseline.

## Rebuild and schema evolution

`RebuildOrderProjection(orderId)` is an internal admin command. Read a consistent local canonical snapshot with its revision, build using the SAME projector, and conditionally replace only if source revision still matches. Concurrent newer projection wins; rebuild retries from a fresh snapshot. It emits no sale/refund events, calls no provider and increments no CommercialVersion. ProjectionSchemaVersion is separate from OrderRevision.

Global rebuild pages indexed OrderIds and runs per Order, resumably. Do not truncate live projections and expose missing data. Use shadow projection version + atomic activation for incompatible view schemas; enforce a single active writer path. API view additions are backward-compatible; removing or reinterpreting a field requires API versioning.

## Outbox and Inbox

Outbox transport is at-least-once. Event identity/payload remain unchanged across publication retries. Publisher leases with fencing protect dispatcher bookkeeping; duplicate sends are allowed and consumers dedup by event identity. Partition/order by OrderId/EventOrdinal where supported; consumers must detect gaps/out-of-order cases rather than assuming a transport-wide total order.

Inbox dedup key is `(OwnerAirlineId, SourceSystem, EventId, ConsumerName)`, with payload hash conflict detection. Individual observations in a batch additionally use ObservationKey. A no-MessageId fact is rejected/quarantined according to ingress contract, not processed without dedup. A handler with external work accepts a durable operation locally then acknowledges; recovery continues outside the broker transaction. Never keep a broker delivery open during a long remote saga.
