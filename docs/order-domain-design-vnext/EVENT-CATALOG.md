# Semantic integration events

Ordering-owned target schema; production Ledger topology/envelope agreement remains BD-011. EconomicRole is one of CommercialMovement, DocumentEvidence, PaymentEvidence, DeliveryEvidence, Reclassification or WorkflowCompletion. EventType supplies the specific fact. Do not infer a new charge from every documentary or completion event.

EventOrdinal is allocated monotonically per owning stream in the local fact transaction and preserved on republish. EventId identifies the fact. There is no competing streamSequence counter. Multiple facts can share one CommercialVersion. Envelope: `eventId`, `eventType`, `schemaVersion`, `ownerAirlineId`, `streamKind`, `streamId`, `eventOrdinal`, `occurredAt`, `recordedAt`, `correlationId`, `causationId`, `orderRevision`, `economicFactId`, `economicRole`, `commercialVersion`, `financialSequence`, `operationId`, `changeId`. Nullable financialSequence/operationId/changeId/economicFactId require a genuinely inapplicable role, not missing mandatory money lineage.

## EV-001 - OrderCreated

Stage: S1. EconomicRole: `CommercialMovement`.

Payload: `orderId`, `changeId`, `commercialVersion`, `acceptedSourceDigest`, `priceChangeSetId`, `financialSequence`.

Accepted original sale components; counterpart document events are documentary, not another sale.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components reference exactly one committed PriceChangeSet.

## EV-002 - TicketIssued

Stage: S4. EconomicRole: `DocumentEvidence`.

Payload: `orderId`, `ticketId`, `documentNumber`, `issuerNamespace`, `couponServiceLinks`, `documentVersion`, `issuanceAmounts`, `priceFactRefs`.

None; no new customer money.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components reference exactly one committed PriceChangeSet.

## EV-003 - EmdIssued

Stage: S4. EconomicRole: `DocumentEvidence`.

Payload: `orderId`, `emdId`, `documentNumber`, `issuerNamespace`, `purpose`, `couponAssociations`, `priceOrExternalValueRefs`.

None; no new customer money.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components reference exactly one committed PriceChangeSet.

## EV-004 - OrderScopeCancelled

Stage: S5. EconomicRole: `CommercialMovement`.

Payload: `orderId`, `changeId`, `serviceIds`, `priceChangeSetRef`, `commercialVersion`, `releaseEvidenceRefs`.

Only appended cancellation lines; no second reversal on inventory/funding release.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components reference exactly one committed PriceChangeSet.

## EV-005 - OrderServicesAdded

Stage: S6. EconomicRole: `CommercialMovement`.

Payload: `orderId`, `changeId`, `serviceIds`, `sourceProductRefs`, `priceChangeSetId`, `obligationRefs`, `commercialVersion`.

New accepted ancillary commercial value once.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components reference exactly one committed PriceChangeSet.

## EV-006 - TicketVoided

Stage: S8. EconomicRole: `DocumentEvidence`.

Payload: `orderId`, `ticketId`, `documentVersion`, `voidEvidenceRef`, `commercialChangeRef`.

None; no new customer money.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components reference exactly one committed PriceChangeSet.

## EV-007 - EmdVoided

Stage: S8. EconomicRole: `DocumentEvidence`.

Payload: `orderId`, `emdId`, `documentVersion`, `voidEvidenceRef`, `commercialChangeRef`.

None; no new customer money.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components reference exactly one committed PriceChangeSet.

## EV-008 - RefundAuthorized

Stage: S9. EconomicRole: `CommercialMovement`.

Payload: `orderId`, `refundId`, `changeId`, `priceChangeSetId`, `commercialVersion`, `refundAuthorityRef`, `payoutPlan`.

Accepted commercial credit/penalty once, before confirmed payout; outstanding refund remains visible.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components reference exactly one committed PriceChangeSet.

## EV-009 - RefundConfirmed

Stage: S9. EconomicRole: `PaymentEvidence`.

Payload: `orderId`, `refundId`, `originalChangeId`, `ownerMovementRefs`, `confirmedPayoutAmounts`, `completedAt`.

Payment/value movement confirmation, NOT another commercial credit.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components reference exactly one committed PriceChangeSet.

## EV-010 - ExchangeApplied

Stage: S10. EconomicRole: `CommercialMovement`.

Payload: `orderId`, `exchangeId`, `changeId`, `commercialVersion`, `oldNewLineage`, `priceChangeSetId`, `documentRefs`.

Accepted old/new delta once; never sum delta and full replacement.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components reference exactly one committed PriceChangeSet.

## EV-011 - ReissueCompleted

Stage: S10. EconomicRole: `DocumentEvidence`.

Payload: `orderId`, `exchangeId`, `originalDocumentRefs`, `newDocumentRefs`, `originalChangeId`.

None; no new customer money.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components reference exactly one committed PriceChangeSet.

## EV-012 - ExchangeCompleted

Stage: S10. EconomicRole: `WorkflowCompletion`.

Payload: `orderId`, `exchangeId`, `originalChangeId`, `cleanupEvidenceRefs`.

Completion of the original accepted exchange, not a second sale.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components reference exactly one committed PriceChangeSet.

## EV-013 - ServiceDelivered

Stage: S12. EconomicRole: `DeliveryEvidence`.

Payload: `orderIdAtOccurrence`, `currentOrderId`, `serviceId`, `observationId`, `observationKey`, `aspect`, `portion`, `quantity`, `sourceVersion`.

Ledger decides recognition from certified delivery fact; boarding alone is not delivered.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components reference exactly one committed PriceChangeSet.

## EV-014 - ServiceDeliveryCorrected

Stage: S12. EconomicRole: `DeliveryEvidence`.

Payload: `originalObservationId`, `correctingObservationId`, `serviceId`, `aspect`, `sourceRevision`, `reason`.

Correct the referenced recognition fact; not another delivery.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components reference exactly one committed PriceChangeSet.

## EV-015 - TravelerCorrected

Stage: S13. EconomicRole: `CommercialMovement`.

Payload: `orderId`, `changeId`, `travelerId`, `commercialVersion`, `protectedChangeRef`.

No monetary value unless a separate accepted price change exists.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components reference exactly one committed PriceChangeSet.

## EV-016 - OrderSplitApplied

Stage: S14. EconomicRole: `Reclassification`.

Payload: `splitTransferId`, `sourceOrderId`, `childOrderId`, `pairedChangeIds`, `serviceOwnershipMap`, `pairedPriceTransferRefs`, `applicationTransferRef`.

Balanced reclassification; not refund plus new sale.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components reference exactly one committed PriceChangeSet.

## EV-017 - GroupNameMaterialized

Stage: S15. EconomicRole: `WorkflowCompletion`.

Payload: `groupId`, `rowIdentity`, `childOrderId`, `blockAllocationRefs`, `priceDispositionRef`, `depositApplicationRef`.

Apply existing group/deposit commercial treatment; do not charge charter contract again.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components reference exactly one committed PriceChangeSet.

Read committed source snapshots rather than fetching current owners to reconstruct past facts. A corrective fact receives a new EventId and explicit original identity, never a rewritten earlier event.
