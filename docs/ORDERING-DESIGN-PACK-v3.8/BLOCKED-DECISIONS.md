# Decisions not established by available owner evidence

A BLOCKED_DECISION is scoped to the capability requiring that authority. It does not nullify the local reference design or justify inventing an endpoint. Empty Answer lines/recommendations in historical reports remain unanswered. No implementation agent can promote a simulator requirement into an approved production owner contract.

| ID | Exact missing authority | Required evidence to close | What may proceed | What stays blocked |
|---|---|---|---|---|
| BD-001 | AirOffer immutable acceptance/validity and customer/channel binding; current Details reprices | Owner-approved token/snapshot validation semantics, exact wire fixture, validity/source scope and changed-offer tests | S1 simulator and explicit live-candidate sandbox Create/Get | LIVE_CERTIFIED(AcceptedOffer)/real-sale enablement |
| BD-002 | FlightFlow seat requirement/infant/gender, Revenue valuation, expiry policy, reference/allotment mapping | Owner/RM-approved field meanings and examples; validated full member request | Capacity target simulator and verified wire DTO mapping code | Real Reserve writes using guessed defaults |
| BD-003 | FlightFlow authoritative effect/resource read-back, commit/release proof, key retention/concurrency/expiry safety | Owner behavior and fail/replay/race tests, distinguish not-found/expired/applied | S2/S4 complete target contract tests | Real capacity mutation/issue gate certification |
| BD-004 | TicketingDeadline ultimate source/composition and enforcement policy | Named policy owner and source evidence, timezone/scope/late-result rules | Preserve observed LastTicketingDate and separate TTL facts | Invented generic Order TTL or real ticketing deadline policy |
| BD-005 | JetPay coverage/issue/refund/transfer authority and safe recovery binding | Actual owner contract/version, authenticated wire, durability/retention/scope and value-conservation tests | Full funding/refund/exchange target simulators | Real money/credit issue/refund/transfer |
| BD-006 | Production airline issuer/stock namespace/limits/void/control authority | Airline-approved issuer profile/stock ranges and local-or-external authority decision | Local reference issuance plus external-profile simulation | Issuing real carrier documents |
| BD-007 | AirPrice immutable historic servicing decision/version/validity mappings | Owner quote/validation contract with components, scope and original-context examples | Refund/exchange/ancillary decision simulation | Real servicing based on guessed fare rules |
| BD-008 | Ancillary/supplier product profile/booking/partial/cancel mapping | Versioned owner product and supplier contracts | Typed catalog/supplier simulators and independent product slices | Real unsupported supplier fulfillment |
| BD-009 | SkyDispatch/DCS control/consumption chronology and outbound acknowledgment | Gateway contract, source correlation/version/correction rules, control reclaim proof | Delivery/control simulators and Ordering service API | Production operational/control interpretation |
| BD-010 | Disruption option authority/customer decision/deadline mapping | External case/option event and command contract, acceptance/waiver authority | Inbound target API and reference high-fanout scenarios | Guessed auto-reaccommodation/refund at deadlines |
| BD-011 | Ledger production envelope/topology/economic dedup interpretation | Owner-agreed event schema/version and posting/correlation semantics | Outbox, durable recorder and publish/replay tests | Claims of accounting/live posting certification |
| BD-012 | Group/block divide/application and partner/interline servicing capabilities | Owner contract for the particular enabled profile, value/capacity-conservation proof | S14/S15 reference profiles | Unsupported real group/split/partner mutations |
| BD-013 | Deployment-specific SLO/RPO/RTO, throughput/security approval and retention policies | Owner-approved operational values and release test evidence | Record actual benchmark/load/restore results | Production readiness claim without operating targets |

## Actual vs reference policy

Reference fixtures define artificial price decisions, stock ranges and durable coverage guarantees solely to exercise target behavior. They do not decide real tax, refund, revenue, issuer or inventory policy. Production startup rejects reference/sandbox profiles. Filling a config value locally is not evidence of owner approval.

## Blocker report format

Decision ID, affected capability/stages, exact source gap, source evidence, safe reference behavior, disabled live paths, owner/evidence needed and tests that will close it. Distinguish an unavailable credential/host (`BLOCKED_ENVIRONMENT`) from an unspecified business guarantee. Do not keep a guessed endpoint in code behind a TODO.
