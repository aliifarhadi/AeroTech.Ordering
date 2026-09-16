# Ordering API contracts and canonical handlers

## Common HTTP rules

Base surfaces are `/service/v1`, `/backoffice/v1`, `/otapanel/v1`, `/ota/v1`, `/internal/v1`. Service/office/agency sale routes are wrappers around the SAME Application command. Internal is administration only. Expose only permitted commands on each surface and test the authorization matrix.

Every mutation requires `Idempotency-Key` and authenticated caller context. Commercial changes carry expectedCommercialVersion; document operations also carry expectedDocumentVersion and the accepted plan's relevant version vector. IDs are opaque strings; monetary values are exact decimal strings on target HTTP contracts. Owner DTO serialization stays in its ACL.

Terminal receipt replay returns the original status/result with `Idempotent-Replay: true`; GET retrieves latest local state. Same key/different payload is 409. Pending external work is 202 with operationId and a same-surface local operation URL. Unknown never maps to a false permanent business denial. Responses cannot expose another customer's existing result.

Error body: `type`, `title`, HTTP `status`, stable `code`, `detail` (redacted), `correlationId`, optional `operationId`, `retryable`, `reasons[]` and capability/decision ID where applicable. 400 malformed shape; 401/403 authentication/authorization; 404 missing or intentionally undisclosed scope; 409 key/version/claim conflicts; 422 known business invalidity/unsupported scenario; 503 pre-dispatch capability unavailable. Once a mutation may have happened, report pending operation evidence rather than blind 503 retry advice.

## S1 - exact contract

### Prepare

`POST /{surface}/v1/order-preparations` (sale surfaces, not internal).

Body: offerId (opaque, required), clientReference (optional), requested product selection only when source supports explicit subset. Default is the full candidate; no arbitrary slicing of a priced package. Caller/customer/owner scope is validated from authenticated context; any submitted financialCustomerId is checked for authority, never trusted as identity.

Application: resolve candidate BEFORE acceptance, normalize/validate its source semantics, then atomically persist preparation and receipt/result. It creates no Order, no hold, no payment and no document. Concurrent same-key preparations may query the owner more than once but only one immutable local snapshot wins. To refresh price, use a new preparation/key and explicitly display/accept it.

201 body: preparationId, sourceOfferId, acceptedSnapshotDigest, capturedAt, pricedAt, validity facts, acceptanceAssurance, permittedAcceptanceProfile, normalized traveler/segment/item/price view, and clear production-acceptance blocking reasons. Does not claim the local digest is a source version.

### Create

`POST /{surface}/v1/orders/from-offer`.

Body: preparationId, acceptedSnapshotDigest, acceptedAt (client assertion retained separately from server acceptance instant), financialCustomerId when delegated, travelerBindings[] (`sourceTravellerRef`, `clientTravelerRef`, supplied name/DOB/PTC/identity/guardian fields as required), contacts[] and optional clientReference. No caller-set total, status, owner, ticket number, expiry or capacity-confirmed flag.

Require exact complete traveler reference binding, permitted acceptance profile, source scope and validity. One local TX consumes preparation and writes Order/items/services/sold pricing/change/receipt/projection/outbox. 201 body: orderId, orderReference, commercialVersionAtCommit=1, orderRevisionAtCommit, acceptedSourceDigest and local order URL. Same preparation consumed with a DIFFERENT key gives 409 PreparationAlreadyConsumed and an authorized existing result reference; never another Order.

### Read

`GET /{surface}/v1/orders/{orderId}` returns local OrderDetails. `?minRevision=` is optional and must be honored or report lag. Root view includes identity/customer/owner-safe summary, accepted source assurance, travelers, items, typed services/coverage, sold and current operational itinerary separately, commercial/pricing/funding totals, capacity/document/delivery facets, active operations, reasons and as-of revisions. Protected payloads are redacted by permissions. Owner outage cannot make an existing local Order disappear.

`GET /{surface}/v1/operations/{operationId}` returns local phase, outcomes, per-target evidence summaries, pending actions and safe recovery status. It does not poll a provider as a side effect.

## Later command bodies

All scoped commands use stable service/item/coupon IDs; expansion of coupled scope is returned for explicit acceptance when it exceeds caller intent. The commands in SPEC/commands.json define required fields and stage/handler. Here are the canonical contracts:

| Route after surface/v1 | Method / semantic type | Required body / result |
|---|---|---|
| orders/{id}/reservations | POST Command | expectedCommercialVersion, serviceIds, reservationPolicyRef; OperationResult |
| orders/{id}/reservation-releases | POST Command | reservation/member refs, expected resource facts, reason; OperationResult |
| orders/{id}/funding-coverage | POST Command | obligation refs/versions, approved funding intent reference, exact scope; OperationResult |
| orders/{id}/funding-releases | POST Command | coverage/authority refs and reason; OperationResult |
| orders/{id}/issues | POST Command | expectedCommercialVersion, serviceIds/document plan ref, expected facts; OperationResult |
| orders/{id}/cancellation-quotes | POST Query+local quote capture | scope, expectedCommercialVersion; immutable quote ref/digest/economics |
| orders/{id}/cancellations | POST Command | accepted quote/plan ref+digest, expectedCommercialVersion, scope; OperationResult |
| orders/{id}/ancillary-preparations | POST preparation Command | product/version, quantities, beneficiary/coverage refs; immutable priced candidate |
| orders/{id}/ancillaries | POST Command | accepted preparation/digest, expectedCommercialVersion; new item/service/obligation refs |
| orders/{id}/documents/{documentId}/voids | POST Command | expectedDocumentVersion, document scope, mode, approved plan/authority; OperationResult |
| orders/{id}/refund-quotes | POST Query+local quote capture | explicit coupons/services, expectedCommercialVersion; decision ref/digest |
| orders/{id}/refunds | POST Command | accepted refund decision/digest, expected versions, payout authority intent ref; OperationResult |
| orders/{id}/exchange-quotes | POST Query+local quote capture | old scope + new candidate prep, expected versions; full old/new decision |
| orders/{id}/exchanges | POST Command | accepted full exchange decision/digest, expected versions; OperationResult |
| orders/{id}/traveler-corrections | POST Command | travelerId, exact protected new data, reason/authority, expectedCommercialVersion; OperationResult |
| orders/{id}/revalidations | POST Command | document/coupon refs, accepted binding/authority, expected vector; OperationResult |
| orders/{id}/splits | POST Command | whole traveler IDs, partition decision/digest, expected vector; source+child OperationResult |
| related-orders | POST Command | authorized Order refs + relation/reason; relation record only |
| groups | POST Command (backoffice/service only) | accepted group contract/price/context and block intent; GroupOperationResult |
| groups/{id}/materializations | POST Command (backoffice/service only) | stable per-row client refs, named passenger data, allocations/pricing; row results |

Quote capture is locally stateful preparation even when the pricing owner call is a Query. It must use request idempotency for the capture and distinguish that local Command from the remote Query. No generic `PATCH status` or `PUT Order` endpoint exists.

## Admin and service-only ingress

Internal: projection rebuild, recovery-attempt scheduling, typed reconciliation resolution and DocumentStock administration. Each has specific privileges and cannot be used to set commercial truth arbitrarily. Service-only: delivery observations, correlation resolution, disruption impacts/reaccommodation and authenticated provider evidence ingress. These all use the canonical application/inbox path, not special direct database writers.
