# Canonical typed application ports

These names and method shapes are the canonical seams. Async suffix is allowed; alternative parallel ports are not. Semantic external-service ports follow the standing `Domain/Ports/{Area}/` convention. Application handlers consume those ports; provider projects implement them. Application-only technical orchestration abstractions may remain in Application, but they are not external owner ports. Local issuance is a local domain/SQL operation, not an implementation of a fake remote effect. Every real binding remains subject to owner approval/certification; no production URL is inferred from a method name.

## IOfferSourcePort

Owner: AirOffer. Detailed contract: `CONTRACTS/02-AIROFFER.md`.

| Method | Semantic type | Required typed fields beyond applicable common context | Result |
|---|---|---|---|
| ResolveCandidate | Query | offerId, authorizedSalesContext, requestedSelection | CandidateResolution |
| ReadBoundCandidate | Query | ownerBindingRef, expectedSourceDigest, authorizedSalesContext | BoundCandidateEvidence or UnsupportedCapability |

## IPricingDecisionPort

Owner: AirPrice. Detailed contract: `CONTRACTS/03-AIRPRICE.md`.

| Method | Semantic type | Required typed fields beyond applicable common context | Result |
|---|---|---|---|
| QuoteAncillary | Query | productSnapshot, beneficiaryCoverage, quantity, salesContext, originalOrderContext | PricedChangeDecision |
| QuoteCancellation | Query | immutableOriginalPricingContext, targetScope, sourceFacts, reason | CancellationDecision |
| QuoteRefund | Query | originalPricingAndDocumentFacts, targetScope, consumptionControlFacts, reason | RefundDecision |
| QuoteExchange | Query | originalPricingContext, oldScope, newCandidate, relevantVersions, reasonOrWaiverAuthority | ExchangeDecision |
| QuotePartition | Query | sourceOrderContext, travelerPartition, sharedServiceDisposition, relevantVersions | PartitionDecision |
| ValidateTicketing | Query | acceptedPricingContext, targetScope, currentOwnerAuthorityFacts | TicketingDecision |
| ReadDecision | Query | decisionRef, expectedVersion, expectedDigest | ImmutableDecisionEvidence |

## IReservationPort

Owner: FlightFlow. Detailed contract: `CONTRACTS/04-FLIGHTFLOW.md`.

| Method | Semantic type | Required typed fields beyond applicable common context | Result |
|---|---|---|---|
| Reserve | Command | members, couplingGroups, sourceCapacityRefs, requestedHoldPolicy, approvedRevenueFacts | CapacityEffectResult |
| Commit | Command | reservationMemberRefs, expectedResourceVersions, issueOrExchangeOperationRef | CapacityEffectResult |
| Release | Command | heldMemberRefs, expectedResourceVersions, reason | CapacityEffectResult |
| CancelCommitted | Command | committedMemberRefs, expectedResourceVersions, reason, servicingAuthorityRef | CapacityEffectResult |
| Divide | Command | originalReservationRefs, exactMemberPartition, stableTargetOrderRefs | CapacityPartitionResult |
| AcquireGroupBlock | Command | contractRef, perFlightBlockRequests, requestedPolicy | CapacityBlockResult |
| AllocateExistingBlock | Command | blockRefsAndVersions, namedMemberAssignments, materializationRef | CapacityAllocationResult |
| ReleaseGroupBlock | Command | blockRef, expectedBlockVersion, exactUnusedQuantity, reason | CapacityBlockResult |
| ReadOperation | Query | originalOperationKey, providerProfileId, originalScopeHash | OperationReadResult |
| ReadResource | Query | canonicalResourceRefs, expectedNamespace | CapacityResourceReadResult |

## IFundingCoveragePort

Owner: JetPay. Detailed contract: `CONTRACTS/05-JETPAY.md`.

| Method | Semantic type | Required typed fields beyond applicable common context | Result |
|---|---|---|---|
| EstablishCoverage | Command | obligations, authorizedFundingIntentRef, exactRequestedValues | FundingEffectResult |
| AcquireIssueAuthority | Command | coverageRefs, obligationVersions, documentPlanScope, issueOperationId | IssueAuthorityResult |
| FinalizeIssueAuthority | Command | issueAuthorityRef, committedDocumentEvidence, appliedScope | FundingEffectResult |
| AcquireExchangeFundingAuthority | Command | acceptedExchangePlan, oldApplicationRefs, oldNewObligationVersions, additionalCollection, residualDisposition, exchangeOperationId | ExchangeFundingAuthority |
| RequestRelease | Command | coverageOrAuthorityRefs, exactUnconsumedScope, reason | FundingEffectResult |
| AcquireRefundAuthority | Command | acceptedRefundPlan, originalTenderApplicationRefs, exactPayoutValues | RefundAuthorityResult |
| ExecuteRefund | Command | refundAuthorityRef, commercialRefundPivotRef, payoutPlan | RefundPayoutResult |
| AcquireApplicationTransfer | Command | sourceApplicationRefs, sourceTargetObligationScopes, balancedTransferPlan | TransferAuthorityResult |
| FinalizeApplicationTransfer | Command | transferAuthorityRef, committedSplitOrExchangeRef | FundingTransferResult |
| ReadOperation | Query | originalOperationKey, providerProfileId, originalScopeHash | OperationReadResult |
| ReadCoverage | Query | coverageOrAuthorityRefs, obligationRefs | CoverageReadResult |

## IAncillaryCatalogPort

Owner: Ancillary. Detailed contract: `CONTRACTS/06-ANCILLARY-AND-SUPPLIERS.md`.

| Method | Semantic type | Required typed fields beyond applicable common context | Result |
|---|---|---|---|
| ReadProduct | Query | productRef, version | ProductDefinitionSnapshot |
| EvaluateEligibility | Query | productVersion, beneficiaryCoverage, sourceServiceFacts | ProductEligibilityDecision |

## ISupplierFulfillmentPort

Owner: Non-flight supplier. Detailed contract: `CONTRACTS/06-ANCILLARY-AND-SUPPLIERS.md`.

| Method | Semantic type | Required typed fields beyond applicable common context | Result |
|---|---|---|---|
| Reserve | Command | productSnapshot, beneficiaries, quantity, coverage, acceptedSupplierTerms | SupplierResourceResult |
| Confirm | Command | supplierReservationRef, expectedVersion, fulfillmentAuthority | SupplierResourceResult |
| Cancel | Command | supplierResourceRef, acceptedCancellationRef, reason | SupplierResourceResult |
| ReadOperation | Query | originalOperationKey, providerProfileId, scopeHash | OperationReadResult |
| ReadResource | Query | supplierResourceRef | SupplierResourceReadResult |

## IDocumentIssuancePort

Owner: External ETKT issuer only. Detailed contract: `CONTRACTS/09-DOCUMENT-AUTHORITY.md`.

| Method | Semantic type | Required typed fields beyond applicable common context | Result |
|---|---|---|---|
| Issue | Command | frozenTicketPlan, stockAllocationWhenClientOwned, gateEvidenceRefs | ExternalTicketResult |
| ReadOperation | Query | originalOperationKey, documentRole, assignedNumberWhenAny | OperationReadResult |

## IEmdIssuancePort

Owner: External EMD issuer only. Detailed contract: `CONTRACTS/09-DOCUMENT-AUTHORITY.md`.

| Method | Semantic type | Required typed fields beyond applicable common context | Result |
|---|---|---|---|
| Issue | Command | frozenEmdPlan, purposeReferences, couponAssociations, gateEvidenceRefs | ExternalEmdResult |
| ReadOperation | Query | originalOperationKey, documentRole, assignedNumberWhenAny | OperationReadResult |

## IDocumentServicingPort

Owner: External document issuer. Detailed contract: `CONTRACTS/09-DOCUMENT-AUTHORITY.md`.

| Method | Semantic type | Required typed fields beyond applicable common context | Result |
|---|---|---|---|
| Void | Command | documentCouponRefs, approvedVoidPlan, controlEvidence | ExternalDocumentDispositionResult |
| RedeemForRefund | Command | documentCouponRefs, acceptedRefundPlanRef, refundAuthorityRef, controlEvidence | ExternalDocumentDispositionResult |
| Exchange | Command | oldNewDocumentPlan, acceptedExchangeRef, controlAndGateEvidence | ExternalExchangeResult |
| Revalidate | Command | documentCouponRefs, approvedBindingChange, controlEvidence | ExternalDocumentBindingResult |
| ReassociateEmd | Command | emdCouponRefs, oldNewTicketCouponRefs, acceptedAssociationPlan, expectedControlVersions | ExternalDocumentBindingResult |
| ReadOperation | Query | originalOperationKey, documentRole, scopeHash | OperationReadResult |
| ReadDocument | Query | documentNamespaceAndNumber, couponRefs | ExternalDocumentReadResult |

## ICouponControlPort

Owner: SkyDispatch / certified issuer profile. Detailed contract: `CONTRACTS/07-SKYDISPATCH-DISRUPTION.md`.

| Method | Semantic type | Required typed fields beyond applicable common context | Result |
|---|---|---|---|
| RequestRelease | Command | couponRefs, expectedControlVersions, servicingOperationId | CouponControlResult |
| ReadOperation | Query | originalOperationKey, couponScope | OperationReadResult |
| ReadControl | Query | couponRefs | CouponControlReadResult |

`commonMutationRequest` in SPEC/ports.json applies only to mutations. Queries use authenticated lookup scope and never acquire resources. The typed result names refer to the scope/evidence records in the relevant Domain/Contract documents, not arbitrary success booleans. Exact code members preserve those meanings.
