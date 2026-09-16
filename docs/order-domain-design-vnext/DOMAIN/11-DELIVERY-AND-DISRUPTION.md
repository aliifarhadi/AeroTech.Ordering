# Delivery, DCS and Disruption - separate operational facts

## Ingress ownership

SkyDispatch translates the DCS protocol and invokes Ordering service commands/queries. Ordering does not parse airline-specific DCS wire protocols in Domain, and DCS does not directly mutate SQL. Disruption owns cases/options/orchestration; Ordering owns applying a specifically authorized commercial change to one Order. A DisruptionImpact projection is not a duplicate disruption aggregate.

## Observation model

DeliveryObservation fields: ObservationId, SourceSystem/EventId, ObservationKey (for batch rows), source epoch/sequence/revision when available, external correlation refs, resolved stable ServiceId, OrderIdAtReceipt/Occurrence, traveler/segment refs where valid, Aspect, Type, OccurredAt, ReceivedAt, SupersedesObservationId, protected payload reference/hash.

Aspects: CheckIn, Boarding, Travel, Seat, Baggage and Consumption. Types include CheckedIn, CheckInCancelled, Boarded, Offloaded, NoShow, Flown, SeatAssigned/Changed, BaggageAccepted/Loaded/Delivered and ServiceConsumed. Store each normalized fact; source ordering is per aspect/entity/epoch as contract defines, not a global timestamp sort.

Duplicate `(SourceSystem, EventId, ObservationKey)` with identical payload is no-op; different payload is conflict/quarantine. A later ReceivedAt does not make an older source revision current. A correction names the prior fact/revision and preserves history. When no reliable ordering rule exists, retain evidence as unresolved and block unsafe servicing; do not fabricate a sequence.

## Operational facets

ServiceDeliveryState exposes per-aspect progress, consumption quantities/portions, current assignment and relevant evidence provenance. SegmentOperationalState exposes current schedule, actual operational legs, aircraft, departure/arrival and diversion/return/cancellation outcomes. Original sold details remain immutable and separately visible.

Boarding is not flown; offload can legitimately change boarding without reversing unrelated baggage facts. Airport standby is not inventory waitlist. A baggage acceptance observation is not necessarily consumption of a multi-sector paid baggage service. Consumption has an owner-defined occurrence identity and portion/quantity to prevent double counting.

## Documents and concurrent servicing

Only a certified consumption/control observation can transition document usage/control. Raw CheckedIn/Boarded does not automatically set financial coupon Used. External control must be released/secured by the controlling owner before refund/void/exchange when required. Local operation claims alone cannot guarantee a passenger will not board in DCS.

Observations are never rejected just because an Order has an active servicing claim. Ingest them durably. Apply safe operational facets and raise the Order fact revision; a conflicting fact suspends the coordinator before its next irreversible dispatch/pivot. If it contradicts an already committed disposition, retain both facts and create reconciliation; do not overwrite a refunded/exchanged coupon as Open or invent an automatic financial reversal.

## Readiness and outbound control

`ReadyForDelivery` requires a concrete assigned passenger segment, required committed capacity, valid issued document or explicit document-free profile, sufficient funding authority, delivery correlation and compatible control/operational restrictions. OpenAir is not ready until binding. These are computed facets, not a new globally mutable Order state.

Ordering emits delivery-ready/change facts through outbox. A command requiring SkyDispatch/DCS acknowledgment is a durable `IDeliveryControlPort` operation, distinct from a notification. An event being published is not acknowledgment that DCS transferred control. The exact owner wire mapping is BD-009; the reference simulator implements the required control semantics.

## Disruption

Ingest versioned external case impact with affected external flights/services, case/version, reason, offered action and customer-acceptance/deadline facts when supplied. High-fanout flight/case input is accepted once into an inbox/work table and expands asynchronously into per-Order work keys. Each Order applies atomically and retries independently; never one distributed transaction for all passengers.

A schedule/cancellation impact alone neither refunds money nor replaces sold services. A Disruption request to reaccommodate supplies an authorized accepted plan/option; reuse the exchange/revalidation coordinators with involuntary reason/waiver/authority evidence from AirPrice/Disruption. Do not assume zero penalty or auto-accept at a deadline. Store external case lineage and expose actual pending choice locally.

After Split, correlation resolves by stable ServiceId/current ownership while preserving original message Order reference. Old correlation maps remain versioned; no direct use of a stale external OrderId to mutate the wrong current Order.
