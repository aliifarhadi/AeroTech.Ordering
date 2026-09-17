# AeroTech.Ordering - Day 0 Design Pack 3.8

**Target repository:** https://github.com/aliifarhadi/AeroTech.Ordering  
**Observed branch:** `k8s-stg`  
**Reviewed bootstrap commit:** `2f2b9033f675387f68ef7ff95c9d0a575d836fb5`  
**Issued:** 2026-09-17  
**Artifact:** domain/architecture design specification, not implemented software, environment prescription or production certification.

## Mission

Build a fresh airline Ordering domain and application on the technical shell already in the target repository. Preserve the owner's business boundaries, not the previous implementation's mistakes. This pack replaces the previous `ORDERING-VNEXT-FRESHBUILD-DESIGN-PACK` as the target domain and architecture specification. No other design pack or business repository is required to understand the target.

Start at [START-HERE.md](START-HERE.md), then execute the separately supplied [AGENT-START-PROMPT.md](AGENT-START-PROMPT.md). Do not create another repository. Do not recopy old business code. The current shell is an input to certify, not a certified architecture.



## Pack 3.8 integrity corrections

Pack 3.8 closes the prior reference/alignment gaps and the contradictions found in the full-pack audit: fixture/example references are package-validated; `SPEC/layers.json` is a logical responsibility policy; Domain→Messages is an explicit type allowlist rather than a project-reference-removal rule; `OwnerAirlineId` remains on `IHomeOperatorProvider`/`ReferenceDataHomeOperatorProvider`; replay correctness has no invented client-visible flag; event economic rules are role-specific rather than forcing every event through one PriceChangeSet; and simulator slices no longer assume deterministic business adapters already exist. A targeted branch check at `k8s-stg@957757471d9db3fde91922142d537a559b8084e4` found the `Providers.Deterministic` project/composition shell but no registered deterministic business adapters.

`CLAUDE.md` remains repository guidance, not a competing business-design source. The user authorizes a limited conflict-alignment edit when needed: update only statements that directly contradict this pack's design authority, approval rule, deterministic-provider status or shared-envelope rule; preserve all unrelated repository mechanics. Any broader `CLAUDE.md` change requires separate user approval.

## Authority boundary — what this pack may and may not decide

This pack is binding for **Ordering business semantics and architecture boundaries**: domain ownership, aggregates and invariants, accepted historical facts, layer responsibilities, transaction semantics, idempotency/recovery semantics, external-service ownership, semantic ports, integration facts, persistence invariants, capability prerequisites and scenario acceptance criteria.

This pack is **not** authority for developer/team operating choices unless the user separately asked for them. It must not choose: SQL Server instance/location, Docker/container use, broker provisioning, local JWT/test issuer, CI runner, test duration policy, exact report/evidence directory, script names, automatic-vs-manual continuation, IDE/worktree layout, or creation of extra test/tool projects solely for convenience. Those follow explicit user instructions and existing repository/team conventions.

Likewise, existing framework conventions for API envelopes, exception plumbing, integration-event base classes, naming and host/test setup are preserved unless an Ordering semantic requirement makes them insufficient. When a semantic requirement conflicts with an explicit repository convention, record the conflict for user decision instead of silently replacing the convention.

## Authority and honesty

`USER_BINDING` marks the user's established requirements. `TARGET_DECISION` is a concrete design choice made in this revision for implementation. `OBSERVED_SOURCE` describes inspected code, not deployed behavior. `OWNER_REQUIRED` defines an external capability Ordering needs; it does not assert the external owner implements it. `BLOCKED_DECISION` means a required authoritative decision is missing. These categories are never interchangeable.

The pack defines a complete reference implementation path and explicit extension boundaries. It cannot guarantee correctness for every unspecified future business rule. The guarantee to enforce is narrower: no unsupported scenario is silently accepted, no original fact is erased, and an extension has a named owner, invariant, compatibility change and test. See [DOMAIN/14-FUTURE-SCENARIO-BOUNDARIES.md](DOMAIN/14-FUTURE-SCENARIO-BOUNDARIES.md).

## Sequence and visible results

`D0 inventory + design conformance -> B0 certify/repair shell -> S1 real-offer preparation/acceptance/Create/Get -> S2 reservation -> S3 funding -> S4 issuance -> S5 cancellation -> S6 ancillary sale -> S7 EMD extensions -> S8 void -> S9 refund -> S10 exchange/reissue -> S11 disruption -> S12 DCS -> S13 traveler/revalidation servicing -> S14 split -> S15 groups -> S16 release certification`.

D0 is a design/source conformance checkpoint. B0 certifies the technical shell. Every S-stage is defined as an end-to-end capability boundary so implementation can be verified incrementally. The pack defines the required behavior and proof, but does **not** decide the agent's review cadence, local database location, container tooling, developer-machine setup, test issuer, script names, report folders or other team execution mechanics.

## Important corrections

The target already has a solution and framework. Its Domain currently references `AeroTech.Messages` and its DbContext globally maps decimal to 18,2. The Messages project reference is not automatically a defect. B0 verifies actual type usage against C1: `AeroTech.Messages.Ordering.Enums.*` plus exactly the current `BusinessContextType`, `PrincipalType` and `AuthorizationSurface` caller-context exceptions; it rejects every other Messages dependency unless explicitly approved. The reference is retained when that allowlist passes. The blanket decimal mapping still requires review against monetary/source precision semantics. AirOffer's inspected Details endpoint invokes its price pipeline, so a bare offerId cannot be treated as an immutable previously accepted price. S1 therefore resolves and displays a candidate BEFORE acceptance; Create commits the exact stored snapshot accepted by the caller and never reprices. A local snapshot digest is not an owner-issued validity guarantee.

Local ETKT/EMD issuance is the reference document-authority profile; external-authority issuance has a separate specified protocol. Production issuer/stock authorization remains a real owner decision, not something an agent approves for itself. Simulators implement the target capability contracts; real bindings must independently pass the same relevant tests.

## Reading map

- Start, priorities and checkpoints: root Markdown files and `GOVERNANCE/`.
- Actual layer placement and transaction ownership: `ARCHITECTURE/`.
- Aggregate definitions, money, documents, operations and servicing: `DOMAIN/`.
- Required capabilities versus observed wire contracts: `CONTRACTS/`.
- Incremental implementation and reproducible tests: `SLICES/`, `RUNBOOKS/`.
- Machine-readable requirements and examples: `SPEC/`, `EXAMPLES/`.
- What was reviewed and what remains unverified: `REVIEW/`, `VALIDATION.md`.

Do not treat a green document validator, a mock HTTP test or a skeleton commit as a passed live business stage.
