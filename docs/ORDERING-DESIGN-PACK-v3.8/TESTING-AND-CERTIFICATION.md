# Testing and certification contract

## Test organization

Use the existing test-project topology where practical. Ensure there are tests covering architecture boundaries, application behavior, provider contracts, API behavior and E2E/recovery where the active slice requires them. This pack does not require specific test-project names. A single test can cover several invariants, but every required scenario must have explicit evidence/result.

The implementation must provide a reproducible way to execute the required stage scenarios, but this pack does **not** mandate script names, shell, container composition, SQL Server location, CI runner or evidence directory. Reuse the target repository/team's established test and launch mechanics. If none exist, propose the minimum mechanism separately rather than treating it as an Ordering domain decision. The test run must still be nonempty and must fail when a required scenario is missing or fails.

## Actual database, transaction and concurrency proof

Use real SQL Server for filtered unique claims, receipt/preparation uniqueness, child scope constraints, decimal round-trips, rowversion, explicit transactions, projection atomicity, stock range overlap, multi-worker races and split/group bounded transactions. EF InMemory/SQLite tests are supplementary, not substitutes. Each test database has an unmistakable disposable name and no production credentials. Migration tests cover both empty DB and previous stage upgrade. Test data ownership is checked before drop/reset.

## Fault injection

Implement named injection points with deterministic clock and persistent owner state: before intent commit, after intent commit, after dispatch, after owner commit with lost response, before local result commit, after local commit before HTTP response, broker acknowledgment loss, lease/fence race, stale projection rebuild, outage exceeding key retention, duplicate source identity with different payload. Restart both host and worker processes where relevant. A thrown exception in one process is not automatically equivalent to a crash/restore test.

Record expected owner-effect count and actual authoritative count. Confirmed requires exact evidence; Pending/Unknown exercise recovery rather than weakening tests. Test HTTP cancellation after dispatch; it must not erase the operation. Couple tests prove incomplete batches cannot bypass IssueGate.

## Domain proof before and through slices

S1 uses artificial OW/RT/two-OW/technical-stop/opaque/shared-pricing graphs to validate composition and arithmetic. Later stages exercise shared beneficiaries, partial consumption, quantity changes, optional document requirements, fee/value EMD-S, nested control/servicing and split/group conservation. Property tests assert signed money conservation, allocation reconciliation, unique current service membership, lineage acyclicity, no coupon duplication and idempotent replay. Include adversarial combinations: external issue Unknown with expiry, delivery during exchange, refund payout failure after pivot, group deposit transfer plus retry.

## Mutation checks for historical mistakes

A deliberately wrong test-only implementation must make the suite fail for: Create calls Details after acceptance; holds before intent save; Unknown is Rejected; receipt check follows AlreadyReserved; capacity commit omitted; guarantee counted as cash; wrong currency accepted; global decimal18,2 truncation; no projection in Create TX; reused random ticket number; cancellation completes on Unknown release; Boarded sets Used; price delta plus full replacement both posted; refund payout appends credit twice; split duplicates payment; group capacity summed across flights; second projector/handler registration.

## Live certification

Capture owner profile/version/environment, actual wire and source guarantee approval. Run shared semantic suite against both simulator and live adapter using an isolated owner sandbox, with supported fault/read-back controls. Where live owner cannot inject a fault, document the untested claim and obtain equivalent independent evidence; do not silently mark the whole suite certified. Source code/controller inspection is not a deployed behavior test.

S1 has an additional live-candidate gate because current Details reprices and omits acceptance guarantees. It proves actual candidate integration only. LIVE_CERTIFIED requires closure of relevant owner decisions; RELEASE_CERTIFIED additionally requires all enabled capability profiles, security, workload targets and restore operations.

## Structural pack validation

`tools/validate_pack.py` validates this specification's file graph, IDs, trace coverage, schema examples, command/stage references and checksum integrity. Its successful result says nothing about whether the future .NET implementation, SQL runtime or owner integrations passed. VALIDATION.md separates those evidence classes.
