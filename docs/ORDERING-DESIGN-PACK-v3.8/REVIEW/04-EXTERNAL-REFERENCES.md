# Primary external references and limited use

These references were consulted for implementation mechanics and context, not to replace the user's domain authority or assert airline standards certification.

- Microsoft EF Core transactions: https://learn.microsoft.com/en-us/ef/core/saving/transactions . Supports the distinction between a single SaveChanges transaction, explicitly shared relational transactions, and special care around execution strategies/savepoints. The pack's exact per-Order transaction choreography is a project target decision.
- Microsoft EF Core optimistic concurrency: https://learn.microsoft.com/en-us/ef/core/saving/concurrency . Supports concurrency-token/rowversion mechanics. The deliberate root version update when children change is required by this project's mutation boundary, not automatically supplied by child writes.
- IATA ONE Order program overview: https://www.iata.org/en/programs/airline-distribution/retailing/one-order/ . Provides retailing/order-management context. This pack intentionally retains ETKT/EMD fulfillment roots under the user's requirements; it is not an assertion of IATA message-schema, accounting, interline or certification compliance.

For actual issuer limits/stock formatting, tax/refund/FX rules, provider authority, payment execution and retention, use the named owner contracts. Do not turn a generic public overview into an invented cross-service rule. No licensed airline standards were assumed reviewed here.
