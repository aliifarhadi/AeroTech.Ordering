# FlightFlow - required capacity capabilities and observed wire

## Required target operations

`IReservationPort`: Reserve, Commit, ReleaseHeld, ReleaseCommitted, Extend, ReadOperation, ReadReservation, DivideReservation, ReserveGroupBlock, AllocateFromBlock and ReleaseGroupBlock. These are typed methods/plans within ONE capacity boundary, not legacy and vNext competing rails. Methods not needed until later slices remain unsupported until implemented, with declared capability status.

Reserve request contains exact resource/Service/traveler mappings, capacity type, quantity/unit, cabin/RBD/allotment/block refs when applicable and a requested validity policy reference. It never claims a passenger gender/revenue/expiry value came from an owner when it did not. Response supplies canonical resource/member refs, actual quantity, source version, Held/Waitlisted/etc and actual HoldExpiresAt. Commit is a distinct mutation with operation-bound idempotency and proof of Committed resources, not just a successful Reserve.

ReleaseHeld differs from ReleaseCommitted. ReadOperation is effect history; ReadReservation current capacity state. Expiry, partial outcomes and marriage/atomic groups follow DOMAIN/05. Group block allocation consumes the existing block exactly once rather than holding general-sale inventory again.

## Observed current controller

Inspected source routes (case retained):

```text
POST   /Service/v1/Flights/Seat-Holds
POST   /Service/v1/Flights/Seat-Holds/{id}/Confirmations
PATCH  /Service/v1/Flights/Seat-Holds/{id}
DELETE /Service/v1/Flights/Seat-Holds/{id}
POST   /Service/v1/Flights/Seat-Holds/{id}/Splits
POST   /Service/v1/Flights/Seat-Confirmations/{id}/Reversals
POST   /Service/v1/Flights/Seat-Confirmations/{id}/Cancellation
```

The inspected controller's proposed `GET SeatHolds/{RequestId}` is commented out. Confirm/extend/release actions return NoContent after command dispatch, with no resource evidence body. This demonstrates a mapping/evidence gap in the inspected surface; the broader old audit reports additional owner concurrency/expiry defects. Those additional defects were not independently rerun in this review and are not asserted fixed or still present merely from this controller.

Observed HoldSeatsRequest:

```text
IdempotencyKey:string; Reference:string; ExpiresAt:DateTimeOffset
Passengers[{PaxReference:string, Type:PassengerTypeCode, Gender:Gender}]
Flights[{FlightCapId:string, FlightAllotmentId?:string,
         Seats[{PaxReference:string, Revenue:decimal, Seat?:string}]}]
```

BD-002: exact seat requirement/infant/gender handling, Revenue meaning and valuation, requested expiry authority/policy, reference scope and allotment semantics. Sending Revenue=0 or a guessed default expiry just because legacy did is prohibited. BD-003: authoritative read-back/commit/release evidence, no-effect proof, idempotency retention, concurrency/expiry safety and complete batch membership. A 204 cannot prove a missing/expired hold was committed.

The real adapter can be developed and tested for verified mappings, but a write capability is not LIVE_CERTIFIED until both decisions and owner behavior are verified. No hidden fallback to direct owner SQL, AirOffer capacity tables or legacy Ordering is allowed. The reference capacity simulator provides all required lifecycle/read-back/concurrency behavior in an isolated durable owner store.
