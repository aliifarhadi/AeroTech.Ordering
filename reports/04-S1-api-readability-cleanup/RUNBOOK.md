# S1 cleanup — runbook

## Running the service locally

`src/AeroTech.Ordering.ServiceHost/appsettings.Development.json` is **git-ignored**, so every machine needs these keys.
A missing key fails fast at startup by design; the combination that used to break this machine was
`Providers:UseDeterministicTestAdapters: true` with no owner store.

| Key | Local value | Needed because |
|---|---|---|
| `ConnectionStrings:CommandDbContext` / `:QueryDbContext` | `Server=localhost\SQLEXPRESS;Database=DotAirOrderingVNext;Trusted_Connection=True;TrustServerCertificate=True` | order + projection databases |
| `ConnectionStrings:DeterministicOwnerDbContext` | `…;Database=DotAirOrderingVNextOwner;…` | required **whenever** `Providers:UseDeterministicTestAdapters` is `true` |
| `Providers:UseDeterministicTestAdapters` | `false` for a normal dev run against the real AirOffer | `true` replaces the AirOffer adapter with the reference simulator |
| `Idempotency:DigestKey` | any local string | the receipt request digest is keyed; no default by design |
| `Jwt:Authority` / `Jwt:Audience` / `Jwt:RequireHttpsMetadata` | `http://localhost:5050/`, `pss-api`, `false` | bearer validation for the authenticated surfaces |
| `Offer:BaseUrl` | `http://localhost:5095/service/` | real AirOffer |
| `Redis:Host`, `RabbitMq:*` | `localhost` (`guest`/`guest`) | distributed lock, bus |

```
set ASPNETCORE_ENVIRONMENT=Development
dotnet ef database update --context OrderingDbContext   --project src/AeroTech.Ordering.Persistence   --startup-project src/AeroTech.Ordering.ServiceHost
dotnet ef database update --context ReferenceDbContext  --project src/AeroTech.Ordering.ReferenceData --startup-project src/AeroTech.Ordering.ServiceHost
dotnet ef database update --context OrderQueryDbContext --project src/AeroTech.Ordering.Query         --startup-project src/AeroTech.Ordering.ServiceHost
dotnet run --project src/AeroTech.Ordering.ServiceHost          # launchSettings listens on http://localhost:5555
curl http://localhost:5555/api/v1/Ping
curl -X POST http://localhost:5555/Syncer/v1/OperatorSettings   # home operator from Core; also Customers/Currencies/Airports
```

A sale needs an **active customer** in `ReferenceData.Customers`. Core staging currently returns none, so a local dev row is
required before the first booking:

```sql
INSERT INTO ReferenceData.Customers (Id, CustomerNumber, Type, TravelAgencyId, SubjectId, SubjectName, Status, PreferredCurrencyId, LastUpdateTime)
VALUES (1, N'C1', 1, NULL, 1, N'Local Dev Customer', 1, NULL, SYSDATETIMEOFFSET());
```

Then the calls in `requests.http` work; `Service/v1/Bookings` needs no token.

## Tests

```
dotnet test tests/AeroTech.Ordering.Domain.Tests
dotnet test tests/AeroTech.Ordering.Persistence.Tests --filter "Category!=Live"
ORDERING_LIVE_AIROFFER_BASEURL=http://localhost:5095/Service/ ORDERING_LIVE_AIROFFER_OFFERID=<priced offer> \
  dotnet test tests/AeroTech.Ordering.Persistence.Tests --filter "Category=Live"
ORDERING_OPENAPI_OUT=reports/04-S1-api-readability-cleanup/openapi.json \
  dotnet test tests/AeroTech.Ordering.Persistence.Tests --filter "FullyQualifiedName~OpenApiDocumentTests"
```
