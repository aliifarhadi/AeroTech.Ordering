# B0 — Open decisions (recorded, not decided)

Each item stops only its own path. Everything else in B0 proceeds.

## OD-B0-01 — Inbox dedup identity

Pack `ARCHITECTURE/03`: dedup key `(OwnerAirlineId, SourceSystem, EventId, ConsumerName)` with payload-hash conflict
detection. Shell today: `(MessageId, Consumer)` where `MessageId` is the MassTransit message id.

Gap: the owner events Ordering will consume do not carry that identity. Example:
`AeroTech.Messages.FlightFlow.IntegrationEvents.FlightReservationExpiredEvent(string ReferenceId)` does not derive from
`BaseIntegrationEvent` — no `EventId`, no `SourceSystem`. Which value is `EventId`/`SourceSystem` for such a fact (transport
`MessageId`? an owner-side change?) and where `OwnerAirlineId` is taken from inside a consumer is a cross-service
contract decision.

B0 does: keep `(MessageId, Consumer)`, reject a message without `MessageId`, classify only the inbox key violation as a
duplicate, commit marker and work together. B0 does not: reshape the key or add the payload hash.

Answer (owner, 2026-09-17) — CLOSED:
- `OwnerAirlineId`: Core `Service/v1/OperatorSettings` (`HOME_OPERATOR`, `homeAirlineId`) synced into ReferenceData, read
  through `IHomeOperatorProvider`.
- `SourceSystem` on Ordering's own events follows StoredValue (`IntegrationEventOptions.SourceSystem`, section
  `IntegrationEvents`, service name as default).
- Every integration event derives from `BaseIntegrationEvent`. Siblings offer no other identity: StoredValue/Identity
  dedup on transport `(MessageId, Consumer)`; Core/FlightFlow/AirPrice dedup on `(EventId, ConsumerName)` read from the
  message itself. Ordering therefore takes `SourceSystem` + `EventId` from the shared envelope
  (`BaseIntegrationEvent` / `BaseCommand`) and rejects a message that has none.
- Owner facts that do not exist yet (FlightFlow hold expiry, flight change) are handoffs to their owner:
  `E:\Projects\DotAir\handoffs\OR-001-flightflow-capacity-facts-with-event-envelope.md`. Ordering builds its side
  after the owner delivers.

Implemented: key `(OwnerAirlineId, SourceSystem, EventId, Consumer)`, `PayloadHash` (SHA-256 of the serialized message),
same identity + different hash → `InboxPayloadConflictException`.

## OD-B0-02 — SC-B0-010 authenticated diagnostics

SC-B0-010 requires "authenticated diagnostics" and "invalid credentials denied". Owner decision 2 forbids a test issuer
or bypass in B0 and places authenticated E2E outside B0 until the Identity/Aegis convention exists. B0 proves only host
start, liveness, readiness and restart, and inventories the anonymous surface. Known and unchanged: JWT validation is
not configured when `JwtSecrets:Key` is empty (`Framework.Presentation/PresentationExtensions`).

Answer (owner, 2026-09-17): `JwtSecrets` removed. Bearer validation follows Identity the way Core does: `Jwt:Authority`
(Identity `Protocol:Issuer`), `Jwt:Audience` = `pss-api`, `Jwt:RequireHttpsMetadata`; issuer, audience, lifetime and
signing key validated, RS256/ES256. Still outside this answer: a real Identity-issued token E2E test (no test issuer).

## OD-B0-03 — SDK pin and script names

`SLICES/B0.md` item 5: pin SDK and add reproducible boot/test scripts. A `global.json` (local SDK is 10.0.401) affects
the Docker/k8s build images, and script names/locations are a repository convention. B0 documents the exact commands in
the `REPORT.md` runbook and adds neither file.

Answer:
