# Future scenario boundaries and extension discipline

The design protects identities, monetary history, owner truth and recovery under change. It does not claim that all possible airline products or future contracts are already implemented. Every family below is either a required slice/scenario, an explicitly gated profile or a rejected capability; none can disappear behind a generic status.

| Scenario family | Structural support / required behavior | Release scope |
|---|---|---|
| One-way, round-trip, two one-ways, multi-city/open-jaw, connections | Explicit journey and source fare construction, not inferred by route shape | S1 fixtures; pricing-dependent servicing S9/S10 |
| Technical stops/change of gauge | Legs under sold passenger segment; stable coupon/service count | S1/S12 |
| Different travelers/fares/itineraries in one Order | Per-traveler service and source pricing group scope | S1/S10; no compulsory split |
| Infant without seat / infant with seat | Explicit guardian and capacity requirement | S1/S2/S13; owner mapping BD-002 |
| Seat, bag, meal, lounge, shared hotel/transfer | Typed coverage/beneficiaries, separate prices/capacity/delivery | S6/S7 |
| Through baggage / partial delivered quantity | Coverage portions and consumption occurrence identity | S6/S12 |
| EMD-A, fee/deposit/residual EMD-S | Distinct purpose and coupon association, external value truth | S4/S7 |
| Partial issue/refund/exchange and Unknown outcomes | Per-effect evidence, scoped claims, explicit pivots | S2-S10 |
| Credit/guarantee sale, mixed applications, added collection | Scoped obligations/guarantees not fake captured cash | S3/S10 |
| Multi-currency source and historical FX | Exact original/sale values and source provenance; no local conversion engine | S1/S9/S10 |
| Open dated/unassigned transport | OpenAir type, explicit issue exemption only by profile, binding/revalidation later | S13 profile-gated |
| Involuntary changes/waivers/disruption | External case authority; reuse accepted servicing coordinators | S11 |
| DCS control, offload, late/corrected consumption | Per-aspect chronology, no commercial rewrite | S12 |
| Traveler corrections and document revalidation | Explicit audited changes with profile control | S13 |
| Split after partial travel and shared services | Stable IDs, paired transfers, no double funding/capacity | S14 |
| Unnamed group/charter/allotment | Group root and canonical block allocation, not fake passenger Orders | S15 |
| Interline / partner-issued documents | Explicit issuer/operating/seller identities and External authority profile | Model can represent; production servicing requires partner contract, not automatically enabled |
| Supplier hotel/cars and complex ancillary cancellation | Registered detail/fulfillment contract + owner pricing/booking operations | Gated until real supplier capability; reference simulator tests |
| True commercial merge / different-financial-customer split | RelatedOrder links only in baseline | UnsupportedCapability; new priced/owner transfer design required |
| Multi-owning-airline pooled tenancy | Owner-keyed records reduce coupling but not full tenant isolation proof | Separate security/data migration; not v2 baseline |
| Revenue recognition/accounting settlement/BSP/interline proration | Semantic event identity and source facts only | Ledger/settlement owners; no formula engine in Ordering |
| Unlimited arbitrary new products or opaque scripts | Registered typed schema/profile with explicit capability declaration | Unknown schemas/operations rejected before acceptance |

## Extension acceptance checklist

Name the business owner, identity/granularity, beneficiaries/coverage/quantity, accepted pricing boundary and source, document/capacity/funding/delivery requirements, scoped lifecycle, effect keys/read-back, TTL owner, monetary/lineage treatment, APIs/events, persistence/migration, backward-compatible read model and positive/negative/crash/concurrency scenarios. Identify which existing invariant must stay unchanged. If changing it, file the change record before code.

Do not add an enum member and assume the old switch default safely implements a new product. Domain factories require a registered validator/profile; unsupported capability fails explicitly. No new product may bypass idempotency, quantity conservation, documentary uniqueness or owner authority because its DTO was accepted.
