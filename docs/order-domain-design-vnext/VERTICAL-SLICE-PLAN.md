# Vertical-slice execution plan

D0 establishes the day-zero source/design baseline. B0 certifies the already copied framework/layers. All later stages add running behavior in the existing target, not parallel legacy rails. Every end-stage checkpoint is delivered before moving on. The full domain is specified now; implementation is introduced slice by slice.

| Stage | Observable result | Primary spec |
|---|---|---|
| [D0](SLICES/D0.md) | A verified inventory of the actual target, design reading and exact B0 corrections, without business code. | `REVIEW/02-TARGET-BASELINE.md` |
| [B0](SLICES/B0.md) | The existing solution starts as an authenticated empty-business service with disposable SQL and durable messaging evidence. | `ARCHITECTURE/02-BOOTSTRAP.md` |
| [S1](SLICES/S1.md) | A real AirOffer candidate can be captured, explicitly accepted into a sandbox Order, replayed and read after restart with AirOffer offline; authoritative simulator Orders can continue to later stages. | `CONTRACTS/02-AIROFFER.md` |
| [S2](SLICES/S2.md) | An authoritative reference Order obtains exact scoped capacity evidence and can release/recover it safely after response loss. | `DOMAIN/05-RESERVATION-AND-CAPACITY.md` |
| [S3](SLICES/S3.md) | Coverage and payment evidence are visible by obligation; guarantees, cash, new-product collection and release have independent recoverable evidence. | `DOMAIN/06-FUNDING-OBLIGATIONS.md` |
| [S4](SLICES/S4.md) | Eligible scope obtains one correct local ETKT/EMD set after required capacity commit and funding authority; broker/issuer response loss is recoverable. | `DOMAIN/07-DOCUMENTS-AND-STOCK.md` |
| [S5](SLICES/S5.md) | A caller can cancel eligible unissued scope and see exact resource/funding release and commercial cancellation without a parallel Withdraw rail. | `DOMAIN/09-SERVICING-CHOREOGRAPHIES.md` |
| [S6](SLICES/S6.md) | Seat, baggage, meal, lounge, hotel and transfer products can be accepted with exact beneficiaries/coverage/pricing and appropriate resource owner. | `DOMAIN/02-COMMERCIAL-COMPOSITION.md` |
| [S7](SLICES/S7.md) | Paid ancillary scope can reuse the issue pipeline to obtain correctly associated EMD-A or purpose-specific EMD-S without extra capture. | `DOMAIN/07-DOCUMENTS-AND-STOCK.md` |
| [S8](SLICES/S8.md) | Document-only Void and commercial VoidAndCancel have distinct observable outcomes, correct issuer control and no duplicated cancellation/refund. | `DOMAIN/09-SERVICING-CHOREOGRAPHIES.md` |
| [S9](SLICES/S9.md) | The Order records a priced refund once, reserves valid payout authority and shows payout pending/confirmed independently. | `DOMAIN/09-SERVICING-CHOREOGRAPHIES.md` |
| [S10](SLICES/S10.md) | A source-priced old/new scope can be replaced without losing fare coupling, multiplying money or duplicating documents under failure. | `DOMAIN/09-SERVICING-CHOREOGRAPHIES.md` |
| [S11](SLICES/S11.md) | External cases can mark local impacts and invoke accepted per-Order remedies without a flight-wide transaction or invented case truth. | `DOMAIN/11-DELIVERY-AND-DISRUPTION.md` |
| [S12](SLICES/S12.md) | Normalized gateway observations update delivery/control views with correct ordering and corrections while sold truth remains unchanged. | `DOMAIN/11-DELIVERY-AND-DISRUPTION.md` |
| [S13](SLICES/S13.md) | Authorized identity corrections and document/flight binding servicing preserve history and do not invent seats or fare changes. | `DOMAIN/04-TRAVELERS-JOURNEYS-AND-PRIVACY.md` |
| [S14](SLICES/S14.md) | Selected whole travelers can move to one child with stable service/document history and balanced resource/value transfers. | `DOMAIN/12-SPLIT-GROUPS-AND-RELATED-ORDERS.md` |
| [S15](SLICES/S15.md) | An unnamed group block can materialize named Orders row by row without double-consuming capacity or duplicating deposits. | `DOMAIN/12-SPLIT-GROUPS-AND-RELATED-ORDERS.md` |
| [S16](SLICES/S16.md) | The reference system is reproducibly executable; live release status is tied to actual certified capabilities and measured operations. | `GOVERNANCE/03-STAGE-GATES.md` |

Read each slice and its runbook. LOCAL_DONE is separate from owner/live/production certification. Missing source validity at S1 is not hidden by a successful sandbox test.
