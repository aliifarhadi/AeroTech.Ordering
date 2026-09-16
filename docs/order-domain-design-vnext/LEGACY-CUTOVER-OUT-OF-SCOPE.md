# Legacy cutover is a separate project

This pack defines the clean target and its staged implementation in AeroTech.Ordering. It does not authorize moving production rows, running old migrations, dual-writing read models, switching legacy endpoints or retiring in-flight operations.

A later separately approved LEGACY-CUTOVER-AND-DATA-MIGRATION plan must inventory actual legacy orders/documents/payments/capacity/tasks, establish one execution owner per resource, map immutable historical facts, preserve unknown operations under their original provider keys and prove reconciliation before traffic switch. Recovery obligations cannot be solved by copying TrafficDocument/FulfillmentTask rails into the target domain.

No design or implementation agent may use coexistence needs to change the target's commercial/pricing/authority boundaries silently. Explicit adapters/import tools may be built later, outside the canonical new sale/servicing use cases, with their own validation/cutover authorization.
