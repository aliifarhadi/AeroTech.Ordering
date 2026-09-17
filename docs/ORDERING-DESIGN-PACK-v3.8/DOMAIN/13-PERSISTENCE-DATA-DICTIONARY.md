# Persistence data dictionary and required constraints

## Schemas and common types

One initial SQL Server database, schema areas `Commercial`, `Fulfillment`, `Operations`, `Delivery`, `ReadModel`, `Messaging`, `Groups`, `Reference`. These are new target mappings, not old-row migration names. Use existing EF framework conventions where semantically compatible. Local IDs bigint (opaque on HTTP as strings); external refs nvarchar with explicit per-contract limits; timestamps datetimeoffset; SQL rowversion byte concurrency token. Money/rate capacities follow DOMAIN/03. JSON payload columns explicitly nvarchar(max) with schema version/validation; never rely on blanket string(256).

All root/current ownership keys include OwnerAirlineId where needed for integrity/security. No cascade deletes across monetary/document/operation history. Restrict deletes on accepted facts; protected payload lifecycle is separate. Supporting indexes are part of a slice, not deferred optimization.

| Table group | Important persisted fields | Mandatory constraint/index |
|---|---|---|
| Orders | ID/reference, owner/customer, sales context, root/parent, summary, totals, CV/FinancialSequence/OrderRevision/rowversion | Unique owner+reference; parent/root consistency; indexed customer+created/status |
| OrderPreparations | ID, caller/customer/owner, normalized source/digest, schema/profile, validity/assurance, consumed OrderId, rowversion | Unique consumption; digest immutable; index expiry/consumed; no open Order created yet |
| OrderItems | ID/current OrderId, kind, source/product/terms, original accepted snapshot, change/status | FK current Order; indexed current status; immutable accepted snapshot |
| OrderServices + typed detail tables | ID/current item/Order, version, type/profile, disposition | Exactly one detail per registered core type; unique current ownership; covered scope constraints |
| Beneficiaries/Coverage/Dependencies | service, traveler, segment/portion, kind/policy | Unique relation; no RequiresAirService cycle; required same-current-Order validation |
| Travelers/Contacts/Journeys/Segments | stable IDs, current ownership, protected refs, sold context and kinds | Source traveler binding unique per acceptance; no fake Surface air service |
| ItemServiceLinks/ServiceLineage/ItemLineage | occurrence OrderId, old/new refs/change, scope snapshot | Append-only, explicit many-to-many; historical refs not constrained to current owner |
| OrderChanges/PriceChangeSets | IDs, type/reason/actor, decision/context, sequence | Unique Order+ChangeId; unique Order+FinancialSequence; one set per monetary change |
| PricingLines | magnitude currencies, direction/effect/role, source occurrence, original line, provenance | Nonnegative magnitudes; valid matrix; FK original; monetary reversal ceiling under serialized commit |
| AllocationSets/Allocations | parent line/purpose/version/method/completeness and scope amounts | Unique line+purpose+version; complete/partial reconciliation; immutable supersession |
| FareConstructions/Groups/Units/Components/Bindings | source context, traveler groups, explicit coupling and current bindings | Current-binding uniqueness; historical construction immutable |
| FundingObligations/Applications/Evidence | exact scope/version/currency/value, movement IDs, source revisions/authority | Unique obligation+version; unique source movement; no double assignment/refund reservation |
| Reservations/ReservationMembers/CouplingGroups | profile, exact resource/Service refs, states, expiry/revision | Unique resource/member namespace binding; explicit coupling closure |
| Tickets/TicketCoupons/EMDs/EmdCoupons/Associations | document facts, financial/control aspects, original/current owner, values | Number namespace unique; active documentary slot uniqueness; coupon ordinal unique per document |
| StockNamespaces/Stocks/Allocations | range/format/state/next/version, operation+role+number | Nonoverlap under namespace serialization; number unique; operation+role unique |
| Receipts/ServicingOperations/Claims | scoped key/hash, plan, phase, versions, active flag | Unique receipt scope; filtered unique active claim per Order; operation/step key constraints |
| ExternalOperations/Attempts/Evidence | exact request, profile/key, dispatch markers, hashes and evidence | Unique parent+step+target; stable key; immutable request and evidence identities |
| WorkerLeases/PhaseCommits | lease/fence, CommitToken and phase | Conditional fenced update; unique CommitToken; no clock-only business claim release |
| Observations/OperationalFacets/Correlations | source event/key/aspect/epoch/revision, stable service lookup | Dedup source+event+row; versioned correlation; indexed unresolved conflicts |
| Outbox/Inbox | envelope/payload hash/stream ordinal, delivery lease/attempt, dedup consumer | EventId unique; owner+StreamKind+StreamId+EventOrdinal unique; inbox dedup and payload conflict |
| OrderDetails/Search indexes | owner/customer, projection JSON/schema, source OrderRevision | One row per Order/projection version; conditional rebuild replacement |
| GroupBlocks/NameSlots/Materializations | block capacity evidence, row refs, resulting Order | Per-block counters; unique group+client row/materialization ID |

## Database enforcement versus domain enforcement

Use DB unique/FK/check constraints for stable uniqueness, sign/valid enum and referential rules. Cross-row sums, coupling closure, range nonoverlap and reversal ceilings require a serialized/fenced transaction and domain policy; a unique range-start index does not prevent overlapping ranges. Use concurrency tests on the actual SQL Server provider, not EF InMemory/SQLite as substitute.

S1 introduces only its required commercial/preparation/receipt/money/projection/outbox tables. S2 adds operations/reservation infrastructure; later slices add their tables. The day-zero data dictionary reserves semantics, not a requirement to create empty tables for every future feature in B0.

## Migration gates

Fresh DB migration, previous-stage upgrade, constraints/index inspection, no implicit precision loss, no data reset and replay/read-model rebuild with the new schema. Destructive schema changes require a data migration and recovery plan. Historical event payloads carry explicit SchemaVersion; new code must read retained supported versions or upcast without changing their monetary meaning.
