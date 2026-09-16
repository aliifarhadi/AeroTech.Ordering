# Nonnegotiable implementation rules

These are USER_BINDING unless a narrower target detail is identified below.

1. One accepted commercial truth in Order; no Entitlement v1 and no global mutable Paid/Ticketed/Flown Order state machine.
2. An OrderItem follows an explicitly accepted pricing boundary. It is not automatically one passenger, segment, ticket or ancillary category.
3. OrderService has stable identity, typed details, explicit beneficiaries and coverage. Flight replacement creates successors; price-only change does not duplicate delivery identity.
4. Original pricing, fare construction, sold schedule, accepted terms and document issue facts are immutable. Corrections append; operational facts do not rewrite sales.
5. Inventory is FlightFlow truth; funding/payment execution is JetPay truth; ledger entries and balances are Ledger truth. Ordering stores evidence and its own commercial obligations, not competing balances or capacity.
6. GetOrder reads only local denormalized data. A command re-evaluates canonical eligibility; UI summaries never authorize mutations.
7. Every public mutation has scoped durable idempotency. Replay is resolved before new-operation eligibility checks. Same key plus different semantic payload is conflict.
8. Persist remote mutation intent and exact request identity before dispatch. No remote call, broker publish, callback wait or retry sleep inside a SQL transaction.
9. Unknown is not Failed, Rejected, Absent or Confirmed. Recover the same operation; never invent a new key/number to bypass uncertainty.
10. Per-target evidence and dependency coupling survive partial outcomes. An aggregate success flag cannot hide an unknown member.
11. Capacity commitment, valid funding authority and document eligibility are required before document issue. Ledger is never a synchronous gate.
12. CommercialVersion increments once per committed commercial change, not per event, database save, polling result or fulfillment fact.
13. Command-side mutation, deterministic local projection, receipt finalization and outbox facts commit together at each stated local boundary.
14. Service-to-service uses `/service/`; `/internal/` is privileged administration only. Surface wrappers do not create separate mutation rails.
15. Work on the actual target repository and existing layer shell. Copy no previous Order/business code, business EF mappings/migrations or business tests.
16. Simulators have independent durable owner stores and target-contract behavior. They cannot share Ordering's transaction or mutate its database. No silent fallback from real to simulator.
17. Retain provider profile/contract version on every operation and resource. Switching DI configuration does not move old simulated or real resources to a new provider.
18. Each stage must start and execute end to end. Unit tests, source inspection and an Agent report are not runtime certification.
19. Expiries have independent owners. A local cache retention limit is never OfferExpiresAt, PriceValidUntil, HoldExpiresAt or TicketingDeadline.
20. Unspecified owner behavior is BLOCKED_DECISION. A proposed option or blank approval in an old report is not an approved contract.
21. No speculative generalized workflow engine, universal document aggregate, cross-service shared business database or shared domain assembly.
22. No lowering acceptance gates, disabling tests or silently changing the pack to get a green build. A semantic change needs an explicit decision, impacted requirements, updated examples and regression tests.
