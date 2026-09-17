# Split, group blocks and related Orders

## Split policy (S14)

An Order can contain travelers with different itineraries; difference alone does not require split. The reference DivideOrder selects whole travelers and ALL exclusively owned current service identities, including their historical consumed associations. Preserve TravelerId/ServiceId. Adult and linked infant remain together. Keep OwnerAirlineId, sale currency and FinancialCustomerId identical across source and child; changing financial customer is a separate unsupported financial transfer capability, not this split.

Reject cross-side shared hotel/transfer/pooled baggage unless an owner-supported priced partition supplies replacement scopes, quantities and value conservation. A shared service may move intact only with all its beneficiaries.

### Fixed steps

1. Claim source Order; reserve stable child OrderId, SplitTransferId and row-level mappings. Validate no other operation, dependency/coupling/guardian closure, current document ownership and source pricing partition.
2. Obtain confirmed owner reservation divide/allocation mapping and funding application transfer authority. These are external operations with original requests saved. Unknown prevents another divide or new funded action.
3. In one local SQL transaction create child, move current traveler/service ownership, partition current items, create child-local journey snapshots and explicit segment mappings, append immutable membership and many-to-many lineage. Preserve all historical OrderIdAtOccurrence and issued facts.
4. Append equal/opposite source/child SplitTransfer lines per currency/component/effect and valuation. Sum of both Orders' commercial amounts equals pre-split amount unless a SEPARATE approved charge adjustment exists. Increment source CommercialVersion once; child starts at1. Each monetary set gets its own FinancialSequence.
5. Move CurrentServicingOrderId only for complete supported document relationships. OriginalOrderId, issue amounts/numbers and used/refunded/exchanged statuses never change. Persist correlation redirects for late DCS/provider messages.
6. Publish paired source/child transfer facts with one TransferGroupId and economic reclassification identity; not refund+new sale. Finalize owner funding transfer once and reconcile pending acknowledgment without copying coverage.

Local split rollback is atomic, but external divide cannot be rolled back by SQL. If external confirmation precedes a failed local pivot, resume the SAME split with its mappings; preserve restrictions until local application. Post-pivot uncertainty is forward-recovery, not cloning the source again.

## GroupBooking (S15)

A group block of unnamed capacity is a distinct aggregate, not an Order with fictitious travelers. Group fields: GroupId/reference, owner/customer/sales context, contract/price refs, block refs, name slots, separate deadlines, deposit application refs, spawned Orders and group-local revision.

Each SeatBlock identifies flight/resource/cabin/RBD and canonical InventoryBlockRef, confirmed capacity, allocated and released quantities, owner version. For EACH block `Allocated + Released <= ConfirmedCapacity`. A traveler spanning outbound and inbound consumes one unit from EACH block; never validate against sum of both flights' capacity.

NameSlot uses stable NameSlotId, ClientPassengerRef, MaterializationRequestId, per-block assignments, protected staging name data and resulting OrderId/TravelerId. The reference materialization creates a normal fresh Order from an accepted group pricing slice and owner allocation evidence. It uses AllocateFromBlock, not another general-sale Reserve. Group deposit transfer/application is an owner-confirmed balanced movement, not copied cash.

Bulk import validates the batch and records row-level outcomes. A retry of a successful row returns the same result; one invalid passport must not create 49 extra Orders/charges on retry. Reserve IDs and claim block quantities before parallel row materialization. The bounded local Group+new Order pivot is explicitly allowed; separate rows are independent operations, not one giant 50-Order transaction.

Contractual charter/customer liability and retail traveler price are distinct pricing/effect scopes. Name materialization does not automatically charge the charter contract again. Release of unused block capacity obeys owner expiry/contract, not a local timer assuming released seats.

## Related Orders, not physical merge

`LinkRelatedOrders` records authorized shared servicing visibility with a reason and access rules, without merging money, coupons or travelers. A true commercial merge is UnsupportedCapability until priced/provider-supported merge, document control and financial application ownership are specified. Do not implement merge by copying child rows.


## Value basis after a split

The partition decision identifies exact source value portions (source pricing/allocation identity, scope and both original/sale magnitudes). Transfer-out consumes that portion's available attribution in the source; transfer-in creates the corresponding child attribution with original lineage, not a second original sale. A source Order can no longer refund transferred-out scope. A child can refund only its received remaining attribution and owner-authorized tender/application scope.

When testing a monetary reversal ceiling, subtract both already reversed value and value transferred away from that Order's applicable attribution. Do not validate only against the original unsplit price magnitude. Moving a service alone is not enough: paired value and application evidence must agree. Split and refund serialize their affected monetary attribution rows/claims; partial source allocations without a defensible partition are blocked, not equally split by the agent.

Required regression: split original120 into retained70/transferred50; source refund is capped at its remaining70 and child at50, with shared original lineage and independent original-tender constraints. Repeating either operation cannot increase the combined120 available original value. Authorized goodwill is a separate adjustment and does not expand the reversal ceiling.
