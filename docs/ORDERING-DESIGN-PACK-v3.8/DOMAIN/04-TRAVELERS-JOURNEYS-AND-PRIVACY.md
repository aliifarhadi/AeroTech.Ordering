# Travelers, journeys, operational separation and protected data

## Person roles

Buyer, financial Customer, Traveler, payer, agency/seller and actor are separate references. Ordering owns the traveler data accepted for this booking; an optional CustomerId link is not a command to overwrite the passenger from today's Customer master. SalesContext and accepted traveler identity snapshots remain historical.

Traveler fields: stable TravelerId, current OrderId, source TravellerRef binding, PTC, protected NameSnapshotRef, DOB when required, source gender/nationality/identity/loyalty references where provided, and InfantParentTravelerId for linked infants. Create requires exactly one unique binding for every selected owner traveler reference. Reject missing/duplicate/foreign references; do not generate fictitious names or default gender from PTC.

Infant-parent relation is explicit, same current Order and valid guardian scope; no self-reference/cycles. Seat consumption is a product/resource rule, not `PTC != INF`. An infant with a purchased seat and an infant without a seat have distinct resource requirement evidence. Unknown requirements block reservation, not silently consume/free a seat.

Contact has role (primary/emergency/agency/traveler/other) and protected details. Minimum data depends on accepted product/issuer requirements. Strict document eligibility validates missing data at issuance; do not insert placeholder passports to make Create compile.

## Passenger segment versus physical leg

Journey groups customer travel. JourneySegment is the sold board-point to off-point passenger segment. Fields include stable SegmentId, journey/sequence, SegmentKind, sold marketing/operating identity, sold schedule snapshot, external flight ID/version, optional ThroughFlightGroupRef and historical planned leg references.

ScheduledAir requires an unambiguous dated flight and UTC instants from authoritative source/time-zone context. Preserve source DateTimeOffset and local/time-zone identifiers if supplied. OpenAir explicitly lacks a dated binding; no fake midnight/departure ID. Surface is ground/gap context and creates neither seat consumption nor ETKT coupon by itself.

One sold segment may have several physical legs. Legs do not multiply service/coupon count. The same flight number may still represent several separately sold passenger segments. Connections are explicit source facts: Connection/Stopover/SurfaceBreak/Unknown plus Protected/Unprotected/Unknown. Elapsed time does not establish protection or fare coupling.

A schedule change updates SegmentOperationalState and observations. It never edits SoldScheduleSnapshot. A customer-accepted flight replacement creates a new commercial segment/service lineage; a technical time update does not. OpenAir assignment/revalidation is a named commercial operation preserving the original open-sale record and document associations.

## Privacy and immutable history

Monetary immutability is not permission to retain unlimited PII. Store protected payload references for names/passports/guardian/assistance information with access and retention policy. History stores non-PII correlation IDs and redacted metadata. Deleting/erasing a payload does not delete original money, operation identity or document-number uniqueness records.

Do not hash raw low-entropy personal data for publicly queryable fingerprints. Idempotency uses a server-side keyed digest over the canonical semantic request or a protected request representation; scope prevents cross-customer disclosure. Rotation/retention of digest keys must preserve duplicate detection for the allowed replay horizon. Credentials/payment card data never go into Order, logs, source fixtures or Ledger events.

Name correction is an explicit versioned commercial change and issuer/control workflow when required. It preserves before/after protected references and updates document display only under authorized revalidation/reissue. It is not arbitrary Traveler PUT bypassing tickets, dependency claims and funding context.
