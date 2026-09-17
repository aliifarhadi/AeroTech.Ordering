# Historical scope correction — predecessor Pack 2.1

Predecessor Pack 2.0 overreached by turning several implementation/team mechanics into binding architecture. Pack 2.1 corrected that overreach. This file is retained as historical audit evidence; the current pack is 3.8 and the binding standing resolutions are in `GOVERNANCE/05-STANDING-RULE-CONFLICT-RESOLUTION.md`.

## Removed from design authority

The pack no longer decides or mandates:

- SQL Server instance/location or whether it is local/shared/containerized;
- Docker/container composition;
- RabbitMQ/local broker provisioning;
- JWT/test issuer choice;
- exact script names such as `Start-Local.ps1` / `Test-Stage.ps1`;
- exact evidence/report directory;
- automatic versus manual agent continuation;
- fixed test-suite duration policy;
- creation/naming of Architecture/Application/API/E2E test projects or a SimulatorHost solely because the pack prefers them;
- replacement of the existing framework API error envelope;
- replacement/modification of the shared integration-event base envelope;
- the concrete technical source of `OwnerAirlineId` when the platform convention has not been established.

## Still binding

- domain model, aggregate boundaries and invariants;
- immutable commercial/pricing/history rules;
- layer responsibilities and forbidden dependencies needed for domain purity;
- explicit local transaction boundaries and no network call inside SQL transaction;
- idempotency, durable external-operation identity, Pending/Unknown/recovery semantics;
- one mutation rail and one projection writer;
- service ownership and semantic integration ports;
- required authoritative read-back/evidence for effectful owners;
- SQL Server data invariants/constraints and precision requirements where business semantics demand them (not the server location);
- vertical-slice capability dependencies and scenario acceptance criteria;
- blocked-decision discipline when owner semantics are unavailable.

Existing repository/framework conventions remain authoritative for mechanics not required to satisfy these semantic rules. The historical bullet above about an unestablished `OwnerAirlineId` source no longer describes the current target: Pack 3.8 C15 records the now-observed `IHomeOperatorProvider` → `ReferenceDataHomeOperatorProvider` convention.
