# S1 final shape — open decisions (recorded, not decided)

Raised during stage 08 (`reports/08-S1-authoritative-final-closure/`). Each item stops only its own path; everything
else in S1 is closed. Neither item blocks the architect review of the S1 final shape, because neither can be produced
or consumed by an S1 command.

---

## OD-S1-08 — `ScopeAtAssociation` has no Pack-defined type or semantics

**Where.** `docs/ORDERING-DESIGN-PACK-v3.8/DOMAIN/02-COMMERCIAL-COMPOSITION.md` line 13:

> Store immutable `OrderItemServiceLink(LinkId, OrderIdAtAssociation, OrderItemId, OrderServiceId, ScopeAtAssociation,
> LinkedByChangeId)`. Current service owns exactly one OrderId/OrderItemId. New membership supersedes a current
> binding; old links remain history.

**Evidence of the gap.** `ScopeAtAssociation` occurs exactly once in the whole Pack — that line. Verified by
`grep -rn "AtAssociation" docs/ORDERING-DESIGN-PACK-v3.8/`, which returns that single hit. No other Pack file gives it
a type, a value set, a source or a rule. `DOMAIN/13-PERSISTENCE-DATA-DICTIONARY.md` does not list it either.

**Why it cannot be settled from the source.** "Scope" carries at least three distinct meanings in the Pack, and the
sentence does not say which one applies:

1. the funding/pricing scope vocabulary of `DOMAIN/06-FUNDING-OBLIGATIONS.md` (item / service / pricing line),
2. the sales scope of `DOMAIN/04` and `DOMAIN/05` (owner airline, financial customer, sales context, buyer),
3. the coverage scope of `DOMAIN/02` itself (which travellers and segments the service covers).

Each reading produces a different column, a different type and a different invariant. Choosing one would be inventing
a persistence-identity decision the Pack did not make.

**What S1 implemented instead.** The rest of the tuple is persisted, and `OrderIdAtAssociation` is enforced as a real
scoping key rather than a stored number: all three outbound foreign keys of `OrderItemServiceLinks` are composite
against `(OrderId, Id)` alternate keys of `OrderItems`, `OrderServices` and `OrderChanges`, so a link cannot bind
rows belonging to a different Order. Proven by `CurrentOwnershipConstraintTests`.

**What is blocked.** Nothing in S1. An original sale creates exactly one link per (item, service) pair inside one
change; the field would be constant for the entire S1 lifetime of the Order. The field becomes load-bearing only in the
stage where a service can be re-associated to another item, because that is where "the scope it had at association"
starts to differ from its current scope.

**Question for the owner.** Which of the three readings is `ScopeAtAssociation`, and what is its persisted type?

**Status.** OPEN. Recorded in `docs/ORDERING-DESIGN-PACK-v3.8/ERRATA/S1-CLOSURE-CLARIFICATIONS.md` section 1.3 as
DEFERRED. Not implemented, not guessed.

---

## OD-S1-09 — Protected personal-data payload store

**Where.** `docs/ORDERING-DESIGN-PACK-v3.8/DOMAIN/04-TRAVELERS-JOURNEYS-AND-PRIVACY.md` line 25:

> Monetary immutability is not permission to retain unlimited PII. Store protected payload references for
> names/passports/guardian/assistance information with access and retention policy. History stores non-PII correlation
> IDs and redacted metadata.

**Current state.** S1 stores traveller identity inline — `OrderTravellerIdentities.GivenName`, `.Surname`,
`.DateOfBirth`, and `OrderContacts.Email`, `.Phone` — as ordinary columns of the Ordering database. No protected
payload reference, no access policy, no retention policy exists.

**What is already correct.** No Ordering history table holds personal data: `OrderChange`, `PriceChangeSet`,
`PricingLine`, `FundingObligation`, `OrderComponentTotal` and the outbox carry identifiers and money only, so the
monetary record already survives erasure of the personal payload. Personal data is reachable only through the current
`OrderTravellers` / `OrderContacts` rows. S1 stores no passport, document, guardian-assistance or special-needs
payload, because no S1 command accepts one.

**Why this is not a code-local fix.** A protected payload store is a service-level capability: a key-managed store, an
access-control surface, and a retention job. Where it lives (inside Ordering, in Identity, in a dedicated service),
who owns the keys, and what the retention window is are owner decisions with cross-service consequences. Ordering
cannot pick one without making an architectural decision for other services.

**Consequence stated plainly.** S1 is **not privacy-complete**. The report does not claim this Pack line is satisfied.

**Question for the owner.** Where does the protected payload store live, who owns the keys, and what retention policy
applies to traveller name / date of birth / contact details?

**Status.** OPEN. Recorded in `docs/ORDERING-DESIGN-PACK-v3.8/ERRATA/S1-CLOSURE-CLARIFICATIONS.md` section 6 as a
scoped DIVERGENCE. Nothing invented.
