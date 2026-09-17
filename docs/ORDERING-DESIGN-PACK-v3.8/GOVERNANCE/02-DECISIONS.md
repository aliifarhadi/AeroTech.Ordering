# Target decisions and change control

| ID | Class | Binding decision |
|---|---|---|
| TD-001 | TARGET_DECISION | Reuse/certify the existing AeroTech.Ordering shell; do not create another repository or solution. |
| TD-002 | USER_BINDING / TARGET_CONSTRAINT | Preserve actual project names and standing conventions: Ordering-owned enums remain under `Contracts/AeroTech.Messages/Ordering/Enums/`; semantic external-service ports remain under `Domain/Ports/{Area}/`; `{X}Aggregate` folders, `{Name}CommandHandler` naming and Query placement remain. Domain Messages usage is an explicit type allowlist: `AeroTech.Messages.Ordering.Enums.*` plus the existing `BusinessContextType`, `PrincipalType`, and `AuthorizationSurface` caller-context types. No integration-event/provider-wire/base-transport types are allowed, and any new non-Ordering Messages type requires explicit user approval. |
| TD-003 | TARGET_DECISION | One SQL database initially; command and local read-model updates share one explicit transaction, one unit-of-work registration and one projection writer. |
| TD-004 | TARGET_DECISION | Prepare a priced candidate before explicit acceptance; Create consumes the immutable locally stored accepted candidate plus exact acceptance digest. |
| TD-005 | TARGET_DECISION | AirOffer's current unversioned full offer projection can be represented conservatively as one accepted package item. Its coupon allocations do not create independent fare contracts. This bridge is preview/sandbox only until owner acceptance binding is certified. |
| TD-006 | TARGET_DECISION | Reference document authority is Local: ETKT and EMD roots plus Stock in Ordering. External authority is a separate explicit profile. Actual production issuer authorization is BD-006. |
| TD-007 | TARGET_DECISION | Funding obligations are scoped and independently versioned; a paid add creates a delta obligation rather than charging the original sale again. |
| TD-008 | TARGET_DECISION | One active exclusive commercial/fulfillment operation claim per Order initially. Observations are still ingested; conflicting evidence can suspend the operation. Worker leases are not business claims. |
| TD-009 | TARGET_DECISION | Reference exchange choreography has a stated local commercial/document pivot and independently tracked post-pivot cleanup. Adapter limitations cannot silently reorder it. |
| TD-010 | TARGET_DECISION | Ordinary commits are atomic per Order. Split is a deliberate source+child same-database atomic exception, never a general cross-order distributed transaction. |
| TD-011 | TARGET_DECISION | Operational quantity/control/aspect histories remain separate from commercial service quantity and identity. |
| TD-012 | TARGET_DECISION | Stage LOCAL_DONE and per-capability LIVE_CERTIFIED are separate; production release needs S16. |
| TD-013 | TARGET_DECISION | Optional complex products require registered versioned detail schemas. An unknown schema is rejected before acceptance. |
| TD-014 | TARGET_DECISION | Keep the source net10.0 target and package conventions unless a reviewed compatibility/security decision requires change. Monetary SQL storage is explicit per field, never global 18,2. |
| TD-015 | TARGET_DECISION | Initial one owner-airline deployment. Seller, payer, financial customer and operating/marketing/issuing carriers are distinct roles. |
| TD-016 | TARGET_DECISION | Split/group/traveler corrections and revalidation are specified here, implemented in later vertical slices rather than silently dropped. |

A target capability contract is a request to an owner, not a statement that its current endpoint supports it. The reference simulator implements that request. The production owner can implement equivalent semantics through a different wire protocol; the ACL maps the wire without changing domain invariants.

## Change record

Before a semantic deviation, add a record containing `DecisionId`, original rule, proposed rule, source evidence, why an adapter/local implementation cannot preserve the rule, impacted aggregates/ports/schema/events/stages, migration, regression scenarios, rollout/recovery and decision owner. Technical details inside a prescribed boundary may be implemented without another approval ceremony. Owner truth, issuer authority, accepted pricing or financial policy cannot be self-approved by the implementation agent.

Status vocabulary: Proposed, AcceptedForReferenceProfile, OwnerApproved, Rejected, Superseded. A reference-profile choice never promotes itself to OwnerApproved.
