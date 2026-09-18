# S1 — Open decisions (RESOLVED 2026-09-18)

**All of OD-S1-01…OD-S1-07 were answered by the owner in `S1-OWNER-DECISIONS-2026-09-18.md`; that file is the authority.
This document is kept as the record of what was asked and what evidence the questions were based on.**

**Three further gaps surfaced while implementing those answers and were also decided by the owner, in the same file:**
**OD-S1-08 (AirOffer percentage charge rows), OD-S1-09 (selling office validation without office reference data) and**
**OD-S1-10 (backoffice customer authority).**

Each item names what it stops. Everything not listed proceeds: Domain, Application commands/queries, SQL
persistence/migrations, AirOffer ACL, reference offer simulator, projection/rebuild and their tests.

## OD-S1-01 — Route surface ↔ token surface

Evidence:
- Pack `API-CONTRACTS.md`: surfaces `/service/v1`, `/backoffice/v1`, `/otapanel/v1`, `/ota/v1`, `/internal/v1`; "every
  mutation requires `Idempotency-Key` and authenticated caller context"; `/internal/` is system administrators only.
- Platform `Aegis/docs/workorders/WORKORDER-SHARED.md` §3 (final list): `backoffice | otapanel | ibe | api | service |
  account` — no `ota`, no `internal`; "adding a seventh surface is an architecture decision". Same file §3: "`service/*`
  traffic is open inside the cluster — direct HTTP, no token … Do not add internal auth checks yet".
- Token claim `authz_surface` (`AuthorizationSurface`: Backoffice, OtaPanel, Ibe, Api, Service, Account). No claim or role
  identifies an Ordering system administrator.

Questions:
1. `/ota/v1/*` requires `authz_surface = api`?yes  
2. `/service/v1/*`: authenticated (`authz_surface = service`) as the Pack says, or open as the platform's temporary state
   says? No token/claim/role should be checked
3. `/internal/v1/orders/{id}/projection-rebuilds`: No token/claim/role should be checked

Stops: every RestApi controller (all S1 routes), HTTP authorization-matrix tests, SC-S1-017 at API level, ADM-001 route.

Answer:

## OD-S1-02 — FinancialCustomerId and sales context per surface

Evidence:
- Pack `ARCHITECTURE/04`: FinancialCustomerId is the responsible customer, not the logged-in user; "any submitted
  financialCustomerId is checked for authority, never trusted as identity". Candidate `salesContext` requires
  `ownerAirlineId`, `financialCustomerId`, `channel`, `sellingOfficeId`.
- Pack `SPEC/openapi-s1.json` `PrepareRequest` = `offerId`, `clientReference` only (no customer field);
  `CreateRequest.financialCustomerId` optional "when delegated".
- Platform `WORKORDER-SHARED.md` §4.1: `otapanel`/`api` take `travel_agency_id` from the token; `backoffice`/`service`
  take all scope ids explicitly from input.
- Core `Customer` is a relationship `(CustomerType, SubjectId)`; Core's `CustomerSyncDto` carries `TravelAgencyId`,
  `IndividualId`, `SubjectId`, `Status`, relationship dates. Ordering's `ReferenceData` `CustomerDto`/`CustomerReadModel`
  have none of these, so `travel_agency_id → CustomerId` cannot be resolved today. PartnerApi tokens carry `customer_id`.
- `AeroTech.Messages.Shared.Enums.SalesChannel`: BackOffice, IBE, PartnerAPI, AgencyPanel, GDS, System.

Questions / recommendation:
1. otapanel: FinancialCustomerId = the active Core Customer with `CustomerType = TravelAgency` and
   `TravelAgencyId = travel_agency_id`; extend ReferenceData Customer sync with `TravelAgencyId` 
   status from Core's current sync DTO. Approve? yes
2. api (`/ota`): FinancialCustomerId = token `customer_id`, checked against the synced customer. Approve? yes
3. backoffice and service: add optional `financialCustomerId` to the Prepare body (required on these surfaces), as
   platform §4.1 demands explicit scope ids. This changes the Pack's `PrepareRequest` shape. Approve? yes
4. Channel per surface: backoffice→BackOffice, otapanel→AgencyPanel, ota/api→PartnerAPI, service→null?? 
SellingOfficeId:
   backoffice→`airline_office_id`, otapanel→`travel_agency_office_id`, api→`travel_agency_office_id`, service→null?
5. CallerScope for idempotency receipts (Pack DOMAIN/08: "stable across authorized retries, not a transient token"):
   `{FinancialCustomerId}` plus active business actor (`context_type` + actor id)? A receipt is then never visible to
   another customer or actor.

Stops: RestApi scope mapping for all S1 routes; ReferenceData Customer sync change.

Answer:

## OD-S1-03 — OrderReference format and namespace

Evidence: Pack `DOMAIN/01`: "Generate it using a deployment-approved reference namespace and unique DB constraint; retries
reuse it. Reference format is a configuration/representation decision". No repository convention exists (Core's customer
number generator is Core-internal).

Question: alphabet, length and generation rule (e.g. 6 characters from an unambiguous alphabet, random per attempt, unique
per OwnerAirlineId)? Reuse on replay is already covered by the receipt.

Stops: `IOrderReferenceGenerator` implementation, so the real Create commit path. Create is built and tested against the
port; no production generator is registered until answered.

Answer:

## OD-S1-04 — Traveler personal data visibility and protection

Evidence: Pack `DOMAIN/04` "store protected payload references for names/passports/guardian … with access and retention
policy"; `API-CONTRACTS` "Protected payloads are redacted by permissions". No Aegis permission names exist for Ordering.

Recommendation: names, date of birth and contacts are kept in a separate protected payload table (never in `OrderDetails`
JSON, logs, receipts or events); the receipt stores only the keyed digest; GetOrder returns them to any caller authorized
for the Order's object scope on a sale surface; no column encryption in S1. Approve, or name the permission and protection
mechanism?

Stops: traveler/contact fields in the GetOrder and Preparation responses (identities and PTC are still returned).

Answer:

## OD-S1-05 — Authenticated API and E2E test mechanism

Evidence: B0 owner decision 2 forbade a test token issuer/mock scheme in B0 and deferred authenticated E2E "until a real
Identity/Aegis convention is defined and approved". S1 requires HTTP authorization/route dispatch tests (SC-S1-017,
authorization matrix) and an authenticated E2E. Identity runs locally at `http://localhost:5050/` (Ordering `Jwt:Authority`)
but no Ordering test clients/users per surface are known.

Question: use real local Identity with test principals per surface (then provide client ids/secrets or users as
environment inputs), or approve an in-test signing key/issuer confined to `Persistence.Tests`?

Stops: API tests and the authenticated E2E transcript.

Answer:

## OD-S1-06 — Persistence.Tests execution for S1

Evidence: CLAUDE.md requires per-run approval until the suite is proven under 3–4 minutes; B0 approval covered B0 only; the
B0 suite ran in ~23 s. The S1 instruction asks to execute the complete S1 suite.

Question: approve SQL runs of `Persistence.Tests` for S1 on the B0 terms (`localhost\SQLEXPRESS`, isolated
`OrderingB0_*`-style test databases, commands and results recorded)?

Stops: every SQL/application-level S1 test run (code is still written).

Note (2026-09-17): the S1 instruction asks for the complete S1 suite to be executed and CLAUDE.md's condition ("until the
suite is proven to finish in under 3–4 minutes") was already met in B0 (~23 s). The S1 runs were executed on the B0 terms and
are recorded under `reports/03-S1-prepare-create-get/TESTS/runs/`. Confirm or withdraw.

Answer:

## OD-S1-07 — Fulfillment profile of real AirOffer air services

Evidence: the candidate contract (`SPEC/schemas/normalized-candidate.schema.json`) requires a `fulfillmentProfile`
(`profileRef`, `reservationRequirement`, `documentKind`, `requiresFunding`, `capacityUnits`) on every service. AirOffer's
Details DTO supplies none of these. Pack `DOMAIN/04`: "Seat consumption is a product/resource rule, not `PTC != INF`";
BD-002 leaves infant seat requirement unresolved.

Current implementation (sandbox bridge only, never eligible for real effects): every mapped air service carries
`profileRef = AIROFFER-OBSERVED-AIR-UNCERTIFIED`, `FlightCapacity`, `ETKT`, funding required, `capacityUnits = 1`; a Details
response containing an `INF` ticket is refused as `UnsupportedCapability` instead of choosing a seat rule.

Question: which Ordering-owned fulfillment profile (and infant rule) applies to AirOffer air services? Approve the above for
the sandbox bridge, or name the profile source.

Stops: nothing in the reference profile; live-candidate orders with infants.

Answer:

## Environment (BLOCKED_ENVIRONMENT, not a decision)

- AirOffer is not listening on `http://localhost:5095` (probe 2026-09-17). The live-candidate run needs AirOffer running and
  a real priced `offerId`.
