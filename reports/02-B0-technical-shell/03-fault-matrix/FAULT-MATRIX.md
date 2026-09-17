# B0 — Fault matrix

All rows run on real SQL Server (`localhost\SQLEXPRESS`, isolated database). Test classes are under
`tests/AeroTech.Ordering.Persistence.Tests`. "Red proof" = the test fails when the repair is reverted
(`MUTATION-RUNS.txt`).

| # | Fault injected | Expected | Test | Result | Red proof |
|---|---|---|---|---|---|
| F1 | Message arrives without `MessageId` | rejected with `InboxMessageIdentityMissingException`, consumer not run, no marker | `Inbox/InboxConsumeFilterOutcomeTests.Message_without_source_identity_is_rejected_before_the_consumer_runs` | pass | M1 |
| F2 | Consumer crashes before commit (real MassTransit in-memory pipeline, scoped filter) | no marker, no work; redelivery commits both once; third delivery skipped | `Inbox/InboxConsumeFilterTests.Business_crash_commits_neither_marker_nor_work_and_redelivery_commits_both_once` | pass | — |
| F3 | Marker + work | both inserted by the same `SaveChanges` | `…OutcomeTests.Marker_and_work_commit_in_one_save` | pass | — |
| F4 | Unrelated unique-key violation inside the consumer | `DbUpdateException` rethrown, no marker, fault published | `…OutcomeTests.Unrelated_sql_failure_is_rethrown_and_commits_nothing`, `…FilterTests.Unrelated_sql_failure_is_not_classified_as_a_duplicate` | pass | M4 |
| F5 | Concurrent instance commits the same marker first | loser discarded, its work not committed, one marker | `…OutcomeTests.Concurrent_duplicate_marker_is_discarded_without_committing_the_losing_work`, `…FilterTests.Concurrent_duplicate_marker_discards_the_losing_delivery_without_committing_its_work` | pass | — |
| F6 | Only `PK_InboxMessages` violation counts as duplicate | classifier true for marker key, false for any other key | `…FilterTests.Classifier_accepts_only_the_inbox_key_violation` | pass | — |
| F7 | Local transaction rolled back after outbox write | no outbox row; committed write carries the domain `EventId` in row and payload | `Outbox/OutboxDispatcherTests.Outbox_fact_is_committed_or_rolled_back_with_the_local_transaction` | pass | — |
| F8 | Same `EventId` written twice | unique index rejects | `…Same_event_identity_cannot_be_written_twice` | pass | — |
| F9 | Broker accepts, acknowledgement lost, publisher restarts | row stays pending with `LastError`; republished after lease expiry with identical `MessageId`, `EventId`, payload; receiver inbox commits one fact | `…Lost_acknowledgement_republishes_the_same_identity_and_payload_and_the_receiver_commits_one_fact` | pass | M3 |
| F10 | Two publishers poll concurrently (40 rows, batch 7) | every row published exactly once, none pending | `…Two_publishers_never_hold_the_same_row` | pass | — |
| F11 | Lease expires, second publisher claims, first tries to complete / record failure | stale holder affects 0 rows (fencing by owner + `LeaseVersion`) | `…Expired_lease_holder_cannot_complete_a_row_claimed_by_another_publisher` | pass | M2, M3 |
| F12 | Row whose `EventId` cannot yield a stable `MessageId` | not published, `AttemptCount`/`LastError` recorded, later rows still published | `…Unusable_row_is_recorded_as_failed_and_does_not_block_usable_rows` | pass | — |
| F13 | Ordering transaction rolled back after a simulator owner effect; provider rebuilt | owner effect still readable from its own database | `Deterministic/DeterministicEffectStoreTests.Owner_effect_survives_an_ordering_rollback_and_a_restart` | pass | — |
| F14 | Same owner key, different request hash | `DeterministicEffectConflictException`; same hash replays first result | `…Same_key_and_hash_replays_and_a_different_hash_conflicts` | pass | — |
| F15 | Two `IUnitOfWork` / real + deterministic binding of one contract | startup guard throws naming service and both implementations | `Composition/SingleRegistrationGuardTests` | pass | — |
| F16 | Deterministic flag true without its own connection string; broker config missing | host composition fails closed, naming the key | `Composition/HostCompositionTests` | pass | — |

Not proven in B0: ordered publication per stream (`StreamKind/StreamId/EventOrdinal`, DOMAIN/13) — no stream exists
before S1; real RabbitMQ publisher-confirm loss (the transport seam is faked, the SQL side is real).
