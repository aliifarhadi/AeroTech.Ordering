# Review of the previous pack and previous failure patterns

## Material actually reviewed

The entire supplied consolidated FreshBuild pack (5,053 lines), its Agent prompt, and all four P1 audit reports were read. Selected original domain sections and selected target/AirOffer/AirPrice/FlightFlow source files were independently inspected. This is NOT a full re-audit of every old business source file or a live service test. Exact paths/hashes and review scope are in SOURCE-REGISTER.

| Finding | Evidence location in previous material | Correction in this pack | Required regression proof |
|---|---|---|---|
| Agent told to start a brand-new repo without an exact baseline | Previous AGENT-START-PROMPT; B0 | START-HERE pins actual target and audits existing shell | D0 inventory, no destructive reset/re-copy |
| Layer descriptions did not enforce actual shell boundaries | Previous 03-TECHNICAL-ARCHITECTURE | ARCHITECTURE/01 and B0 correction manifest | Dependency graph + DI tests |
| Domain list omitted important historical membership/split/fare nuances | Previous 02-DOMAIN-MODEL vs original 01 sections 5-8,15 | DOMAIN/02,03,11,12 | Price-only identity, multi-leg, split/shared-service scenarios |
| Bare offerId was presented as accepted immutable source | Previous S1 algorithm/runbook; audit G-C3 | Pre-acceptance preparation, explicit digest and authority evidence | Changed candidate never silently accepted |
| Audit called old OfferProvider a non-reprice because it only mapped a response | P1 gap G-C3; direct AirOffer ServiceFlightOfferDetailService inspection | Distinguish caller mapping from owner PriceAsync behavior | Preparation can resolve price; accepted Create performs zero owner calls |
| Query versus effect combined in EstablishOrVerifyCoverage | Previous S3/JetPay contract | Separate Establish, ReadCoverage, AcquireIssueAuthority, Release, Refund methods | Query-effect counters and crash/read-back suite |
| External issuer assumed as universal prerequisite | Previous S4/DOCUMENT-ISSUER; original document Authority supports Local/External | Explicit LOCAL-AIRLINE and EXTERNAL profiles | Local document transaction; external response-loss tests |
| CanIssue ambiguous about held vs committed capacity | Previous 16/S4 | CanStartIssue vs final CanCommitDocuments | Held scope can start, never issue before commit |
| Whole-order amount/version could invite re-funding old sales on add | Previous S3/S7 | Scoped obligations/delta coverage and application ownership | 120 sale + 20 add requests only new 20 |
| Current UI expiry-based capability could remain stale forever | Previous projection/capability text | LOCAL clock evaluation with as-of labels, command re-evaluation | Time advances without DB writes; Get capability changes safely |
| Exchange sequencing left to an unspecified later ADR even for simulator | Previous S10, BD-012 | Fixed reference phase graph + explicit pivot and cleanup | Failure before/after every step, no guessed compensation |
| Ancillary scarce-capacity sequencing left to later agent policy | Previous S6 | Sale accepts commercial package; explicit reserve then issue; capacity guarantee not implied | Capacity-free products never reserve; coupled product rules explicit |
| Worker lease and business claim not sufficiently separated | Previous external-operation protocol | Fenced worker lease, durable claim, immutable request | Two workers + late response + expired lease |
| Rejected reserve treated as reserved; replay checked after eligibility | Audit G-R1/G-R2 | Receipt-first routing and explicit resource lifecycle | Unknown resume; confirmed rejection allows new operation |
| Capacity commit missing in Issue | Audit G-R7/G-I2 | Mandatory distinct commit evidence and coupling checks | Issuer factory/adapter not called on hold-only evidence |
| Create/projector committed separately; duplicate writers | Audit G-C4/G-G1 | One local transaction and sole projector | Rollback at each save, restart GET, rebuild race |
| Funding currency/reference not checked; evidence not durable | Audit G-P4/G-P5 | Exact obligation/scope/currency/ref/authority validation | Wrong-currency/ref replay and restart |
| Blank Answer lines could be mistaken for decisions | P1-INTEGRATION-A-DECISIONS Q1-Q12 | Recommendations are historical proposals only | Owner approval required; no default revenue/TTL inference |
| Structural PASS could be read as implementation readiness | Previous PACK-VALIDATION | Separate static validation from runtime/live certification | VALIDATION explicitly lists tests not executed |
| Limited later stages omitted group/split/names from delivery map | Previous S1-S12 sequence | S13-S15 vertical slices + explicit unsupported future behavior | Coverage catalog with status for every scenario |

## What is retained

Single Order commercial truth, stable services, immutable accepted pricing, per-owner TTL, separate documents, provider ports, durable external intents, local GetOrder, transactional outbox, no Ledger issue gate and end-of-stage evidence remain intact. Fresh business implementation replaces migration of old P2/P3 code; their useful semantics are restated here without requiring that code.

## Not carried forward

Legacy coexistence, production row conversion, old API compatibility, FulfillmentTask/TrafficDocument/Payment rails and dual read-model writers are not target-domain assumptions. A later legacy cutover is an independent project with its own inventory and migration approvals, not a reason to weaken fresh-domain invariants.
