# Independently observed target baseline

Repository: `aliifarhadi/AeroTech.Ordering`; branch `k8s-stg`; commit `2f2b9033f675387f68ef7ff95c9d0a575d836fb5`.

Observed bootstrap message: `Bootstrap Ordering vNext skeleton from Ordering k8s-stg`. The solution's project list is reproduced in ARCHITECTURE/01. The Domain and Framework.Core projects target net10.0.

Directly inspected facts:
- Domain.csproj references Framework.Core and Contracts/AeroTech.Messages. The project reference itself is allowed. Current Domain type usage includes Ordering-owned enums plus the existing `ICallerContext` platform identity types `BusinessContextType`, `PrincipalType` and `AuthorizationSurface`; B0 must enforce this exact allowlist and reject integration-event/provider-wire/base-transport or other Messages types unless explicitly approved.
- OrderingDbContext currently derives from generic CommandDbContext, implements IUnitOfWork, exposes Inbox/Outbox and globally configures decimal(18,2), string(256).
- InboxConsumeFilter bypasses inbox dedup if MessageId is absent, enlists a marker before invoking the next consumer, then persists it; its DbUpdateException catch checks whether a marker exists. Atomicity and exception classification require the B0 tests/corrections.
- InboxStore uses the same OrderingDbContext, tracks an enlisted marker and has methods that call SaveChanges. Whether that is safe depends on the calling phase and explicit transaction.
- Program.cs is a composition root chaining generic infrastructure, persistence, providers, deterministic-provider activation, query, synchronizer, consumers, application, reference data and presentation.
- Targeted drift check on `k8s-stg@957757471d9db3fde91922142d537a559b8084e4`: `src/AeroTech.Ordering.Providers.Deterministic` contains project/options/activation/composition shell, and `AddDeterministicProviders` returns the service collection without registering business adapters. Treat it as an approved location/seam, not an existing simulator implementation.

- Current `ICallerContext` imports `AeroTech.Messages.Aegis.Enums` and `AeroTech.Messages.Shared.Enums` and exposes `BusinessContextType`, `PrincipalType` and `AuthorizationSurface`; these three types are the only non-Ordering Messages exceptions approved by this pack.
- `IHomeOperatorProvider.GetOwnerAirlineIdAsync` remains the owner-airline authority seam, currently implemented in ServiceHost by `ReferenceDataHomeOperatorProvider` over ReferenceData operator settings. `ICallerContext` is not the owner-airline source.

No build, DB migration, host run, production data inspection or full framework correctness certification was performed here. Existing code outside the listed files is not declared defective merely by association. B0 inventories and certifies it rather than discarding it.
