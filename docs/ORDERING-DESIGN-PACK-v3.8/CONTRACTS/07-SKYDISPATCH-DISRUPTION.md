# SkyDispatch and Disruption contracts owned/exposed by Ordering

## SkyDispatch inbound

`POST /service/v1/delivery-observations` accepts an authenticated normalized event batch as a command to record facts, not a command to declare the Order flown. Required batch fields: SourceSystem, SourceEventId, SchemaVersion, observations[]; each row has ObservationKey, aspect/type, stable or external correlation, occurredAt, source epoch/sequence/revision when provided, supersession and protected payload reference as appropriate. Return 202 after durable inbox/observation acceptance, with row correlation status and processing operation reference when deferred.

`POST /service/v1/delivery-correlations/resolve` is a semantic Query despite POST, for bounded sets of external ticket/coupon/reservation/service refs. It reads local correlation maps, applies service authorization and returns current service/Order ownership plus historical source ref. No external lookup in GetOrder/correlation resolution.

`IDeliveryControlPort.AcquireControl`, ReleaseControl and ReadOperation/ReadControl are outbound commands/queries only when DCS/issuer control acknowledgment is needed. Transport is HTTPS/JSON in reference profiles. An outbox event announcing document issue is not the command's ack. Actual SkyDispatch mapping is BD-009. DCS protocol specifics stay in SkyDispatch.

## Disruption inbound

`POST /service/v1/disruption-impacts` accepts a source-versioned impact batch into inbox/fanout work. Owner is Disruption for case facts; Ordering owns each resulting local projection. Required: case ID/version, source event ID, flight/service scope and effect/option/deadline facts actually supplied. Idempotency is case/event/row based.

`POST /service/v1/orders/{id}/reaccommodations` accepts a specifically authorized, customer-accepted-when-required option/decision with expected versions, service scope, offer preparation/pricing decision refs and authority. It reuses the canonical servicing application; it is not a direct SQL edit by Disruption. Return operation status/URL with 202 when external work remains.

Queries for affected Orders/operation status are local, authorized and paginated. High fanout schedules per-Order work asynchronously; one failure does not roll back all Orders. Exactly-once local change is provided by receipt/inbox/version constraints, not by assuming broker exactly-once delivery. BD-010 covers actual Disruption envelope/authority/deadline mapping, not permission to invent cases inside Ordering.
