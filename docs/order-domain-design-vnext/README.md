# AeroTech.Ordering - Day 0 Design Pack 2.0

**Target repository:** https://github.com/aliifarhadi/AeroTech.Ordering  
**Observed branch:** `k8s-stg`  
**Reviewed bootstrap commit:** `2f2b9033f675387f68ef7ff95c9d0a575d836fb5`  
**Issued:** 2026-09-16  
**Artifact:** implementation specification, not implemented software or production certification.

## Mission

Build a fresh airline Ordering domain and application on the technical shell already in the target repository. Preserve the owner's business boundaries, not the previous implementation's mistakes. This pack replaces the previous `ORDERING-VNEXT-FRESHBUILD-DESIGN-PACK` as the implementation instruction set. No other design pack or business repository is required to understand the target.

Start at [START-HERE.md](START-HERE.md), then execute the separately supplied [AGENT-START-PROMPT.md](AGENT-START-PROMPT.md). Do not create another repository. Do not recopy old business code. The current shell is an input to certify, not a certified architecture.

## Authority and honesty

`USER_BINDING` marks the user's established requirements. `TARGET_DECISION` is a concrete design choice made in this revision for implementation. `OBSERVED_SOURCE` describes inspected code, not deployed behavior. `OWNER_REQUIRED` defines an external capability Ordering needs; it does not assert the external owner implements it. `BLOCKED_DECISION` means a required authoritative decision is missing. These categories are never interchangeable.

The pack defines a complete reference implementation path and explicit extension boundaries. It cannot guarantee correctness for every unspecified future business rule. The guarantee to enforce is narrower: no unsupported scenario is silently accepted, no original fact is erased, and an extension has a named owner, invariant, compatibility change and test. See [DOMAIN/14-FUTURE-SCENARIO-BOUNDARIES.md](DOMAIN/14-FUTURE-SCENARIO-BOUNDARIES.md).

## Sequence and visible results

`D0 inventory + design conformance -> B0 certify/repair shell -> S1 real-offer preparation/acceptance/Create/Get -> S2 reservation -> S3 funding -> S4 issuance -> S5 cancellation -> S6 ancillary sale -> S7 EMD extensions -> S8 void -> S9 refund -> S10 exchange/reissue -> S11 disruption -> S12 DCS -> S13 traveler/revalidation servicing -> S14 split -> S15 groups -> S16 release certification`.

D0 is a short design/source checkpoint. B0 is a running technical service. Every S-stage is an executable vertical slice: API, application, domain, SQL migrations, local projection, recovery where applicable, tests and an actual evidence bundle. Publish that bundle at the end of the stage, not at the end of the project. Proceed automatically only after the applicable stage gate.

## Important corrections

The target already has a solution and framework. Its Domain currently references integration Messages and its DbContext globally maps decimal to 18,2; B0 has explicit corrections. AirOffer's inspected Details endpoint invokes its price pipeline, so a bare offerId cannot be treated as an immutable previously accepted price. S1 therefore resolves and displays a candidate BEFORE acceptance; Create commits the exact stored snapshot accepted by the caller and never reprices. A local snapshot digest is not an owner-issued validity guarantee.

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
