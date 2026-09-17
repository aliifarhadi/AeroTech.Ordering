# Pack manifest - version 3.8

Target: `aliifarhadi/AeroTech.Ordering`. Reviewed bootstrap: `2f2b9033f675387f68ef7ff95c9d0a575d836fb5`. Start with `README.md`, `START-HERE.md`, `GOVERNANCE/05-STANDING-RULE-CONFLICT-RESOLUTION.md` and `AGENT-START-PROMPT.md`.

This is a self-contained **Ordering domain/architecture specification**. It is authoritative for business semantics, invariants, service ownership, layer responsibilities and transaction/recovery semantics. It is not authority for developer-machine/environment choices or team-process mechanics. Explicit prior user/repository conventions are preserved as documented in Governance 05.

## Inventory

- `AGENT-START-PROMPT.md`
- `API-CONTRACTS.md`
- `ARCHITECTURE/01-LAYERS.md`
- `ARCHITECTURE/02-BOOTSTRAP.md`
- `ARCHITECTURE/03-TRANSACTIONS-AND-PROJECTION.md`
- `ARCHITECTURE/04-SECURITY-AND-OPERATIONS.md`
- `BLOCKED-DECISIONS.md`
- `CONTRACTS/00-INTERACTION-CATALOG.md`
- `CONTRACTS/01-COMMON-PROTOCOL.md`
- `CONTRACTS/02-AIROFFER.md`
- `CONTRACTS/03-AIRPRICE.md`
- `CONTRACTS/04-FLIGHTFLOW.md`
- `CONTRACTS/05-JETPAY.md`
- `CONTRACTS/06-ANCILLARY-AND-SUPPLIERS.md`
- `CONTRACTS/07-SKYDISPATCH-DISRUPTION.md`
- `CONTRACTS/08-LEDGER.md`
- `CONTRACTS/09-DOCUMENT-AUTHORITY.md`
- `CONTRACTS/10-SIMULATOR-SPECIFICATION.md`
- `CONTRACTS/11-TYPED-PORT-INDEX.md`
- `CONTRACTS/12-REFERENCE-DATA-AND-CUSTOMER.md`
- `DOMAIN/01-AGGREGATES.md`
- `DOMAIN/02-COMMERCIAL-COMPOSITION.md`
- `DOMAIN/03-PRICING-AND-FARE-CONSTRUCTION.md`
- `DOMAIN/04-TRAVELERS-JOURNEYS-AND-PRIVACY.md`
- `DOMAIN/05-RESERVATION-AND-CAPACITY.md`
- `DOMAIN/06-FUNDING-OBLIGATIONS.md`
- `DOMAIN/07-DOCUMENTS-AND-STOCK.md`
- `DOMAIN/08-OPERATIONS-IDEMPOTENCY-AND-RECOVERY.md`
- `DOMAIN/09-SERVICING-CHOREOGRAPHIES.md`
- `DOMAIN/10-ELIGIBILITY-VERSIONS-AND-TIME.md`
- `DOMAIN/11-DELIVERY-AND-DISRUPTION.md`
- `DOMAIN/12-SPLIT-GROUPS-AND-RELATED-ORDERS.md`
- `DOMAIN/13-PERSISTENCE-DATA-DICTIONARY.md`
- `DOMAIN/14-FUTURE-SCENARIO-BOUNDARIES.md`
- `DOMAIN/15-EVIDENCE-TEMPORAL-CONSISTENCY.md`
- `EVENT-CATALOG.md`
- `EXAMPLES/README.md`
- `EXAMPLES/capacity-effect-confirmed.json`
- `EXAMPLES/capacity-effect-unknown.json`
- `EXAMPLES/create-request.json`
- `EXAMPLES/negative/confirmed-without-resource.json`
- `EXAMPLES/negative/incorrect-total.json`
- `EXAMPLES/negative/missing-beneficiary.json`
- `EXAMPLES/negative/settlement-tax.json`
- `EXAMPLES/normalized-candidate.json`
- `EXAMPLES/pricing-ledger.json`
- `EXAMPLES/stage-status-not-executed.json`
- `GOVERNANCE/01-NONNEGOTIABLES.md`
- `GOVERNANCE/02-DECISIONS.md`
- `GOVERNANCE/03-STAGE-GATES.md`
- `GOVERNANCE/04-NO-DEVIATION-PROTOCOL.md`
- `GOVERNANCE/05-STANDING-RULE-CONFLICT-RESOLUTION.md`
- `IMPLEMENTATION-INSTRUCTIONS.md`
- `LEGACY-CUTOVER-OUT-OF-SCOPE.md`
- `MANIFEST.md`
- `README.md`
- `REVIEW/01-PREVIOUS-PACK-REVIEW.md`
- `REVIEW/02-TARGET-BASELINE.md`
- `REVIEW/03-SOURCE-REGISTER.md`
- `REVIEW/04-EXTERNAL-REFERENCES.md`
- `REVIEW/05-PACK-2.1-SCOPE-CORRECTION.md`
- `REVIEW/source-fingerprints.json`
- `RUNBOOKS/00-COMMON.md`
- `RUNBOOKS/B0.md`
- `RUNBOOKS/D0.md`
- `RUNBOOKS/S1.md`
- `RUNBOOKS/S10.md`
- `RUNBOOKS/S11.md`
- `RUNBOOKS/S12.md`
- `RUNBOOKS/S13.md`
- `RUNBOOKS/S14.md`
- `RUNBOOKS/S15.md`
- `RUNBOOKS/S16.md`
- `RUNBOOKS/S2.md`
- `RUNBOOKS/S3.md`
- `RUNBOOKS/S4.md`
- `RUNBOOKS/S5.md`
- `RUNBOOKS/S6.md`
- `RUNBOOKS/S7.md`
- `RUNBOOKS/S8.md`
- `RUNBOOKS/S9.md`
- `SCENARIO-CATALOG.md`
- `SLICES/B0.md`
- `SLICES/D0.md`
- `SLICES/S1.md`
- `SLICES/S10.md`
- `SLICES/S11.md`
- `SLICES/S12.md`
- `SLICES/S13.md`
- `SLICES/S14.md`
- `SLICES/S15.md`
- `SLICES/S16.md`
- `SLICES/S2.md`
- `SLICES/S3.md`
- `SLICES/S4.md`
- `SLICES/S5.md`
- `SLICES/S6.md`
- `SLICES/S7.md`
- `SLICES/S8.md`
- `SLICES/S9.md`
- `SPEC/README.md`
- `SPEC/SCHEMA-SCOPE.md`
- `SPEC/commands.json`
- `SPEC/events.json`
- `SPEC/interactions.json`
- `SPEC/invariants.json`
- `SPEC/layers.json`
- `SPEC/negative-fixtures.json`
- `SPEC/openapi-s1.json`
- `SPEC/ports.json`
- `SPEC/pricing-example-vectors.json`
- `SPEC/scenarios.json`
- `SPEC/schemas/capacity-effect-result.schema.json`
- `SPEC/schemas/create-request.schema.json`
- `SPEC/schemas/normalized-candidate.schema.json`
- `SPEC/schemas/stage-status.schema.json`
- `SPEC/stages.json`
- `START-HERE.md`
- `TESTING-AND-CERTIFICATION.md`
- `VALIDATION.md`
- `VERTICAL-SLICE-PLAN.md`
- `tools/requirements-validation.txt`
- `tools/validate_pack.py`

`SHA256SUMS` is generated during packaging and covers every other file. Companion ZIP, consolidated Markdown and separate Agent prompt are delivered outside this root.

## Requirement counts

18 stages; 60 invariants; 149 acceptance scenarios; 31 canonical command/query contracts; 10 semantic ports; 17 event types; 52 expanded interactions. These are design requirements, not executed runtime tests.

## Structural validation

```text
python tools/validate_pack.py .
```

The validator checks document/spec consistency only. It does not certify .NET build, SQL runtime, owner integrations or production readiness.
