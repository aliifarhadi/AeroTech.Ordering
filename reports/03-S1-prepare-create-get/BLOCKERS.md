# S1 — Blockers

## BLOCKED_DECISION — all closed

Owner answers: `reports/00-decisions/S1-OWNER-DECISIONS-2026-09-18.md` (the questions as raised are kept in
`reports/00-decisions/S1-OPEN-DECISIONS.md`).

| ID | Question | Answer applied in S1 |
|---|---|---|
| OD-S1-01 | Route surface ↔ token surface | `/backoffice`, `/otapanel` require their own `authz_surface`; `/ota` maps to platform `api`; `/service` and `/internal` have no token/claim/role gate in this platform phase; no fabricated actor |
| OD-S1-02 | FinancialCustomerId, sales context and idempotency scope per surface | customer/office from the token on `/otapanel` and `/ota`, explicit on `/backoffice` and `/service`; `CallerScope` = surface + customer + office-or-`none` + stable org/credential scope; Create consumes the preparation's sales context |
| OD-S1-03 | OrderReference format | 8 characters from `23456789ABCDEFGHJKLMNPQRSTUVWXYZ`, CSPRNG, unique per `(OwnerAirlineId, OrderReference)`, regenerate and retry on collision, replay reuses the original |
| OD-S1-04 | Traveler personal data visibility | protected payloads stay in their own tables; authenticated `/backoffice`, `/otapanel`, `/ota` read them after object scope; `/service` and `/internal` never do |
| OD-S1-05 | Authenticated API/E2E mechanism | real `JwtBearer` handler with a test-only ephemeral RSA key and the real platform claim names |
| OD-S1-06 | Persistence.Tests execution | approved; databases renamed to the `OrderingS1_` prefix |
| OD-S1-07 | AirOffer fulfillment profile | `AIROFFER-OBSERVED-AIR-UNCERTIFIED` kept as S1 evidence only; INF stays `UnsupportedCapability` |
| OD-S1-08 | AirOffer percentage charge rows | valued from `equivalentAmount` when it is in the sale currency; otherwise still `ContractMismatch` |
| OD-S1-09 | Selling office validation | token-scoped only; `/service` records the supplied office without a reference check (no office read model exists) |
| OD-S1-10 | Backoffice customer authority | any customer that is Active in the synced Core customer reference data |

## Pack blocked decisions (owner-held, unchanged by S1)

| ID | Effect in S1 |
|---|---|
| BD-001 | `LIVE_ACCEPTANCE_BLOCKED(BD-001)`: AirOffer supplies no immutable binding/validity; sandbox candidates only; `ReadBoundCandidate` unsupported for the real adapter |
| BD-002 | Infant seat requirement (see OD-S1-07): a live candidate containing `INF` is `UnsupportedCapability` |
| BD-004 | `LastTicketingDate` retained in the ticketing reason only; ticketing validity `NotSupplied` |
| BD-011 | `OrderCreated` V2 carries `EventOrdinal`/`CommercialVersion`/`FinancialSequence` in the payload; envelope binding uncertified |

## BLOCKED_ENVIRONMENT — cleared

- AirOffer is reachable on `http://localhost:5095` (2026-09-18). A real priced offer was resolved through the real adapter
  end to end: `E2E/LIVE-AIROFFER-RUN.txt`, payload `E2E/airoffer-live-details.json`.
- The Core staging customer endpoint (`https://aerotech-core.dotair.stg.agidp.ir/service/v1/Customers`) currently returns an
  empty collection, so the customer projection is proven against the Core owner contract and seeded fixtures, not against
  live customer rows.

## Open for later stages

- Real IdentityServer end-to-end (test clients/users) — not required for S1 `LOCAL_DONE` per OD-S1-05.
- An authoritative office/relationship projection would replace the token-scoped office rule of OD-S1-09.
- Production acceptance stays blocked until BD-001 is answered; S1 orders are sandbox/reference scoped.
