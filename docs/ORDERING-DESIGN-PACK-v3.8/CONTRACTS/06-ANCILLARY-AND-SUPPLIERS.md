# Ancillary and non-flight suppliers

Ancillary owns catalog/product definition/eligibility/fulfillment metadata; AirPrice owns pricing; FlightFlow owns flight-capacity resources only where needed. A catalog product is not an accepted OrderService until explicit commercial acceptance. A price request is not inventory reservation.

`IAncillaryCatalogPort.GetProductVersion` and `EvaluateEligibility` are queries. Results bind product/version/schema, beneficiaries/coverage/quantity constraints, accepted fulfillment/document/control profile, source validity and dependencies. Product metadata does not supply cash coverage or a locally computed price. Unknown registered schema blocks acceptance; unavailable catalog prevents new sales needing it but does not break GetOrder.

A supplier booking/fulfillment mutation, where a non-flight product needs one, belongs to an explicit `ISupplierFulfillmentPort` capability, not a fake FlightFlow air-seat hold. Target methods Reserve/Commit/Cancel/ReadOperation/ReadBooking share the durable external-operation protocol with typed product scope and owner resource identity. Exact real supplier routes and guarantees are BD-008.

Reference products: included baggage, paid additional piece, cabin seat product, meal, capacity-free lounge, shared room-stay and shared transfer. The simulator proves independent catalog/pricing/capacity/fulfillment ownership. Do not move all ancillary pricing into Ordering just because the catalog service is incomplete. A stale catalog update never rewrites an already accepted product/profile snapshot.
