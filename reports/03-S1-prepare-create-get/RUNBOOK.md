# S1 — Runbook (as executed)

Environment: Windows, .NET SDK 10.0.401, `localhost\SQLEXPRESS` (Windows authentication). The automated S1 suite needs no
broker, Redis, Identity or AirOffer; the live-candidate run needs AirOffer only.

## Build and schema

```
dotnet restore
dotnet build --no-restore
# pending model changes (Development config, real adapter binding)
set ASPNETCORE_ENVIRONMENT=Development
set Providers__UseDeterministicTestAdapters=false
dotnet ef migrations has-pending-model-changes --context OrderingDbContext   --project src/AeroTech.Ordering.Persistence   --startup-project src/AeroTech.Ordering.ServiceHost
dotnet ef migrations has-pending-model-changes --context OrderQueryDbContext --project src/AeroTech.Ordering.Query         --startup-project src/AeroTech.Ordering.ServiceHost
dotnet ef migrations has-pending-model-changes --context ReferenceDbContext  --project src/AeroTech.Ordering.ReferenceData --startup-project src/AeroTech.Ordering.ServiceHost
dotnet ef database update --context OrderingDbContext   --project src/AeroTech.Ordering.Persistence   --startup-project src/AeroTech.Ordering.ServiceHost
dotnet ef database update --context ReferenceDbContext  --project src/AeroTech.Ordering.ReferenceData --startup-project src/AeroTech.Ordering.ServiceHost
dotnet ef database update --context OrderQueryDbContext --project src/AeroTech.Ordering.Query         --startup-project src/AeroTech.Ordering.ServiceHost
```

`dotnet ef` builds the host, so the design-time secrets must be present:
`Idempotency__DigestKey=<any value>` and, while `Providers:UseDeterministicTestAdapters` is true in Development,
`ConnectionStrings__DeterministicOwnerDbContext=<a local SQLEXPRESS database>`.

## Tests

```
dotnet build-server shutdown
dotnet test tests/AeroTech.Ordering.Domain.Tests      --no-build --logger "console;verbosity=normal"
dotnet test tests/AeroTech.Ordering.Persistence.Tests --no-build --filter "Category!=Live" --logger "console;verbosity=normal"
```

`Category!=Live` is the standard suite. `Persistence.Tests` creates `OrderingS1_<runid>`, `OrderingS1_<runid>_Owner` and
`OrderingS1_Upgrade_<id>` on `localhost\SQLEXPRESS` and drops exactly those (OD-S1-06).

Filtered runs (each must report a nonzero test count):

```
dotnet test tests/AeroTech.Ordering.Persistence.Tests --no-build --filter "FullyQualifiedName~.S1."
dotnet test tests/AeroTech.Ordering.Persistence.Tests --no-build --filter "FullyQualifiedName~.Api."
```

## Live AirOffer candidate run (evidence, not part of the standard suite)

```
set ORDERING_LIVE_AIROFFER_BASEURL=http://localhost:5095/Service/
set ORDERING_LIVE_AIROFFER_OFFERID=<a currently priced offer id>
dotnet test tests/AeroTech.Ordering.Persistence.Tests --no-build --filter "Category=Live" --logger "console;verbosity=detailed"
```

Transcript of the executed run: `E2E/LIVE-AIROFFER-RUN.txt`; the captured owner response is `E2E/airoffer-live-details.json`
and the same payload is replayed deterministically by `AirOfferLiveOwnerTests.Recorded_live_owner_response_is_normalized_and_prepared`.

## OpenAPI document

```
set ORDERING_OPENAPI_OUT=reports/03-S1-prepare-create-get/API/openapi.json
dotnet test tests/AeroTech.Ordering.Persistence.Tests --no-build --filter "FullyQualifiedName~OpenApiDocumentTests"
```

The test asserts the published operation inventory and, with the variable set, writes the document to `API/openapi.json`.
Manual calls: `API/requests.http`.

## Required host configuration for the S1 commands

| Key | Purpose | Default |
|---|---|---|
| `Idempotency:DigestKey` | server-side HMAC key for receipt request digests (secret) | none — fails when a command handler is first built |
| `Jwt:Authority`, `Jwt:Audience`, `Jwt:RequireHttpsMetadata` | IdentityServer bearer validation for the authenticated surfaces | none for Authority/Audience — startup fails |
| `Offer:BaseUrl`, `Offer:RequestTimeout` (s), `Offer:RetryCount`, `Offer:RetryInterval` (ms) | real AirOffer Details call | existing section |
| `Providers:UseDeterministicTestAdapters` | `true` → reference offer simulator (needs `ConnectionStrings:DeterministicOwnerDbContext`); refused in Production | existing key |
| `OrderCreation:MaxReferenceAttempts` | retries of a local Create when a generated `OrderReference` collides | `3` (in the options class) |

## Not executable in this checkpoint

- Production acceptance of an AirOffer candidate: `LIVE_ACCEPTANCE_BLOCKED(BD-001)` and the acceptance-profile policy refuse
  every sandbox/reference profile outside non-production environments.
- Real IdentityServer end-to-end: the authenticated surfaces are proven with the real JwtBearer handler and a test-only
  ephemeral RSA key (OD-S1-05); provisioning real test clients/users is separate environment evidence.
