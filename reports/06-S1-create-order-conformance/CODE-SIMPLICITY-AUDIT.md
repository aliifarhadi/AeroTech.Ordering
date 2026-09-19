# Code Simplicity and Maintainability Audit

Stage: 06-S1-create-order-conformance · 2026-09-19 · HEAD `e861711`

Scope: the current S1 business path only —
`HTTP → authorization → CreateOrderFromOfferService → AirOffer ACL → CandidateValidator → Order.AcceptOriginalSale → repository/UoW → OrderProjector/outbox`.

Simple here means **fewer concepts and indirections**, never fewer guarantees. Nothing below proposes removing idempotency, source evidence, transaction atomicity, concurrency guards, PII separation, owner boundaries, projection rebuild or typed canonical facts.

## Size of the path

| File | Lines |
|---|---|
| `Domain/OrderPreparationAggregate/Serialization/NormalizedCandidateJson.cs` | 738 |
| `Providers/AirOffer/Services/AirOfferCandidateMapper.cs` | 441 |
| `Domain/OrderPreparationAggregate/Policies/CandidateValidator.cs` | 414 |
| `Domain/OrderAggregate/Order.cs` | 373 |
| `Synchronizer/OrderAggregate/OrderProjectionBuilder.cs` | 275 |
| `Query/OrderAggregate/Projection/OrderProjectionDocument.cs` | 229 |
| `Application/…/CreateOrderFromOffer/CreateOrderFromOfferService.cs` | 229 |
| `Query/OrderAggregate/Projection/OrderProjectionMapper.cs` | 104 |

## Findings

| # | File | Complexity | Why harmful | Can simplify without losing an invariant? | Minimal change |
|---|---|---|---|---|---|
| 1 | `CandidateService.Details` (`IReadOnlyDictionary<string,string>`) + `ServiceDetailSchemaRegistry.{CabinRef,RbdRef,BookingClass}` | A dynamic string dictionary carrying exactly three known core concepts, then unpacked into the typed `AirTransportDetail` by three `TryGetValue` lookups in `OrderService`'s constructor | This is the "dynamic dictionary for a known core concept" that `CLAUDE.md` forbids. Cabin, RBD and booking class are first-class, typed at rest and typed in the projection — only the candidate hop is stringly typed. The dictionary also forces `ServiceDetailSchemaRegistry` to validate *key names*, which is weaker than a compiler check, and it makes the values `string` even where the accepted record wants them typed. | **Yes.** The schema/version gate (INV-058, SC-S1-021) lives in `EnsureRegistered(type, schema, version, …)` and is independent of how the attributes are carried. | Give `CandidateService` a typed `CandidateAirTransportDetail?` (cabin, RBD, booking class) beside the existing typed baggage and sold-term members, and keep `ServiceDetailSchemaRegistry` for the `(type, schema, version)` gate only. The dictionary disappears; the registry and its invariant stay. |
| 2 | `ServiceDetailSchemaRegistry.Registered` | A registry dictionary with exactly **one** entry (`AirTransportation`, `AirTransport`, v2) | Named by the brief as "registries with only one meaningful case". | **No — keep it.** It is the only thing that makes SC-S1-021 / INV-058 fail closed, and S6 adds six more registered types. Removing it to save a dictionary would delete a guarantee. | None. Recorded as deliberately retained. |
| 3 | `NormalizedCandidateJson` (738 lines: `ToNode` writer + `Read` reader + a `Node` struct with 18 accessors) | The largest file in the path, and the writer and reader are hand-mirrored member by member | A new candidate member must be added in two places; forgetting one drifts the canonical form. | **Partly.** The mirroring is *structurally* guarded: `Node.Only(...)` requires the read key set to match exactly, so a member added to the writer and missed in the reader fails every existing round-trip test rather than silently dropping. That is a real guard, not a convention. What is unguarded is a member added to a **record** and to neither side — it would simply never be canonicalised. | Add one reflection-based test asserting every public member of `NormalizedCandidate` and its nested records appears in the canonical key set. One test file, no production change, closes the only unguarded drift path. |
| 4 | Three mirrored model families: `Candidate*` (18 records) → domain entities (11 new) → `Projected*` (19 records), with hand-written mapping between each pair | ~600 lines of read-side code and ~440 lines of mapper for one sale | Looks like triplicated truth. | **No.** Each layer answers a different question — what the owner offered, what was accepted, what a reader sees — and the Pack requires the separation (`ARCHITECTURE/01`, DOMAIN/15). Collapsing any pair would either let the wire shape leak into the accepted record or make the ratified public DTO the storage format, which is precisely the mistake the last stage had to undo. | None. Recorded so a future reader does not "simplify" it by accident. |
| 5 | Four per-surface command + handler + validator triplets under `Commands/CreateOrderFromOffer/{Backoffice,Ota,OtaPanel,Service}` | 12 small files that each build the same `CreateOrderFromOfferArgs` and call the same `ICreateOrderFromOfferService` | Mechanical duplication. | **No.** This shape is the ratified `S1-API-READABILITY-OWNER-DECISION` (explicit controller and command per surface, modelled on the historical repository). The duplication is deliberate and each triplet carries a different authorization resolution. | None. Any change here is an owner decision, not a refactor. |
| 6 | `AirOfferCandidateMapper.Map` — one method, ~120 lines, five nested loops plus the candidate construction | The single largest method in the path; it validates, reconciles and maps in one pass | Hard to read end to end; the reconciliation arithmetic is interleaved with the mapping. | **Yes, cosmetically.** The loop must stay single-pass because coupon totals reconcile against ticket totals against the root, and splitting it would either duplicate the traversal or need an intermediate accumulator. | Extract the two clearly separable bodies — `Journeys(details)` and the ticket/coupon loop's line construction — into private business-named methods, leaving `Map` as the reconciliation skeleton. No behavior change. Low priority. |
| 7 | `AirOfferCandidateMapper` enum conversion assumptions | `Categories` (string) and `CategoryValues` (`0..3`) maps, `WeightUnit` (`KG`/`LB`/`LBS`), `PassengerType` (exact `Enum.TryParse`), `RawValue(JsonElement)` | Hidden assumptions about owner vocabularies. | Already handled correctly: every one of them throws `AirOfferContractMismatchException` on an unknown value rather than defaulting, and the numeric category map is the one the Pack's own AirOffer contract records. | None. Verified, not a finding. |
| 8 | `OrderService` constructor | Builds beneficiaries, coverage, the fulfillment snapshot and the typed air detail in one ~45-line constructor | Constructors that do work are harder to read than named behaviour. | Marginal. The entity is immutable-at-construction by design, so there is no lifecycle to split it into. | None. Below the threshold worth changing. |
| 9 | `CandidateValidator` | 414 lines, 12 private `Ensure*` methods | Long, but each method is business-named and single-purpose, and the file is the single home of pre-acceptance invariants. | Already simple in the sense that matters: one concept per method, no indirection. | None. |
| 10 | `Order.AcceptOriginalSale` | 373-line file, factory split into eight private `Add*` methods | Was a single long method before the stage-04 cleanup; it is now readable. | — | None. |
| 11 | `OrderProjectionMapper.CoveredSegment` | Throws `20288` when an air service does not cover exactly one segment | Looks like defensive noise. | **No — keep it.** It is the deterministic failure the owner required instead of `FirstOrDefault()`, and it is the only thing standing between a malformed service and a silently wrong public `airTransport` block. | None. |

## Complexity that is *missing* rather than excessive

| # | Observation | Why it matters |
|---|---|---|
| 12 | `PricingLineMatrix.EnsureAllowed` validates the component × effect × direction × role matrix but **not** DOMAIN/03 §13's "SettlementOnly requires party/category/currency" — because no such fields exist | The simplest possible enforcement (a two-field check inside the method that already runs on every line) is absent, so the rule is unenforceable rather than merely unenforced |
| 13 | `AirOfferCandidateMapper.Map` validates six different shapes of source inconsistency but not the one that matters most — that the response prices the offer that was requested | One ordinal string comparison is missing from a method that already contains six `ContractMismatch` throws |

Both are recorded as defects in `REPORT.md`, not as simplifications.

## Verdict

The S1 path is **not over-abstracted**. There is one genuine unnecessary indirection (finding 1, the string detail dictionary), one cheap drift guard worth adding (finding 3), and one cosmetic method extraction (finding 6). Everything else that *looks* duplicated is a boundary the Pack requires or an owner decision already ratified.

The more interesting maintainability result is inverted: the two real problems in this path are **missing** checks, not excess machinery.
