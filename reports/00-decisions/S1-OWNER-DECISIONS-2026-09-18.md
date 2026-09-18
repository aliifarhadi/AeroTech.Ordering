# S1 — Owner decisions

**Status:** OD-S1-01 through OD-S1-07 resolved for S1.  
**Date:** 2026-09-18  
**Authority:** These are explicit owner decisions for S1. Where a decision below conflicts with Pack 3.8 wording, this file is the approved S1 override for that named point only. Do not generalize an override to later stages or other surfaces.

Everything not listed here continues under Pack 3.8 and `CLAUDE.md`. No new semantic, identity/cardinality, public-contract or repository-convention decision may be inferred from these answers.

---

## OD-S1-01 — Route surface ↔ token surface

### Evidence

- Pack `API-CONTRACTS.md`: surfaces `/service/v1`, `/backoffice/v1`, `/otapanel/v1`, `/ota/v1`, `/internal/v1`; every mutation normally requires `Idempotency-Key` and authenticated caller context; `/internal/` is system-administrator scope.
- Platform `Aegis/docs/workorders/WORKORDER-SHARED.md` §3 defines `backoffice | otapanel | ibe | api | service | account`; there is no `ota` or `internal` authorization-surface value. The same platform rule currently keeps `service/*` open inside the cluster and says not to add internal auth checks yet.
- Token claim `authz_surface` uses `AuthorizationSurface`: Backoffice, OtaPanel, Ibe, Api, Service, Account.
- Current `ClaimsCallerContext` reads claims only when a principal is authenticated; therefore an open `/service` or `/internal` request has no authenticated actor unless a valid token happens to be present.

### Decision — CLOSED

1. `/backoffice/v1/*`
   - Requires authentication.
   - Requires `authz_surface = backoffice`.

2. `/otapanel/v1/*`
   - Requires authentication.
   - Requires `authz_surface = otapanel`.

3. `/ota/v1/*`
   - Requires authentication.
   - Maps to the existing platform surface `api`.
   - Requires `authz_surface = api`.
   - Do **not** create a new `ota` enum/surface.

4. `/service/v1/*`
   - For the current platform phase, no token, claim or role is required or checked.
   - Do **not** add a temporary `service` JWT requirement merely to satisfy Pack wording.
   - If a valid authenticated principal is present, its caller context may be retained for audit, but authorization must not depend on it in S1.

5. `/internal/v1/orders/{id}/projection-rebuilds`
   - For the current platform phase, no token, claim or role is required or checked.
   - Do **not** invent an Ordering administrator role or seventh authorization surface.
   - The route remains an internal operational route by deployment/topology convention; this decision does not authorize exposing it externally.

6. Actor handling on temporarily unauthenticated surfaces:
   - Never fabricate `ActorId = 0`.
   - If no authenticated caller exists, actor identity is absent/null and the request surface/origin is retained as evidence.
   - Do not manufacture a user/service identity from request data.
   - This is a temporary consequence of the owner-approved open `service/internal` platform state, not a new general actor model.

### Required test matrix

- authenticated Backoffice token → backoffice routes allowed; other authenticated surfaces rejected;
- authenticated OtaPanel token → otapanel routes allowed;
- authenticated Api token → `/ota` routes allowed;
- wrong authenticated surface → rejected on authenticated sale surfaces;
- `/service` → succeeds/fails only on business/object rules, not token/claim/role presence;
- projection rebuild `/internal` → no token/claim/role gate in S1.

### Stops after this decision

None for S1 route implementation or authorization-matrix tests.

---

## OD-S1-02 — FinancialCustomerId, sales context and idempotency scope per surface

### Evidence

- Pack `ARCHITECTURE/04`: `FinancialCustomerId` is the responsible customer, not the logged-in user. Any submitted value is checked for authority and never trusted as identity.
- Candidate `SalesContext` requires `OwnerAirlineId`, `FinancialCustomerId`, `Channel`, and `SellingOfficeId` (nullable at the normalized model level).
- Platform rules: `otapanel`/`api` derive agency scope from token context; `backoffice`/`service` take operational scope explicitly.
- Core Customer is a relationship. The current Ordering ReferenceData customer projection is insufficient to resolve `travel_agency_id -> CustomerId`.
- `SalesChannel` currently contains `BackOffice`, `IBE`, `PartnerAPI`, `AgencyPanel`, `GDS`, `System`.

### Decision — CLOSED

#### 1. OwnerAirlineId

Unchanged:

- always comes from `IHomeOperatorProvider`;
- never comes from request data, token customer scope or `ICallerContext`.

#### 2. `/otapanel`

- `FinancialCustomerId` = the active synced Core Customer whose:
  - `CustomerType = TravelAgency`;
  - `TravelAgencyId = token travel_agency_id`.
- Extend the existing ReferenceData Customer sync only with fields actually required for this resolution:
  - `TravelAgencyId`;
  - active/status information if not already available.
- Do not mirror the entire Core Customer model.
- `Channel = SalesChannel.AgencyPanel`.
- `SellingOfficeId = token travel_agency_office_id`.
- Missing/invalid agency/customer/office relationship is rejected; do not substitute another customer.

#### 3. `/ota` (platform `api` surface)

- `FinancialCustomerId = token customer_id`.
- The customer must exist and be active in the synced customer reference data.
- `Channel = SalesChannel.PartnerAPI`.
- `SellingOfficeId = token travel_agency_office_id` when present and valid; otherwise `null`.
- Do not infer an office when the credential is customer-level.

#### 4. `/backoffice`

`PrepareRequest` is extended for this surface:

- `financialCustomerId` — required;
- `sellingOfficeId` — required.

Rules:

- `FinancialCustomerId` must identify an active synced customer and must be within the authenticated backoffice caller's authority.
- `SellingOfficeId` is the explicitly selected airline office scope and must be within the caller's permitted office scope.
- `Channel = SalesChannel.BackOffice`.

This is an approved S1 change from Pack 3.8's generic `PrepareRequest` shape.

#### 5. `/service`

`PrepareRequest` is extended for this surface:

- `financialCustomerId` — required;
- `sellingOfficeId` — explicitly accepted and may be `null` when the service operation is not office-scoped.

Rules:

- `FinancialCustomerId` must resolve to an active synced customer.
- A non-null `SellingOfficeId` must resolve to a valid reference office; do not invent one.
- `Channel = SalesChannel.System`.
- This does not create a new sales-channel enum.

#### 6. Create consistency

`CreateOrderFromOffer` must consume the immutable SalesContext captured by the accepted preparation.

- It must not silently replace customer/channel/office scope at Create time.
- If a delegated `financialCustomerId` is present on Create, it must match an explicitly permitted/validated delegation and the preparation semantics.
- A mismatch is rejected; it does not create a second interpretation of the same preparation.

#### 7. CallerScope for CommandReceipt

Do **not** scope idempotency by an individual user/actor ID. That would make an authorized retry by another user in the same business scope create a different receipt namespace.

Use a stable business-principal scope:

- canonical route surface;
- `FinancialCustomerId`;
- `SellingOfficeId` or an explicit `none`;
- stable organization/credential scope where the platform supplies one:
  - `TravelAgencyId` for OtaPanel;
  - `PartnerApiAccessProfileId` (or stable API client identity when that is the platform credential identity) for OTA/API;
  - no individual `AirlineUserId` / `TravelAgencyUserId`.

For `/service`, where no authenticated principal is required, the stable scope is the service surface + financial customer + explicit office/null scope.

The receipt uniqueness remains:

`(OwnerAirlineId, CallerScope, CommandKind, IdempotencyKey)`

The initiating actor, when available, is stored separately for audit and is **not** part of the receipt identity.

### Required contract changes

- Update the S1 surface-specific request DTO/OpenAPI representation so backoffice/service Prepare can carry the approved customer/office fields.
- Do not add customer/office fields to surfaces that derive them from token scope.
- Update ReferenceData Customer sync only as described above.

### Stops after this decision

None for S1 scope mapping or ReferenceData customer resolution.

---

## OD-S1-03 — OrderReference format and namespace

### Evidence

Pack `DOMAIN/01` requires a deployment-approved reference namespace and a unique DB constraint. It explicitly treats the visible format as an AeroTech representation/configuration decision, not a ticket/PNR algorithm.

### Decision — CLOSED

Use an AeroTech-owned customer-facing OrderReference with these rules:

- exactly **8 uppercase characters**;
- alphabet:

  `23456789ABCDEFGHJKLMNPQRSTUVWXYZ`

  (`0`, `1`, `I`, `O` are excluded);
- generated with a cryptographically secure random generator;
- normalized to uppercase on generation and lookup;
- no provider prefix, no check digit and no embedded business meaning;
- uniqueness is enforced by a DB unique constraint on:

  `(OwnerAirlineId, OrderReference)`;

- if a generated value collides with the unique constraint, generate another value and retry the local Create operation;
- a receipt replay never generates another reference; it returns/reuses the reference already bound to the original Order;
- provider PNR/locator values remain `ExternalReference` values and are never reused as `OrderReference`.

This gives a human-manageable reference without pretending it is an airline PNR or document number.

### Stops after this decision

None. `IOrderReferenceGenerator` may be implemented and registered.

---

## OD-S1-04 — Traveler personal data visibility and protection

### Evidence

- Pack `DOMAIN/04` requires protected payload references for names/passports/guardian/contact data and access/retention policy.
- `API-CONTRACTS` says protected payloads are redacted by permission.
- There is currently no Ordering-specific Aegis permission vocabulary for these fields.
- `/service` and `/internal` are temporarily unauthenticated by OD-S1-01, so returning PII there by default would contradict the protection requirement.

### Decision — CLOSED

Approve the proposed protected-payload approach, with the following exact S1 behavior:

1. Store names, date of birth, contacts, identity/passport/guardian details (when present) in separate protected payload storage referenced by stable IDs from the Order/Traveler model.
2. Do not place plaintext protected values in:
   - `OrderDetails` projection JSON;
   - CommandReceipt payload/result;
   - idempotency metadata;
   - logs;
   - outbox/integration events;
   - generic operation evidence.
3. Receipt/request equality uses the protected canonical request digest, not plaintext duplication.
4. S1 does **not** introduce custom column-level encryption. Use the existing SQL/environment at-rest controls. A later platform encryption mechanism may replace/augment storage without changing the domain identity.
5. Read behavior:
   - authenticated `/backoffice`, `/otapanel` and `/ota`: return protected traveler/contact data only after the caller passes the Order's object/customer scope authorization;
   - no extra fictional Aegis permission is invented in S1;
   - `/service`: protected personal fields are always redacted while the surface is unauthenticated;
   - `/internal` projection rebuild does not return protected personal payloads.
6. Prepare responses should expose the normalized traveler identity needed for acceptance (for example source traveler ref/PTC) but must not echo protected source PII unless the same protected-read rule has been satisfied.
7. No `ActorId = 0`, placeholder passport, placeholder DOB or fabricated person data is allowed.

This is an S1 access baseline, not a claim of final production privacy/security certification.

### Stops after this decision

None for traveler/contact persistence or S1 response shaping.

---

## OD-S1-05 — Authenticated API and E2E test mechanism

### Evidence

- B0 correctly did not invent a test issuer or mock authentication scheme.
- S1 now genuinely requires authenticated HTTP authorization-matrix tests for backoffice/otapanel/ota.
- Production `Framework.Presentation` uses the real ASP.NET Core JwtBearer handler with issuer, audience, lifetime, signing-key and algorithm validation.

### Decision — CLOSED

For `Persistence.Tests`, approve a **test-only ephemeral JWT signing setup** with these constraints:

1. Use the real ASP.NET Core `JwtBearer` authentication handler.
2. Do **not** introduce a fake/mock authentication scheme.
3. Generate an ephemeral RSA signing key for the test run.
4. In the test host only, post-configure JwtBearer validation with an in-memory OIDC/JWT configuration using:
   - test issuer;
   - existing Ordering test audience;
   - ephemeral signing key;
   - normal issuer/audience/lifetime/signature validation.
5. Do not change production Jwt configuration or add a production test-key path.
6. Issue test principals containing the real platform claim names needed by each surface, including:
   - `authz_surface`;
   - context/principal type;
   - customer/agency/office/API-profile claims as applicable.
7. `/service` and `/internal` tests run without requiring a token, per OD-S1-01.
8. A real local IdentityServer E2E may be added later when suitable test clients/users are provisioned, but it is **not required for S1 LOCAL_DONE**. It is separate environment/live-integration evidence.

This supersedes the B0-only prohibition because S1 now has an actual authorization behavior to test, while still exercising the real JwtBearer pipeline.

### Stops after this decision

None for S1 HTTP authorization tests or authenticated local E2E.

---

## OD-S1-06 — Persistence.Tests execution for S1

### Evidence

- B0's SQL-backed suite completed in about 23 seconds.
- S1 requires real SQL Server migration/constraint/concurrency and application-level evidence.
- S1 test runs have already been executed under the B0 isolation rules and recorded in `reports/03-S1-prepare-create-get/TESTS/runs/`.

### Decision — CLOSED

The S1 SQL test runs are approved and the runs already executed under these rules are confirmed.

For the remainder of S1:

- `Persistence.Tests` may run without asking for approval before every run;
- SQL Server is only `localhost\SQLEXPRESS`;
- use isolated S1 test databases, preferably:
  - `OrderingS1_<runid>`;
  - `OrderingS1_<runid>_Owner` for deterministic-owner state when required;
- tests may create/drop only databases matching the S1 test prefix;
- never use shared, staging or production databases;
- record the exact command and result in S1 evidence;
- if the full suite grows beyond the existing 3–4 minute threshold, stop before subsequent full-suite reruns and request approval again.

This approval is for S1. It does not automatically authorize later stages.

### Stops after this decision

None.

---

## OD-S1-07 — Fulfillment profile of real AirOffer air services

### Evidence

- S1 normalized candidate schema requires `fulfillmentProfile`.
- Current AirOffer Details does not provide reservation requirement, document kind, funding requirement or seat/capacity units as authoritative fulfillment facts.
- Pack explicitly says seat consumption is a product/resource rule, not a PTC shortcut.
- Infant seat semantics remain unresolved by an owner contract/product rule.

### Decision — CLOSED for S1; real later effects remain owner-gated

Approve the current AirOffer bridge **only as an S1 uncertified candidate profile**, with these exact limits:

- `profileRef = AIROFFER-OBSERVED-AIR-UNCERTIFIED`;
- `reservationRequirement = FlightCapacity`;
- `documentKind = ETKT`;
- `requiresFunding = true`;
- `capacityUnits = 1`;
- all of these are S1 bridge assumptions, not AirOffer owner facts.

Rules:

1. The profile must be explicitly marked uncertified/sandbox in acceptance evidence.
2. It may be used to prove:
   - real AirOffer candidate retrieval;
   - normalization;
   - Prepare;
   - explicit acceptance;
   - local Create/Get behavior.
3. It must **not** authorize:
   - S2 real reservation/capacity effects;
   - S3 funding authority;
   - S4 document issuance;
   - production certification.
4. A real AirOffer candidate containing `INF` remains `UnsupportedCapability` for this bridge. Do not infer `0` or `1` seat from PTC.
5. Do not invent a special infant profile in Ordering.
6. The deterministic/reference simulator may provide a fully authoritative fulfillment profile and may therefore continue into later simulator stages.
7. When an authoritative product/offer/inventory profile source is available, introduce the certified profile from that source. Historical accepted candidates keep the profile/evidence they were accepted with; do not silently rewrite them.
8. BD-002 remains open for the authoritative infant/resource rule and becomes relevant before real capacity/issuance behavior, not for ordinary non-INF S1 local completion.

### Stops after this decision

- Nothing for S1 reference/sandbox profile.
- Real-effect progression for uncertified AirOffer fulfillment facts remains blocked at the relevant later owner gate.
- Live AirOffer candidate Orders containing INF remain blocked.

---

## Environment — BLOCKED_ENVIRONMENT, not a business decision

- AirOffer was not listening on `http://localhost:5095` at the recorded probe.
- A real priced `offerId` and reachable AirOffer are required only for the live-candidate evidence.
- This does not block deterministic/reference S1 LOCAL_DONE.
- Do not fabricate a real offer or claim `LIVE_CANDIDATE_PROVEN` until the actual owner response is exercised.

---

## Implementation summary

After applying these decisions, S1 has no remaining BLOCKED_DECISION in OD-S1-01..07.

The implementation agent may proceed with all S1 work and tests under these decisions. It must still stop on any **new** missing semantic/identity/cardinality/public-contract decision not covered here.

The main approved Pack-3.8 S1 overrides/clarifications are:

1. `/ota` maps to Aegis `api`.
2. `/service` and S1 `/internal` projection rebuild have no token/claim/role gate in the current platform phase.
3. Backoffice/service Prepare receive explicit customer scope; service uses `SalesChannel.System`.
4. Idempotency is scoped by stable business principal/customer/office scope, not individual user identity.
5. OrderReference is an AeroTech-owned 8-character unambiguous random reference.
6. Protected PII is separate from the normal projection; unauthenticated service reads are redacted.
7. Authenticated S1 API tests use the real JwtBearer pipeline with a test-only ephemeral signing configuration.
8. S1 SQL test execution is approved under isolated local-SQL rules.
9. The current AirOffer fulfillment mapping is S1-uncertified evidence only and cannot authorize later real effects.

---

## OD-S1-08 — AirOffer percentage charge rows (raised and answered 2026-09-18)

### Evidence

Live `POST http://localhost:5095/Service/v1/FlightOffers/Details` for a real priced offer returns an order-level charge
row of the form:

```json
{ "category": 1, "name": "IR", "code": "IR", "amount": 10.0, "currencyId": 0,
  "isPercentage": true, "equivalentAmount": 24000000, "equivalentCurrencyId": 70,
  "rateOfExchangePeriodId": "70" }
```

`amount` / `currencyId` are not a monetary valuation (10 = percent, currency `0` does not exist). `equivalentAmount` is a
real amount in the sale currency and is exactly what makes the source hierarchy reconcile:
`240005012` (tickets) + `24000000` (order charge) = `264005012` (`data.totalAmount`).

The mapper previously rejected every `isPercentage = true` row as `ContractMismatch`, which would reject this real
candidate.

### Decision — CLOSED

For a row with `isPercentage = true`:

- the monetary valuation is `equivalentAmount` in `equivalentCurrencyId`, and that currency **must** equal the root sale
  currency; otherwise the row remains `ContractMismatch` (no sale-currency valuation);
- `saleValue` and `originalValue` are both that sale-currency amount — there is no other monetary original;
- the percentage basis (`amount = 10`, `isPercentage`, `reference`) is retained only in the stored raw source evidence and
  through `sourceLineRef`; the normalized candidate contract is **not** extended with percentage fields in S1;
- `rateOfExchangePeriodId` is retained as `sourceConversionRef` provenance.

Non-percentage rows keep the existing rule (Amount when its currency is the sale currency, else EquivalentAmount when its
currency matches, else ContractMismatch).

### Stops after this decision

None. The live AirOffer candidate path may be exercised.

---

## OD-S1-09 — Selling office validation in S1 (raised and answered 2026-09-18)

### Evidence

OD-S1-02 requires a non-null `SellingOfficeId` to be "within the caller's permitted office scope" (backoffice) and to
"resolve to a valid reference office" (`/service`). Ordering's `ReferenceData` has **no** office read model or syncer
(only Currency, City, Country, Airport, AirportTerminal, Airline, Customer, OperatorSettings), and `/service` carries no
token, so there is nothing to validate a service-supplied office against.

### Decision — CLOSED

Token-scoped office only; no new ReferenceData model in S1.

- `/backoffice`: the supplied `sellingOfficeId` must equal the token's `airline_office_id`; a different value, or a
  token without that claim, is rejected (403 `AuthorizedScopeRequired`).
- `/otapanel`: office is the token's `travel_agency_office_id`; a missing claim is rejected.
- `/ota`: office is the token's `travel_agency_office_id` when present, otherwise `null`; no office is inferred.
- `/service`: the supplied office is accepted as an explicit operational scope and recorded as supplied, with no
  reference check, because no office reference data exists and the surface is unauthenticated.

An authoritative office projection may replace this rule later; accepted orders keep the office they were accepted with.

---

## OD-S1-10 — Backoffice customer authority in S1 (raised and answered 2026-09-18)

### Evidence

OD-S1-02 §4 requires the backoffice `financialCustomerId` to be "within the authenticated backoffice caller's
authority". The platform token for a backoffice user carries `airline_user_id` / `airline_office_id` and no claim that
restricts which customers that user may sell for, and Ordering holds no customer/office relationship projection.

### Decision — CLOSED

For S1 the backoffice surface itself is the authority: an authenticated backoffice caller may sell for any customer that
exists and is `Active` in the synced Core customer reference data. Unknown, suspended or closed customers are rejected
(403 `AuthorizedScopeRequired`). No per-user customer restriction and no fictional Aegis permission is invented.
