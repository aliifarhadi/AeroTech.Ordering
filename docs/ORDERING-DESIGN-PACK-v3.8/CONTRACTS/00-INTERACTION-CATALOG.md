# Service interaction catalog

Every record materializes all required dimensions. OWNER_REQUIRED is a target requirement, not an assertion that a production wire endpoint exists. Operation identities cannot be reused across distinct effects even when they share transport.

## IX-01 - PrepareOrderFromOffer

| Dimension | Contract |
|---|---|
| caller | Authorized sale caller |
| owner | Ordering |
| endpointSurface | /service / backoffice / otapanel / ota/v1 |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | Ordering local SQL transaction; external owners own only their separate transactions |
| idempotency | Owner + authorized customer/caller scope + operation kind + Idempotency-Key |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | CommandReceipt returns stored preparation; retry before commit may re-query before acceptance |
| ttlExpiryOwner | Owner validity preserved; local preparation retention is not offer validity |
| readModelEffect | Preparation view only; no Order |
| wireBinding | POST order-preparations; Ordering-owned target route |
| authorityStatus | TARGET_DECISION |

## IX-02 - Resolve candidate before acceptance

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | AirOffer |
| endpointSurface | /service/ |
| semanticType | Query |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | AirOffer query/calculation; Ordering captures candidate in a later local TX |
| idempotency | Query has no sale effect; local preparation receipt binds the returned candidate |
| timeoutSemantics | Read timeout is unavailable evidence; do not mutate commercial facts. |
| retrySemantics | Bounded read retry with original lookup scope and deadline. |
| unknownSemantics | Unavailable/Absent without authoritative scope and retention proof cannot satisfy an eligibility gate. |
| authoritativeRecovery | Repeat side-effect-free owner query using exact source identity/revision. |
| ttlExpiryOwner | AirOffer offer validity; AirPrice price validity; neither inferred from PricedAt |
| readModelEffect | None until captured candidate is explicitly accepted into an Order |
| wireBinding | OBSERVED POST /Service/v1/FlightOffers/Details; may reprice |
| authorityStatus | OBSERVED_SOURCE |

## IX-03 - CreateOrderFromOffer

| Dimension | Contract |
|---|---|
| caller | Authorized sale caller |
| owner | Ordering |
| endpointSurface | /service / backoffice / otapanel / ota/v1 |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | Ordering local SQL transaction; external owners own only their separate transactions |
| idempotency | CommandReceipt plus unique consumed PreparationId |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Local receipt/preparation-consumption read-back; no remote call on Create or replay |
| ttlExpiryOwner | Consume source validity under accepted profile; no generic Order TTL |
| readModelEffect | Atomic OrderDetails with immutable sold snapshot |
| wireBinding | POST orders/from-offer |
| authorityStatus | TARGET_DECISION |

## IX-04 - GetOrder/GetOperation

| Dimension | Contract |
|---|---|
| caller | Authorized reader |
| owner | Ordering |
| endpointSurface | /service / backoffice / otapanel / ota/v1 |
| semanticType | Query |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | Ordering read-only local SQL snapshot |
| idempotency | Read-only; authorize before returning cached data |
| timeoutSemantics | Read timeout is unavailable evidence; do not mutate commercial facts. |
| retrySemantics | Bounded read retry with original lookup scope and deadline. |
| unknownSemantics | Unavailable/Absent without authoritative scope and retention proof cannot satisfy an eligibility gate. |
| authoritativeRecovery | Repeat local read; never fan out |
| ttlExpiryOwner | Local clock can make eligibility stale/expired; it does not declare owner release |
| readModelEffect | No write; returns local projection/as-of revisions |
| wireBinding | GET orders/{id}; GET operations/{id} |
| authorityStatus | TARGET_DECISION |

## IX-10 - QuoteAncillary

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | AirPrice |
| endpointSurface | /service/ |
| semanticType | Query |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | AirPrice decision; Ordering stores immutable quote separately |
| idempotency | Decision ID/version/digest binds scope and inputs; repeated lookup must not silently replace accepted decision |
| timeoutSemantics | Read timeout is unavailable evidence; do not mutate commercial facts. |
| retrySemantics | Bounded read retry with original lookup scope and deadline. |
| unknownSemantics | Unavailable/Absent without authoritative scope and retention proof cannot satisfy an eligibility gate. |
| authoritativeRecovery | Repeat side-effect-free owner query using exact source identity/revision. |
| ttlExpiryOwner | AirPrice decision validity and accepted historical pricing context |
| readModelEffect | Prepared decision or later append-only commercial facts; never live GetOrder fanout |
| wireBinding | Target capability; exact complete decision route not certified |
| authorityStatus | OWNER_REQUIRED |

## IX-11 - QuoteCancellation

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | AirPrice |
| endpointSurface | /service/ |
| semanticType | Query |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | AirPrice decision; Ordering stores immutable quote separately |
| idempotency | Decision ID/version/digest binds scope and inputs; repeated lookup must not silently replace accepted decision |
| timeoutSemantics | Read timeout is unavailable evidence; do not mutate commercial facts. |
| retrySemantics | Bounded read retry with original lookup scope and deadline. |
| unknownSemantics | Unavailable/Absent without authoritative scope and retention proof cannot satisfy an eligibility gate. |
| authoritativeRecovery | Repeat side-effect-free owner query using exact source identity/revision. |
| ttlExpiryOwner | AirPrice decision validity and accepted historical pricing context |
| readModelEffect | Prepared decision or later append-only commercial facts; never live GetOrder fanout |
| wireBinding | Target capability; exact complete decision route not certified |
| authorityStatus | OWNER_REQUIRED |

## IX-12 - QuoteRefund

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | AirPrice |
| endpointSurface | /service/ |
| semanticType | Query |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | AirPrice decision; Ordering stores immutable quote separately |
| idempotency | Decision ID/version/digest binds scope and inputs; repeated lookup must not silently replace accepted decision |
| timeoutSemantics | Read timeout is unavailable evidence; do not mutate commercial facts. |
| retrySemantics | Bounded read retry with original lookup scope and deadline. |
| unknownSemantics | Unavailable/Absent without authoritative scope and retention proof cannot satisfy an eligibility gate. |
| authoritativeRecovery | Repeat side-effect-free owner query using exact source identity/revision. |
| ttlExpiryOwner | AirPrice decision validity and accepted historical pricing context |
| readModelEffect | Prepared decision or later append-only commercial facts; never live GetOrder fanout |
| wireBinding | Target capability; exact complete decision route not certified |
| authorityStatus | OWNER_REQUIRED |

## IX-13 - QuoteExchange

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | AirPrice |
| endpointSurface | /service/ |
| semanticType | Query |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | AirPrice decision; Ordering stores immutable quote separately |
| idempotency | Decision ID/version/digest binds scope and inputs; repeated lookup must not silently replace accepted decision |
| timeoutSemantics | Read timeout is unavailable evidence; do not mutate commercial facts. |
| retrySemantics | Bounded read retry with original lookup scope and deadline. |
| unknownSemantics | Unavailable/Absent without authoritative scope and retention proof cannot satisfy an eligibility gate. |
| authoritativeRecovery | Repeat side-effect-free owner query using exact source identity/revision. |
| ttlExpiryOwner | AirPrice decision validity and accepted historical pricing context |
| readModelEffect | Prepared decision or later append-only commercial facts; never live GetOrder fanout |
| wireBinding | Target capability; exact complete decision route not certified |
| authorityStatus | OWNER_REQUIRED |

## IX-14 - ValidateHistoricalTicketing

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | AirPrice |
| endpointSurface | /service/ |
| semanticType | Query |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | AirPrice decision; Ordering stores immutable quote separately |
| idempotency | Decision ID/version/digest binds scope and inputs; repeated lookup must not silently replace accepted decision |
| timeoutSemantics | Read timeout is unavailable evidence; do not mutate commercial facts. |
| retrySemantics | Bounded read retry with original lookup scope and deadline. |
| unknownSemantics | Unavailable/Absent without authoritative scope and retention proof cannot satisfy an eligibility gate. |
| authoritativeRecovery | Repeat side-effect-free owner query using exact source identity/revision. |
| ttlExpiryOwner | AirPrice decision validity and accepted historical pricing context |
| readModelEffect | Prepared decision or later append-only commercial facts; never live GetOrder fanout |
| wireBinding | Target capability; exact complete decision route not certified |
| authorityStatus | OWNER_REQUIRED |

## IX-15 - QuotePartition

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | AirPrice |
| endpointSurface | /service/ |
| semanticType | Query |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | AirPrice decision; Ordering stores immutable quote separately |
| idempotency | Decision ID/version/digest binds scope and inputs; repeated lookup must not silently replace accepted decision |
| timeoutSemantics | Read timeout is unavailable evidence; do not mutate commercial facts. |
| retrySemantics | Bounded read retry with original lookup scope and deadline. |
| unknownSemantics | Unavailable/Absent without authoritative scope and retention proof cannot satisfy an eligibility gate. |
| authoritativeRecovery | Repeat side-effect-free owner query using exact source identity/revision. |
| ttlExpiryOwner | AirPrice decision validity and accepted historical pricing context |
| readModelEffect | Prepared decision or later append-only commercial facts; never live GetOrder fanout |
| wireBinding | Target capability; exact complete decision route not certified |
| authorityStatus | OWNER_REQUIRED |

## IX-20 - ReserveCapacity

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | FlightFlow |
| endpointSurface | /service/ |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | FlightFlow capacity TX; Ordering intent/evidence TXs separate |
| idempotency | Provider profile + stable effect key + immutable exact member scope/hash |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | FlightFlow returned HoldExpiresAt/resource lifecycle; requested expiry is not a local authority |
| readModelEffect | Per-service capacity evidence/coupling/operation facet |
| wireBinding | Observed routes and missing guarantees in CONTRACTS/04; no target production URI invented |
| authorityStatus | OWNER_REQUIRED |

## IX-21 - CommitCapacity

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | FlightFlow |
| endpointSurface | /service/ |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | FlightFlow capacity TX; Ordering intent/evidence TXs separate |
| idempotency | Provider profile + stable effect key + immutable exact member scope/hash |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | FlightFlow returned HoldExpiresAt/resource lifecycle; requested expiry is not a local authority |
| readModelEffect | Per-service capacity evidence/coupling/operation facet |
| wireBinding | Observed routes and missing guarantees in CONTRACTS/04; no target production URI invented |
| authorityStatus | OWNER_REQUIRED |

## IX-22 - ReleaseHeldCapacity

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | FlightFlow |
| endpointSurface | /service/ |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | FlightFlow capacity TX; Ordering intent/evidence TXs separate |
| idempotency | Provider profile + stable effect key + immutable exact member scope/hash |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | FlightFlow returned HoldExpiresAt/resource lifecycle; requested expiry is not a local authority |
| readModelEffect | Per-service capacity evidence/coupling/operation facet |
| wireBinding | Observed routes and missing guarantees in CONTRACTS/04; no target production URI invented |
| authorityStatus | OWNER_REQUIRED |

## IX-23 - CancelCommittedCapacity

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | FlightFlow |
| endpointSurface | /service/ |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | FlightFlow capacity TX; Ordering intent/evidence TXs separate |
| idempotency | Provider profile + stable effect key + immutable exact member scope/hash |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | FlightFlow returned HoldExpiresAt/resource lifecycle; requested expiry is not a local authority |
| readModelEffect | Per-service capacity evidence/coupling/operation facet |
| wireBinding | Observed routes and missing guarantees in CONTRACTS/04; no target production URI invented |
| authorityStatus | OWNER_REQUIRED |

## IX-24 - ChangeOrDivideReservation

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | FlightFlow |
| endpointSurface | /service/ |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | FlightFlow capacity TX; Ordering intent/evidence TXs separate |
| idempotency | Provider profile + stable effect key + immutable exact member scope/hash |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | FlightFlow returned HoldExpiresAt/resource lifecycle; requested expiry is not a local authority |
| readModelEffect | Per-service capacity evidence/coupling/operation facet |
| wireBinding | Observed routes and missing guarantees in CONTRACTS/04; no target production URI invented |
| authorityStatus | OWNER_REQUIRED |

## IX-25 - AcquireGroupBlock

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | FlightFlow |
| endpointSurface | /service/ |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | FlightFlow capacity TX; Ordering intent/evidence TXs separate |
| idempotency | Provider profile + stable effect key + immutable exact member scope/hash |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | FlightFlow returned HoldExpiresAt/resource lifecycle; requested expiry is not a local authority |
| readModelEffect | Per-service capacity evidence/coupling/operation facet |
| wireBinding | Observed routes and missing guarantees in CONTRACTS/04; no target production URI invented |
| authorityStatus | OWNER_REQUIRED |

## IX-26 - AllocateExistingBlock

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | FlightFlow |
| endpointSurface | /service/ |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | FlightFlow capacity TX; Ordering intent/evidence TXs separate |
| idempotency | Provider profile + stable effect key + immutable exact member scope/hash |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | FlightFlow returned HoldExpiresAt/resource lifecycle; requested expiry is not a local authority |
| readModelEffect | Per-service capacity evidence/coupling/operation facet |
| wireBinding | Observed routes and missing guarantees in CONTRACTS/04; no target production URI invented |
| authorityStatus | OWNER_REQUIRED |

## IX-27 - Read capacity operation/resource

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | FlightFlow |
| endpointSurface | /service/ |
| semanticType | Query |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | FlightFlow authoritative read |
| idempotency | Exact effect key or canonical reservation/member IDs |
| timeoutSemantics | Read timeout is unavailable evidence; do not mutate commercial facts. |
| retrySemantics | Bounded read retry with original lookup scope and deadline. |
| unknownSemantics | Unavailable/Absent without authoritative scope and retention proof cannot satisfy an eligibility gate. |
| authoritativeRecovery | Original key/profile/scope lookup; NotFound/204 alone does not prove no effect |
| ttlExpiryOwner | FlightFlow source time, expiry, key retention and read consistency |
| readModelEffect | Evidence application in Ordering TX may update capacity facet |
| wireBinding | OWNER_REQUIRED side-effect-free lookup; inspected hold GET is commented out |
| authorityStatus | OWNER_REQUIRED |

## IX-30 - EstablishCoverage

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | JetPay |
| endpointSurface | /service/ |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | JetPay value/coverage TX; Ordering never owns tender/balance truth |
| idempotency | Original effect key + obligation/refund/transfer ID and immutable scope/hash |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | JetPay coverage expiry and operation-bound authority lifetime |
| readModelEffect | Coverage/payment movement/refund facets separate from commercial totals |
| wireBinding | Target required capability only; JetPay wire contract is not authoritative yet |
| authorityStatus | OWNER_REQUIRED |

## IX-31 - AcquireIssueAuthority

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | JetPay |
| endpointSurface | /service/ |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | JetPay value/coverage TX; Ordering never owns tender/balance truth |
| idempotency | Original effect key + obligation/refund/transfer ID and immutable scope/hash |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | JetPay coverage expiry and operation-bound authority lifetime |
| readModelEffect | Coverage/payment movement/refund facets separate from commercial totals |
| wireBinding | Target required capability only; JetPay wire contract is not authoritative yet |
| authorityStatus | OWNER_REQUIRED |

## IX-32 - FinalizeIssueAuthority

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | JetPay |
| endpointSurface | /service/ |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | JetPay value/coverage TX; Ordering never owns tender/balance truth |
| idempotency | Original effect key + obligation/refund/transfer ID and immutable scope/hash |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | JetPay coverage expiry and operation-bound authority lifetime |
| readModelEffect | Coverage/payment movement/refund facets separate from commercial totals |
| wireBinding | Target required capability only; JetPay wire contract is not authoritative yet |
| authorityStatus | OWNER_REQUIRED |

## IX-33 - ReleaseFunding

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | JetPay |
| endpointSurface | /service/ |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | JetPay value/coverage TX; Ordering never owns tender/balance truth |
| idempotency | Original effect key + obligation/refund/transfer ID and immutable scope/hash |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | JetPay coverage expiry and operation-bound authority lifetime |
| readModelEffect | Coverage/payment movement/refund facets separate from commercial totals |
| wireBinding | Target required capability only; JetPay wire contract is not authoritative yet |
| authorityStatus | OWNER_REQUIRED |

## IX-34 - AcquireRefundAuthority

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | JetPay |
| endpointSurface | /service/ |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | JetPay value/coverage TX; Ordering never owns tender/balance truth |
| idempotency | Original effect key + obligation/refund/transfer ID and immutable scope/hash |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | JetPay coverage expiry and operation-bound authority lifetime |
| readModelEffect | Coverage/payment movement/refund facets separate from commercial totals |
| wireBinding | Target required capability only; JetPay wire contract is not authoritative yet |
| authorityStatus | OWNER_REQUIRED |

## IX-35 - ExecuteRefund

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | JetPay |
| endpointSurface | /service/ |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | JetPay value/coverage TX; Ordering never owns tender/balance truth |
| idempotency | Original effect key + obligation/refund/transfer ID and immutable scope/hash |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | JetPay coverage expiry and operation-bound authority lifetime |
| readModelEffect | Coverage/payment movement/refund facets separate from commercial totals |
| wireBinding | Target required capability only; JetPay wire contract is not authoritative yet |
| authorityStatus | OWNER_REQUIRED |

## IX-36 - AcquireApplicationTransfer

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | JetPay |
| endpointSurface | /service/ |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | JetPay value/coverage TX; Ordering never owns tender/balance truth |
| idempotency | Original effect key + obligation/refund/transfer ID and immutable scope/hash |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | JetPay coverage expiry and operation-bound authority lifetime |
| readModelEffect | Coverage/payment movement/refund facets separate from commercial totals |
| wireBinding | Target required capability only; JetPay wire contract is not authoritative yet |
| authorityStatus | OWNER_REQUIRED |

## IX-37 - FinalizeApplicationTransfer

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | JetPay |
| endpointSurface | /service/ |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | JetPay value/coverage TX; Ordering never owns tender/balance truth |
| idempotency | Original effect key + obligation/refund/transfer ID and immutable scope/hash |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | JetPay coverage expiry and operation-bound authority lifetime |
| readModelEffect | Coverage/payment movement/refund facets separate from commercial totals |
| wireBinding | Target required capability only; JetPay wire contract is not authoritative yet |
| authorityStatus | OWNER_REQUIRED |

## IX-38 - Read funding/refund/transfer evidence

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | JetPay |
| endpointSurface | /service/ |
| semanticType | Query |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | JetPay authoritative read |
| idempotency | Original effect/resource IDs and exact obligation scope |
| timeoutSemantics | Read timeout is unavailable evidence; do not mutate commercial facts. |
| retrySemantics | Bounded read retry with original lookup scope and deadline. |
| unknownSemantics | Unavailable/Absent without authoritative scope and retention proof cannot satisfy an eligibility gate. |
| authoritativeRecovery | Repeat side-effect-free owner query using exact source identity/revision. |
| ttlExpiryOwner | JetPay source time/version/retention |
| readModelEffect | Updates local evidence only; not a new charge |
| wireBinding | Required, not invented JetPay endpoint |
| authorityStatus | OWNER_REQUIRED |

## IX-40 - Read product/version/eligibility

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | Ancillary |
| endpointSurface | /service/ |
| semanticType | Query |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | Ancillary metadata/eligibility decision |
| idempotency | Product version + declared traveler/coverage context |
| timeoutSemantics | Read timeout is unavailable evidence; do not mutate commercial facts. |
| retrySemantics | Bounded read retry with original lookup scope and deadline. |
| unknownSemantics | Unavailable/Absent without authoritative scope and retention proof cannot satisfy an eligibility gate. |
| authoritativeRecovery | Repeat side-effect-free owner query using exact source identity/revision. |
| ttlExpiryOwner | Ancillary product/eligibility validity; price validity remains AirPrice |
| readModelEffect | Accepted product/profile snapshot; catalog updates do not rewrite old sale |
| wireBinding | Target contract, real route not yet certified |
| authorityStatus | OWNER_REQUIRED |

## IX-41 - ReserveSupplierService

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | Non-flight supplier via certified adapter |
| endpointSurface | Supplier profile; not assumed PSS route |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | Supplier resource TX; separate Ordering operation TX |
| idempotency | Stable operation key, supplier booking reference and exact room/vehicle/service scope |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | Supplier deadline/expiry; not FlightFlow |
| readModelEffect | Non-flight reservation/fulfillment evidence |
| wireBinding | Certified supplier-specific wire required |
| authorityStatus | OWNER_REQUIRED |

## IX-42 - ConfirmSupplierService

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | Non-flight supplier via certified adapter |
| endpointSurface | Supplier profile; not assumed PSS route |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | Supplier resource TX; separate Ordering operation TX |
| idempotency | Stable operation key, supplier booking reference and exact room/vehicle/service scope |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | Supplier deadline/expiry; not FlightFlow |
| readModelEffect | Non-flight reservation/fulfillment evidence |
| wireBinding | Certified supplier-specific wire required |
| authorityStatus | OWNER_REQUIRED |

## IX-43 - CancelSupplierService

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | Non-flight supplier via certified adapter |
| endpointSurface | Supplier profile; not assumed PSS route |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | Supplier resource TX; separate Ordering operation TX |
| idempotency | Stable operation key, supplier booking reference and exact room/vehicle/service scope |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | Supplier deadline/expiry; not FlightFlow |
| readModelEffect | Non-flight reservation/fulfillment evidence |
| wireBinding | Certified supplier-specific wire required |
| authorityStatus | OWNER_REQUIRED |

## IX-44 - Read supplier operation/resource

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | Non-flight supplier via certified adapter |
| endpointSurface | Supplier profile |
| semanticType | Query |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | Supplier authoritative read |
| idempotency | Saved effect/resource reference |
| timeoutSemantics | Read timeout is unavailable evidence; do not mutate commercial facts. |
| retrySemantics | Bounded read retry with original lookup scope and deadline. |
| unknownSemantics | Unavailable/Absent without authoritative scope and retention proof cannot satisfy an eligibility gate. |
| authoritativeRecovery | Repeat side-effect-free owner query using exact source identity/revision. |
| ttlExpiryOwner | Supplier retention/expiry |
| readModelEffect | Recovered supplier evidence |
| wireBinding | Supplier-specific authoritative read-back required |
| authorityStatus | OWNER_REQUIRED |

## IX-50 - IssueExternalDocument

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | External document authority |
| endpointSurface | Certified external issuer profile |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | External issuer TX; Ordering evidence/doc projection TX separate |
| idempotency | Stable effect identity + document role/number when locally allocated + frozen scope |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | Issuer validity/void window/control-token lifetime |
| readModelEffect | Document/coupon evidence; no implicit commercial money changes |
| wireBinding | Not a mandatory dependency for Local authority; wire profile required when External |
| authorityStatus | OWNER_REQUIRED |

## IX-51 - VoidExternalDocument

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | External document authority |
| endpointSurface | Certified external issuer profile |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | External issuer TX; Ordering evidence/doc projection TX separate |
| idempotency | Stable effect identity + document role/number when locally allocated + frozen scope |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | Issuer validity/void window/control-token lifetime |
| readModelEffect | Document/coupon evidence; no implicit commercial money changes |
| wireBinding | Not a mandatory dependency for Local authority; wire profile required when External |
| authorityStatus | OWNER_REQUIRED |

## IX-52 - ExchangeExternalDocument

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | External document authority |
| endpointSurface | Certified external issuer profile |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | External issuer TX; Ordering evidence/doc projection TX separate |
| idempotency | Stable effect identity + document role/number when locally allocated + frozen scope |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | Issuer validity/void window/control-token lifetime |
| readModelEffect | Document/coupon evidence; no implicit commercial money changes |
| wireBinding | Not a mandatory dependency for Local authority; wire profile required when External |
| authorityStatus | OWNER_REQUIRED |

## IX-53 - ReleaseExternalCouponControl

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | External document authority |
| endpointSurface | Certified external issuer profile |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | External issuer TX; Ordering evidence/doc projection TX separate |
| idempotency | Stable effect identity + document role/number when locally allocated + frozen scope |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | Issuer validity/void window/control-token lifetime |
| readModelEffect | Document/coupon evidence; no implicit commercial money changes |
| wireBinding | Not a mandatory dependency for Local authority; wire profile required when External |
| authorityStatus | OWNER_REQUIRED |

## IX-54 - Read document/control outcome

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | External document authority |
| endpointSurface | Certified external issuer profile |
| semanticType | Query |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | Issuer authoritative read |
| idempotency | Operation/number/coupon IDs, no random replacement number |
| timeoutSemantics | Read timeout is unavailable evidence; do not mutate commercial facts. |
| retrySemantics | Bounded read retry with original lookup scope and deadline. |
| unknownSemantics | Unavailable/Absent without authoritative scope and retention proof cannot satisfy an eligibility gate. |
| authoritativeRecovery | Repeat side-effect-free owner query using exact source identity/revision. |
| ttlExpiryOwner | Issuer source version and retention |
| readModelEffect | Recovered exact document facts |
| wireBinding | Profile-specific side-effect-free lookup required |
| authorityStatus | OWNER_REQUIRED |

## IX-55 - IssueLocalDocuments

| Dimension | Contract |
|---|---|
| caller | Ordering Application |
| owner | Ordering document domain + Persistence |
| endpointSurface | No network endpoint |
| semanticType | Command |
| interaction | Sync |
| transport | In-process; SQL Server |
| transactionOwner | One Ordering SQL TX for stock/documents/projection/outbox |
| idempotency | CommandReceipt + unique document role/scope + stock allocation |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Read local commit token/receipt/stock allocation; no external issuer simulator needed |
| ttlExpiryOwner | Approved local issuer policy; source capacity/funding authority remains external |
| readModelEffect | Atomic ETKT/EMD view |
| wireBinding | In-process canonical issuance use case |
| authorityStatus | TARGET_DECISION |

## IX-60 - Ingest delivery observations/control evidence

| Dimension | Contract |
|---|---|
| caller | SkyDispatch |
| owner | Ordering |
| endpointSurface | /service/ |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | Ordering inbox + accepted observation TX |
| idempotency | SourceSystem/EventId + observationKey; equal identity/different hash quarantined |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Same source event replay; unresolved correlation retained, never guessed |
| ttlExpiryOwner | DCS event chronology/control validity; no commercial TTL |
| readModelEffect | Per-aspect delivery/control facets; sold data unchanged |
| wireBinding | POST /service/v1/delivery-observations (Ordering target) |
| authorityStatus | TARGET_DECISION |

## IX-61 - Request DCS control/release or approved instruction

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | SkyDispatch |
| endpointSurface | /service/ |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | Gateway/DCS semantic effect and separate Ordering evidence TX |
| idempotency | Stable instruction key + exact coupon/service/control version |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | DCS control authority and instruction validity |
| readModelEffect | Control pending/confirmed evidence; transport ACK not effect proof |
| wireBinding | Required target capability; gateway wire binding not approved |
| authorityStatus | OWNER_REQUIRED |

## IX-62 - Record impact / request accepted reaccommodation

| Dimension | Contract |
|---|---|
| caller | Disruption |
| owner | Ordering |
| endpointSurface | /service/ |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | Ordering inbound acceptance TX; separate atomic per-Order work |
| idempotency | Case/option/version + source command identity |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Local receipt and per-Order operations; no global fanout TX |
| ttlExpiryOwner | Disruption owns option/deadline disposition; pricing/hold/payment keep independent deadlines |
| readModelEffect | Impact facet first; commercial replacement only accepted remedy commit |
| wireBinding | POST /service/v1/disruption-impacts; POST /service/v1/reaccommodations |
| authorityStatus | TARGET_DECISION |

## IX-63 - Fanout per-Order servicing

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | Ordering workers |
| endpointSurface | No public HTTP surface |
| semanticType | Command |
| interaction | Async |
| transport | Durable local queue or broker with inbox |
| transactionOwner | One Order TX per work item; batch receipt only tracks routing |
| idempotency | Source case/version + OrderId + remedy instruction ID |
| timeoutSemantics | No delivery acknowledgment is not a business failure; retain durable delivery intent. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Re-enqueue unresolved items under same identities; completed items no-op |
| ttlExpiryOwner | Source option validity; lease expiry only execution scheduling |
| readModelEffect | Independent operation/projection per Order |
| wireBinding | Durable work queue/internal broker command |
| authorityStatus | TARGET_DECISION |

## IX-64 - Publish semantic financial/document/delivery facts

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | Ledger |
| endpointSurface | Not an HTTP endpoint |
| semanticType | Event |
| interaction | Async |
| transport | Transactional outbox -> durable broker |
| transactionOwner | Ordering outbox atomically with fact; Ledger posting in its own TX |
| idempotency | Stable EventId plus EconomicFactId/role; recipient inbox and economic dedup |
| timeoutSemantics | No delivery acknowledgment is not a business failure; retain durable delivery intent. |
| retrySemantics | Republish same EventId; consumer deduplicates in its local transaction. |
| unknownSemantics | Ordering retains the outbox item until transport confirmation; this is not remote business acceptance. |
| authoritativeRecovery | Local outbox/inbox identity and receiver reconciliation; re-delivery is not a second fact. |
| ttlExpiryOwner | No sale TTL on committed facts; delivery retention is operational policy |
| readModelEffect | Local published/pending transport facet only; not Issue gate |
| wireBinding | Ordering-owned event catalog; production topology/envelope agreement BD-011 |
| authorityStatus | TARGET_DECISION |

## IX-65 - DCS protocol exchange

| Dimension | Contract |
|---|---|
| caller | DCS |
| owner | SkyDispatch |
| endpointSurface | DCS protocol surface, not Ordering HTTP |
| semanticType | Command |
| interaction | Async |
| transport | Gateway/DCS certified protocol |
| transactionOwner | DCS/gateway owners |
| idempotency | Gateway maps protocol identities to stable source event/instruction IDs |
| timeoutSemantics | No delivery acknowledgment is not a business failure; retain durable delivery intent. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | DCS/gateway protocol and control lifetimes |
| readModelEffect | Indirect normalized observations only |
| wireBinding | Gateway-owned protocol; Ordering consumes normalized contract only |
| authorityStatus | OWNER_REQUIRED |

## IX-66 - Rebuild projection / schedule recovery / stock administration

| Dimension | Contract |
|---|---|
| caller | System administrator |
| owner | Ordering |
| endpointSurface | /internal/ |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | Ordering local TX; recovery schedules canonical application use cases |
| idempotency | Admin command receipt + typed scope and privileges |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Local administrative receipt; never arbitrary mark-success or raw DB mutation |
| ttlExpiryOwner | No authority to override provider TTL/evidence |
| readModelEffect | Rebuild or technical recovery status; stock administration scoped |
| wireBinding | Explicit administrative endpoints in API-CONTRACTS |
| authorityStatus | TARGET_DECISION |

## IX-67 - Resolve authorized financial customer/reference snapshot

| Dimension | Contract |
|---|---|
| caller | Ordering ReferenceData |
| owner | Customer service |
| endpointSurface | /service/ |
| semanticType | Query |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | Customer authoritative read; Ordering stores local reference snapshot |
| idempotency | Customer identity/version, authorized caller/customer scope |
| timeoutSemantics | Read timeout is unavailable evidence; do not mutate commercial facts. |
| retrySemantics | Bounded read retry with original lookup scope and deadline. |
| unknownSemantics | Unavailable/Absent without authoritative scope and retention proof cannot satisfy an eligibility gate. |
| authoritativeRecovery | Read authoritative customer by exact identity; stale/missing snapshot cannot invent authorization |
| ttlExpiryOwner | Customer source revision and reference cache policy, never Order commercial TTL |
| readModelEffect | Local customer display/reference snapshot; accepted buyer history never rewritten |
| wireBinding | Owner-approved lookup or authenticated existing snapshot; no URI inferred |
| authorityStatus | OWNER_REQUIRED |

## IX-68 - Reference snapshot update

| Dimension | Contract |
|---|---|
| caller | Reference owners (Customer / AirInfo) |
| owner | Ordering ReferenceData |
| endpointSurface | Not Ordering business HTTP |
| semanticType | Event |
| interaction | Async |
| transport | Broker/inbox or approved snapshot synchronization |
| transactionOwner | Ordering reference inbox/cache TX only; commercial history untouched |
| idempotency | Source event identity + entity source version |
| timeoutSemantics | No delivery acknowledgment is not a business failure; retain durable delivery intent. |
| retrySemantics | Republish same EventId; consumer deduplicates in its local transaction. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Local outbox/inbox identity and receiver reconciliation; re-delivery is not a second fact. |
| ttlExpiryOwner | Reference source revision/retention policy |
| readModelEffect | Refresh current reference display; no replacement of frozen sale facts |
| wireBinding | Owner-approved event binding required |
| authorityStatus | OWNER_REQUIRED |

## IX-69 - Read operational flight version

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | FlightFlow |
| endpointSurface | /service/ |
| semanticType | Query |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | FlightFlow authoritative operational read |
| idempotency | Flight identity plus source version |
| timeoutSemantics | Read timeout is unavailable evidence; do not mutate commercial facts. |
| retrySemantics | Bounded read retry with original lookup scope and deadline. |
| unknownSemantics | Unavailable/Absent without authoritative scope and retention proof cannot satisfy an eligibility gate. |
| authoritativeRecovery | Repeat side-effect-free owner query using exact source identity/revision. |
| ttlExpiryOwner | FlightFlow source chronology, not Offer/Order TTL |
| readModelEffect | Operational schedule facet only; never sold itinerary overwrite |
| wireBinding | OBSERVED flight/version-history routes in CONTRACTS/04 |
| authorityStatus | OBSERVED_SOURCE |

## IX-70 - AcquireExchangeFundingAuthority

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | JetPay |
| endpointSurface | /service/ |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | JetPay value/coverage TX; Ordering never owns tender/balance truth |
| idempotency | Original effect key + obligation/refund/transfer ID and immutable scope/hash |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | JetPay coverage expiry and operation-bound authority lifetime |
| readModelEffect | Exchange funding/transfer/additional collection/residual authority evidence, never a duplicate original capture |
| wireBinding | Target required capability only; JetPay wire contract is not authoritative yet |
| authorityStatus | OWNER_REQUIRED |

## IX-71 - RedeemDocumentForRefund

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | External document authority |
| endpointSurface | Certified external issuer profile |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | External issuer TX; Ordering evidence/doc projection TX separate |
| idempotency | Stable effect identity + document role/number when locally allocated + frozen scope |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | Issuer validity/void window/control-token lifetime |
| readModelEffect | Documentary refund disposition evidence; tender payout remains an explicit separate owner effect |
| wireBinding | Not a mandatory dependency for Local authority; wire profile required when External |
| authorityStatus | OWNER_REQUIRED |

## IX-72 - ReassociateEmdCoupon

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | External document authority |
| endpointSurface | Certified external issuer profile |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | External issuer TX; Ordering evidence/doc projection TX separate |
| idempotency | Stable effect identity + document role/number when locally allocated + frozen scope |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | Issuer validity/void window/control-token lifetime |
| readModelEffect | Versioned EMD coupon association history; no rewritten original issued snapshot |
| wireBinding | Not a mandatory dependency for Local authority; wire profile required when External |
| authorityStatus | OWNER_REQUIRED |

## IX-73 - ReleaseUnusedGroupBlock

| Dimension | Contract |
|---|---|
| caller | Ordering |
| owner | FlightFlow |
| endpointSurface | /service/ |
| semanticType | Command |
| interaction | Sync |
| transport | HTTPS/JSON |
| transactionOwner | FlightFlow capacity TX; Ordering intent/evidence TXs separate |
| idempotency | Provider profile + stable effect key + immutable exact member scope/hash |
| timeoutSemantics | Before durable acceptance: unavailable. After acceptance/possible dispatch: return/persist operation Pending or Unknown. |
| retrySemantics | Resume saved operation; no generic mutation retry, no new key. Replay only under certified idempotency semantics. |
| unknownSemantics | Keep unresolved intent/evidence and relevant claim; suspend dependent effects, not independent factual ingestion. |
| authoritativeRecovery | Owner read-back of original operation and resource; no create-as-lookup inference. |
| ttlExpiryOwner | FlightFlow returned HoldExpiresAt/resource lifecycle; requested expiry is not a local authority |
| readModelEffect | Per-block unused/released/allocated quantities; no addition of capacities across flights |
| wireBinding | Observed routes and missing guarantees in CONTRACTS/04; no target production URI invented |
| authorityStatus | OWNER_REQUIRED |
