# Fixture meaning and use

All files here are artificial specification examples, NOT captured real offers, document numbers, verified payment guarantees or executed test results. Fixed clock is 2026-10-01T10:00:00Z. Runtime tests must inject that clock or construct validity relative to their controlled clock; do not make these expired fixtures authoritative by changing global production time.

The normalized candidate is the target Application DTO, NOT the current AirOffer wire DTO. The real adapter maps the observed wire structure from CONTRACTS/02 and produces this semantic structure with correct source assurance. The example has one conservative package, one air service and a 100 fare + 20 tax. Its missing ticketing deadline remains explicit; later reference issuance needs a separately declared reference issuer/ticketing policy, not a fabricated AirOffer deadline.

The Create example's digest is SHA256 over normalized candidate JSON with sorted object keys, UTF-8, compact separators, exact decimal strings and semantic array order retained. This is illustrative content for local `ordering-canonical-json-v1`. In the implementation the preparation's hashed binding envelope ALSO includes its versioned authorized sales/actor scope and any profile-bound acceptance context; never trust a client-computed digest as source authority. Use the digest returned by the actual Prepare response, not the static example, when calling the live API.

Candidate schema requires registered validation of typed details; permissive `details` JSON in the transport schema does not bypass Domain validators. Structural schema cannot enforce sum/identity/coupling/authority invariants alone. Negative examples intentionally demonstrate those failures. The small document checker exercises only fixture consistency, not the full future domain implementation.

The stage-status example is intentionally NOT_STARTED/NOT_RUN and has no source commit or fake certificate. It is a shape example, not a certification report. IDs in examples are synthetic opaque source/local references; runtime local IDs use the existing platform generator and HTTP decimal strings.
