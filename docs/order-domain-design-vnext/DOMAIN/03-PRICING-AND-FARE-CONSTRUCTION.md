# Pricing, attribution and fare construction

## Four different questions

OrderItem: what was priced as one commercial item? OrderService: what is independently serviced/delivered? AirFareConstruction: what source fare/rule coupling was accepted? PricingLine/Allocation: what money changed and how was it attributed? None substitutes for another.

## PriceChangeSet and sign contract

Every accepted monetary mutation appends one immutable `PriceChangeSet(SetId, OrderId, ChangeId, FinancialSequence, Reason, SourceDecisionRef, BaseCommercialVersion, CommittedAt)` with all its lines in the commercial transaction. A quote is not a committed line. A zero-delta accepted repricing can have a set with explicit zero-delta evidence; a pure delivery update never invents one.

A line has `LineId`, set/order/item scope, ComponentType, Effect, Direction, LineRole, OriginalValue, SaleValue, exact source occurrence/reference and calculation provenance. Amounts are NONNEGATIVE magnitudes. Direction alone supplies sign: Debit = +1, Credit = -1. Effect is CustomerBalance, SettlementOnly or Informational, NEVER a second sign. `CustomerTotal = sum(sign(Direction) * SaleValue.Amount where Effect=CustomerBalance)`.

This is commercial arithmetic, not instructions to debit/credit GL accounts. SettlementOnly requires party/category/currency and is excluded from customer payable totals. Informational is excluded from payable/settlement totals.

| Component | Normal customer direction | SettlementOnly | Note |
|---|---|---|---|
| Fare / ProductCharge / CarrierSurcharge | Debit | Explicit supplier/contract valuation only | Preserve source type; do not silently relabel surcharges as fare |
| Tax | Debit | Forbidden in this baseline | Customer tax stays customer-effective; accounting settlement is Ledger-owned |
| Fee / Markup / Penalty | Debit | Explicit source settlement basis | Cancellation penalty is a new charge, not a negative refund |
| Discount | Credit | Explicit settlement discount only | Reversal of discount is Debit |
| Commission | Not allowed as customer charge | Explicit commission entitlement/reduction | Customer concession is Discount |
| Adjustment | Debit or Credit with code/reason/authority | Explicit party/category | Goodwill above original value is an adjustment, not an over-reversal |
| Other | Informational only | Not allowed | Unsupported customer economics must be defined, not silently accepted |

LineRole: Original, Reversal, Adjustment or Transfer. `OriginalPricingLineId` alone does not make a reversal. A Reversal requires opposite Direction, same component/effect/currencies, historical conversion provenance and cumulative original/sale magnitude within the still-reversible original. Full reversal copies exact accepted magnitudes. Partial reversal needs an authoritative split or approved source allocation. Serialize outstanding-value validation with the Order monetary commit. Undo a reversal using a named correction/adjustment, not an unbounded reversal-of-reversal algorithm.

Replacement normalization chooses EITHER source component deltas OR explicit old reversals plus full new components. It never posts both full new price and differential. Source rounding residuals are retained if actually supplied; mismatches are not repaired with invented Adjustment lines.

## Monetary representation

Use existing generic/platform decimal and currency identity conventions, including source CurrencyId and source CurrencyCode snapshot. Do not introduce a currency master/rounding/FX engine. The domain requires exact amount plus currency semantics; implementing a small local immutable record for these values is not permission to create a competing shared framework.

SQL reference storage capacity is decimal(28,8) for accepted extended amounts, decimal(28,12) for supplied rates/unit prices and decimal(18,6) for quantities. These are STORAGE capacities, not currency rounding rules. Preserve source scale/provenance. Reject RepresentationOverflow before commit rather than round/truncate; adjust storage through an explicit technical migration if an actual certified source requires more precision. Three-decimal and zero-decimal currency tests are mandatory. Never infer two-decimal currency behavior from the DB schema.

Original currency/value and sale currency/value are separate accepted facts. Never sum different currencies or calculate a missing conversion. Source calculation/FX evidence includes rate reference, original/sale amounts, convention and rounding evidence only when supplied. Preserve strings/decimal numeric values losslessly at the ACL; float conversion is forbidden for money. A source float rounding-factor metadata value is not used to recompute accepted money.

## Allocation sets

`AllocationSet` belongs to exactly one PricingLine, one Purpose and one Version. Baseline persists CommercialValue when needed; additional Servicing/Accounting/Settlement/Reporting sets require a concrete consumer. Ledger owns accounting allocation policy. RefundBasis is NOT an implicit allocation purpose or entitlement.

Set fields: id, source, method, completeness, version, supersedes, rule/weights where derived. Rows identify service/traveler/segment/portion and original/sale attribution. Complete sums equal the parent magnitude exactly per currency; Partial sums do not exceed parent and show residual; Unavailable has no fabricated zero shares. Rows inherit parent Direction/Effect and NEVER add new money to totals. Do not sum several allocation versions or purposes together. Direct single-service attribution may be derived from immutable basis without redundant rows.

A refund quote can credit an amount different from historic proration. Allocation attribution, reversible commercial value, tender refund capacity and payment execution are separate constraints.

## Fare construction

`AirFareConstruction` is an immutable Order-owned context snapshot, not an aggregate. Fields: construction ID, OrderIdAtCreation, created change, superseded reference, item refs, construction type, source decision/payload reference, pricing groups.

PricingGroup identifies exact source-grouped travelers, PTC and quantity. Default one traveler per group; equal PTC alone cannot combine them. Monetary line values are extended once, not multiplied again by group quantity.

PricingUnit has source identity, type (OneWay/RoundTrip/OpenJaw/CircleTrip/Other), combination method and components. FareComponent has explicit covered ServiceIds or traveler+segment pairs, source fare ID, fare basis/family/brand, cabin/RBD, fare owner/rule/routing/tariff references and accepted terms where supplied. A component may span several passenger segments and a construction may couple multiple items.

A round-trip itinerary may contain one true RoundTrip PU or two independent OneWay PUs. A technical leg does not create a fare component. JourneySequence does not define fare breaks. Quotes identify the full affected pricing units/items/travelers and any wider repricing scope; Ordering cannot silently shrink it to the clicked coupon.

`OpaquePricingContext` is supported when no reliable construction is supplied. It contains immutable source facts/payload and explicitly absent structural fields; it is not guessed from itinerary shape. Granular servicing requiring unavailable context returns PricingContextUnavailable. Original sale need not fabricate a fare construction to populate a table.

## Numerical reference

Synthetic EUR: Fare Debit 400 + bag Debit 50 - Discount Credit 45 = 405. SettlementOnly Commission Debit 20 leaves CustomerTotal 405. Reversing the discount adds Debit 45 -> 450. A refund approval credit 100 changes commercial obligation once; a later cash refund movement changes AppliedNet once and must not append another credit 100. See EXAMPLES/pricing-ledger.json.


## Reversal ceilings across ownership transfer

For split-transferred value, outstanding attribution is original assigned value minus prior direct reversals minus transfers out, plus explicitly accepted transfers in. Apply caps to the currently owned, traceable portion in each original/sale currency. A transfer does not create another refundable original sale. DOMAIN/12 defines the paired transfer/payout authority checks; do not compare a post-split refund only with an unsplit historic line total.
