# S1 — Tables, constraints and indexes

Migrations: `OrderingDbContext` `20260917160501_S1CommercialCreate` (after B0 `20260917144148_B0InfrastructureShell`);
`OrderQueryDbContext` `20260917160511_S1OrderDetailsProjection` (first). Scripts: `ORDERING-COMMAND-MIGRATIONS.sql`,
`ORDERING-QUERY-MIGRATIONS.sql`. `has-pending-model-changes`: none for both (`migrations.txt`). All deletes `RESTRICT`.

| Table | Guard | Kind | Invariant |
|---|---|---|---|
| `Operations.CommandReceipts` | `UX_CommandReceipts_Scope_Key (OwnerAirlineId, CallerScope, CommandKind, IdempotencyKey)` | unique | INV-007 |
| | `OperationId` | unique | QRY-002 identity |
| `Commercial.OrderPreparations` | `UX_OrderPreparations_ConsumedByOrderId` filtered not null | unique | INV-008 |
| | `RowVersion` | concurrency token | INV-008 race |
| | `CK_OrderPreparations_Consumption`, `CK_OrderPreparations_Digest (LEN=64)` | check | INV-006 |
| `Commercial.PreparationSourceEvidence` | `(PreparationId, EvidenceRef)` | unique | raw evidence separate from normalized facts |
| `Commercial.Orders` | `UX_Orders_Owner_Reference (OwnerAirlineId, OrderReference)` | unique | DOMAIN/01 |
| | `UX_Orders_Owner_SourcePreparation (OwnerAirlineId, SourcePreparationId)` + FK to preparations | unique | INV-008 second guard |
| | `CK_Orders_Revisions`, `CK_Orders_Root (RootOrderId = Id)` | check | INV-054 |
| | `(OwnerAirlineId, FinancialCustomerId, CreatedAt)`, `(OwnerAirlineId, CommercialSummary)` | index | DOMAIN/13 |
| `Commercial.OrderTravelers` | `(OrderId, SourceTravellerRef)`, `(OrderId, ClientTravelerRef)` | unique | INV-011 binding |
| | `CK_OrderTravelers_Guardian` + self FK | check | no self guardian |
| `Commercial.OrderTravelerIdentities`, `Commercial.OrderContacts` | separate tables | protected payload | DOMAIN/04 |
| `Commercial.OrderSegments` / `OrderSegmentLegs` | `(OrderId, SourceSegmentRef)`, `(OrderId, Sequence)` / `(SegmentId, Sequence)` | unique | INV-010 |
| `Commercial.OrderItems` | `(OrderId, SourceItemRef)` | unique | INV-009 |
| `Commercial.OrderServices` | `(OrderId, SourceServiceRef)`; `CK_OrderServices_Quantity > 0`; `CK_OrderServices_Version >= 1`; quantity `decimal(18,6)` | unique/check | INV-009/010 |
| `Commercial.AirTransportServiceDetails` | 1:1 by `ServiceId` | registered typed detail | TD-013 |
| `Commercial.OrderServiceBeneficiaries` / `OrderServiceCoverage` | `(ServiceId, TravelerId)` / `(ServiceId, SegmentId)` | unique | INV-011 |
| `Commercial.OrderItemServiceLinks` | `(OrderItemId, OrderServiceId, LinkedByChangeId)` | unique, append-only | INV-011 history |
| `Commercial.OrderChanges` | `(OrderId, CommercialVersion)` | unique | INV-054 |
| `Commercial.PriceChangeSets` | `(OrderId, FinancialSequence)`, `ChangeId`; `CK_PriceChangeSets_Sequence >= 1` | unique/check | one set per monetary change |
| `Commercial.PricingLines` | amounts `decimal(28,8)`; `CK_PricingLines_OriginalMagnitude/SaleMagnitude >= 0`; `CK_PricingLines_TaxNotSettlement`; `CK_PricingLines_CommissionNotCustomer`; `CK_PricingLines_OtherInformational`; `CK_PricingLines_Direction`; `CK_PricingLines_ReversalReference`; `(PriceChangeSetId, CandidateLineRef)` unique | check/unique | INV-012/013/059 |
| `Commercial.FareConstructions` | `UX_FareConstructions_CurrentPerOrder` filtered `SupersededByConstructionId IS NULL` | unique | INV-015 |
| `Commercial.FundingObligations` | `CK_FundingObligations_Version >= 1`, `CK_FundingObligations_Amount >= 0`; `(OrderId, Id, Version)` unique | check/unique | DOMAIN/06 |
| `dbo.OutboxMessages` (B0) | + `StreamKind`, `StreamId`, `EventOrdinal`; `UX_OutboxMessages_Stream_EventOrdinal` filtered; `CK_OutboxMessages_Stream` | unique/check | EVENT-CATALOG ordinal. `OwnerAirlineId` is not in the key: `StreamId` is a global snowflake Order id |
| `ReadModel.OrderDetails` | PK `OrderId`; `(OwnerAirlineId, OrderReference)` unique; `CK_OrderDetails_Json (ISJSON)`; `CK_OrderDetails_Revision` | unique/check | INV-016 |

| `ReferenceData.Customers` (S1CustomerScope) | `CustomerNumber` (nvarchar(64), required, indexed), `TravelAgencyId` indexed, `Status` = Core `CustomerStatus` | index | resolves `travel_agency_id → CustomerId` and the Active check of OD-S1-02/OD-S1-10 |

Cross-row rules enforced in the serialized domain commit, not by DB: customer total = signed customer-balance lines, item
totals, obligation amounts, traveler binding completeness, guardian acyclicity, air service = one traveler × one segment.

Upgrade proof: `S1/MigrationUpgradeTests` migrates a fresh database to B0, inserts B0 outbox/inbox rows, applies S1 and
checks the rows survive with null stream columns and no pending model changes.

Protected payload tables (`Commercial.OrderTravelers`, `Commercial.OrderTravelerIdentities`, `Commercial.OrderContacts`) are
owned by `OrderingDbContext`; the Query layer maps them read-only with `ExcludeFromMigrations` so authorized surfaces can read
names/DOB/contacts without those values ever entering `ReadModel.OrderDetails`, receipts, logs or the outbox (OD-S1-04).
