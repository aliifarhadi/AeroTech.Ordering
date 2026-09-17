# Semantic integration events

Ordering-owned target schema; production Ledger topology/envelope agreement remains BD-011. EconomicRole is one of CommercialMovement, DocumentEvidence, PaymentEvidence, DeliveryEvidence, Reclassification or WorkflowCompletion. EventType supplies the specific fact. Do not infer a new charge from every documentary or completion event.

EventOrdinal is allocated monotonically per owning stream in the local fact transaction and preserved on republish. EventId identifies the fact. There is no competing streamSequence counter. Multiple facts can share one CommercialVersion. **Transport/base-envelope representation follows the existing `AeroTech.Messages` convention and must not be replaced by this pack. `SPEC/events.json` intentionally does not enumerate shared envelope fields.** Ordering events must nevertheless carry or unambiguously bind the semantic identities required to interpret the fact: event identity/type/schema version, owning Order/stream identity, EventOrdinal, occurrence/recording time as supported by the platform, CommercialVersion, and FinancialSequence/OperationId/ChangeId/economic-fact lineage when applicable. Exact shared-envelope binding remains part of BD-011 / platform contract certification.

## EV-001 - OrderCreated

Stage: S1. EconomicRole: `CommercialMovement`.

Payload: `orderId`, `changeId`, `commercialVersion`, `acceptedSourceDigest`, `priceChangeSetId`, `financialSequence`.

Accepted original sale components; counterpart document events are documentary, not another sale.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components, when present, reference the single committed PriceChangeSet for this commercial change.

## EV-002 - TicketIssued

Stage: S4. EconomicRole: `DocumentEvidence`.

Payload: `orderId`, `ticketId`, `documentNumber`, `issuerNamespace`, `couponServiceLinks`, `documentVersion`, `issuanceAmounts`, `priceFactRefs`.

None; no new customer money.

Snapshot actual committed document facts; never fetch latest order to reinterpret history. Trace issuance/void amounts to historical price/economic facts; the documentary event does not create another commercial PriceChangeSet.

## EV-003 - EmdIssued

Stage: S4. EconomicRole: `DocumentEvidence`.

Payload: `orderId`, `emdId`, `documentNumber`, `issuerNamespace`, `purpose`, `couponAssociations`, `priceOrExternalValueRefs`.

None; no new customer money.

Snapshot actual committed document facts; never fetch latest order to reinterpret history. Trace issuance/void amounts to historical price/economic facts; the documentary event does not create another commercial PriceChangeSet.

## EV-004 - OrderScopeCancelled

Stage: S5. EconomicRole: `CommercialMovement`.

Payload: `orderId`, `changeId`, `serviceIds`, `priceChangeSetRef`, `commercialVersion`, `releaseEvidenceRefs`.

Only appended cancellation lines; no second reversal on inventory/funding release.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components, when present, reference the single committed PriceChangeSet for this commercial change.

## EV-005 - OrderServicesAdded

Stage: S6. EconomicRole: `CommercialMovement`.

Payload: `orderId`, `changeId`, `serviceIds`, `sourceProductRefs`, `priceChangeSetId`, `obligationRefs`, `commercialVersion`.

New accepted ancillary commercial value once.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components, when present, reference the single committed PriceChangeSet for this commercial change.

## EV-006 - TicketVoided

Stage: S8. EconomicRole: `DocumentEvidence`.

Payload: `orderId`, `ticketId`, `documentVersion`, `voidEvidenceRef`, `commercialChangeRef`.

None; no new customer money.

Snapshot actual committed document facts; never fetch latest order to reinterpret history. Trace issuance/void amounts to historical price/economic facts; the documentary event does not create another commercial PriceChangeSet.

## EV-007 - EmdVoided

Stage: S8. EconomicRole: `DocumentEvidence`.

Payload: `orderId`, `emdId`, `documentVersion`, `voidEvidenceRef`, `commercialChangeRef`.

None; no new customer money.

Snapshot actual committed document facts; never fetch latest order to reinterpret history. Trace issuance/void amounts to historical price/economic facts; the documentary event does not create another commercial PriceChangeSet.

## EV-008 - RefundAuthorized

Stage: S9. EconomicRole: `CommercialMovement`.

Payload: `orderId`, `refundId`, `changeId`, `priceChangeSetId`, `commercialVersion`, `refundAuthorityRef`, `payoutPlan`.

Accepted commercial credit/penalty once, before confirmed payout; outstanding refund remains visible.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components, when present, reference the single committed PriceChangeSet for this commercial change.

## EV-009 - RefundConfirmed

Stage: S9. EconomicRole: `PaymentEvidence`.

Payload: `orderId`, `refundId`, `originalChangeId`, `ownerMovementRefs`, `confirmedPayoutAmounts`, `completedAt`.

Payment/value movement confirmation, NOT another commercial credit.

Snapshot the confirmed owner movement and reference the original authorized commercial/economic fact; never create a second commercial credit or PriceChangeSet from payment confirmation.

## EV-010 - ExchangeApplied

Stage: S10. EconomicRole: `CommercialMovement`.

Payload: `orderId`, `exchangeId`, `changeId`, `commercialVersion`, `oldNewLineage`, `priceChangeSetId`, `documentRefs`.

Accepted old/new delta once; never sum delta and full replacement.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components, when present, reference the single committed PriceChangeSet for this commercial change.

## EV-011 - ReissueCompleted

Stage: S10. EconomicRole: `DocumentEvidence`.

Payload: `orderId`, `exchangeId`, `originalDocumentRefs`, `newDocumentRefs`, `originalChangeId`.

None; no new customer money.

Snapshot actual committed document facts; never fetch latest order to reinterpret history. Trace issuance/void amounts to historical price/economic facts; the documentary event does not create another commercial PriceChangeSet.

## EV-012 - ExchangeCompleted

Stage: S10. EconomicRole: `WorkflowCompletion`.

Payload: `orderId`, `exchangeId`, `originalChangeId`, `cleanupEvidenceRefs`.

Completion of the original accepted exchange, not a second sale.

Snapshot completion against the original operation/change/economic references; workflow completion does not create a second commercial PriceChangeSet.

## EV-013 - ServiceDelivered

Stage: S12. EconomicRole: `DeliveryEvidence`.

Payload: `orderIdAtOccurrence`, `currentOrderId`, `serviceId`, `observationId`, `observationKey`, `aspect`, `portion`, `quantity`, `sourceVersion`.

Ledger decides recognition from certified delivery fact; boarding alone is not delivered.

Snapshot the authoritative delivery observation/correction with service and consumption lineage; delivery evidence does not create a commercial PriceChangeSet.

## EV-014 - ServiceDeliveryCorrected

Stage: S12. EconomicRole: `DeliveryEvidence`.

Payload: `originalObservationId`, `correctingObservationId`, `serviceId`, `aspect`, `sourceRevision`, `reason`.

Correct the referenced recognition fact; not another delivery.

Snapshot the authoritative delivery observation/correction with service and consumption lineage; delivery evidence does not create a commercial PriceChangeSet.

## EV-015 - TravelerCorrected

Stage: S13. EconomicRole: `CommercialMovement`.

Payload: `orderId`, `changeId`, `travelerId`, `commercialVersion`, `protectedChangeRef`.

No monetary value unless a separate accepted price change exists.

Snapshot actual committed facts; never fetch latest order to reinterpret history. Monetary components, when present, reference the single committed PriceChangeSet for this commercial change.

## EV-016 - OrderSplitApplied

Stage: S14. EconomicRole: `Reclassification`.

Payload: `splitTransferId`, `sourceOrderId`, `childOrderId`, `pairedChangeIds`, `serviceOwnershipMap`, `pairedPriceTransferRefs`, `applicationTransferRef`.

Balanced reclassification; not refund plus new sale.

Snapshot the balanced source/child transfer and paired price-transfer references; reclassification may reference both sides and does not create new customer value.

## EV-017 - GroupNameMaterialized

Stage: S15. EconomicRole: `WorkflowCompletion`.

Payload: `groupId`, `rowIdentity`, `childOrderId`, `blockAllocationRefs`, `priceDispositionRef`, `depositApplicationRef`.

Apply existing group/deposit commercial treatment; do not charge charter contract again.

Snapshot completion against the original operation/change/economic references; workflow completion does not create a second commercial PriceChangeSet.

Read committed source snapshots rather than fetching current owners to reconstruct past facts. A corrective fact receives a new EventId and explicit original identity, never a rewritten earlier event.
