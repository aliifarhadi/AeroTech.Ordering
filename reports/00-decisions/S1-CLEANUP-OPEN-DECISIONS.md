# S1 cleanup — open decisions (owner answers required)

Every item below is a decision. Items 2–9 were taken by me during the cleanup **before** the owner's instruction to stop
deciding; they are in the code now and need ratification or reversal. Item 1 is not applied: I started it and rolled it back.

Answer each line with `Answer:`; nothing moves until then.

---

## OD-C-01 — Database schema name for the order tables (NOT APPLIED, rolled back)

- Pack `DOMAIN/13-PERSISTENCE-DATA-DICTIONARY.md` line 5: "schema areas `Commercial`, `Fulfillment`, `Operations`,
  `Delivery`, `ReadModel`, `Messaging`, `Groups`, `Reference`". That is where `Commercial` came from.
- Legacy `E:\Projects\DotAir\Ordering` (main): `OrderingDbContext.HasDefaultSchema("Order")`, order tables in `Order`,
  fulfillment tasks in `Fulfillment`, inbox/outbox in `dbo`, query in `ReadModel`, reference in `ReferenceData`.
- Current state: order/preparation tables in `Commercial`, receipts in `Operations`, query in `ReadModel`, reference in
  `ReferenceData`, inbox/outbox in `dbo`.
- Options: (a) legacy convention — order tables to `Order`, receipts to `Order` as well; (b) legacy for order tables
  (`Order`) and pack for receipts (`Operations`); (c) keep the pack as is (`Commercial` + `Operations`).
- Cost of (a)/(b): regenerate the S1 migration and update the persistence evidence. No data exists yet.

**Answer (owner, 2026-09-18): order tables use schema `Order`.** Applied: every order, preparation and evidence table moved
from `Commercial` to `Order`; the S1 migration was regenerated as `20260918201810_S1OrderCreate` and applied to the dev
database. `ReadModel` (query), `ReferenceData` and `dbo` (inbox/outbox) already matched the legacy service.

### OD-C-01b — schema of `CommandReceipts` (still open)

The answer named the order tables. `CommandReceipts` currently sits in `Operations` (pack `DOMAIN/13`); the legacy service has
no receipt table, so there is no legacy precedent. It was left untouched. Options: keep `Operations`, or move it to `Order`.

Answer:`Operations`

---

## OD-C-02 — `SalesChannel` inside the Domain

`CLAUDE.md` C1 allows Domain to use only `Ordering.Enums.*` plus `BusinessContextType`, `PrincipalType`,
`AuthorizationSurface`. To replace the invented `string Channel` I used `Messages.Shared.Enums.SalesChannel` in the Domain
and added it to the allowlist test.

Answer:`Messages.Shared.Enums.SalesChannel` is allowed

---

## OD-C-03 — `PassengerSegment` added to `OrderItemUnitOfMeasure`

The pack's candidate uses `quantityUnit: "PassengerSegment"`; the shared enum had only `Each` and `PassengerFare`, so I added
the member instead of keeping a string.

Answer:

---

## OD-C-04 — Public `/operations/{id}` and `GetOperation` deleted

The owner's cleanup said the public operation lookup is not part of the S1 consumer contract. I deleted the query, its read
model and the routes entirely (the legacy service has no equivalent). The alternative was keeping it on `Internal/v1`.

Answer:

---

## OD-C-05 — Allocation and reversal policies removed

`AllocationPolicy`, `AllocationProposal`, `AllocationShare`, `PricingReversalPolicy`, `ReversibleLine` deleted per the
cleanup brief. Consequence: pack scenario SC-S1-014 and the reversal half of SC-S1-013 are no longer proven in S1.

Answer:

---

## OD-C-06 — Prepare merged into Create

`PrepareOrderFromOffer` no longer exists as a command; the preparation is captured inside `CreateOrderFromOfferService` in
the same transaction. Pack CMD-001 therefore survives only as an internal step.

Answer:

---

## OD-C-07 — Create response shape

Create returns the legacy-style small result `{ orderId, orderReference, status, grandTotal, currencyRef }` rather than the
full order document. `GET` returns the full typed `OrderDto`.

Answer:

---

## OD-C-08 — Amount rendering

Money is rendered as a normalized invariant string: `"120"`, not `"120.00"` and not a JSON number. This keeps the projection
byte-identical on rebuild.

Answer:

---

## OD-C-09 — Candidate detail fields stay pack-shaped

`detailSchema` / `detailSchemaVersion` / `details` stay on the normalized candidate because
`SPEC/schemas/normalized-candidate.schema.json` defines them and the pack example digest depends on them; the domain and the
API expose the typed `AirTransportDetail`. Making the candidate typed changes every digest.

Answer:

---

## OD-C-10 — Development configuration values

`appsettings.Development.json` (git-ignored) now has `Providers:UseDeterministicTestAdapters: false`, a local
`ConnectionStrings:DeterministicOwnerDbContext`, and `Idempotency:DigestKey = local-development-digest-key`.

Answer:
