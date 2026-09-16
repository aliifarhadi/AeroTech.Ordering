# Independently observed target baseline

Repository: `aliifarhadi/AeroTech.Ordering`; branch `k8s-stg`; commit `2f2b9033f675387f68ef7ff95c9d0a575d836fb5`.

Observed bootstrap message: `Bootstrap Ordering vNext skeleton from Ordering k8s-stg`. The solution's project list is reproduced in ARCHITECTURE/01. The Domain and Framework.Core projects target net10.0.

Directly inspected facts:
- Domain.csproj references Framework.Core AND Contracts/AeroTech.Messages. The latter conflicts with the target boundary.
- OrderingDbContext currently derives from generic CommandDbContext, implements IUnitOfWork, exposes Inbox/Outbox and globally configures decimal(18,2), string(256).
- InboxConsumeFilter bypasses inbox dedup if MessageId is absent, enlists a marker before invoking the next consumer, then persists it; its DbUpdateException catch checks whether a marker exists. Atomicity and exception classification require the B0 tests/corrections.
- InboxStore uses the same OrderingDbContext, tracks an enlisted marker and has methods that call SaveChanges. Whether that is safe depends on the calling phase and explicit transaction.
- Program.cs is a composition root chaining generic infrastructure, persistence, providers, deterministic providers, query, synchronizer, consumers, application, reference data and presentation.

No build, DB migration, host run, production data inspection or full framework correctness certification was performed here. Existing code outside the listed files is not declared defective merely by association. B0 inventories and certifies it rather than discarding it.
